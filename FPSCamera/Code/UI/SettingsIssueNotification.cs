namespace FPSCamera.UI
{
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;
    using FPSCamera.Settings.Tabs;

    public class SettingsIssueNotification : ListNotification
    {

        /// <summary>
        /// Gets the "dont show again" button instance.
        /// </summary>
        public UIButton DSAButton { get; set; }

        /// <summary>
        /// Gets the "Quick fix" button instance.
        /// </summary>
        public UIButton QuickFixButton { get; set; }

        /// <summary>
        /// Gets the number of buttons for this panel (for layout).
        /// </summary>
        protected override int NumButtons => 3;

        /// <summary>
        /// Adds buttons to the notification panel.
        /// </summary>
        public override void AddButtons()
        {
            base.AddButtons();
            QuickFixButton = AddButton(2, NumButtons, Translations.Translate("SETTINGS_ISSUE_DETECTED_QUICKFIX"), () => { GeneralOptions.Reset(); Close(); });
            // Add don't show again button.
            DSAButton = AddButton(3, NumButtons, Translations.Translate("NOTE_DONTSHOWAGAIN"), Close);
        }
    }
}