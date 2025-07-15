using AlgernonCommons;
using ColossalFramework.UI;
using FPSCamera.Cam.Controller;
using FPSCamera.Game;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace FPSCamera.Patches
{
    [HarmonyPatch]

    internal class CommuterDestinationPatch
    {
        private static bool shouldHide = true;
        private static bool Prepare()
        {
            bool result = AssemblyUtils.IsAssemblyPresent("CommuterDestination.CS1");
            if (result)
                FPSCamController.OnCameraEnabled += () => shouldHide = false;
            return result;
        }
        private static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("CommuterDestination.CS1.UI.StopDestinationInfoPanel, CommuterDestination.CS1"), "CheckForClose");
        private static bool Prefix(UIPanel __instance)
        {
            if (KeyCode.Escape.KeyTriggered())
            {
                if (!shouldHide)
                    shouldHide = true;
                else
                    __instance.Hide();
            }
            return false;
        }
    }
}
