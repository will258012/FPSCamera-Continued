using AlgernonCommons;
using AlgernonCommons.Translation;
using ColossalFramework;
using FPSCamera.Cam.Controller;
using FPSCamera.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
                CitizenInstanceID = GetCitizen().m_instance;
            }
            else if (id.Type == InstanceType.CitizenInstance)
            {
                CitizenInstanceID = id.CitizenInstance;
                var citizenId = CitizenManager.instance.m_instances.m_buffer[id.CitizenInstance].m_citizen;
                FollowInstance = new() { Citizen = citizenId };
                FollowID = citizenId;
            }
            if (IsValid())
            {
                PrefabName = GetCitizenInstance().Info.name;
                FollowName = CitizenManager.instance.GetCitizenName(FollowID) ?? CitizenManager.instance.GetInstanceName(CitizenInstanceID) ?? PrefabName;

                isRace = GetCitizenInstance().m_racerIndex != default || GetCitizenInstance().m_performerIndex != default || GetCitizenInstance().m_flags.IsFlagSet(CitizenInstance.Flags.Cheering | CitizenInstance.Flags.Spectating);
            }

            Logging.KeyMessage("Citizen cam started");
            Logging.Message($"Prefab:{PrefabName} FollowID:{FollowID} isRace:{isRace}");
        }
        public string Name => Translations.Translate("INFO_FOLLOW");
        public uint FollowID { get; private set; }
        public ushort CitizenInstanceID { get; private set; }
        public InstanceID FollowInstance { get; private set; }
        /// <summary>
        /// Will be used if the citizen enters a vehicle. Use caution!
        /// </summary>
        public VehicleCam AnotherCam { get; private set; } = null;
        private void CheckAnotherCam()
        {
            if (isinVehicle)
            {
                if (GetCitizen().m_vehicle == default || !(AnotherCam?.IsValid() ?? false))
                {
                    isinVehicle = false;
                    AnotherCam?.DisableCam();
                    AnotherCam = null;
                    SyncCamOffset();
                    Logging.KeyMessage("Citizen cam: Stopped another cam");
                }
            }
            else if (GetCitizen().m_vehicle != default)
            {
                ushort vehicleId = GetCitizen().m_vehicle;
                isinVehicle = true;
                AnotherCam = new VehicleCam(new InstanceID() { Vehicle = vehicleId });
                SyncCamOffset();
                Logging.KeyMessage("Citizen cam: Started another cam");
            }
        }
        public Dictionary<string, string> GetInfo()
        {
            var info = new Dictionary<string, string>();
            InfoUtils.GetMoreInfo(ref info, GetCitizen(), GetCitizenInstance(), FollowID, isRace);

            var anotherDetails = AnotherCam?.GetInfo();
            if (anotherDetails != null)
                info = info.Concat(anotherDetails).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return info;
        }
        public Positioning GetPositioning()
        {
            if (isinVehicle)
                return AnotherCam.GetPositioning();
            GetCitizenInstance().GetSmoothPosition(GetCitizen().m_instance, out var pos, out var rotation);
            //If the citizen sit down, adjust the rotation to adapt to the actual direction
            if (GetCitizenInstance().m_flags.IsFlagSet(CitizenInstance.Flags.SittingDown))
            {
                rotation *= Quaternion.Euler(0, 180, 0);
            }
            return new Positioning(pos, rotation);
        }
        public string FollowName { get; private set; }
        public string PrefabName { get; private set; }
        public string GetStatus()
        {
            if (isRace) return null;
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
        public float GetSpeed() => isinVehicle ? AnotherCam.GetSpeed() : GetCitizenInstance().GetLastFrameData().m_velocity.magnitude;

        public bool IsValid()
        {
            var flags = GetCitizenInstance().m_flags;
            if (
                !flags.IsFlagSet(CitizenInstance.Flags.None) &&
                !flags.IsFlagSet(CitizenInstance.Flags.Deleted) &&
                flags.IsFlagSet(CitizenInstance.Flags.Created))
            {
                CheckAnotherCam();
                return true;
            }
            return false;
        }
        public void SyncCamOffset()
        {
            if (isinVehicle)
                FPSCamController.Instance.SyncCamOffset(AnotherCam);
            else FPSCamController.Instance.SyncCamOffset(this);
        }
        public void SaveCamOffset()
        {
            if (isinVehicle)
                FPSCamController.Instance.SaveCamOffset(AnotherCam);
            else FPSCamController.Instance.SaveCamOffset(this);
        }
        public void DisableCam()
        {
            FollowID = CitizenInstanceID = default;
            FollowInstance = default;
            FollowName = PrefabName = null;

            if (isinVehicle)
            {
                AnotherCam.DisableCam();
                isinVehicle = false;
            }
            AnotherCam = null;
        }

        private Citizen GetCitizen() => CitizenManager.instance.m_citizens.m_buffer[FollowID];
        private CitizenInstance GetCitizenInstance() => CitizenManager.instance.m_instances.m_buffer[CitizenInstanceID];



        private bool isinVehicle = false;
        private readonly bool isRace = false;
    }
}





