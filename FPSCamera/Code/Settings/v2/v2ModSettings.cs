using AlgernonCommons.XML;
using ColossalFramework.IO;
using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace FPSCamera.Settings.v2
{
    [XmlRoot("Config")]
    public class v2ModSettings : SettingsXMLBase
    {
        [XmlIgnore]
        internal static readonly string SettingsFileName = Path.Combine(DataLocation.executableDirectory, "FPSCameraConfig.xml");

        internal static void Load()
        {
            using (var reader = new StreamReader(SettingsFileName))
            {
                var xmlSerializer = new XmlSerializer(typeof(v2ModSettings));
                if (!(xmlSerializer.Deserialize(reader) is v2ModSettings xmlFile))
                {
                    throw new FileLoadException("couldn't deserialize XML file ", SettingsFileName);
                }
            }
        }
        // old config file use "True" and "False" which the xml Serializater could not read it, we need to convert it by ourselves
        [XmlElement("HideGameUI")]
        public string HideGameUI { get => ModSettings.HideGameUI.ToString(); set => ModSettings.HideGameUI = bool.Parse(value); }

        [XmlElement("SetBackCamera")]
        public string SetBackCamera { get => ModSettings.SetBackCamera.ToString(); set => ModSettings.SetBackCamera = bool.Parse(value); }

        [XmlElement("UseMetricUnit")]
        public string UseMetricUnit { get => ModSettings.UseMetricUnit.ToString(); set => ModSettings.UseMetricUnit = bool.Parse(value); }

        [XmlElement("ShowInfoPanel")]
        public string ShowInfoPanel { get => ModSettings.ShowInfoPanel.ToString(); set => ModSettings.ShowInfoPanel = bool.Parse(value); }


        [XmlElement("MaxPitchDeg")]
        public float MaxPitchDeg { get => ModSettings.MaxPitchDeg; set => ModSettings.MaxPitchDeg = value; }

        [XmlElement("MovementSpeed")]
        public float MovementSpeed { get => ModSettings.MovementSpeed; set => ModSettings.MovementSpeed = value; }

        [XmlElement("SpeedUpFactor")]
        public float SpeedUpFactor { get => ModSettings.SpeedUpFactor; set => ModSettings.SpeedUpFactor = value; }

        [XmlElement("InvertRotateHorizontal")]
        public string InvertRotateHorizontal
        { get => ModSettings.InvertRotateHorizontal.ToString(); set => ModSettings.InvertRotateHorizontal = bool.Parse(value); }

        [XmlElement("InvertRotateVertical")]
        public string InvertRotateVertical { get => ModSettings.InvertRotateVertical.ToString(); set => ModSettings.InvertRotateVertical = bool.Parse(value); }

        [XmlElement("RotateSensitivity")]
        public float RotateSensitivity { get => ModSettings.RotateSensitivity; set => ModSettings.RotateSensitivity = value; }

        [XmlElement("RotateKeyFactor")]
        public float RotateKeyFactor { get => ModSettings.RotateKeyFactor; set => ModSettings.RotateKeyFactor = value; }

        [XmlElement("EnableDof")]
        public string EnableDof { get => ModSettings.Dof.ToString(); set => ModSettings.Dof = bool.Parse(value); }

        [XmlElement("CamFieldOfView")]
        public float CamFieldOfView { get => ModSettings.CamFieldOfView; set => ModSettings.CamFieldOfView = value; }

        [XmlElement("ShowCursor4Free")]
        public string ShowCursor4Free { get => ModSettings.ShowCursorFree.ToString(); set => ModSettings.ShowCursorFree = bool.Parse(value); }


        [XmlElement("GroundClippingOption")]
        public string GroundClippingOption
        {
            get => ModSettings.GroundClipping.ToString();
            set
            {
                int setValue;
                switch (value)
                {//v2.2.0
                    case "None": setValue = 0; break;
                    case "AboveGround": setValue = 1; break;
                    case "SnapToGround": setValue = 2; break;
                    case "AboveRoad": setValue = 3; break;
                    case "SnapToRoad": setValue = 4; break;
                    default: // v2.4.1
                        setValue = int.Parse(value);
                        break;
                }
                ModSettings.GroundClipping = setValue;
            }
        }

        [XmlElement("GroundLevelOffset")]
        public float GroundLevelOffset { get => ModSettings.GroundLevelOffset; set => ModSettings.GroundLevelOffset = value; }

        [XmlElement("RoadLevelOffset")]
        public float RoadLevelOffset { get => ModSettings.RoadLevelOffset; set => ModSettings.RoadLevelOffset = value; }

        [XmlElement("ShowCursor4Follow")]
        public string ShowCursor4Follow { get => ModSettings.ShowCursorFollow.ToString(); set => ModSettings.ShowCursorFollow = bool.Parse(value); }

        [XmlElement("StickToFrontVehicle")]
        public string StickToFrontVehicle { get => ModSettings.StickToFrontVehicle.ToString(); set => ModSettings.StickToFrontVehicle = bool.Parse(value); }

        [XmlElement("InstantMoveMax")]
        public float InstantMoveMax { get => ModSettings.MinTransDistance; set => ModSettings.MinTransDistance = value; }

        [XmlElement("FollowCamOffset")]
        public string FollowCamOffset
        {
            get => $"{ModSettings.FollowCamOffset.z},{ModSettings.FollowCamOffset.y},{ModSettings.FollowCamOffset.x}";
            set
            {
                var split = value.Split(',');
                ModSettings.FollowCamOffset = new Vector3(
                    float.Parse(split[2]),
                    float.Parse(split[1]),
                    float.Parse(split[0]));
            }
        }

        [XmlElement("Period4Walk")]
        public float Period4Walk { get => ModSettings.PeriodWalk; set => ModSettings.PeriodWalk = value; }

        [XmlElement("ManualSwitch4Walk")]
        public string ManualSwitch4Walk { get => ModSettings.ManualSwitchWalk.ToString(); set => ModSettings.ManualSwitchWalk = bool.Parse(value); }

        [XmlElement("SelectPedestrian")]
        public string SelectPedestrian { get => ModSettings.SelectPedestrian.ToString(); set => ModSettings.SelectPedestrian = bool.Parse(value); }

        [XmlElement("SelectPassenger")]
        public string SelectPassenger { get => ModSettings.SelectPassenger.ToString(); set => ModSettings.SelectPassenger = bool.Parse(value); }

        [XmlElement("SelectWaiting")]
        public string SelectWaiting { get => ModSettings.SelectWaiting.ToString(); set => ModSettings.SelectWaiting = bool.Parse(value); }

        [XmlElement("SelectDriving")]
        public string SelectDriving { get => ModSettings.SelectDriving.ToString(); set => ModSettings.SelectDriving = bool.Parse(value); }

        [XmlElement("SelectPublicTransit")]
        public string SelectPublicTransit { get => ModSettings.SelectPublicTransit.ToString(); set => ModSettings.SelectPublicTransit = bool.Parse(value); }

        [XmlElement("SelectService")]
        public string SelectService { get => ModSettings.SelectService.ToString(); set => ModSettings.SelectService = bool.Parse(value); }

        [XmlElement("SelectCargo")]
        public string SelectCargo { get => ModSettings.SelectCargo.ToString(); set => ModSettings.SelectCargo = bool.Parse(value); }

        [XmlElement("KeyCamToggle")]
        public string KeyCamToggle { get => ModSettings.KeyCamToggle.ToString(); set => ModSettings.KeyCamToggle.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeySpeedUp")]
        public string KeySpeedUp { get => ModSettings.KeySpeedUp.ToString(); set => ModSettings.KeySpeedUp.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeyCamReset")]
        public string KeyCamReset { get => ModSettings.KeyCamReset.ToString(); set => ModSettings.KeyCamReset.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeyCursorToggle")]
        public string KeyCursorToggle { get => ModSettings.KeyCursorToggle.ToString(); set => ModSettings.KeyCursorToggle.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeyAutoMove")]
        public string KeyAutoMove { get => ModSettings.KeyAutoMove.ToString(); set => ModSettings.KeyAutoMove.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeySaveOffset")]
        public string KeySaveOffset { get => ModSettings.KeySaveOffset.ToString(); set => ModSettings.KeySaveOffset.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeyMoveForward")]
        public string KeyMoveForward { get => ModSettings.KeyMoveForward.ToString(); set => ModSettings.KeyMoveForward.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeyMoveBackward")]
        public string KeyMoveBackward { get => ModSettings.KeyMoveBackward.ToString(); set => ModSettings.KeyMoveBackward.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeyMoveLeft")]
        public string KeyMoveLeft { get => ModSettings.KeyMoveLeft.ToString(); set => ModSettings.KeyMoveLeft.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeyMoveRight")]
        public string KeyMoveRight { get => ModSettings.KeyMoveRight.ToString(); set => ModSettings.KeyMoveRight.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeyMoveUp")]
        public string KeyMoveUp { get => ModSettings.KeyMoveUp.ToString(); set => ModSettings.KeyMoveUp.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeyMoveDown")]
        public string KeyMoveDown { get => ModSettings.KeyMoveDown.ToString(); set => ModSettings.KeyMoveDown.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeyRotateLeft")]
        public string KeyRotateLeft { get => ModSettings.KeyRotateLeft.ToString(); set => ModSettings.KeyRotateLeft.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeyRotateRight")]
        public string KeyRotateRight { get => ModSettings.KeyRotateRight.ToString(); set => ModSettings.KeyRotateRight.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeyRotateUp")]
        public string KeyRotateUp { get => ModSettings.KeyRotateUp.ToString(); set => ModSettings.KeyRotateUp.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("KeyRotateDown")]
        public string KeyRotateDown { get => ModSettings.KeyRotateDown.ToString(); set => ModSettings.KeyRotateDown.Key = (int)Enum.Parse(typeof(KeyCode), value); }


        [XmlElement("KeyUUIToggle")]
        public string KeyUUIToggle
        {
            get => ModSettings.KeyUUIToggle.ToString();
            set
            {
                var split = value.Split('+');
                ModSettings.KeyUUIToggle = new AlgernonCommons.Keybinding.Keybinding((KeyCode)Enum.Parse(typeof(KeyCode), split[1]),
                                                                                     split[0] == "Control",
                                                                                     split[0] == "Shift",
                                                                                     split[0] == "Alt");
            }
        }

        [XmlElement("SmoothTransition")]
        public string SmoothTransition { get => ModSettings.SmoothTransition.ToString(); set => ModSettings.SmoothTransition = bool.Parse(value); }

        [XmlElement("GiveUpTransDistance")]
        public float GiveUpTransDistance { get => ModSettings.MaxTransDistance; set => ModSettings.MaxTransDistance = value; }

        [XmlElement("LODOptimization")]
        public string LODOptimization { get => (ModSettings.LodOpt >= 1).ToString(); set => ModSettings.LodOpt = bool.Parse(value) ? 1 : 0; }

        [XmlElement("ShadowsOptimization")]
        public string ShadowsOptimization { get => ModSettings.ShadowsOpt.ToString(); set => ModSettings.ShadowsOpt = bool.Parse(value); }

        [XmlElement("MainPanelBtnPos")]
        public string MainPanelBtnPos
        {
            get => $"{ModSettings.MainButtonPos.x},{ModSettings.MainButtonPos.y}";
            set
            {
                var split = value.Split(',');
                ModSettings.MainButtonPos = new Vector2(
                    float.Parse(split[0]),
                    float.Parse(split[1])
                );
            }
        }

        [XmlElement("CamNearClipPlane")]
        public float CamNearClipPlane { get => ModSettings.CamNearClipPlane; set => ModSettings.CamNearClipPlane = value; }

        [XmlElement("FoViewScrollfactor")]
        public float FoViewScrollfactor { get => ModSettings.FoViewScrollfactor; set => ModSettings.FoViewScrollfactor = value; }

        [XmlElement("VehicleFixedOffset")]
        public string VehicleFixedOffset
        {
            get => $"{ModSettings.VehicleFixedOffset.z},{ModSettings.VehicleFixedOffset.y},{ModSettings.VehicleFixedOffset.x}";
            set
            {
                var split = value.Split(',');
                ModSettings.VehicleFixedOffset = new Vector3(
                    float.Parse(split[2]),
                    float.Parse(split[1]),
                    float.Parse(split[0])
                );
            }
        }

        [XmlElement("MidVehFixedOffset")]
        public string MidVehFixedOffset
        {
            get => $"{ModSettings.MidVehFixedOffset.z},{ModSettings.MidVehFixedOffset.y},{ModSettings.MidVehFixedOffset.x}";
            set
            {
                var split = value.Split(',');
                ModSettings.MidVehFixedOffset = new Vector3(
                    float.Parse(split[2]),
                    float.Parse(split[1]),
                    float.Parse(split[0])
                );
            }
        }

        [XmlElement("PedestrianFixedOffset")]
        public string PedestrianFixedOffset
        {
            get => $"{ModSettings.PedestrianFixedOffset.z},{ModSettings.PedestrianFixedOffset.y},{ModSettings.PedestrianFixedOffset.x}";
            set
            {
                var split = value.Split(',');
                ModSettings.PedestrianFixedOffset = new Vector3(
                    float.Parse(split[2]),
                    float.Parse(split[1]),
                    float.Parse(split[0])
                );
            }
        }
    }
}
