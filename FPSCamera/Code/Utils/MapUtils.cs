namespace FPSCamera.Utils
{
    using FPSCamera.Settings;
    using UnityEngine;

    public static class MapUtils
    {
        const float defaultHeightOffset = 2f;

        public static float ToKilometer(this float gameDistance)
            => gameDistance * 5f / 3f;

        public static float ToMile(this float gameDistance)
            => gameDistance.ToKilometer() * .621371f;

        public static float GetTerrainLevel(Vector3 position)
            => TerrainManager.instance.SampleDetailHeightSmooth(position);
        public static float GetWaterLevel(Vector3 position)
            => TerrainManager.instance.WaterLevel(position);

        /// <summary>
        /// Retrieves the height of the closest road segment at the given position.
        /// </summary>
        /// <param name="position">The world position where the height needs to be calculated.</param>
        /// <param name="height">Outputs the height of the nearest road segment at the specified position.</param>
        /// <returns>A boolean indicating whether a valid road segment height was found.</returns>
        public static bool GetClosestSegmentLevel(Vector3 position, out float height)
        {
            height = default;
            var input = RayCastTool.GetRaycastInput(position, -5f, 5f); // Configure raycast input parameters
            input.m_netService.m_service = ItemClass.Service.Road;
            input.m_netService.m_itemLayers = ItemClass.Layer.Default |// ItemClass.Layer.PublicTransport is sonly for TransportLine, not for Road.
                                              ItemClass.Layer.MetroTunnels;
            if (ModSettings.PathsDetection)
                input.m_netService2.m_service = ItemClass.Service.Beautification; // For paths

            input.m_ignoreSegmentFlags = NetSegment.Flags.Deleted |
                                         NetSegment.Flags.Collapsed |
                                         NetSegment.Flags.Flooded;

            // Perform the raycast and check for a result:
            if (RayCastTool.RayCast(input, out var result, 5f))
            {
                // Set the height to the hit position plus the default offset.
                height = result.m_hitPos.y + defaultHeightOffset;
                return true;
            }

            if (ModSettings.TracksDetection)
            {
                // If no result, change the service to ItemClass.Service.PublicTransport (for tracks).
                input.m_netService.m_service = ItemClass.Service.PublicTransport;

                // Perform the raycast again:
                if (RayCastTool.RayCast(input, out var result2, 5f))
                {
                    height = result2.m_hitPos.y + defaultHeightOffset;
                    return true;
                }
            }
            // Return false if no valid result was found.
            return false;
        }
        public static float GetMinHeightAt(Vector3 position)
            => Mathf.Max(GetTerrainLevel(position), GetWaterLevel(position)) + defaultHeightOffset;
        public static InstanceID RayCastRoad(Vector3 position)
        {
            var input = RayCastTool.GetRaycastInput(position);
            input.m_netService.m_service = ItemClass.Service.Road;
            input.m_netService2.m_service = ItemClass.Service.Beautification;

            input.m_netService.m_itemLayers = ItemClass.Layer.Default;
            input.m_ignoreSegmentFlags = NetSegment.Flags.None;

            return RayCastTool.RayCast(input, out var result, 5f) ?
                   new InstanceID() { NetSegment = result.m_netSegment } : default;
        }
        public static InstanceID RayCastDistrict(Vector3 position)
        {
            var input = RayCastTool.GetRaycastInput(position);
            input.m_ignoreDistrictFlags = District.Flags.None;

            return RayCastTool.RayCast(input, out var result, 5f) ?
                   new InstanceID() { District = result.m_district } : default;
        }
        public static InstanceID RayCastPark(Vector3 position)
        {
            var input = RayCastTool.GetRaycastInput(position);
            input.m_ignoreParkFlags = DistrictPark.Flags.None | DistrictPark.Flags.Invalid;

            return RayCastTool.RayCast(input, out var result, 5f) ?
                   new InstanceID() { Park = result.m_park } : default;
        }

        public class RayCastTool : ToolBase
        {
            /// <summary>
            /// Performs a raycast with an optional positional offset to handle nearby objects.
            /// If an offset is provided, the method will attempt multiple raycasts around the original position to find a valid hit.
            /// </summary>
            /// <param name="rayCastInput">The input parameters for the raycast, including origin and direction.</param>
            /// <param name="result">A <see cref="ToolBase.RaycastOutput"/> with the hit details.</param>
            /// <param name="offset">An optional offset to adjust the raycast's origin position. Defaults to 0f.</param>
            /// <returns>
            /// A boolean indicating whether the raycast was successful.
            /// </returns>
            public static bool RayCast(RaycastInput rayCastInput, out RaycastOutput result, float offset = 0f)
            {
                result = default;
                if (offset.AlmostEquals(0f))
                {
                    // Perform a single raycast if no offset is provided
                    if (ToolBase.RayCast(rayCastInput, out var result2))
                    {
                        result = result2;
                        return true;
                    }
                    else return false;
                }

                // Perform multiple raycasts with offsets if provided
                foreach (var delta in new Vector3[] { Vector3.zero, Vector3.forward * offset,
                                          Vector3.left * offset, Vector3.right * offset,
                                          Vector3.back * offset })
                {
                    var input = rayCastInput;
                    input.m_ray.origin = rayCastInput.m_ray.origin + delta;
                    if (ToolBase.RayCast(input, out var result2))
                    {
                        result = result2;
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Constructs a <see cref="ToolBase.RaycastInput"/> object with default vertical range parameters for a given position.
            /// The raycast is directed downwards from a height of 100 units above the position.
            /// </summary>
            /// <param name="position">The world position from which the raycast will originate.</param>
            /// <returns>
            /// A <see cref="ToolBase.RaycastInput"/> configured with a downward ray starting from 100 units above the specified position.
            /// </returns>
            public static RaycastInput GetRaycastInput(Vector3 position)
                => GetRaycastInput(position, -100f, 100f);

            /// <summary>
            /// Creates a RaycastInput object with specified minimum and maximum vertical offsets for the raycast.
            /// The raycast is directed downwards from the maximum height to the minimum height.
            /// </summary>
            /// <param name="position">The world position from which the raycast will originate.</param>
            /// <param name="min">The minimum vertical offset for the raycast.</param>
            /// <param name="max">The maximum vertical offset for the raycast.</param>
            /// <returns>
            /// A <see cref="ToolBase.RaycastInput"/> configured with a downward ray spanning from (position + max height) to (position + min height).
            /// </returns>
            public static RaycastInput GetRaycastInput(Vector3 position,
                                                          float min, float max)
            {
                var input = new RaycastInput(
                                new Ray(position + Vector3.up * max, Vector3.down),
                                        max - min)
                { m_ignoreTerrain = true };
                return input;
            }
        }

    }
}
