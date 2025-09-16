using System;
using System.Collections.Generic;
using XUnity.Common.Logging;

namespace XUnity.Common.Compatibility
{
    /// <summary>
    /// 兼容性工具类，提供Unity版本和IL2CPP环境的统一兼容性处理
    /// </summary>
    public static class CompatibilityHelper
    {
        private static bool _initialized = false;
        private static CompatibilityInfo _compatibilityInfo;

        /// <summary>
        /// 兼容性信息
        /// </summary>
        public static CompatibilityInfo CompatibilityInfo
        {
            get
            {
                if (!_initialized)
                {
                    InitializeCompatibility();
                }
                return _compatibilityInfo;
            }
        }

        /// <summary>
        /// 初始化兼容性检测
        /// </summary>
        private static void InitializeCompatibility()
        {
            try
            {
                _compatibilityInfo = new CompatibilityInfo
                {
                    UnityVersion = UnityVersionDetector.UnityVersion,
                    IsIL2CPP = IL2CPPDetector.IsIL2CPP,
                    IsUnity2022OrHigher = UnityVersionDetector.IsUnity2022OrHigher,
                    IsUnity2022_3 = UnityVersionDetector.IsUnity2022_3,
                    IsUnity2023 = UnityVersionDetector.IsUnity2023,
                    IsUnity2024 = UnityVersionDetector.IsUnity2024,
                    DetectionTime = DateTime.Now
                };

                _initialized = true;

                LogCompatibilityInfo();
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, "初始化兼容性检测时发生错误");
                
                // 使用默认兼容性信息
                _compatibilityInfo = new CompatibilityInfo
                {
                    UnityVersion = new Version(2022, 3, 0),
                    IsIL2CPP = false,
                    IsUnity2022OrHigher = true,
                    IsUnity2022_3 = true,
                    IsUnity2023 = false,
                    IsUnity2024 = false,
                    DetectionTime = DateTime.Now
                };
                
                _initialized = true;
            }
        }

        /// <summary>
        /// 记录兼容性信息
        /// </summary>
        private static void LogCompatibilityInfo()
        {
            var info = _compatibilityInfo;
            XuaLogger.AutoTranslator.Info($"=== 兼容性检测结果 ===");
            XuaLogger.AutoTranslator.Info($"Unity版本: {info.UnityVersion}");
            XuaLogger.AutoTranslator.Info($"运行环境: {(info.IsIL2CPP ? "IL2CPP" : "Mono")}");
            XuaLogger.AutoTranslator.Info($"Unity 2022+: {info.IsUnity2022OrHigher}");
            XuaLogger.AutoTranslator.Info($"Unity 2022.3: {info.IsUnity2022_3}");
            XuaLogger.AutoTranslator.Info($"Unity 2023: {info.IsUnity2023}");
            XuaLogger.AutoTranslator.Info($"Unity 2024: {info.IsUnity2024}");
            XuaLogger.AutoTranslator.Info($"检测时间: {info.DetectionTime}");
            XuaLogger.AutoTranslator.Info($"========================");
        }

        /// <summary>
        /// 检查是否需要特殊兼容性处理
        /// </summary>
        public static bool RequiresSpecialHandling()
        {
            return CompatibilityInfo.IsIL2CPP || CompatibilityInfo.IsUnity2022OrHigher;
        }

        /// <summary>
        /// 检查特定Unity版本的功能支持
        /// </summary>
        public static bool IsFeatureSupported(string featureName)
        {
            var info = CompatibilityInfo;
            
            switch (featureName.ToLower())
            {
                case "assetbundle":
                    return true; // 所有版本都支持AssetBundle
                case "textmeshpro":
                    return info.IsUnity2022OrHigher; // TextMeshPro在Unity 2022+有改进
                case "il2cpp_hooks":
                    return info.IsIL2CPP; // 只有IL2CPP环境需要特殊Hook处理
                case "reflection":
                    return true; // 所有版本都支持反射，但IL2CPP有限制
                case "unsafe_code":
                    return info.IsIL2CPP; // IL2CPP环境支持unsafe代码
                case "dynamic_loading":
                    return !info.IsIL2CPP; // Mono环境支持动态加载
                default:
                    return true;
            }
        }

