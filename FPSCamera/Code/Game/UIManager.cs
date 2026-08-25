using AlgernonCommons;
using ColossalFramework.UI;
using FPSCamera.Cam.Controller;
using FPSCamera.Utils;
using System.Collections;
namespace FPSCamera.Game
{
    public class UIManager
    {
        public static IEnumerator ToggleUI(bool visible)
        {
            try
            {

                if (GameCamController.Instance.UICamera != null)
                    GameCamController.Instance.UICamera.enabled = visible;
                else
                    UIView.Show(visible);

                NotificationManager.instance.NotificationsVisible = visible;
                GameAreaManager.instance.BordersVisible = visible;
                DistrictManager.instance.NamesVisible = visible;
                NetManager.instance.RoadNamesVisible = visible;
                GuideManager.instance.TutorialDisabled = !visible;
                DisasterManager.instance.MarkersVisible = visible;
                PropManager.instance.MarkersVisible = visible;

                if (ModSupport.FoundToggleIt)
                    ModSupport.ToggleIt_ToggleUI(visible);
            }
            catch (System.Exception e)
            {
                Logging.LogException(e, "Failed to toggle UI");
                UIView.Show(true);
            }
            yield break;
        }
    }
}


