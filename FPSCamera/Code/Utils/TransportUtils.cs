using ColossalFramework;
using FPSCamera.Cam;
using FPSCamera.Cam.Controller;
using System;
using UnityEngine;

namespace FPSCamera.Utils
{
    public class TransportUtils
    {
        public static readonly TransportInfo.TransportType[] stationTransportType =
        [
            TransportInfo.TransportType.Train,
            TransportInfo.TransportType.Metro,
            TransportInfo.TransportType.Monorail,
            TransportInfo.TransportType.Tram,
            TransportInfo.TransportType.Bus,
            TransportInfo.TransportType.TouristBus,
            TransportInfo.TransportType.Helicopter,
            TransportInfo.TransportType.Ship,
            TransportInfo.TransportType.Trolleybus,
            ];

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
        /// <summary>
        /// Tracks public-transport passenger exchange at the current stop.
        /// The game calls UnloadPassengers before LoadPassengers: unload Prefix/Postfix measures
        /// alighting, then load Prefix/Postfix measures boarding from the post-unload passenger count.
        /// </summary>
        public static class PassengerExchangeTracker
        {
            public static bool TryGetExchange(out int alighted, out int boarded)
            {
                alighted = 0;
                boarded = 0;

                var vehicleID = GetHeadVehicleID();
                if (vehicleID == default || !IsStopped(vehicleID))
                    return false;

                var snapshot = exchangeSnapshot;
                if (!snapshot.IsValid || snapshot.VehicleID != vehicleID)
                    return false;

                alighted = snapshot.AlightedCount;
                boarded = snapshot.BoardedCount;
                return true;
            }

            internal static int BeginUnload(ushort vehicleID)
            {
                if (!IsFollowingVehicle(vehicleID))
                    return -1;

                exchangeSnapshot = new ExchangeSnapshot(vehicleID, false, 0, 0);

                return GetPassengerCount(vehicleID);
            }

            internal static void EndUnload(ushort vehicleID, int previousPassengerCount)
            {
                if (previousPassengerCount < 0 || !IsFollowingVehicle(vehicleID))
                    return;

                // UnloadPassengers Postfix: arrival load - remaining load = passengers who alighted.
                var alighted = Math.Max(0, previousPassengerCount - GetPassengerCount(vehicleID));
                exchangeSnapshot = new(vehicleID, true, alighted, 0);
            }

            internal static int BeginBoarding(ushort vehicleID)
            {
                if (!IsFollowingVehicle(vehicleID))
                    return -1;

                // LoadPassengers Prefix runs after unloading; capture the remaining load for the whole consist.
                return GetPassengerCount(vehicleID);
            }

            internal static void EndBoarding(ushort vehicleID, int previousPassengerCount)
            {
                if (previousPassengerCount < 0 || !IsFollowingVehicle(vehicleID))
                    return;

                var snapshot = exchangeSnapshot;
                if (snapshot.VehicleID != vehicleID)
                    return;

                // LoadPassengers Postfix: final load - post-unload load = passengers who boarded.
                // Alighted passengers must not be added again because the Prefix runs after unloading.
                exchangeSnapshot = new(
                    vehicleID,
                    true,
                    snapshot.AlightedCount,
                    Math.Max(0, GetPassengerCount(vehicleID) - previousPassengerCount));
            }

            internal static void Reset()
            {
                exchangeSnapshot = ExchangeSnapshot.Empty;
            }

            private static bool IsStopped(ushort vehicleID)
            {
                var vehicles = VehicleManager.instance.m_vehicles.m_buffer;
                return vehicleID < vehicles.Length &&
                       vehicles[vehicleID].m_flags.IsFlagSet(Vehicle.Flags.Stopped | Vehicle.Flags.WaitingLoading);
            }

            private static bool IsFollowingVehicle(ushort vehicleID)
            {
                if (vehicleID == default || ModSupport.FollowVehicleID == default || vehicleID != GetHeadVehicleID())
                    return false;

                return VehicleCam.GetVehicle(vehicleID).m_leadingVehicle == default;
            }
            private static ushort GetHeadVehicleID() => VehicleCam.GetVehicle(ModSupport.FollowVehicleID).GetFirstVehicle(ModSupport.FollowVehicleID);

            private static int GetPassengerCount(ushort vehicleID)
            {
                if (vehicleID == default)
                    return default;

                var vehicle = VehicleCam.GetVehicle(vehicleID);
                // GetBufferStatus supplies the total load, including trailers where applicable.
                var load = 0;
                Cam.VehicleCam.GetVehicle(vehicleID).Info?.m_vehicleAI?.GetBufferStatus(vehicleID, ref vehicle, out _, out load, out _);
                return load;
            }

            private sealed class ExchangeSnapshot
            {
                internal static readonly ExchangeSnapshot Empty = new(default, false, 0, 0);

                internal ExchangeSnapshot(ushort vehicleID, bool isValid, int alightedCount, int boardedCount)
                {
                    VehicleID = vehicleID;
                    IsValid = isValid;
                    AlightedCount = alightedCount;
                    BoardedCount = boardedCount;
                }

                internal ushort VehicleID { get; }
                internal bool IsValid { get; }
                internal int AlightedCount { get; }
                internal int BoardedCount { get; }
            }

            private static volatile ExchangeSnapshot exchangeSnapshot = ExchangeSnapshot.Empty;
        }
    }

}
