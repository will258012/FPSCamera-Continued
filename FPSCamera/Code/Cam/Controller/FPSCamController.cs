﻿using AlgernonCommons;
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
        public static FPSCamController Instance { get; private set; }
        public IFPSCam FPSCam { get; private set; }
        /// <summary>
        /// Called when the script instance is being loaded. Initializes the singleton instance.
        /// </summary>
        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Enables the camera and associated UI elements or settings.
        /// </summary>
        private void EnableCam()
        {
            if (ModSettings.ShowInfoPanel)
                CamInfoPanel.Instance.EnableCamInfoPanel();
            if (ModSettings.HideGameUI)
                StartCoroutine(UIManager.ToggleUI(false));
            if (ModSettings.LodOpt)
                StartCoroutine(LodManager.ToggleLODOpt(true));
            if (ModSettings.ShadowsOpt)
                StartCoroutine(ShadowsManager.ToggleShadowsOpt(true));
            FollowButtons.Instance.enabled = false;
            GameCamController.Instance.Initialize();
        }

        /// <summary>
        /// Disables the camera and restores the original camera settings.
        /// </summary>
        private void DisableCam()
        {
            FPSCam.StopCam();
            FPSCam = _cachedCam = null;
            FollowButtons.Instance.enabled = true;
            if (ModSettings.ShowInfoPanel)
                CamInfoPanel.Instance.DisableCamInfoPanel();
            if (ModSettings.LodOpt)
                StartCoroutine(LodManager.ToggleLODOpt(false));
            if (ModSettings.ShadowsOpt)
                StartCoroutine(ShadowsManager.ToggleShadowsOpt(false));

            if (ModSettings.SmoothTransition)
            {
                if (ModSettings.SetBackCamera)
                    StartTransitioningOnDisabled(GameCamController.Instance._cachedPositioning);
                else
                    StartTransitioningOnDisabled(new Positioning(GameCamController.Instance.MainCamera.transform.position + new Vector3(0f, 50f, 0f), GameCamController.Instance.MainCamera.transform.rotation));
            }
            else
            {
                AfterTransition();
            }
        }
        private void AfterTransition()
        {
            if (ModSettings.HideGameUI)
            {
                GameCamController.Instance.MainCamera.rect = CameraController.kFullScreenWithoutMenuBarRect;
                StartCoroutine(UIManager.ToggleUI(true));
            }
            GameCamController.Instance.Restore();
        }
        /// <summary>
        /// Starts following the specified instance with the camera.
        /// </summary>
        /// <param name="instanceID">The instance ID to follow.</param>
        public void StartFollowing(InstanceID instanceID)
        {
            switch (instanceID.Type)
            {
                case InstanceType.Vehicle: { FPSCam = new VehicleCam(instanceID); break; }
                case InstanceType.Citizen: { FPSCam = new CitizenCam(instanceID); break; }
            }
            EnableCam();
            SyncCamOffset();
        }

        /// <summary>
        /// Starts the free camera mode.
        /// </summary>
        public void StartFreeCam()
        {
            FPSCam = new FreeCam();
            EnableCam();
            _offset = new Positioning(Vector3.zero, GameCamController.Instance.MainCamera.transform.rotation);
        }

        /// <summary>
        /// Starts the walk-through camera mode.
        /// </summary>
        public void StartWalkThruCam()
        {
            FPSCam = new WalkThruCam();
            EnableCam();

        }
        /// <summary>
        /// Starts transitioning when the camera needs to be disabled.
        /// </summary>
        /// <param name="endPos">where to end transitioning.</param>
        public void StartTransitioningOnDisabled(Positioning endPos)
        {
            _targetFoV = ModSettings.CamFieldOfView;

            if (!ModSettings.HideGameUI)
                GameCamController.Instance.MainCamera.rect = CameraController.kFullScreenWithoutMenuBarRect;

            var cameraTransform = GameCamController.Instance.MainCamera.transform;
            if (cameraTransform.position.DistanceTo(endPos.pos) <= ModSettings.MinTransDistance ||
                    cameraTransform.position.DistanceTo(endPos.pos) > ModSettings.MaxTransDistance)
            {
                AfterTransition();
                return;
            }
            _isTransitioning = true;
            this._endPos = endPos;
        }
        /// <summary>
        /// Updates the camera state each frame.
        /// </summary>
        private void Update()
        {
            try
            {
                if (FPSCam != null && FPSCam.IsActivated)
                {
                    if (!FPSCam.IsVaild()) { DisableCam(); return; }
                    if (FPSCam is CitizenCam citizenCamera)
                        CheckAnotherCam(citizenCamera);
                    if (_cachedCam != null)
                    {
                        CheckAnotherCam(_cachedCam);
                    }
                    if (FPSCam is WalkThruCam walkThruCam)
                        walkThruCam.ElapseTime(Time.deltaTime);
                }
            }
            catch (Exception e)
            {
                Logging.Error("FPSCamController: ");
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
                if (_isTransitioning)
                {
                    UpdatTransitionPos();
                }
                else
                {
                    if (FPSCam != null && FPSCam.IsActivated)
                    {
                        if (FPSCam is IFollowCam)
                            UpdateFollowCamPos();
                        else if (FPSCam is FreeCam)
                            UpdateFreeCamPos();
                    }
                    HandleInput();
                }
            }
            catch (Exception e)
            {
                Logging.Error("FPSCamController: ");
                Logging.LogException(e);
            }
        }

        /// <summary>
        /// Handles the escape key press to disable the camera.
        /// </summary>
        /// <returns>True if the camera was disabled, false otherwise.</returns>
        public bool OnEsc()
        {
            if (FPSCam != null && FPSCam.IsActivated)
            {
                DisableCam();
                return true;
            }
            return false;
        }



        /// <summary>
        /// Checks if there is another camera to switch to for a citizen camera.
        /// </summary>
        /// <param name="citizenCamera">The citizen camera to check.</param>
        private void CheckAnotherCam(CitizenCam citizenCamera)
        {
            if (citizenCamera.CheckAnotherCam())
            {
                if (_cachedCam == null)
                {
                    _cachedCam = citizenCamera;
                    FPSCam = citizenCamera.AnotherCam;
                }
            }
            else
            {
                if (_cachedCam != null)
                {
                    FPSCam = _cachedCam;
                    _cachedCam = null;
                }
            }
        }

        /// <summary>
        /// Handles user input for camera control.
        /// </summary>
        private void HandleInput()
        {
            if (InputManager.KeyTriggered(ModSettings.KeyCamToggle) && !SimulationManager.instance.ForcedSimulationPaused)
            {
                if (FPSCam != null && FPSCam.IsActivated) DisableCam();
                else StartFreeCam();
            }
            if (FPSCam == null) return;

            if (InputManager.MouseTriggered(InputManager.MouseButton.Middle) ||
                InputManager.KeyTriggered(ModSettings.KeyCamReset))
            {
                if (FPSCam is WalkThruCam cam)
                    cam.SyncCamOffset();
                else if (FPSCam is IFollowCam)
                    SyncCamOffset();
                if (ModSettings.SmoothTransition)
                    _targetFoV = ModSettings.CamFieldOfView;
                else
                    GameCamController.Instance.MainCamera.fieldOfView = ModSettings.CamFieldOfView;
            }

            if (InputManager.MouseTriggered(InputManager.MouseButton.Secondary) && ModSettings.ManualSwitchWalk)
                (FPSCam as WalkThruCam)?.SwitchTarget();
            if (InputManager.KeyTriggered(ModSettings.KeyAutoMove))
                (FPSCam as FreeCam)?.ToggleAutoMove();
            if (InputManager.KeyTriggered(ModSettings.KeySaveOffset) && (FPSCam is IFollowCam))
            {
                SaveCamOffset();
            }

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
                    _targetFoV = nowFoV / (ModSettings.FoViewScrollfactor * ModSettings.FoViewScrollfactor);
                else if (scroll < 0f && nowFoV < 75f)
                    _targetFoV = nowFoV * (ModSettings.FoViewScrollfactor * ModSettings.FoViewScrollfactor);

                GameCamController.Instance.MainCamera.fieldOfView = Mathf.Lerp(nowFoV, _targetFoV, Time.deltaTime * ModSettings.TransSpeed);
            }
            else
            {
                var FoV = GameCamController.Instance.MainCamera.fieldOfView;

                if (scroll > 0f && FoV > 10f)
                    GameCamController.Instance.MainCamera.fieldOfView = FoV / ModSettings.FoViewScrollfactor;
                else if (scroll < 0f && FoV < 75f)
                    GameCamController.Instance.MainCamera.fieldOfView = FoV * ModSettings.FoViewScrollfactor;
            }
        }
        /// <summary>
        /// Saves the current camera offset.
        /// </summary>
        private void SaveCamOffset()
        {
            var name = (FPSCam as IFollowCam)?.GetPrefabName() ?? null;
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
        /// <param name="followCam">Given camera. If not provided, synchronize based on the current camera.</param>
        internal void SyncCamOffset(IFollowCam followCam = null)
        {
            if (followCam == null) followCam = FPSCam as IFollowCam;
            OffsetsSettings.Load();

            var name = followCam?.GetPrefabName() ?? null;
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
            var roadHeight = MapUtils.GetClosestSegmentLevel(instancePos) ?? default; // Get the height of the closest road segment, if available.
            instancePos.y = Math.Max(instancePos.y, roadHeight); // Ensure the camera is at least at the height of the road.


            // Limit the camera's position to the allowed area.
            instancePos = CameraController.ClampCameraPosition(instancePos);
            // Apply the calculated position and rotation to the camera.
            if (ModSettings.SmoothTransition)
            {
                cameraTransform.position = cameraTransform.position.DistanceTo(instancePos) > ModSettings.MinTransDistance &&
                                            cameraTransform.position.DistanceTo(instancePos) <= ModSettings.MaxTransDistance
                    ? Vector3.Lerp(cameraTransform.position, instancePos, Time.deltaTime * ModSettings.TransSpeed)
                    : instancePos;
                cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, instanceRotation, Time.deltaTime * ModSettings.TransSpeed);
            }
            else
            {
                cameraTransform.position = instancePos;
                cameraTransform.rotation = instanceRotation;
            }
        }

        /// <summary>
        /// Updates the camera's position and rotation in free camera mode.
        /// </summary>
        private void UpdateFreeCamPos()
        {
            var freecam = FPSCam as FreeCam;

            // Automatically move the camera forward if AutoMove is enabled and the secondary mouse button is not pressed.
            if (freecam.AutoMove && !InputManager.MousePressed(InputManager.MouseButton.Secondary))
                _offset.pos.x += Time.deltaTime * ModSettings.MovementSpeed / MapUtils.ToKilometer(1f);

            var cameraTransform = GameCamController.Instance.MainCamera.transform;

            freecam.RecordLastPositioning();
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
                if ((ModSettings.GroundClipping == 3 || ModSettings.GroundClipping == 4 ?
                        MapUtils.GetClosestSegmentLevel(instancePos) : null)
                        is float roadHeight)
                    minHeight = roadHeight + ModSettings.RoadLevelOffset; // Adjust minHeight if road height is applicable.

                if (ModSettings.GroundClipping == 2 || ModSettings.GroundClipping == 4 || instancePos.y < minHeight)
                    instancePos.y = minHeight; // Apply the minimum height to the camera's y-axis.
            }

            // Limit the camera's position to the allowed area.
            instancePos = CameraController.ClampCameraPosition(instancePos);

            // Apply the calculated position and rotation to the camera.
            if (ModSettings.SmoothTransition)
            {
                cameraTransform.position = freecam._positioning.pos = cameraTransform.position.DistanceTo(instancePos) > ModSettings.MinTransDistance &&
                                            cameraTransform.position.DistanceTo(instancePos) <= ModSettings.MaxTransDistance
                    ? Vector3.Lerp(cameraTransform.position, instancePos, Time.deltaTime * ModSettings.TransSpeed)
                    : instancePos;
                cameraTransform.rotation = freecam._positioning.rotation = Quaternion.Lerp(cameraTransform.rotation, instanceRotation, Time.deltaTime * ModSettings.TransSpeed);
            }
            else
            {
                cameraTransform.position = freecam._positioning.pos = instancePos;
                cameraTransform.rotation = freecam._positioning.rotation = instanceRotation;
            }

            // Reset the position offset after applying.
            _offset.pos = Vector3.zero;
        }

        /// <summary>
        /// Updates the camera's position and rotation in transition.
        /// </summary>
        private void UpdatTransitionPos()
        {
            var cameraTransform = GameCamController.Instance.MainCamera.transform;

            if (cameraTransform.position.DistanceTo(_endPos.pos) <= ModSettings.MinTransDistance)
            {
                _isTransitioning = false;
                AfterTransition();
                return;
            }
            // Apply the calculated position and rotation to the camera.
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, _endPos.pos, Time.deltaTime * ModSettings.TransSpeed);
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, _endPos.rotation, Time.deltaTime * ModSettings.TransSpeed);
        }
        private bool _isTransitioning = false;
        private Positioning _endPos = new Positioning(Vector3.zero);
        private CitizenCam _cachedCam = null;
        private Positioning _offset = new Positioning(Vector3.zero);
        private float _targetFoV = ModSettings.CamFieldOfView;
    }
}
