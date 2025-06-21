using FPSCamera.Settings;
using FPSCamera.Utils;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using static FPSCamera.Utils.MathUtils;
namespace FPSCamera.Cam.Controller
{
    /// <summary>
    /// Control game's Camera.
    /// </summary>
    public class GameCamController
    {
        /// <summary>
        /// Gets the active instance reference of <see cref="GameCamController"/>.
        /// </summary>
        public static GameCamController Instance
        {
            get
            {
                if (_instance == null || _instance.CameraController == null)
                {
                    _instance = new GameCamController();
                    if (_instance.CameraController == null) _instance = null;
                }
                return _instance;
            }
        }
        private static GameCamController _instance = null;
        /// <summary>
        /// public constructor for the <see cref="GameCamController"/>.
        /// </summary>
        public GameCamController()
        {
            camDoF = GetComponent<DepthOfField>();
            camTiltEffect = GetComponent<TiltShiftEffect>();
        }

        /// <summary>
        /// Gets the game's main camera instance.
        /// </summary>
        public Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                    _mainCamera = AccessUtils.GetFieldValue<Camera>(CameraController, "m_camera");
                return _mainCamera;
            }
        }
        private Camera _mainCamera = null;
        /// <summary>
        /// Gets the current <see cref="global::CameraController"/>.
        /// </summary>
        public CameraController CameraController => ToolsModifierControl.cameraController;

        /// <summary>
        /// Checks Dof status.
        /// </summary>
        public bool IsDoFEnabled => !CameraController.isDepthOfFieldDisabled;

        /// <summary>
        /// Checks Tilt Shift status.
        /// </summary>
        public bool IsTiltEffectEnabled => !CameraController.isTiltShiftDisabled;
        /// <summary>
        /// Gets a component from the CameraController.
        /// </summary>
        /// <typeparam name="TComp">The type of component to get.</typeparam>
        /// <returns>The instance of the retrieved component.</returns>
        public TComp GetComponent<TComp>() where TComp : MonoBehaviour
            => CameraController.gameObject.GetComponent<TComp>();
        /// <summary>
        /// Initializes the camera settings when FPS Camera is enabled.
        /// </summary>
        public void Initialize()
        {
            CameraController.enabled = false;
            ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.SetTool<DefaultTool>();
            if (ModSettings.HideGameUI)
            {
                savedRect = Camera.main.rect;//need to control Camera.main instead of MainCamera we got, fixed for Dynamic Resolution
                Camera.main.rect = CameraController.kFullScreenRect;
            }
            if (camTiltEffect != null) camTiltEffect.enabled = false;
            if (ModSettings.Dof)
            {
                if (camDoF != null)
                    camDoF.enabled = true;
            }
            else
            {
                if (camDoF != null && IsDoFEnabled)
                    camDoF.enabled = false;
            }
            if (ModSettings.SetBackCamera)
            {
                transitionEndPositioning = Positioning.MainCameraPositioning;
                savedControllerPositioning = ControllerPositioning.Save();
            }

            savedFoV = MainCamera.fieldOfView;
            MainCamera.fieldOfView = ModSettings.CamFieldOfView;
            savedNearClipPlane = MainCamera.nearClipPlane;
            MainCamera.nearClipPlane = ModSettings.CamNearClipPlane;
        }
        /// <summary>
        /// Restores the camera settings to their initial state when FPS Camera is disabled.
        /// </summary>
        public void Restore()
        {
            if (camDoF != null) camDoF.enabled = IsDoFEnabled;
            if (camTiltEffect != null) camTiltEffect.enabled = IsTiltEffectEnabled;

            MainCamera.fieldOfView = savedFoV;
            MainCamera.nearClipPlane = savedNearClipPlane;
            if (ModSettings.HideGameUI)
                Camera.main.rect = savedRect;

            if (!ModSupport.ACMEDisabling)
                if (ModSettings.SetBackCamera && CameraController.GetTarget().IsEmpty)
                {
                    savedControllerPositioning.Load();
                    MainCamera.transform.position = transitionEndPositioning.pos;
                    MainCamera.transform.rotation = transitionEndPositioning.rotation;
                }
                else
                    Positioning.MainCameraPositioning.ToControllerPositioning().Load();

            if (ModSupport.FoundACME)
            {
                ModSupport.ACMEDisabling = false;
                ModSupport.ACME_DisableFPSMode();
            }
            transitionEndPositioning = default;
            CameraController.enabled = true;
        }

        private readonly DepthOfField camDoF;
        private readonly TiltShiftEffect camTiltEffect;

        internal Positioning transitionEndPositioning;
        private ControllerPositioning savedControllerPositioning;
        private Rect savedRect = CameraController.kFullScreenWithoutMenuBarRect;
        private float savedFoV;
        private float savedNearClipPlane;
    }
}