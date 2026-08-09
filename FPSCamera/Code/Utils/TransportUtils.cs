using ColossalFramework;
using FPSCamera.Cam;
using System;
#if DEBUG
using AlgernonCommons;
using System.Text;
#endif
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

                var passengerCount = GetPassengerCount(vehicleID);
#if DEBUG
                LogPassengerState("Unload Prefix", vehicleID, passengerCount);
#endif
                return passengerCount;
            }

            internal static void EndUnload(ushort vehicleID, int previousPassengerCount)
            {
                if (previousPassengerCount < 0 || !IsFollowingVehicle(vehicleID))
                    return;

                // UnloadPassengers Postfix: arrival load - remaining load = passengers who alighted.
                var passengerCount = GetPassengerCount(vehicleID);
                var alighted = Math.Max(0, previousPassengerCount - passengerCount);
#if DEBUG
                LogPassengerState("Unload Postfix", vehicleID, passengerCount, previousPassengerCount, alighted);
#endif
                exchangeSnapshot = new(vehicleID, true, alighted, 0);
            }

            internal static int BeginBoarding(ushort vehicleID)
            {
                if (!IsFollowingVehicle(vehicleID))
                    return -1;

                // LoadPassengers Prefix runs after unloading; capture the remaining load for the whole consist.
                var passengerCount = GetPassengerCount(vehicleID);
#if DEBUG
                LogPassengerState("Load Prefix", vehicleID, passengerCount);
#endif
                return passengerCount;
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
                var passengerCount = GetPassengerCount(vehicleID);
                var boarded = Math.Max(0, passengerCount - previousPassengerCount);
#if DEBUG
                LogPassengerState("Load Postfix", vehicleID, passengerCount, previousPassengerCount, boarded);
#endif
                exchangeSnapshot = new(
                    vehicleID,
                    true,
                    snapshot.AlightedCount,
                    boarded);
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

#if DEBUG
            private static void LogPassengerState(string phase, ushort vehicleID, int reportedLoad, int previousLoad = -1, int exchangeCount = -1)
            {
                var vehicles = VehicleManager.instance.m_vehicles.m_buffer;
                if (vehicleID == default || vehicleID >= vehicles.Length)
                {
                    Logging.Message($"Passenger exchange {phase}: invalid vehicle {vehicleID}");
                    return;
                }

                var chain = new StringBuilder();
                var reportingVehicle = vehicles[vehicleID];
                var lineID = reportingVehicle.m_transportLine;
                var stopID = reportingVehicle.m_targetBuilding;
                var stationName = lineID != default && stopID != default
                    ? TransportUtils.GetStationName(stopID, lineID)
                    : "<unavailable>";
                var verifiedLoad = 0;
                var reportedCapacity = 0;
                reportingVehicle.Info?.m_vehicleAI?.GetBufferStatus(
                    vehicleID,
                    ref reportingVehicle,
                    out _,
                    out verifiedLoad,
                    out reportedCapacity);
                var currentVehicleID = vehicleID;
                var rawTransferSize = 0;
                var rawCapacity = 0;
                var vehicleCount = 0;
                while (currentVehicleID != default && currentVehicleID < vehicles.Length && vehicleCount < 16384)
                {
                    var currentVehicle = vehicles[currentVehicleID];
                    var info = currentVehicle.Info;
                    var capacity = info?.m_vehicleAI?.GetPassengerCapacity(false) ?? 0;

                    if (chain.Length > 0)
                        chain.Append(" -> ");
                    chain.Append(currentVehicleID)
                         .Append('[')
                         .Append(info?.m_vehicleAI?.GetType().Name ?? "NoAI")
                         .Append(" transfer=")
                         .Append(currentVehicle.m_transferSize)
                         .Append(" capacity=")
                         .Append(capacity)
                         .Append(']');

                    rawTransferSize += currentVehicle.m_transferSize;
                    rawCapacity += capacity;
                    currentVehicleID = currentVehicle.m_trailingVehicle;
                    vehicleCount++;
                }

                if (currentVehicleID != default)
                    chain.Append(currentVehicleID >= vehicles.Length ? " -> invalid ID" : " -> traversal limit reached");

                Logging.Message(
                    $"Passenger exchange {phase}: vehicle={vehicleID}, followed={ModSupport.FollowVehicleID}, " +
                    $"line={lineID}, stop={stopID}, station=\"{stationName}\", " +
                    $"reportedLoad={reportedLoad}, verifiedLoad={verifiedLoad}, reportedCapacity={reportedCapacity}, " +
                    $"previousLoad={previousLoad}, exchange={exchangeCount}, " +
                    $"rawTransferSum={rawTransferSize}, rawCapacitySum={rawCapacity}, chain={chain}");
            }
#endif

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