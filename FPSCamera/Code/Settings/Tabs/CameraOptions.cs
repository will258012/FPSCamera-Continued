﻿using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using FPSCamera.UI;
using UnityEngine;

namespace FPSCamera.Settings.Tabs
{
    public sealed class CameraOptions
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float LeftMargin = 24f;
        private const float GroupMargin = 40f;
        private const float TitleMargin = 50f;
        private const float SliderMargin = 60f;

        private static UICheckBox dof_CheckBox;
        private static UICheckBox invertRotateHorizontal_CheckBox;
        private static UICheckBox invertRotateVertical_CheckBox;
        private static UISlider speedUpFactor_Slider;
        private static UISlider rotateSensitivity_Slider;
        private static UISlider rotateKeyFactor_Slider;
        private static UISlider maxPitchDeg_Slider;
        private static UISlider fov_Slider;
        private static UISlider nearClipPlane_Slider;
        private static UISlider foViewScrollfactor_Slider;
        private static UICheckBox pathsDetection_CheckBox;
        private static UICheckBox tracksDetection_CheckBox;

        private static UISlider movementSpeed_Slider;
        private static UICheckBox showCursorFree_CheckBox;
        private static UIDropDown groundCliping_dropDown;
        private static UISlider groundLevelOffset_Slider;
        private static UISlider roadLevelOffset_Slider;

        private static UISlider offsetMovementSpeed_Slider;
        private static UICheckBox showCursorFollow_CheckBox;
        private static UICheckBox stickToFrontVehicle_CheckBox;
        private static OffsetSliders FollowCamOffset;
        private static OffsetSliders VehicleFixedOffset;
        private static OffsetSliders MidVehFixedOffset;
        private static OffsetSliders PedestrianFixedOffset;

        private static UISlider periodWalk_Slider;
        private static UICheckBox manualSwitchWalk_CheckBox;
        private static UICheckBox selectPedestrian_CheckBox;
        private static UICheckBox selectPassenger_CheckBox;
        private static UICheckBox selectWaiting_CheckBox;
        private static UICheckBox selectDriving_CheckBox;
        private static UICheckBox selectPublicTransit_CheckBox;
        private static UICheckBox selectService_CheckBox;
        private static UICheckBox selectCargo_CheckBox;

        private float currentY = GroupMargin;
        private UITabstrip tabStrip;


        /// <summary>
        /// Initializes a new instance of the <see cref="CameraOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal CameraOptions(UITabstrip tabStrip, int tabIndex)
        {

            var panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("SETTINGS_GROUPNAME_CAM"), tabIndex, out var _, autoLayout: false);
            this.tabStrip = AutoTabstrip.AddTabstrip(panel, 0f, 0f, panel.width, panel.height, out _, tabHeight: 40f);
            CameraControls(0);
            FreeMode(1);
            FollowMode(2);
            WalkThruMode(3);

