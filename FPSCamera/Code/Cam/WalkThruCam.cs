using AlgernonCommons;
using ColossalFramework;
using ColossalFramework.UI;
using FPSCamera.Cam.Controller;
using FPSCamera.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FPSCamera.Utils.MathUtils;

namespace FPSCamera.Cam
{
    public class WalkThruCam : IFollowCam, IFPSCam
    {
        public WalkThruCam()
        {
            IsActivated = true;
        }
        public uint FollowID => _currentCam.FollowID;
        public InstanceID FollowInstance => _currentCam.FollowInstance;
        public bool IsActivated { get; private set; }
        public float GetSpeed() => _currentCam.GetSpeed();
        public string GetFollowName() => _currentCam?.GetFollowName();
        public string GetPrefabName() => _currentCam?.GetPrefabName();
        public Dictionary<string, string> GetInfos() => _currentCam?.GetInfos();
        public string GetStatus() => _currentCam?.GetStatus();
        public Positioning GetPositioning() => _currentCam.GetPositioning();
        public void SwitchTarget() => SetRandomCam();
        public void ElapseTime(float seconds) => _elapsedTime += seconds;
        public float GetElapsedTime() => _elapsedTime;
        public void SyncCamOffset() => FPSCamController.Instance.SyncCamOffset(_currentCam);
        public bool IsVaild()
        {
            if (!IsActivated) return false;

            var status = _currentCam?.IsVaild() ?? false;
            if (!ModSettings.ManualSwitchWalk &&
                _elapsedTime > ModSettings.PeriodWalk) status = false;
            if (!status)
            {
                SetRandomCam();
                status = _currentCam?.IsVaild() ?? false;
                if (!status)
                {
                    Logging.Error("no target for Walk-Thru mode");
                    AudioManager.instance.PlaySound(disabledClickSound);
                }
            }
            return status;
        }
        private void SetRandomCam()
        {
            _currentCam = null;
            Logging.Message(" -- switching target");

            items = GetVehicles((v) =>
            {
                if (v.IsFlagSet(VehicleInfo.VehicleCategory.PassengerCar) || v.IsFlagSet(VehicleInfo.VehicleCategory.Bicycle))
                    return ModSettings.SelectDriving;
                if (v.IsFlagSet(VehicleInfo.VehicleCategory.PublicTransport))
                    return ModSettings.SelectPublicTransit;
                if (v.IsFlagSet(VehicleInfo.VehicleCategory.CityServices) || v.IsFlagSet(CityServiceCopters))
                    return ModSettings.SelectService;
                if (v.IsFlagSet(VehicleInfo.VehicleCategory.Cargo))
                    return ModSettings.SelectCargo;
                Logging.Error("WalkThru selection: unknown vehicle type:" + v);
                return false;

            }).Concat(GetCitizenInstances((c) =>
                    {
                        if (c.IsFlagSet(CitizenInstance.Flags.HangAround))
                            return false;
                        if (c.IsFlagSet(CitizenInstance.Flags.Transition))
                            return ModSettings.SelectPassenger;

                        if (c.IsFlagSet(CitizenInstance.Flags.EnteringVehicle) || c.IsFlagSet(CitizenInstance.Flags.RidingBicycle))
                            return false;    // already selected by Vehicle

                        if (c.IsFlagSet(CitizenInstance.Flags.WaitingTransport))
                            return ModSettings.SelectWaiting;

                        return ModSettings.SelectPedestrian;
                    }));

            if (!items.Any()) return;
            Logging.Message("Total: " + items.Count().ToString());
            int attempt = 3;
            do
            {
                var followInstance = items.GetRandomOne();
                _currentCam = followInstance.Type == InstanceType.CitizenInstance ? new CitizenCam(followInstance) : new VehicleCam(followInstance) as IFollowCam;
            }
            while (!(_currentCam?.IsVaild() ?? false) && --attempt >= 0);

            _elapsedTime = 0f;
            FPSCamController.Instance.SyncCamOffset(_currentCam);
        }


        public void StopCam()
        {
            _currentCam?.StopCam();
            _currentCam = null;
            IsActivated = false;
        }
        private const VehicleInfo.VehicleCategory CityServiceCopters = VehicleInfo.VehicleCategory.AmbulanceCopter | VehicleInfo.VehicleCategory.FireCopter | VehicleInfo.VehicleCategory.PoliceCopter | VehicleInfo.VehicleCategory.DisasterCopter;
        private IFollowCam _currentCam;
        private float _elapsedTime;
        private IEnumerable<InstanceID> items;
        private readonly AudioClip disabledClickSound = UIView.GetAView().defaultDisabledClickSound;

        /// <summary>
        /// Get a <see cref="IEnumerable{InstanceID}"/> list of vaild vehicles.
        /// </summary>
        private IEnumerable<InstanceID> GetVehicles(System.Func<VehicleInfo.VehicleCategory, bool> filter) => Enumerable.Range(1, VehicleManager.instance.m_vehicles.m_buffer.Length - 1)
                    .Select(i => new InstanceID() { Vehicle = (ushort)i })
                    .Where(v =>
                    VehicleManager.instance.m_vehicles.m_buffer[v.Vehicle].Info.vehicleCategory != VehicleInfo.VehicleCategory.None &&
                    VehicleManager.instance.m_vehicles.m_buffer[v.Vehicle].m_flags.IsFlagSet(Vehicle.Flags.Created) &&
                    filter(VehicleManager.instance.m_vehicles.m_buffer[v.Vehicle].Info.vehicleCategory));
        /// <summary>
        /// Get a <see cref="IEnumerable{InstanceID}"/> list of vaild citizen instances.
        /// </summary>
        private IEnumerable<InstanceID> GetCitizenInstances(System.Func<CitizenInstance.Flags, bool> filter) => Enumerable.Range(1, CitizenManager.instance.m_instances.m_buffer.Length - 1)
                .Select(i => new InstanceID() { CitizenInstance = (ushort)i })
                .Where(c =>
                CitizenManager.instance.m_instances.m_buffer[c.CitizenInstance].m_flags.IsFlagSet(CitizenInstance.Flags.Created) &&
                filter(CitizenManager.instance.m_instances.m_buffer[c.CitizenInstance].m_flags));

    }

}
