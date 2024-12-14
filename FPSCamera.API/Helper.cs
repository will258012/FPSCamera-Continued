using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using System;
using System.Reflection;
using static ColossalFramework.Plugins.PluginManager;

namespace FPSCameraAPI
{
    /// <summary>
    /// Based on boformer's CitiesHarmony.API.HarmonyHelper. Many thanks!
    /// </summary>
    public static class Helper
    {
        internal const ulong WorkshopId = 3198388677uL;
        public static void CheckFPSCamera()
        {
            Notification.InstallNotification();
        }

        public static void GetFPSCameraStatus(out bool isInstalled, out bool isEnabled)
        {
            isInstalled = isEnabled = false;
            if (Type.GetType("FPSCamera.Utils.ModSupport, FPSCamera") != null) isInstalled = true;
            foreach (PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (plugin.isEnabled)
                {
                    foreach (Assembly assembly in plugin.GetAssemblies())
                    {
                        if (assembly.GetName().Name.Equals("FPSCamera") && assembly.GetType("FPSCamera.Utils.ModSupport") != null)
                        {
                            isEnabled = true;
                        }
                    }
                }
            }
        }
        public static bool IsFPSCameraInstalledAndEnabled
        {
            get
            {
                GetFPSCameraStatus(out var isInstalled, out var isEnabled);
                return isInstalled && isEnabled;
            }
        }
        public static bool IsWorkshopItemSubscribed
        {
            get
            {
                var subscribedIds = PlatformService.workshop.GetSubscribedItems();
                if (subscribedIds == null) return false;

                foreach (var id in subscribedIds)
                {
                    if (id.AsUInt64 == WorkshopId) return true;
                }

                return false;
            }
        }
    }
}