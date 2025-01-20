namespace FPSCamera.UI
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;
    using FPSCamera.Cam.Controller;
    using FPSCamera.Settings;
    using FPSCamera.Utils;
    using System;
    using UnifiedUI.GUI;
    using UnityEngine;

    public class MainPanel : MonoBehaviour
    {
        public UIButton GetMainButton() => _mainBtn ?? UUISupport.UUIButton as UIButton;
        public UIPanel Panel { get; set; }
        private const float Margin = 10f;
        private const float SliderMargin = 60f;
        public static MainPanel Instance { get; private set; }
        private void Awake()
        {
            Instance = this;

            #region Main Panel

            Panel = UIView.GetAView().AddUIComponent(typeof(UIPanel)) as UIPanel;
            Panel.autoLayout = false;
            Panel.canFocus = true;
            Panel.isInteractive = true;
            Panel.atlas = UITextures.InGameAtlas;
            Panel.backgroundSprite = "UnlockingPanel2";
            Panel.width = 400f;

            AddSettings();

            Panel.eventVisibilityChanged += OnChangedVisibility;
            Panel.isVisible = false;

            if (ModSupport.FoundUUI)
            {
                UUISupport.UUIRegister();
                return;
            }

            #endregion

            #region Main Button
            float x = ModSettings.MainButtonPos.x, y = ModSettings.MainButtonPos.y;
            if (x < 0f || y < 0f)
            {
                var escbutton = UIView.GetAView().FindUIComponent("Esc");
                x = escbutton.absolutePosition.x;
                y = escbutton.absolutePosition.y + escbutton.height * 1.5f;

                ModSettings.MainButtonPos = new Vector3(x, y);
            }
            _mainBtn = UIView.GetAView().AddUIComponent(typeof(UIButton)) as UIButton;
            _mainBtn.name = "MainButton";
            _mainBtn.tooltip = Translations.Translate("MAINPANELBTN_TOOLTIP");
            _mainBtn.absolutePosition = new Vector3(x, y);
            _mainBtn.size = new Vector2(40f, 40f);
            _mainBtn.scaleFactor = .8f;
            _mainBtn.pressedBgSprite = "OptionBasePressed";
            _mainBtn.normalBgSprite = "OptionBase";
            _mainBtn.hoveredBgSprite = "OptionBaseHovered";
            _mainBtn.disabledBgSprite = "OptionBaseDisabled";
            _mainBtn.normalFgSprite = "InfoPanelIconFreecamera";
            _mainBtn.textColor = new Color32(255, 255, 255, 255);
            _mainBtn.disabledTextColor = new Color32(7, 7, 7, 255);
            _mainBtn.hoveredTextColor = new Color32(255, 255, 255, 255);
            _mainBtn.focusedTextColor = new Color32(255, 255, 255, 255);
            _mainBtn.pressedTextColor = new Color32(30, 30, 44, 255);
            _mainBtn.eventClick += (_, m) =>
            {
                Panel.absolutePosition = new Vector3(_mainBtn.absolutePosition.x +
                    (_mainBtn.absolutePosition.x < Screen.height / 2f ? _mainBtn.width - 10f : -Panel.width + 10f),
                                                           _mainBtn.absolutePosition.y +
                (_mainBtn.absolutePosition.y < Screen.height / 2f ? _mainBtn.height - 15f : -Panel.height + 15f));
                Panel.isVisible = !Panel.isVisible;
            };

            //drag
            var mainBtn_drag = _mainBtn.AddUIComponent<UIDragHandle>();
            mainBtn_drag.name = _mainBtn.name + "_drag";
            mainBtn_drag.size = _mainBtn.size;
            mainBtn_drag.relativePosition = Vector3.zero;
            mainBtn_drag.target = _mainBtn;
            mainBtn_drag.transform.parent = _mainBtn.transform;
            mainBtn_drag.eventMouseDown += (_, p) => Panel.isVisible = false;
            mainBtn_drag.eventMouseUp += (_, p) => { ModSettings.MainButtonPos = _mainBtn.absolutePosition; ModSettings.Save(); };
            #endregion
        }

        private void AddSettings()
        {
            //settings
            var currentY = Margin;
            var hideUI_CheckBox = UICheckBoxes.AddPlainCheckBox(Panel, Margin, currentY, Translations.Translate("SETTINGS_HIDEUI"), Panel.width - Margin);
            hideUI_CheckBox.isChecked = ModSettings.HideGameUI;
            hideUI_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.HideGameUI = isChecked;
            currentY += hideUI_CheckBox.height + Margin;

            var setBackCamera_CheckBox = UICheckBoxes.AddPlainCheckBox(Panel, Margin, currentY, Translations.Translate("SETTINGS_SETBACKCAMERA"), Panel.width - Margin);
            setBackCamera_CheckBox.tooltip = Translations.Translate("SETTINGS_SETBACKCAMERA_DETAIL");
            setBackCamera_CheckBox.isChecked = ModSettings.SetBackCamera;
            setBackCamera_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.SetBackCamera = isChecked;
            currentY += setBackCamera_CheckBox.height + Margin;

            var showInfoPanel_CheckBox = UICheckBoxes.AddPlainCheckBox(Panel, Margin, currentY, Translations.Translate("SETTINGS_SHOWINFOPANEL"), Panel.width - Margin);
            showInfoPanel_CheckBox.isChecked = ModSettings.ShowInfoPanel;
            showInfoPanel_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.ShowInfoPanel = isChecked;
            currentY += showInfoPanel_CheckBox.height + Margin;

            var dof_CheckBox = UICheckBoxes.AddPlainCheckBox(Panel, Margin, currentY, Translations.Translate("SETTINGS_ENABLEDOF"), Panel.width - Margin);
            dof_CheckBox.isChecked = ModSettings.Dof;
            dof_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.Dof = isChecked;
            currentY += dof_CheckBox.height + Margin;

            var movementSpeed_Slider = UISliders.AddPlainSliderWithValue(Panel, Margin, currentY, Translations.Translate("SETTINGS_MOVEMENTSPEED"), 0f, 60f, .5f, ModSettings.MovementSpeed, Panel.width - 70f);
            movementSpeed_Slider.eventValueChanged += (_, value) => ModSettings.MovementSpeed = value;
            currentY += movementSpeed_Slider.height + SliderMargin;

            var fov_Slider = UISliders.AddPlainSliderWithValue(Panel, Margin, currentY, Translations.Translate("SETTINGS_FIELDOFVIEW"), 10f, 75f, 1f, ModSettings.CamFieldOfView, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1f, numberFormat: "N0", suffix: "°"), Panel.width - 70f);
            fov_Slider.eventValueChanged += (_, value) => ModSettings.CamFieldOfView = value;
            currentY += fov_Slider.height + SliderMargin;

            string[] groundClippingItems = new[]
            {
                Translations.Translate("SETTINGS_GROUNDCLIPING_NONE"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_ABOVE_GROUND"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_SNAP_TO_GROUND"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_ABOVE_ROAD"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_SNAP_TO_ROAD")
            };

            var groundClipping_dropDown = UIDropDowns.AddPlainDropDown(Panel, Margin, currentY, Translations.Translate("SETTINGS_GROUNDCLIPING"), groundClippingItems, (int)ModSettings.GroundClipping, 150f);
            groundClipping_dropDown.tooltip = string.Format(Translations.Translate("SETTINGS_GROUNDCLIPING_DETAIL"), "\n");
            groundClipping_dropDown.eventSelectedIndexChanged += (_, index) => ModSettings.GroundClipping = (ModSettings.GroundClippings)index;
            groundClipping_dropDown.parent.relativePosition = new Vector2(Margin, currentY);
            groundClipping_dropDown.canFocus = false;
            currentY += groundClipping_dropDown.parent.height + Margin;

            if (ToolsModifierControl.isGame)
            {
                var stickToFrontVehicle_CheckBox = UICheckBoxes.AddPlainCheckBox(Panel, Margin, currentY, Translations.Translate("SETTINGS_STICKTOFRONTVEHICLE"), Panel.width - Margin);
                stickToFrontVehicle_CheckBox.isChecked = ModSettings.StickToFrontVehicle;
                stickToFrontVehicle_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.StickToFrontVehicle = isChecked;
                currentY += stickToFrontVehicle_CheckBox.height + Margin;


                var periodWalk_Slider = UISliders.AddPlainSliderWithValue(Panel, Margin, currentY, Translations.Translate("SETTINGS_PERIODWALK"), 5f, 200f, 1f, ModSettings.PeriodWalk, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1f, numberFormat: "N0", suffix: "s"), Panel.width - 70f);
                periodWalk_Slider.eventValueChanged += (_, value) => ModSettings.PeriodWalk = value;
                currentY += periodWalk_Slider.height + SliderMargin;

                var manualSwitchWalk_CheckBox = UICheckBoxes.AddPlainCheckBox(Panel, Margin, currentY, Translations.Translate("SETTINGS_MANUALSWITCHWALK"), Panel.width - Margin);
                manualSwitchWalk_CheckBox.tooltip = Translations.Translate("SETTINGS_MANUALSWITCHWALK_DETAIL");
                manualSwitchWalk_CheckBox.isChecked = ModSettings.ManualSwitchWalk;
                manualSwitchWalk_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.ManualSwitchWalk = isChecked;
                currentY += manualSwitchWalk_CheckBox.height + Margin;

                var walkThruBtn = UIButtons.AddButton(Panel, (Panel.width - 200f) / 2f, currentY, Translations.Translate("WALKTHRUBTN_TEXT"), 200f, 40f);
                walkThruBtn.playAudioEvents = true;
                walkThruBtn.eventClick += (_, m) =>
                {
                    FPSCamController.Instance.StartWalkThruCam();
                    OnEsc();
                };
                Panel.height = currentY + walkThruBtn.height + Margin;
            }
            else
            {
                Panel.height = currentY;
            }
        }
        private void OnDestory()
        {
            Panel.eventVisibilityChanged -= OnChangedVisibility;

            Destroy(Panel);
            Destroy(GetMainButton());

        }
        private void Close()
        {
            foreach (var component in Panel.components)
            {
                Destroy(component.gameObject);
            }
        }
        public bool OnEsc()
        {
            if (Panel.isVisible)
            {
                Panel.isVisible = false;
                if (ModSupport.FoundUUI)
                {
                    (GetMainButton() as ButtonBase).IsActive = false;
                }
                return true;
            }
            return false;
        }
        public void LocaleChanged()
        {
            Close();
            AddSettings();
        }
        private void OnChangedVisibility(UIComponent component, bool value)
        {
            if (isAnimating) return;
            if (!value)
            {
                if (ModSettings.Fade)
                {
                    isAnimating = true;
                    RunFadeInOrOutAnimation(value, () =>
                    {
                        Panel.isVisible = false;
                        isAnimating = false;
                        ModSettings.Save();
                    });
                }
                else
                {
                    ModSettings.Save();
                    Panel.opacity = 0f;
                }
            }
            else
            {
                Panel.opacity = 1f;
                if (ModSettings.Fade)
                {
                    isAnimating = true;
                    RunFadeInOrOutAnimation(value, () => isAnimating = false);
                }
            }
        }
        //Edited from BrokenNodeDetector.UI.RunFadeInOrOutAnimation() by krzychu1245. Many Thanks!
        private void RunFadeInOrOutAnimation(bool status, Action action = null)
        {
            if (!Panel.isVisible)
            {
                Panel.isVisible = true;
            }

            ValueAnimator.Animate("fade_in_out",
                f => Panel.opacity = f,
                new AnimatedFloat(status ? 0f : 1f, status ? 1f : 0.0f, 0.2f, EasingType.SineEaseOut),
                () => action?.Invoke());
        }
        private UIButton _mainBtn = null;
        private bool isAnimating = false;
    }
}