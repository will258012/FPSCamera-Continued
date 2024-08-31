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
            _cachedRect = _mainCamera.rect;
            _mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
            if (_camTiltEffect != null) _camTiltEffect.enabled = false;
            if (ModSettings.Dof && _camDoF != null) _camDoF.enabled = true;
            if (ModSettings.SetBackCamera)
            {
                _cachedPositioning = new Positioning(_mainCamera.transform.position, _mainCamera.transform.rotation);
            }
            _cachedfieldOfView = _mainCamera.fieldOfView;
            _mainCamera.fieldOfView = ModSettings.CamFieldOfView;
            _cachednearClipPlane = _mainCamera.nearClipPlane;
            _mainCamera.nearClipPlane = ModSettings.CamNearClipPlane;
        }

        public void Restore()
        {
            if (_camDoF != null) _camDoF.enabled = _IsDoFEnabled;
            if (_camTiltEffect != null) _camTiltEffect.enabled = _IsTiltEffectEnabled;
            _mainCamera.fieldOfView = _cachedfieldOfView;
            _mainCamera.nearClipPlane = _cachednearClipPlane;

            if (!ModSettings.SetBackCamera)
            {
                _controller.m_currentPosition = _controller.m_targetPosition = _mainCamera.transform.position;
                _controller.m_currentAngle = _controller.m_targetAngle = new Vector2(_mainCamera.transform.eulerAngles.y, _mainCamera.transform.eulerAngles.x);
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

            if (_camDoF != null) _IsDoFEnabled = _camDoF.enabled;
            if (_camTiltEffect != null) _IsTiltEffectEnabled = _camTiltEffect.enabled;
        }

        private readonly CameraController _controller;
        private readonly Camera _mainCamera;

        private readonly DepthOfField _camDoF;
        private readonly TiltShiftEffect _camTiltEffect;

        private readonly bool _IsDoFEnabled;
        private readonly bool _IsTiltEffectEnabled;

        internal Positioning _cachedPositioning;
        internal Rect _cachedRect;
        private float _cachedfieldOfView;
        private float _cachednearClipPlane;
    }
}