namespace FPSCamera
{
    using Config;
    using CSkyL;
    using CSkyL.Transform;
    using CamController = CSkyL.Game.CamController;
    using CLOD = CSkyL.Game.Control.LodManager;
    using Control = CSkyL.Game.Control;
    using CUtils = CSkyL.Game.Utils;
    using Log = CSkyL.Log;

    public class Controller : CSkyL.Game.Behavior
    {
        public bool IsActivated => _state == State.Activated;
        public bool IsIdle => _state == State.Idle;

        public void StartFreeCam()
        {
            Log.Msg("Starting FreeCam mode");
            _SetCam(new Cam.FreeCam(_camGame.Positioning));
        }

        public void StartFollow(CSkyL.Game.ID.ObjectID idToFollow)
        {
            Log.Msg("Starting Follow mode");
            var newCam = Cam.FollowCam.Follow(idToFollow);
            if (newCam is Cam.FollowCam) _SetCam(newCam);
            else Log.Msg($"Fail to start Follow mode (ID: {idToFollow})");
        }
        public void StartWalkThruMode()
        {
            Log.Msg("Starting WalkThru mode");
            _SetCam(new Cam.WalkThruCam());
        }

        public void StopFPSCam()
        {
            if (!IsActivated) return;
            _camMod = null;

            _exitingTimer = Config.Config.instance.MaxExitingDuration;
            _camGame.AllSetting = _originalSetting;
            if (!Config.Config.instance.SetBackCamera)
                _camGame.Positioning = CamController.instance.LocateAt(_camGame.Positioning);
            if (_uiHidden) {
                StartCoroutine(Control.UIManager.ToggleUI(true));
                _uiHidden = false;
            }
            if (ModSupport.IsTrainDisplayFoundandEnabled && ModSupport.FollowVehicleID != default)
                ModSupport.FollowVehicleID = default;

            _uiCamInfoPanel.enabled = false;

            _state = State.Exiting;
        }

        public bool OnEsc()
        {
            if (_uiMainPanel.OnEsc()) return true;
            if (_camMod is object) {
                StopFPSCam();
                return true;
            }
            return false;
        }

        private void _SetCam(Cam.Base newCam)
        {
            _camMod = newCam;
            _uiCamInfoPanel.SetAssociatedCam(newCam);
            if (IsIdle) _EnableFPSCam();
            else _uiMainPanel.OnCamActivate();
        }

        private void _EnableFPSCam()
        {
            _originalSetting = _camGame.AllSetting;
            _ResetCamGame();

            CamController.instance.SetDepthOfField(Config.Config.instance.EnableDof);
            CamController.instance.Disable();

            _uiMainPanel.OnCamActivate();

            if (Config.Config.instance.LODOptimization)
                StartCoroutine(CLOD.ToggleLODOptimization(true));

            _state = State.Activated;
        }
        private void _DisableFPSCam()
        {
            Log.Msg("FPS camera stopped");
            _uiMainPanel.OnCamDeactivate();
            if (Config.Config.instance.LODOptimization)
                StartCoroutine(CLOD.ToggleLODOptimization(false));
            Control.ToggleCursor(true);
            _camGame.SetFullScreen(false);
            CamController.instance.Restore();
            _state = State.Idle;
        }
        private void _ResetCamGame()
        {
            _camGame.ResetTarget();
            _camGame.FieldOfView = Config.Config.instance.CamFieldOfView;
            _camGame.NearClipPlane = Config.Config.instance.CamNearClipPlane;
        }

