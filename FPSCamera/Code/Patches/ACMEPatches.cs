extern alias ACME;
using AlgernonCommons.Translation;
using FPSCamera.Cam.Controller;
using FPSCamera.Settings;
using FPSCamera.UI;
using FPSCamera.Utils;
using HarmonyLib;
using static ACME.ACME.CameraPositions;
using static FPSCamera.Utils.MathUtils;
namespace FPSCamera.Patches
{
    [HarmonyPatch]
    internal class ACMEPatches
    {
        private static readonly ACME.ACME.ModSettings ACMESettings = new();

        [HarmonyPatch(typeof(ACME.ACME.CameraPositions), "SavePosition")]
        [HarmonyPostfix]
        [AccessUtils.UsedReflection]
        private static void SavePosition(int positionIndex)
        {
            if (FPSCamController.Instance == null || FPSCamController.Instance.Status == FPSCamController.CamStatus.Disabled) return;

            var ControllerPositioning = Positioning.MainCameraPositioning.ToControllerPositioning();
            if (ToolManager.instance.m_properties.m_mode == ItemClass.Availability.Game)
            {

                AccessUtils.GetStaticFieldValue<SavedPosition[]>(typeof(ACME.ACME.CameraPositions), "GameSavedPositions")[positionIndex] = new SavedPosition
                {
                    IsValid = true,
                    Position = ControllerPositioning.pos,
                    Angle = ControllerPositioning.targetAngle,
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
                    AngleX = ControllerPositioning.targetAngle.x,
                    AngleY = ControllerPositioning.targetAngle.y,
                    Size = ControllerPositioning.size,
                    Height = ControllerPositioning.height,
                    FOV = GameCamController.Instance.MainCamera.fieldOfView
                };
                ACME.ACME.Mod.Instance?.SaveSettings();
            }

            CamInfoPanel.Instance?.SetFooterMessage(string.Format(Translations.Translate("INFO_ACMEPOSSAVED"), positionIndex));
        }

        [HarmonyPatch(typeof(ACME.ACME.CameraPositions), "LoadPosition")]
        [HarmonyPostfix]
        [AccessUtils.UsedReflection]
        private static void LoadPosition(int positionIndex)
        {
            if (FPSCamController.Instance == null || FPSCamController.Instance.Status == FPSCamController.CamStatus.Disabled) return;

            var savedPosition =
                (ToolManager.instance.m_properties.m_mode == ItemClass.Availability.Game) ?
                AccessUtils.GetStaticFieldValue<SavedPosition[]>(typeof(ACME.ACME.CameraPositions), "GameSavedPositions")[positionIndex] :
                AccessUtils.GetStaticFieldValue<SavedPosition[]>(typeof(ACME.ACME.CameraPositions), "EditorSavedPositions")[positionIndex];
            if (!savedPosition.IsValid)
                return;

            var positioning = new ControllerPositioning
            {
                pos = savedPosition.Position,
                targetAngle = savedPosition.Angle,
                size = savedPosition.Size,
                height = savedPosition.Height,
            }.ToPositioning();

            if (ModSettings.ACMEBehavior == ModSettings.ACMEBehaviors.FreeCam)
            {
                FPSCamController.Instance.StartFreeCam(positioning, savedPosition.FOV.Clamp(FPSCamController.MinFoV, FPSCamController.MaxFoV));
            }
            else
            {
                GameCamController.Instance.transitionEndPositioning = positioning;
                GameCamController.Instance.savedFoV = savedPosition.FOV;
                FPSCamController.Instance.OverrideSetBackCamera = FPSCamController.OverrideSetBack.ACME;
                FPSCamController.Instance.FPSCam = null;
            }
        }
    }
}
