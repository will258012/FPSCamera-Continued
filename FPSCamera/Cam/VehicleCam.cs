namespace FPSCamera.Cam
{
    using Config;
    using CSkyL;
    using CSkyL.Game.ID;
    using CSkyL.Game.Object;
    using CSkyL.Transform;
    using Log = CSkyL.Log;

    public class VehicleCam : FollowCam<VehicleID, Vehicle>
    {
        public VehicleCam(VehicleID vehicleID) : base(vehicleID)
        {
            if (!IsOperating) {
                Log.Warn($"vehicle(ID:{_id}) to follow does not exist");
                return;
            }
            if (Config.instance.StickToFrontVehicle &&
                !_SwitchTarget(_target.GetFrontVehicleID())) {
                Log.Warn($"vehicle(ID:{_id}) to follow does not exist");
                return;
            }
            Log.Msg($" -- following vehicle(ID:{_id})");

            if (ModSupport.IsTrainDisplayFoundandEnabled) {
                ModSupport.FollowVehicleID = _id.Value;
            }

            _wasReversed = _target.IsReversed;
        }

        public override bool Validate()
        {
            if (!base.Validate()) return false;

            if (_target.IsReversed != _wasReversed) {
                Log.Msg($" -- vehicle(ID:{_id}) changes direction");
                _wasReversed = !_wasReversed;
                if (Config.instance.StickToFrontVehicle &&
                    !_SwitchTarget(_target.GetFrontVehicleID())) return false;
            }

            if (!_target.IsSpawned) {
                if (_target.IsExporting || _target.IsImporting) {
                    return _target.IsGoingBack;
                }
                return false;
            }

            return true;
        }

        protected override Offset _LocalOffset
            => Config.instance.VehicleFixedOffset.ToOffset().FollowedBy(
                   !_target.IsHead && _target.GetPrefabName() !=
                       (Object.Of(_target.GetFrontVehicleID()) as Vehicle).GetPrefabName() ?
                   Config.instance.MidVehFixedOffset.ToOffset() : Offset.None);

        private bool _wasReversed;
    }
}