            // Select first tab.
            this.tabStrip.selectedIndex = -1;
            this.tabStrip.selectedIndex = 0;
        }
        private void CameraControls(int tabIndex)
        {
            #region Camera Controls
            var panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("SETTINGS_GROUPNAME_CAMCONTROL"), tabIndex, out var _, autoLayout: false);
            currentY = GroupMargin;

            //Add Scrollbar. 
            var scrollPanel = panel.AddUIComponent<UIScrollablePanel>();
            scrollPanel.relativePosition = new Vector2(0, Margin);
            scrollPanel.autoSize = false;
            scrollPanel.autoLayout = false;
            scrollPanel.width = panel.width - 15f;
            scrollPanel.height = panel.height - 15f;
            scrollPanel.clipChildren = true;
            scrollPanel.builtinKeyNavigation = true;
            scrollPanel.scrollWheelDirection = UIOrientation.Vertical;
            //Fix occasional content offset issue when switching pages (Why?)
            scrollPanel.eventVisibilityChanged += (_, isShow) => { if (isShow) scrollPanel.Reset(); };
            UIScrollbars.AddScrollbar(panel, scrollPanel);

            dof_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_ENABLEDOF"));
            dof_CheckBox.isChecked = ModSettings.Dof;
            dof_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.Dof = isChecked;
            currentY += dof_CheckBox.height + Margin;


            invertRotateHorizontal_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_INVERTROTATEHORIZONTAL"));
            invertRotateHorizontal_CheckBox.isChecked = ModSettings.InvertRotateHorizontal;
            invertRotateHorizontal_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.InvertRotateHorizontal = isChecked;
            currentY += invertRotateHorizontal_CheckBox.height + Margin;


            invertRotateVertical_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_INVERTROTATEVERTICAL"));
            invertRotateVertical_CheckBox.isChecked = ModSettings.InvertRotateVertical;
            invertRotateVertical_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.InvertRotateVertical = isChecked;
            currentY += invertRotateVertical_CheckBox.height + Margin;

            speedUpFactor_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SPEEDUPFACTOR"), 1.25f, 10f, .25f, ModSettings.SpeedUpFactor);
            speedUpFactor_Slider.eventValueChanged += (_, value) => ModSettings.SpeedUpFactor = value;
            currentY += speedUpFactor_Slider.height + SliderMargin;

            rotateSensitivity_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_ROTATESENSITIVITY"), .5f, 10f, .5f, ModSettings.RotateSensitivity);
            rotateSensitivity_Slider.eventValueChanged += (_, value) => ModSettings.RotateSensitivity = value;
            currentY += rotateSensitivity_Slider.height + SliderMargin;

            rotateKeyFactor_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_ROTATEKEYFACTOR"), .5f, 32f, .5f, ModSettings.RotateKeyFactor);
            rotateKeyFactor_Slider.eventValueChanged += (_, value) => ModSettings.RotateKeyFactor = value;
            currentY += rotateKeyFactor_Slider.height + SliderMargin;

            maxPitchDeg_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_MAXPITSHDEG"), 10f, 89f, 1f, ModSettings.MaxPitchDeg, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1f, numberFormat: "N0", suffix: "°"));
            maxPitchDeg_Slider.tooltip = Translations.Translate("SETTINGS_MAXPITSHDEG_DETAIL");
            maxPitchDeg_Slider.eventValueChanged += (_, value) => ModSettings.MaxPitchDeg = value;
            currentY += maxPitchDeg_Slider.height + SliderMargin;


            fov_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_FIELDOFVIEW"), 10f, 75f, 1f, ModSettings.CamFieldOfView, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1f, numberFormat: "N0", suffix: "°"));
            fov_Slider.eventValueChanged += (_, value) => ModSettings.CamFieldOfView = value;
            currentY += fov_Slider.height + SliderMargin;

            nearClipPlane_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_CAMNEARCLIPPLANE"), .1f, 32f, .1f, ModSettings.CamNearClipPlane);
            nearClipPlane_Slider.eventValueChanged += (_, value) => ModSettings.CamNearClipPlane = value;
            currentY += nearClipPlane_Slider.height + SliderMargin;

            foViewScrollfactor_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_FOVIEWSCROLLFACTOR"), 1.01f, 2f, .01f, ModSettings.FoViewScrollfactor);
            foViewScrollfactor_Slider.tooltip = Translations.Translate("SETTINGS_FOVIEWSCROLLFACTOR_DETAIL");
            foViewScrollfactor_Slider.eventValueChanged += (_, value) => ModSettings.FoViewScrollfactor = value;
            currentY += foViewScrollfactor_Slider.height + SliderMargin;
            #endregion
        }
        private void FreeMode(int tabIndex)
        {
            #region Free-Camera Mode Options
            var panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("SETTINGS_GROUPNAME_FREECAM"), tabIndex, out var _, autoLayout: false);
            currentY = GroupMargin;

            var scrollPanel = panel.AddUIComponent<UIScrollablePanel>();
            scrollPanel.relativePosition = new Vector2(0, Margin);
            scrollPanel.autoSize = false;
            scrollPanel.autoLayout = false;
            scrollPanel.width = panel.width - 15f;
            scrollPanel.height = panel.height - 15f;
            scrollPanel.clipChildren = true;
            scrollPanel.builtinKeyNavigation = true;
            scrollPanel.scrollWheelDirection = UIOrientation.Vertical;
            scrollPanel.eventVisibilityChanged += (_, isShow) => { if (isShow) scrollPanel.Reset(); };
            UIScrollbars.AddScrollbar(panel, scrollPanel);

            showCursorFree_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SHOWCURSORFREE"));
            showCursorFree_CheckBox.isChecked = ModSettings.ShowCursorFree;
            showCursorFree_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.ShowCursorFree = isChecked;
            currentY += showCursorFree_CheckBox.height + Margin;

            movementSpeed_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_MOVEMENTSPEED"), 1f, 60f, .5f, ModSettings.MovementSpeed, new UISliders.SliderValueFormat(1, 0.5f, "N", "km/h"));
            movementSpeed_Slider.eventValueChanged += (_, value) => ModSettings.MovementSpeed = value;
            currentY += movementSpeed_Slider.height + SliderMargin;

            string[] groundClippingItems = new[]
            {
                Translations.Translate("SETTINGS_GROUNDCLIPING_NONE"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_ABOVE_GROUND"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_SNAP_TO_GROUND"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_ABOVE_ROAD"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_SNAP_TO_ROAD")
            };

            groundCliping_dropDown = UIDropDowns.AddPlainDropDown(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_GROUNDCLIPING"), groundClippingItems, (int)ModSettings.GroundClipping, 300);
            groundCliping_dropDown.tooltip = Translations.Translate("SETTINGS_GROUNDCLIPING_DETAIL");
            groundCliping_dropDown.eventSelectedIndexChanged += (_, index) => ModSettings.GroundClipping = (ModSettings.GroundClippings)index;
            groundCliping_dropDown.parent.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += groundCliping_dropDown.parent.height + Margin;

            groundLevelOffset_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_GROUNDLEVELOFFSET"), -5f, 10f, .5f, ModSettings.GroundLevelOffset);
            groundLevelOffset_Slider.tooltip = Translations.Translate("SETTINGS_GROUNDLEVELOFFFSET_DETAIL");
            groundLevelOffset_Slider.eventValueChanged += (_, value) => ModSettings.GroundLevelOffset = value;
            currentY += groundLevelOffset_Slider.height + SliderMargin;

            roadLevelOffset_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_ROADLEVELOFFSET"), -5f, 10f, .5f, ModSettings.RoadLevelOffset);
            roadLevelOffset_Slider.tooltip = Translations.Translate("SETTINGS_ROADLEVELOFFSET_DETAIL");
            roadLevelOffset_Slider.eventValueChanged += (_, value) => ModSettings.RoadLevelOffset = value;
            currentY += roadLevelOffset_Slider.height + SliderMargin;

            pathsDetection_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_PATHS_DETECTION"));
            pathsDetection_CheckBox.tooltip = Translations.Translate("SETTINGS_DETECTIONS_DETAIL");
            pathsDetection_CheckBox.isChecked = ModSettings.PathsDetection;
            pathsDetection_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.PathsDetection = isChecked;
            currentY += pathsDetection_CheckBox.height + Margin;

            tracksDetection_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_TRACKS_DETECTION"));
            tracksDetection_CheckBox.tooltip = Translations.Translate("SETTINGS_DETECTIONS_DETAIL");
            tracksDetection_CheckBox.isChecked = ModSettings.TracksDetection;
            tracksDetection_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.TracksDetection = isChecked;
            currentY += tracksDetection_CheckBox.height + Margin;
            #endregion
        }
        private void FollowMode(int tabIndex)
        {
            #region Follow Mode Options
            var panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("SETTINGS_GROUPNAME_FOLLOW"), tabIndex, out var _, autoLayout: false);
            currentY = GroupMargin;

            var scrollPanel = panel.AddUIComponent<UIScrollablePanel>();
            scrollPanel.relativePosition = new Vector2(0, Margin);
            scrollPanel.autoSize = false;
            scrollPanel.autoLayout = false;
            scrollPanel.width = panel.width - 15f;
            scrollPanel.height = panel.height - 15f;
            scrollPanel.clipChildren = true;
            scrollPanel.builtinKeyNavigation = true;
            scrollPanel.scrollWheelDirection = UIOrientation.Vertical;
            scrollPanel.eventVisibilityChanged += (_, isShow) => { if (isShow) scrollPanel.Reset(); };
            UIScrollbars.AddScrollbar(panel, scrollPanel);

            showCursorFollow_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SHOWCURSORFOLLOW"));
            showCursorFollow_CheckBox.isChecked = ModSettings.ShowCursorFollow;
            showCursorFollow_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.ShowCursorFollow = isChecked;
            currentY += showCursorFollow_CheckBox.height + Margin;

            stickToFrontVehicle_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_STICKTOFRONTVEHICLE"));
            stickToFrontVehicle_CheckBox.isChecked = ModSettings.StickToFrontVehicle;
            stickToFrontVehicle_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.StickToFrontVehicle = isChecked;
            currentY += stickToFrontVehicle_CheckBox.height + Margin;

            offsetMovementSpeed_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_OFFSETMOVEMENTSPEED"), 1f, 60f, .5f, ModSettings.OffsetMovementSpeed, new UISliders.SliderValueFormat(1, 0.5f, "N", "km/h"));
            offsetMovementSpeed_Slider.eventValueChanged += (_, value) => ModSettings.OffsetMovementSpeed = value;
            currentY += offsetMovementSpeed_Slider.height + SliderMargin;

            FollowCamOffset = OffsetSliders.AddOffsetSlidersWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_FOLLOWCAMOFFSET"), -20f, 20f, .1f, ModSettings.FollowCamOffset);
            FollowCamOffset.x_Slider.eventValueChanged += (_, value) => ModSettings.FollowCamOffset.x = value;
            FollowCamOffset.y_Slider.eventValueChanged += (_, value) => ModSettings.FollowCamOffset.y = value;
            FollowCamOffset.z_Slider.eventValueChanged += (_, value) => ModSettings.FollowCamOffset.z = value;
            currentY += FollowCamOffset.slidersPanel.height;

            VehicleFixedOffset = OffsetSliders.AddOffsetSlidersWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_VEHICLEFIXEDOFFERT"), -20f, 20f, .1f, ModSettings.VehicleFixedOffset);
            VehicleFixedOffset.x_Slider.eventValueChanged += (_, value) => ModSettings.VehicleFixedOffset.x = value;
            VehicleFixedOffset.y_Slider.eventValueChanged += (_, value) => ModSettings.VehicleFixedOffset.y = value;
            VehicleFixedOffset.z_Slider.eventValueChanged += (_, value) => ModSettings.VehicleFixedOffset.z = value;
            currentY += VehicleFixedOffset.slidersPanel.height;

            MidVehFixedOffset = OffsetSliders.AddOffsetSlidersWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_MIDVEHFIXEDOFFSET"), -20f, 20f, .1f, ModSettings.MidVehFixedOffset);
            MidVehFixedOffset.x_Slider.eventValueChanged += (_, value) => ModSettings.MidVehFixedOffset.x = value;
            MidVehFixedOffset.y_Slider.eventValueChanged += (_, value) => ModSettings.MidVehFixedOffset.y = value;
            MidVehFixedOffset.z_Slider.eventValueChanged += (_, value) => ModSettings.MidVehFixedOffset.z = value;
            currentY += MidVehFixedOffset.slidersPanel.height;

            PedestrianFixedOffset = OffsetSliders.AddOffsetSlidersWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_PEDSTRIANFIXEDOFFSET"), -20f, 20f, .1f, ModSettings.PedestrianFixedOffset);
            PedestrianFixedOffset.x_Slider.eventValueChanged += (_, value) => ModSettings.PedestrianFixedOffset.x = value;
            PedestrianFixedOffset.y_Slider.eventValueChanged += (_, value) => ModSettings.PedestrianFixedOffset.y = value;
            PedestrianFixedOffset.z_Slider.eventValueChanged += (_, value) => ModSettings.PedestrianFixedOffset.z = value;
            currentY += PedestrianFixedOffset.slidersPanel.height;
            #endregion
        }
        private void WalkThruMode(int tabIndex)
        {
            #region Walk-Through Mode Options
            var panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("SETTINGS_GROUPNAME_WALKTHRU"), tabIndex, out var _, autoLayout: false);
            currentY = GroupMargin;

            var scrollPanel = panel.AddUIComponent<UIScrollablePanel>();
            scrollPanel.relativePosition = new Vector2(0, Margin);
            scrollPanel.autoSize = false;
            scrollPanel.autoLayout = false;
            scrollPanel.width = panel.width - 15f;
            scrollPanel.height = panel.height - 15f;
            scrollPanel.clipChildren = true;
            scrollPanel.builtinKeyNavigation = true;
            scrollPanel.scrollWheelDirection = UIOrientation.Vertical;
            scrollPanel.eventVisibilityChanged += (_, isShow) => { if (isShow) scrollPanel.Reset(); };
            UIScrollbars.AddScrollbar(panel, scrollPanel);

            periodWalk_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_PERIODWALK"), 5f, 200f, 1f, ModSettings.PeriodWalk, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1f, numberFormat: "N0", suffix: "s"));
            periodWalk_Slider.eventValueChanged += (_, value) => ModSettings.PeriodWalk = value;
            currentY += periodWalk_Slider.height + SliderMargin;

            manualSwitchWalk_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_MANUALSWITCHWALK"));
            manualSwitchWalk_CheckBox.tooltip = Translations.Translate("SETTINGS_MANUALSWITCHWALK_DETAIL");
            manualSwitchWalk_CheckBox.isChecked = ModSettings.ManualSwitchWalk;
            manualSwitchWalk_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.ManualSwitchWalk = isChecked;
            currentY += manualSwitchWalk_CheckBox.height + Margin;

            UISpacers.AddTitle(scrollPanel, Margin, currentY, Translations.Translate("SETTINGS_TARGETSTOFOLLOW"));
            currentY += TitleMargin;

            selectPedestrian_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SELECTPEDESTRIAN"));
            selectPedestrian_CheckBox.isChecked = ModSettings.SelectPedestrian;
            selectPedestrian_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.SelectPedestrian = isChecked;
            currentY += selectPedestrian_CheckBox.height + Margin;

            selectPassenger_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SELECTPASSENGER"));
            selectPassenger_CheckBox.isChecked = ModSettings.SelectPassenger;
            selectPassenger_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.SelectPassenger = isChecked;
            currentY += selectPassenger_CheckBox.height + Margin;

            selectWaiting_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SELECTWAITING"));
            selectWaiting_CheckBox.isChecked = ModSettings.SelectWaiting;
            selectWaiting_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.SelectWaiting = isChecked;
            currentY += selectWaiting_CheckBox.height + Margin;

            selectDriving_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SELECTDRIVING"));
            selectDriving_CheckBox.isChecked = ModSettings.SelectDriving;
            selectDriving_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.SelectDriving = isChecked;
            currentY += selectDriving_CheckBox.height + Margin;

            selectPublicTransit_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SELECTPUBLICTRANSIT"));
            selectPublicTransit_CheckBox.isChecked = ModSettings.SelectPublicTransit;
            selectPublicTransit_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.SelectPublicTransit = isChecked;
            currentY += selectPublicTransit_CheckBox.height + Margin;

            selectService_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SELECTSERVICE"));
            selectService_CheckBox.isChecked = ModSettings.SelectService;
            selectService_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.SelectService = isChecked;
            currentY += selectService_CheckBox.height + Margin;

            selectCargo_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SELECTCARGO"));
            selectCargo_CheckBox.isChecked = ModSettings.SelectCargo;
            selectCargo_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.SelectCargo = isChecked;
            currentY += selectCargo_CheckBox.height + Margin;

            #endregion
        }
    }
}