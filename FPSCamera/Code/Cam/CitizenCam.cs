using AlgernonCommons;
using AlgernonCommons.Translation;
using ColossalFramework;
using ColossalFramework.Math;
using FPSCamera.Cam.Controller;
using System.Collections.Generic;
using System.Linq;
using static FPSCamera.Utils.MathUtils;
using static RenderManager;

namespace FPSCamera.Cam
{
    /// <summary>
    /// Following camera for citizens
    /// </summary>
    public class CitizenCam : IFollowCam, IFPSCam
    {
        public CitizenCam(InstanceID id)
        {
            if (id.Type == InstanceType.Citizen)
            {
                FollowInstance = id;
                FollowID = FollowInstance.Citizen;
                IsActivated = true;
            }
            else if (id.Type == InstanceType.CitizenInstance)
            {
                var citizenId = CitizenManager.instance.m_instances.m_buffer[id.CitizenInstance].m_citizen;
                FollowInstance = new InstanceID() { Citizen = citizenId };
                FollowID = citizenId;
                IsActivated = true;
            }
            Logging.KeyMessage("Citizen cam started");
        }
        public uint FollowID { get; private set; }
        public bool IsActivated { get; private set; }
        public InstanceID FollowInstance { get; private set; }
        /// <summary>
        /// Will be used if the citizen enters a vehicle. Use caution!
        /// </summary>
        public VehicleCam AnotherCam { get; private set; } = null;
        private void CheckAnotherCam()
        {
            if (IsinVehicle)
            {
                if (GetCitizen().m_vehicle == default || !(AnotherCam?.IsValid() ?? false))
                {
                    IsinVehicle = false;
                    AnotherCam?.DisableCam();
                    AnotherCam = null;
                    SyncCamOffset();
                    Logging.KeyMessage("Citizen cam: Stopped another cam");
                }
            }
            else if (GetCitizen().m_vehicle != default)
            {
                ushort vehicleId = GetCitizen().m_vehicle;
                IsinVehicle = true;
                AnotherCam = new VehicleCam(new InstanceID() { Vehicle = vehicleId });
                SyncCamOffset();
                Logging.KeyMessage("Citizen cam: Started another cam");
            }
        }
        public Dictionary<string, string> GetInfos()
        {
            var details = new Dictionary<string, string>();
            var flags = GetCitizen().m_flags;
            details[Translations.Translate("INFO_HUMAN_HOME")] =
            GetCitizen().m_homeBuilding != default ? BuildingManager.instance.GetBuildingName(GetCitizen().m_homeBuilding, new InstanceID() { Building = GetCitizen().m_homeBuilding }) :
            Translations.Translate("INFO_HUMAN_HOMELESS");

            details[Translations.Translate("INFO_HUMAN_OCCUPATION")] = GetOccupation();

            if (GetCitizen().m_hotelBuilding != default)
                details[Translations.Translate("INFO_HUMAN_HOTEL")] =
                    BuildingManager.instance.GetBuildingName(GetCitizen().m_hotelBuilding, new InstanceID() { Building = GetCitizen().m_hotelBuilding });
            var anotherDetails = AnotherCam?.GetInfos();
            if (anotherDetails != null)
                details = details.Concat(anotherDetails).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return details;
        }
        public Positioning GetPositioning()
        {
            if (IsinVehicle)
                return AnotherCam.GetPositioning();
            // We should check if CitizenInstance is valid. If not do so the game will crash!!
            if (GetCitizen().m_instance == default) return default;
            GetCitizenInstance().GetSmoothPosition(GetCitizen().m_instance, out var pos, out var rotation);
            return new Positioning(pos, rotation);
        }
        public string GetFollowName() => CitizenManager.instance.GetCitizenName(FollowID);
        public string GetPrefabName() => GetCitizenInstance().Info.name;
        public string GetStatus()
        {
            var citizen = GetCitizen();
            var status = GetCitizenInstance().Info.m_citizenAI.GetLocalizedStatus(
                                FollowID, ref citizen, out var implID);
            switch (implID.Type)
            {
                case InstanceType.Building: status += BuildingManager.instance.GetBuildingName(implID.Building, implID); break;
                case InstanceType.NetNode:
                    if (implID.TransportLine != default)
                        status += TransportManager.instance.GetLineName(implID.TransportLine);
                    break;
            }
            return status;

        }
        public float GetSpeed() => IsinVehicle ? AnotherCam.GetSpeed() : GetCitizenInstance().GetLastFrameData().m_velocity.magnitude;

