using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using static FPSCamera.Game.LodManager.LodConfig;
namespace FPSCamera.Patches
{
    //Edited from the code of algernon's Visibility Control. Many Thanks!
    internal static class LodPatches
    {
        internal static void Apply(Harmony harmony)
        {
            PatchPostfix(harmony, AccessTools.DeclaredMethod(typeof(BuildingInfoBase), nameof(BuildingInfoBase.RefreshLevelOfDetail), [typeof(Vector3)]), nameof(BuildingInfoBaseRefreshLOD), typeof(BuildingInfoBase));
            PatchPostfix(harmony, AccessTools.DeclaredMethod(typeof(BuildingInfo), nameof(BuildingInfo.RefreshLevelOfDetail), Type.EmptyTypes), nameof(BuildingRefreshLOD), typeof(BuildingInfo));
            PatchPostfix(harmony, AccessTools.DeclaredMethod(typeof(BuildingInfoSub), nameof(BuildingInfoSub.RefreshLevelOfDetail), Type.EmptyTypes), nameof(BuildingSubRefreshLOD), typeof(BuildingInfoSub));
            PatchPostfix(harmony, AccessTools.DeclaredMethod(typeof(CitizenInfo), nameof(CitizenInfo.RefreshLevelOfDetail), Type.EmptyTypes), nameof(CitizenRefreshLOD), typeof(CitizenInfo));
            PatchPostfix(harmony, AccessTools.DeclaredMethod(typeof(NetInfo), nameof(NetInfo.RefreshLevelOfDetail), Type.EmptyTypes), nameof(NetRefreshLOD), typeof(NetInfo));
            PatchPostfix(harmony, AccessTools.DeclaredMethod(typeof(PropInfo), nameof(PropInfo.RefreshLevelOfDetail), Type.EmptyTypes), nameof(PropRefreshLOD), typeof(PropInfo));
            PatchPostfix(harmony, AccessTools.DeclaredMethod(typeof(TreeInfo), nameof(TreeInfo.RefreshLevelOfDetail), Type.EmptyTypes), nameof(TreeRefreshLOD), typeof(TreeInfo));
            PatchPostfix(harmony, AccessTools.DeclaredMethod(typeof(VehicleInfo), nameof(VehicleInfo.RefreshLevelOfDetail), Type.EmptyTypes), nameof(VehicleRefreshLOD), typeof(VehicleInfo));
            PatchPostfix(harmony, AccessTools.DeclaredMethod(typeof(VehicleInfoBase), nameof(VehicleInfoBase.RefreshLevelOfDetail), [typeof(Vector3)]), nameof(VehicleSubRefreshLOD), typeof(VehicleInfoBase));
            PatchPostfix(harmony, AccessTools.DeclaredMethod(typeof(VehicleInfoSub), nameof(VehicleInfoSub.RefreshLevelOfDetail), Type.EmptyTypes), nameof(VehicleSubRefreshLOD), typeof(VehicleInfoSub));
        }

        private static void BuildingInfoBaseRefreshLOD(BuildingInfoBase __instance)
        {
            // If there's no active LOD configuration, return (this adjustment is invoked when FPSCamera disabled / isn't related to FPSCamera).
            // To use the result of the original method / other LOD mods
            if (ActiveConfig == null) return;

            // Applies only to instances with LODs.
            if (__instance.m_lodMesh != null)
            {
                // Applies the smaller LOD distance.
                __instance.m_minLodDistance = Mathf.Min(__instance.m_minLodDistance, ActiveConfig.BuildingLodDistance);
            }
        }

        private static void BuildingRefreshLOD(BuildingInfo __instance)
        {
            if (ActiveConfig == null) return;
            // Only applies to instances with LODs.
            if (__instance.m_lodMesh != null)
            {
                __instance.m_minLodDistance =
                    Mathf.Min(__instance.m_minLodDistance, ActiveConfig.BuildingLodDistance);
            }
        }

        private static void BuildingSubRefreshLOD(BuildingInfoSub __instance)
        {
            if (ActiveConfig == null) return;
            // Only applies to instances with LODs.
            if (__instance.m_lodMesh != null)
            {
                __instance.m_minLodDistance =
                    Mathf.Min(__instance.m_minLodDistance, ActiveConfig.BuildingLodDistance);

            }
        }

