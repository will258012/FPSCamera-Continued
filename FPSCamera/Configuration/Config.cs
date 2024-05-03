namespace FPSCamera.Configuration
{
    using CSkyL.Config;
    using CSkyL.UI;
    using System;
    using UnityEngine;
    using CfFlag = CSkyL.Config.ConfigData<bool>;
    using CfKey = CSkyL.Config.ConfigData<UnityEngine.KeyCode>;
    using Ctransl = CSkyL.Translation.Translations;

    public class Config : Base
    {
        private const string defaultPath = "FPSCameraConfig.xml";
        public static readonly Config G = new Config();  // G: Global config

        public Config() : this(defaultPath) { }
        public Config(string filePath) : base(filePath) { }

        public static Config Load(string path = defaultPath) => Load<Config>(path);

        #region General Options
        [Config("Language", "LANGUAGE_CHOICE")]
        public readonly ConfigData<string> Language = new ConfigData<string>("default");

        [Config("HideGameUI", "SETTINGS_HIDEUI")]
        public readonly CfFlag HideGameUI = new CfFlag(false);

        [Config("SetBackCamera", "SETTINGS_SETBACKCAMERA", "SETTINGS_SETBACKCAMERA_DETAIL")]
        public readonly CfFlag SetBackCamera = new CfFlag(true);

        [Config("UseMetricUnit", "SETTINGS_USEMETRICUNIT")]
        public readonly CfFlag UseMetricUnit = new CfFlag(true);

        [Config("ShowInfoPanel", "SETTINGS_SHOWINFOPANEL")]
        public readonly CfFlag ShowInfoPanel = new CfFlag(true);

        [Config("InfoPanelHeightScale", "SETTINGS_INFOPANELHEIGHTSCALE")]
        public readonly CfFloat InfoPanelHeightScale = new CfFloat(1f, min: .5f, max: 2f);

        [Config("MaxPitchDeg", "SETTINGS_MAXPITSHDEG",
                "SETTINGS_MAXPITSHDEG_DETAIL")]
        public readonly CfFloat MaxPitchDeg = new CfFloat(70f, min: 0f, max: 90f);
        #endregion

        #region Camera Controls
        [Config("MovementSpeed", "SETTINGS_MOVEMENTSPEED")]
        public readonly CfFloat MovementSpeed = new CfFloat(30f, min: 0f, max: 60f);

        [Config("SpeedUpFactor", "SETTINGS_SPEEDUPFACTOR")]
        public readonly CfFloat SpeedUpFactor = new CfFloat(4f, min: 1.25f, max: 10f);

        [Config("InvertRotateHorizontal", "SETTINGS_INVERTROTATEHORIZONTAL")]
        public readonly CfFlag InvertRotateHorizontal = new CfFlag(false);

        [Config("InvertRotateVertical", "SETTINGS_INVERTROTATEVERTICAL")]
        public readonly CfFlag InvertRotateVertical = new CfFlag(false);

        [Config("RotateSensitivity", "SETTINGS_ROTATESENSITIVITY")]
        public readonly CfFloat RotateSensitivity = new CfFloat(5f, min: .25f, max: 10f);

        [Config("RotateKeyFactor", "SETTINGS_ROTATEKEYFACTOR")]
        public readonly CfFloat RotateKeyFactor = new CfFloat(8f, min: .5f, max: 32f);

        [Config("EnableDOF", "SETTINGS_ENBALEDOF")]
        public readonly CfFlag EnableDof = new CfFlag(false);

        [Config("FieldOfView", "SETTINGS_FIELDOFVIEW", "SETTINGS_FIELDOFVIEW_DETAIL")]
        public readonly CfFloat CamFieldOfView = new CfFloat(45f, min: 10f, max: 75f);
        #endregion

        #region Free-Camera Mode Options
        [Config("ShowCursor4Free", "SETTINGS_SHOWCORSOR4FREE")]
        public readonly CfFlag ShowCursor4Free = new CfFlag(false);

        public static string[] GroundClipping = {
            Ctransl.Translate("SETTINGS_GROUNDCLIPING_NONE"),
            Ctransl.Translate("SETTINGS_GROUNDCLIPING_ABOVE_GROUND"),
            Ctransl.Translate("SETTINGS_GROUNDCLIPING_SNAP_TO_GROUND"),
            Ctransl.Translate("SETTINGS_GROUNDCLIPING_ABOVE_ROAD"),
            Ctransl.Translate("SETTINGS_GROUNDCLIPING_SNAP_TO_ROAD")
        };


        [Config("GroundClipping", "SETTINGS_GROUNDCLIPING", "SETTINGS_GROUNDCLIPING_DETAIL")]
        public readonly ConfigData<int> GroundClippingOption
                                = new ConfigData<int>(0);

        [Config("GroundLevelOffset", "SETTINGS_GROUNDLEVELOFFSET",
                "SETTINGS_GROUNDLEVELOFFFSET_DETAIL")]
        public readonly CfFloat GroundLevelOffset = new CfFloat(0f, min: -2f, max: 10f);

        [Config("RoadLevelOffset", "SETTINGS_ROADLEVELOFFSET",
                "SETTINGS_ROADLEVELOFFSET_DETAIL")]
        public readonly CfFloat RoadLevelOffset = new CfFloat(0f, min: -2f, max: 10f);
        #endregion

        #region Follow Mode Options
        [Config("ShowCursor4Follow", "SETTINGS_SHOWCURSOR4FOLLOW")]
        public readonly CfFlag ShowCursor4Follow = new CfFlag(false);

        [Config("StickToFrontVehicle", "SETTINGS_STICKTOFRONTVEHICLE")]
        public readonly CfFlag StickToFrontVehicle = new CfFlag(true);

        [Config("LookAhead", "SETTINGS_LOOKAHEAD",
                "SETTINGS_LOOKAHAND_DETAIL")]
        public readonly CfFlag LookAhead = new CfFlag(false);

        [Config("InstantMoveMax", "SETTINGS_INSTANTMOVEMAX",
                "SETTINGS_INSTANTMOVEMAX_DETAIL")]
        public readonly CfFloat InstantMoveMax = new CfFloat(15f, min: 5f, max: 50f);

        [Config("FollowCamOffset", "SETTINGS_FOLLOWCAMOFFSET")]
        public readonly CfOffset FollowCamOffset = new CfOffset(
        new CfFloat(0f, min: -20f, max: 20f),
        new CfFloat(0f, min: -20f, max: 20f),
        new CfFloat(0f, min: -20f, max: 20f)
        );
        #endregion

        #region Walk-Through Mode Options
        [Config("Period4Walk", "SETTINGS_PERIOD4WALK")]
        public readonly CfFloat Period4Walk = new CfFloat(20f, min: 5f, max: 300f);

        [Config("ManualSwitch4Walk", "SETTINGS_MANUALSWITCH4WALK",
                "SETTINGS_MANUALSWITCH4WALK_DETAIL")]
        public readonly CfFlag ManualSwitch4Walk = new CfFlag(false);

        [Config("SelectPedestrian", "SETTINGS_SELECTPEDESTRIAN")]
        public readonly CfFlag SelectPedestrian = new CfFlag(true);

        [Config("SelectPassenger", "SETTINGS_SELECTPASSENGER")]
        public readonly CfFlag SelectPassenger = new CfFlag(true);

        [Config("SelectWaiting", "SETTINGS_SELECTWAITING")]
        public readonly CfFlag SelectWaiting = new CfFlag(true);

        [Config("SelectDriving", "SETTINGS_SELECTDRIVING")]
        public readonly CfFlag SelectDriving = new CfFlag(true);

        [Config("SelectPublicTransit", "SETTINGS_SELECTPUBLICTRANSIT")]
        public readonly CfFlag SelectPublicTransit = new CfFlag(true);

        [Config("SelectService", "SETTINGS_SELECTSERVICE")]
        public readonly CfFlag SelectService = new CfFlag(true);

        [Config("SelectCargo", "SETTINGS_SELECTCARGO")]
        public readonly CfFlag SelectCargo = new CfFlag(true);
        #endregion

        #region Key Mappings
        [Config("KeyCamToggle", "SETTINGS_KETCAMTOGGLE")]
        public readonly CfKey KeyCamToggle = new CfKey(KeyCode.BackQuote);

        [Config("KeySpeedUp", "SETTINGS_KEYSPPEDUP")]
        public readonly CfKey KeySpeedUp = new CfKey(KeyCode.CapsLock);

        [Config("KeyCamReset", "SETTINGS_KEYCAMRESET")]
        public readonly CfKey KeyCamReset = new CfKey(KeyCode.Backspace);

        [Config("KeyCursorToggle", "SETTINGS_KEYCURSORTOGGLE")]
        public readonly CfKey KeyCursorToggle = new CfKey(KeyCode.LeftControl);

        [Config("KeyAutoMove", "SETTINGS_KEYAUTOMOVE",
                "SETTINGS_KEYAUTOMOVE_DETAIL")]
        public readonly CfKey KeyAutoMove = new CfKey(KeyCode.E);

        [Config("KeySaveOffset", "SETTINGS_KEYSAVEFOFFSET",
                "SETTINGS_KEYSAVEFOFFSET_DETAIL")]
        public readonly CfKey KeySaveOffset = new CfKey(KeyCode.Backslash);

        [Config("KeyMoveForward", "SETTINGS_KEYMOVEFORWARD")]
        public readonly CfKey KeyMoveForward = new CfKey(KeyCode.W);

        [Config("KeyMoveBackward", "SETTINGS_KEYMOVEBACKWARD")]
        public readonly CfKey KeyMoveBackward = new CfKey(KeyCode.S);

        [Config("KeyMoveLeft", "SETTINGS_KEYMOVELEFT")]
        public readonly CfKey KeyMoveLeft = new CfKey(KeyCode.A);

        [Config("KeyMoveRight", "SETTINGS_KEYMOVERIFHT")]
        public readonly CfKey KeyMoveRight = new CfKey(KeyCode.D);

        [Config("KeyMoveUp", "SETTINGS_KEYMOVEUP")]
        public readonly CfKey KeyMoveUp = new CfKey(KeyCode.PageUp);

        [Config("KeyMoveDown", "SETTINGS_KEYMOVEDOWN")]
        public readonly CfKey KeyMoveDown = new CfKey(KeyCode.PageDown);

        [Config("KeyRotateLeft", "SETTINGS_KEYROTATELEFT")]
        public readonly CfKey KeyRotateLeft = new CfKey(KeyCode.LeftArrow);

        [Config("KeyRotateRight", "SETTINGS_KEYROTATERIGHT")]
        public readonly CfKey KeyRotateRight = new CfKey(KeyCode.RightArrow);

        [Config("KeyRotateUp", "SETTINGS_KEYROTATEUP")]
        public readonly CfKey KeyRotateUp = new CfKey(KeyCode.UpArrow);

        [Config("KeyRotateDown", "SETTINGS_KEYROTATEDOWN")]
        public readonly CfKey KeyRotateDown = new CfKey(KeyCode.DownArrow);
        #endregion

        #region Smooth Transition Options
        [Config("SmoothTransition", "SETTINGS_SMOOTRANSITION",
                "SETTINGS_SMOOTRANSITION_DETAIL")]
        public readonly CfFlag SmoothTransition = new CfFlag(true);

        [Config("TransitionRate", "SETTINGS_TRANSITIONRATE")]
        public readonly CfFloat TransRate = new CfFloat(.5f, min: .1f, max: .9f);

        [Config("GiveUpTransitionDistance", "SETTINGS_GIVEUPTRANSITIONDISTANCE",
                "SETTINGS_GIVEUPTRANSITIONDISTANCE_DETAIL")]
        public readonly CfFloat GiveUpTransDistance = new CfFloat(500f, min: 100f, max: 2000f);

        [Config("DeltaPosMin", "SETTINGS_DELTAPOSMIN")]
        public readonly CfFloat MinTransMove = new CfFloat(.5f, min: .1f, max: 5f);

        [Config("DeltaPosMax", "SETTINGS_DELTAPOSMAX")]
        public readonly CfFloat MaxTransMove = new CfFloat(30f, min: 5f, max: 100f);

        [Config("DeltaRotateMin", "SETTINGS_ROTATEMIN")]
        public readonly CfFloat MinTransRotate = new CfFloat(.1f, min: .05f, max: 5f);

        [Config("DeltaRotateMax", "SETTINGS_ROTATEMAX")]
        public readonly CfFloat MaxTransRotate = new CfFloat(10f, min: 5f, max: 45f);
        #endregion

        #region Configurable constants
        //this setting will not displayed in the settings screen
        [Config("MainPanelBtnPos", "SETTINGS_MAINPANELBTNPOS")]
        public readonly CfScreenPosition MainPanelBtnPos
                = new CfScreenPosition(CSkyL.Math.Vec2D.Position(-1f, -1f));

        [Config("CamNearClipPlane", "SETTINGS_CAMNEARCLIPPLANE")]
        public readonly CfFloat CamNearClipPlane = new CfFloat(1f, min: .125f, max: 64f);

        [Config("FoViewScrollfactor", "SETTINGS_FOVIEWSCROLLFACTOR", "SETTINGS_FOVIEWSCROLLFACTOR_DETAIL")]
        public readonly CfFloat FoViewScrollfactor = new CfFloat(1.05f, 1.01f, 2f);

        [Config("VehicleFixedOffset", "SETTINGS_VEHICLEFIXEDOFFERT")]
        public readonly CfOffset VehicleFixedOffset = new CfOffset(
        new CfFloat(3f, min: -20f, max: 20f),
        new CfFloat(2f, min: -20f, max: 20f),
        new CfFloat(0f, min: -20f, max: 20f));

        [Config("MidVehFixedOffset", "SETTINGS_MIDVEHFIXEDOFFSET")]
        public readonly CfOffset MidVehFixedOffset = new CfOffset(
        new CfFloat(-2f, min: -20f, max: 20f),
        new CfFloat(3f, min: -20f, max: 20f),
        new CfFloat(0f, min: -20f, max: 20f));

        [Config("PedestrianFixedOffset", "SETTINGS_PEDSTRIANFIXEDOFFSET")]
        public readonly CfOffset PedestrianFixedOffset = new CfOffset(
        new CfFloat(0f, min: -20f, max: 20f),
        new CfFloat(2f, min: -20f, max: 20f),
        new CfFloat(0f, min: -20f, max: 20f));

        [Config("MaxExitingDuration", "SETTINGS_MAXEXITINGDURATION")]
        public readonly CfFloat MaxExitingDuration = new CfFloat(2f, min: .1f, max: 10f);
        /*-------------------------------------------------------------------*/

        // Return a ratio[0f, 1f] representing the proportion to advance to the target
        //  *advance ratio per unit(.1 sec): TransRate
        //  *retain ratio per unit: 1f - AdvanceRatioPUnit   *units: elapsedTime / .1f
        //  *retain ratio: RetainRatioPUnit ^ units          *advance ratio: 1f - RetainRatio
        public float GetAdvanceRatio(float elapsedTime)
            => 1f - (float) Math.Pow(1f - TransRate, elapsedTime / .1f);
        #endregion
    }
}
