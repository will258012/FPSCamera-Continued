using AlgernonCommons;
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
        public IFPSCam FPSCam { get; private set; }

        /// <summary>
        /// Gets the current status of the camera.
        /// Useful for status determination.
        /// </summary>
        public CamStatus Status { get; private set; } = CamStatus.Disabled;
        /// <summary>
        /// Invoked after FPS Camera is enabled.
        /// </summary>
        public Action OnCameraEnabled { get; set; }
        /// <summary>
        /// Invoked after FPS Camera is disabled.
        /// </summary>
        public Action OnCameraDisabled { get; set; }
        /// <summary>
        /// Called when the script instance is being loaded. Initializes the singleton instance.
        /// </summary>
        private void Awake() => Instance = this;

        /// <summary>
        /// Enables the camera and associated UI elements or settings.
        /// </summary>
        private void EnableCam()
        {
            if (ModSettings.ShowInfoPanel)
                CamInfoPanel.Instance.EnableCamInfoPanel();
            if (ModSettings.HideGameUI)
                StartCoroutine(UIManager.ToggleUI(false));
            if (ModSettings.LodOpt != 0)
                StartCoroutine(LodManager.ToggleLODOpt(true));
            if (ModSettings.ShadowsOpt)
                StartCoroutine(ShadowsManager.ToggleShadowsOpt(true));
            GameCamController.Instance.Initialize();
            Status = CamStatus.Enabled;
            OnCameraEnabled?.Invoke();
        }

        /// <summary>
        /// Disables the camera and restores the original camera settings.
        /// </summary>
        private void DisableCam()
        {
            Logging.KeyMessage("Disabling FPS Camera");
            FPSCam?.StopCam();
            FPSCam = null;
            if (ModSettings.ShowInfoPanel)
                CamInfoPanel.Instance.DisableCamInfoPanel();
            if (ModSettings.LodOpt != 0)
                StartCoroutine(LodManager.ToggleLODOpt(false));
            if (ModSettings.ShadowsOpt)
                StartCoroutine(ShadowsManager.ToggleShadowsOpt(false));

            if (ModSettings.SmoothTransition)
            {
                if (ModSettings.SetBackCamera)
                    StartTransitioningOnDisabled(GameCamController.Instance._cachedPositioning);
                else
                    StartTransitioningOnDisabled(
                        new Positioning(GameCamController.Instance.MainCamera.transform.position +
                        new Vector3(0f, 50f, 0f), 
                        GameCamController.Instance.MainCamera.transform.rotation));
            }
            else
            {
                AfterTransition();
            }
            OnCameraDisabled?.Invoke();
        }
        private void AfterTransition()
        {
            if (ModSettings.HideGameUI)
                StartCoroutine(UIManager.ToggleUI(true));
            Status = CamStatus.Disabled;
            GameCamController.Instance.Restore();
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
            var cameraTransform = GameCamController.Instance.MainCamera.transform;
            if (cameraTransform.position.DistanceTo(endPos.pos) <= ModSettings.MinTransDistance ||
                    cameraTransform.position.DistanceTo(endPos.pos) > ModSettings.MaxTransDistance)
            {
                AfterTransition();
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
                if (Status == CamStatus.Enabled)
                {
                    if (!FPSCam.IsValid())
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
                Logging.Error("FPS Camera is about to exit due to some issues");
                Logging.Error("at FPSCamController.Update()");
                Logging.LogException(e);
                DisableCam();
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
                if (Status == CamStatus.Enabled)
                {
                    Logging.Error("FPS Camera is about to exit due to some issues");
                    DisableCam();
                }
                Logging.Error("at FPSCamController.LateUpdate()");
                Logging.LogException(e);
            }
        }

        /// <summary>
        /// Handles the escape key press to disable the camera.
        /// </summary>
        /// <returns>True if the camera was disabled, false otherwise.</returns>
        public bool OnEsc()
        {
            if (Status == CamStatus.Enabled)
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
                if (Status == CamStatus.Enabled) DisableCam();
                else StartFreeCam();
            }
            if (Status == CamStatus.Disabled) return;

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

            //Limit pitch
            var eulerAngles = _offset.rotation.eulerAngles;
            if (eulerAngles.x > 180f) eulerAngles.x -= 360f;
            eulerAngles.x = eulerAngles.x.Clamp(-ModSettings.MaxPitchDeg, ModSettings.MaxPitchDeg);
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
                Logging.Message($"Offset saved for \"{name}\"");
            }
        }
        /// <summary>
        /// Synchronizes the camera offset based on the given camera.
        /// </summary>
        /// <param name="followCam">Given camera.</param>
        internal void SyncCamOffset(IFollowCam followCam)
        {
            OffsetsSettings.Load();

            var name = followCam?.GetPrefabName();
            var newOffset = new Positioning(Vector3.zero);

            if (name != null && OffsetsSettings.Offsets.TryGetValue(name, out var offset))
            {
                newOffset = offset;
            }

            newOffset.pos += ModSettings.FollowCamOffset;
            if (followCam is CitizenCam)
                newOffset.pos += ModSettings.PedestrianFixedOffset;
            else if (followCam is VehicleCam cam)
            {
                if (cam.GetVehicle().m_leadingVehicle != default && cam.GetPrefabName() != cam.GetVehicle(cam.GetFrontVehicleID()).Info.name)
                    newOffset.pos += ModSettings.MidVehFixedOffset;
                else
                    newOffset.pos += ModSettings.VehicleFixedOffset;
            }
            _offset = newOffset;
        }

        /// <summary>
        /// Updates the camera's position and rotation in follow camera mode.
        /// </summary>
        private void UpdateFollowCamPos()
        {
            var cameraTransform = GameCamController.Instance.MainCamera.transform;

            // Calculate the desired position and rotation of the camera by applying the offset to the FPSCam's current position and rotation.
            var instancePos = FPSCam.GetPositioning().pos + (FPSCam.GetPositioning().rotation * _offset.pos);
            var instanceRotation = FPSCam.GetPositioning().rotation * _offset.rotation;

            // Adjust the y-axis to ensure the camera is above the road.
            if (MapUtils.GetClosestSegmentLevel(instancePos, out var height)) // Get the height of the closest road segment, if available.
                instancePos.y = Math.Max(instancePos.y, height); // Ensure the camera is at least at the height of the road.


            // Limit the camera's position to the allowed area.
            instancePos = CameraController.ClampCameraPosition(instancePos);
            // Apply the calculated position and rotation to the camera.
            if (ModSettings.SmoothTransition)
            {

                GameCamController.Instance.CameraController.m_targetPosition =
                    cameraTransform.position =
                    cameraTransform.position.DistanceTo(instancePos) > ModSettings.MinTransDistance &&
                    cameraTransform.position.DistanceTo(instancePos) <= ModSettings.MaxTransDistance
                    ? Vector3.Lerp(cameraTransform.position, instancePos, Time.deltaTime * ModSettings.TransSpeed)
                    : instancePos;
                cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, instanceRotation, Time.deltaTime * ModSettings.TransSpeed);
            }
            else
            {
                GameCamController.Instance.CameraController.m_targetPosition =
                    cameraTransform.position = instancePos;
                cameraTransform.rotation = instanceRotation;
            }
            GameCamController.Instance.CameraController.m_targetAngle = new Vector2(cameraTransform.eulerAngles.y, cameraTransform.eulerAngles.x).ClampEulerAngles();
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
            var cameraTransform = GameCamController.Instance.MainCamera.transform;

            freeCam.RecordLastPositioning();
            // Calculate the desired position and rotation of the camera by applying the offset to the FPSCam's current position and rotation.
            var instancePos = FPSCam.GetPositioning().pos + _offset.pos;
            var instanceRotation = _offset.rotation;

            /*
            Ground clipping options:
            NONE = 0
            ABOVEGROUND = 1
            SNAPTOGROUND = 2
            ABOVEROAD = 3
            SNAPTOROAD = 4
            */
            if (ModSettings.GroundClipping != 0)
            {
                var minHeight = MapUtils.GetMinHeightAt(instancePos) + ModSettings.GroundLevelOffset; // Get minimum height including ground level offset.
                if ((ModSettings.GroundClipping == 3 || ModSettings.GroundClipping == 4) && MapUtils.GetClosestSegmentLevel(instancePos, out var height))
                    minHeight = height + ModSettings.RoadLevelOffset; // Adjust minHeight if road height is applicable.

                if (ModSettings.GroundClipping == 2 || ModSettings.GroundClipping == 4 || instancePos.y < minHeight)
                    instancePos.y = minHeight; // Apply the minimum height to the camera's y-axis.
            }

            // Limit the camera's position to the allowed area.
            instancePos = CameraController.ClampCameraPosition(instancePos);

            // Apply the calculated position and rotation to the camera.
            if (ModSettings.SmoothTransition)
            {
                GameCamController.Instance.CameraController.m_targetPosition =
                    cameraTransform.position =
                    freeCam._positioning.pos =
                    cameraTransform.position.DistanceTo(instancePos) > ModSettings.MinTransDistance &&
                     cameraTransform.position.DistanceTo(instancePos) <= ModSettings.MaxTransDistance
                    ? Vector3.Lerp(cameraTransform.position, instancePos, Time.deltaTime * ModSettings.TransSpeed)
                    : instancePos;
                cameraTransform.rotation =
                    freeCam._positioning.rotation =
                    Quaternion.Slerp(cameraTransform.rotation, instanceRotation, Time.deltaTime * ModSettings.TransSpeed);
            }
            else
            {
                GameCamController.Instance.CameraController.m_targetPosition =
                    cameraTransform.position =
                    freeCam._positioning.pos =
                    instancePos;
                cameraTransform.rotation = freeCam._positioning.rotation = instanceRotation;

            }
            GameCamController.Instance.CameraController.m_targetAngle =
                new Vector2(cameraTransform.eulerAngles.y, cameraTransform.eulerAngles.x).ClampEulerAngles();
            // Reset the position offset after applying.
            _offset.pos = Vector3.zero;
        }

        /// <summary>
        /// Updates the camera's position and rotation in transition.
        /// </summary>
        private void UpdateTransitionPos()
        {
            var cameraTransform = GameCamController.Instance.MainCamera.transform;
            _transitionTimer += Time.deltaTime;
            if (cameraTransform.position.DistanceTo(_endPos.pos) <= ModSettings.MinTransDistance ||
                _transitionTimer >= MaxTransitioningTime)
            {
                _transitionTimer = 0f;
                _endPos = new Positioning(Vector3.zero);
                AfterTransition();
                return;
            }
            // Apply the calculated position and rotation to the camera.
            GameCamController.Instance.CameraController.m_targetPosition =
                cameraTransform.position =
                Vector3.Lerp(cameraTransform.position, _endPos.pos, Time.deltaTime * ModSettings.TransSpeed);
            cameraTransform.rotation =
                Quaternion.Slerp(cameraTransform.rotation, _endPos.rotation, Time.deltaTime * ModSettings.TransSpeed);
            GameCamController.Instance.CameraController.m_targetAngle =
                new Vector2(cameraTransform.eulerAngles.y, cameraTransform.eulerAngles.x).ClampEulerAngles();
        }
        public enum CamStatus
        {
            Disabled,
            Enabled,
            Transitioning
        }


        private bool _isScrollTransitioning = false;
        private Positioning _endPos = new Positioning(Vector3.zero);
        private Positioning _offset = new Positioning(Vector3.zero);
        private float _targetFoV = ModSettings.CamFieldOfView;

        private float _transitionTimer = 0f;
        private const float MaxTransitioningTime = 5f;
    }
}
