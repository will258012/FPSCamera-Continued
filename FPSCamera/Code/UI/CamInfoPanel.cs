﻿using AlgernonCommons;
using AlgernonCommons.Translation;
using FPSCamera.Cam;
using FPSCamera.Cam.Controller;
using FPSCamera.Settings;
using FPSCamera.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace FPSCamera.UI
{
    public class CamInfoPanel : MonoBehaviour
    {
        public static CamInfoPanel Instance { get; private set; }
        public bool UIEnabled { get; set; }
        private void OnEnable()
        {
            elapsedTime = 0f;
            lastBufferStrUpdateTime = tempFooterElapsedTime = -1f;
        }

        private void OnDisable()
        {
            leftInfo.Clear();
            rightInfo.Clear();
        }
        private void SetEnable() { enabled = true; if (ModSettings.ShowInfoPanel) UIEnabled = true; }
        private void SetDisable() { enabled = false; UIEnabled = false; }
        private void Awake()
        {
            Instance = this;
            elapsedTime = 0f; lastBufferStrUpdateTime = tempFooterElapsedTime = -1f;
            mid = footer = "";
            leftInfo = rightInfo = new Dictionary<string, string>();

            panelTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            panelTexture.SetPixel(0, 0, new Color32(45, 40, 105, 200));
            panelTexture.Apply();
            infoFieldTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            infoFieldTexture.SetPixel(0, 0, new Color32(255, 255, 255, 40));
            infoFieldTexture.Apply();
            FPSCamController.OnCameraEnabled += SetEnable;
            FPSCamController.OnCameraDisabled += SetDisable;
            FPSCamController.EventModeSwitched += OnModeSwitched;
            enabled = UIEnabled = false;
        }

        private void Update()
        {
            try
            {
                if (Cam?.IsValid() ?? false)
                {
                    elapsedTime += Time.deltaTime;
                    if (elapsedTime - lastBufferStrUpdateTime > bufferUpdateInterval && UIEnabled)
                    {
                        UpdateStatus();
                        UpdateTargetInfo();
                        UpdateSpeed();

                        if (tempFooterElapsedTime > elapsedTime)
                        {
                            footer = tempFooter;
                        }
                        else
                        {
                            footer = Translations.Translate("INFO_TIME");
                            if (Cam is WalkThruCam walkThruCam)
                            {
                                var time = walkThruCam.GetElapsedTime();
                                footer += $"{(uint)time / 60:00}:{(uint)time % 60:00} / ";
                            }

                            footer += $"{(uint)elapsedTime / 60:00}:{(uint)elapsedTime % 60:00}";
                        }

                        lastBufferStrUpdateTime = elapsedTime;
                    }
                }
                else
                    enabled = false;
            }
            catch (System.Exception e)
            {
                enabled = false;
                Logging.LogException(e, "CamInfoPanel is disabled due to some issues");
            }
        }

        private void OnDestroy()
        {
            FPSCamController.OnCameraEnabled -= SetEnable;
            FPSCamController.OnCameraDisabled -= SetDisable;
            FPSCamController.EventModeSwitched -= OnModeSwitched;
        }
        private void OnModeSwitched(string modeName) => SetFooterMessage(modeName, 2f);
        /// <summary>
        /// Display a temporary message at the info panel's footer.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="duration">Display time.</param>
        public void SetFooterMessage(string message, float duration = 3f)
        {
            tempFooter = message;
            tempFooterElapsedTime = elapsedTime + duration;
        }

        private void UpdateStatus()
        {
            leftInfo = InfoUtils.GetGeoInfo(Cam);

            if (Cam is IFollowCam followcam)
            {
                leftInfo[Translations.Translate("INFO_NAME")] = followcam.GetFollowName();
                var status = followcam.GetStatus();
                if (!string.IsNullOrEmpty(status))
                    leftInfo[Translations.Translate("INFO_STATUS")] = followcam.GetStatus();
                if (Cam is CitizenCam citizenCam)
                {
                    var anotherStatus = citizenCam.AnotherCam?.GetStatus();
                    if (!string.IsNullOrEmpty(anotherStatus))
                        leftInfo[Translations.Translate("INFO_VSTATUS")] = anotherStatus;
                }
            }
        }
        private void UpdateTargetInfo()
        {
            if (Cam is IFollowCam followCam)
            {
                rightInfo = followCam.GetInfo();
            }
        }
        private void UpdateSpeed()
            => mid = string.Format("{0,5:F1} {1}",
                ModSettings.SpeedUnit.IsMile() ? Cam.GetSpeed().ToMph() : Cam.GetSpeed().ToKmph(),
                ModSettings.SpeedUnit.GetSpeedUnitString());

        private void OnGUI()
        {
            if (UIEnabled) DrawPanel();
            else if (tempFooterElapsedTime > elapsedTime) DrawMinimalMessage();
        }

        private void DrawMinimalMessage()
        {
            if (string.IsNullOrEmpty(tempFooter)) return;

            var style = new GUIStyle
            {
                fontSize = (int)(16f * ModSettings.InfoPanelHeightScale),
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 1f, 1f, 0.8f) }
            };

            var textSize = style.CalcSize(new GUIContent(tempFooter));
            float textWidth = textSize.x + 20f;
            float textHeight = 30f * ModSettings.InfoPanelHeightScale;

            float centerX = (Screen.width - textWidth) / 2f;
            float posY = 10f;
            var rect = new Rect(centerX, posY, textWidth, textHeight);

            GUI.Box(rect, panelTexture);
            GUI.Label(rect, tempFooter, style);
        }

        private void DrawPanel()
        {
            var width = (float)Screen.width;
            var height = (Screen.height * heightRatio).Clamp(100f, 800f)
                                                       * ModSettings.InfoPanelHeightScale;

            GUI.Box(new Rect(0f, -10f, width, height + 10f), panelTexture);

            var style = new GUIStyle
            {
                fontSize = (int)(height * fontHeightRatio),
                normal = { textColor = new Color(1f, 1f, 1f, .8f) }
            };

            var margin = (width * marginWidthRatio).Clamp(0f, height * marginHeightRatio);
            var infoMargin = margin * infoMarginRatio;
            var blockWidth = (width - margin) / 5f;
            var infoWidth = blockWidth * 2f - margin;
            var fieldWidth = (infoWidth * fieldWidthRatio)
                                 .Clamp(style.fontSize * 5f, style.fontSize * 8f);
            var textWidth = infoWidth - fieldWidth - margin;

            var rect = new Rect(margin, margin, infoWidth, height - margin);
            style.alignment = TextAnchor.MiddleLeft;
            var columnRect = rect; columnRect.width = fieldWidth;
            DrawInfoFields(leftInfo, style, columnRect, infoMargin);
            columnRect.x += fieldWidth + margin; columnRect.width = textWidth;
            DrawListInRows(leftInfo.Values, style, columnRect, infoMargin);

            rect.x += blockWidth * 3f;
            style.alignment = TextAnchor.MiddleRight;
            columnRect = rect; columnRect.width = textWidth;
            DrawListInRows(rightInfo.Values, style, columnRect, infoMargin);
            columnRect.x += textWidth + margin; columnRect.width = fieldWidth;
            DrawInfoFields(rightInfo, style, columnRect, infoMargin);

            var timerHeight = height / 6f;
            rect.x -= blockWidth; rect.y = 0f;
            rect.height = height - timerHeight; rect.width = blockWidth;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = (int)(style.fontSize * 1.8f);
            GUI.Label(rect, mid, style);

            rect.y += rect.height; rect.height = timerHeight;
            style.fontSize = (int)Mathf.Max(8f, style.fontSize / 2.5f);
            GUI.Label(rect, footer, style);
        }

        private void DrawInfoFields(Dictionary<string, string> info, GUIStyle style, Rect rect, float margin)
        {
            style.normal.background = infoFieldTexture;
            var oAlign = style.alignment;
            var oFontSize = style.fontSize;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = (int)(oFontSize * fieldFontSizeRatio);

            DrawListInRows(info.Keys, style, rect, margin);

            style.normal.background = null;
            style.alignment = oAlign; style.fontSize = oFontSize;
        }
        private void DrawListInRows(IEnumerable<string> strings,
                                     GUIStyle style, Rect rect, float margin)
        {
            var rowHeight = rect.height * infoRowRatio;
            var rowRect = rect; rowRect.height = rowHeight - margin;
            foreach (var str in strings)
            {
                GUI.Label(rowRect, str, style);
                rowRect.y += rowHeight;
            }
        }

        private const float bufferUpdateInterval = .25f;

        private const float heightRatio = .15f;
        private const float fontHeightRatio = .14f;
        private const float marginWidthRatio = .015f;
        private const float marginHeightRatio = .05f;
        private const float infoMarginRatio = .5f;
        private const float infoRowRatio = .2f;
        private const float fieldWidthRatio = .16f;
        private const float fieldFontSizeRatio = .8f;

        private float elapsedTime, lastBufferStrUpdateTime;

        private string tempFooter;
        private float tempFooterElapsedTime;

        private string mid, footer;
        private Dictionary<string, string> leftInfo, rightInfo;
        private Texture2D panelTexture, infoFieldTexture;
        private static IFPSCam Cam => FPSCamController.Instance.FPSCam;
    }
}