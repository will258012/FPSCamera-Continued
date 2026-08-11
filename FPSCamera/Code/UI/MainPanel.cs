using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using FPSCamera.Cam.Controller;
using FPSCamera.Settings;
using FPSCamera.Utils;
using UnifiedUI.GUI;
using UnityEngine;

namespace FPSCamera.UI
{
    public class MainPanel : MonoBehaviour
    {

        /// <summary>
        /// Gets the active button instance.
        /// </summary>
        public UIButton GetMainButton() => _mainBtn ?? UUISupport.UUIButton as UIButton;

        /// <summary>
        /// Gets the active panel instance.
        /// </summary>
        public UIPanel Panel { get; set; }
        /// <summary>
        /// Gets tje active <see cref="MainPanel"/> instance.
        /// </summary>
        public static MainPanel Instance { get; private set; }
        /// <summary>
        /// Gets or sets the panel's last saved position.
        /// </summary>
        public static Vector3 SavedPanelPosition { get; set; } = DefaultPosition;
        /// <summary>
        /// Gets or sets the button's last saved position.
        /// </summary>
        public static Vector3 SavedButtonPosition { get; set; } = DefaultPosition;

        public static Vector3 DefaultPosition => Vector3.left;
        private const float Margin = 10f;
        private const float SliderMargin = 60f;
        private const float CloseButtonSize = 35f;
        private const float MainButtonSize = 40f;
        private const float TitleHeight = 13f;
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
            Panel.opacity = 0f;

            AddSettings();

            Panel.isVisible = false;
            Panel.eventVisibilityChanged += OnChangedVisibility;
            fadeHelper.OnFadeCompleted += OnFadeCompleted;


            if (ModSupport.FoundUUI)
            {
                UUISupport.UUIRegister();
                return;
            }

            #endregion

            #region Main Button
            float x = SavedButtonPosition.x, y = SavedButtonPosition.y;
            if (x < 0f || y < 0f)
            {
                var escbutton = UIView.GetAView().FindUIComponent("Esc");
                x = escbutton.absolutePosition.x;
                y = escbutton.absolutePosition.y + escbutton.height * 1.5f;

                SavedButtonPosition = new Vector3(x, y);
            }
            _mainBtn = UIView.GetAView().AddUIComponent(typeof(UIButton)) as UIButton;
            _mainBtn.name = "MainButton";
            _mainBtn.tooltip = Translations.Translate("MAINPANELBTN_TOOLTIP");
            _mainBtn.absolutePosition = new Vector3(x, y);
            _mainBtn.size = new Vector2(MainButtonSize, MainButtonSize);
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
                if (!targetVisible) LoadPanelPosition();

                OnChangedVisibility(Panel, !targetVisible);
            };

