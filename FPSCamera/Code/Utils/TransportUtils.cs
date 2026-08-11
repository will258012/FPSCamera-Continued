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
        /// Tracks public-transport passenger exchange from the most recently completed stop.
        /// The game calls UnloadPassengers before LoadPassengers: unload Prefix/Postfix measures
        /// alighting, then load Prefix/Postfix measures boarding from the post-unload passenger count.
        /// </summary>
        public static class PassengerExchangeTracker
        {
            public static bool TryGetExchange(out int alighted, out int boarded)
            {
                alighted = 0;
                boarded = 0;

                var snapshot = exchangeSnapshot;
                if (!snapshot.IsValid || snapshot.VehicleID == default)
                    return false;

                alighted = snapshot.AlightedCount;
                boarded = snapshot.BoardedCount;
                return true;
            }

            internal static ExchangeState BeginUnload(ushort vehicleID)
            {
                var state = BeginExchange(vehicleID);
                if (!state.IsValid)
                    return state;

#if DEBUG
                LogPassengerState("Unload Prefix", vehicleID, state.PreviousPassengerCount);
#endif
                return state;
            }

            internal static void EndUnload(ExchangeState state)
            {
                if (!IsCurrentExchange(state))
                    return;

                // UnloadPassengers Postfix: arrival load - remaining load = passengers who alighted.
                var passengerCount = GetPassengerCount(state.VehicleID);
                var alighted = Math.Max(0, state.PreviousPassengerCount - passengerCount);
#if DEBUG
                LogPassengerState("Unload Postfix", state.VehicleID, passengerCount, state.PreviousPassengerCount, alighted);
#endif
                exchangeSnapshot = new(state.VehicleID, true, alighted, 0);
                boardingBaseline = new(
                    state.VehicleID,
                    state.FollowVehicleID,
                    SimulationManager.instance.m_currentFrameIndex,
                    passengerCount);
            }

            internal static ExchangeState BeginBoarding(ushort vehicleID)
            {
                var baseline = boardingBaseline;
                boardingBaseline = BoardingBaseline.Empty;

                ExchangeState state;
                if (baseline.Matches(vehicleID, ModSupport.FollowVehicleID, SimulationManager.instance.m_currentFrameIndex))
                    state = new(vehicleID, baseline.FollowVehicleID, baseline.PassengerCount);
                else
                    state = BeginExchange(vehicleID);

                if (!state.IsValid)
                    return state;

                // LoadPassengers follows UnloadPassengers in the same simulation frame, so reuse the
                // post-unload count instead of querying the whole consist a second time.
#if DEBUG
                LogPassengerState("Load Prefix", vehicleID, state.PreviousPassengerCount);
#endif
                return state;
            }

            internal static void EndBoarding(ExchangeState state)
            {
                if (!IsCurrentExchange(state))
                    return;

                var snapshot = exchangeSnapshot;
                if (snapshot.VehicleID != state.VehicleID)
                    return;

                // LoadPassengers Postfix: final load - post-unload load = passengers who boarded.
                // Alighted passengers must not be added again because the Prefix runs after unloading.
                var passengerCount = GetPassengerCount(state.VehicleID);
                var boarded = Math.Max(0, passengerCount - state.PreviousPassengerCount);
#if DEBUG
                LogPassengerState("Load Postfix", state.VehicleID, passengerCount, state.PreviousPassengerCount, boarded);
#endif
                exchangeSnapshot = new(
                    state.VehicleID,
                    true,
                    snapshot.AlightedCount,
                    boarded);
            }

            internal static void Reset()
            {
                exchangeSnapshot = ExchangeSnapshot.Empty;
                boardingBaseline = BoardingBaseline.Empty;
            }

            internal static bool IsSameVehicleConsist(ushort firstVehicleID, ushort secondVehicleID)
            {
                if (firstVehicleID == default || secondVehicleID == default)
                    return false;

                var vehicles = VehicleManager.instance.m_vehicles.m_buffer;

                var firstVehicle = vehicles[firstVehicleID];
                var secondVehicle = vehicles[secondVehicleID];

                return firstVehicle.GetFirstVehicle(firstVehicleID) ==
                       secondVehicle.GetFirstVehicle(secondVehicleID);
            }

            private static ExchangeState BeginExchange(ushort vehicleID)
            {
                var followVehicleID = ModSupport.FollowVehicleID;
                if (!IsFollowingVehicle(vehicleID, followVehicleID))
                    return default;

                return new(vehicleID, followVehicleID, GetPassengerCount(vehicleID));
            }

            private static bool IsCurrentExchange(ExchangeState state)
                => state.IsValid && state.FollowVehicleID == ModSupport.FollowVehicleID;

            private static bool IsFollowingVehicle(ushort vehicleID, ushort followVehicleID)
            {
                if (vehicleID == default || followVehicleID == default || !(VehicleCam.GetVehicle(vehicleID).Info?.vehicleCategory.IsFlagSet(VehicleInfo.VehicleCategory.PublicTransport) ?? false))
                    return false;

                return vehicleID == GetHeadVehicleID(followVehicleID) && VehicleCam.GetVehicle(vehicleID).m_leadingVehicle == default;
            }
            private static ushort GetHeadVehicleID(ushort followVehicleID) => VehicleCam.GetVehicle(followVehicleID).GetFirstVehicle(followVehicleID);

            private static int GetPassengerCount(ushort vehicleID)
            {
                if (vehicleID == default)
                    return default;

                var vehicle = VehicleCam.GetVehicle(vehicleID);
                // GetBufferStatus supplies the total load, including trailers where applicable.
                var load = 0;
                VehicleCam.GetVehicle(vehicleID).Info?.m_vehicleAI?.GetBufferStatus(vehicleID, ref vehicle, out _, out load, out _);
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

            private sealed class BoardingBaseline
            {
                internal static readonly BoardingBaseline Empty = new(default, default, default, default);

                internal BoardingBaseline(ushort vehicleID, ushort followVehicleID, uint frameIndex, int passengerCount)
                {
                    VehicleID = vehicleID;
                    FollowVehicleID = followVehicleID;
                    FrameIndex = frameIndex;
                    PassengerCount = passengerCount;
                }

                internal ushort VehicleID { get; }
                internal ushort FollowVehicleID { get; }
                internal uint FrameIndex { get; }
                internal int PassengerCount { get; }

                internal bool Matches(ushort vehicleID, ushort followVehicleID, uint frameIndex)
                    => VehicleID != default &&
                       VehicleID == vehicleID &&
                       FollowVehicleID == followVehicleID &&
                       FrameIndex == frameIndex;
            }

            internal readonly struct ExchangeState
            {
                internal ExchangeState(ushort vehicleID, ushort followVehicleID, int previousPassengerCount)
                {
                    VehicleID = vehicleID;
                    FollowVehicleID = followVehicleID;
                    PreviousPassengerCount = previousPassengerCount;
                }

                internal ushort VehicleID { get; }
                internal ushort FollowVehicleID { get; }
                internal int PreviousPassengerCount { get; }
                internal bool IsValid => VehicleID != default && FollowVehicleID != default;
            }

            private static volatile ExchangeSnapshot exchangeSnapshot = ExchangeSnapshot.Empty;
            private static volatile BoardingBaseline boardingBaseline = BoardingBaseline.Empty;
        }
    }

}
