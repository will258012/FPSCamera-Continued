using AlgernonCommons;
using AlgernonCommons.Translation;
using ColossalFramework;
using FPSCamera.Cam;
using FPSCamera.Cam.Controller;
using FPSCamera.Settings;
using FPSCamera.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FPSCamera.UI
{
    public class CamInfoPanel : MonoBehaviour
    {
        public static CamInfoPanel Instance { get; private set; }
        public bool UIEnabled
        {
            get => field;
            set
            {
                var wasUIEnabled = field;
                field = value;
                if (value)
                {
                    fadeDrawMode = GetPanelDrawMode();
                    fadeHelper.FadeIn();
                }
                else if (!HasMinimalMessage || !FPSCamController.Instance.Status.IsFlagSet(FPSCamController.CamStatus.Enabled))
                {
                    if (wasUIEnabled)
                        fadeDrawMode = GetPanelDrawMode();
                    fadeHelper.FadeOut();
                }
                else fadeDrawMode = DrawMode.MinimalMessage;
            }
        }
        private void Awake()
        {
            Instance = this;
            elapsedTime = 0f; lastBufferStrUpdateTime = tempFooterElapsedTime = -1f;
            mid = footer = "";
            slope = 0f;
            leftInfo = rightInfo = [];

            panelTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            panelTexture.SetPixel(0, 0, new Color32(45, 40, 105, 200));
            panelTexture.Apply();
            infoFieldTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            infoFieldTexture.SetPixel(0, 0, new Color32(255, 255, 255, 40));
            infoFieldTexture.Apply();
            FPSCamController.OnCameraEnabled += SetEnable;
            FPSCamController.OnCameraDisabled += SetDisable;
            FPSCamController.EventModeSwitched += OnModeSwitched;
            fadeHelper.OnFadeCompleted += OnFadeCompleted;
            enabled = UIEnabled = false;
        }
        private void OnEnable()
        {
            elapsedTime = 0f;
            elapsedSimTime = TimeSpan.Zero;
            lastGameTime = SimulationManager.instance.m_currentGameTime;
            tempFooter = null;
            lastBufferStrUpdateTime = tempFooterElapsedTime = -1f;
        }

        private void OnDisable()
        {
            leftInfo.Clear();
            rightInfo.Clear();
            fadeHelper.Reset();
        }
        private void SetEnable()
        {
            enabled = true;
            UIEnabled = ModSettings.ShowInfoPanel;
        }

        private void SetDisable()
        {
            UIEnabled = false;
        }

        private void OnDestroy()
        {
            FPSCamController.OnCameraEnabled -= SetEnable;
            FPSCamController.OnCameraDisabled -= SetDisable;
            FPSCamController.EventModeSwitched -= OnModeSwitched;
            fadeHelper.OnFadeCompleted -= OnFadeCompleted;
        }
        private void OnModeSwitched(string modeName)
        {
            SetFooterMessage(modeName, 2f);
            leftInfo.Clear();
            rightInfo.Clear();
        }
        private void OnFadeCompleted(FadeHelper.FadeType fadeType)
        {
            if (fadeType == FadeHelper.FadeType.Out && !FPSCamController.Instance.Status.IsFlagSet(FPSCamController.CamStatus.Enabled))
                enabled = false;
            if (HasMinimalMessage && elapsedTime >= tempFooterElapsedTime) ClearFooterMessage();
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
                        if (ModSettings.ShowStatus)
                        {
                            UpdateStatus();
                            UpdateTargetInfo();
                        }
                        UpdateSpeed();

                        if (tempFooterElapsedTime > elapsedTime)
                        {
                            footer = tempFooter;
                        }
                        else
                        {
                            footer = string.Empty;
                            if (ModSettings.ShowElapsedTime || ModSettings.ShowInGameTime) footer = Translations.Translate("INFO_TIME");
                            if (ModSettings.ShowElapsedTime)
                            {
                                if (Cam is WalkThruCam walkThruCam)
                                {
                                    var time = walkThruCam.GetElapsedTime();
                                    footer += $"{(uint)time / 60:00}:{(uint)time % 60:00} / ";
                                }

                                footer += $"{(uint)elapsedTime / 60:00}:{(uint)elapsedTime % 60:00}";
                            }
                            if (ModSettings.ShowInGameTime)
                            {
                                if (ModSettings.ShowElapsedTime)
                                    footer += " / ";
                                footer += SimulationManager.instance.m_currentGameTime.ToString("HH:mm:ss");
                            }
                            if (ModSettings.ShowSlope && !FPSCamController.Instance.Status.IsFlagSet(FPSCamController.CamStatus.PluginEnabled))
                            {
                                UpdateSlope();
                                if (!string.IsNullOrEmpty(footer)) footer += "\n";
                                footer += $"{Translations.Translate("INFO_SLOPE")}{slope:F1}°";
                            }
                        }

                        lastBufferStrUpdateTime = elapsedTime;
                    }
                }
                else if (!UIEnabled && !string.IsNullOrEmpty(tempFooter))
                {
                    elapsedTime += Time.deltaTime;
                }
                else if (fadeHelper.Status == FadeHelper.FadeType.None)
                    enabled = false;
            }
            catch (System.Exception e)
            {
                enabled = false;
                Logging.LogException(e, "CamInfoPanel is disabled due to some issues");
            }
        }

        private void LateUpdate()
        {
            try
            {
                if (HasMinimalMessage && elapsedTime >= tempFooterElapsedTime)
                {
                    if (!UIEnabled)
                    {
                        if (!fadeHelper.IsFading)
                            fadeHelper.FadeOut();
                    }
                    else ClearFooterMessage();
                }
            }
            catch (System.Exception e)
            {
                enabled = false;
                Logging.LogException(e, "CamInfoPanel is disabled due to some issues");
            }
        }


        /// <summary>
        /// Display a temporary message at the info panel's footer.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="duration">Display time.</param>
        public void SetFooterMessage(string message, float duration = 3f)
        {
            tempFooter = message;
            tempFooterElapsedTime = elapsedTime + duration;

            if (!UIEnabled)
            {
                fadeDrawMode = DrawMode.MinimalMessage;
                fadeHelper.FadeIn();
            }
        }
        /// <summary>
        /// Remove the footer temporary message immediately.
        /// </summary>
        public void ClearFooterMessage()
        {
            tempFooter = null;
            tempFooterElapsedTime = -1f;
        }
        private bool HasMinimalMessage => !string.IsNullOrEmpty(tempFooter);

        private void UpdateStatus()
        {
            leftInfo = InfoUtils.GetGeoInfo(Cam);

            if (Cam is IFollowCam followCam)
            {
                var name = followCam.FollowName;
                if (!string.IsNullOrEmpty(name))
                    leftInfo[Translations.Translate("INFO_NAME")] = name;

                var status = followCam.GetStatus();
                if (!string.IsNullOrEmpty(status))
                    leftInfo[Translations.Translate("INFO_STATUS")] = status;

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
            => mid = string.Format("{0:F1} {1}",
                ModSettings.SpeedUnit.IsMile() ? Cam.GetSpeed().ToMph() : Cam.GetSpeed().ToKmph(),
                ModSettings.SpeedUnit.GetSpeedUnitString());

        private void UpdateSlope()
        {
            if (Cam is IFollowCam followCam)
                slope = -Mathf.DeltaAngle(0f, followCam.GetPositioning().rotation.eulerAngles.x);
            else if (Cam is FreeCam freeCam)
            {
                var velocity = freeCam.Velocity;
                var horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;

                slope = horizontalSpeed > 0.01f
                    ? Mathf.Atan2(velocity.y, horizontalSpeed) * Mathf.Rad2Deg
                    : slope;
            }
        }

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint || fadeHelper.Opacity <= 0f)
                return;

            var originalColor = GUI.color;
            GUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a * fadeHelper.Opacity);
            try
            {
                switch (UIEnabled && !fadeHelper.IsFading ? GetPanelDrawMode() : fadeDrawMode)
                {
                    case DrawMode.MinimalMessage:
                        DrawMinimalMessage();
                        break;
                    case DrawMode.Panel:
                        DrawPanel();
                        break;
                    case DrawMode.CompactPanel:
                        DrawCompactPanel();
                        break;
                }
            }
            finally
            {
                GUI.color = originalColor;
            }
        }
        private static DrawMode GetPanelDrawMode() => ModSettings.ShowStatus ? DrawMode.Panel : DrawMode.CompactPanel;

        private void DrawMinimalMessage()
        {
            var style = new GUIStyle
            {
                fontSize = (int)(16f * ModSettings.InfoPanelHeightScale),
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 1f, 1f, .8f) }
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
            var fieldWidth = (infoWidth * fieldWidthRatio).Clamp(style.fontSize * 5f, style.fontSize * 8f);

            var measureStyle = new GUIStyle(style)
            {
                fontSize = (int)(style.fontSize * fieldFontSizeRatio)
            };

            var leftFieldWidth = Mathf.Max(fieldWidth, GetMaxFieldWidth(leftInfo.Keys, measureStyle) + measureStyle.fontSize);
            var rightFieldWidth = Mathf.Max(fieldWidth, GetMaxFieldWidth(rightInfo.Keys, measureStyle) + measureStyle.fontSize);

            var rect = new Rect(margin, margin, infoWidth, height - margin);

            // LEFT
            style.alignment = TextAnchor.MiddleLeft;
            var columnRect = rect;
            columnRect.width = leftFieldWidth;
            DrawInfoFields(leftInfo, style, columnRect, infoMargin);

            columnRect.x += leftFieldWidth + margin;
            columnRect.width = infoWidth - leftFieldWidth - margin;
            DrawListInRows(leftInfo.Values, style, columnRect, infoMargin);

            // RIGHT
            rect.x += blockWidth * 3f;
            style.alignment = TextAnchor.MiddleRight;
            columnRect = rect;
            columnRect.width = infoWidth - rightFieldWidth - margin;
            DrawListInRows(rightInfo.Values, style, columnRect, infoMargin);

            columnRect.x += columnRect.width + margin;
            columnRect.width = rightFieldWidth;
            DrawInfoFields(rightInfo, style, columnRect, infoMargin);

            var footerLines = string.IsNullOrEmpty(footer) ? 0 : footer.Split('\n').Length;
            var timerHeight = GetFooterHeight(height, footerLines);
            rect = new Rect((width - blockWidth) / 2f, 0f, blockWidth, height - timerHeight);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = (int)(style.fontSize * 1.8f);
            GUI.Label(rect, mid, style);

            if (footerLines > 0)
            {
                rect.y += rect.height; rect.height = timerHeight;
                style.fontSize = (int)Mathf.Max(8f, style.fontSize / 2.5f);
                GUI.Label(rect, footer, style);
            }
        }

        private void DrawCompactPanel()
        {
            var height = (Screen.height * heightRatio).Clamp(100f, 800f)
                                                       * ModSettings.InfoPanelHeightScale;
            var baseStyle = new GUIStyle
            {
                fontSize = (int)(height * fontHeightRatio),
                normal = { textColor = new Color(1f, 1f, 1f, .8f) }
            };
            var speedStyle = new GUIStyle(baseStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = (int)(baseStyle.fontSize * 1.8f)
            };
            var footerStyle = new GUIStyle(speedStyle)
            {
                fontSize = (int)Mathf.Max(8f, speedStyle.fontSize / 2.5f)
            };

            var speedSize = speedStyle.CalcSize(new GUIContent(mid));
            var footerSize = footerStyle.CalcSize(new GUIContent(footer));
            var padding = Mathf.Max(8f, height * .06f);
            var panelWidth = Mathf.Max(speedSize.x, footerSize.x) + (padding * 2f);
            panelWidth = Mathf.Min(panelWidth, Screen.width - (padding * 2f));

            var speedHeight = speedSize.y + (padding * 2f);
            var footerLines = string.IsNullOrEmpty(footer) ? 0 : footer.Split('\n').Length;
            var timerHeight = GetFooterHeight(height, footerLines);
            var panelX = (Screen.width - panelWidth) / 2f;
            var panelY = (height * .375f) - (speedHeight / 2f);

            GUI.Box(new Rect(panelX, panelY, panelWidth, speedHeight + timerHeight), panelTexture);
            GUI.Label(new Rect(panelX, panelY, panelWidth, speedHeight), mid, speedStyle);

            if (timerHeight > 0f)
                GUI.Label(new Rect(panelX, panelY + speedHeight, panelWidth, timerHeight), footer, footerStyle);
        }

        private static float GetFooterHeight(float panelHeight, int lineCount)
            => lineCount == 0 ? 0f : panelHeight * (Mathf.Min(lineCount, 3) + 1f) / 12f;

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
                if (str.Contains("<color"))
                {
                    GUIStyle newStyle = new(style)
                    {
                        richText = true
                    };
                    GUI.Label(rowRect, str, newStyle);
                }
                else
                    GUI.Label(rowRect, str, style);
                rowRect.y += rowHeight;
            }
        }
        private float GetMaxFieldWidth(IEnumerable<string> keys, GUIStyle style)
        {
            float maxWidth = 0f;
            var content = new GUIContent();
            foreach (var key in keys)
            {
                content.text = key;
                var size = style.CalcSize(content);
                maxWidth = Mathf.Max(maxWidth, size.x);
            }

            return maxWidth;
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
        private TimeSpan elapsedSimTime;
        private DateTime lastGameTime;
        private float slope;

        private readonly FadeHelper fadeHelper = new InfoPanelFadeHelper();
        private DrawMode fadeDrawMode;

        private string tempFooter;
        private float tempFooterElapsedTime;

        private string mid, footer;
        private Dictionary<string, string> leftInfo, rightInfo;
        private Texture2D panelTexture, infoFieldTexture;

        private sealed class InfoPanelFadeHelper : FadeHelper
        {
            public override string FadeID => Mod.Instance.HarmonyID + ".CamInfoPanel.Fade";
            public override float Opacity { get; set; }
        }

        private enum DrawMode
        {
            None,
            MinimalMessage,
            Panel,
            CompactPanel,
        }


        private static IFPSCam Cam => FPSCamController.Instance.FPSCam;

    }
}
