using AlgernonCommons.Keybinding;
using System;
using UnityEngine;

namespace FPSCamera.Game
{
    public static class InputManager
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

        public static bool MouseTriggered(this MouseButton btn)
            => Input.GetMouseButtonDown((int)btn);
        public static bool MousePressed(this MouseButton btn)
            => Input.GetMouseButton((int)btn);
        public static bool KeyTriggered(this KeyCode key) => Input.GetKeyDown(key);
        public static bool KeyPressed(this KeyCode key) => Input.GetKey(key);
        public static bool KeyTriggered(this KeyOnlyBinding key) => Input.GetKeyDown((KeyCode)key.Key);
        public static bool KeyTriggered(this Keybinding key)
        {
            // Check primary key.
            if (!Input.GetKeyDown((KeyCode)key.Key))
            {
                return false;
            }

            // Check modifier keys.
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) != key.Control)
            {
                return false;
            }

            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) != key.Shift)
            {
                return false;
            }

            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr)) != key.Alt)
            {
                return false;
            }

            // If we got here, all checks passed.
            return true;
        }

        [Obsolete("Use AlgernonCommons.Keybinding.KeyOnlyBinding.IsPressed() instead")]
        public static bool KeyPressed(KeyOnlyBinding key) => key.IsPressed();

        public enum MouseButton : int { Primary = 0, Secondary = 1, Middle = 2 }
        public static void ToggleCursor(bool visibility)
        {
            Cursor.visible = visibility;
            Cursor.lockState = !visibility ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}

