using FPSCamera.Cam.Controller;
using FPSCamera.UI;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
namespace FPSCamera.Patches
{
    [HarmonyPatch]
    internal class ESCPatches
    {
        private static readonly MethodBase[] TargetMethods = [
            AccessTools.Method(typeof(GameKeyShortcuts), "Escape"),
            AccessTools.Method(typeof(MapEditorKeyShortcuts), "Escape"),
            AccessTools.Method(typeof(ThemeEditorKeyShortcuts), "Escape"),
            AccessTools.Method(typeof(DecorationKeyShortcuts), "Escape"),
            AccessTools.Method(typeof(ScenarioEditorKeyShortcuts), "Escape"),
            AccessTools.Method(typeof(GameKeyShortcuts), "SteamEscape"),
            AccessTools.Method(typeof(MapEditorKeyShortcuts), "SteamEscape"),
            AccessTools.Method(typeof(ThemeEditorKeyShortcuts), "SteamEscape"),
            AccessTools.Method(typeof(DecorationKeyShortcuts), "SteamEscape"),
            AccessTools.Method(typeof(ScenarioEditorKeyShortcuts), "SteamEscape"),
        ];
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethodsGetter() => TargetMethods;

        [HarmonyPrefix]
        private static bool HandleEscape() => !(FPSCamController.Instance?.OnEsc() ?? false) && !(MainPanel.Instance?.OnEsc() ?? false);
    }
}