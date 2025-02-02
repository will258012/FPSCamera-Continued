using AlgernonCommons;
using AlgernonCommons.Notifications;
using AlgernonCommons.Translation;
using ColossalFramework;
using FPSCamera.Game;
using FPSCamera.Settings;
using FPSCamera.UI;
using FPSCamera.Utils;
using System;
using UnityEngine;
using static FPSCamera.Utils.MathUtils;

namespace FPSCamera.Cam.Controller
{
    /// <summary>
    /// Control Mod's Camera.
    /// </summary>
    public class FPSCamController : MonoBehaviour
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="FPSCamController"/>.
        /// </summary>
        public static FPSCamController Instance { get; private set; }

        /// <summary>
        /// Gets the current FPS camera instance.
        /// </summary>
        public IFPSCam FPSCam { get; set; } = null;

        /// <summary>
        /// Gets the current status of the camera.
        /// Useful for status determination.
        /// </summary>
        public CamStatus Status { get; private set; } = CamStatus.Disabled;
        /// <summary>
        /// Invoked after FPS Camera is enabled.
        /// </summary>
        public static Action OnCameraEnabled { get; set; }
        /// <summary>
        /// Invoked after FPS Camera is disabled.
        /// </summary>
        public static Action OnCameraDisabled { get; set; }
        /// <summary>
        /// Called when the script instance is being loaded. Initializes the singleton instance.
        /// </summary>
        private void Awake() => Instance = this;

        /// <summary>
        /// Enables the camera and associated UI elements or settings.
        /// </summary>
        public void EnableCam(bool IsPlugin = false)
        {
            OffsetsSettings.Load();
            if (ModSettings.ShowInfoPanel)
                CamInfoPanel.Instance.enabled = true;
            if (ModSettings.HideGameUI)
                StartCoroutine(UIManager.ToggleUI(false));
            if (ModSettings.LodOpt != 0)
                StartCoroutine(LodManager.ToggleLODOpt(true));
            if (ModSettings.ShadowsOpt)
                StartCoroutine(ShadowsManager.ToggleShadowsOpt(true));
            GameCamController.Instance.Initialize();
            Status = IsPlugin ? CamStatus.PluginEnabled : CamStatus.Enabled;
            OnCameraEnabled?.Invoke();
        }

        /// <summary>
        /// Disables the camera and restores the original camera settings.
        /// </summary>
        private void DisableCam()
        {
            Logging.KeyMessage("Disabling FPS Camera");
            Status = CamStatus.Disabling;
            FPSCam?.DisableCam();
            FPSCam = null;
            if (ModSettings.ShowInfoPanel)
                CamInfoPanel.Instance.enabled = false;
            if (ModSettings.LodOpt != 0)
                StartCoroutine(LodManager.ToggleLODOpt(false));
            if (ModSettings.ShadowsOpt)
                StartCoroutine(ShadowsManager.ToggleShadowsOpt(false));

            if (ModSettings.SetBackCamera)
            {
                if (ModSettings.SmoothTransition)
                    StartTransitioningOnDisabled(GameCamController.Instance._cachedPositioning);
                else AfterTransition(GameCamController.Instance._cachedPositioning);
            }
            else
            {
                if (ModSettings.HideGameUI)
                    StartCoroutine(UIManager.ToggleUI(true));
                GameCamController.Instance.Restore();
                Status = CamStatus.Disabled;
            }
            OnCameraDisabled?.Invoke();
        }
        private void AfterTransition(Positioning positioning)
        {
            if (ModSettings.HideGameUI)
                StartCoroutine(UIManager.ToggleUI(true));
            GameCamController.Instance.MainCamera.transform.position = positioning.pos;
            GameCamController.Instance.MainCamera.transform.rotation = positioning.rotation;
            GameCamController.Instance.Restore();
            Status = CamStatus.Disabled;
        }

        /// <summary>
        /// Starts Follow mode.
        /// </summary>
        /// <param name="instanceID">The <see cref="InstanceID"/> to follow.</param>
        public void StartFollowing(InstanceID instanceID)
        {
            Logging.KeyMessage("Starting Follow mode");

            if (instanceID.Type == InstanceType.Vehicle)
                FPSCam = new VehicleCam(instanceID);
            else if (instanceID.Type == InstanceType.Citizen || instanceID.Type == InstanceType.CitizenInstance)
                FPSCam = new CitizenCam(instanceID);
            else return;

            EnableCam();
            (FPSCam as IFollowCam).SyncCamOffset();
        }

