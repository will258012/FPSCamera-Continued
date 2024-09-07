using AlgernonCommons.Patching;
using AlgernonCommons.Translation;
using FPSCamera.Settings;
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

    }
}
