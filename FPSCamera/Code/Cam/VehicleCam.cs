using AlgernonCommons;
using AlgernonCommons.Translation;
using ColossalFramework;
using FPSCamera.Cam.Controller;
using FPSCamera.Settings;
using FPSCamera.Utils;
using System.Collections.Generic;
using static FPSCamera.Utils.MathUtils;


namespace FPSCamera.Cam
{
    /// <summary>
    /// Following camera for vehicles
    /// </summary>
    public class VehicleCam : IFollowCam, IFPSCam
    {
        public VehicleCam(InstanceID id)
        {
            FollowInstance = id;
            FollowID = FollowInstance.Vehicle;
            IsActivated = true;
            if (ModSettings.StickToFrontVehicle && !SwitchTarget(GetFrontVehicleID())) return;
            hasReversed = GetVehicle().m_flags.IsFlagSet(Vehicle.Flags.Reversed);
        }
        private bool SwitchTarget(ushort id)
        {
            FollowID = id;
            FollowInstance = new InstanceID() { Vehicle = id };
            if (!IsVaild()) return false;
            FPSCamController.Instance.SyncCamOffset();
            return true;
        }

        public uint FollowID { get; private set; }

        public bool IsActivated { get; private set; }

        public InstanceID FollowInstance { get; private set; }

        public Dictionary<string, string> GetInfos()
        {
            var infos = new Dictionary<string, string>();
            var headVehicle = GetVehicle(GetHeadVehicleID());
            var ownerid = headVehicle.Info.m_vehicleAI.GetOwnerID(GetHeadVehicleID(), ref headVehicle);
            switch (ownerid.Type)
            {
                case InstanceType.Building:
                    infos[Translations.Translate("INFO_VEHICLE_OWNER")] = BuildingManager.instance.GetBuildingName(ownerid.Building, ownerid); break;
                case InstanceType.Citizen:
                    infos[Translations.Translate("INFO_VEHICLE_OWNER")] = CitizenManager.instance.GetCitizenName(ownerid.Citizen); break;
            }

            InfosUtils.GetMoreInfos(ref infos, headVehicle, GetHeadVehicleID());
            return infos;
        }

        public Positioning GetPositioning()
        {
            GetVehicle().GetSmoothPosition((ushort)FollowID, out var position, out var rotation);
            return new Positioning(position, rotation);
        }
        public string GetFollowName() => VehicleManager.instance.GetVehicleName((ushort)FollowID);
        public string GetPrefabName() => GetVehicle().Info.name;
        public float GetSpeed() => GetVehicle().GetSmoothVelocity((ushort)FollowID).magnitude;
        public string GetStatus()
        {
            var headVehicle = GetVehicle(GetHeadVehicleID());
            var status = headVehicle.Info.m_vehicleAI.GetLocalizedStatus(
                                GetHeadVehicleID(), ref headVehicle, out var implID);
            switch (implID.Type)
            {
                case InstanceType.Building:
                    status += " " + BuildingManager.instance.GetBuildingName(implID.Building, implID); break;
                case InstanceType.Citizen:
                    status += " " + CitizenManager.instance.GetCitizenName(implID.Citizen); break;
            }
            return status;
        }
        public bool IsVaild()
        {
            if (!IsActivated) return false;
            var flags = GetVehicle().m_flags;
            if (flags.IsFlagSet(Vehicle.Flags.Reversed) != hasReversed)
            {
                Logging.Message($" -- vehicle(ID:{FollowID}) changes direction");
                hasReversed = !hasReversed;
                if (ModSettings.StickToFrontVehicle && !SwitchTarget(GetFrontVehicleID()))
                {
                    return false;
                }

            }

            if (!flags.IsFlagSet(Vehicle.Flags.Spawned))
            {
                if (flags.IsFlagSet(Vehicle.Flags.Importing) || flags.IsFlagSet(Vehicle.Flags.Exporting))
                {
                    return flags.IsFlagSet(Vehicle.Flags.GoingBack);
                }
                return false;
            }
            return true;
        }
        public void StopCam()
        {
            FollowID = default;
            FollowInstance = default;
            IsActivated = false;
        }
        public ushort GetHeadVehicleID() => GetVehicle().GetFirstVehicle((ushort)FollowID);
        public ushort GetFrontVehicleID() => GetVehicle().m_flags.IsFlagSet(Vehicle.Flags.Reversed) ? GetVehicle().GetLastVehicle((ushort)FollowID) : GetHeadVehicleID();
        public Vehicle GetVehicle() => VehicleManager.instance.m_vehicles.m_buffer[FollowID];
        public Vehicle GetVehicle(ushort id) => VehicleManager.instance.m_vehicles.m_buffer[id];
        bool hasReversed = false;
    }

}