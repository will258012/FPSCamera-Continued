using AlgernonCommons;
using AlgernonCommons.Notifications;
using AlgernonCommons.Translation;
using HarmonyLib;

namespace FPSCameraAPI
{
    public class Detector
    {
        public static bool CheckFPSCamera()
        {
            if (AssemblyUtils.GetEnabledAssembly("FPSCamera") == null ||
                AccessTools.TypeByName("FPSCamera.Utils.ModSupport, FPSCamera") == null)
            {
                Logging.Error("FPSCamera not detected");
                return false;
            }
            return true;
        }
        public static void ShowNotificationWhenFPSCameraIsNotDetected()
        {
            if (!CheckFPSCamera())
            {
                var notification = NotificationBase.ShowNotification<ListNotification>();
                notification.AddParas(Translations.Translate("FPSCAMERA_NOT_DETECTED"));
            }
        }
    }
}