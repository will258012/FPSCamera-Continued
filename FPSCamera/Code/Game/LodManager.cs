using AlgernonCommons;
using FPSCamera.Settings;
using System;
using System.Collections;
namespace FPSCamera.Game
{
    public class LodManager
    {
        public static IEnumerator ToggleLODOpt(bool status)
        {
            try
            {
                if (status)
                {
                    switch (ModSettings.LodOpt)
                    {
                        case 1:
                            LodConfig.ActiveConfig = LodConfig.Low;
                            break;
                        case 2:
                            LodConfig.ActiveConfig = LodConfig.Mid;
                            break;
                        case 3:
                            LodConfig.ActiveConfig = LodConfig.High;
                            break;
                    }
                }
                else
                {
                    LodConfig.ActiveConfig = null;
                }
                Logging.Message("-- Refreshing LOD");
                RefreshLODs();
            }

            catch (Exception e)
            {
                Logging.LogException(e, "Failed to perform LOD optimization");
            }
            yield break;
        }
        private static void RefreshLODs()
        {
            refreshLods<TreeInfo>();
            refreshLods<PropInfo>();
            refreshLods<BuildingInfo>();
            refreshLods<NetInfo>();
            refreshLods<VehicleInfo>();
            refreshLods<CitizenInfo>();
        }

        private static void refreshLods<T>()
            where T : PrefabInfo
        {
            // Iterate through all loaded prefabs of the specified type.
            uint prefabCount = (uint)PrefabCollection<T>.LoadedCount();
            for (uint i = 0; i < prefabCount; ++i)
            {
                // Refresh LODs for all valid prefabs.
                PrefabInfo prefab = PrefabCollection<T>.GetLoaded(i);
                if (prefab)
                {
                    prefab.RefreshLevelOfDetail();
                }
            }

            // Also refresh any edit prefab.
            if (ToolsModifierControl.toolController && ToolsModifierControl.toolController.m_editPrefabInfo is T)
            {
                ToolsModifierControl.toolController.m_editPrefabInfo.RefreshLevelOfDetail();
            }
        }
        internal class LodConfig
        {
            internal LodConfig(float citizenLodDistance,
                               float treeLodDistance,
                               float propLodDistance,
                               float decalPropFadeDistance,
                               float buildingLodDistance,
                               float networkLodDistance,
                               float vehicleLodDistance)
            {
                CitizenLodDistance = citizenLodDistance;
                TreeLodDistance = treeLodDistance;
                PropLodDistance = propLodDistance;
                DecalPropFadeDistance = decalPropFadeDistance;
                BuildingLodDistance = buildingLodDistance;
                NetworkLodDistance = networkLodDistance;
                VehicleLodDistance = vehicleLodDistance;
            }

            internal float CitizenLodDistance { get; set; }
            internal float TreeLodDistance { get; set; }
            internal float PropLodDistance { get; set; }
            internal float DecalPropFadeDistance { get; set; }
            internal float BuildingLodDistance { get; set; }
            internal float NetworkLodDistance { get; set; }
            internal float VehicleLodDistance { get; set; }

            internal static LodConfig Low =>
                new LodConfig(256f,
                    256f,
                    10000f,
                    10000f,
                    10000f,
                    512f,
                    256f
                    );
            internal static LodConfig Mid =>
                new LodConfig(256f,
                    256f,
                    128f,
                    256f,
                    512f,
                    512f,
                    256f
                    );
            internal static LodConfig High =>
                new LodConfig(64f,
                    128f,
                    64f,
                    128f,
                    256f,
                    256f,
                    128f
                    );
            internal static LodConfig ActiveConfig = null;
        }
    }
}


