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
            if (ModSettings.StickToFrontVehicle)
                SwitchTarget(GetFrontVehicleID());
            hasReversed = GetVehicle().m_flags.IsFlagSet(Vehicle.Flags.Reversed);
            if (ModSupport.FoundTrainDisplay) ModSupport.FollowVehicleID = (ushort)FollowID;

            isRace = GetVehicle().m_eventRoute != default && GetVehicle().Info?.m_vehicleAI is RaceCarAI or RaceBicycleAI or ParadeFloatAI;
            Logging.KeyMessage("Vehicle cam started");
            Logging.Message($"FollowID:{FollowID} isRace:{isRace}");
        }
        private void SwitchTarget(ushort id)
        {
            if (id == FollowID) return;
            FollowID = id;
            FollowInstance = new() { Vehicle = id };
            SyncCamOffset();
            if (ModSupport.FoundTrainDisplay) ModSupport.FollowVehicleID = id;
        }
        public string Name => Translations.Translate("INFO_FOLLOW");
        public uint FollowID { get; private set; }
        public InstanceID FollowInstance { get; private set; }
        public Dictionary<string, string> GetInfo()
        {
            var info = new Dictionary<string, string>();
            var headVehicle = GetVehicle(GetHeadVehicleID());

            var ownerId = headVehicle.Info.m_vehicleAI.GetOwnerID(GetHeadVehicleID(), ref headVehicle);
            switch (ownerId.Type)
            {
                case InstanceType.Building when !isRace:
                    info[Translations.Translate("INFO_VEHICLE_OWNER")] = BuildingManager.instance.GetBuildingName(ownerId.Building, ownerId); break;
                case InstanceType.Citizen when !isRace:
                    info[Translations.Translate("INFO_VEHICLE_OWNER")] = CitizenManager.instance.GetCitizenName(ownerId.Citizen); break;

                case InstanceType.Building when isRace:
                case InstanceType.Citizen when isRace:
                    info[Translations.Translate("INFO_RACE_EVENTROUTE")] = BuildingManager.instance.GetBuildingName(EventManager.instance.m_eventRoutes[headVehicle.m_eventRoute].m_startBuilding, ownerId);
                    break;
            }

            InfoUtils.GetMoreInfo(ref info, headVehicle, GetHeadVehicleID(), isRace);
            return info;
        }

        public Positioning GetPositioning()
        {
            GetVehicle().GetSmoothPosition((ushort)FollowID, out var position, out var rotation);
            return new Positioning(position, rotation);
        }
        public string GetFollowName() => VehicleManager.instance.GetVehicleName((ushort)FollowID) ?? GetPrefabName();
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
                    status += BuildingManager.instance.GetBuildingName(implID.Building, implID); break;
                case InstanceType.Citizen:
                    status += CitizenManager.instance.GetCitizenName(implID.Citizen); break;
            }
            return status;
        }
        public bool IsValid()
        {
            var flags = GetVehicle().m_flags;

            if (!flags.IsFlagSet(Vehicle.Flags.Spawned))
            {
                if (flags.IsFlagSet(Vehicle.Flags.Importing) || flags.IsFlagSet(Vehicle.Flags.Exporting))
                {
                    return flags.IsFlagSet(Vehicle.Flags.GoingBack);
                }
                return false;
            }

            if (flags.IsFlagSet(Vehicle.Flags.Reversed) != hasReversed)
            {
                Logging.KeyMessage("Vehicle cam: Vehicle changes direction");
                hasReversed = !hasReversed;
                if (ModSettings.StickToFrontVehicle)
                {
                    SwitchTarget(GetFrontVehicleID());
                }
            }

            return true;
        }

        public void SyncCamOffset() => FPSCamController.Instance.SyncCamOffset(this);
        public void SaveCamOffset() => FPSCamController.Instance.SaveCamOffset(this);
        public void DisableCam()
        {
            FollowID = default;
            FollowInstance = default;
            if (ModSupport.FoundTrainDisplay)
                ModSupport.FollowVehicleID = default;
        }
        public ushort GetHeadVehicleID() => GetVehicle().GetFirstVehicle((ushort)FollowID);
        public ushort GetFrontVehicleID() => GetVehicle().m_flags.IsFlagSet(Vehicle.Flags.Reversed) ? GetVehicle().GetLastVehicle((ushort)FollowID) : GetHeadVehicleID();
        public Vehicle GetVehicle() => VehicleManager.instance.m_vehicles.m_buffer[FollowID];
        public static Vehicle GetVehicle(ushort id) => VehicleManager.instance.m_vehicles.m_buffer[id];
        private bool hasReversed = false;
        private readonly bool isRace = false;
    }

}