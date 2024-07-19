using ColossalFramework.UI;
using CSkyL.Translation;
using CSkyL.UI;
using System.IO;
using UnifiedUI.API;
using UnifiedUI.Helpers;
using UnityEngine;
using static CSkyL.Math;

namespace FPSCamera.UI
{
    internal class UUISupport
    {
        /// <summary>
        /// Property to hold the reference to the UUI button
        /// </summary>
        internal static UIComponent UUIButton { get; set; }

        /// <summary>
        /// Method to update the toggle key for the UUI button
        /// </summary>
        internal static void UpdateToggleKey()
        {
            // Try to get the keymap from the config
            var v = TryGetKey(out var keyCode, out var control, out var shift, out var alt);
            if (v) {
                // Save new keymap
                ToggleKey.value = ColossalFramework.SavedInputKey.Encode(keyCode, control, shift, alt);
                // Update UUI button's tooltip to show correct keymap prompt
                UUIButton.tooltip = $"{Translations.Translate("MAINPANELBTN_TOOLTIP")} ({ToggleKey})";
            }
        }

        /// <summary>
        /// Define the default toggle key configuration
        /// </summary>
        internal static ColossalFramework.SavedInputKey ToggleKey = new ColossalFramework.SavedInputKey("KeyUUIToggle", "FPSCamera", KeyCode.F, false, true, false, false);

        /// <summary>
        /// Method to register the UUI button
        /// </summary>
        internal static void UUIRegister()
        {
            try {
                CSkyL.Log.Msg("UUISupport: Registering UUI button");
                // Subscribe the UpdateToggleKey method to the event
                UUIKeySetting.UpdateToggleKeyEvent += UpdateToggleKey;

                // Register the UUI button with specified properties
                UUIButton = UUIAPI.Register(
                    name: "MainPanelBtn",
                    groupName: null,
                    tooltip: Translations.Translate("MAINPANELBTN_TOOLTIP"),
                    texture: UUIHelpers.LoadTexture(Path.Combine(Utils.AssemblyPath, "Resources/icon.png")),
                    onToggle: (value) => {
                        var UUIpos = UnifiedUI.GUI.MainPanel.Instance.absolutePosition;
                        var UUIwidth = UnifiedUI.GUI.MainPanel.Instance.width;
                        var UUIheight = UnifiedUI.GUI.MainPanel.Instance.height;
                        var panel = MainPanel.Instance._mainPanel;
                        // Position the main panel properly based on UUI button position
                        panel.position = Vec2D.Position(
                        UUIpos.x + (UUIpos.x < Helper.ScreenWidth / 2f ?
                        UUIwidth - 10f : -panel.width + 10f),
                        UUIpos.y + (UUIpos.y < Helper.ScreenHeight / 2f ?
                        UUIheight - 15f : -panel.height + 15f));
                        // Set main panel visibility
                        panel.Visible = value;
                    },
                    onToolChanged: null,
                    activationKey: ToggleKey,
                    activeKeys: null);

                // Update keymap after setting the button
                UpdateToggleKey();

                // Customize button appearance if it's a UIButton
                if (UUIButton is UIButton btn) {
                    btn.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
                    btn.scaleFactor = .75f;
                    btn.color = btn.focusedColor = Style.basic.color.ToColor32();
                    btn.hoveredColor = Style.basic.textColor.ToColor32();
                    btn.pressedColor = Style.basic.bgColor.ToColor32();
                    btn.disabledColor = Style.basic.colorDisabled.ToColor32();
                }
            }
            catch (System.Exception e) {
                CSkyL.Log.Err("UUISupport: " + e.ToString());
            }
        }

        /// <summary>
        /// Method to get the key configuration from the config
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="control"></param>
        /// <param name="shift"></param>
        /// <param name="alt"></param>
        /// <returns></returns>
        private static bool TryGetKey(out KeyCode keyCode, out bool control, out bool shift, out bool alt)
        {
            control = alt = shift = false;
            try {
                var keys = Config.Config.instance.KeyUUIToggle;
                keyCode = keys.Key;
                if (keys.Key == KeyCode.None && keys.Modifiers == EventModifiers.None) return false; // Config does not bind keys
                switch (keys.Modifiers) {
                case EventModifiers.Shift: { shift = true; break; }
                case EventModifiers.Alt: { alt = true; break; }
                case EventModifiers.Control: { control = true; break; }
                }
                return true;
            }
            catch (System.Exception e) {
                CSkyL.Log.Err("UUISupport: " + e.ToString());
                keyCode = KeyCode.None;
                return false;
            }
        }
    }
}