            //drag
            var mainBtn_drag = _mainBtn.AddUIComponent<UIDragHandle>();
            mainBtn_drag.name = _mainBtn.name + "_drag";
            mainBtn_drag.size = _mainBtn.size;
            mainBtn_drag.relativePosition = Vector3.zero;
            mainBtn_drag.target = _mainBtn;
            mainBtn_drag.transform.parent = _mainBtn.transform;
            mainBtn_drag.eventMouseDown += (_, p) => OnChangedVisibility(Panel, false);
            mainBtn_drag.eventMouseUp += (_, p) => { SavedButtonPosition = _mainBtn.absolutePosition; ModSettings.Save(); };
            #endregion
        }

        private void AddSettings()
        {
            //settings
            var currentY = CloseButtonSize + Margin;
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

            var movementSpeed_Slider = UISliders.AddPlainSliderWithValue(Panel, Margin, currentY, Translations.Translate("SETTINGS_MOVEMENTSPEED"), 1f, 60f, .5f, ModSettings.MovementSpeed, Panel.width - 70f);
            movementSpeed_Slider.eventValueChanged += (_, value) => ModSettings.MovementSpeed = value;
            currentY += movementSpeed_Slider.height + SliderMargin;

            var offsetMovementSpeed_Slider = UISliders.AddPlainSliderWithValue(Panel, Margin, currentY, Translations.Translate("SETTINGS_OFFSETMOVEMENTSPEED"), 1f, 60f, .5f, ModSettings.OffsetMovementSpeed, Panel.width - 70f);
            offsetMovementSpeed_Slider.eventValueChanged += (_, value) => ModSettings.OffsetMovementSpeed = value;
            currentY += offsetMovementSpeed_Slider.height + SliderMargin;

            var fov_Slider = UISliders.AddPlainSliderWithValue(Panel, Margin, currentY, Translations.Translate("SETTINGS_FIELDOFVIEW"), 10f, 75f, 1f, ModSettings.CamFieldOfView, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1f, numberFormat: "N0", suffix: "°"), Panel.width - 70f);
            fov_Slider.eventValueChanged += (_, value) => ModSettings.CamFieldOfView = value;
            currentY += fov_Slider.height + SliderMargin;

            string[] groundClippingItems =
            [
                Translations.Translate("SETTINGS_GROUNDCLIPING_NONE"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_ABOVE_GROUND"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_SNAP_TO_GROUND"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_ABOVE_ROAD"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_SNAP_TO_ROAD")
            ];

            var groundClipping_dropDown = UIDropDowns.AddPlainDropDown(Panel, Margin, currentY, Translations.Translate("SETTINGS_GROUNDCLIPING"), groundClippingItems, (int)ModSettings.GroundClipping, Panel.width - 70f);
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
                currentY += walkThruBtn.height + Margin;
                var allSettingsBtn = UIButtons.AddButton(Panel, UILayout.PositionUnder(walkThruBtn), Translations.Translate("ALLSETTINGSBTN_TEXT"));
                allSettingsBtn.eventClick += (_, e) =>
                {
                    OpenSettingsPanel();
                };
                Panel.height = currentY + allSettingsBtn.height + Margin;
            }
            else
            {
                var allSettingsBtn = UIButtons.AddButton(Panel, (Panel.width - 200f) / 2f, currentY, Translations.Translate("ALLSETTINGSBTN_TEXT"));
                allSettingsBtn.eventClick += (_, e) =>
                {
                    OpenSettingsPanel();
                };
                Panel.height = currentY + allSettingsBtn.height + Margin;
            }
            // Title
            {
                // Drag bar.
                var dragHandle = Panel.AddUIComponent<UIDragHandle>();
                dragHandle.size = Panel.size;
                dragHandle.relativePosition = Vector3.zero;
                dragHandle.target = Panel;
                dragHandle.SendToBack();

                // Title label.
                var titleLabel = UILabels.AddLabel(Panel, CloseButtonSize, TitleHeight, Translations.Translate("MAINPANELBTN_TOOLTIP"), Panel.width - CloseButtonSize - CloseButtonSize, alignment: UIHorizontalAlignment.Center);
                titleLabel.SendToBack();

                // Close button.
                var closeButton = Panel.AddUIComponent<UIButton>();
                closeButton.relativePosition = new Vector2(Panel.width - CloseButtonSize, 2);
                closeButton.atlas = UITextures.InGameAtlas;
                closeButton.normalBgSprite = "buttonclose";
                closeButton.hoveredBgSprite = "buttonclosehover";
                closeButton.pressedBgSprite = "buttonclosepressed";
                closeButton.eventClick += (c, p) => OnEsc();
            }
        }
        private void OnDestroy()
        {
            fadeHelper.Reset();
            Panel.eventVisibilityChanged -= OnChangedVisibility;
            fadeHelper.OnFadeCompleted -= OnFadeCompleted;

            Destroy(Panel);
            Destroy(GetMainButton());
        }
        public bool OnEsc()
        {
            if (targetVisible)
            {
                OnChangedVisibility(Panel, false);
                if (ModSupport.FoundUUI)
                {
                    (GetMainButton() as ButtonBase)?.IsActive = false;
                }
                return true;
            }
            return false;
        }
        public void LocaleChanged()
        {
            var wasVisible = targetVisible;
            fadeHelper.Reset();
            foreach (var component in Panel.components)
            {
                Destroy(component.gameObject);
            }
            AddSettings();
            targetVisible = wasVisible;
            Panel.opacity = wasVisible ? 1f : 0f;
            SetPanelVisibilityImmediate(wasVisible);
        }
        public static void OpenSettingsPanel(string modName)
        {
            var panel = UIView.library.ShowModal<OptionsMainPanel>("OptionsPanel");
            panel.SelectMod(modName);
        }
        public void LoadPanelPosition()
        {
            if (Panel == null) return;
            var view = UIView.GetAView();

            Panel.absolutePosition = SavedPanelPosition.x >= 0f
                ? SavedPanelPosition
                : new Vector3(Mathf.Floor((view.fixedWidth - Panel.width) / 2), Mathf.Floor((view.fixedHeight - Panel.height) / 2));

            // Ensure panel is fully visible on screen (in case of e.g. UI scaling changes).
            float clampedXpos = Mathf.Clamp(Panel.absolutePosition.x, 0f, view.fixedWidth - Panel.width);
            float clampedYpos = Mathf.Clamp(Panel.absolutePosition.y, 0f, view.fixedHeight - Panel.height);
            Panel.absolutePosition = new Vector2(clampedXpos, clampedYpos);
        }

        private static void OpenSettingsPanel() => OpenSettingsPanel(Mod.Instance.Name);
        internal void OnChangedVisibility(UIComponent _, bool visible)
        {
            if (changingPanelVisibility)
                return;

            var wasTargetVisible = targetVisible;
            targetVisible = visible;

            if (!visible && wasTargetVisible)
            {
                SavedPanelPosition = Panel.absolutePosition;
                ModSettings.Save();
            }

            if (!visible && !Panel.isVisible && fadeHelper.Opacity <= 0f && !fadeHelper.IsFading)
                return;

            // Keep the panel active while fading so opacity changes remain visible.
            SetPanelVisibilityImmediate(true);

            if (visible)
                fadeHelper.FadeIn();
            else
                fadeHelper.FadeOut();
        }
        private void OnFadeCompleted(FadeHelper.FadeType fadeType)
        {
            if (fadeType == FadeHelper.FadeType.Out && !targetVisible)
                SetPanelVisibilityImmediate(false);
        }

        private void SetPanelVisibilityImmediate(bool visible)
        {
            if (Panel.isVisible == visible)
                return;

            changingPanelVisibility = true;
            try
            {
                Panel.isVisible = visible;
            }
            finally
            {
                changingPanelVisibility = false;
            }
        }

        private UIButton _mainBtn = null;

        private FadeHelper fadeHelper = new MainPanelFadeHelper();
        private bool targetVisible;
        private bool changingPanelVisibility;

        private sealed class MainPanelFadeHelper() : FadeHelper
        {

            public override string FadeID => Mod.Instance.HarmonyID + ".MainPanel.Fade";
            public override float Opacity
            {
                get => Instance.Panel.opacity;
                set => Instance.Panel.opacity = value;
            }
        }
    }
}
