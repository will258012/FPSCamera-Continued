﻿using FPSCamera.Cam.Controller;
using FPSCamera.Utils;
using UnityEngine;
using static FPSCamera.Utils.MathUtils;

namespace FPSCamera.Cam
{
    public class FreeCam : IFPSCam
    {
        public FreeCam()
        {
            _positioning.pos = GameCamController.Instance.CameraController.transform.position;
            _positioning.rotation = Quaternion.identity;
            _lastPositioning = _positioning;
            IsActivated = true;
        }

        public bool IsActivated { get; private set; }

        public Positioning GetPositioning() => _positioning;
        internal void RecordLastPositioning() => _lastPositioning = _positioning;
        public bool AutoMove { get; set; }
        public void ToggleAutoMove() => AutoMove = !AutoMove;
        public float GetSpeed() => _lastPositioning.pos.DistanceTo(_positioning.pos) / Time.deltaTime;
        public bool IsVaild() => true;
        public void StopCam() { IsActivated = false; _positioning = _lastPositioning = default; }

        internal Positioning _positioning;
        private Positioning _lastPositioning;
    }
}
