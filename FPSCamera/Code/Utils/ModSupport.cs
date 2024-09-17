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
        internal static List<string> CheckModConflicts()
        {
            var list = new List<string>();
            if (AccessTools.TypeByName("FPSCamera.FPSCamera") != null) list.Add("First Person Camera: Updated");
            if (AccessTools.TypeByName("FPSCamera.Controller") != null) list.Add("First Person Camera v2.x");
            if (AccessTools.TypeByName("EnhancedZoomContinued.EnhancedZoomMod") != null) list.Add("Enhanced Zoom Continued");
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
                Logging.Error($"ModSupport: Falled to finding the mod");
                Logging.LogException(e);
            }
        }
    }
}

