using ColossalFramework.UI;
using CSkyL.Translation;
using CSkyL.UI;
using System.IO;
using UnifiedUI.API;
using UnifiedUI.Helpers;
using static CSkyL.Math;

namespace FPSCamera.UI
{
    internal class UUISupport
    {
        internal static UIComponent UUIButton { get; set; }
        internal static void UUIRegister()
        {
            try {
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
                        panel.position = Vec2D.Position(
                        UUIpos.x + (UUIpos.x < Helper.ScreenWidth / 2f ?
                        UUIwidth - 10f : -panel.width + 10f),
                        UUIpos.y + (UUIpos.y < Helper.ScreenHeight / 2f ?
                        UUIheight - 15f : -panel.height + 15f));
                        panel.Visible = value;
                    },
                    onToolChanged: null,
                    activationKey: null,
                    activeKeys: null);

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
                CSkyL.Log.Err(e.ToString());
            }
        }
    }
}
