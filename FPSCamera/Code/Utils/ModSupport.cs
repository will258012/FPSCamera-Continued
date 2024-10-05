using AlgernonCommons;
using HarmonyLib;
using System;
using System.Collections.Generic;
namespace FPSCamera.Utils
{
    public class ModSupport
    {
        public static bool FoundToggleIt { get; private set; }
        public static bool FoundUUI { get; private set; }
        public static bool FoundTLM { get; private set; }
        public static bool FoundTrainDisplay { get; private set; }
        public static ushort FollowVehicleID { get; internal set; }

        internal static List<string> CheckModConflicts()
        {
            var list = new List<string>();
            if (AccessTools.TypeByName("FPSCamera.FPSCamera") != null) list.Add("First Person Camera: Updated");
            if (AccessTools.TypeByName("FPSCamera.Controller") != null) list.Add("First Person Camera v2.x");
            if (AssemblyUtils.IsAssemblyPresent("EnhancedZoom")) list.Add("Enhanced Zoom Continued");
            if (AssemblyUtils.IsAssemblyPresent("IINS.AutoWalking")) list.Add("First-person Auto-walking");
            return list;
        }
        internal static void Initialize()
        {
            try
            {
                if (AssemblyUtils.IsAssemblyPresent("ToggleIt"))
                    FoundToggleIt = true;

                if (AssemblyUtils.IsAssemblyPresent("UnifiedUILib"))
                    FoundUUI = true;

                if (AssemblyUtils.IsAssemblyPresent("TrainDisplay"))
                    FoundTrainDisplay = true;

                var assembly = AssemblyUtils.GetEnabledAssembly("TransportLinesManager");

                if (assembly != null)
                {
                    var n = assembly?.GetType("Klyte.TransportLinesManager.TLMController");

                    if (n != null)
                    {
                        Logging.Error("ModSupport: Found an older version of Transport Lines Manager by Klyte45. Please update to the version by t1a2l for full feature support");
                    }
                    else
                    {
                        FoundTLM = true;
                    }
                }
            }

            catch (Exception e)
            {
                Logging.Error($"ModSupport: Failed to finding the mod");
                Logging.LogException(e);
            }
        }
    }
}

