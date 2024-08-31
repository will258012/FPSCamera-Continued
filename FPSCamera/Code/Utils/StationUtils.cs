using ColossalFramework;
using UnityEngine;

namespace FPSCamera.Utils
{
    public class StationUtils
    {
        public static readonly TransportInfo.TransportType[] stationTransportType =
        {
            TransportInfo.TransportType.Train,
            TransportInfo.TransportType.Metro,
            TransportInfo.TransportType.Monorail,
            TransportInfo.TransportType.Tram,
            TransportInfo.TransportType.Bus,
            TransportInfo.TransportType.TouristBus,
            TransportInfo.TransportType.Helicopter,
            TransportInfo.TransportType.Ship,
            TransportInfo.TransportType.Trolleybus,
            };
        private static string GetStopName(ushort stopId)
        {
            var id = new InstanceID() { NetNode = stopId };
            string savedName = InstanceManager.instance.GetName(id);
            if (!savedName.IsNullOrWhiteSpace())
            {
                return savedName;
            }

            var nm = Singleton<NetManager>.instance;
            var nn = nm.m_nodes.m_buffer[stopId];
            var pos = nn.m_position;
            //building
            ushort buildingId = FindTransportBuilding(pos, 100f);
            savedName = GetTransportBuildingName(buildingId);
            if (!savedName.IsNullOrWhiteSpace())
            {
                return savedName;
            }
            //road
            savedName = $"{stopId} {GetStationRoadName(pos)}";
            if (!savedName.IsNullOrWhiteSpace())
            {
                return savedName;
            }
            //district
            savedName = $"{GetStationDistrictName(pos)}";
            if (!savedName.IsNullOrWhiteSpace())
            {
                return savedName;
            }
            return $"<Somewhere>[{stopId}]";
        }

        private static string GetTransportBuildingName(ushort buildingId)
        {
            InstanceID bid = default;
            bid.Building = buildingId;
            return Singleton<BuildingManager>.instance.GetBuildingName(buildingId, bid);
        }
        public static string GetStationName(ushort stopId) => GetStopName(stopId) ?? "(" + stopId + ")";
        private static string GetStationRoadName(Vector3 pos)
        {
            var segmentid = MapUtils.RayCastRoad(pos);
            var name = NetManager.instance.GetSegmentName(segmentid.NetSegment);
            return name;
        }
        private static object GetStationDistrictName(Vector3 pos)
        {
            var districtId = MapUtils.RayCastDistrict(pos);
            var name = DistrictManager.instance.GetDistrictName(districtId.District);
            return name;
        }
        private static ushort FindTransportBuilding(Vector3 pos, float maxDistance)
        {
            var bm = Singleton<BuildingManager>.instance;

            foreach (var tType in stationTransportType)
            {
                ushort buildingid = bm.FindTransportBuilding(pos, maxDistance, tType);

                if (buildingid != 0)
                {
                    if (bm.m_buildings.m_buffer[buildingid].m_parentBuilding != 0)
                    {
                        buildingid = Building.FindParentBuilding(buildingid);
                    }
                    return buildingid;
                }
            }
            return default;
        }
    }

}
