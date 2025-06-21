using AlgernonCommons;
using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using TransportLinesManager.ModShared;
namespace FPSCamera.Utils
{
    public class ModSupport
    {
        public static bool FoundToggleIt { get; private set; }
        public static bool FoundUUI { get; private set; }
        public static bool FoundTLM { get; private set; }
        public static bool FoundTrainDisplay { get; private set; }
        public static bool FoundACME { get; private set; }
        public static ushort FollowVehicleID { get; internal set; }
        internal static bool FoundK45TLM = false;
        internal static bool ACMEDisabling = false;
        internal static List<string> CheckModConflicts()
        {
            try
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
            catch (Exception e)
            {
                Logging.LogException(e, "Failed to search conflict mods");
            }
            return null;
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
                                case "ACME":
                                    FoundACME = true;
                                    Logging.KeyMessage("found ACME, version ", assembly.GetName().Version);
                                    break;
                            }
                        }
                }
            }

            catch (Exception e)
            {
                Logging.LogException(e, "Failed to search supported mods");
            }
        }
        internal static void ACME_DisableFPSMode()
        {
            if (AccessUtils.GetStaticFieldValue<bool>(Type.GetType("ACME.FPSMode, ACME"), "s_modeActive"))
                AccessUtils.InvokeMethod("ACME.FPSMode, ACME", "ToggleMode", null);
        }
        internal static string TLM_GetStopName(ushort stopId, ushort lineId)
        {
            var subService = TransportManager.instance.m_lines.m_buffer[lineId].Info.m_netSubService;
            return TLMFacade.GetFullStationName(stopId, lineId, false, subService);
        }
        internal static string TLM_GetLineCode(ushort lineId) => TLMFacade.GetLineStringId(lineId, false);
        internal static void ToggleIt_ToggleUI(bool visible)
        {
            if (!visible)
            {
                TerrainManager.instance.RenderTopography =
                NotificationManager.instance.NotificationsVisible =
                GameAreaManager.instance.BordersVisible =
                DistrictManager.instance.NamesVisible =
                NetManager.instance.RoadNamesVisible = false;
            }
            else ToggleIt.Managers.ToggleManager.Instance.ApplyAll();
        }
    }
}