        /// <summary>
        /// Starts Free-camera mode.
        /// </summary>
        public void StartFreeCam()
        {
            Logging.KeyMessage("Starting Free-Camera mode");
            FPSCam = new FreeCam();
            EnableCam();
            _offset = new Positioning(Vector3.zero, GameCamController.Instance.MainCamera.transform.rotation);
        }

        /// <summary>
        /// Starts Walk-through mode.
        /// </summary>
        public void StartWalkThruCam()
        {
            Logging.KeyMessage("Starting Walk-Through mode");
            FPSCam = new WalkThruCam();
            EnableCam();

        }
        /// <summary>
        /// Starts transitioning when the camera needs to be disabled.
        /// </summary>
        /// <param name="endPos">where to end transitioning.</param>
        public void StartTransitioningOnDisabled(Positioning endPos)
        {
            _transitionTimer = 0f;
            _targetFoV = ModSettings.CamFieldOfView;
            if (CameraTransform.position.DistanceTo(endPos.pos) <= 2f ||
                CameraTransform.position.DistanceTo(endPos.pos) > ModSettings.MaxTransDistance)
            {
                AfterTransition(endPos);
                return;
            }
            Status = CamStatus.Transitioning;
            _endPos = endPos;
        }
        /// <summary>
        /// Updates the camera state each frame.
        /// </summary>
        private void Update()
        {
            try
            {
                if (Status == CamStatus.Enabled || Status == CamStatus.PluginEnabled)
                {
                    if (!(FPSCam?.IsValid() ?? false))
                    {
                        DisableCam();
                        return;
                    }
                    if (FPSCam is WalkThruCam walkThruCam)
                        walkThruCam.ElapseTime(Time.deltaTime);
                }
            }
            catch (Exception e)
            {
                if (Status == CamStatus.Enabled || Status == CamStatus.PluginEnabled)
                {
                    Logging.Error("FPS Camera is about to exit due to some issues (Update)");
                    DisableCam();
                }

                var notification = NotificationBase.ShowNotification<ListNotification>();
                notification.AddParas(Translations.Translate("ERROR"));
                notification.AddSpacer();
                notification.AddParas(e.ToString());
                Logging.LogException(e);
            }
        }

        /// <summary>
        /// Updates the camera state after all other updates are complete.
        /// </summary>
        private void LateUpdate()
        {
            try
            {
                HandleInput();
                if (Status == CamStatus.Transitioning)
                {
                    UpdateTransitionPos();
                }
                else if (Status == CamStatus.Enabled)
                {
                    if (FPSCam is IFollowCam)
                        UpdateFollowCamPos();
                    else if (FPSCam is FreeCam freeCam)
                        UpdateFreeCamPos(freeCam);
                }
            }
            catch (Exception e)
            {
                if (Status == CamStatus.Enabled || Status == CamStatus.PluginEnabled)
                {
                    Logging.Error("FPS Camera is about to exit due to some issues (LateUpdate)");
                    DisableCam();
                }
                var notification = NotificationBase.ShowNotification<ListNotification>();
                notification.AddParas(Translations.Translate("ERROR"));
                notification.AddSpacer();
                notification.AddParas(e.ToString());
                Logging.LogException(e);
            }
        }

