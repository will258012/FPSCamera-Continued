namespace FPSCamera.UI
{
    using AlgernonCommons.Translation;
    using FPSCamera.Cam;
    using FPSCamera.Cam.Controller;
    using FPSCamera.Settings;
    using FPSCamera.Utils;
    using System.Collections.Generic;
    using UnityEngine;

    public class CamInfoPanel : MonoBehaviour
    {
        public static CamInfoPanel Instance { get; private set; }
        public void EnableCamInfoPanel()
        {
            _elapsedTime = 0f;
            _lastBufferStrUpdateTime = -1f;
            enabled = true;
        }

        public void DisableCamInfoPanel()
        {
            _leftInfos.Clear();
            _rightInfos.Clear();
            enabled = false;
        }
        private void Awake()
        {
            Instance = this;
            _elapsedTime = 0f; _lastBufferStrUpdateTime = -1f;
            _mid = _footer = "";
            _leftInfos = _rightInfos = new Dictionary<string, string>();

            _panelTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _panelTexture.SetPixel(0, 0, new Color32(45, 40, 105, 200));
            _panelTexture.Apply();
            _infoFieldTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _infoFieldTexture.SetPixel(0, 0, new Color32(255, 255, 255, 40));
            _infoFieldTexture.Apply();

            enabled = false;
        }

        private void Update()
        {
            if (Cam.IsVaild())
            {
                _elapsedTime += Time.deltaTime;
                if (_elapsedTime - _lastBufferStrUpdateTime > _bufferUpdateInterval)
                {
                    UpdateStatus(); UpdateTargetInfos(); UpdateSpeed();

                    _footer = Translations.Translate("INFO_TIME");
                    if (Cam is WalkThruCam walkThruCam)
                    {
                        var time = walkThruCam.GetElapsedTime();
                        _footer += $"{(uint)time / 60:00}:{(uint)time % 60:00} / ";
                    }

                    _footer += $"{(uint)_elapsedTime / 60:00}:{(uint)_elapsedTime % 60:00}";
                    _lastBufferStrUpdateTime = _elapsedTime;
                }
            }
            else
            {
                _leftInfos.Clear(); _rightInfos.Clear();
                _footer = ""; _mid = Translations.Translate("INVALID");
            }
        }

        private void UpdateStatus()
        {
            _leftInfos.Clear();

            _leftInfos = InfosUtils.GetGeoInfos(Cam);

            if (Cam is IFollowCam followcam)
            {
                _leftInfos[Translations.Translate("INFO_NAME")] = followcam.GetFollowName();
                var status = followcam.GetStatus();
                if (!string.IsNullOrEmpty(status))
                    _leftInfos[Translations.Translate("INFO_STATUS")] = followcam.GetStatus();
                if (Cam is CitizenCam citizenCam)
                {
                    var anotherStatus = citizenCam.GetAnotherCamStatus();
                    if (!string.IsNullOrEmpty(anotherStatus))
                        _leftInfos[Translations.Translate("INFO_VSTATUS")] = anotherStatus;
                }
            }
        }
        private void UpdateTargetInfos()
        {
            if (Cam is IFollowCam followCam)
            {
                _rightInfos = followCam.GetInfos();
            }
            else _rightInfos.Clear();
        }
        private void UpdateSpeed()
            => _mid = string.Format("{0,5:F1} {1}",
                ModSettings.UseMetricUnit ? Cam.GetSpeed().ToKilometer() : Cam.GetSpeed().ToMile(),
                ModSettings.UseMetricUnit ? "km/h" : "mph");

        private void OnGUI()
        {
            var width = (float)Screen.width;
            var height = (Screen.height * _heightRatio).Clamp(100f, 800f)
                                                       * ModSettings.InfoPanelHeightScale;

            GUI.Box(new Rect(0f, -10f, width, height + 10f), _panelTexture);

            var style = new GUIStyle
            {
                fontSize = (int)(height * _fontHeightRatio)
            };
            style.normal.textColor = new Color(1f, 1f, 1f, .8f);

            var margin = (width * _marginWidthRatio).Clamp(0f, height * _marginHeightRatio);
            var infoMargin = margin * _infoMarginRatio;
            var blockWidth = (width - margin) / 5f;
            var infoWidth = blockWidth * 2f - margin;
            var fieldWidth = (infoWidth * _fieldWidthRatio)
                                 .Clamp(style.fontSize * 5f, style.fontSize * 8f);
            var textWidth = infoWidth - fieldWidth - margin;

            var rect = new Rect(margin, margin, infoWidth, height - margin);
            style.alignment = TextAnchor.MiddleLeft;
            var columnRect = rect; columnRect.width = fieldWidth;
            DrawInfoFields(_leftInfos, style, columnRect, infoMargin);
            columnRect.x += fieldWidth + margin; columnRect.width = textWidth;
            DrawListInRows(_leftInfos.Values, style, columnRect, infoMargin);

            rect.x += blockWidth * 3f;
            style.alignment = TextAnchor.MiddleRight;
            columnRect = rect; columnRect.width = textWidth;
            DrawListInRows(_rightInfos.Values, style, columnRect, infoMargin);
            columnRect.x += textWidth + margin; columnRect.width = fieldWidth;
            DrawInfoFields(_rightInfos, style, columnRect, infoMargin);

            var timerHeight = height / 6f;
            rect.x -= blockWidth; rect.y = 0f;
            rect.height = height - timerHeight; rect.width = blockWidth;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = (int)(style.fontSize * 1.8f);
            GUI.Label(rect, _mid, style);

            rect.y += rect.height; rect.height = timerHeight;
            style.fontSize = (int)Mathf.Max(8f, style.fontSize / 2.5f);
            GUI.Label(rect, _footer, style);
        }

        private void DrawInfoFields(Dictionary<string, string> infos, GUIStyle style, Rect rect, float margin)
        {
            style.normal.background = _infoFieldTexture;
            var oAlign = style.alignment;
            var oFontSize = style.fontSize;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = (int)(oFontSize * _fieldFontSizeRatio);

            DrawListInRows(infos.Keys, style, rect, margin);

            style.normal.background = null;
            style.alignment = oAlign; style.fontSize = oFontSize;
        }
        private void DrawListInRows(IEnumerable<string> strings,
                                     GUIStyle style, Rect rect, float margin)
        {
            var rowHeight = rect.height * _infoRowRatio;
            var rowRect = rect; rowRect.height = rowHeight - margin;
            foreach (var str in strings)
            {
                GUI.Label(rowRect, str, style);
                rowRect.y += rowHeight;
            }
        }

        private const float _bufferUpdateInterval = .25f;

        private const float _heightRatio = .15f;
        private const float _fontHeightRatio = .14f;
        private const float _marginWidthRatio = .015f;
        private const float _marginHeightRatio = .05f;
        private const float _infoMarginRatio = .5f;
        private const float _infoRowRatio = .2f;
        private const float _fieldWidthRatio = .16f;
        private const float _fieldFontSizeRatio = .8f;


        private float _elapsedTime, _lastBufferStrUpdateTime;

        private string _mid, _footer;
        private Dictionary<string, string> _leftInfos, _rightInfos;
        private Texture2D _panelTexture, _infoFieldTexture;
        private IFPSCam Cam => FPSCamController.Instance.FPSCam;
    }
}