using AlgernonCommons;
using AlgernonCommons.Notifications;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using FPSCamera.Settings.v2;
using FPSCamera.UI;
using FPSCamera.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace FPSCamera.Settings.Tabs
{
    public sealed class GeneralOptions
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float LeftMargin = 24f;
        private const float GroupMargin = 40f;
        private const float TitleMargin = 50f;
        private const float SliderMargin = 60f;

        private readonly UIDropDown language_DropDown;
        private readonly UICheckBox logging_CheckBox;
        private readonly UICheckBox hideUI_CheckBox;
        private readonly UICheckBox setBackCamera_CheckBox;
        private readonly UIDropDown speedUnit_DropDown;
        private readonly UICheckBox showInfoPanel_CheckBox;
        private readonly UISlider heightScale_Slider;
        private readonly UIButton defaults_Button;
        private readonly UIDropDown LodOpt_DropDown;
        private readonly UICheckBox ShadowsOpt_CheckBox;
        private readonly UICheckBox fade_CheckBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal GeneralOptions(UITabstrip tabStrip, int tabIndex)
        {

            var panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("SETTINGS_GROUPNAME_GENERAL"), tabIndex, out var _, autoLayout: false);
            var headerWidth = OptionsPanelManager<OptionsPanel>.PanelWidth - (Margin * 2f);
            var currentY = LeftMargin;

            //Add Scrollbar. 
            var scrollPanel = panel.AddUIComponent<UIScrollablePanel>();
            scrollPanel.relativePosition = new Vector2(0, Margin);
            scrollPanel.autoSize = false;
            scrollPanel.autoLayout = false;
            scrollPanel.width = scrollPanel.width - 15f;
            scrollPanel.height = scrollPanel.height - 15f;
            scrollPanel.clipChildren = true;
            scrollPanel.builtinKeyNavigation = true;
            scrollPanel.scrollWheelDirection = UIOrientation.Vertical;
            //Fix occasional content offset issue when switching pages (Why?)
            scrollPanel.eventVisibilityChanged += (_, isShow) => { if (isShow) scrollPanel.Reset(); };
            UIScrollbars.AddScrollbar(panel, scrollPanel);

            language_DropDown = UIDropDowns.AddPlainDropDown(scrollPanel, LeftMargin, currentY, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
            language_DropDown.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
                MainPanel.Instance?.LocaleChanged();
            };
            language_DropDown.parent.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += language_DropDown.parent.height + LeftMargin;

            logging_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("DETAIL_LOGGING"));
            logging_CheckBox.isChecked = Logging.DetailLogging;
            logging_CheckBox.eventCheckChanged += (_, isChecked) => Logging.DetailLogging = isChecked;
            currentY += logging_CheckBox.height + LeftMargin;

            hideUI_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_HIDEUI"));
            hideUI_CheckBox.isChecked = ModSettings.HideGameUI;
            hideUI_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.HideGameUI = isChecked;
            currentY += hideUI_CheckBox.height + LeftMargin;

            setBackCamera_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SETBACKCAMERA"));
            setBackCamera_CheckBox.tooltip = Translations.Translate("SETTINGS_SETBACKCAMERA_DETAIL");
            setBackCamera_CheckBox.isChecked = ModSettings.SetBackCamera;
            setBackCamera_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.SetBackCamera = isChecked;
            currentY += setBackCamera_CheckBox.height + LeftMargin;

            showInfoPanel_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SHOWINFOPANEL"));
            showInfoPanel_CheckBox.isChecked = ModSettings.ShowInfoPanel;
            showInfoPanel_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.ShowInfoPanel = isChecked;
            currentY += showInfoPanel_CheckBox.height + LeftMargin;

            heightScale_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_INFOPANELHEIGHTSCALE"), .5f, 2f, .1f, ModSettings.InfoPanelHeightScale);
            heightScale_Slider.eventValueChanged += (_, value) => ModSettings.InfoPanelHeightScale = value;
            currentY += heightScale_Slider.height + SliderMargin;

            speedUnit_DropDown = UIDropDowns.AddPlainDropDown(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SPEEDUNIT"),
                Enum.GetValues(typeof(ModSettings.SpeedUnits))
               .Cast<ModSettings.SpeedUnits>()
               .Select(speedUnit => speedUnit.ToSpeedUnitString())
               .ToArray(),
                (int)ModSettings.SpeedUnit);
            speedUnit_DropDown.eventSelectedIndexChanged += (_, index) => ModSettings.SpeedUnit = (ModSettings.SpeedUnits)index;
            speedUnit_DropDown.parent.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += speedUnit_DropDown.parent.height + LeftMargin;

            fade_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_FADE"));
            fade_CheckBox.isChecked = ModSettings.Fade;
            fade_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.Fade = isChecked;
            currentY += fade_CheckBox.height + LeftMargin;

            UISpacers.AddTitleSpacer(scrollPanel, LeftMargin, currentY, headerWidth, Translations.Translate("SETTINGS_GROUPNAME_OPT"));
            currentY += TitleMargin;

            string[] LodOpt_Items = new[]
            {
                Translations.Translate("DISABLED"),
                Translations.Translate("LOW"),
                Translations.Translate("MID"),
                Translations.Translate("HIGH")
            };

            LodOpt_DropDown = UIDropDowns.AddPlainDropDown(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_LODOPT"), LodOpt_Items, ModSettings.LodOpt, 300);
            LodOpt_DropDown.tooltip = string.Format(Translations.Translate("SETTINGS_LODOPT_DETAIL"), "\n");
            LodOpt_DropDown.eventSelectedIndexChanged += (_, value) => ModSettings.LodOpt = value;
            LodOpt_DropDown.parent.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += LodOpt_DropDown.parent.height + Margin;

            ShadowsOpt_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SHADOWSOPT"));
            ShadowsOpt_CheckBox.tooltip = Translations.Translate("SETTINGS_SHADOWSOPT_DETAIL");
            ShadowsOpt_CheckBox.isChecked = ModSettings.ShadowsOpt;
            ShadowsOpt_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.ShadowsOpt = isChecked;
            currentY += ShadowsOpt_CheckBox.height + LeftMargin;

            // Reset to defaults.
            defaults_Button = UIButtons.AddButton(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_RESETBTN"), 200f, 40f);
            defaults_Button.eventClicked += (c, _) => Reset();

            var importButton = UIButtons.AddButton(scrollPanel, LeftMargin + 200f + LeftMargin, currentY, Translations.Translate("SETTINGS_IMPORT"), 200f, 40f);
            importButton.eventClicked += (c, _) =>
            {
                var notification = NotificationBase.ShowNotification<YesNoNotification>();
                notification.AddParas(Translations.Translate("SETTINGS_IMPORTCONFIRM"));
                notification.YesButton.eventClicked += (y, m) =>
                {
                    try
                    {
                        v2ModSettings.Load();
                        ModSettings.Save();
                        OptionsPanelManager<OptionsPanel>.LocaleChanged();
                        MainPanel.Instance?.LocaleChanged();

                        OffsetsSettings.Load();
                        v2OffsetsSettings.Load();
                        OffsetsSettings.Save();

                        var successedNotification = NotificationBase.ShowNotification<ListNotification>();
                        successedNotification.AddParas(Translations.Translate("SETTINGS_IMPORTSUCCESSED"));
                    }
                    catch (Exception e)
                    {
                        var failedNotification = NotificationBase.ShowNotification<ListNotification>();
                        failedNotification.AddParas(Translations.Translate("ERROR"));
                        failedNotification.AddSpacer();
                        failedNotification.AddParas(e.ToString());
                        Logging.LogException(e, "Failed to import FPSCamera v2 settings");
                    }
                };
            };
        }
        /// <summary>
        /// <see cref="Reset()"/> for default button in <see cref="GeneralOptions"/>.
        /// default values are in <seealso cref="ModSettings"/>.
        /// </summary>
        internal static void Reset()
        {
            ModSettings.ResetToDefaults();
            ModSettings.Save();
            OptionsPanelManager<OptionsPanel>.LocaleChanged();
            MainPanel.Instance?.LocaleChanged();
        }
    }
}
