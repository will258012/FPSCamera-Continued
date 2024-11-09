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
        public FreeCam() => IsActivated = true;

        public bool IsActivated { get; private set; }
        public Positioning GetPositioning() =>
            new Positioning(GameCamController.Instance.MainCamera.transform.position,
            GameCamController.Instance.MainCamera.transform.rotation);
        internal void UpdateSpeed(Vector3 a, Vector3 b) => speed = a.DistanceTo(b) / Time.deltaTime;
        public bool AutoMove { get; set; }
        public void ToggleAutoMove() => AutoMove = !AutoMove;
        public float GetSpeed() => speed;
        public bool IsValid() => true;
        public void DisableCam() { IsActivated = false; AutoMove = false; speed = 0f; }

        private float speed = 0f;
    }
}
