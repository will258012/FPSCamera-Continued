namespace FPSCamera.Game
{
    using AlgernonCommons.Keybinding;
    using UnityEngine;
    public class InputManager
    {
        /// <summary>
        /// +/-: right/left
        /// </summary>
        public static float MouseMoveHori => Input.GetAxis("Mouse X");
        /// <summary>
        /// +/-: up/down
        /// </summary>
        public static float MouseMoveVert => Input.GetAxis("Mouse Y");
        /// <summary>
        /// +/-: up/down
        /// </summary>
        public static float MouseScroll => Input.GetAxisRaw("Mouse ScrollWheel");

        public static bool MouseTriggered(MouseButton btn)
            => Input.GetMouseButtonDown((int)btn);
        public static bool MousePressed(MouseButton btn)
            => Input.GetMouseButton((int)btn);
        public static bool KeyTriggered(KeyCode key) => Input.GetKeyDown(key);
        public static bool KeyPressed(KeyCode key) => Input.GetKey(key);
        public static bool KeyTriggered(KeyOnlyBinding key) => Input.GetKeyDown((KeyCode)key.Key);
        public static bool KeyPressed(KeyOnlyBinding key) => Input.GetKey((KeyCode)key.Key);

        public enum MouseButton : int { Primary = 0, Secondary = 1, Middle = 2 }
        public static void ToggleCursor(bool visibility) => Cursor.visible = visibility;
    }
}

