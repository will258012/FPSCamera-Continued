﻿using AlgernonCommons;
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
                    cachedDist = QualitySettings.shadowDistance;
                    QualitySettings.shadowDistance = Mathf.Min(Opt, cachedDist);
                }
                else
                    QualitySettings.shadowDistance = cachedDist;
            }
            catch (Exception e)
            {
                Logging.LogException(e, "Failed to perform shadows distance optimization");
            }
            yield break;
        }
        private static float cachedDist;
        private const float Opt = 512f;
    }

}
