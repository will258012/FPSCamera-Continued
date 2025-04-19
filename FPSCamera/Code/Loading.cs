using AlgernonCommons;
using AlgernonCommons.Notifications;
using AlgernonCommons.Patching;
using AlgernonCommons.Translation;
using FPSCamera.Cam.Controller;
using FPSCamera.Settings;
using FPSCamera.UI;
using FPSCamera.Utils;
using ICities;
using System.Collections.Generic;
using UnityEngine;

namespace FPSCamera
{
    public sealed class Loading : PatcherLoadingBase<OptionsPanel, PatcherBase>
    {
        protected override List<AppMode> PermittedModes => new List<AppMode> { AppMode.Game, AppMode.MapEditor, AppMode.AssetEditor };
        protected override List<string> CheckModConflicts() => ModSupport.CheckModConflicts();
        public override void OnLevelUnloading()
        {
            if (gameObject != null)
            {
                Object.Destroy(gameObject);
                gameObject = null;
            }
            base.OnLevelUnloading();
        }
        protected override void LoadedActions(LoadMode mode)
        {
            base.LoadedActions(mode);
            if (gameObject != null)
            {
                Object.Destroy(gameObject);
            }
            gameObject = new GameObject("FPSCamera");
            gameObject.AddComponent<FPSCamController>();
            gameObject.AddComponent<CamInfoPanel>();
            gameObject.AddComponent<MainPanel>();
            if (ToolsModifierControl.isGame)
                gameObject.AddComponent<FollowButtons>();
            if (WhatsNew.LastNotifiedVersion < AssemblyUtils.CurrentVersion)
            {
                if (ModSupport.FoundK45TLM)
                {
                    var notification = NotificationBase.ShowNotification<DontShowAgainNotification>();
                    notification.AddParas(Translations.Translate("K45_TLM_DETECTED"));
                    notification.DSAButton.eventClicked += (_, clickEvent) =>
                    {
                        WhatsNew.LastNotifiedVersion = AssemblyUtils.CurrentVersion;
                        ModSettings.Save();
                    };
                }
            }
            if (!ModSettings.DSAForCameraIssue &&
                (ModSettings.FollowCamOffset.y < 0f ||
                (ModSettings.VehicleFixedOffset.y + ModSettings.FollowCamOffset.y) < 2f ||
                (ModSettings.MidVehFixedOffset.y + ModSettings.FollowCamOffset.y) < 3f ||
                (ModSettings.PedestrianFixedOffset.y + ModSettings.FollowCamOffset.y) < 2f))
            {
                var notification = NotificationBase.ShowNotification<SettingsIssueNotification>();
                notification.AddParas(Translations.Translate("SETTINGS_ISSUE_DETECTED"));
                notification.DSAButton.eventClicked += (_, clickEvent) =>
                {
                    ModSettings.DSAForCameraIssue = true;
                    ModSettings.Save();
                };
            }
        }
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            ModSupport.Initialize();
        }
        private GameObject gameObject = null;
    }
}
