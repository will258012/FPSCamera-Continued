using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using UnityEngine;
using static AlgernonCommons.UI.UISliders;

namespace FPSCamera.UI
{
    public class OffsetSliders
    {
        public UISlider x_Slider;
        public UISlider y_Slider;
        public UISlider z_Slider;
        public UIPanel slidersPanel;
        private const float Margin = 5f;
        private const float SliderMargin = 60f;

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

            var z_Slider = AddPlainSliderWithValue(slidersLabel, 0f, currentY, Translations.Translate("SETTINGS_KEYMOVEFORWARD"), min, max, step, defaultValue.z, format, width);
            currentY += z_Slider.height + SliderMargin;

            var y_Slider = AddPlainSliderWithValue(slidersLabel, 0f, currentY, Translations.Translate("SETTINGS_KEYMOVEUP"), min, max, step, defaultValue.y, format, width);
            currentY += y_Slider.height + SliderMargin;

            var x_Slider = AddPlainSliderWithValue(slidersLabel, 0f, currentY, Translations.Translate("SETTINGS_KEYMOVELEFT"), min, max, step, defaultValue.x, format, width);
            currentY += x_Slider.height + SliderMargin;

            slidersPanel.height = currentY;

            return new OffsetSliders(x_Slider, y_Slider, z_Slider, slidersPanel);
        }
    }
}
