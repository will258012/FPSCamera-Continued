namespace FPSCamera.Configuration
{
    using CSkyL.Config;
    using System;
    using UnityEngine;
    using CfFlag = CSkyL.Config.ConfigData<bool>;
    using CfKey = CSkyL.Config.ConfigData<UnityEngine.KeyCode>;
    
    public class Config : Base
    {
        private const string defaultPath = "FPSCameraConfig.xml";
        public static readonly Config G = new Config();  // G: Global config

        public Config() : this(defaultPath) { }
        public Config(string filePath) : base(filePath) { }

        public static Config Load(string path = defaultPath) => Load<Config>(path);

        [Config("Language", "LANGUAGE_CHOICE")]
        public readonly ConfigData<int> Language = new ConfigData<int>(0);

        [Config("HideGameUI", "SETTINGS_HIDEUI")]
        public readonly CfFlag HideGameUI = new CfFlag(false);

        [Config("SetBackCamera", "SETTINGS_SETBACKCAMERA", "SETTINGS_SETBACKCAMERA_DETAIL")]
        public readonly CfFlag SetBackCamera = new CfFlag(true);

        [Config("UseMetricUnit", "SETTINGS_USEMETRICUNIT")]
        public readonly CfFlag UseMetricUnit = new CfFlag(true);

        [Config("ShowInfoPanel", "SETTINGS_SHOWINFOPANEL")]
        public readonly CfFlag ShowInfoPanel = new CfFlag(true);

        [Config("InfoPanelHeightScale", "SETTINGS_INFOPANELHEIGHTSCALE")]
        public readonly CfFloat InfoPanelHeightScale = new CfFloat(1f, min: .5f, max: 2f);

        [Config("MaxPitchDeg", "SETTINGS_MAXPITSHDEG",
                "SETTINGS_MAXPITSHDEG_DETAIL")]
        public readonly CfFloat MaxPitchDeg = new CfFloat(70f, min: 0f, max: 90f);

        // 摄像机控制
        [Config("MovementSpeed", "移动/偏移速度")]
        public readonly CfFloat MovementSpeed = new CfFloat(30f, min: 0f, max: 60f);

        [Config("SpeedUpFactor", "移动/偏移的加速因子")]
        public readonly CfFloat SpeedUpFactor = new CfFloat(4f, min: 1.25f, max: 10f);

        [Config("InvertRotateHorizontal", "反转水平旋转")]
        public readonly CfFlag InvertRotateHorizontal = new CfFlag(false);

        [Config("InvertRotateVertical", "反转垂直旋转")]
        public readonly CfFlag InvertRotateVertical = new CfFlag(false);

        [Config("RotateSensitivity", "摄像机旋转灵敏度")]
        public readonly CfFloat RotateSensitivity = new CfFloat(5f, min: .25f, max: 10f);

        [Config("RotateKeyFactor", "使用键盘旋转速度")]
        public readonly CfFloat RotateKeyFactor = new CfFloat(8f, min: .5f, max: 32f);

        [Config("EnableDOF", "应用景深效果")]
        public readonly CfFlag EnableDof = new CfFlag(false);

        [Config("FieldOfView", "摄像机视野", "摄像机的视野范围（度）")]
        public readonly CfFloat CamFieldOfView = new CfFloat(45f, min: 10f, max: 75f);

        // 自由模式配置
        [Config("ShowCursor4Free", "自由模式下始终显示光标")]
        public readonly CfFlag ShowCursor4Free = new CfFlag(false);

        public enum GroundClipping { None, AboveGround, SnapToGround, AboveRoad, SnapToRoad }

        [Config("GroundClipping", "地面贴靠选项",
                "自由模式下：\n-[None]（无）自由移动\n" +
                "-[AboveGround]（高于地面）摄像机始终在地面上方\n" +
                "-[SnapToGround]（贴近地面）摄像机贴着地面移动\n" +
                "-[AboveRoad]（高于道路）摄像机始终在最近的道路上方\n" +
                "-[SnapToRoad]（贴近道路）摄像机贴着最近的道路或地面移动")]
        public readonly ConfigData<GroundClipping> GroundClippingOption
                                = new ConfigData<GroundClipping>(GroundClipping.AboveGround);

        [Config("GroundLevelOffset", "地面水平偏移",
                "地面裁剪选项的地面水平偏移")]
        public readonly CfFloat GroundLevelOffset = new CfFloat(0f, min: -2f, max: 10f);

        [Config("RoadLevelOffset", "道路水平偏移",
                "地面裁剪选项的道路水平偏移")]
        public readonly CfFloat RoadLevelOffset = new CfFloat(0f, min: -2f, max: 10f);

        // 跟随模式配置
        [Config("ShowCursor4Follow", "跟随/漫游模式下始终显示光标")]
        public readonly CfFlag ShowCursor4Follow = new CfFlag(false);

        [Config("StickToFrontVehicle", "始终跟随前方车辆")]
        public readonly CfFlag StickToFrontVehicle = new CfFlag(true);

        [Config("LookAhead", "前瞻",
                "摄像机朝向目标位置。")]
        public readonly CfFlag LookAhead = new CfFlag(false);

        [Config("InstantMoveMax", "平滑过渡的最小距离",
                "在跟随模式下，即使启用了平滑过渡，摄像机也需要立即跟随目标。\n" +
                "这设置了开始应用平滑过渡的最小距离。")]
        public readonly CfFloat InstantMoveMax = new CfFloat(15f, min: 5f, max: 50f);

        [Config("FollowCamOffset", "跟随模式通用摄像机偏移")]
        public readonly CfOffset FollowCamOffset = new CfOffset(
        new CfFloat(0f, min: -20f, max: 20f),
        new CfFloat(0f, min: -20f, max: 20f),
        new CfFloat(0f, min: -20f, max: 20f)
        );

        // 漫游模式配置
        [Config("Period4Walk", "切换随机目标的周期（秒）")]
        public readonly CfFloat Period4Walk = new CfFloat(20f, min: 5f, max: 300f);

        [Config("ManualSwitch4Walk", "右键手动切换目标",
                "右键点击来切换跟随的目标。")]
        public readonly CfFlag ManualSwitch4Walk = new CfFlag(false);

        [Config("SelectPedestrian", "行走的市民")]
        public readonly CfFlag SelectPedestrian = new CfFlag(true);

        [Config("SelectPassenger", "公共交通工具上的市民")]
        public readonly CfFlag SelectPassenger = new CfFlag(true);

        [Config("SelectWaiting", "等待公共交通工具的市民")]
        public readonly CfFlag SelectWaiting = new CfFlag(true);

        [Config("SelectDriving", "驾驶/乘坐车辆的市民")]
        public readonly CfFlag SelectDriving = new CfFlag(true);

        [Config("SelectPublicTransit", "公共交通工具")]
        public readonly CfFlag SelectPublicTransit = new CfFlag(true);

        [Config("SelectService", "服务车辆")]
        public readonly CfFlag SelectService = new CfFlag(true);

        [Config("SelectCargo", "货运车辆")]
        public readonly CfFlag SelectCargo = new CfFlag(true);

        // 键盘映射
        [Config("KeyCamToggle", "第一人称摄像机切换")]
        public readonly CfKey KeyCamToggle = new CfKey(KeyCode.BackQuote);

        [Config("KeySpeedUp", "加速移动/偏移")]
        public readonly CfKey KeySpeedUp = new CfKey(KeyCode.CapsLock);

        [Config("KeyCamReset", "重置摄像机偏移和旋转")]
        public readonly CfKey KeyCamReset = new CfKey(KeyCode.Backspace);

        [Config("KeyCursorToggle", "切换光标可见性")]
        public readonly CfKey KeyCursorToggle = new CfKey(KeyCode.LeftControl);

        [Config("KeyAutoMove", "自由模式下自动移动",
                "在自由模式下，摄像机会自动向前移动。")]
        public readonly CfKey KeyAutoMove = new CfKey(KeyCode.E);

        [Config("KeySaveOffset", "保存当前摄像机设置为默认值",
                "在跟随/漫游模式下，保存跟随目标的当前摄像机设置")]
        public readonly CfKey KeySaveOffset = new CfKey(KeyCode.Backslash);

        [Config("KeyMoveForward", "向前移动/偏移")]
        public readonly CfKey KeyMoveForward = new CfKey(KeyCode.W);

        [Config("KeyMoveBackward", "向后移动/偏移")]
        public readonly CfKey KeyMoveBackward = new CfKey(KeyCode.S);

        [Config("KeyMoveLeft", "向左移动/偏移")]
        public readonly CfKey KeyMoveLeft = new CfKey(KeyCode.A);

        [Config("KeyMoveRight", "向右移动/偏移")]
        public readonly CfKey KeyMoveRight = new CfKey(KeyCode.D);

        [Config("KeyMoveUp", "向上移动/偏移")]
        public readonly CfKey KeyMoveUp = new CfKey(KeyCode.PageUp);

        [Config("KeyMoveDown", "向下移动/偏移")]
        public readonly CfKey KeyMoveDown = new CfKey(KeyCode.PageDown);

        [Config("KeyRotateLeft", "向左旋转/查看")]
        public readonly CfKey KeyRotateLeft = new CfKey(KeyCode.LeftArrow);

        [Config("KeyRotateRight", "向右旋转/查看")]
        public readonly CfKey KeyRotateRight = new CfKey(KeyCode.RightArrow);

        [Config("KeyRotateUp", "向上旋转/查看")]
        public readonly CfKey KeyRotateUp = new CfKey(KeyCode.UpArrow);

        [Config("KeyRotateDown", "向下旋转/查看")]
        public readonly CfKey KeyRotateDown = new CfKey(KeyCode.DownArrow);

        // 平滑过渡
        [Config("SmoothTransition", "启用平滑过渡",
                "摄像机移动、旋转或缩放时，过渡可以是平滑的或瞬时的。\n" +
                "启用此选项会使摄像机看起来有延迟。")]
        public readonly CfFlag SmoothTransition = new CfFlag(true);

        [Config("TransitionRate", "平滑过渡速率")]
        public readonly CfFloat TransRate = new CfFloat(.5f, min: .1f, max: .9f);

        [Config("GiveUpTransitionDistance", "平滑过渡的最大距离",
                "摄像机目标位置越远，平滑过渡需要的时间越长。\n" +
                "此数字设置关闭平滑过渡的距离。")]
        public readonly CfFloat GiveUpTransDistance = new CfFloat(500f, min: 100f, max: 2000f);

        [Config("DeltaPosMin", "平滑过渡的最小移动")]
        public readonly CfFloat MinTransMove = new CfFloat(.5f, min: .1f, max: 5f);

        [Config("DeltaPosMax", "平滑过渡的最大移动")]
        public readonly CfFloat MaxTransMove = new CfFloat(30f, min: 5f, max: 100f);

        [Config("DeltaRotateMin", "平滑过渡的最小旋转", "单位：度")]
        public readonly CfFloat MinTransRotate = new CfFloat(.1f, min: .05f, max: 5f);

        [Config("DeltaRotateMax", "平滑过渡的最大旋转", "单位：度")]
        public readonly CfFloat MaxTransRotate = new CfFloat(10f, min: 5f, max: 45f);

        /*--------- 位于配置文件中的隐藏设置 ----------------------------------*/

        [Config("MainPanelBtnPos", "游戏主面板按钮位置")]
        public readonly CfScreenPosition MainPanelBtnPos
                = new CfScreenPosition(CSkyL.Math.Vec2D.Position(-1f, -1f));

        [Config("CamNearClipPlane", "摄像机近裁剪平面","(位于配置文件中的隐藏设置)")]
        public readonly CfFloat CamNearClipPlane = new CfFloat(1f, min: .125f, max: 64f);

        [Config("FoViewScrollfactor", "滚轮缩放的视野缩放因子","调整在第一人称摄像机中，滚轮缩放的速度(位于配置文件中的隐藏设置)")]
        public readonly CfFloat FoViewScrollfactor = new CfFloat(1.05f, 1.01f, 2f);

        [Config("VehicleFixedOffset", "车辆的摄像机固定偏移","(位于配置文件中的隐藏设置)")]
        public readonly CfOffset VehicleFixedOffset = new CfOffset(
        new CfFloat(3f,min: -20f, max: 20f), 
        new CfFloat(2f,min: -20f, max: 20f), 
        new CfFloat(0f,min: -20f, max: 20f));

        [Config("MidVehFixedOffset", "车辆中间的摄像机固定偏移","(位于配置文件中的隐藏设置)")]
        public readonly CfOffset MidVehFixedOffset = new CfOffset(
        new CfFloat(-2f,min: -20f, max: 20f), 
        new CfFloat(3f,min: -20f, max: 20f), 
        new CfFloat(0f,min: -20f, max: 20f));

        [Config("PedestrianFixedOffset", "行人的摄像机固定偏移","(位于配置文件中的隐藏设置)")]
        public readonly CfOffset PedestrianFixedOffset = new CfOffset(
        new CfFloat(0f,min: -20f, max: 20f), 
        new CfFloat(2f,min: -20f, max: 20f), 
        new CfFloat(0f,min: -20f, max: 20f));

        [Config("MaxExitingDuration", "退出第一人称摄像机的最大持续时间","(位于配置文件中的隐藏设置)")]
        public readonly CfFloat MaxExitingDuration = new CfFloat(2f,min:.1f,max:10f);
        /*-------------------------------------------------------------------*/

        // Return a ratio[0f, 1f] representing the proportion to advance to the target
        //  *advance ratio per unit(.1 sec): TransRate
        //  *retain ratio per unit: 1f - AdvanceRatioPUnit   *units: elapsedTime / .1f
        //  *retain ratio: RetainRatioPUnit ^ units          *advance ratio: 1f - RetainRatio
        public float GetAdvanceRatio(float elapsedTime)
            => 1f - (float) Math.Pow(1f - TransRate, elapsedTime / .1f);
    }
}
