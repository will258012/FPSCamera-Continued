namespace FPSCamera.UI
{
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;
    using FPSCamera.Settings.Tabs;

    public class SettingsIssueNotification : ListNotification
    {
        /// <summary>
        /// Gets the "Quick fix" button instance.
        /// </summary>
        public UIButton QuickFixButton { get; set; }

        /// <summary>
        /// Gets the number of buttons for this panel (for layout).
        /// </summary>
        protected override int NumButtons => 2;

        /// <summary>
        /// Adds buttons to the notification panel.
        /// </summary>
        public override void AddButtons()
        {
            base.AddButtons();
            QuickFixButton = AddButton(2, NumButtons, Translations.Translate("SETTINGS_ISSUE_DETECTED_QUICKFIX"), () => { GeneralOptions.ResetModSettings(); Close(); });
        }
    }
}