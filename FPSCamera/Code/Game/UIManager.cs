using AlgernonCommons;
using FPSCamera.Utils;
using System.Collections;
using System.Linq;
using UnityEngine;
namespace FPSCamera.Game
{
    public class UIManager
    {
        /// <summary>
        /// Gets the game's UI camera instance.
        /// </summary>
        public static Camera UICamera
        {
            get
            {
                if (uiCamera == null)
                    uiCamera = Object.FindObjectsOfType<Camera>().FirstOrDefault(cam => cam.name == "UIView");
                return uiCamera;
            }
        }
        private static Camera uiCamera = null;
        public static IEnumerator ToggleUI(bool visible)
        {
            try
            {
                if (ModSupport.FoundToggleIt)
                {
                    ModSupport.ToggleIt_ToggleUI(visible);
                }
                else
                {
                    NotificationManager.instance.NotificationsVisible = visible;
                    GameAreaManager.instance.BordersVisible = visible;
                    DistrictManager.instance.NamesVisible = visible;
                    NetManager.instance.RoadNamesVisible = visible;
                }
                GuideManager.instance.TutorialDisabled = !visible;
                DisasterManager.instance.MarkersVisible = visible;
                PropManager.instance.MarkersVisible = visible;

                UICamera.enabled = visible;
                if (!visible)
                    Object.FindObjectOfType<ToolsModifierControl>().CloseEverything();
            }
            catch (System.Exception e)
            {
                Logging.LogException(e, "Failed to toggle UI");
            }
            yield break;
        }
    }
}


