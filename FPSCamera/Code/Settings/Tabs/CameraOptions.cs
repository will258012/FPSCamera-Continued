using AlgernonCommons.Translation;
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
        private static UISlider movementSpeed_Slider;
        private static UISlider speedUpFactor_Slider;
        private static UISlider rotateSensitivity_Slider;
        private static UISlider rotateKeyFactor_Slider;
        private static UISlider maxPitchDeg_Slider;
        private static UISlider fov_Slider;
        private static UISlider nearClipPlane_Slider;
        private static UISlider foViewScrollfactor_Slider;


        private static UICheckBox showCursorFree_CheckBox;
        private static UIDropDown groundCliping_dropDown;
        private static UISlider groundLevelOffset_Slider;
        private static UISlider roadLevelOffset_Slider;

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
        private static UICheckBox smoothTransition_CheckBox;
        private static UISlider transSpeed_Slider;
        private static UISlider minTransDistance_Slider;
        private static UISlider maxTransDistance_Slider;





        /// <summary>
        /// Initializes a new instance of the <see cref="CameraOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal CameraOptions(UITabstrip tabStrip, int tabIndex)
        {

            var panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("SETTINGS_GROUPNAME_CAM"), tabIndex, out var _, autoLayout: false);
            var currentY = GroupMargin;
            var headerWidth = OptionsPanelManager<OptionsPanel>.PanelWidth - (Margin * 2f);

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

            #region Camera Controls
            UISpacers.AddTitle(scrollPanel, Margin, currentY, Translations.Translate("SETTINGS_GROUPNAME_CAMCONTROL"));
            currentY += TitleMargin;

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


            movementSpeed_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_MOVEMENTSPEED"), 1f, 60f, .5f, ModSettings.MovementSpeed);
            movementSpeed_Slider.eventValueChanged += (_, value) => ModSettings.MovementSpeed = value;
            currentY += movementSpeed_Slider.height + SliderMargin;

            speedUpFactor_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SPEEDUPFACTOR"), 1.25f, 10f, .25f, ModSettings.SpeedUpFactor);
            speedUpFactor_Slider.eventValueChanged += (_, value) => ModSettings.SpeedUpFactor = value;
            currentY += speedUpFactor_Slider.height + SliderMargin;

            rotateSensitivity_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_ROTATESENSITIVITY"), .25f, 10f, .5f, ModSettings.RotateSensitivity);
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

            nearClipPlane_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_CAMNEARCLIPPLANE"), .125f, 32f, .5f, ModSettings.CamNearClipPlane);
            nearClipPlane_Slider.eventValueChanged += (_, value) => ModSettings.CamNearClipPlane = value;
            currentY += nearClipPlane_Slider.height + SliderMargin;

            foViewScrollfactor_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_FOVIEWSCROLLFACTOR"), 1.01f, 2f, .01f, ModSettings.FoViewScrollfactor);
            foViewScrollfactor_Slider.tooltip = Translations.Translate("SETTINGS_FOVIEWSCROLLFACTOR_DETAIL");
            foViewScrollfactor_Slider.eventValueChanged += (_, value) => ModSettings.FoViewScrollfactor = value;
            currentY += foViewScrollfactor_Slider.height + SliderMargin;
            #endregion
            #region Free-Camera Mode Options

            UISpacers.AddTitleSpacer(scrollPanel, Margin, currentY, headerWidth, Translations.Translate("SETTINGS_GROUPNAME_FREECAM"));
            currentY += TitleMargin;

            showCursorFree_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SHOWCURSORFREE"));
            showCursorFree_CheckBox.isChecked = ModSettings.ShowCursorFree;
            showCursorFree_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.ShowCursorFree = isChecked;
            currentY += showCursorFree_CheckBox.height + Margin;

            string[] groundClipingItems = new[]
            {
                Translations.Translate("SETTINGS_GROUNDCLIPING_NONE"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_ABOVE_GROUND"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_SNAP_TO_GROUND"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_ABOVE_ROAD"),
                Translations.Translate("SETTINGS_GROUNDCLIPING_SNAP_TO_ROAD")
            };

            groundCliping_dropDown = UIDropDowns.AddPlainDropDown(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_GROUNDCLIPING"), groundClipingItems, ModSettings.GroundClipping, 300);
            groundCliping_dropDown.tooltip = string.Format(Translations.Translate("SETTINGS_GROUNDCLIPING_DETAIL"), "\n");
            groundCliping_dropDown.eventSelectedIndexChanged += (_, index) => ModSettings.GroundClipping = index;
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


            #endregion
            #region Follow Mode Options
            UISpacers.AddTitleSpacer(scrollPanel, Margin, currentY, headerWidth, Translations.Translate("SETTINGS_GROUPNAME_FOLLOW"));
            currentY += TitleMargin;

            showCursorFollow_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SHOWCURSORFOLLOW"));
            showCursorFollow_CheckBox.isChecked = ModSettings.ShowCursorFollow;
            showCursorFollow_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.ShowCursorFollow = isChecked;
            currentY += showCursorFollow_CheckBox.height + Margin;

            stickToFrontVehicle_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_STICKTOFRONTVEHICLE"));
            stickToFrontVehicle_CheckBox.isChecked = ModSettings.StickToFrontVehicle;
            stickToFrontVehicle_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.StickToFrontVehicle = isChecked;
            currentY += stickToFrontVehicle_CheckBox.height + Margin;

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
            #region Walk-Through Mode Options
            UISpacers.AddTitleSpacer(scrollPanel, Margin, currentY, headerWidth, Translations.Translate("SETTINGS_GROUPNAME_WALKTHRU"));
            currentY += TitleMargin;

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
            #region Smooth Transition Options
            UISpacers.AddTitleSpacer(scrollPanel, Margin, currentY, headerWidth, Translations.Translate("SETTINGS_GROUPNAME_SMOOTHTRANS"));
            currentY += TitleMargin;

            smoothTransition_CheckBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_SMOOTRANSITION"));
            smoothTransition_CheckBox.tooltip = string.Format(Translations.Translate("SETTINGS_SMOOTRANSITION_DETAIL"), "\n");
            smoothTransition_CheckBox.isChecked = ModSettings.SmoothTransition;
            smoothTransition_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.SmoothTransition = isChecked;
            currentY += smoothTransition_CheckBox.height + Margin;

            transSpeed_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_TRANSSPEED"), 1f, 20f, 1f, ModSettings.TransSpeed);
            transSpeed_Slider.eventValueChanged += (_, value) => ModSettings.TransSpeed = value;
            currentY += transSpeed_Slider.height + SliderMargin;

            minTransDistance_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_MINTRANSDISTANCE"), 5f, 50f, .1f, ModSettings.MinTransDistance, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: .1f, suffix: "m"));
            minTransDistance_Slider.tooltip = string.Format(Translations.Translate("SETTINGS_MINTRANSDISTANCE_DETAIL"), "\n");
            minTransDistance_Slider.eventValueChanged += (_, value) => ModSettings.MinTransDistance = value;
            currentY += minTransDistance_Slider.height + SliderMargin;

            maxTransDistance_Slider = UISliders.AddPlainSliderWithValue(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_MAXTRANSDISTANCE"), 100f, 2000f, 1f, ModSettings.MaxTransDistance, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1f, numberFormat: "N0", suffix: "m"));
            maxTransDistance_Slider.tooltip = string.Format(Translations.Translate("SETTINGS_MAXTRANSDISTANCE_DETAIL"), "\n");
            maxTransDistance_Slider.eventValueChanged += (_, value) => ModSettings.MaxTransDistance = value;
            currentY += maxTransDistance_Slider.height + SliderMargin;

            #endregion
        }



        /// <summary>
        /// <see cref="Reset()"/> for default button in <see cref="GeneralOptions"/>.
        /// default values are in <seealso cref="ModSettings"/>.
        /// </summary>
        internal static void Reset()
        {
            dof_CheckBox.isChecked = invertRotateHorizontal_CheckBox.isChecked = invertRotateVertical_CheckBox.isChecked = false;
            movementSpeed_Slider.value = 30f;
            speedUpFactor_Slider.value = 4f;
            rotateSensitivity_Slider.value = 5f;
            rotateKeyFactor_Slider.value = 8f;
            maxPitchDeg_Slider.value = 70f;
            fov_Slider.value = 45f;
            nearClipPlane_Slider.value = 1f;
            foViewScrollfactor_Slider.value = 1.2f;

            showCursorFree_CheckBox.isChecked = false;
            groundCliping_dropDown.selectedIndex = 0;
            groundLevelOffset_Slider.value = roadLevelOffset_Slider.value = 0f;

            showCursorFollow_CheckBox.isChecked = false;
            stickToFrontVehicle_CheckBox.isChecked = true;

            FollowCamOffset.x_Slider.value = FollowCamOffset.y_Slider.value = FollowCamOffset.z_Slider.value = 0f;

            VehicleFixedOffset.x_Slider.value = 0f;
            VehicleFixedOffset.y_Slider.value = 2f;
            VehicleFixedOffset.z_Slider.value = 3f;

            MidVehFixedOffset.x_Slider.value = 0f;
            MidVehFixedOffset.y_Slider.value = 3f;
            MidVehFixedOffset.z_Slider.value = -2f;

            PedestrianFixedOffset.x_Slider.value = 0f;
            PedestrianFixedOffset.y_Slider.value = 2f;
            PedestrianFixedOffset.z_Slider.value = 0f;


            periodWalk_Slider.value = 20f;
            manualSwitchWalk_CheckBox.isChecked = false;
            selectPedestrian_CheckBox.isChecked = selectPassenger_CheckBox.isChecked =
                selectWaiting_CheckBox.isChecked = selectDriving_CheckBox.isChecked =
                selectPublicTransit_CheckBox.isChecked = selectService_CheckBox.isChecked =
                selectCargo_CheckBox.isChecked = true;

            smoothTransition_CheckBox.isChecked = true;
            transSpeed_Slider.value = 10f;
            minTransDistance_Slider.value = 5f;
            maxTransDistance_Slider.value = 500f;
        }
    }
}