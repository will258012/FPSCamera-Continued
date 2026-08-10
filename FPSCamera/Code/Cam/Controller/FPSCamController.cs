using AlgernonCommons;
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
        public IFPSCam FPSCam
        {
            get;
            set
            {
                if (Status == CamStatus.Transitioning || Status == CamStatus.Disabling)
                {
                    Logging.Error($"Ignored FPSCam assignment to {value?.Name ?? "null"} due to camera is still busy for disabling");
                    return;
                }

                field?.DisableCam();
                field = value;
                if (field != null)
                {
                    if (Status == CamStatus.Disabled)
                        EnableCam();
                    Status = (field) switch
                    {
                        VehicleCam or FreeCam or CitizenCam or WalkThruCam => CamStatus.Enabled,
                        _ => CamStatus.Enabled | CamStatus.PluginEnabled,
                    };
                    EventModeSwitched.Invoke(field.Name);
                }
                else
                    DisableCam();
            }
        }

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
        /// Camera mode switching event handler delegate.
        /// </summary>
        public delegate void ModeSwitchingEventHandler(string modeName);

        /// <summary>
        /// Camera mode switched event.
        /// </summary>
        public static event ModeSwitchingEventHandler EventModeSwitched;
        /// <summary>
        /// Sets if the FPS Camera should ignore the <see cref="ModSettings.SetBackCamera"/> setting when disabled.
        /// Useful for compatibility issues, auto set to <see cref="OverrideSetBack.None"/> after disabled.
        /// </summary>
        public OverrideSetBack OverrideSetBackCamera { get; set; } = OverrideSetBack.None;
        /// <summary>
        /// Called when the script instance is being loaded. Initializes the singleton instance.
        /// </summary>
        private void Awake()
        {
            Instance = this;
            isGame = ToolsModifierControl.isGame;
        }

        /// <summary>
        /// Enables the camera and associated UI elements or settings.
        /// </summary>
        private void EnableCam()
        {
            Logging.KeyMessage("Enabling FPS Camera");
            OffsetsSettings.Load();
            GameCamController.Instance.Initialize();

            StopAllCoroutines();
            if (ModSettings.HideGameUI)
                StartCoroutine(UIManager.ToggleUI(false));
            if (ModSettings.LodOpt != 0)
                StartCoroutine(LodManager.ToggleLODOpt(true));
            if (ModSettings.ShadowsOpt)
                StartCoroutine(ShadowsManager.ToggleShadowsOpt(true));
            if (ModSettings.SmoothTransition)
                targetFoV = ModSettings.CamFieldOfView;

            OnCameraEnabled?.Invoke();
        }

        /// <summary>
        /// Disables the camera and restores the original camera settings.
        /// </summary>
        private void DisableCam()
        {
            Logging.KeyMessage("Disabling FPS Camera");
            Status = CamStatus.Disabling;

            StopAllCoroutines();
            if (ModSettings.LodOpt != 0)
                StartCoroutine(LodManager.ToggleLODOpt(false));
            if (ModSettings.ShadowsOpt)
                StartCoroutine(ShadowsManager.ToggleShadowsOpt(false));

            if (!GameCamController.Instance.CameraController.GetTarget().IsEmpty)
                OverrideSetBackCamera = OverrideSetBack.False;

            if (ModSettings.SmoothTransition)
            {
                if ((ModSettings.SetBackCamera || OverrideSetBackCamera == OverrideSetBack.ACME) && OverrideSetBackCamera != OverrideSetBack.False)
                    StartTransitioningOnDisabling();
                else StartFOVOnlyTransitioningOnDisabling();
            }
            else AfterTransition();

            OnCameraDisabled?.Invoke();
        }
        private void AfterTransition()
        {
            if (ModSettings.HideGameUI)
                StartCoroutine(UIManager.ToggleUI(true));
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
            if (instanceID.IsEmpty)
            {
                Logging.Error("Given InstanceID is empty");
                return;
            }
            if (instanceID.Type == InstanceType.Citizen)
            {
                var newInstanceID = InstanceManager.GetLocation(instanceID);
                if (newInstanceID.Type != InstanceType.Vehicle)
                    instanceID = newInstanceID;
            }

            switch (instanceID.Type)
            {
                case InstanceType.Vehicle:
                    FPSCam = new VehicleCam(instanceID);
                    break;
                case InstanceType.Citizen:
                case InstanceType.CitizenInstance:
                    FPSCam = new CitizenCam(instanceID);
                    break;
                default:
                    Logging.Error($"This kind of InstanceID.Type is not supported: {instanceID.Type}");
                    return;
            }
            (FPSCam as IFollowCam).SyncCamOffset();
        }

        /// <summary>
        /// Starts Free-camera mode.
        /// </summary>
        public void StartFreeCam()
        {
            Logging.KeyMessage("Starting Free-Camera mode");
            FPSCam = new FreeCam();
            offset = new Positioning(Vector3.zero, GameCamController.Instance.MainCamera.transform.rotation);
        }
        /// <summary>
        /// Starts Free-camera mode.
        /// </summary>
        public void StartFreeCam(Positioning positioning, float fov = -1f)
        {
            Logging.KeyMessage("Starting Free-Camera mode");
            FPSCam = new FreeCam();
            GameCamController.Instance.MainCamera.transform.position = positioning.pos;
            offset = new Positioning(Vector3.zero, positioning.rotation);
            if (fov == -1f) fov = ModSettings.CamFieldOfView;
            targetFoV = fov;
        }

        /// <summary>
        /// Starts Walk-through mode.
        /// </summary>
        public void StartWalkThruCam()
        {
            Logging.KeyMessage("Starting Walk-Through mode");
            FPSCam = new WalkThruCam();
        }
        /// <summary>
        /// Starts transitioning when the camera needs to be disabled.
        /// </summary>
        public void StartTransitioningOnDisabling()
        {
            transitionTimer = 0f;
            float dist = CameraTransform.position.DistanceTo(GameCamController.Instance.transitionEndPositioning.pos);
            if (dist > ModSettings.MaxTransDistance)
            {
                AfterTransition();
                return;
            }

            targetFoV = GameCamController.Instance.savedFoV;
            if (!GameCamController.Instance.MainCamera.fieldOfView.AlmostEquals(targetFoV, .1f))
                isFOVTransitioning = true;

            Status = CamStatus.Transitioning;
        }
        /// <summary>
        /// Starts FOV-only transitioning when the camera needs to be disabled.
        /// </summary>
        public void StartFOVOnlyTransitioningOnDisabling()
        {
            targetFoV = GameCamController.Instance.savedFoV;
            if (GameCamController.Instance.MainCamera.fieldOfView.AlmostEquals(targetFoV, .1f))
            {
                AfterTransition();
                return;
            }
            isFOVTransitioning = true;
            Status = CamStatus.FOVOnlyTransitioning;
        }
        /// <summary>
        /// Updates the camera state each frame.
        /// </summary>
        private void Update()
        {
            try
            {
                if (Status.IsFlagSet(CamStatus.Enabled))
                {
                    if (!(FPSCam?.IsValid() ?? false))
                    {
                        FPSCam = null;
                        return;
                    }
                    if (FPSCam is WalkThruCam walkThruCam)
                        walkThruCam.ElapseTime(Time.deltaTime);
                }
            }
            catch (Exception e)
            {
                FPSCam = null;
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
                switch (Status)
                {
                    case CamStatus.Enabled:
                        {
                            if (FPSCam is IFollowCam)
                                UpdateFollowCamPos();
                            else if (FPSCam is FreeCam freeCam)
                                UpdateFreeCamPos(freeCam);
                            break;
                        }
                    case CamStatus.Transitioning:
                        UpdateTransitionPos();
                        UpdateFOVTransition();
                        break;
                    case CamStatus.FOVOnlyTransitioning:
                        UpdateFOVOnlyTransition();
                        break;
                }
            }
            catch (Exception e)
            {
                if (Status.IsFlagSet(CamStatus.Enabled))
                    FPSCam = null;
                else
                    AfterTransition();

                Logging.LogException(e);
            }
        }

        /// <summary>
        /// Handles the escape key press to disable the camera.
        /// </summary>
        /// <returns>True if the camera was disabled, false otherwise.</returns>
        public bool OnEsc()
        {
            if (Status.IsFlagSet(CamStatus.Enabled))
            {
                FPSCam = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles user input for camera control.
        /// </summary>
        private void HandleInput()
        {
            if (!SimulationManager.instance.ForcedSimulationPaused) // If the game isn't in the pause menu
            {
                if (ModSettings.KeyCamToggle.KeyTriggered())
                {
                    if (FPSCam is not FreeCam) StartFreeCam();
                    else FPSCam = null;
                }
                if (isGame)
                {
                    if (ModSettings.KeyWalkThruToggle.KeyTriggered())
                    {
                        if (FPSCam is not WalkThruCam) StartWalkThruCam();
                        else FPSCam = null;
                    }
                    if (ModSettings.KeyFollowToggle.KeyTriggered())
                    {
                        if (FPSCam is IFollowCam followCam)
                        {
                            GameCamController.Instance.CameraController.SetTarget(followCam.FollowInstance, followCam.GetPositioning().pos, true);
                            FPSCam = null;
                        }
                        else if (Status == CamStatus.Disabled && !GameCamController.Instance.CameraController.GetTarget().IsEmpty)
                            StartFollowing(GameCamController.Instance.CameraController.GetTarget());
                    }
                }
            }
            if (Status.IsFlagSet(CamStatus.Enabled) && ModSettings.KeyInfoPanelToggle.KeyTriggered())
                CamInfoPanel.Instance.UIEnabled = !CamInfoPanel.Instance.UIEnabled;

            if (Status != CamStatus.Enabled) return;

            if (InputManager.MouseButton.Middle.MouseTriggered() ||
                ModSettings.KeyCamReset.KeyTriggered())
            {
                (FPSCam as IFollowCam)?.SyncCamOffset();
                targetFoV = ModSettings.CamFieldOfView;
            }

            if (InputManager.MouseButton.Secondary.MouseTriggered() && ModSettings.ManualSwitchWalk)
                (FPSCam as WalkThruCam)?.SwitchTarget();
            if (ModSettings.KeyAutoMove.KeyTriggered())
                (FPSCam as FreeCam)?.ToggleAutoMove();
            if (ModSettings.KeySaveOffset.KeyTriggered())
                (FPSCam as IFollowCam)?.SaveCamOffset();

            { // key movement
                var movementFactor = ((ModSettings.KeySpeedUp.IsPressed() ? ModSettings.SpeedUpFactor : 1f)
                                     * (FPSCam is IFollowCam ? ModSettings.OffsetMovementSpeed : ModSettings.MovementSpeed) * Time.deltaTime).FromKmph();

                var LocalMovement = Vector3.zero;
                if (ModSettings.KeyMoveForward.IsPressed()) LocalMovement += Vector3.forward * movementFactor;
                if (ModSettings.KeyMoveBackward.IsPressed()) LocalMovement += Vector3.back * movementFactor;
                if (ModSettings.KeyMoveRight.IsPressed()) LocalMovement += Vector3.right * movementFactor;
                if (ModSettings.KeyMoveLeft.IsPressed()) LocalMovement += Vector3.left * movementFactor;
                if (ModSettings.KeyMoveUp.IsPressed()) LocalMovement += Vector3.up * movementFactor;
                if (ModSettings.KeyMoveDown.IsPressed()) LocalMovement += Vector3.down * movementFactor;

                offset.pos += offset.rotation * LocalMovement;
            }

            var cursorVisible = ModSettings.KeyCursorToggle.IsPressed() ^ (
                                FPSCam is FreeCam ? ModSettings.ShowCursorFree
                                                    : ModSettings.ShowCursorFollow);
            InputManager.ToggleCursor(cursorVisible);

            float yawDegree = 0f, pitchDegree = 0f;
            { // key rotation
                var rotateFactor = ModSettings.RotateKeyFactor * Time.deltaTime;

                if (ModSettings.KeyRotateRight.IsPressed()) yawDegree += 1f * rotateFactor;
                if (ModSettings.KeyRotateLeft.IsPressed()) yawDegree -= 1f * rotateFactor;
                if (ModSettings.KeyRotateUp.IsPressed()) pitchDegree -= 1f * rotateFactor;
                if (ModSettings.KeyRotateDown.IsPressed()) pitchDegree += 1f * rotateFactor;

                if (yawDegree == 0f && pitchDegree == 0f && !cursorVisible)
                {
                    // mouse rotation
                    yawDegree = InputManager.MouseMoveHori * ModSettings.RotateSensitivity *
                                (ModSettings.InvertRotateHorizontal ? -1f : 1f) * MouseFactor;
                    pitchDegree = InputManager.MouseMoveVert * ModSettings.RotateSensitivity *
                                  (ModSettings.InvertRotateVertical ? 1f : -1f) * MouseFactor;
                }
            }
            var yawRotation = Quaternion.Euler(0f, yawDegree, 0f);
            var pitchRotation = Quaternion.Euler(pitchDegree, 0f, 0f);
            offset.rotation = yawRotation * offset.rotation * pitchRotation;

            // Limit pitch
            var eulerAngles = offset.rotation.eulerAngles;
            if (eulerAngles.x > 180f) eulerAngles.x -= 360f;
            eulerAngles.x = eulerAngles.x.Clamp(-ModSettings.MaxPitchDeg, ModSettings.MaxPitchDeg);
            eulerAngles.z = 0f;
            offset.rotation = Quaternion.Euler(eulerAngles);

            // scroll zooming
            var scroll = InputManager.MouseScroll;
            var currentFoV = GameCamController.Instance.MainCamera.fieldOfView;
            if (scroll > 0f && currentFoV > MinFoV)
            {
                targetFoV = currentFoV / ModSettings.FoViewScrollfactor;
            }
            else if (scroll < 0f && currentFoV < MaxFoV)
            {
                targetFoV = currentFoV * ModSettings.FoViewScrollfactor;
            }

            if (ModSettings.SmoothTransition)
            {
                if (!isFOVTransitioning && !currentFoV.AlmostEquals(targetFoV))
                    isFOVTransitioning = true;

                UpdateFOVTransition();
            }
            else
            {
                GameCamController.Instance.MainCamera.fieldOfView = targetFoV;
            }
        }

        /// <summary>
        /// Saves the given camera offset.
        /// </summary>
        /// <param name="followCam">Given camera.</param>
        internal void SaveCamOffset(IFollowCam followCam)
        {
            var name = followCam?.PrefabName;
            if (name != null)
            {
                OffsetsSettings.Offsets[name] = offset;
                OffsetsSettings.Save();
                CamInfoPanel.Instance.SetFooterMessage(string.Format(Translations.Translate("INFO_OFFSETSAVED"), name));
                Logging.Message($"Saved type setting for \"{name}\"");
            }
        }
        /// <summary>
        /// Synchronizes the camera offset based on the given camera.
        /// </summary>
        /// <param name="followCam">Given camera.</param>
        internal void SyncCamOffset(IFollowCam followCam)
        {
            var name = followCam?.PrefabName;
            offset = default;
            offsetFromSetting = default;

            if (name != null && OffsetsSettings.Offsets.TryGetValue(name, out var newOffset))
            {
                offset = newOffset;
            }

            offsetFromSetting += ModSettings.FollowCamOffset;
            if (followCam is CitizenCam)
                offsetFromSetting += ModSettings.PedestrianFixedOffset;
            else if (followCam is VehicleCam cam)
            {
                if (cam.GetVehicle().m_leadingVehicle != default && cam.PrefabName != VehicleCam.GetVehicle(cam.GetFrontVehicleID()).Info.name)
                    offsetFromSetting += ModSettings.MidVehFixedOffset;
                else
                    offsetFromSetting += ModSettings.VehicleFixedOffset;
            }
        }

        /// <summary>
        /// Updates the camera's position and rotation in follow camera mode.
        /// </summary>
        private void UpdateFollowCamPos()
        {
            // Calculate the desired position and rotation of the camera by applying the offset to the FPSCam's current position and rotation.
            var instancePos = FPSCam.GetPositioning().pos + (FPSCam.GetPositioning().rotation * (offset.pos + offsetFromSetting));
            var instanceRotation = FPSCam.GetPositioning().rotation * offset.rotation;

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
            var previousPosition = CameraTransform.position;

            // Automatically move the camera forward if AutoMove is enabled and the secondary mouse button is not pressed.
            if (freeCam.AutoMove && !InputManager.MouseButton.Secondary.MousePressed())
            {
                var movement = Vector3.zero;
                movement.z += (Time.deltaTime * ModSettings.MovementSpeed).FromKmph();
                offset.pos += offset.rotation * movement;
            }
            // Calculate the desired position and rotation of the camera by applying the offset to the camera's current position and rotation.
            var instancePos = CameraTransform.position + offset.pos;
            var instanceRotation = offset.rotation;

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

            // When in free-camera mode, update the speed for info panel using the final applied position.
            if (CamInfoPanel.Instance.UIEnabled)
                freeCam.UpdateSpeed(previousPosition, CameraTransform.position);

            // Reset the position offset after applying.
            offset.pos = Vector3.zero;
        }

        /// <summary>
        /// Updates the camera's position and rotation in transition.
        /// </summary>
        private void UpdateTransitionPos()
        {
            transitionTimer += Time.deltaTime;
            // If we've reached the distance of the end or time is out
            if ((CameraTransform.position.DistanceTo(GameCamController.Instance.transitionEndPositioning.pos) <= 2f && !isFOVTransitioning) ||
                transitionTimer >= MaxTransitioningTime)
            {
                transitionTimer = 0f;
                AfterTransition();
                return;
            }
            // Apply the transition position and rotation to the camera.
            CameraTransform.position =
                Vector3.Lerp(CameraTransform.position, GameCamController.Instance.transitionEndPositioning.pos, Time.deltaTime * ModSettings.TransSpeed);
            CameraTransform.rotation =
                Quaternion.Slerp(CameraTransform.rotation, GameCamController.Instance.transitionEndPositioning.rotation, Time.deltaTime * ModSettings.TransSpeed);
        }
        /// <summary>
        /// Updates the camera's FOV during a scroll transition to smoothly adjust to the target FOV.
        /// </summary>
        private void UpdateFOVTransition()
        {
            if (!isFOVTransitioning)
                return;

            if (GameCamController.Instance.MainCamera.fieldOfView.AlmostEquals(targetFoV, .1f))
            {
                GameCamController.Instance.MainCamera.fieldOfView = targetFoV;
                isFOVTransitioning = false;
            }
            GameCamController.Instance.MainCamera.fieldOfView = Mathf.Lerp(GameCamController.Instance.MainCamera.fieldOfView, targetFoV, Time.deltaTime * ModSettings.TransSpeed);
        }
        /// <summary>
        /// Updates the camera's FOV during FOV-only transitioning to smoothly adjust to the target FOV.
        /// </summary>
        private void UpdateFOVOnlyTransition()
        {
            if (!isFOVTransitioning)
                return;

            transitionTimer += Time.deltaTime;
            if (GameCamController.Instance.MainCamera.fieldOfView.AlmostEquals(targetFoV, .1f) || transitionTimer >= MaxTransitioningTime)
            {
                transitionTimer = 0f;
                GameCamController.Instance.MainCamera.fieldOfView = targetFoV;
                isFOVTransitioning = false;
                AfterTransition();
            }
            GameCamController.Instance.MainCamera.fieldOfView = Mathf.Lerp(GameCamController.Instance.MainCamera.fieldOfView, targetFoV, Time.deltaTime * ModSettings.TransSpeed);
        }
        [Flags]
        public enum CamStatus
        {
            Disabled,
            Enabled,
            PluginEnabled = 2,
            Transitioning = 4,
            Disabling = 8,
            FOVOnlyTransitioning = 16,
        }
        public enum OverrideSetBack
        {
            None,
            /// <summary>
            /// Force the camera to behave as if <see cref="ModSettings.SetBackCamera"/> were disabled.
            /// </summary>
            False,
            /// <summary>
            /// Special setting for ACME support.
            /// </summary>
            ACME
        }
        private static Transform CameraTransform => GameCamController.Instance.MainCamera.transform;
        private bool isFOVTransitioning = false;
        private Positioning offset = default;
        private Vector3 offsetFromSetting = default;
        private bool isGame = false;

        private float transitionTimer = 0f;
        private const float MaxTransitioningTime = 5f;

        internal float targetFoV = ModSettings.CamFieldOfView;
        private const float MinFoV = 10f;
        private const float MaxFoV = 75f;
        private const float MouseFactor = .2f;
    }
}
