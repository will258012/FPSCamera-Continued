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
        public static GameCamController Instance
        {
            get
            {
                if (_instance is null || _instance._controller == null)
                {
                    _instance = new GameCamController();
                    if (_instance._controller is null) _instance = null;
                }
                return _instance;
            }
        }
        private static GameCamController _instance = null;

        public Camera MainCamera => _mainCamera;
        public CameraController CameraController => _controller;

        public TComp AddComponent<TComp>() where TComp : MonoBehaviour
            => _controller.gameObject.AddComponent<TComp>();
        public TComp GetComponent<TComp>() where TComp : MonoBehaviour
            => _controller.gameObject.GetComponent<TComp>();
        public void Initialize()
        {
            _controller.enabled = false;
            _mainCamera.rect = CameraController.kFullScreenRect;
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

            if (ModSettings.SetBackCamera) _cachedPositioning = new Positioning(_mainCamera.transform.position, _mainCamera.transform.rotation);

            _cachedfieldOfView = _mainCamera.fieldOfView;
            _mainCamera.fieldOfView = ModSettings.CamFieldOfView;
            _cachednearClipPlane = _mainCamera.nearClipPlane;
            _mainCamera.nearClipPlane = ModSettings.CamNearClipPlane;
        }

        public void Restore()
        {
            if (_camDoF != null) _camDoF.enabled = IsDoFEnabled;
            if (_camTiltEffect != null) _camTiltEffect.enabled = IsTiltEffectEnabled;
            _mainCamera.fieldOfView = _cachedfieldOfView;
            _mainCamera.nearClipPlane = _cachednearClipPlane;

            if (!ModSettings.SetBackCamera)
            {
                _controller.m_currentPosition = _controller.m_targetPosition = _mainCamera.transform.position;
                _controller.m_currentAngle = _controller.m_targetAngle = new Vector2(_mainCamera.transform.eulerAngles.y, _mainCamera.transform.eulerAngles.x).ClampEulerAngles();
                _controller.m_currentHeight = _controller.m_targetHeight = _mainCamera.transform.position.y;
            }
            _controller.enabled = true;
        }
        private GameCamController()
        {
            _controller = ToolsModifierControl.cameraController;
            if (_controller == null) return;

            _mainCamera = PrivateField.GetValue<Camera>(_controller, "m_camera");

            _camDoF = GetComponent<DepthOfField>();
            _camTiltEffect = GetComponent<TiltShiftEffect>();
        }

        private readonly CameraController _controller;
        private readonly Camera _mainCamera;

        private readonly DepthOfField _camDoF;
        private readonly TiltShiftEffect _camTiltEffect;

        private bool IsDoFEnabled => !CameraController.isDepthOfFieldDisabled;
        private bool IsTiltEffectEnabled => !CameraController.isTiltShiftDisabled;

        internal Positioning _cachedPositioning;
        private float _cachedfieldOfView;
        private float _cachednearClipPlane;
    }
}