        private Offset _GetInputOffsetAfterHandleInput()
        {
            if (Control.KeyTriggered(Config.Config.instance.KeyCamToggle)) {
                if (IsActivated) StopFPSCam();
                else StartFreeCam();
            }
            if (!IsActivated || !_camMod.Validate()) return null;

            if (Control.MouseTriggered(Control.MouseButton.Middle) ||
                Control.KeyTriggered(Config.Config.instance.KeyCamReset)) {
                _camMod.InputReset();
                _ResetCamGame();
            }

            if (Control.MouseTriggered(Control.MouseButton.Secondary) && Config.Config.instance.ManualSwitch4Walk)
                (_camMod as Cam.WalkThruCam)?.SwitchTarget();
            if (Control.KeyTriggered(Config.Config.instance.KeyAutoMove))
                (_camMod as Cam.FreeCam)?.ToggleAutoMove();
            if (Control.KeyTriggered(Config.Config.instance.KeySaveOffset) &&
                    _camMod is Cam.FollowCam followCam) {
                if (followCam.SaveOffset() is string name)
                    _uiMainPanel.ShowMessage($"Offset saved for <{name}>");
            }


            var movement = LocalMovement.None;
            { // key movement
                if (Control.KeyPressed(Config.Config.instance.KeyMoveForward)) movement.forward += 1f;
                if (Control.KeyPressed(Config.Config.instance.KeyMoveBackward)) movement.forward -= 1f;
                if (Control.KeyPressed(Config.Config.instance.KeyMoveRight)) movement.right += 1f;
                if (Control.KeyPressed(Config.Config.instance.KeyMoveLeft)) movement.right -= 1f;
                if (Control.KeyPressed(Config.Config.instance.KeyMoveUp)) movement.up += 1f;
                if (Control.KeyPressed(Config.Config.instance.KeyMoveDown)) movement.up -= 1f;
                movement *= (Control.KeyPressed(Config.Config.instance.KeySpeedUp) ? Config.Config.instance.SpeedUpFactor : 1f)
                            * Config.Config.instance.MovementSpeed * CUtils.TimeSinceLastFrame
                            / CSkyL.Game.Map.ToKilometer(1f);
            }

            var cursorVisible = Control.KeyPressed(Config.Config.instance.KeyCursorToggle) ^ (
                                _camMod is Cam.FreeCam ? Config.Config.instance.ShowCursor4Free
                                                    : Config.Config.instance.ShowCursor4Follow);
            Control.ToggleCursor(cursorVisible);

            float yawDegree = 0f, pitchDegree = 0f;
            { // key rotation
                if (Control.KeyPressed(Config.Config.instance.KeyRotateRight)) yawDegree += 1f;
                if (Control.KeyPressed(Config.Config.instance.KeyRotateLeft)) yawDegree -= 1f;
                if (Control.KeyPressed(Config.Config.instance.KeyRotateUp)) pitchDegree += 1f;
                if (Control.KeyPressed(Config.Config.instance.KeyRotateDown)) pitchDegree -= 1f;

                if (yawDegree != 0f || pitchDegree != 0f) {
                    var factor = Config.Config.instance.RotateKeyFactor * CUtils.TimeSinceLastFrame;
                    yawDegree *= factor; pitchDegree *= factor;
                }
                else if (!cursorVisible) {
                    // mouse rotation
                    const float factor = .2f;
                    yawDegree = Control.MouseMoveHori * Config.Config.instance.RotateSensitivity *
                                (Config.Config.instance.InvertRotateHorizontal ? -1f : 1f) * factor;
                    pitchDegree = Control.MouseMoveVert * Config.Config.instance.RotateSensitivity *
                                  (Config.Config.instance.InvertRotateVertical ? -1f : 1f) * factor;
                }
            }
            { // scroll zooming
                var scroll = Control.MouseScroll;
                var targetFoV = _camGame.TargetFoV;
                if (scroll > 0f && targetFoV > Config.Config.instance.CamFieldOfView.Min)
                    _camGame.FieldOfView = targetFoV / Config.Config.instance.FoViewScrollfactor;
                else if (scroll < 0f && targetFoV < Config.Config.instance.CamFieldOfView.Max)
                    _camGame.FieldOfView = targetFoV * Config.Config.instance.FoViewScrollfactor;
            }
            return new Offset(movement, new DeltaAttitude(yawDegree, pitchDegree));
        }

