using AlgernonCommons;
using System;
using System.Collections;
namespace FPSCamera.Game
{
    public class ShadowsManager
    {
        public static IEnumerator ToggleShadowsOpt(bool status)
        {
            try
            {
                Logging.Message("-- Setting shadows distance");
                if (status)
                {
                    _cachedDist = UnityEngine.QualitySettings.shadowDistance;
                    UnityEngine.QualitySettings.shadowDistance = Opt;
                }
                else
                {
                    UnityEngine.QualitySettings.shadowDistance = _cachedDist;
                }
            }

            catch (Exception e)
            {
                Logging.LogException(e);
            }
            yield break;
        }
        private static float _cachedDist;
        private const float Opt = 512f;
    }

}
