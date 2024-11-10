using AlgernonCommons;
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
        /// Gets the instance of <see cref="GameCamController"/>.
        /// If the instance does not exist or <see cref="global::CameraController"/> is null, a new instance is created.
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
        /// Gets the game's main camera instance.
        /// If it has not been initialized, it retrieves the camera instance using reflection.
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
        private bool IsDoFEnabled => !CameraController.isDepthOfFieldDisabled;

        /// <summary>
        /// Checks Tilt Shift status.
        /// </summary>
        private bool IsTiltEffectEnabled => !CameraController.isTiltShiftDisabled;
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
            if (ModSettings.HideGameUI)
            {
                _cachedRect = Camera.main.rect;//need to control Camera.main instead of MainCamera we got, fixed for Dynamic Resolution
                Camera.main.rect = CameraController.kFullScreenRect;
            }
            if (_camTiltEffect != null) _camTiltEffect.enabled = false;
            if (ModSettings.Dof)
            {
                if (_camDoF != null)
                    _camDoF.enabled = true;
            }
            else
            {
                if (_camDoF != null && IsDoFEnabled)
                    _camDoF.enabled = false;
            }
            if (ModSettings.SetBackCamera)
            {
                _cachedPositioning = new Positioning(MainCamera.transform.position, MainCamera.transform.rotation);
                _savedCameraView = new CameraController.SavedCameraView(CameraController);
            }

            _cachedfieldOfView = MainCamera.fieldOfView;
            MainCamera.fieldOfView = ModSettings.CamFieldOfView;
            _cachednearClipPlane = MainCamera.nearClipPlane;
            MainCamera.nearClipPlane = ModSettings.CamNearClipPlane;
        }
        /// <summary>
        /// Restores the camera settings to their initial state when FPS Camera is disabled.
        /// </summary>
        public void Restore()
        {
            if (_camDoF != null) _camDoF.enabled = IsDoFEnabled;
            if (_camTiltEffect != null) _camTiltEffect.enabled = IsTiltEffectEnabled;
            MainCamera.fieldOfView = _cachedfieldOfView;
            MainCamera.nearClipPlane = _cachednearClipPlane;
            if (ModSettings.HideGameUI)
                Camera.main.rect = _cachedRect;

            if (ModSettings.SetBackCamera)
            {
                ResetFromSavedCameraView(_savedCameraView);
                _cachedPositioning = new Positioning(Vector3.zero);
                _savedCameraView = default;
            }
            else
                SyncCameraControllerFromTransform();
            CameraController.enabled = true;
        }
        /// <summary>
        /// Resets the <see cref="CameraController"/>'s properties (position, angle, size, etc.) from a saved camera view.
        /// </summary>
        /// <param name="view">The <see cref="CameraController.SavedCameraView"/> to restore the camera to.</param>
        public void ResetFromSavedCameraView(CameraController.SavedCameraView view)
        {
            CameraController.m_targetSize = CameraController.m_currentSize = view.m_size;
            //CameraController.m_targetAngle = CameraController.m_currentAngle = view.m_angle;
            CameraController.m_targetHeight = CameraController.m_currentHeight = view.m_height;
            CameraController.m_targetPosition = CameraController.m_currentPosition = view.m_position;
        }

        /// <summary>
        /// Synchronizes the camera controller's position and angle with the <see cref="MainCamera"/> transform.
        /// This function adjusts the <see cref="CameraController"/> to match the camera's position and orientation in the scene.
        /// </summary>
        public void SyncCameraControllerFromTransform()
        {
            // Set the size (Z offset) to the minimum value (40f) for the game.
            CameraController.m_targetSize = CameraController.m_currentSize = 40f;

            // Calculate the new position based on the Z offset, moving forward from the camera's transform.
            var newPos = MainCamera.transform.position + (MainCamera.transform.rotation * Vector3.forward * 40f);

            // Limit the camera's position to the allowed area.
            newPos = CameraController.ClampCameraPosition(newPos);

            // Update the CameraController's position based on the adjusted new position.
            CameraController.m_targetPosition = CameraController.m_currentPosition = newPos;

            // Calculate the height based on the new position and the minimum height of the terrain.
            CameraController.m_targetHeight = CameraController.m_currentHeight = newPos.y + CameraController.CalculateCameraHeightOffset(newPos, 40f);

            // Set the target angle based on the camera's transform, clamping the angles to valid ranges.
            CameraController.m_targetAngle = CameraController.m_currentAngle =
                new Vector2(MainCamera.transform.rotation.eulerAngles.y, MainCamera.transform.rotation.eulerAngles.x).ClampEulerAngles();
        }

        /// <summary>
        /// Private constructor for the <see cref="GameCamController"/>.
        /// Initializes components if <see cref="global::CameraController"/> is found.
        /// </summary>
        private GameCamController()
        {
            if (CameraController == null)
            {
                Logging.Error("CameraController is not found");
                return;
            }
            _camDoF = GetComponent<DepthOfField>();
            _camTiltEffect = GetComponent<TiltShiftEffect>();
        }
        private readonly DepthOfField _camDoF;
        private readonly TiltShiftEffect _camTiltEffect;

        internal Positioning _cachedPositioning;
        private CameraController.SavedCameraView _savedCameraView;
        internal Rect _cachedRect = CameraController.kFullScreenWithoutMenuBarRect;

        private float _cachedfieldOfView;
        private float _cachednearClipPlane;
    }
}