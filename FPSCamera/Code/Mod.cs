using AlgernonCommons;
using AlgernonCommons.Notifications;
using AlgernonCommons.Patching;
using AlgernonCommons.Translation;
using FPSCamera.Settings;
using FPSCamera.UI;
using ICities;

namespace FPSCamera
{
    public sealed class Mod : PatcherMod<OptionsPanel, PatcherBase>, IUserMod
    {
        public override string BaseName => "First Person Camera - Continued";
        public override string HarmonyID => "Will258012.FPSCamera.Continued";
        public string Description => Translations.Translate("MODDESCRIPTION");
        public override void SaveSettings() => ModSettings.Save();
        public override void LoadSettings() => ModSettings.Load();
        public override WhatsNewMessage[] WhatsNewMessages => new WhatsNewMessage[]
        {
            new WhatsNewMessage
            {
                Version = AssemblyUtils.CurrentVersion,
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "WHATSNEW_L1",
                    "WHATSNEW_L2",
                    "WHATSNEW_L3"
                }
            }
        };
        public override void OnEnabled()
        {
            base.OnEnabled();
            Logging.EventExceptionOccured += (message) => ErrorNotification.ShowNotification(message);
        }
    }
}
