using AlgernonCommons;
using AlgernonCommons.Translation;
using ColossalFramework;
using ColossalFramework.Math;
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

        internal static void GetMoreInfo(ref Dictionary<string, string> info, Vehicle vehicle, ushort vehicleid, bool isRace)
        {
            var modifyInfo = info;
            var ai = vehicle.Info.m_vehicleAI;

            if (isRace)
            {
                RaceInfo();
                return;
            }

            switch (ai)
            {
                case BusAI:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_BUS")); break;
                case TramAI:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_TRAM")); break;
                case MetroTrainAI:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_METRO")); break;
                case PassengerTrainAI:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_TRAIN")); break;
                case PassengerPlaneAI:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_FLIGHT")); break;
                case PassengerBlimpAI:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_BLIMP")); break;
                case CableCarAI:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_GONDOLA")); break;
                case TrolleybusAI:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_TROLLEYBUS")); break;
                case PassengerFerryAI:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_FERRY")); break;
                case PassengerShipAI:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_SHIP")); break;
                case PassengerHelicopterAI:
                    TransitInfo(Translations.Translate("VEHICLE_AITYPE_HELICOPTER")); break;

                case CargoTruckAI:
                case CargoTrainAI:
                case CargoShipAI:
                case CargoPlaneAI:
                    CargoInfo(); break;

                case AmbulanceAI:
                case AmbulanceCopterAI:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_MEDICAL"), true); break;
                case DisasterResponseVehicleAI:
                case DisasterResponseCopterAI:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_DISASTERRESPONSE")); break;
                case FireCopterAI:
                case FireTruckAI:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_FIREFIGHTING")); break;
                case PoliceCopterAI:
                case PoliceCarAI:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_POLICE"), true); break;
                case GarbageTruckAI:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_GARBAGE")); break;
                case HearseAI:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_DEATHCARE")); break;
                case PostVanAI:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_POSTAL")); break;
                case SnowTruckAI:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_SNOWPLOWING")); break;
                case WaterTruckAI:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_WATERPUMPING")); break;
                case BankVanAI:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_BANK")); break;
                case TaxiAI:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_TAXI"), true); break;
                case MaintenanceTruckAI:
                case ParkMaintenanceVehicleAI:
                    ServiceInfo(Translations.Translate("VEHICLE_AITYPE_MAINTENANCE"), true); break;

                case PrivatePlaneAI:
                case PassengerCarAI:
                case BicycleAI:
                case BalloonAI:
                case FishingBoatAI:
                case RocketAI:
                    return;//These have no more info

                default:
                    Logging.Error($"Vehicle(ID:{vehicleid} of type [{ai.GetType().Name}] is not recognized.");
                    return;
            }
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
            void RaceInfo()
            {
                var eventRoute = vehicle.m_eventRoute;
                var eventNum = (ushort)((eventRoute > 0) ? EventManager.instance.m_eventRoutes.m_buffer[eventRoute].m_event : 0);
                var eventData = EventManager.instance.m_events.m_buffer[eventNum];
                var eventInfo = eventData.Info;
                if (eventNum <= 0)
                    return;

                var raceEventData = eventData.m_raceEventData;
                byte racerIndex = vehicle.m_racerIndex;
                var racerData = raceEventData.m_racerData[racerIndex];

                switch (ai)
                {
                    case RaceBicycleAI:
                        modifyInfo[Translations.Translate("INFO_RACE_DRIVER")] = CitizenManager.instance.GetInstanceName(racerData.m_racerID);
                        break;
                    case RaceCarAI:
                        modifyInfo[Translations.Translate("INFO_RACE_DRIVER")] = racerData.m_localisedName;
                        modifyInfo[Translations.Translate("INFO_RACE_TEAM")] = racerData.RaceTeamInfo.GetLocalizedTitle();
                        modifyInfo[Translations.Translate("INFO_RACE_POS")] = raceEventData.GetRacerPosition(racerIndex) + " / " + raceEventData.m_racerCount;
                        break;
                    case ParadeFloatAI:
                        break;
                }

                float progress = default;
                switch (ai)
                {
                    case RaceCarAI:
                    case RaceBicycleAI:
                        float lapProgress = (float)racerData.m_pathIndex / raceEventData.m_trackPathLength;
                        progress = lapProgress / raceEventData.m_lapCount;
                        break;

                    case ParadeFloatAI:
                        if (eventInfo.m_eventAI is ParadeAI paradeAI)
                        {
                            float groupDistance = paradeAI.GetGroupDistance(eventNum, ref eventData, racerIndex);
                            progress = groupDistance / raceEventData.m_routeSpline.m_totalLength;
                        }
                        break;
                }
                if (progress != default)
                    modifyInfo[Translations.Translate("INFO_RACE_PROGRESS")] = progress.ToString("P1");
            }
        }

        internal static void GetMoreInfo(ref Dictionary<string, string> info, Citizen citizen, CitizenInstance citizenInstance, uint citizenId, bool isRace)
        {
            var modifyinfo = info;
            if (!isRace) GetRegularInfo();
            else GetRaceInfo();
            return;

            void GetRegularInfo()
            {
                if (citizen.m_flags.IsFlagSet(Citizen.Flags.Tourist) && citizen.m_hotelBuilding != default)
                {
                    modifyinfo[Translations.Translate("INFO_HUMAN_HOTEL")] =
                        BuildingManager.instance.GetBuildingName(citizen.m_hotelBuilding, InstanceID.Empty);
                }
                else
                {
                    modifyinfo[Translations.Translate("INFO_HUMAN_HOME")] =
                    citizen.m_homeBuilding != default ? BuildingManager.instance.GetBuildingName(citizen.m_homeBuilding, InstanceID.Empty) :
                    Translations.Translate("INFO_HUMAN_HOMELESS");
                }
                modifyinfo[Translations.Translate("INFO_HUMAN_OCCUPATION")] = GetOccupation();
            }

            void GetRaceInfo()
            {
                modifyinfo[Translations.Translate("INFO_RACE_EVENTROUTE")] = BuildingManager.instance.GetBuildingName(citizenInstance.m_sourceBuilding, InstanceID.Empty);

                var building = BuildingManager.instance.m_buildings.m_buffer[citizenInstance.m_sourceBuilding];
                var eventData = EventManager.instance.m_events.m_buffer[building.m_eventIndex];
                var eventInfo = eventData.Info;
                var raceEventData = eventData.m_raceEventData;
                var racerData = raceEventData.m_racerData[citizenInstance.m_racerIndex];
                var eventRouteData = EventManager.instance.m_eventRoutes.m_buffer[building.m_eventRouteIndex];

                var isParade = eventData.Info.m_type == EventManager.EventType.Parade;
                var isRacer = citizenInstance.Info.GetAI() is RaceCitizenAI;

                float progress = 0f;

                if (isParade)
                {
                    if (eventInfo.m_eventAI is ParadeAI paradeAI)
                    {
                        float groupDistance = paradeAI.GetGroupDistance(eventRouteData.m_event, ref eventData, citizenInstance.m_racerIndex);
                        progress = groupDistance / raceEventData.m_routeSpline.m_totalLength;
                    }
                }
                else
                {
                    if (isRacer)
                        modifyinfo[Translations.Translate("INFO_RACE_POS")] = raceEventData.GetRacerPosition(citizenInstance.m_racerIndex) + " / " + raceEventData.m_racerCount;

                    float lapProgress = (float)racerData.m_pathIndex / raceEventData.m_trackPathLength;
                    progress = lapProgress / raceEventData.m_lapCount;
                }

                if (progress != default)
                    modifyinfo[Translations.Translate("INFO_RACE_PROGRESS")] = progress.ToString("P1");
            }

            string GetOccupation()
            {
                var currentSchoolLevel = citizen.GetCurrentSchoolLevel(citizenId);
                if (citizen.m_flags.IsFlagSet(Citizen.Flags.Tourist))
                {
                    if (SteamHelper.IsDLCOwned(SteamHelper.DLC.CampusDLC))
                    {
                        float num = Singleton<ImmaterialResourceManager>.instance.CheckExchangeStudentAttractivenessBonus() * 100f;
                        var m_randomizer = new Randomizer(citizenId);
                        int num2 = m_randomizer.Int32(0, 100);
                        if (num2 < num)
                        {
                            return ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_EXCHANGESTUDENT");
                        }
                    }

                    return citizen.m_touristType switch
                    {
                        Citizen.TouristType.Sightseeing => ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_TOURIST_SIGHTSEEING"),
                        Citizen.TouristType.Shopping => ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_TOURIST_SHOPPING"),
                        Citizen.TouristType.Business => ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_TOURIST_BUSINESS"),
                        Citizen.TouristType.Nature => ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_TOURIST_NATURE"),
                        _ => ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_TOURIST"),
                    };
                }

                if (currentSchoolLevel != ItemClass.Level.None)
                {
                    return ColossalFramework.Globalization.Locale.Get("CITIZEN_SCHOOL_LEVEL", currentSchoolLevel.ToString());
                }

                return (citizen.m_workBuilding == default) ? ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_UNEMPLOYED") : GetJobTitle();
            }

            string GetJobTitle()
            {
                ushort workBuilding = citizen.m_workBuilding;
                var educationLevel = citizen.EducationLevel;
                var gender = citizenInstance.Info.m_gender;
                string text = string.Empty;
                if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[workBuilding].Info.m_buildingAI is CommonBuildingAI commonBuildingAI)
                {
                    text = commonBuildingAI.GetTitle(gender, educationLevel, workBuilding, citizenId);
                }

                if (text == string.Empty)
                {
                    int num = new Randomizer(workBuilding + citizenId).Int32(1, 5);
                    switch (educationLevel)
                    {
                        case Citizen.Education.Uneducated:
                            text = ColossalFramework.Globalization.Locale.Get((gender != Citizen.Gender.Female) ? "CITIZEN_OCCUPATION_PROFESSION_UNEDUCATED" : "CITIZEN_OCCUPATION_PROFESSION_UNEDUCATED_FEMALE", num.ToString());
                            break;
                        case Citizen.Education.OneSchool:
                            text = ColossalFramework.Globalization.Locale.Get((gender != Citizen.Gender.Female) ? "CITIZEN_OCCUPATION_PROFESSION_EDUCATED" : "CITIZEN_OCCUPATION_PROFESSION_EDUCATED_FEMALE", num.ToString());
                            break;
                        case Citizen.Education.TwoSchools:
                            text = ColossalFramework.Globalization.Locale.Get((gender != Citizen.Gender.Female) ? "CITIZEN_OCCUPATION_PROFESSION_WELLEDUCATED" : "CITIZEN_OCCUPATION_PROFESSION_WELLEDUCATED_FEMALE", num.ToString());
                            break;
                        case Citizen.Education.ThreeSchools:
                            text = ColossalFramework.Globalization.Locale.Get((gender != Citizen.Gender.Female) ? "CITIZEN_OCCUPATION_PROFESSION_HIGHLYEDUCATED" : "CITIZEN_OCCUPATION_PROFESSION_HIGHLYEDUCATED_FEMALE", num.ToString());
                            break;
                    }
                }
                return text + " " + ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_LOCATIONPREPOSITION") + " " + Singleton<BuildingManager>.instance.GetBuildingName(workBuilding, InstanceID.Empty);
            }

        }
    }
}
