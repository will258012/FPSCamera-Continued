using AlgernonCommons;
using System;
using System.Collections;
using UnityEngine;
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
                    _cachedDist = QualitySettings.shadowDistance;
                    QualitySettings.shadowDistance = Mathf.Min(Opt, _cachedDist);
                }
                else
                {
                    QualitySettings.shadowDistance = _cachedDist;
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
