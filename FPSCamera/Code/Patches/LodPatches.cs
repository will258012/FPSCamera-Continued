using FPSCamera.Game;
using HarmonyLib;
using System;
using UnityEngine;

namespace FPSCamera.Patches
{
    [HarmonyPatch]
    [HarmonyAfter("com.github.algernon-A.csl.visibilitycontrol", "boformer.TrueLodToggler")]
    internal static class LodPatches
    {

        [HarmonyPatch(typeof(BuildingInfoBase), nameof(BuildingInfoBase.RefreshLevelOfDetail), new Type[] { typeof(Vector3) })]
        [HarmonyPostfix]
        private static void BuildingInfoBaseRefreshLOD(BuildingInfoBase __instance)
        {
            if (LodManager.LodConfig.ActiveConfig == null) return;
            // Only applies to instances with LODs.
            if (__instance.m_lodMesh != null)
            {
                __instance.m_minLodDistance = LodManager.LodConfig.ActiveConfig.BuildingLodDistance;

            }
        }

        [HarmonyPatch(typeof(BuildingInfo), nameof(BuildingInfo.RefreshLevelOfDetail))]
        [HarmonyPostfix]
        private static void BuildingRefreshLOD(BuildingInfo __instance)
        {
            if (LodManager.LodConfig.ActiveConfig == null) return;
            // Only applies to instances with LODs.
            if (__instance.m_lodMesh != null)
            {
                __instance.m_minLodDistance = LodManager.LodConfig.ActiveConfig.BuildingLodDistance;

            }
        }

        [HarmonyPatch(typeof(BuildingInfoSub), nameof(BuildingInfoSub.RefreshLevelOfDetail))]
        [HarmonyPostfix]
        private static void BuildingSubRefreshLOD(BuildingInfoSub __instance)
        {
            if (LodManager.LodConfig.ActiveConfig == null) return;
            // Only applies to instances with LODs.
            if (__instance.m_lodMesh != null)
            {
                __instance.m_minLodDistance = LodManager.LodConfig.ActiveConfig.BuildingLodDistance;

            }
        }

        [HarmonyPatch(typeof(CitizenInfo), nameof(CitizenInfo.RefreshLevelOfDetail))]
        [HarmonyPostfix]
        private static void CitizenRefreshLOD(CitizenInfo __instance)
        {
            if (LodManager.LodConfig.ActiveConfig == null) return;
            // Only applies to instances with LODs.
            if (__instance.m_lodMesh != null)
            {
                __instance.m_lodRenderDistance = LodManager.LodConfig.ActiveConfig.CitizenLodDistance;
            }
        }

        [HarmonyPatch(typeof(NetInfo), nameof(NetInfo.RefreshLevelOfDetail))]
        [HarmonyPostfix]
        private static void NetRefreshLOD(NetInfo __instance)
        {
            if (LodManager.LodConfig.ActiveConfig == null) return;
            // Iterate through all segments in net.
            NetInfo.Segment[] segments = __instance.m_segments;
            if (segments != null)
            {
                for (int i = 0; i < segments.Length; ++i)
                {
                    // Only applies to segments with LODs.
                    if (segments[i].m_lodMesh != null)
                    {
                        segments[i].m_lodRenderDistance = LodManager.LodConfig.ActiveConfig.NetworkLodDistance;
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
                        nodes[i].m_lodRenderDistance = LodManager.LodConfig.ActiveConfig.NetworkLodDistance;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PropInfo), nameof(PropInfo.RefreshLevelOfDetail))]
        [HarmonyPostfix]
        private static void PropRefreshLOD(PropInfo __instance)
        {
            if (LodManager.LodConfig.ActiveConfig == null) return;
            // Decal or prop?
            if (__instance.m_isDecal && __instance.m_material && __instance.m_material.shader.name.Equals("Custom/Props/Decal/Blend"))
            {
                var distence = LodManager.LodConfig.ActiveConfig.DecalPropFadeDistance;
                // Apply visibility.
                __instance.m_lodRenderDistance = distence;
                __instance.m_material.SetFloat("_FadeDistanceFactor", 1f / (distence * distence));
            }
            else
            {
                // Non-decal prop.
                __instance.m_lodRenderDistance = LodManager.LodConfig.ActiveConfig.PropLodDistance;
            }
        }

        [HarmonyPatch(typeof(TreeInfo), nameof(TreeInfo.RefreshLevelOfDetail))]
        [HarmonyPostfix]
        private static void TreeRefreshLOD(TreeInfo __instance)
        {
            if (LodManager.LodConfig.ActiveConfig == null) return;
            __instance.m_lodRenderDistance = LodManager.LodConfig.ActiveConfig.TreeLodDistance;
        }

        [HarmonyPatch(typeof(VehicleInfo), nameof(VehicleInfo.RefreshLevelOfDetail))]
        [HarmonyPostfix]
        private static void VehicleRefreshLOD(VehicleInfo __instance)
        {
            if (LodManager.LodConfig.ActiveConfig == null) return;
            __instance.m_lodRenderDistance = LodManager.LodConfig.ActiveConfig.VehicleLodDistance;
        }

        [HarmonyPatch(typeof(VehicleInfoBase), nameof(VehicleInfoBase.RefreshLevelOfDetail))]
        [HarmonyPostfix]
        private static void VehicleSubRefreshLOD(VehicleInfoBase __instance)
        {
            if (LodManager.LodConfig.ActiveConfig == null) return;
            __instance.m_lodRenderDistance = LodManager.LodConfig.ActiveConfig.VehicleLodDistance;
        }

        [HarmonyPatch(typeof(VehicleInfoSub), nameof(VehicleInfoSub.RefreshLevelOfDetail))]
        [HarmonyPostfix]
        private static void VehicleSubRefreshLOD(VehicleInfoSub __instance)
        {
            if (LodManager.LodConfig.ActiveConfig == null) return;
            __instance.m_lodRenderDistance = LodManager.LodConfig.ActiveConfig.VehicleLodDistance;
        }
    }
}
