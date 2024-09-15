namespace FPSCamera.Utils
{
    using UnityEngine;
    using static FPSCamera.Utils.MathUtils;

    public static class MapUtils
    {
        const float defaultHeightOffset = 2f;

        public static float ToKilometer(this float gameDistance)
            => gameDistance * 5f / 3f;

        public static float ToMile(this float gameDistance)
            => gameDistance.ToKilometer() * .621371f;

        public static float GetTerrainLevel(Vector3 pos)
            => TerrainManager.instance.SampleDetailHeightSmooth(pos);
        public static float GetWaterLevel(Vector3 pos)
            => TerrainManager.instance.WaterLevel(pos);

        public static float? GetClosestSegmentLevel(Vector3 pos)
        {

            var input = Tool.GetRaycastInput(pos, new Range(-100f, 2f));
            input.m_netService.m_service = ItemClass.Service.Road;
            input.m_netService.m_itemLayers = ItemClass.Layer.Default |
                                              ItemClass.Layer.PublicTransport |
                                              ItemClass.Layer.MetroTunnels;

            input.m_netService2.m_service = ItemClass.Service.Beautification;

            input.m_ignoreSegmentFlags = NetSegment.Flags.Deleted |
                                                 NetSegment.Flags.Collapsed |
                                               NetSegment.Flags.Flooded;
            return Tool.RayCast(input, 5f) is ToolBase.RaycastOutput result ?
                    (float?)result.m_hitPos.y + defaultHeightOffset : null;
        }

        public static float GetMinHeightAt(Vector3 position)
            => Mathf.Max(GetTerrainLevel(position), GetWaterLevel(position)) + defaultHeightOffset;

        public static InstanceID RayCastRoad(Vector3 position)
        {
            var input = Tool.GetRaycastInput(position);
            input.m_netService.m_service = ItemClass.Service.Road;
            input.m_netService.m_itemLayers = ItemClass.Layer.Default |
                                              ItemClass.Layer.PublicTransport;

            input.m_netService2.m_service = ItemClass.Service.Beautification;

            input.m_ignoreSegmentFlags = NetSegment.Flags.None;
            return Tool.RayCast(input, 5f) is ToolBase.RaycastOutput result ?
                   new InstanceID() { NetSegment = result.m_netSegment } : default;
        }

        public static InstanceID RayCastDistrict(Vector3 position)
        {
            var input = Tool.GetRaycastInput(position);
            input.m_ignoreDistrictFlags = District.Flags.None;

            return Tool.RayCast(input, 5f) is ToolBase.RaycastOutput result ?
                   new InstanceID() { District = result.m_district } : default;
        }

        public static InstanceID RayCastPark(Vector3 position)
        {
            var input = Tool.GetRaycastInput(position);
            input.m_ignoreParkFlags = DistrictPark.Flags.None | DistrictPark.Flags.Invalid;

            return Tool.RayCast(input, 5f) is ToolBase.RaycastOutput result ?
                  new InstanceID() { Park = result.m_park } : default;
        }
        private class Tool : ToolBase
        {
            internal static RaycastOutput? RayCast(
                    RaycastInput rayCastInput, float offset = 0f)
            {
                if (offset.AlmostEquals(0f))
                    return RayCast(rayCastInput, out var result) ?
                           (RaycastOutput?)result : null;
                foreach (var delta in new Vector3[] { Vector3.zero, Vector3.forward * offset,
                                                  Vector3.left * offset, Vector3.right * offset,
                                                  Vector3.back * offset })
                {
                    var input = rayCastInput;
                    input.m_ray.origin = rayCastInput.m_ray.origin + delta;
                    if (RayCast(input, out var result)) return result;
                }
                return null;
            }

            internal static RaycastInput GetRaycastInput(Vector3 position)
                => GetRaycastInput(position, new Range(-100f, 100f));
            internal static RaycastInput GetRaycastInput(Vector3 position,
                                                          Range verticalRange)
            {
                var input = new RaycastInput(
                                new Ray(position + Vector3.up * verticalRange.max, Vector3.down),
                                        verticalRange.Size)
                { m_ignoreTerrain = true };
                return input;
            }
        }

    }
}