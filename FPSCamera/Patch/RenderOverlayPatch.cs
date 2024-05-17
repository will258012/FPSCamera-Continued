#if DEBUG
namespace FPSCamera.Patch
{
    using HarmonyLib;
    [HarmonyPatch]
    internal class RenderOverlay
    {
        [HarmonyPatch(typeof(DefaultTool), "RenderOverlay")]
        [HarmonyPostfix]
        public static void RenderOverlayPatch(RenderManager.CameraInfo cameraInfo)
        {
            ThreadingExtension.Controller?.RenderOverlay(cameraInfo);
        }
    }
}
#endif
