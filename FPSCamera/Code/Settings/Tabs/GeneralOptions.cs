using AlgernonCommons;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using UnityEngine;

namespace FPSCamera.Settings.Tabs
{

    internal class GeneralOptions
    {
        // Layout constants.
        private const float LeftMargin = 24f;
        private const float GroupMargin = 40f;
        private const float SliderMargin = 60f;

        private readonly UIDropDown language_DropDown;
        private readonly UICheckBox logging_CheckBox;
        private readonly UICheckBox hideUI_CheckBox;
        private readonly UICheckBox setBackCamera_CheckBox;
        private readonly UICheckBox metricUnit_CheckBox;
        private readonly UICheckBox showInfoPanel_CheckBox;
        private readonly UISlider heightScale_Slider;
        private readonly UIButton defaults_Button;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal GeneralOptions(UITabstrip tabStrip, int tabIndex)
        {

            var panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("SETTINGS_GROUPNAME_GENERAL"), tabIndex, out var _, autoLayout: false);

            var currentY = GroupMargin;

            language_DropDown = UIDropDowns.AddPlainDropDown(panel, LeftMargin, currentY, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
            language_DropDown.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            };
            language_DropDown.parent.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += language_DropDown.parent.height + GroupMargin;

            logging_CheckBox = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("DETAIL_LOGGING"));
            logging_CheckBox.isChecked = Logging.DetailLogging;
            logging_CheckBox.eventCheckChanged += (_, isChecked) => Logging.DetailLogging = isChecked;
            currentY += logging_CheckBox.height + GroupMargin;

            hideUI_CheckBox = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("SETTINGS_HIDEUI"));
            hideUI_CheckBox.isChecked = ModSettings.HideGameUI;
            hideUI_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.HideGameUI = isChecked;
            currentY += hideUI_CheckBox.height + GroupMargin;

            setBackCamera_CheckBox = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("SETTINGS_SETBACKCAMERA"));
            setBackCamera_CheckBox.tooltip = Translations.Translate("SETTINGS_SETBACKCAMERA_DETAIL");
            setBackCamera_CheckBox.isChecked = ModSettings.SetBackCamera;
            setBackCamera_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.SetBackCamera = isChecked;
            currentY += setBackCamera_CheckBox.height + GroupMargin;

            showInfoPanel_CheckBox = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("SETTINGS_SHOWINFOPANEL"));
            showInfoPanel_CheckBox.isChecked = ModSettings.ShowInfoPanel;
            showInfoPanel_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.ShowInfoPanel = isChecked;
            currentY += showInfoPanel_CheckBox.height + GroupMargin;

            heightScale_Slider = UISliders.AddPlainSliderWithValue(panel, LeftMargin, currentY, Translations.Translate("SETTINGS_INFOPANELHEIGHTSCALE"), .5f, 2f, .1f, ModSettings.InfoPanelHeightScale);
            heightScale_Slider.eventValueChanged += (_, value) => ModSettings.InfoPanelHeightScale = value;
            currentY += heightScale_Slider.height + SliderMargin;

            metricUnit_CheckBox = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("SETTINGS_USEMETRICUNIT"));
            metricUnit_CheckBox.isChecked = ModSettings.UseMetricUnit;
            metricUnit_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.UseMetricUnit = isChecked;
            currentY += metricUnit_CheckBox.height + GroupMargin;

            // Reset to defaults.
            defaults_Button = UIButtons.AddButton(panel, LeftMargin, currentY, Translations.Translate("SETTINGS_RESETBTN"), 200f, 40f);
            defaults_Button.eventClicked += (c, _) => Reset();
        }
        /// <summary>
        /// <see cref="Reset()"/> for default button in <see cref="GeneralOptions"/>.
        /// default values are in <seealso cref="ModSettings"/>.
        /// </summary>
        internal void Reset()
        {
            logging_CheckBox.isChecked = false;
            hideUI_CheckBox.isChecked = setBackCamera_CheckBox.isChecked =
                metricUnit_CheckBox.isChecked = showInfoPanel_CheckBox.isChecked = true;
            heightScale_Slider.value = 1f;
            CameraOptions.Reset();
            HotKeyOptions.Reset();
            //Put language updates last
            language_DropDown.selectedIndex = 0;
        }
    }
}
