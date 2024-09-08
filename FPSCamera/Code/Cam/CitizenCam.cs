using AlgernonCommons.Translation;
using ColossalFramework;
using System.Collections.Generic;
using static FPSCamera.Utils.MathUtils;

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
                var citizenId = GetCitizenInstance().m_citizen;
                FollowInstance = new InstanceID() { Citizen = citizenId };
                FollowID = citizenId;
                IsActivated = true;
            }
        }
        public uint FollowID { get; private set; }
        public bool IsActivated { get; private set; }
        public InstanceID FollowInstance { get; private set; }
        internal bool CheckAnotherCam()
        {
            if (!IsActivated) return false;
            var flags = GetCitizenInstance().m_flags;
            if (IsinVehicle)
            {
                if (GetCitizen().m_vehicle == default)
                {
                    IsinVehicle = false;
                    AnotherCam.StopCam();
                    AnotherCam = null;
                    return false;
                }
                return true;
            }

            if (flags.IsFlagSet(CitizenInstance.Flags.EnteringVehicle))
            {
                if (GetCitizen().m_vehicle != default)
                {
                    ushort vehicleId = GetCitizen().m_vehicle;
                    IsinVehicle = true;
                    AnotherCam = new VehicleCam(new InstanceID() { Vehicle = vehicleId });
                    return true;
                }
            }
            return false;
        }
        public Dictionary<string, string> GetInfos()
        {
            var details = new Dictionary<string, string>();
            string occupation;
            var flags = GetCitizen().m_flags;

            if ((flags & Citizen.Flags.Tourist) != 0)
                occupation = Translations.Translate("INFO_HUMAN_TOURIST");
            else
            {
                if (GetCitizen().m_workBuilding != default)
                {
                    if (((flags & Citizen.Flags.Student) != 0))
                    {
                        occupation = string.Format(
                        Translations.Translate("INFO_HUMAN_STUDENTAT"),
                        BuildingManager.instance.GetBuildingName(GetCitizen().m_workBuilding, new InstanceID() { Building = GetCitizen().m_workBuilding }));
                    }
                    else
                    {
                        occupation = string.Format(
                        Translations.Translate("INFO_HUMAN_WORKAT"),
                        BuildingManager.instance.GetBuildingName(GetCitizen().m_workBuilding, new InstanceID() { Building = GetCitizen().m_workBuilding }));
                    }
                }
                else
                {
                    occupation = Translations.Translate("INFO_HUMAN_UNENPLOYED");
                }

                details[Translations.Translate("INFO_HUMAN_HOME")] =
                    GetCitizen().m_homeBuilding != default ? BuildingManager.instance.GetBuildingName(GetCitizen().m_homeBuilding, new InstanceID() { Building = GetCitizen().m_homeBuilding }) :
                    Translations.Translate("INFO_HUMAN_HOMELESS");

            }
            details[Translations.Translate("INFO_HUMAN_OCCUPATION")] = occupation;

            if (GetCitizen().m_hotelBuilding != default)
                details[Translations.Translate("INFO_HUMAN_HOTEL")] =
                    BuildingManager.instance.GetBuildingName(GetCitizen().m_hotelBuilding, new InstanceID() { Building = GetCitizen().m_hotelBuilding });

            return details;
        }
        public Positioning GetPositioning()
        {
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
        public float GetSpeed() => GetCitizenInstance().GetLastFrameData().m_velocity.magnitude;
        public bool IsVaild()
        {
            var flags = GetCitizenInstance().m_flags;
            return IsActivated && ((flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted)) == CitizenInstance.Flags.Created);
        }
        public void StopCam()
        {
            FollowID = default;
            FollowInstance = default;
            IsActivated = false;
            if (IsinVehicle)
            {
                AnotherCam.StopCam();
            }
        }



        internal IFollowCam AnotherCam = null;
        private bool IsinVehicle = false;

        private Citizen GetCitizen() => CitizenManager.instance.m_citizens.m_buffer[FollowID];
        private CitizenInstance GetCitizenInstance() => CitizenManager.instance.m_instances.m_buffer[GetCitizen().m_instance];
    }
}





