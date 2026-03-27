using AlgernonCommons;
using ColossalFramework.UI;
using FPSCamera.Cam.Controller;
using FPSCamera.Utils;
using System.Collections;
using UnityEngine;
namespace FPSCamera.Game
{
    public class UIManager
    {
        private static ToolsModifierControl ToolsModifierControl
        {
            get
            {
                field ??= Object.FindObjectOfType<ToolsModifierControl>();
                return field;
            }
        }

        public static IEnumerator ToggleUI(bool visible)
        {
            try
            {
                NotificationManager.instance.NotificationsVisible = visible;
                GameAreaManager.instance.BordersVisible = visible;
                DistrictManager.instance.NamesVisible = visible;
                NetManager.instance.RoadNamesVisible = visible;
                GuideManager.instance.TutorialDisabled = !visible;
                DisasterManager.instance.MarkersVisible = visible;
                PropManager.instance.MarkersVisible = visible;

                if (ModSupport.FoundToggleIt)
                    ModSupport.ToggleIt_ToggleUI(visible);

                GameCamController.Instance.UICamera.enabled = visible;
                if (!visible)
                    ToolsModifierControl.CloseEverything();
            }
            catch (System.Exception e)
            {
                Logging.LogException(e, "Failed to toggle UI");
                UIView.Show(visible);
            }
            yield break;
        }
    }
}


