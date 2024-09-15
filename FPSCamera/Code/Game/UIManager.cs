using AlgernonCommons;
using FPSCamera.Cam.Controller;
using FPSCamera.Utils;
using System.Collections;
using UnityEngine;
using ToggleItManager = ToggleIt.Managers.ToggleManager;
namespace FPSCamera.Game
{
    public class UIManager
    {
        private class UIState
        {
            internal bool NotificationsVisible { get; set; }
            internal bool BordersVisible { get; set; }
            internal bool DirectNamesVisible { get; set; }
            internal bool RoadNamesVisible { get; set; }
            internal bool ContoursVisible { get; set; }
        }

        private static UIState savedState;

        public static IEnumerator ToggleUI(bool visibility)
        {
            if (ModSupport.FoundToggleIt)
            {
                try
                {
                    SetUIVisibilityByToggleIt(visibility);
                }
                catch (System.Exception e)
                {
                    Logging.Error($"ModSupport: Failed to toggle UI using \"Toggle It!\". Falling back to the vanilla way.");
                    Logging.LogException(e);
                    SetUIVisibilityDirectly(visibility);
                }
            }
            else
            {
                SetUIVisibilityDirectly(visibility);
            }
            yield break;
        }

        private static void SaveState()
        {
            savedState = new UIState
            {
                NotificationsVisible = ToggleItManager.Instance.GetById(1).On,
                RoadNamesVisible = ToggleItManager.Instance.GetById(2).On,
                BordersVisible = ToggleItManager.Instance.GetById(4).On,
                ContoursVisible = ToggleItManager.Instance.GetById(5).On,
                DirectNamesVisible = ToggleItManager.Instance.GetById(10).On,
            };
            Logging.Message($"ModSupport: Saved UI state from \"Toggle It!\":\n" +
                     $"  NotificationIcons = {savedState.NotificationsVisible}\n" +
                     $"  BorderLines = {savedState.BordersVisible}\n" +
                     $"  ContourLines = {savedState.ContoursVisible}\n" +
                     $"  RoadNames = {savedState.RoadNamesVisible}\n" +
                     $"  DistrictNames = {savedState.DirectNamesVisible}");
        }

        private static void RestoreState()
        {
            if (savedState != null)
            {
                ToggleItManager.Instance.Apply(1, savedState.NotificationsVisible);
                ToggleItManager.Instance.Apply(2, savedState.RoadNamesVisible);
                ToggleItManager.Instance.Apply(4, savedState.BordersVisible);
                ToggleItManager.Instance.Apply(5, savedState.ContoursVisible);
                ToggleItManager.Instance.Apply(10, savedState.DirectNamesVisible);
                Logging.Message("ModSupport: Restored saved UI state using \"Toggle It!\"");
            }
        }

        private static void SetUIVisibilityByToggleIt(bool visibility)
        {
            if (!visibility)
            {
                Object.FindObjectOfType<ToolsModifierControl>().CloseEverything();
                SaveState();
                ToggleItManager.Instance.Apply(1, false);
                ToggleItManager.Instance.Apply(2, false);
                ToggleItManager.Instance.Apply(4, false);
                ToggleItManager.Instance.Apply(5, false);
                ToggleItManager.Instance.Apply(10, false);
                Logging.Message("ModSupport: Hid UI using \"Toggle It!\"");
            }
            else
            {
                RestoreState();
            }
            PropManager.instance.MarkersVisible = visibility;
            GuideManager.instance.TutorialDisabled = !visibility;
            DisasterManager.instance.MarkersVisible = visibility;
            GameCamController.Instance.UICamera.enabled = visibility;

        }

        private static void SetUIVisibilityDirectly(bool visibility)
        {
            NotificationManager.instance.NotificationsVisible = visibility;
            GameAreaManager.instance.BordersVisible = visibility;
            DistrictManager.instance.NamesVisible = visibility;
            PropManager.instance.MarkersVisible = visibility;
            GuideManager.instance.TutorialDisabled = !visibility;
            DisasterManager.instance.MarkersVisible = visibility;
            NetManager.instance.RoadNamesVisible = visibility;
            GameCamController.Instance.UICamera.enabled = visibility;
            if (!visibility)
                Object.FindObjectOfType<ToolsModifierControl>().CloseEverything();
        }
    }
}


