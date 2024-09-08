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
                if (_instance is null || _instance.CameraController == null)
                {
                    _instance = new GameCamController();
                    if (_instance.CameraController is null) _instance = null;
                }
                return _instance;
            }
        }
        private static GameCamController _instance = null;

        public Camera MainCamera => Camera.main;
        public CameraController CameraController => ToolsModifierControl.cameraController;
        private bool IsDoFEnabled => !CameraController.isDepthOfFieldDisabled;
        private bool IsTiltEffectEnabled => !CameraController.isTiltShiftDisabled;

        public TComp AddComponent<TComp>() where TComp : MonoBehaviour
            => CameraController.gameObject.AddComponent<TComp>();
        public TComp GetComponent<TComp>() where TComp : MonoBehaviour
            => CameraController.gameObject.GetComponent<TComp>();
        public void Initialize()
        {
            CameraController.enabled = false;
            MainCamera.rect = CameraController.kFullScreenRect;
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
            if (ModSettings.SetBackCamera) _cachedPositioning = new Positioning(MainCamera.transform.position, MainCamera.transform.rotation);

            _cachedfieldOfView = MainCamera.fieldOfView;
            MainCamera.fieldOfView = ModSettings.CamFieldOfView;
            _cachednearClipPlane = MainCamera.nearClipPlane;
            MainCamera.nearClipPlane = ModSettings.CamNearClipPlane;
        }

        public void Restore()
        {
            if (_camDoF != null) _camDoF.enabled = IsDoFEnabled;
            if (_camTiltEffect != null) _camTiltEffect.enabled = IsTiltEffectEnabled;
            MainCamera.fieldOfView = _cachedfieldOfView;
            MainCamera.nearClipPlane = _cachednearClipPlane;

            if (!ModSettings.SetBackCamera)
            {
                CameraController.m_currentPosition = CameraController.m_targetPosition = MainCamera.transform.position;
                CameraController.m_currentAngle = CameraController.m_targetAngle = new Vector2(MainCamera.transform.eulerAngles.y, MainCamera.transform.eulerAngles.x).ClampEulerAngles();
                CameraController.m_currentHeight = CameraController.m_targetHeight = MainCamera.transform.position.y;
            }
            CameraController.enabled = true;
        }
        private GameCamController()
        {
            if (CameraController == null) return;

            _camDoF = GetComponent<DepthOfField>();
            _camTiltEffect = GetComponent<TiltShiftEffect>();
        }
        private readonly DepthOfField _camDoF;
        private readonly TiltShiftEffect _camTiltEffect;

        internal Positioning _cachedPositioning;
        private float _cachedfieldOfView;
        private float _cachednearClipPlane;
    }
}