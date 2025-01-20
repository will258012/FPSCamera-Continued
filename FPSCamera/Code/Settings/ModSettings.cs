using AlgernonCommons;
using AlgernonCommons.Keybinding;
using AlgernonCommons.Notifications;
using AlgernonCommons.Translation;
using AlgernonCommons.XML;
using ColossalFramework.IO;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace FPSCamera.Settings
{
    [XmlRoot("FPSCamera")]
    public sealed class ModSettings : SettingsXMLBase
    {
        // Settings file name
        [XmlIgnore]
        internal static readonly string SettingsFileName = Path.Combine(DataLocation.localApplicationData, "FPSCamera_Continued.xml");

        internal static void Load() => XMLFileUtils.Load<ModSettings>(SettingsFileName);

        internal static void Save() => XMLFileUtils.Save<ModSettings>(SettingsFileName);
        [XmlIgnore]
        public static ModSettings Instance
        {
            get
            {
                if (_modSettings == null)
                {
                    _modSettings = new ModSettings();
                }
                return _modSettings;
            }
        }
        [XmlIgnore]
        private static ModSettings _modSettings;

        // Remember edit values here if the settings have edited!
        internal static void ResetToDefaults()
        {
            Translations.CurrentLanguage = "default";
            Logging.DetailLogging = false;
            WhatsNew.LastNotifiedVersionString = "0.0";

            HideGameUI = true;
            SetBackCamera = true;
            ShowInfoPanel = true;
            InfoPanelHeightScale = 1f;
            SpeedUnit = SpeedUnits.km_slash_h;
            Fade = true;

            LodOpt = 0;
            ShadowsOpt = false;

            Dof = false;
            InvertRotateHorizontal = false;
            InvertRotateVertical = false;
            MovementSpeed = 30f;
            SpeedUpFactor = 4f;
            RotateSensitivity = 5f;
            RotateKeyFactor = 8f;
            MaxPitchDeg = 70f;
            CamFieldOfView = 45f;
            CamNearClipPlane = 1f;
            FoViewScrollfactor = 1.2f;

            ShowCursorFree = false;
            GroundClipping = GroundClippings.None;
            GroundLevelOffset = 0f;
            RoadLevelOffset = 0f;
            PathsDetection = true;
            TracksDetection = true;

            ShowCursorFollow = false;
            StickToFrontVehicle = true;
            FollowCamOffset = Vector3.zero;
            VehicleFixedOffset = new Vector3(0f, 2f, 3f);
            MidVehFixedOffset = new Vector3(0f, 3f, -2f);
            PedestrianFixedOffset = new Vector3(0f, 2f, 0f);

            PeriodWalk = 20f;
            ManualSwitchWalk = false;
            SelectPedestrian = true;
            SelectPassenger = true;
            SelectWaiting = true;
            SelectDriving = true;
            SelectPublicTransit = true;
            SelectService = true;
            SelectCargo = true;

            SmoothTransition = true;
            TransSpeed = 10f;
            MinTransDistance = 15f;
            MaxTransDistance = 500f;

            KeyCamToggle = new KeyOnlyBinding(KeyCode.BackQuote);
            KeySpeedUp = new KeyOnlyBinding(KeyCode.CapsLock);
            KeyCamReset = new KeyOnlyBinding(KeyCode.Minus);
            KeyCursorToggle = new KeyOnlyBinding(KeyCode.Tab);
            KeyAutoMove = new KeyOnlyBinding(KeyCode.E);
            KeySaveOffset = new KeyOnlyBinding(KeyCode.Backslash);
            KeyMoveForward = new KeyOnlyBinding(KeyCode.W);
            KeyMoveBackward = new KeyOnlyBinding(KeyCode.S);
            KeyMoveLeft = new KeyOnlyBinding(KeyCode.A);
            KeyMoveRight = new KeyOnlyBinding(KeyCode.D);
            KeyMoveUp = new KeyOnlyBinding(KeyCode.PageUp);
            KeyMoveDown = new KeyOnlyBinding(KeyCode.PageDown);
            KeyRotateLeft = new KeyOnlyBinding(KeyCode.LeftArrow);
            KeyRotateRight = new KeyOnlyBinding(KeyCode.RightArrow);
            KeyRotateUp = new KeyOnlyBinding(KeyCode.UpArrow);
            KeyRotateDown = new KeyOnlyBinding(KeyCode.DownArrow);
            KeyUUIToggle = new Keybinding(KeyCode.F, false, true, false);
            MainButtonPos = new Vector3(0f, 0f);
        }
        public enum SpeedUnits
        {
            km_slash_h,
            kph,
            kmph,
            mph
        };

        public enum GroundClippings
        {
            None,
            AboveGround,
            SnapToGround,
            AboveRoad,
            SnapToRoad
        };
        #region General Options
        [XmlElement("HideGameUI")]
        public bool XMLHideGameUI { get => HideGameUI; set => HideGameUI = value; }
        [XmlIgnore]
        internal static bool HideGameUI = true;

        [XmlElement("SetBackCamera")]
        public bool XMLSetBackCamera { get => SetBackCamera; set => SetBackCamera = value; }
        [XmlIgnore]
        internal static bool SetBackCamera = true;

        [XmlElement("ShowInfoPanel")]
        public bool XMLShowInfoPanel { get => ShowInfoPanel; set => ShowInfoPanel = value; }
        [XmlIgnore]
        internal static bool ShowInfoPanel = true;

        [XmlElement("InfoPanelHeightScale")]
        public float XMLInfoPanelHeightScale { get => InfoPanelHeightScale; set => InfoPanelHeightScale = value; }
        [XmlIgnore]
        internal static float InfoPanelHeightScale = 1f;

        [XmlElement("SpeedUnit")]
        public SpeedUnits XMLSpeedUnit { get => SpeedUnit; set => SpeedUnit = value; }
        [XmlIgnore]
        internal static SpeedUnits SpeedUnit = SpeedUnits.km_slash_h;

        [XmlElement("Fade")]
        public bool XMLFade { get => Fade; set => Fade = value; }
        [XmlIgnore]
        internal static bool Fade = true;

        #region Optimization Options
        [XmlElement("LodOpt")]
        public int XMLLodOpt { get => LodOpt; set => LodOpt = value; }
        [XmlIgnore]
        internal static int LodOpt = 0;

        [XmlElement("ShadowsOpt")]
        public bool XMLShadowsOpt { get => ShadowsOpt; set => ShadowsOpt = value; }
        [XmlIgnore]
        internal static bool ShadowsOpt = false;
        #endregion

        #endregion

        #region Camera Options
        #region Camera Controls

        [XmlElement("Dof")]
        public bool XMLDof { get => Dof; set => Dof = value; }
        [XmlIgnore]
        internal static bool Dof = false;
        [XmlElement("InvertRotateHorizontal")]
        public bool XMLInvertRotateHorizontal { get => InvertRotateHorizontal; set => InvertRotateHorizontal = value; }
        [XmlIgnore]
        internal static bool InvertRotateHorizontal = false;

        [XmlElement("InvertRotateVertical")]
        public bool XMLInvertRotateVertical { get => InvertRotateVertical; set => InvertRotateVertical = value; }
        [XmlIgnore]
        internal static bool InvertRotateVertical = false;

        [XmlElement("MovementSpeed")]
        public float XMLMovementSpeed { get => MovementSpeed; set => MovementSpeed = value; }
        [XmlIgnore]
        internal static float MovementSpeed = 30f;

        [XmlElement("SpeedUpFactor")]
        public float XMLSpeedUpFactor { get => SpeedUpFactor; set => SpeedUpFactor = value; }
        [XmlIgnore]
        internal static float SpeedUpFactor = 4f;

        [XmlElement("RotateSensitivity")]
        public float XMLRotateSensitivity { get => RotateSensitivity; set => RotateSensitivity = value; }
        [XmlIgnore]
        internal static float RotateSensitivity = 5f;

        [XmlElement("RotateKeyFactor")]
        public float XMLRotateKeyFactor { get => RotateKeyFactor; set => RotateKeyFactor = value; }
        [XmlIgnore]
        internal static float RotateKeyFactor = 8f;

        [XmlElement("MaxPitchDeg")]
        public float XMLMaxPitchDeg { get => MaxPitchDeg; set => MaxPitchDeg = value; }
        [XmlIgnore]
        internal static float MaxPitchDeg = 70f;

        [XmlElement("CamFieldOfView")]
        public float XMLCamFieldOfView { get => CamFieldOfView; set => CamFieldOfView = value; }
        [XmlIgnore]
        internal static float CamFieldOfView = 45f;
        [XmlElement("CamNearClipPlane")]
        public float XMLCamNearClipPlane { get => CamNearClipPlane; set => CamNearClipPlane = value; }
        [XmlIgnore]
        internal static float CamNearClipPlane = 1f;

        [XmlElement("FoViewScrollfactor")]
        public float XMLFoViewScrollfactor { get => FoViewScrollfactor; set => FoViewScrollfactor = value; }
        [XmlIgnore]
        internal static float FoViewScrollfactor = 1.2f;

        #endregion
        #region Free-Camera Mode Options
        [XmlElement("ShowCursorFree")]
        public bool XMLShowCursorFree { get => ShowCursorFree; set => ShowCursorFree = value; }
        [XmlIgnore]
        internal static bool ShowCursorFree = false;

        [XmlElement("GroundClipping")]
        public GroundClippings XMLGroundClippingOption { get => GroundClipping; set => GroundClipping = value; }
        [XmlIgnore]
        internal static GroundClippings GroundClipping = GroundClippings.None;

        [XmlElement("GroundLevelOffset")]
        public float XMLGroundLevelOffset { get => GroundLevelOffset; set => GroundLevelOffset = value; }
        [XmlIgnore]
        internal static float GroundLevelOffset = 0f;

        [XmlElement("RoadLevelOffset")]
        public float XMLRoadLevelOffset { get => RoadLevelOffset; set => RoadLevelOffset = value; }
        [XmlIgnore]
        internal static float RoadLevelOffset = 0f;

        [XmlElement("PathsDetection")]
        public bool XMLPathsInRoads { get => PathsDetection; set => PathsDetection = value; }
        [XmlIgnore]
        internal static bool PathsDetection = true;

        [XmlElement("TracksDetection")]
        public bool XMLRailsInRoads { get => TracksDetection; set => TracksDetection = value; }
        [XmlIgnore]
        internal static bool TracksDetection = true;

        #endregion
        #region Follow Mode Options
        [XmlElement("ShowCursorFollow")]
        public bool XMLShowCursorFollow { get => ShowCursorFollow; set => ShowCursorFollow = value; }
        [XmlIgnore]
        internal static bool ShowCursorFollow = false;

        [XmlElement("StickToFrontVehicle")]
        public bool XMLStickToFrontVehicle { get => StickToFrontVehicle; set => StickToFrontVehicle = value; }
        [XmlIgnore]
        internal static bool StickToFrontVehicle = true;

        [XmlElement("FollowCamOffset")]
        public Vector3 XMLFollowCamOffset { get => FollowCamOffset; set => FollowCamOffset = value; }
        [XmlIgnore]
        internal static Vector3 FollowCamOffset = Vector3.zero;

        [XmlElement("VehicleFixedOffset")]
        public Vector3 XMLVehicleFixedOffset { get => VehicleFixedOffset; set => VehicleFixedOffset = value; }
        [XmlIgnore]
        internal static Vector3 VehicleFixedOffset = new Vector3(0f, 2f, 3f);

        [XmlElement("MidVehFixedOffset")]
        public Vector3 XMLMidVehFixedOffset { get => MidVehFixedOffset; set => MidVehFixedOffset = value; }
        [XmlIgnore]
        internal static Vector3 MidVehFixedOffset = new Vector3(0f, 3f, -2f);

        [XmlElement("PedestrianFixedOffset")]
        public Vector3 XMLPedestrianFixedOffset { get => PedestrianFixedOffset; set => PedestrianFixedOffset = value; }
        [XmlIgnore]
        internal static Vector3 PedestrianFixedOffset = new Vector3(0f, 2f, 0f);

        #endregion
        #region Walk-Through Mode Options
        [XmlElement("PeriodWalk")]
        public float XMLPeriod4Walk { get => PeriodWalk; set => PeriodWalk = value; }
        [XmlIgnore]
        internal static float PeriodWalk = 20f;

        [XmlElement("ManualSwitchWalk")]
        public bool XMLManualSwitch4Walk { get => ManualSwitchWalk; set => ManualSwitchWalk = value; }
        [XmlIgnore]
        internal static bool ManualSwitchWalk = false;

        [XmlElement("SelectPedestrian")]
        public bool XMLSelectPedestrian { get => SelectPedestrian; set => SelectPedestrian = value; }
        [XmlIgnore]
        internal static bool SelectPedestrian = true;

        [XmlElement("SelectPassenger")]
        public bool XMLSelectPassenger { get => SelectPassenger; set => SelectPassenger = value; }
        [XmlIgnore]
        internal static bool SelectPassenger = true;

        [XmlElement("SelectWaiting")]
        public bool XMLSelectWaiting { get => SelectWaiting; set => SelectWaiting = value; }
        [XmlIgnore]
        internal static bool SelectWaiting = true;

        [XmlElement("SelectDriving")]
        public bool XMLSelectDriving { get => SelectDriving; set => SelectDriving = value; }
        [XmlIgnore]
        internal static bool SelectDriving = true;

        [XmlElement("SelectPublicTransit")]
        public bool XMLSelectPublicTransit { get => SelectPublicTransit; set => SelectPublicTransit = value; }
        [XmlIgnore]
        internal static bool SelectPublicTransit = true;

        [XmlElement("SelectService")]
        public bool XMLSelectService { get => SelectService; set => SelectService = value; }
        [XmlIgnore]
        internal static bool SelectService = true;

        [XmlElement("SelectCargo")]
        public bool XMLSelectCargo { get => SelectCargo; set => SelectCargo = value; }
        [XmlIgnore]
        internal static bool SelectCargo = true;
        #endregion
        #region Smooth Transition Options
        [XmlElement("SmoothTransition")]
        public bool XMLSmoothTransition { get => SmoothTransition; set => SmoothTransition = value; }
        [XmlIgnore]
        internal static bool SmoothTransition = true;

        [XmlElement("TransSpeed")]
        public float XMLTransSpeed { get => TransSpeed; set => TransSpeed = value; }
        [XmlIgnore]
        internal static float TransSpeed = 10f;

        [XmlElement("MinTransDistance")]
        public float XMLMinTransDistance { get => MinTransDistance; set => MinTransDistance = value; }
        [XmlIgnore]
        internal static float MinTransDistance = 15f;


        [XmlElement("MaxTransDistance")]
        public float XMLMaxTransDistance { get => MaxTransDistance; set => MaxTransDistance = value; }
        [XmlIgnore]
        internal static float MaxTransDistance = 500f;

        #endregion
        #endregion

        #region Key Mappings
        [XmlElement("KeyCamToggle")]
        public KeyOnlyBinding XMLKeyCamToggle { get => KeyCamToggle; set => KeyCamToggle = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyCamToggle = new KeyOnlyBinding(KeyCode.BackQuote);

        [XmlElement("KeySpeedUp")]
        public KeyOnlyBinding XMLKeySpeedUp { get => KeySpeedUp; set => KeySpeedUp = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeySpeedUp = new KeyOnlyBinding(KeyCode.CapsLock);

        [XmlElement("KeyCamReset")]
        public KeyOnlyBinding XMLKeyCamReset { get => KeyCamReset; set => KeyCamReset = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyCamReset = new KeyOnlyBinding(KeyCode.Minus);

        [XmlElement("KeyCursorToggle")]
        public KeyOnlyBinding XMLKeyCursorToggle { get => KeyCursorToggle; set => KeyCursorToggle = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyCursorToggle = new KeyOnlyBinding(KeyCode.Tab);

        [XmlElement("KeyAutoMove")]
        public KeyOnlyBinding XMLKeyAutoMove { get => KeyAutoMove; set => KeyAutoMove = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyAutoMove = new KeyOnlyBinding(KeyCode.E);

        [XmlElement("KeySaveOffset")]
        public KeyOnlyBinding XMLKeySaveOffset { get => KeySaveOffset; set => KeySaveOffset = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeySaveOffset = new KeyOnlyBinding(KeyCode.Backslash);

        [XmlElement("KeyMoveForward")]
        public KeyOnlyBinding XMLKeyMoveForward { get => KeyMoveForward; set => KeyMoveForward = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyMoveForward = new KeyOnlyBinding(KeyCode.W);

        [XmlElement("KeyMoveBackward")]
        public KeyOnlyBinding XMLKeyMoveBackward { get => KeyMoveBackward; set => KeyMoveBackward = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyMoveBackward = new KeyOnlyBinding(KeyCode.S);

        [XmlElement("KeyMoveLeft")]
        public KeyOnlyBinding XMLKeyMoveLeft { get => KeyMoveLeft; set => KeyMoveLeft = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyMoveLeft = new KeyOnlyBinding(KeyCode.A);

        [XmlElement("KeyMoveRight")]
        public KeyOnlyBinding XMLKeyMoveRight { get => KeyMoveRight; set => KeyMoveRight = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyMoveRight = new KeyOnlyBinding(KeyCode.D);

        [XmlElement("KeyMoveUp")]
        public KeyOnlyBinding XMLKeyMoveUp { get => KeyMoveUp; set => KeyMoveUp = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyMoveUp = new KeyOnlyBinding(KeyCode.PageUp);

        [XmlElement("KeyMoveDown")]
        public KeyOnlyBinding XMLKeyMoveDown { get => KeyMoveDown; set => KeyMoveDown = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyMoveDown = new KeyOnlyBinding(KeyCode.PageDown);

        [XmlElement("KeyRotateLeft")]
        public KeyOnlyBinding XMLKeyRotateLeft { get => KeyRotateLeft; set => KeyRotateLeft = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyRotateLeft = new KeyOnlyBinding(KeyCode.LeftArrow);

        [XmlElement("KeyRotateRight")]
        public KeyOnlyBinding XMLKeyRotateRight { get => KeyRotateRight; set => KeyRotateRight = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyRotateRight = new KeyOnlyBinding(KeyCode.RightArrow);

        [XmlElement("KeyRotateUp")]
        public KeyOnlyBinding XMLKeyRotateUp { get => KeyRotateUp; set => KeyRotateUp = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyRotateUp = new KeyOnlyBinding(KeyCode.UpArrow);

        [XmlElement("KeyRotateDown")]
        public KeyOnlyBinding XMLKeyRotateDown { get => KeyRotateDown; set => KeyRotateDown = value; }
        [XmlIgnore]
        internal static KeyOnlyBinding KeyRotateDown = new KeyOnlyBinding(KeyCode.DownArrow);

        [XmlElement("KeyUUIToggle")]
        public Keybinding XMLKeyUUIToggle { get => KeyUUIToggle; set => KeyUUIToggle = value; }
        [XmlIgnore]
        internal static Keybinding KeyUUIToggle = new Keybinding(KeyCode.F, false, true, false);
        #endregion

        [XmlElement("MainButtonPos")]
        public Vector3 XMLMainButtonPos { get => MainButtonPos; set => MainButtonPos = value; }
        [XmlIgnore]
        internal static Vector3 MainButtonPos = new Vector3(0f, 0f);
    }
}
