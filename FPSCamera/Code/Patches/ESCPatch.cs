using FPSCamera.Cam.Controller;
using FPSCamera.UI;
using HarmonyLib;
namespace FPSCamera.Patches
{
    [HarmonyPatch(typeof(GameKeyShortcuts), "Escape")]
    internal class EscHandler
    {
        [HarmonyPrefix]
        public static bool ESCPatch() =>
            // cancel calling <Escape> if FPSCamera consumes it
            !FPSCamController.Instance.OnEsc() && !MainPanel.Instance.OnEsc();
    }
}