        private void _SetUpUI()
        {
            _uiMainPanel = gameObject.AddComponent<UI.MainPanel>();
            _uiMainPanel.SetWalkThruCallBack(StartWalkThruMode);
            _uiCamInfoPanel = gameObject.AddComponent<UI.CamInfoPanel>();
            if (CUtils.InGameMode) {
                _uiFollowButtons = gameObject.AddComponent<UI.FollowButtons>();
                _uiFollowButtons.registerFollowCallBack(StartFollow);
            }
        }

        protected override void _Init()
        {
            _state = State.Idle;
            _uiHidden = false;
            ThreadingExtension.Controller = this;
        }
        protected override void _SetUp()
        {
            _camGame = new GameCam();
            _SetUpUI();
        }

        public void SimulationFrame() => _camMod?.SimulationFrame();
#if DEBUG
        public void RenderOverlay(RenderManager.CameraInfo cameraInfo) => _camMod?.RenderOverlay(cameraInfo);
#endif
        protected override void _UpdateLate()
        {
            try {
                var controlOffset = _GetInputOffsetAfterHandleInput();

                if (IsIdle) return;

                if (_state == State.Exiting) {
                    _exitingTimer -= CUtils.TimeSinceLastFrame;
                    if (_camGame.AlmostAtTarget() is bool done && done || _exitingTimer <= 0f) {
                        if (!done) _camGame.AdvanceToTarget();
                        _DisableFPSCam();
                        return;
                    }
                }
                else if (!_camMod.Validate()) { StopFPSCam(); return; }
                else {
                    _camMod.InputOffset(controlOffset);
                    (_camMod as Cam.ICamUsingTimer)?.ElapseTime(CUtils.TimeSinceLastFrame);
                    _camGame.Positioning = _camMod.GetPositioning();
                }

                var distance = _camGame.Positioning.position
                                   .DistanceTo(_camGame.TargetPositioning.position);
                var factor = Config.Config.instance.GetAdvanceRatio(CUtils.TimeSinceLastFrame);
                if (Config.Config.instance.SmoothTransition) {
                    if (distance > Config.Config.instance.GiveUpTransDistance)
                        _camGame.AdvanceToTargetSmooth(factor,
                                                        instantMove: true, instantAngle: true);
                    else if (_camMod is Cam.FollowCam && distance <= Config.Config.instance.InstantMoveMax)
                        _camGame.AdvanceToTargetSmooth(factor, instantMove: true);
                    else _camGame.AdvanceToTargetSmooth(factor);
                }
                else {
                    _camGame.AdvanceToTarget();
                }

                if (IsActivated) {
                    _uiCamInfoPanel.enabled = Config.Config.instance.ShowInfoPanel;
                    if (Config.Config.instance.HideGameUI ^ _uiHidden) {
                        _uiHidden = Config.Config.instance.HideGameUI;
                        StartCoroutine(Control.UIManager.ToggleUI(!_uiHidden));
                        _camGame.SetFullScreen(_uiHidden);
                    }
                }
            }
            catch (System.Exception e) {
                Log.Err("Unrecognized Error: " + e);
            }
        }

        // Cameras
        private Cam.Base _camMod;
        private GameCam _camGame;
        private GameCam.Setting _originalSetting;

        // UI
        [CSkyL.Game.RequireDestruction] private UI.MainPanel _uiMainPanel;
        [CSkyL.Game.RequireDestruction] private UI.CamInfoPanel _uiCamInfoPanel;
        [CSkyL.Game.RequireDestruction] private UI.FollowButtons _uiFollowButtons;
        private bool _uiHidden;

        // state
        private enum State { Idle, Exiting, Activated }
        private State _state = State.Idle;
        private float _exitingTimer = 0f;
    }
}
