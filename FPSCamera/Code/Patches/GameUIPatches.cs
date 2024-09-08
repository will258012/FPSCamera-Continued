﻿using ColossalFramework;
using ColossalFramework.UI;
using FPSCamera.Utils;
using HarmonyLib;
using UnityEngine;
/// UIView.Show() is TOO LAGGING.
/// so here we replace it with UIView.GetAView().uiCamera.enabled. Same effect but much faster.
namespace FPSCamera.Patches.GameUIPatches
{
    [HarmonyPatch(typeof(CameraController), "UpdateFreeCamera")]
    internal class UpdateFreeCameraPatch
    {
        internal static bool Prefix(CameraController __instance)
        {
            if (__instance.m_freeCamera != AccessUtils.GetFieldValue<bool>(__instance, "m_cachedFreeCamera"))
            {
                AccessUtils.SetFieldValue(__instance, "m_cachedFreeCamera", __instance.m_freeCamera);
                UIView.GetAView().uiCamera.enabled = UIView.HasModalInput() || !__instance.m_freeCamera; //UIView.Show(UIView.HasModalInput() || !m_freeCamera);
                Singleton<NotificationManager>.instance.NotificationsVisible = !__instance.m_freeCamera;
                Singleton<GameAreaManager>.instance.BordersVisible = !__instance.m_freeCamera;
                Singleton<DistrictManager>.instance.NamesVisible = !__instance.m_freeCamera;
                Singleton<PropManager>.instance.MarkersVisible = !__instance.m_freeCamera;
                Singleton<GuideManager>.instance.TutorialDisabled = __instance.m_freeCamera;
                Singleton<DisasterManager>.instance.MarkersVisible = !__instance.m_freeCamera;
                Singleton<NetManager>.instance.RoadNamesVisible = !__instance.m_freeCamera;
            }

            Camera.main.rect = AccessUtils.GetFieldValue<bool>(__instance, "m_cachedFreeCamera")
                ? CameraController.kFullScreenRect
                : CameraController.kFullScreenWithoutMenuBarRect;
            return false;
        }
    }

    [HarmonyPatch(typeof(CinematicCameraController), nameof(CinematicCameraController.SetUIVisible))]
    internal class SetUIVisiblePatch
    {
        internal static bool Prefix(bool visible)
        {
            UIView.GetAView().uiCamera.enabled = visible; // UIView.Show(visible);
            Singleton<NotificationManager>.instance.NotificationsVisible = visible;
            Singleton<GameAreaManager>.instance.BordersVisible = visible;
            Singleton<DistrictManager>.instance.NamesVisible = visible;
            Singleton<PropManager>.instance.MarkersVisible = visible;
            Singleton<GuideManager>.instance.TutorialDisabled = !visible;
            Singleton<DisasterManager>.instance.MarkersVisible = visible;
            Singleton<NetManager>.instance.RoadNamesVisible = visible;
            Camera.main.rect = visible ? CameraController.kFullScreenWithoutMenuBarRect : CameraController.kFullScreenRect;
            return false;
        }
    }
}