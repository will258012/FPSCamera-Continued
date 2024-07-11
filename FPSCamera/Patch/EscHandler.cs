namespace FPSCamera.Patch
{
    using HarmonyLib;
    [HarmonyPatch]
    internal class EscHandler
    {
        [HarmonyPatch(typeof(GameKeyShortcuts), "Escape")]
        [HarmonyPrefix]
        public static bool EscapePatch()
        {
            // cancel calling <Escape> if FPSCamera consumes it
            var controller = CSkyL.Game.CamController.instance?.GetComponent<Controller>();

            if (controller != null && controller.OnEsc()) return false;

            return true;

        }
    }
}
