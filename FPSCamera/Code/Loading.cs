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

        /// <summary>
        /// Gets a list of permitted loading modes.
        /// </summary>
        protected override List<AppMode> PermittedModes => new List<AppMode> { AppMode.Game, AppMode.MapEditor, AppMode.AssetEditor };

        /// <summary>
        /// Checks for any mod conflicts.
        /// Called as part of checking prior to executing any OnCreated actions.
        /// </summary>
        /// <returns>A list of conflicting mod names (null or empty if none).</returns>
        protected override List<string> CheckModConflicts() => ModSupport.CheckModConflicts();
        /// <summary>
        /// Called by the game when exiting a level.
        /// </summary>
        public override void OnLevelUnloading()
        {
            if (gameObject != null)
            {
                Object.Destroy(gameObject);
                gameObject = null;
            }
            base.OnLevelUnloading();
        }
        /// <summary>
        /// Performs any actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
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

            if (ModSupport.FoundK45TLM && WhatsNew.LastNotifiedVersion < AssemblyUtils.CurrentVersion)
            {
                var notification = NotificationBase.ShowNotification<DontShowAgainNotification>();
                //notification.AddParas("Since we changed the default values ​​of some settings, you may need to reset the settings for a better experience.");
                //notification.AddParas("由于我们修改了部分设置的默认值，您可能需要重置设置以获得更佳体验。");
                notification.AddParas(Translations.Translate("K45_TLM_DETECTED"));
                notification.DSAButton.eventClicked += (component, clickEvent) =>
                {
                    WhatsNew.LastNotifiedVersion = AssemblyUtils.CurrentVersion;
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
