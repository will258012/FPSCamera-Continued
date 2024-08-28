using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using UnityEngine;
using static AlgernonCommons.UI.UISliders;

namespace FPSCamera.UI
{
    internal class OffsetSliders
    {
        internal UISlider x_Slider;
        internal UISlider y_Slider;
        internal UISlider z_Slider;
        internal UIPanel slidersPanel;
        const float Margin = 5f;

        private OffsetSliders(UISlider x_Slider, UISlider y_Slider, UISlider z_Slider, UIPanel slidersPanel)
        {
            this.x_Slider = x_Slider;
            this.y_Slider = y_Slider;
            this.z_Slider = z_Slider;
            this.slidersPanel = slidersPanel;
        }
        internal static OffsetSliders AddOffsetSlidersWithValue(UIComponent parent, float xPos, float yPos, string text, float min, float max, float step, Vector3 defaultValue, float width = 600f) =>
            AddOffsetSlidersWithValue(parent, xPos, yPos, text, min, max, step, defaultValue, new SliderValueFormat(valueMultiplier: 1, roundToNearest: step, numberFormat: "N", suffix: null), width);

        internal static OffsetSliders AddOffsetSlidersWithValue(UIComponent parent, float xPos, float yPos, string text, float min, float max, float step, Vector3 defaultValue, SliderValueFormat format, float width = 600f)
        {
            // Slider panel configuration
            var slidersPanel = parent.AddUIComponent<UIPanel>();
            slidersPanel.relativePosition = new Vector2(xPos, yPos);
            slidersPanel.width = width;

            float currentY = 0f;
            // Sliders label configuration
            var slidersLabel = slidersPanel.AddUIComponent<UILabel>();
            slidersLabel.autoHeight = true;
            slidersLabel.width = width;
            slidersLabel.textScale = 1.2f;
            slidersLabel.font = UIFonts.SemiBold;
            slidersLabel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            slidersLabel.relativePosition = new Vector3(0f, currentY);
            slidersLabel.text = text;

            currentY += slidersLabel.height + Margin;
            var z_sliderPanel = slidersPanel.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsSliderTemplate")) as UIPanel;
            SetupSliderPanel(z_sliderPanel, Translations.Translate("SETTINGS_KEYMOVEFORWARD"), min, max, step, defaultValue.z, format, width, ref currentY, out var z_Slider);

            var y_sliderPanel = slidersPanel.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsSliderTemplate")) as UIPanel;
            SetupSliderPanel(y_sliderPanel, Translations.Translate("SETTINGS_KEYMOVEUP"), min, max, step, defaultValue.y, format, width, ref currentY, out var y_Slider);

            var x_sliderPanel = slidersPanel.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsSliderTemplate")) as UIPanel;
            SetupSliderPanel(x_sliderPanel, Translations.Translate("SETTINGS_KEYMOVELEFT"), min, max, step, defaultValue.x, format, width, ref currentY, out var x_Slider);

            slidersPanel.height = currentY;

            return new OffsetSliders(x_Slider, y_Slider, z_Slider, slidersPanel);
        }

        private static void SetupSliderPanel(UIPanel sliderPanel, string text, float min, float max, float step, float defaultValue, SliderValueFormat format, float width, ref float currentY, out UISlider slider)
        {
            // Panel layout
            sliderPanel.autoLayout = false;
            sliderPanel.autoSize = false;
            sliderPanel.width = width;
            sliderPanel.height = 65f;

            // Slider label configuration
            var sliderLabel = sliderPanel.Find<UILabel>("Label");
            sliderLabel.autoHeight = true;
            sliderLabel.width = width;
            sliderLabel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            sliderLabel.relativePosition = Vector2.zero;
            sliderLabel.text = text;

            // Slider configuration
            slider = sliderPanel.Find<UISlider>("Slider");
            slider.minValue = min;
            slider.maxValue = max;
            slider.stepSize = step;
            slider.value = defaultValue;

            // Move default slider position to match resized label
            slider.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            slider.relativePosition = UILayout.PositionUnder(sliderLabel);
            slider.width = width;

            sliderPanel.relativePosition = new Vector2(0f, currentY);
            currentY += sliderPanel.height + Margin;

            // Value label
            var valueLabel = sliderPanel.AddUIComponent<UILabel>();
            valueLabel.name = "ValueLabel";
            valueLabel.relativePosition = UILayout.PositionRightOf(slider);

            // Set initial value and event handler to update on value change
            valueLabel.text = format.FormatValue(slider.value);
            slider.eventValueChanged += (c, value) => valueLabel.text = format.FormatValue(value);
        }


    }
}
