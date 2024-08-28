using AlgernonCommons.Translation;
using ColossalFramework.UI;
using FPSCamera.Cam.Controller;
using FPSCamera.Utils;
using UnityEngine;

namespace FPSCamera.UI
{
    public class FollowButtons : MonoBehaviour
    {
        private void OnDestroy()
        {
            Destroy(citizenVehicleCameraButton);
            Destroy(cityServiceVehicleCameraButton);
            Destroy(publicTransportCameraButton);
            Destroy(citizenCameraButton);
            Destroy(touristCameraButton);
        }

        private void Awake()
        {
            var citizenVehicleInfoPanel = UIView.library.Get<CitizenVehicleWorldInfoPanel>(typeof(CitizenVehicleWorldInfoPanel).Name);
            citizenVehicleCameraButton = CreateCameraButton
            (
                citizenVehicleInfoPanel.component,
                (_, param) =>
                {
                    var instance = PrivateField.GetValue<InstanceID>(citizenVehicleInfoPanel, "m_InstanceID");
                    FPSCamController.Instance.StartFollowing(instance);
                }
            );
            citizenVehicleCameraButton.parent.eventVisibilityChanged += (_, isShow) =>
            {
                if (isShow == true)
                {
                    var instance = PrivateField.GetValue<InstanceID>(citizenVehicleInfoPanel, "m_InstanceID");
                    if (instance.Type == InstanceType.ParkedVehicle) citizenVehicleCameraButton.isVisible = false;
                }
            };
            var cityServiceVehicleInfoPanel = UIView.library.Get<CityServiceVehicleWorldInfoPanel>(typeof(CityServiceVehicleWorldInfoPanel).Name);
            cityServiceVehicleCameraButton = CreateCameraButton
            (
                cityServiceVehicleInfoPanel.component,
                (_, param) =>
                {
                    if (!(PrivateField.GetValue<InstanceID>(cityServiceVehicleInfoPanel, "m_InstanceID") is var instance))
                    {
                        cityServiceVehicleCameraButton.isVisible = false;
                        return;
                    }
                    FPSCamController.Instance.StartFollowing(instance);
                }
            );

            var publicTransportVehicleInfoPanel = UIView.library.Get<PublicTransportVehicleWorldInfoPanel>(typeof(PublicTransportVehicleWorldInfoPanel).Name);
            publicTransportCameraButton = CreateCameraButton
            (
                publicTransportVehicleInfoPanel.component,
                (_, param) =>
                {
                    if (!(PrivateField.GetValue<InstanceID>(publicTransportVehicleInfoPanel, "m_InstanceID") is var instance))
                    {
                        publicTransportCameraButton.isVisible = false;
                        return;
                    }
                    FPSCamController.Instance.StartFollowing(instance);
                }
            );

            var citizenInfoPanel = UIView.library.Get<CitizenWorldInfoPanel>(typeof(CitizenWorldInfoPanel).Name);
            citizenCameraButton = CreateCameraButton
            (
                citizenInfoPanel.component,
                (_, param) =>
                {
                    if (!(PrivateField.GetValue<InstanceID>(citizenInfoPanel, "m_InstanceID") is var instance))
                    {
                        citizenCameraButton.isVisible = false;
                        return;
                    }
                    FPSCamController.Instance.StartFollowing(instance);
                }
            );

            var touristInfoPanel = UIView.library.Get<TouristWorldInfoPanel>(typeof(TouristWorldInfoPanel).Name);
            touristCameraButton = CreateCameraButton
            (
                touristInfoPanel.component,
                (_, param) =>
                {
                    if (!(PrivateField.GetValue<InstanceID>(touristInfoPanel, "m_InstanceID") is var instance))
                    {
                        touristCameraButton.isVisible = false;
                        return;
                    }
                    FPSCamController.Instance.StartFollowing(instance);
                }
            );
        }

        UIButton CreateCameraButton(UIComponent parentComponent, MouseEventHandler handler)
        {
            //var button = UIView.GetAView().AddUIComponent(typeof(UIButton)) as UIButton;
            var button = parentComponent.AddUIComponent<UIButton>();
            button.name = parentComponent.name + "_StartFollow";
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
            button.eventClick += handler;
            button.AlignTo(parentComponent, UIAlignAnchor.BottomRight);
            button.relativePosition = new Vector3(parentComponent.width - 10f, parentComponent.height - 30f);
            return button;
        }

        private UIButton citizenVehicleCameraButton;
        private UIButton cityServiceVehicleCameraButton;
        private UIButton publicTransportCameraButton;
        private UIButton citizenCameraButton;
        private UIButton touristCameraButton;
    }

}