        /// <summary>
        /// 获取兼容性建议
        /// </summary>
        public static List<string> GetCompatibilityRecommendations()
        {
            var recommendations = new List<string>();
            var info = CompatibilityInfo;

            if (info.IsIL2CPP)
            {
                recommendations.Add("使用IL2CPP兼容的Hook方法");
                recommendations.Add("避免使用动态代码生成");
                recommendations.Add("谨慎使用反射API");
                recommendations.Add("使用预编译的委托和方法");
            }

            if (info.IsUnity2022OrHigher)
            {
                recommendations.Add("使用Unity 2022+的新API");
                recommendations.Add("注意AssetBundle加载方式的变化");
                recommendations.Add("使用新的TextMeshPro功能");
            }

            if (info.IsUnity2023)
            {
                recommendations.Add("利用Unity 2023的性能改进");
                recommendations.Add("使用新的渲染管线功能");
            }

            if (info.IsUnity2024)
            {
                recommendations.Add("使用Unity 2024的最新功能");
                recommendations.Add("注意API的破坏性变更");
            }

            return recommendations;
        }

        /// <summary>
        /// 检查版本兼容性
        /// </summary>
        public static CompatibilityLevel CheckVersionCompatibility(Version targetVersion)
        {
            var currentVersion = CompatibilityInfo.UnityVersion;
            
            if (currentVersion >= targetVersion)
            {
                return CompatibilityLevel.FullyCompatible;
            }
            else if (currentVersion.Major == targetVersion.Major)
            {
                return CompatibilityLevel.PartiallyCompatible;
            }
            else
            {
                return CompatibilityLevel.Incompatible;
            }
        }

        /// <summary>
        /// 获取环境特定的配置建议
        /// </summary>
        public static Dictionary<string, object> GetEnvironmentSpecificConfig()
        {
            var config = new Dictionary<string, object>();
            var info = CompatibilityInfo;

            if (info.IsIL2CPP)
            {
                config["use_il2cpp_hooks"] = true;
                config["enable_reflection_cache"] = true;
                config["use_unsafe_code"] = true;
                config["disable_dynamic_loading"] = true;
            }
            else
            {
                config["use_il2cpp_hooks"] = false;
                config["enable_reflection_cache"] = false;
                config["use_unsafe_code"] = false;
                config["disable_dynamic_loading"] = false;
            }

            if (info.IsUnity2022OrHigher)
            {
                config["use_new_assetbundle_api"] = true;
                config["enable_textmeshpro_v2"] = true;
                config["use_modern_rendering"] = true;
            }
            else
            {
                config["use_new_assetbundle_api"] = false;
                config["enable_textmeshpro_v2"] = false;
                config["use_modern_rendering"] = false;
            }

            return config;
        }

        /// <summary>
        /// 重置兼容性检测（用于测试）
        /// </summary>
        public static void ResetCompatibilityDetection()
        {
            _initialized = false;
            _compatibilityInfo = null;
        }
    }

    /// <summary>
    /// 兼容性信息结构
    /// </summary>
    public class CompatibilityInfo
    {
        public Version UnityVersion { get; set; }
        public bool IsIL2CPP { get; set; }
        public bool IsUnity2022OrHigher { get; set; }
        public bool IsUnity2022_3 { get; set; }
        public bool IsUnity2023 { get; set; }
        public bool IsUnity2024 { get; set; }
        public DateTime DetectionTime { get; set; }
    }

    /// <summary>
    /// 兼容性级别枚举
    /// </summary>
    public enum CompatibilityLevel
    {
        FullyCompatible,
        PartiallyCompatible,
        Incompatible
    }
}
