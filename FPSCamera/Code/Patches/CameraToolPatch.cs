using ColossalFramework;
using FPSCamera.Cam.Controller;
using HarmonyLib;
namespace FPSCamera.Patches
{
    [HarmonyPatch]
    internal class CameraToolPatch
    {
        /// <summary>
        /// This patch will be invoked when vanilla free camera or cinematic camera is enabled.
        /// </summary>
        [HarmonyPatch(typeof(CameraTool), "OnEnable")]
        private static bool Prefix()
        {
            // If FPS camera is enabled, disable it. This could happen when user presses hotkeys for any of these two modes while FPS camera is enabled.
            if (FPSCamController.Instance.Status.IsFlagSet(FPSCamController.CamStatus.Enabled))
            {
                FPSCamController.Instance.OverrideSetBackCamera = FPSCamController.OverrideSetBack.False;
                FPSCamController.Instance.FPSCam = null;
            }
            return true;
        }
    }
}
