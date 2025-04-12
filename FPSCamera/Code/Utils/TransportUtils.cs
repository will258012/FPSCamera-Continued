using ColossalFramework;
using TransportLinesManager.ModShared;
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

        public static string GetStationName(ushort stopId, ushort lineId)
        {
            return ModSupport.FoundTLM ? GetStopNameByTLMImpt(stopId, lineId) : GetStopName(stopId);
        }
        private static string GetStopNameByTLMImpt(ushort stopId, ushort lineId)
        {
            var subService = TransportManager.instance.m_lines.m_buffer[lineId].Info.m_netSubService;
            return TLMFacade.GetFullStationName(stopId, lineId, false, subService);
        }
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
            savedName = BuildingManager.instance.GetBuildingName(buildingId, InstanceID.Empty);
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
            //park
            savedName = GetStationParkName(pos);
            if (!savedName.IsNullOrWhiteSpace())
            {
                return savedName;
            }
            //district
            savedName = GetStationDistrictName(pos);
            if (!savedName.IsNullOrWhiteSpace())
            {
                return savedName;
            }
            return $"<Somewhere>[{stopId}]";
        }
        public static string GetLineCodeInTLM(ushort lineId)
        {
            if (!ModSupport.FoundTLM) return string.Empty;
            return GetLineCodeInTLMImpl(lineId);
        }
        private static string GetLineCodeInTLMImpl(ushort lineId) => TLMFacade.GetLineStringId(lineId, false);
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
