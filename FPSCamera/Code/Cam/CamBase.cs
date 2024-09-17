using System.Collections.Generic;
using static FPSCamera.Utils.MathUtils;
namespace FPSCamera.Cam
{
    public interface IFPSCam
    {
        bool IsActivated { get; }
        float GetSpeed();
        Positioning GetPositioning();
        bool IsVaild();
        void StopCam();
        void SyncCamOffset();
        void SaveCamOffset();
    }
    public interface IFollowCam : IFPSCam
    {
        uint FollowID { get; }
        InstanceID FollowInstance { get; }
        Dictionary<string, string> GetInfos();
        string GetFollowName();
        string GetPrefabName();
        string GetStatus();

    }
}
