using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace FPSCameraAPI
{
    /// <summary>
    /// Based on boformer's CitiesHarmony.API.SubscriptionPrompt. Many thanks!
    /// </summary>
    public static class Notification
    {
        private const string Marker = "FPSCameraAPINotification";

        internal static void InstallNotification()
        {
            if (GameObject.Find(Marker)) return;

            var go = new GameObject(Marker);
            UnityEngine.Object.DontDestroyOnLoad(go);

            if (LoadingManager.instance.m_currentlyLoading || UIView.library == null)
            {
                LoadingManager.instance.m_introLoaded += OnIntroLoaded;
                LoadingManager.instance.m_levelLoaded += OnLevelLoaded;
            }
            else
            {
                ShowNotification();
            }
        }

        private static void OnIntroLoaded()
        {
            LoadingManager.instance.m_introLoaded -= OnIntroLoaded;
            LoadingManager.instance.m_levelLoaded -= OnLevelLoaded;
            ShowNotification();
        }

        private static void OnLevelLoaded(SimulationManager.UpdateMode updateMode)
        {
            LoadingManager.instance.m_introLoaded -= OnIntroLoaded;
            LoadingManager.instance.m_levelLoaded -= OnLevelLoaded;
            ShowNotification();
        }

        private static void ShowNotification()
        {
            Helper.GetFPSCameraStatus(out var isInstalled, out var isEnabled);
            if (!isInstalled)
                ShowSubscriptionNotification();
            else if (!isEnabled)
                ShowEnableNotification();
        }

        private static void ShowSubscriptionNotification()
        {
            if (!HasSubscriptionHelpMessages(out var reason, out var solution))
            {
                ShowError(reason, solution);
                return;
            }
            else
            {
                ConfirmPanel.ShowModal(
                    L10n.Translate("MissingDependencyTitle"),
                    L10n.Translate("MissingDependencyMessage"),
                    SubOnConfirm
                );
            }
        }

        private static void ShowEnableNotification()
        {
            ConfirmPanel.ShowModal(
                L10n.Translate("DependencyNotEnabledTitle"),
                L10n.Translate("DependencyNotEnabledMessage"),
                EnableOnConfirm
            );
        }

        private static void SubOnConfirm(UIComponent component, int result)
        {
            if (result == 1)
            {
                Debug.Log("Subscribing to FPSCamera workshop item!");

                if (PlatformService.workshop.Subscribe(new PublishedFileId(Helper.WorkshopId)))
                {
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage(
                        "Success!",
                        L10n.Translate("SuccessSub"),
                        false
                    );
                }
                else
                {
                    ShowError(
                        L10n.Translate("ErrorSub"),
                        L10n.Translate("ManualDownload")
                    );
                }
            }
            else
            {
                ShowError(
                    L10n.Translate("RejectSub"),
                    L10n.Translate("RejectSubSln")
                );
            }
        }

        private static void EnableOnConfirm(UIComponent component, int result)
        {
            if (result == 1)
            {
                Debug.Log("Enabling FPSCamera");
                foreach (var plugin in from PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo()
                                       where plugin.assembliesString.Contains("FPSCamera") && plugin.assembliesString.Contains("[3") && !plugin.isEnabled
                                       select plugin)
                {
                    plugin.isEnabled = true;
                    Debug.Log("Enabled FPSCamera successfully!");
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage(
                        "Success!",
                        L10n.Translate("SuccessEnabled"),
                        false
                    );
                    return;
                }
                ShowError(
                    L10n.Translate("ErrorEnable"),
                    L10n.Translate("ManualEnable")
                );
            }
            else
            {
                ShowError(
                    L10n.Translate("RejectEnable"),
                    L10n.Translate("RejectEnableSln")
                );
            }
        }

        private static void ShowError(string reason, string solution)
        {
            var affectedAssemblyNames = new StringBuilder();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in from assembly in assemblies
                                     where IsRequiresFPSCamera(assembly)
                                     select assembly)
            {
                affectedAssemblyNames.Append("• ").Append(GetModName(assembly)).Append('\n');
            }

            var message = string.Format(L10n.Translate("ShowError"), affectedAssemblyNames, reason, solution);

            UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage(
                L10n.Translate("MissingDependencyTitle"),
                message,
                false
            );
        }

        private static bool HasSubscriptionHelpMessages(out string reason, out string solution)
        {
            reason = "";
            solution = "";

            if (PlatformService.platformType != PlatformType.Steam)
            {
                Debug.LogError("Cannot auto-subscribe First Person Camera - Continued on platforms other than Steam!");
                reason = L10n.Translate("NotOnSteam");
                solution = L10n.Translate("ThenManualDownload");
                return false;
            }

            if (PluginManager.noWorkshop)
            {
                Debug.LogError("Cannot auto-subscribe First Person Camera - Continued in --noWorkshop mode!");
                reason = L10n.Translate("NoWorkShop");
                solution = L10n.Translate("NoWorkShopSln");
                return false;
            }

            if (!PlatformService.workshop.IsAvailable())
            {
                Debug.LogError("Cannot auto-subscribe First Person Camera - Continued while workshop is not available");
                reason = L10n.Translate("WorkShopNotAvailable");
                solution = L10n.Translate("ThenManualDownload");
                return false;
            }

            if (Helper.IsWorkshopItemSubscribed)
            {
                Debug.LogError("First Person Camera - Continued workshop item is subscribed, but assembly is not loaded or outdated!");
                reason = L10n.Translate("AssemblyNotLoad");
                solution = L10n.Translate("AssemblyNotLoadSln");
                return false;
            }

            return true;
        }

        private static bool IsRequiresFPSCamera(Assembly assembly)
        {
            if (assembly.GetName().Name == "FPSCamera") return false;
            foreach (var _ in from assemblyName in assembly.GetReferencedAssemblies()
                              where (assemblyName.Name == "FPSCamera") && assemblyName.Version.Major >= 3
                              select new { })
            {
                return true;
            }

            return false;
        }

        private static string GetModName(Assembly assembly)
        {
            foreach (var plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (plugin.userModInstance is IUserMod mod && mod.GetType().Assembly == assembly)
                    return mod.Name;
            }
            return assembly.GetName().Name;
        }
    }
}
