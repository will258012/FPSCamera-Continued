using ColossalFramework.UI;
using UnityEngine;

namespace FPSCamera.Utils
{
    public static class GameUtils
    {
        public static float ScreenWidth => PrivateField.GetValue<Vector2>(UIView.GetAView(), "m_CachedScreenResolution").x;
        public static float ScreenHeight => PrivateField.GetValue<Vector2>(UIView.GetAView(), "m_CachedScreenResolution").y;
        public static bool InGameMode
    => ToolManager.instance is ToolManager m && m.m_properties.m_mode == ItemClass.Availability.Game;
    }
}
