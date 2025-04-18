using AlgernonCommons;
using ColossalFramework;
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
            CameraController.ClearTarget();
            CameraController.enabled = false;
            if (ModSettings.HideGameUI)
            {
                cachedRect = Camera.main.rect;//need to control Camera.main instead of MainCamera we got, fixed for Dynamic Resolution
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
                cachedPositioning = new Positioning(MainCamera.transform.position, MainCamera.transform.rotation);
                savedCameraView = new CameraController.SavedCameraView(CameraController);
            }

            cachedFoV = MainCamera.fieldOfView;
            MainCamera.fieldOfView = ModSettings.CamFieldOfView;
            cachedNearClipPlane = MainCamera.nearClipPlane;
            MainCamera.nearClipPlane = ModSettings.CamNearClipPlane;
        }
        /// <summary>
        /// Restores the camera settings to their initial state when FPS Camera is disabled.
        /// </summary>
        public void Restore()
        {
            if (camDoF != null) camDoF.enabled = IsDoFEnabled;
            if (camTiltEffect != null) camTiltEffect.enabled = IsTiltEffectEnabled;

            MainCamera.fieldOfView = cachedFoV;
            MainCamera.nearClipPlane = cachedNearClipPlane;
            if (ModSettings.HideGameUI)
                Camera.main.rect = cachedRect;

            if (ModSettings.SetBackCamera)
            {
                ResetFromSavedCameraView(savedCameraView);
                cachedPositioning = new Positioning(Vector3.zero);
                savedCameraView = default;
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
        /// 
        /// </summary>
        public void SyncCameraControllerFromTransform()
        {
            // Calculate the ground height.
            var height = MapUtils.GetMinHeightAt(MainCamera.transform.position);

            // Set the target angle based on the camera's transform, clamping the angles to valid ranges.
            CameraController.m_currentAngle = CameraController.m_targetAngle =
                new Vector2(MainCamera.transform.rotation.eulerAngles.y, MainCamera.transform.rotation.eulerAngles.x).ClampEulerAngles();

            // Calculate the new size (height difference between the camera height and the ground, with some adjust by angles)
            var newSize = Mathf.Max(0f, MainCamera.transform.position.y - height)
                / Mathf.Lerp(0.15f, 1f, Mathf.Sin(Mathf.Abs(CameraController.m_currentAngle.y) * Mathf.Deg2Rad));
            newSize = newSize.Clamp(CameraController.m_minDistance, CameraController.m_maxDistance);

            // Set the size if necessary.
            if (Mathf.Abs(newSize - CameraController.m_targetSize) >= 100f)
                CameraController.m_targetSize = CameraController.m_currentSize = newSize;

            // Calculate m_targetAngle.y if necessary.
            if (!ToolManager.instance.m_properties.m_mode.IsFlagSet(ItemClass.Availability.ThemeEditor)
                && !CameraController.m_unlimitedCamera)//This value would be set to true if there's another camera mod, such as ACME.
            {
                // Calculate m_targetAngle.y, which has tilt limit. Derive from the following formula:
                // m_currentAngle.y = (90f - (90f - m_targetAngle.y) * (m_maxTiltDistance * 0.5f / (m_maxTiltDistance * 0.5f + m_targetSize)))
                var newTargetAngleY =
                    -((180f * CameraController.m_targetSize
                    - CameraController.m_currentAngle.y * CameraController.m_maxTiltDistance
                    - 2f * CameraController.m_currentAngle.y * CameraController.m_targetSize)
                    / CameraController.m_maxTiltDistance);
                CameraController.m_targetAngle.y = newTargetAngleY;
            }
            var distance = 0f;
            var newPos = MainCamera.transform.position;
            // Calculate CameraController position based on the camera's transform position.
            for (int i = 1; i <= 3; i++)
            {
                height = TerrainManager.instance.SampleRawHeightSmoothWithWater(newPos, true, 2f);
                newPos.y = height + CameraController.m_targetSize * 0.05f + 10f;
                //Calculate distance (Z offset).
                distance = CameraController.m_targetSize
                            * Mathf.Max(0f, 1f - height / CameraController.m_maxDistance)
                            / Mathf.Tan(MainCamera.fieldOfView * Mathf.Deg2Rad);
                newPos = MainCamera.transform.position + (MainCamera.transform.forward * distance);
                // Limit the camera's position to the allowed area.
                newPos = CameraController.ClampCameraPosition(newPos);
                if (newPos.sqrMagnitude < 0.0001f)
                {
                    break;
                }
            }

            // Update CameraController's position and height if necessary.
            if (Vector3.Distance(newPos, CameraController.m_targetPosition) >= 10f)
                CameraController.m_targetPosition = CameraController.m_currentPosition = newPos;
            if (Mathf.Abs(height - CameraController.m_targetHeight) >= 10f)
                CameraController.m_targetHeight = CameraController.m_currentHeight = height;
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
            camDoF = GetComponent<DepthOfField>();
            camTiltEffect = GetComponent<TiltShiftEffect>();
        }
        private readonly DepthOfField camDoF;
        private readonly TiltShiftEffect camTiltEffect;

        internal Positioning cachedPositioning;
        private CameraController.SavedCameraView savedCameraView;
        internal Rect cachedRect = CameraController.kFullScreenWithoutMenuBarRect;

        private float cachedFoV;
        private float cachedNearClipPlane;
    }
}