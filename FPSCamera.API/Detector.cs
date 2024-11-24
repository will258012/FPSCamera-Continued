using AlgernonCommons;
using AlgernonCommons.Notifications;
using AlgernonCommons.Translation;
using HarmonyLib;

namespace FPSCameraAPI
{
    public class Detector
    {
        public static void CheckFPSCamera()
        {
            if (AccessTools.TypeByName("FPSCamera.Utils.ModSupport, FPSCamera") == null)
            {
                Logging.Error("FPScamera not detected");

                var notification = NotificationBase.ShowNotification<ListNotification>();
                notification.AddParas(Translations.Translate("FPSCAMERA_NOT_DETECTED"));
            }
        }
    }
}