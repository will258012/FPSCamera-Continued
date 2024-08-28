using AlgernonCommons;
using System;
using System.Collections;
namespace FPSCamera.Game
{
    public class ShadowsManager
    {
        public static IEnumerator ToggleShadowsOptimization(bool status)
        {
            try
            {
                Logging.Message("-- Setting shadows distance");
                if (status)
                {
                    beforeOptimization = UnityEngine.QualitySettings.shadowDistance;
                    UnityEngine.QualitySettings.shadowDistance = Optimization;
                }
                else
                {
                    UnityEngine.QualitySettings.shadowDistance = beforeOptimization;
                }
            }

            catch (Exception e)
            {
                Logging.LogException(e);
            }
            yield break;
        }
        private static float beforeOptimization;
        private const float Optimization = 512f;
    }

}
