namespace FPSCamera.Utils
{
    public static class GameUtils
    {
        public static bool InGameMode
    => ToolManager.instance is ToolManager m && m.m_properties.m_mode == ItemClass.Availability.Game;
    }
}
