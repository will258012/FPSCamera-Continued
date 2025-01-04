using FPSCamera.Cam.Controller;
using FPSCamera.UI;
using HarmonyLib;
namespace FPSCamera.Patches
{
    [HarmonyPatch]
    internal class EscHandler
    {

        [HarmonyPatch(typeof(GameKeyShortcuts), "Escape")]
        [HarmonyPrefix]
        static bool Prefix1() => Patch();
        [HarmonyPatch(typeof(MapEditorKeyShortcuts), "Escape")]
        [HarmonyPrefix]
        static bool Prefix2() => Patch();
        [HarmonyPatch(typeof(DecorationKeyShortcuts), "Escape")]
        [HarmonyPrefix]
        static bool Prefix3() => Patch();
        [HarmonyPatch(typeof(GameKeyShortcuts), "SteamEscape")]
        [HarmonyPrefix]
        static bool Prefix4() => Patch();
        [HarmonyPatch(typeof(MapEditorKeyShortcuts), "SteamEscape")]
        [HarmonyPrefix]
        static bool Prefix5() => Patch();
        [HarmonyPatch(typeof(DecorationKeyShortcuts), "SteamEscape")]
        [HarmonyPrefix]
        static bool Prefix6() => Patch();
        static bool Patch() =>
            // cancel calling <Escape> if FPSCamera consumes it
            !FPSCamController.Instance.OnEsc() && !MainPanel.Instance.OnEsc();
    }
}