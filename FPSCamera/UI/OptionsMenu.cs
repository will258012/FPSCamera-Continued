namespace FPSCamera.UI
{
    using Configuration;
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

                var lang = group.Add<LangSetting>(props.Swap(Config.G.Language));
                props.y += lang.height + gap; _settings.Add(lang); 

                var tog = group.Add<ToggleSetting>(props.Swap(Config.G.HideGameUI));
                props.y += tog.height + gap; _settings.Add(tog);

                tog = group.Add<ToggleSetting>(props.Swap(Config.G.SetBackCamera));
                props.y += tog.height + gap; _settings.Add(tog);

                tog = group.Add<ToggleSetting>(props.Swap(Config.G.UseMetricUnit));
                props.y += tog.height + gap; _settings.Add(tog);

                tog = group.Add<ToggleSetting>(props.Swap(Config.G.ShowInfoPanel));
                props.y += tog.height + gap; _settings.Add(tog);

                props.stepSize = .05f; props.valueFormat = "F2";
                var slider = group.Add<SliderSetting>(props.Swap(Config.G.InfoPanelHeightScale));
                props.y += tog.height + gap; _settings.Add(slider);

                props.stepSize = 1f; props.valueFormat = "F0";
                slider = group.Add<SliderSetting>(props.Swap(Config.G.MaxPitchDeg));
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
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.MovementSpeed)));
                props.stepSize = .25f; props.valueFormat = "F2";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.SpeedUpFactor)));

                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.InvertRotateVertical)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.InvertRotateHorizontal)));
                props.stepSize = .25f; props.valueFormat = "F2";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.RotateSensitivity)));
                props.stepSize = .5f; props.valueFormat = "F1";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.RotateKeyFactor)));
                props.stepSize = .1f; props.valueFormat = "F1";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.FoViewScrollfactor)));

                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.EnableDof)));
                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.CamFieldOfView)));
                props.stepSize = .1f; props.valueFormat = "F1";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.CamNearClipPlane)));
            }
            {
                var group = settingPanel.Add<Group>(new LayoutProperties
                {
                   name = "FreeCam", text = Ctransl.Translate("SETTINGS_GROUPNAME_FREECAM"),
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(group);

                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.ShowCursor4Free)));

                _settings.Add(group.Add<ChoiceSetting<Config.GroundClipping>>(
                                    props.Swap(Config.G.GroundClippingOption)));
                props.stepSize = .1f; props.valueFormat = "F1";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.GroundLevelOffset)));
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.RoadLevelOffset)));
            }
            {
                var group = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "FollowWalkThru", text = Ctransl.Translate("SETTINGS_GROUPNAME_FOLLOWWALKTHRU"),
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(group);

                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.ShowCursor4Follow)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.StickToFrontVehicle)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.LookAhead)));

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.InstantMoveMax)));

                _settings.Add(group.Add<OffsetSetting>(props.Swap(Config.G.FollowCamOffset)));
                _settings.Add(group.Add<OffsetSetting>(props.Swap(Config.G.VehicleFixedOffset)));
                _settings.Add(group.Add<OffsetSetting>(props.Swap(Config.G.MidVehFixedOffset)));
                _settings.Add(group.Add<OffsetSetting>(props.Swap(Config.G.PedestrianFixedOffset)));
            }
            {
                var group = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "WalkThru", text = Ctransl.Translate("SETTINGS_GROUPNAME_WALKTHRU"),
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(group);

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.Period4Walk)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.ManualSwitch4Walk)));

                props.text = Ctransl.Translate("SETTINGS_TARGETSTOFOLLOW");
                group.Add<Label>(props);
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.SelectPedestrian)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.SelectPassenger)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.SelectWaiting)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.SelectDriving)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.SelectPublicTransit)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.SelectService)));
                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.SelectCargo)));
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
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeyCamToggle)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeySpeedUp)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeyCamReset)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeyCursorToggle)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeyAutoMove)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeySaveOffset)));

                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeyMoveForward)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeyMoveBackward)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeyMoveLeft)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeyMoveRight)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeyMoveUp)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeyMoveDown)));

                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeyRotateLeft)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeyRotateRight)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeyRotateUp)));
                _settings.Add(group.Add<KeyMapSetting>(props.Swap(Config.G.KeyRotateDown)));
                CStyle.Current = Style.basic;
            }
            {
                var group = settingPanel.Add<Group>(new LayoutProperties
                {
                    name = "SmoothTrans", text = Ctransl.Translate("SETTINGS_GROUPNAME_SMOOTHTRANS"),
                    autoLayout = true, layoutGap = 10
                });
                var props = _DefaultProps(group);

                _settings.Add(group.Add<ToggleSetting>(props.Swap(Config.G.SmoothTransition)));

                props.stepSize = .1f; props.valueFormat = "F1";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.TransRate)));

                props.stepSize = 50f; props.valueFormat = "F0";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.GiveUpTransDistance)));

                props.stepSize = .05f; props.valueFormat = "F2";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.MinTransMove)));

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.MaxTransMove)));

                props.stepSize = .05f; props.valueFormat = "F2";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.MinTransRotate)));

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.MaxTransRotate)));
                props.stepSize = .1f; props.valueFormat = "F1";
                _settings.Add(group.Add<SliderSetting>(props.Swap(Config.G.MaxExitingDuration)));
            }
        }

        private const float rightMargin = 30f;
        private SettingProperties _DefaultProps(Group group) => new SettingProperties
        {
            width = group.contentWidth - rightMargin - CStyle.Current.padding * 2f,
            wideCondition = true,
            configObj = Config.G
        };

        private static readonly CSkyL.Math.Vec2D _btnSize = CSkyL.Math.Vec2D.Size(200f, 40f);
    }
}
