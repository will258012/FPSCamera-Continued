namespace FPSCamera.Cam
{
    using CSkyL.Game;
    using CSkyL.Game.ID;
    using CSkyL.Game.Object;
    using CSkyL.Transform;
    using Ctransl = CSkyL.Translation.Translations;

    public interface ICamUsingTimer
    {
        void ElapseTime(float seconds);
        float GetElapsedTime();
    }

    public abstract class Base
    {
        public bool IsOperating => !(_state is Finish);

        public abstract bool Validate(); // call first before using the other methods.
        public virtual void SimulationFrame() { }
#if DEBUG
        public virtual void RenderOverlay(RenderManager.CameraInfo cameraInfo) { }
#endif
        public abstract Positioning GetPositioning();
        public abstract float GetSpeed();
        public virtual Utils.Infos GetGeoInfos()
        {
            Utils.Infos infos = new Utils.Infos();
            if (!(GetPositioning() is Positioning positioning)) return infos;

            if (Map.RayCastDistrict(positioning.position) is DistrictID disID) {
                var name = District.GetName(disID);
                if (!string.IsNullOrEmpty(name))
                    infos[Ctransl.Translate("INFO_DISTRICT")] = name;
            }
            if (Map.RayCastDLCDistrict(positioning.position) is DistrictID DLCdisID) {
                var name = DLCDistrict.GetName(DLCdisID);
                if (!string.IsNullOrEmpty(name)) {
                    switch (DLCDistrict.GetDistrictType(DLCdisID)) {
                    case 1:
                        infos[Ctransl.Translate("INFO_DLCDISTRICT_PARK")] = name;
                        break;
                    case 2:
                        infos[Ctransl.Translate("INFO_DLCDISTRICT_INDUSTRY")] = name;
                        break;
                    case 4:
                        infos[Ctransl.Translate("INFO_DLCDISTRICT_CAMPUS")] = name;
                        break;
                    case 8:
                        infos[Ctransl.Translate("INFO_DLCDISTRICT_AIRPORT")] = name;
                        break;
                    case 0x10:
                        infos[Ctransl.Translate("INFO_DLCDISTRICT_PEDZONE")] = name;
                        break;
                    default:
                        infos["DLC District"] = name;
                        break;
                    }

                }

            }
            if (Map.RayCastRoad(positioning.position) is SegmentID segID) {
                var name = Segment.GetName(segID);
                if (!string.IsNullOrEmpty(name))
                    infos[Ctransl.Translate("INFO_ROAD")] = name;
            }
            return infos;
        }

        public abstract void InputOffset(Offset _offsetInput);
        public abstract void InputReset();

        protected abstract class State { }
        protected class Normal : State { }
        protected class Finish : State { }

        protected State _state = new Normal();
    }
}
