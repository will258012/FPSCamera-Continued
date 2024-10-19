using FPSCamera.Cam.Controller;
using FPSCamera.Utils;
using UnityEngine;
using static FPSCamera.Utils.MathUtils;

namespace FPSCamera.Cam
{
    /// <summary>
    /// Free camera for free-camera mode
    /// </summary>
    public class FreeCam : IFPSCam
    {
        public FreeCam()
        {
            var transform = GameCamController.Instance.MainCamera.transform;
            _positioning.pos = transform.position;
            _positioning.rotation = Quaternion.identity;
            _lastPositioning = _positioning;
            IsActivated = true;
        }

        public bool IsActivated { get; private set; }
        public Positioning GetPositioning() => _positioning;
        internal void RecordLastPositioning() => _lastPositioning = _positioning;
        public bool AutoMove { get; set; }
        public void ToggleAutoMove() => AutoMove = !AutoMove;
        public float GetSpeed() => _lastPositioning.pos.DistanceTo(_positioning.pos) / Time.deltaTime;
        public bool IsValid() => true;
        public void DisableCam() { IsActivated = false; _positioning = _lastPositioning = default; AutoMove = false; }

        internal Positioning _positioning = new Positioning(Vector3.zero);
        private Positioning _lastPositioning = new Positioning(Vector3.zero);
    }
}
