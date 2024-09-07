using AlgernonCommons;
using AlgernonCommons.Translation;
using FPSCamera.Cam;
using System.Collections.Generic;

namespace FPSCamera.Utils
{
    public static class InfosUtils
    {
        /// <summary>
        /// Retrieves geographical information about the current camera position.
        /// </summary>
        /// <returns>A dictionary containing geographical information.</returns>
        public static Dictionary<string, string> GetGeoInfos(IFPSCam followCam)
        {
            var infos = new Dictionary<string, string>();
            var pos = followCam.GetPositioning().pos;
            if (MapUtils.RayCastDistrict(pos) is InstanceID disID && disID.District != default)
            {
                var name = DistrictManager.instance.GetDistrictName(disID.District);
                if (!string.IsNullOrEmpty(name))
                    infos[Translations.Translate("INFO_DISTRICT")] = name;
            }
            if (MapUtils.RayCastPark(pos) is InstanceID parkID && parkID.Park != default)
            {
                var name = DistrictManager.instance.GetParkName(parkID.Park);
                if (!string.IsNullOrEmpty(name))
                {
                    switch (DistrictPark.GetParkGroup(DistrictManager.instance.m_parks.m_buffer[parkID.Park].m_parkType))
                    {
                        case DistrictPark.ParkGroup.ParkLife:
                            infos[Translations.Translate("INFO_DLCDISTRICT_PARK")] = name;
                            break;
                        case DistrictPark.ParkGroup.Industry:
                            infos[Translations.Translate("INFO_DLCDISTRICT_INDUSTRY")] = name;
                            break;
                        case DistrictPark.ParkGroup.Campus:
                            infos[Translations.Translate("INFO_DLCDISTRICT_CAMPUS")] = name;
                            break;
                        case DistrictPark.ParkGroup.Airport:
                            infos[Translations.Translate("INFO_DLCDISTRICT_AIRPORT")] = name;
                            break;
                        case DistrictPark.ParkGroup.PedestrianZone:
                            infos[Translations.Translate("INFO_DLCDISTRICT_PEDZONE")] = name;
                            break;
                        default:
                            infos["DLC District"] = name;
                            break;
                    }

                }

            }
            if (MapUtils.RayCastRoad(pos) is InstanceID segID && segID.NetSegment != default)
            {
                var name = NetManager.instance.GetSegmentName(segID.NetSegment);
                if (!string.IsNullOrEmpty(name))
                    infos[Translations.Translate("INFO_ROAD")] = name;
            }
            return infos;
        }
        internal static void GetMoreInfos(ref Dictionary<string, string> infos, Vehicle vehicle, ushort vehicleid)
        {
            var modifyinfos = infos;
            var ai = vehicle.Info.m_vehicleAI;
            switch (ai)
            {
                case BusAI _: TransitInfos(Translations.Translate("VEHICLE_AITYPE_BUS")); break;
                case TramAI _:
                    TransitInfos(Translations.Translate("VEHICLE_AITYPE_TRAM")); break;
                case MetroTrainAI _:
                    TransitInfos(Translations.Translate("VEHICLE_AITYPE_METRO")); break;
                case PassengerTrainAI _:
                    TransitInfos(Translations.Translate("VEHICLE_AITYPE_TRAIN")); break;
                case PassengerPlaneAI _:
                    TransitInfos(Translations.Translate("VEHICLE_AITYPE_FLIGHT")); break;
                case PassengerBlimpAI _:
                    TransitInfos(Translations.Translate("VEHICLE_AITYPE_BLIMP")); break;
                case CableCarAI _:
                    TransitInfos(Translations.Translate("VEHICLE_AITYPE_GONDOLA")); break;
                case TrolleybusAI _:
                    TransitInfos(Translations.Translate("VEHICLE_AITYPE_TROLLEYBUS")); break;
                case PassengerFerryAI _:
                    TransitInfos(Translations.Translate("VEHICLE_AITYPE_FERRY")); break;
                case PassengerShipAI _:
                    TransitInfos(Translations.Translate("VEHICLE_AITYPE_SHIP")); break;
                case PassengerHelicopterAI _:
                    TransitInfos(Translations.Translate("VEHICLE_AITYPE_HELICOPTER")); break;

                case CargoTruckAI _:
                case CargoTrainAI _:
                case CargoShipAI _:
                case CargoPlaneAI _: CargoInfos(); break;

                case AmbulanceAI _:
                case AmbulanceCopterAI _:
                    ServiceInfos(Translations.Translate("VEHICLE_AITYPE_MEDICAL")); break;
                case DisasterResponseVehicleAI _:
                case DisasterResponseCopterAI _:
                    ServiceInfos(Translations.Translate("VEHICLE_AITYPE_DISASTERRESPONSE")); break;
                case FireCopterAI _:
                case FireTruckAI _:
                    ServiceInfos(Translations.Translate("VEHICLE_AITYPE_FIREFIGHTING")); break;
                case PoliceCopterAI _:
                case PoliceCarAI _:
                    ServiceInfos(Translations.Translate("VEHICLE_AITYPE_POLICE")); break;
                case GarbageTruckAI _:
                    ServiceInfos(Translations.Translate("VEHICLE_AITYPE_GARBAGE")); break;
                case HearseAI _:
                    ServiceInfos(Translations.Translate("VEHICLE_AITYPE_DEATHCARE")); break;
                case PostVanAI _:
                    ServiceInfos(Translations.Translate("VEHICLE_AITYPE_POSTAL")); break;
                case SnowTruckAI _:
                    ServiceInfos(Translations.Translate("VEHICLE_AITYPE_SNOWPLOWING")); break;
                case WaterTruckAI _:
                    ServiceInfos(Translations.Translate("VEHICLE_AITYPE_WATERPUMPING")); break;
                case BankVanAI _:
                    ServiceInfos(Translations.Translate("VEHICLE_AITYPE_BANK")); break;
                case TaxiAI _:
                    ServiceInfos(Translations.Translate("VEHICLE_AITYPE_TAXI"), true); break;
                case MaintenanceTruckAI _:
                case ParkMaintenanceVehicleAI _:
                    ServiceInfos(Translations.Translate("VEHICLE_AITYPE_MAINTENANCE"), true); break;

                case PrivatePlaneAI _:
                case PassengerCarAI _:
                case BicycleAI _:
                case BalloonAI _:
                case FishingBoatAI _:
                case RocketAI _:
                    return;//These have no more infos

                default:
                    Logging.Error($"Vehicle(ID:{vehicleid} of type [{ai.GetType().Name}] is not recognized.");
                    return;
            }
            infos = modifyinfos;
            return;

            void TransitInfos(string typename)
            {
                var transitID = vehicle.m_transportLine;
                var transitTypeKey = GetTranslateKey();
                var transitLineName = transitID != default ? TransportManager.instance.GetLineName(transitID) : Translations.Translate("INFO_VEHICLE_PUBLICTRANSIT_IRREGULAR");

                modifyinfos[Translations.Translate("INFO_VEHICLE_PUBLICTRANSIT_TRANSIT")] = $"{typename}> {transitLineName}";
                var hasNextStation = TryGetNextStation(out var name);
                if (hasNextStation == true)
                    modifyinfos[Translations.Translate(transitTypeKey)] = name;
                vehicle.Info.m_vehicleAI.GetBufferStatus(vehicleid, ref vehicle, out _, out var load, out var capacity);
                modifyinfos[Translations.Translate("INFO_VEHICLE_PUBLICTRANSIT_PASSENGER")] = $"{load,4} /{capacity,4}";

                string GetTranslateKey() =>
                    typename == Translations.Translate("VEHICLE_AITYPE_TRAM") ||
                    typename == Translations.Translate("VEHICLE_AITYPE_BUS") ||
                    typename == Translations.Translate("VEHICLE_AITYPE_TROLLEYBUS")
                       ? "INFO_VEHICLE_PUBLICTRANSIT_NEXTSTOP"
                       : "INFO_VEHICLE_PUBLICTRANSIT_NEXTSTATIION";

                bool TryGetNextStation(out string stopname)
                {
                    if (transitID != default)
                    {
                        stopname = StationUtils.GetStationName(vehicle.m_targetBuilding);
                        return true;
                    }
                    stopname = null;
                    return false;
                }

            }
            void CargoInfos()
            {
                vehicle.Info.m_vehicleAI.GetBufferStatus(vehicleid, ref vehicle, out _, out var load, out var capacity);
                modifyinfos[Translations.Translate("INFO_VEHICLE_LOAD")] = capacity > 0 ? ((float)load / capacity).ToString("P1")
                                             : Translations.Translate("INVALID");
            }
            void ServiceInfos(string typename, bool work_shift = false)
            {
                modifyinfos[Translations.Translate("INFO_VEHICLE_SERVICE")] = typename;
                vehicle.Info.m_vehicleAI.GetBufferStatus(vehicleid, ref vehicle, out _, out var load, out var capacity);
                if (work_shift)
                {
                    if (capacity > 0) modifyinfos[Translations.Translate("INFO_VEHICLE_WORKSHIFT")] = ((float)load / capacity).ToString("P1");
                }
                else
                {
                    if (capacity > 0) modifyinfos[Translations.Translate("INFO_VEHICLE_LOAD")] = ((float)load / capacity).ToString("P1");
                }
            }
        }


    }
}