        public bool IsValid()
        {
            // We should check if CitizenInstance is valid. If not do so the game will crash!!
            if (!IsActivated || GetCitizen().m_instance == default) return false;
            var flags = GetCitizenInstance().m_flags;
            if (
                !flags.IsFlagSet(CitizenInstance.Flags.None) &&
                !flags.IsFlagSet(CitizenInstance.Flags.Deleted) &&
                flags.IsFlagSet(CitizenInstance.Flags.Created))
            {
                CheckAnotherCam();
                return true;
            }
            else return false;
        }
        public void SyncCamOffset()
        {
            if (IsinVehicle)
                FPSCamController.Instance.SyncCamOffset(AnotherCam);
            else FPSCamController.Instance.SyncCamOffset(this);
        }
        public void SaveCamOffset()
        {
            if (IsinVehicle)
                FPSCamController.Instance.SaveCamOffset(AnotherCam);
            else FPSCamController.Instance.SaveCamOffset(this);
        }
        public void DisableCam()
        {
            FollowID = default;
            FollowInstance = default;
            IsActivated = false;
            if (IsinVehicle)
            {
                AnotherCam.DisableCam();
                IsinVehicle = false;
            }
            AnotherCam = null;
        }

        private bool IsinVehicle = false;

        private Citizen GetCitizen() => CitizenManager.instance.m_citizens.m_buffer[FollowID];
        private CitizenInstance GetCitizenInstance() => CitizenManager.instance.m_instances.m_buffer[GetCitizen().m_instance];
        private string GetOccupation()
        {
            var currentSchoolLevel = GetCitizen().GetCurrentSchoolLevel(FollowID);
            if (GetCitizen().m_flags.IsFlagSet(Citizen.Flags.Tourist))
            {
                if (SteamHelper.IsDLCOwned(SteamHelper.DLC.CampusDLC))
                {
                    float num = Singleton<ImmaterialResourceManager>.instance.CheckExchangeStudentAttractivenessBonus() * 100f;
                    var m_randomizer = new Randomizer(FollowID);
                    int num2 = m_randomizer.Int32(0, 100);
                    if (num2 < num)
                    {
                        return ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_EXCHANGESTUDENT");
                    }
                }

                switch (GetCitizen().m_touristType)
                {
                    case Citizen.TouristType.Sightseeing:
                        return ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_TOURIST_SIGHTSEEING");
                    case Citizen.TouristType.Shopping:
                        return ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_TOURIST_SHOPPING");
                    case Citizen.TouristType.Business:
                        return ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_TOURIST_BUSINESS");
                    case Citizen.TouristType.Nature:
                        return ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_TOURIST_NATURE");
                    default:
                        return ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_TOURIST");
                }
            }

            if (currentSchoolLevel != ItemClass.Level.None)
            {
                return ColossalFramework.Globalization.Locale.Get("CITIZEN_SCHOOL_LEVEL", currentSchoolLevel.ToString());
            }

            return (GetCitizen().m_workBuilding == default) ? ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_UNEMPLOYED") : GetJobTitle();
        }

        private string GetJobTitle()
        {
            ushort workBuilding = GetCitizen().m_workBuilding;
            var educationLevel = GetCitizen().EducationLevel;
            var gender = GetCitizenInstance().Info.m_gender;
            string text = string.Empty;
            if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[workBuilding].Info.m_buildingAI is CommonBuildingAI commonBuildingAI)
            {
                text = commonBuildingAI.GetTitle(gender, educationLevel, workBuilding, FollowID);
            }

            if (text == string.Empty)
            {
                int num = new Randomizer(workBuilding + FollowID).Int32(1, 5);
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
            return text + " " + ColossalFramework.Globalization.Locale.Get("CITIZEN_OCCUPATION_LOCATIONPREPOSITION") + " " + Singleton<BuildingManager>.instance.GetBuildingName(workBuilding, FollowInstance);
        }
    }
}





