namespace FPSCamera.Cam
{
    using Config;
    using CSkyL;
    using CSkyL.Game.ID;
    using CSkyL.Game.Object;
    using CSkyL.Game.Utils;
    using CSkyL.Transform;
    using Log = CSkyL.Log;

    public class PedestrianCam : FollowCamWithCam<PedestrianID, Pedestrian, VehicleCam>
    {
        public PedestrianCam(PedestrianID pedID) : base(pedID)
        {
            if (IsOperating)
                Log.Msg($" -- following pedestrian(ID:{_id})");
            else
                Log.Warn($"pedestrian(ID:{_id}) to follow does not exist");
        }

        public override string GetTargetStatus()
        {
            var status = _target.GetStatus();
            if (_state is UsingOtherCam)
                status = string.Format(CSkyL.Translation.Translations.Translate("INFO_TARGETSTATUS"), _camOther.Target.Name, status);
            return status;
        }

        protected override Offset _LocalOffset
            => Config.instance.PedestrianFixedOffset.ToOffset();

        public override GameUtil.Infos GetTargetInfos()
        {
            var details = _target.GetInfos();
            if (_state is UsingOtherCam) {
                details[CSkyL.Translation.Translations.Translate("INFO_STATUS")] = _camOther.GetTargetStatus();
                foreach (var pair in _camOther.GetTargetInfos())
                    details["v/" + pair.field] = pair.text;
            }

            return details;
        }

        protected override bool _ReadyToSwitchToOtherCam
            => _target.RiddenVehicleID is VehicleID && !_target.IsEnteringVehicle;
        protected override bool _ReadyToSwitchBack {
            get {
                if (_ReadyToSwitchToOtherCam) return false;
                if (ModSupport.IsTrainDisplayFoundandEnabled && ModSupport.FollowVehicleID != default) {
                    ModSupport.FollowVehicleID = default;
                }
                Log.Msg($" -- pedestrian(ID:{_id}) left the vehicle");
                return true;
            }
        }

        protected override VehicleCam _CreateAnotherCam()
        {
            Log.Msg($" -- pedestrian(ID:{_id}) entered a vehicle");
            return new VehicleCam(_target.RiddenVehicleID);
        }
    }
}
