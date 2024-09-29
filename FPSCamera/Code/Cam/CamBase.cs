using System.Collections.Generic;
using static FPSCamera.Utils.MathUtils;
namespace FPSCamera.Cam
{
    public interface IFPSCam
    {
        bool IsActivated { get; }
        float GetSpeed();
        Positioning GetPositioning();
        bool IsValid();
        void StopCam();

    }
    public interface IFollowCam : IFPSCam
    {
        uint FollowID { get; }
        InstanceID FollowInstance { get; }
        Dictionary<string, string> GetInfos();
        string GetFollowName();
        string GetPrefabName();
        string GetStatus();
        void SyncCamOffset();
        void SaveCamOffset();
    }
}
