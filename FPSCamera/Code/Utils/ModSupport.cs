using AlgernonCommons;
using ColossalFramework.Plugins;
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
        internal static bool FoundK45TLM = false;

        internal static List<string> CheckModConflicts()
        {
            var conflictModNames = new List<string>();
            foreach (var plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (var assembly in plugin.GetAssemblies())
                {
                    switch (assembly.GetName().Name)
                    {
                        case "FPSCamera":
                            if (assembly.GetType("FPSCamera.FPSCamera") != null)
                                conflictModNames.Add("First Person Camera: Updated");
                            else if (assembly.GetType("FPSCamera.Controller") != null)
                                conflictModNames.Add("First Person Camera v2.x");
                            break;
                        case "EnhancedZoom":
                            conflictModNames.Add("Enhanced Zoom Continued");
                            break;
                        case "IINS.AutoWalking":
                            conflictModNames.Add("First-person Auto-walking");
                            break;
                    }
                }
            }
            return conflictModNames;
        }
        internal static void Initialize()
        {
            try
            {
                Logging.Message("ModSupport: Start search for supported enabled mods");
                foreach (var plugin in PluginManager.instance.GetPluginsInfo())
                {
                    if (plugin.isEnabled)
                        foreach (var assembly in plugin.GetAssemblies())
                        {
                            switch (assembly.GetName().Name)
                            {
                                case "ToggleIt":
                                    FoundToggleIt = true;
                                    Logging.KeyMessage("found ToggleIt, version ", assembly.GetName().Version);
                                    break;
                                case "UnifiedUIMod":
                                    FoundUUI = true;
                                    Logging.KeyMessage("found UUI, version ", assembly.GetName().Version);
                                    break;
                                case "TrainDisplay":
                                    FoundTrainDisplay = true;
                                    Logging.KeyMessage("found TrainDisplay, version ", assembly.GetName().Version);
                                    break;
                                case "TransportLinesManager":
                                    {
                                        if (assembly.GetType("Klyte.TransportLinesManager.TLMController") != null)
                                        {
                                            Logging.KeyMessage("found an older version of TLM by Klyte45");
                                            FoundK45TLM = true;
                                        }
                                        else
                                        {
                                            Logging.KeyMessage("found TLM by t1a2l, version ", assembly.GetName().Version);
                                            FoundTLM = true;
                                        }
                                    }
                                    break;
                            }
                        }
                }
            }

            catch (Exception e)
            {
                Logging.LogException(e, $"ModSupport: Failed to search mods");
            }
        }
    }
}

