using AlgernonCommons;
using AlgernonCommons.Translation;
using FPSCamera.Cam;
using System.Collections.Generic;

namespace FPSCamera.Utils
{
    public static class InfoUtils
    {
        /// <summary>
        /// Retrieves geographical information about the current camera position.
        /// </summary>
        /// <returns>A dictionary containing geographical information.</returns>
        public static Dictionary<string, string> GetGeoInfo(IFPSCam fpsCam)
        {
            var info = new Dictionary<string, string>();
            var pos = fpsCam.GetPositioning().pos;
            var districtID = MapUtils.RayCastDistrict(pos);
            var parkID = MapUtils.RayCastPark(pos);
            var segID = MapUtils.RayCastRoad(pos);
            if (!districtID.IsEmpty)
            {
                var name = DistrictManager.instance.GetDistrictName(districtID.District);
                if (!string.IsNullOrEmpty(name))
                    info[Translations.Translate("INFO_DISTRICT")] = name;
            }
            if (!parkID.IsEmpty)
            {
                var name = DistrictManager.instance.GetParkName(parkID.Park);
                if (!string.IsNullOrEmpty(name))
                {
                    var parkGruop = DistrictPark.GetParkGroup(DistrictManager.instance.m_parks.m_buffer[parkID.Park].m_parkType);
                    switch (parkGruop)
                    {
                        case DistrictPark.ParkGroup.ParkLife:
                            info[Translations.Translate("INFO_DLCDISTRICT_PARK")] = name;
                            break;
                        case DistrictPark.ParkGroup.Industry:
                            info[Translations.Translate("INFO_DLCDISTRICT_INDUSTRY")] = name;
                            break;
                        case DistrictPark.ParkGroup.Campus:
                            info[Translations.Translate("INFO_DLCDISTRICT_CAMPUS")] = name;
                            break;
                        case DistrictPark.ParkGroup.Airport:
                            info[Translations.Translate("INFO_DLCDISTRICT_AIRPORT")] = name;
                            break;
                        case DistrictPark.ParkGroup.PedestrianZone:
                            info[Translations.Translate("INFO_DLCDISTRICT_PEDZONE")] = name;
                            break;
                        default:
                            Logging.Error($"Unknown parkGruop: {parkGruop}");
                            break;
                    }

                }

            }
            if (!segID.IsEmpty)
            {
                var name = NetManager.instance.GetSegmentName(segID.NetSegment);
                if (!string.IsNullOrEmpty(name))
                    info[Translations.Translate("INFO_ROAD")] = name;
            }
            return info;
        }

