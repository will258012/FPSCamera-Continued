using AlgernonCommons;
using ColossalFramework;
using ColossalFramework.UI;
using FPSCamera.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FPSCamera.Utils.MathUtils;

namespace FPSCamera.Cam
{
    public class WalkThruCam : IFollowCam, IFPSCam
    {
        public WalkThruCam() => IsActivated = true;
        public IFollowCam CurrentCam { get; private set; } = null;
        public uint FollowID => CurrentCam?.FollowID ?? default;
        public InstanceID FollowInstance => CurrentCam?.FollowInstance ?? default;
        public bool IsActivated { get; private set; }
        public float GetSpeed() => CurrentCam?.GetSpeed() ?? default;
        public string GetFollowName() => CurrentCam?.GetFollowName();
        public string GetPrefabName() => CurrentCam?.GetPrefabName();
        public Dictionary<string, string> GetInfo() => CurrentCam?.GetInfo();
        public string GetStatus() => CurrentCam?.GetStatus();
        public Positioning GetPositioning() => CurrentCam?.GetPositioning() ?? default;
        public void SwitchTarget() => SetRandomCam();
        public void ElapseTime(float seconds) => elapsedTime += seconds;
        public float GetElapsedTime() => elapsedTime;
        public void SyncCamOffset() => CurrentCam?.SyncCamOffset();
        public void SaveCamOffset() => CurrentCam?.SaveCamOffset();
        public bool IsValid()
        {
            if (!IsActivated) return false;
            var status = CurrentCam?.IsValid() ?? false;
            if (!ModSettings.ManualSwitchWalk &&
                elapsedTime > ModSettings.PeriodWalk) status = false;
            if (!status)
            {
                SetRandomCam();
                status = CurrentCam?.IsValid() ?? false;
                if (!status)
                {
                    Logging.Error("No target for Walk-Thru mode");
                    AudioManager.instance.PlaySound(disabledClickSound);
                }
            }
            return status;
        }
        private void SetRandomCam()
        {
            CurrentCam?.DisableCam();
            CurrentCam = null;
            Logging.KeyMessage("WalkThru cam: Switching target");

            items = GetVehicles((v) =>
            {
                if (v.IsFlagSet(VehicleInfo.VehicleCategory.PassengerCar) || v.IsFlagSet(VehicleInfo.VehicleCategory.Bicycle))
                    return ModSettings.SelectDriving;
                if (v.IsFlagSet(VehicleInfo.VehicleCategory.PublicTransport))
                    return ModSettings.SelectPublicTransit;
                if (v.IsFlagSet(VehicleInfo.VehicleCategory.CityServices) || v.IsFlagSet(CityServiceCopters) || v.IsFlagSet(VehicleInfo.VehicleCategory.FishingBoat))
                    return ModSettings.SelectService;
                if (v.IsFlagSet(VehicleInfo.VehicleCategory.Cargo))
                    return ModSettings.SelectCargo;
                Logging.Error("WalkThru selection: unknown vehicle type:" + v);
                return false;

            }).Concat(GetCitizenInstances((c) =>
                    {
                        if (c.m_flags.IsFlagSet(CitizenInstance.Flags.HangAround))
                            return false;

                        var vehicleId = CitizenManager.instance.m_citizens.m_buffer[c.m_citizen].m_vehicle;
                        if (vehicleId != default)
                        {
                            var category = VehicleManager.instance.m_vehicles.m_buffer[vehicleId].Info.vehicleCategory;
                            if (category.IsFlagSet(VehicleInfo.VehicleCategory.PublicTransport))
                                return ModSettings.SelectPassenger;
                            else return false; // already selected by Vehicle
                        }

                        if (c.m_flags.IsFlagSet(CitizenInstance.Flags.WaitingTransport))
                            return ModSettings.SelectWaiting;

                        return ModSettings.SelectPedestrian;
                    }));

            if (!items.Any()) return;
            int attempt = 3;
            do
            {
                var followInstance = items.GetRandomOne();
                CurrentCam = followInstance.Type == InstanceType.CitizenInstance ? new CitizenCam(followInstance) : new VehicleCam(followInstance) as IFollowCam;
            }
            while (!(CurrentCam?.IsValid() ?? false) && --attempt >= 0);

            elapsedTime = 0f;
            SyncCamOffset();
        }


        public void DisableCam()
        {
            CurrentCam?.DisableCam();
            CurrentCam = null;
            IsActivated = false;
        }
        private const VehicleInfo.VehicleCategory CityServiceCopters = VehicleInfo.VehicleCategory.AmbulanceCopter | VehicleInfo.VehicleCategory.FireCopter | VehicleInfo.VehicleCategory.PoliceCopter | VehicleInfo.VehicleCategory.DisasterCopter;
        private float elapsedTime;
        private IEnumerable<InstanceID> items;
        private readonly AudioClip disabledClickSound = UIView.GetAView().defaultDisabledClickSound;

        /// <summary>
        /// Get a <see cref="IEnumerable{InstanceID}"/> list of valid vehicles.
        /// </summary>
        private static IEnumerable<InstanceID> GetVehicles(System.Func<VehicleInfo.VehicleCategory, bool> filter) => Enumerable.Range(1, VehicleManager.instance.m_vehicles.m_buffer.Length - 1)
                    .Select(i => new InstanceID() { Vehicle = (ushort)i })
                    .Where(v =>
                    VehicleManager.instance.m_vehicles.m_buffer[v.Vehicle].Info != null &&
                    VehicleManager.instance.m_vehicles.m_buffer[v.Vehicle].Info.vehicleCategory != VehicleInfo.VehicleCategory.None &&
                    VehicleManager.instance.m_vehicles.m_buffer[v.Vehicle].m_flags.IsFlagSet(Vehicle.Flags.Created) &&
                    filter(VehicleManager.instance.m_vehicles.m_buffer[v.Vehicle].Info.vehicleCategory));
        /// <summary>
        /// Get a <see cref="IEnumerable{InstanceID}"/> list of valid citizen instances.
        /// </summary>
        private static IEnumerable<InstanceID> GetCitizenInstances(System.Func<CitizenInstance, bool> filter) => Enumerable.Range(1, CitizenManager.instance.m_instances.m_buffer.Length - 1)
                .Select(i => new InstanceID() { CitizenInstance = (ushort)i })
                .Where(c =>
                CitizenManager.instance.m_instances.m_buffer[c.CitizenInstance].m_flags.IsFlagSet(CitizenInstance.Flags.Created) &&
                filter(CitizenManager.instance.m_instances.m_buffer[c.CitizenInstance]));
    }

}
