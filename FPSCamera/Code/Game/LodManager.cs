using AlgernonCommons;
using System;
using System.Collections;
namespace FPSCamera.Game
{
    public class LodManager
    {
        public static IEnumerator ToggleLODOptimization(bool status)
        {
            try
            {
                if (status)
                {
                    LodConfig.SaveLodConfig();
                    LodConfig.ActiveConfig = LodConfig.Optimized;
                }
                else
                {
                    LodConfig.ActiveConfig = LodConfig.Saved;
                }
                Logging.Message("-- Refreshing LOD");
                RefreshLODs();
                if (!status) LodConfig.ActiveConfig = null;
            }

            catch (Exception e)
            {
                Logging.LogException(e);
            }
            yield break;
        }
        private static void RefreshLODs()
        {
            refreshLods<TreeInfo>();
            refreshLods<PropInfo>();
            refreshLods<BuildingInfo>();
            refreshLods<BuildingInfoSub>();
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

        public static float GetLodDistance<T>() where T : PrefabInfo
        {
            var prefab = PrefabCollection<T>.GetLoaded((uint)(PrefabCollection<T>.LoadedCount() - 1));
            switch (prefab)
            {
                case CitizenInfo info:
                    return info.m_lodRenderDistance;
                case TreeInfo info:
                    return info.m_lodRenderDistance;
                default:
                    throw new InvalidOperationException($"Unsupported PrefabInfo type: {typeof(T)}");
            }
        }
        public static float GetLodDistance<T>(T manager, bool IsPropManager_MaxRenderDistance = false) where T : ISimulationManager
        {
            switch (manager)
            {
                case BuildingManager buildingManager:
                    return PrefabCollection<BuildingInfo>.GetPrefab((uint)buildingManager.m_infoCount - 1).m_minLodDistance;
                case PropManager propManager when !IsPropManager_MaxRenderDistance:
                    return propManager.m_props.m_buffer[propManager.m_infoCount].Info.m_lodRenderDistance;
                case PropManager propManager when IsPropManager_MaxRenderDistance:
                    return propManager.m_props.m_buffer[propManager.m_infoCount].Info.m_maxRenderDistance;
                case NetManager netManager:
                    var count = PrefabCollection<NetInfo>.GetPrefab((uint)netManager.m_infoCount - 1);
                    return count.m_segments[count.m_segments.Length - 1].m_lodRenderDistance;
                case VehicleManager vehicleManager:
                    return PrefabCollection<VehicleInfo>.GetPrefab((uint)vehicleManager.m_infoCount - 1).m_lodRenderDistance;
                default:
                    throw new InvalidOperationException($"Unsupported SimulationManager type: {typeof(T)}");
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

            internal static LodConfig Saved => savedconfig;

            internal static LodConfig Optimized =>
                new LodConfig(64f,
                    128f,
                    32f,
                    128f,
                    256f,
                    256f,
                    128f
                    );

            internal static LodConfig ActiveConfig = null;
            private static LodConfig savedconfig;

            internal static void SaveLodConfig()
            {
                try
                {
                    savedconfig = new LodConfig(
                        citizenLodDistance: GetLodDistance<CitizenInfo>(),
                        treeLodDistance: GetLodDistance<TreeInfo>(),
                        propLodDistance: GetLodDistance(PropManager.instance),
                        decalPropFadeDistance: GetLodDistance(PropManager.instance, true),
                        buildingLodDistance: GetLodDistance(BuildingManager.instance),
                        networkLodDistance: GetLodDistance(NetManager.instance),
                        vehicleLodDistance: GetLodDistance(VehicleManager.instance));
                    if (Logging.DetailLogging)
                        Logging.Message($"Saved LOD Config:\n" +
                            $"  CitizenLodDistance = {savedconfig.CitizenLodDistance}\n" +
                            $"  TreeLodDistance = {savedconfig.TreeLodDistance}\n" +
                            $"  PropLodDistance = {savedconfig.PropLodDistance}\n" +
                            $"  DecalPropFadeDistance = {savedconfig.DecalPropFadeDistance}\n" +
                            $"  BuildingLodDistance = {savedconfig.BuildingLodDistance}\n" +
                            $"  NetworkLodDistance = {savedconfig.NetworkLodDistance}\n" +
                            $"  VehicleLodDistance = {savedconfig.VehicleLodDistance}\n");

                }
                catch (Exception e)
                {
                    Logging.LogException(e);
                }

            }

        }

    }
}


