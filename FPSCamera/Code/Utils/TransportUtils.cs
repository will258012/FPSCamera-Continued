using ColossalFramework;
using UnityEngine;

namespace FPSCamera.Utils
{
    public class TransportUtils
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

        public static string GetStationName(ushort stopId, ushort lineId) => ModSupport.FoundTLM ? ModSupport.TLM_GetStopName(stopId, lineId) : GetStopName(stopId);

        private static string GetStopName(ushort stopId)
        {
            var id = new InstanceID() { NetNode = stopId };
            string savedName = InstanceManager.instance.GetName(id);
            if (!savedName.IsNullOrWhiteSpace())
                return savedName;

            var netManager = NetManager.instance;
            var node = netManager.m_nodes.m_buffer[stopId];
            var pos = node.m_position;
            //building
            ushort buildingId = FindTransportBuilding(pos, 100f);
            savedName = BuildingManager.instance.GetBuildingName(buildingId, InstanceID.Empty);
            if (!savedName.IsNullOrWhiteSpace())
                return savedName;

            //road
            savedName = $"{stopId} {GetStationRoadName(pos)}";
            if (!savedName.IsNullOrWhiteSpace())
                return savedName;

            //park
            savedName = GetStationParkName(pos);
            if (!savedName.IsNullOrWhiteSpace())
                return savedName;

            //district
            savedName = GetStationDistrictName(pos);
            if (!savedName.IsNullOrWhiteSpace())
                return savedName;

            return $"<Somewhere>[{stopId}]";
        }
        public static string GetLineCodeInTLM(ushort lineId) => !ModSupport.FoundTLM ? string.Empty : ModSupport.TLM_GetLineCode(lineId);

        private static string GetStationRoadName(Vector3 pos)
        {
            var segmentid = MapUtils.RayCastRoad(pos);
            var name = NetManager.instance.GetSegmentName(segmentid.NetSegment);
            return name;
        }
        private static string GetStationDistrictName(Vector3 pos)
        {
            var districtId = MapUtils.RayCastDistrict(pos);
            var name = DistrictManager.instance.GetDistrictName(districtId.District);
            return name;
        }
        private static string GetStationParkName(Vector3 pos)
        {
            var parkId = MapUtils.RayCastPark(pos);
            var name = DistrictManager.instance.GetParkName(parkId.Park);
            return name;
        }
        private static ushort FindTransportBuilding(Vector3 pos, float maxDistance)
        {
            foreach (var tType in stationTransportType)
            {
                ushort buildingId = BuildingManager.instance.FindTransportBuilding(pos, maxDistance, tType);

                if (buildingId != 0)
                {
                    if (BuildingManager.instance.m_buildings.m_buffer[buildingId].m_parentBuilding != 0)
                    {
                        buildingId = Building.FindParentBuilding(buildingId);
                    }
                    return buildingId;
                }
            }
            return default;
        }
    }

}