        private static void CitizenRefreshLOD(CitizenInfo __instance)
        {
            if (ActiveConfig == null) return;
            // Only applies to instances with LODs.
            if (__instance.m_lodMesh != null)
            {
                __instance.m_lodRenderDistance =
                    Mathf.Min(__instance.m_lodRenderDistance, ActiveConfig.CitizenLodDistance);
            }
        }

        private static void NetRefreshLOD(NetInfo __instance)
        {
            if (ActiveConfig == null) return;
            // Iterate through all segments in net.
            NetInfo.Segment[] segments = __instance.m_segments;
            if (segments != null)
            {
                for (int i = 0; i < segments.Length; ++i)
                {
                    // Only applies to segments with LODs.
                    if (segments[i].m_lodMesh != null)
                    {
                        segments[i].m_lodRenderDistance =
                            Mathf.Min(segments[i].m_lodRenderDistance, ActiveConfig.NetworkLodDistance);
                    }
                }
            }

            // Iterate through all nodes in net.
            NetInfo.Node[] nodes = __instance.m_nodes;
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Length; ++i)
                {
                    // Only applies to segments with LODs.
                    if (nodes[i].m_lodMesh != null)
                    {
                        nodes[i].m_lodRenderDistance =
                            Mathf.Min(nodes[i].m_lodRenderDistance, ActiveConfig.NetworkLodDistance);
                    }
                }
            }
        }

        private static void PropRefreshLOD(PropInfo __instance)
        {
            if (ActiveConfig == null) return;
            // Decal or prop?
            if (__instance.m_isDecal && __instance.m_material && __instance.m_material.shader.name.Equals("Custom/Props/Decal/Blend"))
            {
                var distence =
                    Mathf.Min(__instance.m_lodRenderDistance, ActiveConfig.DecalPropFadeDistance);
                // Apply visibility.
                __instance.m_lodRenderDistance = distence;
                __instance.m_material.SetFloat("_FadeDistanceFactor", 1f / (distence * distence));
            }
            else
            {
                // Non-decal prop.
                __instance.m_lodRenderDistance =
                    Mathf.Min(__instance.m_lodRenderDistance, ActiveConfig.PropLodDistance);
            }
        }

        private static void TreeRefreshLOD(TreeInfo __instance)
        {
            if (ActiveConfig == null) return;
            __instance.m_lodRenderDistance =
                Mathf.Min(__instance.m_lodRenderDistance, ActiveConfig.TreeLodDistance);
        }

        private static void VehicleRefreshLOD(VehicleInfo __instance)
        {
            if (ActiveConfig == null) return;
            __instance.m_lodRenderDistance =
                Mathf.Min(__instance.m_lodRenderDistance, ActiveConfig.VehicleLodDistance);
        }

        private static void VehicleSubRefreshLOD(VehicleInfoBase __instance)
        {
            if (ActiveConfig == null) return;
            __instance.m_lodRenderDistance =
                Mathf.Min(__instance.m_lodRenderDistance, ActiveConfig.VehicleLodDistance);
        }

        private static void VehicleSubRefreshLOD(VehicleInfoSub __instance)
        {
            if (ActiveConfig == null) return;
            __instance.m_lodRenderDistance =
                Mathf.Min(__instance.m_lodRenderDistance, ActiveConfig.VehicleLodDistance);
        }

        private static void PatchPostfix(Harmony harmony, MethodBase original, string patchName, Type patchArgumentType)
        {
            var patch = AccessTools.Method(typeof(LodPatches), patchName, [patchArgumentType]);
            if (original == null || patch == null)
                throw new MissingMethodException($"Unable to patch LOD method {original?.Name ?? patchName}");
            if (original.DeclaringType != patchArgumentType)
                throw new MissingMethodException($"LOD method {original.Name} is declared by {original.DeclaringType?.FullName}, not {patchArgumentType.FullName}");

            var postfix = new HarmonyMethod(patch)
            {
                after = ["com.github.algernon-A.csl.visibilitycontrol", "boformer.TrueLodToggler"]
            };
            harmony.Patch(original, postfix: postfix);
        }
    }
}
