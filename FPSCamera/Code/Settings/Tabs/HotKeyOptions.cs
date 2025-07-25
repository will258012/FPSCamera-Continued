﻿using AlgernonCommons.Keybinding;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using FPSCamera.Utils;
using UnityEngine;


namespace FPSCamera.Settings.Tabs
{
    public sealed class HotKeyOptions
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float LeftMargin = 24f;
        private const float GroupMargin = 40f;

        private static OptionsKeymapping KeyCamToggle;
        private static OptionsKeymapping KeySpeedUp;
        private static OptionsKeymapping KeyCamReset;
        private static OptionsKeymapping KeyCursorToggle;
        private static OptionsKeymapping KeyAutoMove;
        private static OptionsKeymapping KeySaveOffset;
        private static OptionsKeymapping KeyMoveForward;
        private static OptionsKeymapping KeyMoveBackward;
        private static OptionsKeymapping KeyMoveLeft;
        private static OptionsKeymapping KeyMoveRight;
        private static OptionsKeymapping KeyMoveUp;
        private static OptionsKeymapping KeyMoveDown;
        private static OptionsKeymapping KeyRotateLeft;
        private static OptionsKeymapping KeyRotateRight;
        private static OptionsKeymapping KeyRotateUp;
        private static OptionsKeymapping KeyRotateDown;
        private static UUISupport.UUIKeymapping KeyUUIToggle;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotKeyOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal HotKeyOptions(UITabstrip tabStrip, int tabIndex)
        {

            var panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("SETTINGS_GROUPNAME_KEYMAP"), tabIndex, out var _, autoLayout: false);
            var currentY = GroupMargin;

            //Add Scrollbar. 
            UIScrollablePanel scrollPanel = panel.AddUIComponent<UIScrollablePanel>();
            scrollPanel.relativePosition = new Vector2(0, Margin);
            scrollPanel.autoSize = false;
            scrollPanel.autoLayout = false;
            scrollPanel.width = panel.width - 15f;
            scrollPanel.height = panel.height - 15f;
            scrollPanel.clipChildren = true;
            scrollPanel.builtinKeyNavigation = true;
            scrollPanel.scrollWheelDirection = UIOrientation.Vertical;
            //Fix occasional content offset issue when switching pages (Why?)
            scrollPanel.eventVisibilityChanged += (_, isShow) => { if (isShow) scrollPanel.Reset(); };
            UIScrollbars.AddScrollbar(panel, scrollPanel);

            KeyCamToggle = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYCAMTOGGLE"), ModSettings.KeyCamToggle);
            KeyCamToggle.Panel.tooltip = Translations.Translate("SETTINGS_KEYTOGGLE_DETAIL");
            currentY += KeyCamToggle.Panel.height + Margin;

            var KeyWalkThruToggle = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYWALKTHRUTOGGLE"), ModSettings.KeyWalkThruToggle);
            KeyWalkThruToggle.Panel.tooltip = Translations.Translate("SETTINGS_KEYTOGGLE_DETAIL");
            currentY += KeyWalkThruToggle.Panel.height + Margin;

            var KeyFollowToggle = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYFOLLOWTOGGLE"), ModSettings.KeyFollowToggle);
            KeyFollowToggle.Panel.tooltip = Translations.Translate("SETTINGS_KEYFOLLOWTOGGLE_DETAIL");
            currentY += KeyFollowToggle.Panel.height + Margin;

            var KeyInfoPanelToggle = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYINFOPANELTOGGLE"), ModSettings.KeyInfoPanelToggle);
            currentY += KeyInfoPanelToggle.Panel.height + Margin;

            KeySpeedUp = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYSPPEDUP"), ModSettings.KeySpeedUp);
            currentY += KeySpeedUp.Panel.height + Margin;

            KeyCamReset = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYCAMRESET"), ModSettings.KeyCamReset);
            currentY += KeyCamReset.Panel.height + Margin;

            KeyCursorToggle = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYCURSORTOGGLE"), ModSettings.KeyCursorToggle);
            currentY += KeyCursorToggle.Panel.height + Margin;

            KeyAutoMove = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYAUTOMOVE"), ModSettings.KeyAutoMove);
            KeyAutoMove.Panel.tooltip = Translations.Translate("SETTINGS_KEYAUTOMOVE_DETAIL");
            currentY += KeyAutoMove.Panel.height + Margin;

            KeySaveOffset = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYSAVEFOFFSET"), ModSettings.KeySaveOffset);
            KeySaveOffset.Panel.tooltip = Translations.Translate("SETTINGS_KEYSAVEFOFFSET_DETAIL");
            currentY += KeySaveOffset.Panel.height + Margin;

            KeyMoveForward = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYMOVEFORWARD"), ModSettings.KeyMoveForward);
            currentY += KeyMoveForward.Panel.height + Margin;

            KeyMoveBackward = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYMOVEBACKWARD"), ModSettings.KeyMoveBackward);
            currentY += KeyMoveBackward.Panel.height + Margin;

            KeyMoveLeft = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYMOVELEFT"), ModSettings.KeyMoveLeft);
            currentY += KeyMoveLeft.Panel.height + Margin;

            KeyMoveRight = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYMOVERIFHT"), ModSettings.KeyMoveRight);
            currentY += KeyMoveRight.Panel.height + Margin;

            KeyMoveUp = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYMOVEUP"), ModSettings.KeyMoveUp);
            currentY += KeyMoveUp.Panel.height + Margin;

            KeyMoveDown = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYMOVEDOWN"), ModSettings.KeyMoveDown);
            currentY += KeyMoveDown.Panel.height + Margin;

            KeyRotateLeft = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYROTATELEFT"), ModSettings.KeyRotateLeft);
            currentY += KeyRotateLeft.Panel.height + Margin;

            KeyRotateRight = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYROTATERIGHT"), ModSettings.KeyRotateRight);
            currentY += KeyRotateRight.Panel.height + Margin;

            KeyRotateUp = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYROTATEUP"), ModSettings.KeyRotateUp);
            currentY += KeyRotateUp.Panel.height + Margin;

            KeyRotateDown = OptionsKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY, Translations.Translate("SETTINGS_KEYROTATEDOWN"), ModSettings.KeyRotateDown);
            currentY += KeyRotateDown.Panel.height + Margin;

            KeyUUIToggle = UUISupport.UUIKeymapping.AddKeymapping(scrollPanel, LeftMargin, currentY);
        }
    }
}
