using System.Collections.Generic;
using static FPSCamera.Utils.MathUtils;
namespace FPSCamera.Cam
{
    public interface IFPSCam
    {
        float GetSpeed();
        Positioning GetPositioning();
        bool IsValid();
        void DisableCam();
    }
    public interface IFollowCam : IFPSCam
    {
        uint FollowID { get; }
        InstanceID FollowInstance { get; }
        Dictionary<string, string> GetInfo();
        string GetFollowName();
        string GetPrefabName();
        string GetStatus();
        void SyncCamOffset();
        void SaveCamOffset();
    }
}
