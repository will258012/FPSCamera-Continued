namespace FPSCamera.UI
{
    using Config;
    using CSkyL.Game.Utils;
    using CSkyL.UI;
    using System.Collections.Generic;
    using CStyle = CSkyL.UI.Style;
    using Ctransl = CSkyL.Translation.Translations;
    using Vec2D = CSkyL.Math.Vec2D;

    internal class MainPanel : CSkyL.Game.Behavior
    {
        public void OnCamDeactivate()
        { _msgTimer = 0f; }
        public void OnCamActivate()
        {
            _mainPanel.Visible = false;
            ShowMessage(string.Format(Ctransl.Translate("MAINPANELBTN_EXIGMSG"), Config.instance.KeyCamToggle));
        }

        public void ShowMessage(string msg)
        {
            _msgLabel.text = msg;
            _msgLabel.position = _MsgLabelPosition;
            _msgTimer = _msgDuration;
        }

        public bool OnEsc()
        {
            if (_mainPanel.Visible) {
                _mainPanel.Visible = false;
                return true;
            }
            return false;
        }
        public void SetWalkThruCallBack(System.Action callBackAction)
            => _walkThruCallBack = callBackAction;

        private Vec2D _MsgLabelPosition => Vec2D.Position(
                _panelBtn.x > Helper.ScreenWidth / 2f ?
                    -_msgLabel.width - _msgLabelPadding : _panelBtn.width + _msgLabelPadding,
                (_panelBtn.height - _msgLabel.height) / 2f
        );

        public static MainPanel Instance { get; private set; }

        protected override void _Init()
        {
            Instance = this;
            CStyle.Current = Style.basic;
            {
                CStyle.Current.scale = .75f;
                float x = Config.instance.MainPanelBtnPos.x, y = Config.instance.MainPanelBtnPos.y;
                if (x < 0f || y < 0f) {
                    var escbutton = Helper.GetElement("Esc");
                    x = escbutton.x;
                    y = escbutton.y + escbutton.height * 1.5f;
                    Config.instance.MainPanelBtnPos.Assign(Vec2D.Position(x, y));
                    Config.instance.Save();
                }
                _panelBtn = Element.Root.Add<SpriteButton>(new Properties
                {
                    name = "MainPanelBtn", tooltip = Ctransl.Translate("MAINPANELBTN_TOOLTIP"),
                    x = x, y = y, size = _mainBtnSize,
                    sprite = "InfoPanelIconFreecamera"
                });
                CStyle.Current = Style.basic;
            }

            _msgLabel = _panelBtn.Add<Label>(new Properties { name = "ToggleMsgLabel", });
            _msgLabel.position = _MsgLabelPosition;
            ShowMessage(string.Format(Ctransl.Translate("MAINPANELBTN_TOGGLEMSG"), Config.instance.KeyCamToggle));

            _mainPanel = Element.Root.Add<SpritePanel>(new LayoutProperties
            {
                name = "MainPanel",
                width = 400f, height = 1000f,
                autoLayout = true, layoutGap = 10
            });
            _panelBtn.SetTriggerAction(() => {
                _mainPanel.position = Vec2D.Position(
                    _panelBtn.x + (_panelBtn.x < Helper.ScreenWidth / 2f ?
                         _panelBtn.width - 10f : -_mainPanel.width + 10f),
                    _panelBtn.y + (_panelBtn.y < Helper.ScreenHeight / 2f ?
                        _panelBtn.height - 15f : -_mainPanel.height + 15f)
                  );
                _mainPanel.Visible = !_mainPanel.Visible;
            });
            _panelBtn.MakeDraggable(
                actionDragStart: () => _mainPanel.Visible = false,
                actionDragEnd: () => {
                    Config.instance.MainPanelBtnPos.Assign(Vec2D.Position(_panelBtn.x, _panelBtn.y));
                    Config.instance.Save();
                });

            var props = new SettingProperties
            {
                width = _mainPanel.width - Style.basic.padding * 2f,
                configObj = Config.instance
            };

            _settings.Add(_mainPanel.Add<ToggleSetting>(props.Swap(Config.instance.HideGameUI)));
            _settings.Add(_mainPanel.Add<ToggleSetting>(props.Swap(Config.instance.ShowInfoPanel)));
            _settings.Add(_mainPanel.Add<ToggleSetting>(props.Swap(Config.instance.SetBackCamera)));

            props.stepSize = 1f; props.valueFormat = "F0";
            _settings.Add(_mainPanel.Add<SliderSetting>(props.Swap(Config.instance.MovementSpeed)));

            _settings.Add(_mainPanel.Add<ToggleSetting>(props.Swap(Config.instance.EnableDof)));
            props.stepSize = 1f; props.valueFormat = "F0";
            _settings.Add(_mainPanel.Add<SliderSetting>(props.Swap(Config.instance.CamFieldOfView)));

            _settings.Add(_mainPanel.Add<ToggleSetting>(props.Swap(Config.instance.SmoothTransition)));

            var tmpLast = _mainPanel.Add<ChoiceSettingv2>(props.Swap(Config.instance.GroundClippingOption));
            tmpLast._dropdown.items = Config.GroundClipping; _settings.Add(tmpLast);

            if (GameUtil.InGameMode) {

                _settings.Add(_mainPanel.Add<ToggleSetting>(props.Swap(Config.instance.StickToFrontVehicle)));

                props.stepSize = 1f; props.valueFormat = "F0";
                _settings.Add(_mainPanel.Add<SliderSetting>(props.Swap(Config.instance.Period4Walk)));

                var last = _mainPanel.Add<ToggleSetting>(props.Swap(Config.instance.ManualSwitch4Walk));
                _settings.Add(last);

                _mainPanel.AutoLayout = false;
                _mainPanel.height = last.bottom + _walkThruBtnSize.height +
                                    Style.basic.padding * 2;

                var walkThruBtn = _mainPanel.Add<TextButton>(new Properties
                {
                    name = "WalkThruBtn", text = Ctransl.Translate("WALKTHRUBTN_TEXT"),

                    x = (_mainPanel.width - _walkThruBtnSize.width) / 2f,
                    y = _mainPanel.height - Style.basic.padding - _walkThruBtnSize.height,
                    size = _walkThruBtnSize
                });
                walkThruBtn.SetTriggerAction(() => _walkThruCallBack?.Invoke());
            }
            else {
                _mainPanel.AutoLayout = false;
                _mainPanel.height = tmpLast.bottom + Style.basic.padding;
            }
            _mainPanel.Visible = false;
            if (CSkyL.ModSupport.IsUUIFoundandEnabled) {
                UUISupport.UUIRegister();
                _panelBtn.Visible = false;
            }
        }
        protected override void _UpdateLate()
        {
            foreach (var setting in _settings) setting.UpdateUI();

            _msgTimer -= GameUtil.TimeSinceLastFrame;
            _msgLabel.opacity = _msgTimer / _msgDuration;
        }

        private Label _msgLabel;
        [CSkyL.Game.RequireDestruction] public SpriteButton _panelBtn;
        [CSkyL.Game.RequireDestruction] public Panel _mainPanel;

        private readonly List<ISetting> _settings = new List<ISetting>();

        private float _msgTimer = 0f;
        private System.Action _walkThruCallBack;

        private static readonly Vec2D _mainBtnSize = Vec2D.Size(40f, 39f);
        private static readonly Vec2D _walkThruBtnSize = Vec2D.Size(200f, 40f);
        private const float _msgLabelPadding = 3f;
        private const float _msgDuration = 10f;
    }
}
