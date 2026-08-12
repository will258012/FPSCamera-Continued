using AlgernonCommons.Translation;
using UnityEngine;
using static FPSCamera.Utils.MathUtils;

namespace FPSCamera.Cam
{
    /// <summary>
    /// Free camera for free-camera mode
    /// </summary>
    public class FreeCam : IFPSCam
    {
        public Positioning GetPositioning() => Positioning.MainCameraPositioning;
        internal void UpdateSpeed(Vector3 previousPosition, Vector3 nextPosition)
            => Velocity = Time.deltaTime > 0f ? (nextPosition - previousPosition) / Time.deltaTime : Vector3.zero;
        public bool AutoMove { get; set; }
        public string Name => Translations.Translate("SETTINGS_KEYCAMTOGGLE");
        public void ToggleAutoMove() => AutoMove = !AutoMove;
        public float GetSpeed() => Velocity.magnitude;
        public Vector3 Velocity { get; private set; }
        public bool IsValid() => true;
        public void DisableCam() { AutoMove = false; Velocity = Vector3.zero; }
    }
}