        /// <summary>
        /// Handles the escape key press to disable the camera.
        /// </summary>
        /// <returns>True if the camera was disabled, false otherwise.</returns>
        public bool OnEsc()
        {
            if (Status == CamStatus.Enabled || Status == CamStatus.PluginEnabled)
            {
                DisableCam();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles user input for camera control.
        /// </summary>
        private void HandleInput()
        {
            if (InputManager.KeyTriggered(ModSettings.KeyCamToggle) &&
                !SimulationManager.instance.ForcedSimulationPaused && // when the game isn't in the pause menu
                !GameCamController.Instance.CameraController.m_freeCamera) // when isn't in the game's free mode
            {
                if (Status == CamStatus.Enabled || Status == CamStatus.PluginEnabled) DisableCam();
                else if (Status == CamStatus.Disabled) StartFreeCam();
            }
            if (Status != CamStatus.Enabled) return;

            if (InputManager.MouseTriggered(InputManager.MouseButton.Middle) ||
                InputManager.KeyTriggered(ModSettings.KeyCamReset))
            {
                (FPSCam as IFollowCam)?.SyncCamOffset();
                if (ModSettings.SmoothTransition)
                    _targetFoV = ModSettings.CamFieldOfView;
                else
                    GameCamController.Instance.MainCamera.fieldOfView = ModSettings.CamFieldOfView;
            }

            if (InputManager.MouseTriggered(InputManager.MouseButton.Secondary) && ModSettings.ManualSwitchWalk)
                (FPSCam as WalkThruCam)?.SwitchTarget();
            if (InputManager.KeyTriggered(ModSettings.KeyAutoMove))
                (FPSCam as FreeCam)?.ToggleAutoMove();
            if (InputManager.KeyTriggered(ModSettings.KeySaveOffset))
                (FPSCam as IFollowCam)?.SaveCamOffset();

            { // key movement
                var movementFactor = (InputManager.KeyPressed(ModSettings.KeySpeedUp) ? ModSettings.SpeedUpFactor : 1f)
                                     * ModSettings.MovementSpeed * Time.deltaTime / MapUtils.ToKilometer(1f);

                var LocalMovement = Vector3.zero;
                if (InputManager.KeyPressed(ModSettings.KeyMoveForward)) LocalMovement += Vector3.forward * movementFactor;
                if (InputManager.KeyPressed(ModSettings.KeyMoveBackward)) LocalMovement += Vector3.back * movementFactor;
                if (InputManager.KeyPressed(ModSettings.KeyMoveRight)) LocalMovement += Vector3.right * movementFactor;
                if (InputManager.KeyPressed(ModSettings.KeyMoveLeft)) LocalMovement += Vector3.left * movementFactor;
                if (InputManager.KeyPressed(ModSettings.KeyMoveUp)) LocalMovement += Vector3.up * movementFactor;
                if (InputManager.KeyPressed(ModSettings.KeyMoveDown)) LocalMovement += Vector3.down * movementFactor;

                _offset.pos += _offset.rotation * LocalMovement;
            }


            var cursorVisible = InputManager.KeyPressed(ModSettings.KeyCursorToggle) ^ (
                                FPSCam is FreeCam ? ModSettings.ShowCursorFree
                                                    : ModSettings.ShowCursorFollow);
            InputManager.ToggleCursor(cursorVisible);

            float yawDegree = 0f, pitchDegree = 0f;
            { // key rotation
                var rotateFactor = ModSettings.RotateKeyFactor * Time.deltaTime;

                if (InputManager.KeyPressed(ModSettings.KeyRotateRight)) yawDegree += 1f * rotateFactor;
                if (InputManager.KeyPressed(ModSettings.KeyRotateLeft)) yawDegree -= 1f * rotateFactor;
                if (InputManager.KeyPressed(ModSettings.KeyRotateUp)) pitchDegree -= 1f * rotateFactor;
                if (InputManager.KeyPressed(ModSettings.KeyRotateDown)) pitchDegree += 1f * rotateFactor;

                if (yawDegree == 0f && pitchDegree == 0f && !cursorVisible)
                {
                    // mouse rotation
                    const float mouseFactor = .2f;
                    yawDegree = InputManager.MouseMoveHori * ModSettings.RotateSensitivity *
                                (ModSettings.InvertRotateHorizontal ? -1f : 1f) * mouseFactor;
                    pitchDegree = InputManager.MouseMoveVert * ModSettings.RotateSensitivity *
                                  (ModSettings.InvertRotateVertical ? 1f : -1f) * mouseFactor;
                }
            }
            var yawRotation = Quaternion.Euler(0f, yawDegree, 0f);
            var pitchRotation = Quaternion.Euler(pitchDegree, 0f, 0f);
            _offset.rotation = yawRotation * _offset.rotation * pitchRotation;

            // Limit pitch
            var eulerAngles = _offset.rotation.eulerAngles;
            if (eulerAngles.x > 180f) eulerAngles.x -= 360f;
            eulerAngles.x = eulerAngles.x.Clamp(-ModSettings.MaxPitchDeg, ModSettings.MaxPitchDeg);
            eulerAngles.z = 0f;
            _offset.rotation = Quaternion.Euler(eulerAngles);

            // scroll zooming
            var scroll = InputManager.MouseScroll;
            if (ModSettings.SmoothTransition)
            {
                var nowFoV = GameCamController.Instance.MainCamera.fieldOfView;
                if (scroll > 0f && nowFoV > 10f)
                    _targetFoV = nowFoV / ModSettings.FoViewScrollfactor;
                else if (scroll < 0f && nowFoV < 75f)
                    _targetFoV = nowFoV * ModSettings.FoViewScrollfactor;
                _isScrollTransitioning = true;
            }
            else
            {
                var FoV = GameCamController.Instance.MainCamera.fieldOfView;

                if (scroll > 0f && FoV > 10f)
                    GameCamController.Instance.MainCamera.fieldOfView = FoV / ModSettings.FoViewScrollfactor;
                else if (scroll < 0f && FoV < 75f)
                    GameCamController.Instance.MainCamera.fieldOfView = FoV * ModSettings.FoViewScrollfactor;
            }
            if (_isScrollTransitioning)
            {
                if (GameCamController.Instance.MainCamera.fieldOfView.AlmostEquals(_targetFoV))
                {
                    GameCamController.Instance.MainCamera.fieldOfView = _targetFoV;
                    _isScrollTransitioning = false;
                }
                GameCamController.Instance.MainCamera.fieldOfView = Mathf.Lerp(GameCamController.Instance.MainCamera.fieldOfView, _targetFoV, Time.deltaTime * ModSettings.TransSpeed);
            }
        }
        /// <summary>
        /// Saves the given camera offset.
        /// </summary>
        /// <param name="followCam">Given camera.</param>
        internal void SaveCamOffset(IFollowCam followCam)
        {
            var name = followCam?.GetPrefabName();
            if (name != null)
            {
                OffsetsSettings.Offsets[name] = _offset;
                OffsetsSettings.Save();
                CamInfoPanel.Instance.SetFooterMessage(string.Format(Translations.Translate("INFO_OFFSETSAVED"), name));
                Logging.Message($"Saved offset and rotation for \"{name}\"");
            }
        }
        /// <summary>
        /// Synchronizes the camera offset based on the given camera.
        /// </summary>
        /// <param name="followCam">Given camera.</param>
        internal void SyncCamOffset(IFollowCam followCam)
        {
            var name = followCam?.GetPrefabName();
            _offset = default;
            _offsetFromSetting = default;

            if (name != null && OffsetsSettings.Offsets.TryGetValue(name, out var offset))
            {
                _offset = offset;
            }

            _offsetFromSetting += ModSettings.FollowCamOffset;
            if (followCam is CitizenCam)
                _offsetFromSetting += ModSettings.PedestrianFixedOffset;
            else if (followCam is VehicleCam cam)
            {
                if (cam.GetVehicle().m_leadingVehicle != default && cam.GetPrefabName() != VehicleCam.GetVehicle(cam.GetFrontVehicleID()).Info.name)
                    _offsetFromSetting += ModSettings.MidVehFixedOffset;
                else
                    _offsetFromSetting += ModSettings.VehicleFixedOffset;
            }
        }

        /// <summary>
        /// Updates the camera's position and rotation in follow camera mode.
        /// </summary>
        private void UpdateFollowCamPos()
        {
            // Calculate the desired position and rotation of the camera by applying the offset to the FPSCam's current position and rotation.
            var instancePos = FPSCam.GetPositioning().pos + (FPSCam.GetPositioning().rotation * (_offset.pos + _offsetFromSetting));
            var instanceRotation = FPSCam.GetPositioning().rotation * _offset.rotation;

            // Limit the camera's position to the allowed area.
            instancePos = CameraController.ClampCameraPosition(instancePos);
            // Apply the calculated position and rotation to the camera.
            if (ModSettings.SmoothTransition)
            {
                CameraTransform.position =
                CameraTransform.position.DistanceTo(instancePos) > ModSettings.MinTransDistance &&
                CameraTransform.position.DistanceTo(instancePos) <= ModSettings.MaxTransDistance
                ? Vector3.Lerp(CameraTransform.position, instancePos, Time.deltaTime * ModSettings.TransSpeed)
                : instancePos;
                CameraTransform.rotation = Quaternion.Slerp(CameraTransform.rotation, instanceRotation, Time.deltaTime * ModSettings.TransSpeed);
            }
            else
            {
                CameraTransform.position = instancePos;
                CameraTransform.rotation = instanceRotation;
            }
        }

        /// <summary>
        /// Updates the camera's position and rotation in free camera mode.
        /// </summary>
        private void UpdateFreeCamPos(FreeCam freeCam)
        {
            // Automatically move the camera forward if AutoMove is enabled and the secondary mouse button is not pressed.
            if (freeCam.AutoMove && !InputManager.MousePressed(InputManager.MouseButton.Secondary))
            {
                var movement = Vector3.zero;
                movement.z += Time.deltaTime * ModSettings.MovementSpeed / MapUtils.ToKilometer(1f);
                _offset.pos += _offset.rotation * movement;
            }
            // Calculate the desired position and rotation of the camera by applying the offset to the camera's current position and rotation.
            var instancePos = CameraTransform.position + _offset.pos;
            var instanceRotation = _offset.rotation;

            if (ModSettings.GroundClipping != ModSettings.GroundClippings.None)
            {
                var minHeight = MapUtils.GetMinHeightAt(instancePos) + ModSettings.GroundLevelOffset; // Get minimum height including ground level offset.

                if ((ModSettings.GroundClipping == ModSettings.GroundClippings.AboveRoad || ModSettings.GroundClipping == ModSettings.GroundClippings.SnapToRoad) && MapUtils.GetClosestSegmentLevel(instancePos, out var height))
                    minHeight = height + ModSettings.RoadLevelOffset; // Adjust minHeight if road height is applicable.

                if (ModSettings.GroundClipping == ModSettings.GroundClippings.SnapToGround || ModSettings.GroundClipping == ModSettings.GroundClippings.SnapToRoad || instancePos.y < minHeight)
                    instancePos.y = ModSettings.SmoothTransition &&
                        Math.Abs(instancePos.y - minHeight) <= ModSettings.MinTransDistance ?
                        Mathf.Lerp(instancePos.y, minHeight, Time.deltaTime * ModSettings.TransSpeed) :
                        minHeight; // Apply the minimum height to the camera's y-axis.
            }

            // Limit the camera's position to the allowed area.
            instancePos = CameraController.ClampCameraPosition(instancePos);

            //Update the speed for info panel to display.
            if (ModSettings.ShowInfoPanel)
                freeCam.UpdateSpeed(CameraTransform.position, instancePos);

            // Apply the calculated position and rotation to the camera.
            if (ModSettings.SmoothTransition)
            {
                CameraTransform.position =
                CameraTransform.position.DistanceTo(instancePos) > ModSettings.MinTransDistance &&
                 CameraTransform.position.DistanceTo(instancePos) <= ModSettings.MaxTransDistance
                ? Vector3.Lerp(CameraTransform.position, instancePos, Time.deltaTime * ModSettings.TransSpeed)
                : instancePos;
                CameraTransform.rotation =
                    Quaternion.Slerp(CameraTransform.rotation, instanceRotation, Time.deltaTime * ModSettings.TransSpeed);
            }
            else
            {
                CameraTransform.position = instancePos;
                CameraTransform.rotation = instanceRotation;

            }
            // Reset the position offset after applying.
            _offset.pos = Vector3.zero;
        }

        /// <summary>
        /// Updates the camera's position and rotation in transition.
        /// </summary>
        private void UpdateTransitionPos()
        {
            _transitionTimer += Time.deltaTime;
            // If we've reached the distance of the end or time is out
            if (CameraTransform.position.DistanceTo(_endPos.pos) <= 2f ||
                _transitionTimer >= MaxTransitioningTime)
            {
                _transitionTimer = 0f;
                AfterTransition(_endPos);
                _endPos = default;
                return;
            }
            // Apply the transition position and rotation to the camera.
            CameraTransform.position =
                Vector3.Lerp(CameraTransform.position, _endPos.pos, Time.deltaTime * ModSettings.TransSpeed);
            CameraTransform.rotation =
                Quaternion.Slerp(CameraTransform.rotation, _endPos.rotation, Time.deltaTime * ModSettings.TransSpeed);
        }
        public enum CamStatus
        {
            Disabled,
            Enabled,
            PluginEnabled,
            Transitioning,
            Disabling
        }
        private static Transform CameraTransform => GameCamController.Instance.MainCamera.transform;
        private bool _isScrollTransitioning = false;
        private Positioning _endPos = default;
        private Positioning _offset = default;
        private Vector3 _offsetFromSetting = default;
        private float _targetFoV = ModSettings.CamFieldOfView;

        private float _transitionTimer = 0f;
        private const float MaxTransitioningTime = 5f;
    }
}
