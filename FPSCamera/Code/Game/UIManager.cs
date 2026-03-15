using AlgernonCommons;
using ColossalFramework.UI;
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
                field ??= Object.FindObjectsOfType<Camera>().FirstOrDefault(cam => cam.name == "UIView");
                return field;
            }
        }

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

                UICamera.enabled = visible;
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


