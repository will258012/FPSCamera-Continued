﻿using AlgernonCommons.UI;
using FPSCamera.Settings.Tabs;
namespace FPSCamera.Settings
{
    public class OptionsPanel : OptionsPanelBase
    {
        protected override void Setup()
        {
            var tabStrip = AutoTabstrip.AddTabstrip(this, 0f, 0f, OptionsPanelManager<OptionsPanel>.PanelWidth, OptionsPanelManager<OptionsPanel>.PanelHeight, out _, tabHeight: 30f);

            _ = new GeneralOptions(tabStrip, 0);
            _ = new CameraOptions(tabStrip, 1);
            _ = new HotKeyOptions(tabStrip, 2);

            // Select first tab.
            tabStrip.selectedIndex = -1;
            tabStrip.selectedIndex = 0;
        }
    }
}
