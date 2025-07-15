extern alias ACME;
using FPSCamera.Cam.Controller;
using FPSCamera.Utils;
using HarmonyLib;
using static ACME.ACME.CameraPositions;
using static FPSCamera.Utils.MathUtils;
namespace FPSCamera.Patches
{
    [HarmonyPatch]
    internal class ACMEPatches
    {
        private static readonly ACME.ACME.ModSettings ACMESettings = new ACME.ACME.ModSettings();

        [HarmonyPatch(typeof(ACME.ACME.CameraPositions), "SavePosition")]
        [HarmonyPostfix]
        private static void SavePosition(int positionIndex)
        {
            var ControllerPositioning = Positioning.MainCameraPositioning.ToControllerPositioning();
            if (ToolManager.instance.m_properties.m_mode == ItemClass.Availability.Game)
            {

                AccessUtils.GetStaticFieldValue<SavedPosition[]>(typeof(ACME.ACME.CameraPositions), "GameSavedPositions")[positionIndex] = new SavedPosition
                {
                    IsValid = true,
                    Position = ControllerPositioning.pos,
                    Angle = ControllerPositioning.angle,
                    Size = ControllerPositioning.size,
                    Height = ControllerPositioning.height,
                    FOV = GameCamController.Instance.MainCamera.fieldOfView
                };
            }
            else
            {
                ACMESettings.XMLEditorPositions[positionIndex] = new SerializedPosition
                {
                    Index = positionIndex,
                    PosX = ControllerPositioning.pos.x,
                    PosY = ControllerPositioning.pos.y,
                    PosZ = ControllerPositioning.pos.z,
                    AngleX = ControllerPositioning.angle.x,
                    AngleY = ControllerPositioning.angle.y,
                    Size = ControllerPositioning.size,
                    Height = ControllerPositioning.height,
                    FOV = GameCamController.Instance.MainCamera.fieldOfView
                };
            }
        }

        [HarmonyPatch(typeof(ACME.ACME.CameraPositions), "LoadPosition")]
        [HarmonyPostfix]
        private static void LoadPosition(int positionIndex)
        {
            if (FPSCamController.Instance.Status == FPSCamController.CamStatus.Disabled) return;

            var savedPosition =
                (ToolManager.instance.m_properties.m_mode == ItemClass.Availability.Game) ?
                AccessUtils.GetStaticFieldValue<SavedPosition[]>(typeof(ACME.ACME.CameraPositions), "GameSavedPositions")[positionIndex] :
                AccessUtils.GetStaticFieldValue<SavedPosition[]>(typeof(ACME.ACME.CameraPositions), "EditorSavedPositions")[positionIndex];
            if (savedPosition.IsValid)
            {
                var positioning = new ControllerPositioning
                {
                    pos = savedPosition.Position,
                    angle = savedPosition.Angle,
                    size = savedPosition.Size,
                    height = savedPosition.Height,
                }.ToPositioning();
                GameCamController.Instance.transitionEndPositioning = positioning;
                ModSupport.ACMEDisabling = true;
                FPSCamController.Instance.FPSCam = null;
            }
        }
    }
}
