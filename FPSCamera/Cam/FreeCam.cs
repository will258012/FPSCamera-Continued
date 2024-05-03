namespace FPSCamera.Cam
{
    using Configuration;
    using CSkyL.Game;
    using CSkyL.Transform;

    public class FreeCam : Base
    {
        public override bool Validate() => true;
        public override Positioning GetPositioning() => _positioning;

        public override float GetSpeed()
            => _lastPosition.DistanceTo(_positioning.position) / Utils.TimeSinceLastFrame;

        public override void InputOffset(Offset inputOffset)
        {
            if (_autoMove && !Control.MousePressed(Control.MouseButton.Secondary))
                inputOffset.movement.forward += Utils.TimeSinceLastFrame
                                                * Config.G.MovementSpeed / Map.ToKilometer(1f);
            _lastPosition = _positioning.position;
            _positioning = _positioning.Apply(inputOffset);
            _positioning.angle = _positioning.angle.Clamp(pitchRange:
                    new CSkyL.Math.Range(-Config.G.MaxPitchDeg, Config.G.MaxPitchDeg));
            /*
             NONE = 0 
            ABOVEGROUND = 1
            SNAPTOGROUND = 2
            ABOVEROAD = 3
            SNAPTOROAD = 4 */
            if (Config.G.GroundClippingOption != 0) {
                var minH = Map.GetMinHeightAt(_positioning.position) + Config.G.GroundLevelOffset;
                if ((Config.G.GroundClippingOption == 3 ||
                             Config.G.GroundClippingOption == 4 ?
                             Map.GetClosestSegmentLevel(_positioning.position) : null)
                        is float roadH
                    ) minH = roadH + Config.G.RoadLevelOffset;

                if (Config.G.GroundClippingOption == 2 ||
                    Config.G.GroundClippingOption == 4 ||
                        _positioning.position.up < minH)
                    _positioning.position.up = minH;
            }
        }
        public override void InputReset() { _autoMove = false; }

        public void ToggleAutoMove() { _autoMove = !_autoMove; }

        public FreeCam(Positioning initPositioning)
        { _positioning = initPositioning; _lastPosition = _positioning.position; }

        private Positioning _positioning;
        private Position _lastPosition;
        private bool _autoMove = false;
    }
}
