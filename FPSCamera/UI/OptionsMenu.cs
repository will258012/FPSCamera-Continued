namespace FPSCamera.UI
{
    using Config;
    using CSkyL.UI;
    using CStyle = CSkyL.UI.Style;
    using Ctransl = CSkyL.Translation.Translations;

    public class OptionsMenu : OptionsBase
    {
        public override void Generate(GameElement settingPanel)
        {
            CStyle.Current = Style.basic;
            {
                var group = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "General", text = Ctransl.Translate("SETTINGS_GROUPNAME_GENERAL")
                });
                var props = _DefaultProps(group);
                props.x = CStyle.Current.padding;
                const float gap = 10f;

                var lang = group.Add<LangChoiceSetting>(props.Swap(Config.instance.Language));
                lang._dropdown.items = Ctransl.LanguageList; props.y += lang.height + gap; _settings.Add(lang);

                var tog = group.Add<ToggleSetting>(props.Swap(Config.instance.HideGameUI));
                props.y += tog.height + gap; _settings.Add(tog);

                tog = group.Add<ToggleSetting>(props.Swap(Config.instance.SetBackCamera));
                props.y += tog.height + gap; _settings.Add(tog);

                tog = group.Add<ToggleSetting>(props.Swap(Config.instance.UseMetricUnit));
                props.y += tog.height + gap; _settings.Add(tog);

                tog = group.Add<ToggleSetting>(props.Swap(Config.instance.ShowInfoPanel));
                props.y += tog.height + gap; _settings.Add(tog);

                props.stepSize = .05f; props.valueFormat = "F2";
                var slider = group.Add<SliderSetting>(props.Swap(Config.instance.InfoPanelHeightScale));
                props.y += tog.height + gap; _settings.Add(slider);

                props.stepSize = 1f; props.valueFormat = "F0";
                slider = group.Add<SliderSetting>(props.Swap(Config.instance.MaxPitchDeg));
                props.y += tog.height; _settings.Add(slider);

                group.contentHeight = props.y + CStyle.Current.padding;

                var btnProps = new Properties
                {
                    name = "ReloadConfig", text = Ctransl.Translate("SETTINGS_RELOADBTN"),
                    x = group.width - _btnSize.width - Style.basic.padding * 2f,
                    y = 50f, size = _btnSize
                };
                var btn = group.Add<TextButton>(btnProps);
                btn.SetTriggerAction(() => Mod.I?.LoadConfig());

                btnProps.name = "ResetConfig"; btnProps.text = Ctransl.Translate("SETTINGS_RESETBTN");
                btnProps.y += _btnSize.height;
                btn = group.Add<TextButton>(btnProps);
                btn.SetTriggerAction(() => Mod.I?.ResetConfig());
            }
            {
                var group = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "CamControl", text = Ctransl.Translate("SETTINGS_GROUPNAME_CAMCONTROL"),
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(group);

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.MovementSpeed)));
                props.stepSize = .25f; props.valueFormat = "F2";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.SpeedUpFactor)));

                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.InvertRotateVertical)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.InvertRotateHorizontal)));
                props.stepSize = .25f; props.valueFormat = "F2";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.RotateSensitivity)));
                props.stepSize = .5f; props.valueFormat = "F1";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.RotateKeyFactor)));
                props.stepSize = .1f; props.valueFormat = "F1";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.FoViewScrollfactor)));

                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.EnableDof)));
                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.CamFieldOfView)));
                props.stepSize = .1f; props.valueFormat = "F1";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.CamNearClipPlane)));
            }
            {
                var group = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "FreeCam", text = Ctransl.Translate("SETTINGS_GROUPNAME_FREECAM"),
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(group);

                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.ShowCursor4Free)));
                var choice = group.Add<ChoiceSettingv2>(props.Swap(Config.instance.GroundClippingOption));
                choice._dropdown.items = Config.GroundClipping; _settings.Add(choice);
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.GroundLevelOffset)));
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.RoadLevelOffset)));
            }
            {
                var group = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "FollowWalkThru", text = Ctransl.Translate("SETTINGS_GROUPNAME_FOLLOWWALKTHRU"),
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(group);

                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.ShowCursor4Follow)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.StickToFrontVehicle)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.LookAhead)));

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.InstantMoveMax)));

                _settings.Add(group.Add<OffsetSetting>(props.Swap(Config.instance.FollowCamOffset)));
                _settings.Add(group.Add<OffsetSetting>(props.Swap(Config.instance.VehicleFixedOffset)));
                _settings.Add(group.Add<OffsetSetting>(props.Swap(Config.instance.MidVehFixedOffset)));
                _settings.Add(group.Add<OffsetSetting>(props.Swap(Config.instance.PedestrianFixedOffset)));
            }
            {
                var group = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "WalkThru", text = Ctransl.Translate("SETTINGS_GROUPNAME_WALKTHRU"),
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(group);

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.Period4Walk)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.ManualSwitch4Walk)));

                props.text = Ctransl.Translate("SETTINGS_TARGETSTOFOLLOW");
                group.Add<Label>(props);
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.SelectPedestrian)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.SelectPassenger)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.SelectWaiting)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.SelectDriving)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.SelectPublicTransit)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.SelectService)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.SelectCargo)));
            }
            {
                var group = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "KeyMap", text = Ctransl.Translate("SETTINGS_GROUPNAME_KEYMAP"),
                    autoLayout = true, layoutGap = 0
                });
                group.Add<Label>(new Properties
                {
                    name = "KeyMappingComment",
                    text = Ctransl.Translate("SETTINGS_KEYMAPPINGCOMMENT")
                });
                var props = _DefaultProps(group);

                CStyle.Current.scale = .8f;
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeyCamToggle)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeySpeedUp)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeyCamReset)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeyCursorToggle)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeyAutoMove)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeySaveOffset)));

                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeyMoveForward)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeyMoveBackward)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeyMoveLeft)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeyMoveRight)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeyMoveUp)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeyMoveDown)));

                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeyRotateLeft)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeyRotateRight)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeyRotateUp)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.instance.KeyRotateDown)));
                CStyle.Current = Style.basic;
            }
            {
                var group = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "SmoothTrans", text = Ctransl.Translate("SETTINGS_GROUPNAME_SMOOTHTRANS"),
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(group);

                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.SmoothTransition)));

                props.stepSize = .1f; props.valueFormat = "F1";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.TransRate)));

                props.stepSize = 50f; props.valueFormat = "F0";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.GiveUpTransDistance)));

                props.stepSize = .05f; props.valueFormat = "F2";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.MinTransMove)));

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.MaxTransMove)));

                props.stepSize = .05f; props.valueFormat = "F2";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.MinTransRotate)));

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.MaxTransRotate)));
                props.stepSize = .1f; props.valueFormat = "F1";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.instance.MaxExitingDuration)));
            }
            {
                var group = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "LODOpt", text = Ctransl.Translate("SETTINGS_GROUPNAME_LODSOPT"),
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(group);
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.instance.LODOptimization)));
            }
        }

        private const float rightMargin = 30f;
        private SettingProperties _DefaultProps(Group group) => new SettingProperties
        {
            width = group.contentWidth - rightMargin - CStyle.Current.padding * 2f,
            wideCondition = true,
            configObj = Config.instance
        };

        private static readonly CSkyL.Math.Vec2D _btnSize = CSkyL.Math.Vec2D.Size(200f, 40f);
    }
}
