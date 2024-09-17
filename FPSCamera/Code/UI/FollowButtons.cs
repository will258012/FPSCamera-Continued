using AlgernonCommons.Translation;
using ColossalFramework.UI;
using FPSCamera.Cam.Controller;
using FPSCamera.Utils;
using System;
using UnityEngine;

namespace FPSCamera.UI
{
    public class FollowButtons : MonoBehaviour
    {
        public static FollowButtons Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
            citizenVehicleInfo_Button = Initialize(ref citizenVehicleInfo_Panel);
            cityServiceVehicleInfo_Button = Initialize(ref cityServiceVehicleInfo_Panel);
            publicTransportVehicleInfo_Button = Initialize(ref publicTransportVehicleInfo_Panel);
            citizenInfo_Button = Initialize(ref citizenInfo_Panel);
            touristInfo_Button = Initialize(ref touristInfo_Panel);
        }
        private void Update()
        {
            UpdateButtonVisibility(citizenVehicleInfo_Panel, citizenVehicleInfo_Button,
                id => id.Type != InstanceType.ParkedVehicle);
            UpdateButtonVisibility(cityServiceVehicleInfo_Panel, cityServiceVehicleInfo_Button);
            UpdateButtonVisibility(publicTransportVehicleInfo_Panel, publicTransportVehicleInfo_Button);
            UpdateButtonVisibility(citizenInfo_Panel, citizenInfo_Button);
            UpdateButtonVisibility(touristInfo_Panel, touristInfo_Button);
        }
        private void OnDestroy()
        {
            Destroy(citizenVehicleInfo_Button);
            Destroy(cityServiceVehicleInfo_Button);
            Destroy(publicTransportVehicleInfo_Button);
            Destroy(citizenInfo_Button);
            Destroy(touristInfo_Button);
        }

        private UIButton Initialize<T>(ref T panel) where T : WorldInfoPanel
        {
            panel = UIView.library.Get<T>(typeof(T).Name);
            return CreateCameraButton(panel);
        }

        private UIButton CreateCameraButton<T>(T panel) where T : WorldInfoPanel
        {
            var button = panel.component.AddUIComponent<UIButton>();
            button.name = panel.component.name + "_StartFollow";
            button.tooltip = Translations.Translate("FOLLOWBTN_TOOLTIP");
            button.size = new Vector2(40f, 40f);
            button.scaleFactor = .8f;
            button.pressedBgSprite = "OptionBasePressed";
            button.normalBgSprite = "OptionBase";
            button.hoveredBgSprite = "OptionBaseHovered";
            button.disabledBgSprite = "OptionBaseDisabled";
            button.normalFgSprite = "InfoPanelIconFreecamera";
            button.textColor = new Color32(255, 255, 255, 255);
            button.disabledTextColor = new Color32(7, 7, 7, 255);
            button.hoveredTextColor = new Color32(255, 255, 255, 255);
            button.focusedTextColor = new Color32(255, 255, 255, 255);
            button.pressedTextColor = new Color32(30, 30, 44, 255);
            button.eventClick += (_, p) =>
            {
                FPSCamController.Instance.StartFollowing(GetPanelInstanceID(panel));
                panel.component.isVisible = false;
            };
            button.AlignTo(panel.component, UIAlignAnchor.BottomRight);
            button.relativePosition = new Vector3(button.relativePosition.x - 4f, button.relativePosition.y - 20f);

            return button;
        }
        private void UpdateButtonVisibility<T>(T panel, UIButton button, Func<InstanceID, bool> filter = null) where T : WorldInfoPanel
        {
            if (panel.component.isVisible)
            {
                var instanceID = GetPanelInstanceID(panel);
                button.isVisible = instanceID != default && (filter?.Invoke(instanceID) ?? true);
            }
        }

        private InstanceID GetPanelInstanceID<T>(T panel) where T : WorldInfoPanel => AccessUtils.GetFieldValue<InstanceID>(panel, "m_InstanceID");

        private CitizenVehicleWorldInfoPanel citizenVehicleInfo_Panel;
        private UIButton citizenVehicleInfo_Button;

        private CityServiceVehicleWorldInfoPanel cityServiceVehicleInfo_Panel;
        private UIButton cityServiceVehicleInfo_Button;

        private PublicTransportVehicleWorldInfoPanel publicTransportVehicleInfo_Panel;
        private UIButton publicTransportVehicleInfo_Button;

        private CitizenWorldInfoPanel citizenInfo_Panel;
        private UIButton citizenInfo_Button;

        private TouristWorldInfoPanel touristInfo_Panel;
        private UIButton touristInfo_Button;
    }

}
