namespace FPSCamera
{
    using Config;
    using CSkyL.Game;
    using System.Reflection;
    using Ctransl = CSkyL.Translation.Translations;
    using Log = CSkyL.Log;

    public class Mod : CSkyL.Mod<Config.Config, UI.OptionsMenu>
    {
        public override string FullName => "First Person Camera - Continued";
        public override string ShortName => "FPSCamera";
        public override string Version {
            get {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                return $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
            }
        }
        public override string Description => Ctransl.Translate("MODDESCRIPTION");


        protected override void _PostEnable()
        {
            I = this;
            if (CamController.instance is null) return;
            // Otherwise, this implies it's in game/editor.
            // This usually means dll was just updated.

            Log.Msg("Controller: updating");
            int attempt = 5;
            var timer = new System.Timers.Timer(1000) { AutoReset = false };
            timer.Elapsed += (_, e) => {
                if (_TryInstallController()) return;

                if (attempt > 0) {
                    attempt--;
                    timer.Start();
                }
                else {
                    Log.Msg("Controller: fails to install");
                    timer.Dispose();
                }
            };
            timer.Start();
        }
        protected override void _PreDisable()
        {
            _controller?.Destroy();
        }

        protected override void _PostLoad()
        {
            if (CamController.instance is CamController)
                _TryInstallController();

            else Log.Err("Mod: fail to get <CameraController>.");
        }
        protected override void _PreUnload()
        {
            if (_controller != null) {
                _controller.Destroy();
                Log.Msg("Controller: uninstalled");
            }
        }


        public override void LoadConfig()
        {
            if (Config.Config.Load() is Config.Config config) Config.Config.instance.Assign(config);
            Config.Config.instance.Save();

            if (CamOffset.Load() is CamOffset offset) CamOffset.instance.Assign(offset);
            CamOffset.instance.Save();

            Log.Msg("Config: loaded");
        }
        public override void ResetConfig()
        {
            Config.Config.instance.Reset();
            Config.Config.instance.Save();

            CamOffset.instance.Reset();
            CamOffset.instance.Save();

            Log.Msg("Config: reset");
        }

        protected override Assembly FPSCamAssembly => Assembly.GetExecutingAssembly();

        private bool _TryInstallController()
        {
            if (CamController.instance.GetComponent<Controller>() is Controller c) {
                Log.Warn("Controller: old one not yet removed");
                UnityEngine.Object.Destroy(c);
                return false;
            }

            _controller = CamController.instance.AddComponent<Controller>();
            Log.Msg("Controller: installed");
            return true;
        }
        public static Mod I { get; private set; }

        private Controller _controller;
    }
}
