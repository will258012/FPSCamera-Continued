using ColossalFramework.Globalization;
using System.Collections.Generic;
namespace FPSCameraAPI
{
    internal static class L10n
    {
        private static bool IsZh => LocaleManager.exists && LocaleManager.instance.language == "zh";

        private static readonly Dictionary<string, string> EnTranslations = new()
        {
            ["MissingDependencyTitle"] = "Missing dependency: First Person Camera - Continued",
            ["MissingDependencyMessage"] = "The dependency 'First Person Camera - Continued' is required for some mod(s) to work correctly.\n Do you want to subscribe to it right now?",
            ["DependencyNotEnabledTitle"] = "Dependency not enabled: First Person Camera - Continued",
            ["DependencyNotEnabledMessage"] = "The dependency 'First Person Camera - Continued' is not enabled.\n Do you want to enable it right now?",
            ["SuccessSub"] = "First Person Camera - Continued has been installed successfully. It is recommended to restart the game now!",
            ["ErrorSub"] = "An error occured while attempting to automatically subscribe to First Person Camera - Continued (no network connection?)",
            ["ManualDownload"] = "You can manually download the First Person Camera - Continued mod from github.com/will258012/FPSCamera-Continued/releases",
            ["ThenManualDownload"] = "then manually download the First Person Camera - Continued mod from github.com/will258012/FPSCamera-Continued/releases",
            ["RejectSub"] = "You have rejected to automatically subscribe to First Person Camera - Continued :(",
            ["RejectSubSln"] = "Either unsubscribe those mods or subscribe to the First Person Camera - Continued mod, then restart the game!",
            ["SuccessEnabled"] = "First Person Camera - Continued has been enabled successfully.",
            ["ErrorEnable"] = "An error occured while attempting to enable First Person Camera - Continued",
            ["ManualEnable"] = "You could manually enable it in Content Manager",
            ["RejectEnable"] = "You have rejected to enable First Person Camera - Continued :(",
            ["RejectEnableSln"] = "Either unsubscribe those mods or enable the First Person Camera - Continued mod",
            ["ShowError"] = "The mod(s):\n{0}require the dependency 'First Person Camera - Continued' to work correctly!\n\n{1}\n\nClose the game, {2}",
            ["NotOnSteam"] = "First Person Camera - Continued could not be installed automatically because you are using a platform other than Steam.",
            ["NoWorkShop"] = "First Person Camera - Continued could not be auto-subscribed because you are playing in --noWorkshop mode!",
            ["NoWorkShopSln"] = "then restart without --noWorkshop \n or manually download and install the First Person Camera - Continued mod from github.com/will258012/FPSCamera-Continued/releases",
            ["WorkShopNotAvailable"] = "First Person Camera - Continued could not be installed automatically because the Steam workshop is not available (no network connection?)",
            ["AssemblyNotLoad"] = "It seems that First Person Camera - Continued has already been subscribed, \nbut Steam failed to download the files correctly or they were deleted,\nor the mod is not loaded in Skyve",
            ["AssemblyNotLoadSln"] = "check your network connections \nOr (re)subscribe to the First Person Camera - Continued workshop item\nOr check the status in Skyve (if you have)",
        };

        private static readonly Dictionary<string, string> ZhTranslations = new()
        {
            ["MissingDependencyTitle"] = "缺少前置：First Person Camera - Continued",
            ["MissingDependencyMessage"] = "前置 First Person Camera - Continued 是某些模组正常运行所必需的。\n是否立即订阅？",
            ["DependencyNotEnabledTitle"] = "前置未启用：First Person Camera - Continued",
            ["DependencyNotEnabledMessage"] = "前置 First Person Camera - Continued 尚未启用。\n是否立即启用？",
            ["SuccessSub"] = "已成功订阅！建议立即重启游戏",
            ["ErrorSub"] = "尝试自动订阅 First Person Camera - Continued 时发生错误（无网络连接？）",
            ["ManualDownload"] = "你可以手动从 github.com/will258012/FPSCamera-Continued/releases 下载该模组。",
            ["ThenManualDownload"] = "随后手动下载 (github.com/will258012/FPSCamera-Continued/releases) \n并安装 First Person Camera - Continued 模组",
            ["RejectSub"] = "已拒绝自动订阅 First Person Camera - Continued……",
            ["RejectSubSln"] = "请退订以下模组或订阅 First Person Camera - Continued，并重启游戏",
            ["SuccessEnabled"] = "已成功启用！",
            ["ErrorEnable"] = "尝试启用 First Person Camera - Continued 时发生错误",
            ["ManualEnable"] = "可在内容管理器中手动启用它",
            ["RejectEnable"] = "已拒绝启用 First Person Camera - Continued……",
            ["RejectEnableSln"] = "请退订以下模组或在内容管理器中启用 First Person Camera - Continued",
            ["ShowError"] = "以下模组：\n{0}需要前置 First Person Camera - Continued 才能正常工作！\n{1}\n请关闭游戏，{2}",
            ["NotOnSteam"] = "由于你使用的是 Steam 以外的平台，无法自动订阅 First Person Camera - Continued。",
            ["NoWorkShop"] = "由于游戏在 --noWorkshop 模式下，无法自动订阅 First Person Camera - Continued。",
            ["NoWorkShopSln"] = "请重启游戏并不带 --noWorkshop 参数，\n或者手动下载 (github.com/will258012/FPSCamera-Continued/releases) 并安装 First Person Camera - Continued。",
            ["WorkShopNotAvailable"] = "由于创意工坊不可用，无法自动安装 First Person Camera - Continued （无网络连接？）",
            ["AssemblyNotLoad"] = "似乎已经订阅了 First Person Camera - Continued，\n但 Steam 未能下载文件或文件被删除，\n或是模组未在 Skyve 中加载",
            ["AssemblyNotLoadSln"] = "随后检查网络连接\n或者重新订阅 First Person Camera - Continued\n或者检查 Skyve 的状态（如有）",
        };
      
        internal static string Translate(string key)
        {
            if (IsZh && ZhTranslations.TryGetValue(key, out var value))
                return value;
            else if (EnTranslations.TryGetValue(key, out var value2))
                return value2;
            else return key;
        }
    }
}
