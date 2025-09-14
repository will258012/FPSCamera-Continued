using AlgernonCommons.Notifications;
using AlgernonCommons.Translation;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace FPSCamera.UI
{
    public class ErrorNotification : ListNotification
    {
        /// <summary>
        /// Gets the current instance of <see cref="ErrorNotification"/>.
        /// </summary>
        public static ErrorNotification Instance { get; private set; }
        /// <summary>
        /// Gets the "Copy" button instance.
        /// </summary>
        public UIButton CopyButton { get; set; }

        /// <summary>
        /// Gets the "Support" button instance.
        /// </summary>
        public UIButton SupportButton { get; set; }

        /// <summary>
        /// The Steam Workshop ID for the "Support" button (default is FPC's).
        /// </summary>
        public ulong WorkshopId { get; set; } = 3198388677;
        /// <summary>
        /// Gets the number of buttons for this panel (for layout).
        /// </summary>
        protected override int NumButtons => 3;
        private string errorMessage = string.Empty;
        private static readonly object lockObj = new object();
        /// <summary>
        /// Adds buttons to the notification panel.
        /// </summary>
        public override void AddButtons()
        {
            base.AddButtons();
            CopyButton = AddButton(2, NumButtons, Translations.Translate("ERROR_COPY"),
                () => Clipboard.text = errorMessage);
            SupportButton = AddButton(3, NumButtons, Translations.Translate("ERROR_SUPPORT"),
                () => Application.OpenURL($"https://steamcommunity.com/sharedfiles/filedetails/?id={WorkshopId}"));
        }
        /// <summary>
        /// Displays an error notification with exception details.
        /// </summary>
        /// <param name="title">The mod name of the called mod (used for the title of the notification)</param>
        /// <param name="workshopId">The Steam Workshop ID of the called mod (used for the "Support" button).</param>
        /// <param name="message">Additional custom messages to display before the exception details.</param>
        public static void ShowNotification(string title, ulong workshopId, string message)
        {
            lock (lockObj)
            {

                try
                {
                    if (Instance != null) return;
                    Instance = ShowNotification<ErrorNotification>();
                    Instance.Title = title;
                    Instance.WorkshopId = workshopId;
                    Instance.AddParas(Translations.Translate("ERROR"));
                    Instance.AddSpacer();
                    Instance.AddParas(message);
                    Instance.errorMessage = message;
                }
                catch
                {
                    // Fallback to vanilla exception panel if AlgernonCommons' notification failed
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage(
                        title,
                        $"{Translations.Translate("ERROR")}\n{message}",
                        true
                    );
                }
            }
        }
        internal static void ShowNotification(string message) => ShowNotification(Mod.Instance.Name, 3198388677, message);
    }
}