        internal static void GetMoreInfo(ref Dictionary<string, string> info, Vehicle vehicle, ushort vehicleid)
        {
            var modifyInfo = info;
            var ai = vehicle.Info.m_vehicleAI;
            switch (ai)
            {
                case BusAI _: TransitInfo(Translations.Translate("VEHICLE_AITYPE_BUS")); break;
                case TramAI _:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_TRAM")); break;
                case MetroTrainAI _:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_METRO")); break;
                case PassengerTrainAI _:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_TRAIN")); break;
                case PassengerPlaneAI _:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_FLIGHT")); break;
                case PassengerBlimpAI _:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_BLIMP")); break;
                case CableCarAI _:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_GONDOLA")); break;
                case TrolleybusAI _:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_TROLLEYBUS")); break;
                case PassengerFerryAI _:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_FERRY")); break;
                case PassengerShipAI _:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_SHIP")); break;
                case PassengerHelicopterAI _:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_HELICOPTER")); break;

                case CargoTruckAI _:
                case CargoTrainAI _:
                case CargoShipAI _:
                case CargoPlaneAI _: CargoInfo(); break;

                case AmbulanceAI _:
                case AmbulanceCopterAI _:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_MEDICAL")); break;
                case DisasterResponseVehicleAI _:
                case DisasterResponseCopterAI _:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_DISASTERRESPONSE")); break;
                case FireCopterAI _:
                case FireTruckAI _:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_FIREFIGHTING")); break;
                case PoliceCopterAI _:
                case PoliceCarAI _:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_POLICE")); break;
                case GarbageTruckAI _:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_GARBAGE")); break;
                case HearseAI _:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_DEATHCARE")); break;
                case PostVanAI _:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_POSTAL")); break;
                case SnowTruckAI _:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_SNOWPLOWING")); break;
                case WaterTruckAI _:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_WATERPUMPING")); break;
                case BankVanAI _:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_BANK")); break;
                case TaxiAI _:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_TAXI"), true); break;
                case MaintenanceTruckAI _:
                case ParkMaintenanceVehicleAI _:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_MAINTENANCE"), true); break;

                case PrivatePlaneAI _:
                case PassengerCarAI _:
                case BicycleAI _:
                case BalloonAI _:
                case FishingBoatAI _:
                case RocketAI _:
                    return;//These have no more info

                default:
                    Logging.Error($"Vehicle(ID:{vehicleid} of type [{ai.GetType().Name}] is not recognized.");
                    return;
            }
            info = modifyInfo;
            return;

            void TransitInfo(string typeName)
            {
                var transitID = vehicle.m_transportLine;
                var transitTypeKey = GetTranslateKey();
                var transitLineName = transitID != default ? TransportManager.instance.GetLineName(transitID) : Translations.Translate("INFO_VEHICLE_PUBLICTRANSIT_IRREGULAR");

                modifyInfo[Translations.Translate("INFO_VEHICLE_PUBLICTRANSIT_TRANSIT")] = $"{typeName}> {transitLineName}";

                var hasNextStation = TryGetNextStation(out var name);
                if (hasNextStation)
                    modifyInfo[Translations.Translate(transitTypeKey)] = name;

                vehicle.Info.m_vehicleAI.GetBufferStatus(vehicleid, ref vehicle, out _, out var load, out var capacity);
                modifyInfo[Translations.Translate("INFO_VEHICLE_PUBLICTRANSIT_PASSENGER")] = $"{load,4} /{capacity,4}";

                string GetTranslateKey() =>
                    typeName == Translations.Translate("VEHICLE_AITYPE_TRAM") ||
                    typeName == Translations.Translate("VEHICLE_AITYPE_BUS") ||
                    typeName == Translations.Translate("VEHICLE_AITYPE_TROLLEYBUS")
                       ? "INFO_VEHICLE_PUBLICTRANSIT_NEXTSTOP"
                       : "INFO_VEHICLE_PUBLICTRANSIT_NEXTSTATIION";

                bool TryGetNextStation(out string stopName)
                {
                    if (transitID != default)
                    {
                        stopName = TransportUtils.GetStationName(vehicle.m_targetBuilding, transitID);
                        return true;
                    }
                    stopName = null;
                    return false;
                }

            }
            void CargoInfo()
            {
                vehicle.Info.m_vehicleAI.GetBufferStatus(vehicleid, ref vehicle, out _, out var load, out var capacity);
                modifyInfo[Translations.Translate("INFO_VEHICLE_LOAD")] = capacity > 0 ? ((float)load / capacity).ToString("P1")
                                             : Translations.Translate("INVALID");
            }
            void ServiceInfo(string typeName, bool workShift = false)
            {
                modifyInfo[Translations.Translate("INFO_VEHICLE_SERVICE")] = typeName;
                vehicle.Info.m_vehicleAI.GetBufferStatus(vehicleid, ref vehicle, out _, out var load, out var capacity);
                if (capacity > 0)
                    if (workShift)
                        modifyInfo[Translations.Translate("INFO_VEHICLE_WORKSHIFT")] = ((float)load / capacity).ToString("P1");
                    else
                        modifyInfo[Translations.Translate("INFO_VEHICLE_LOAD")] = ((float)load / capacity).ToString("P1");
            }
        }
    }
}
