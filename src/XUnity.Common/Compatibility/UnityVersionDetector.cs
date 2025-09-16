using System;
using System.Text.RegularExpressions;
using XUnity.Common.Logging;

namespace XUnity.Common.Compatibility
{
    /// <summary>
    /// Unity版本检测器，用于检测当前运行的Unity版本
    /// </summary>
    public static class UnityVersionDetector
    {
        private static Version _unityVersion;
        private static bool _versionDetected = false;

        /// <summary>
        /// 获取当前Unity版本
        /// </summary>
        public static Version UnityVersion
        {
            get
            {
                if (!_versionDetected)
                {
                    DetectUnityVersion();
                }
                return _unityVersion;
            }
        }

        /// <summary>
        /// 检查是否为Unity 2022.3或更高版本
        /// </summary>
        public static bool IsUnity2022OrHigher
        {
            get
            {
                var version = UnityVersion;
                return version != null && version >= new Version(2022, 3);
            }
        }

        /// <summary>
        /// 检查是否为Unity 2022.3.x版本
        /// </summary>
        public static bool IsUnity2022_3
        {
            get
            {
                var version = UnityVersion;
                return version != null && version.Major == 2022 && version.Minor == 3;
            }
        }

        /// <summary>
        /// 检查是否为Unity 2023.x版本
        /// </summary>
        public static bool IsUnity2023
        {
            get
            {
                var version = UnityVersion;
                return version != null && version.Major == 2023;
            }
        }

        /// <summary>
        /// 检查是否为Unity 2024.x版本
        /// </summary>
        public static bool IsUnity2024
        {
            get
            {
                var version = UnityVersion;
                return version != null && version.Major == 2024;
            }
        }

        /// <summary>
        /// 检测Unity版本
        /// </summary>
        private static void DetectUnityVersion()
        {
            try
            {
                // 方法1: 通过Application.unityVersion检测
                var unityVersionString = UnityEngine.Application.unityVersion;
                if (!string.IsNullOrEmpty(unityVersionString))
                {
                    _unityVersion = ParseVersionString(unityVersionString);
                    if (_unityVersion != null)
                    {
                        XuaLogger.AutoTranslator.Info($"检测到Unity版本: {_unityVersion} (通过Application.unityVersion)");
                        _versionDetected = true;
                        return;
                    }
                }

                // 方法2: 通过反射检测UnityEngine.CoreModule版本
                try
                {
                    var coreModule = typeof(UnityEngine.Object).Assembly;
                    var version = coreModule.GetName().Version;
                    if (version != null)
                    {
                        // Unity版本通常与程序集版本有对应关系
                        _unityVersion = EstimateUnityVersionFromAssemblyVersion(version);
                        if (_unityVersion != null)
                        {
                            XuaLogger.AutoTranslator.Info($"检测到Unity版本: {_unityVersion} (通过程序集版本估算)");
                            _versionDetected = true;
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, "通过程序集版本检测Unity版本失败");
                }

                // 方法3: 通过环境变量检测
                var unityVersionEnv = Environment.GetEnvironmentVariable("UNITY_VERSION");
                if (!string.IsNullOrEmpty(unityVersionEnv))
                {
                    _unityVersion = ParseVersionString(unityVersionEnv);
                    if (_unityVersion != null)
                    {
                        XuaLogger.AutoTranslator.Info($"检测到Unity版本: {_unityVersion} (通过环境变量)");
                        _versionDetected = true;
                        return;
                    }
                }

                // 如果所有方法都失败，使用默认版本
                _unityVersion = new Version(2022, 3, 0);
                XuaLogger.AutoTranslator.Warn($"无法检测Unity版本，使用默认版本: {_unityVersion}");
                _versionDetected = true;
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, "检测Unity版本时发生错误");
                _unityVersion = new Version(2022, 3, 0);
                _versionDetected = true;
            }
        }

        /// <summary>
        /// 解析版本字符串
        /// </summary>
        private static Version ParseVersionString(string versionString)
        {
            try
            {
                // 移除可能的额外信息，只保留版本号
                var cleanVersion = Regex.Replace(versionString, @"[^\d\.]", "");
                
                // 确保版本号格式正确
                var parts = cleanVersion.Split('.');
                if (parts.Length >= 2)
                {
                    var major = int.Parse(parts[0]);
                    var minor = int.Parse(parts[1]);
                    var build = parts.Length > 2 ? int.Parse(parts[2]) : 0;
                    var revision = parts.Length > 3 ? int.Parse(parts[3]) : 0;
                    
                    return new Version(major, minor, build, revision);
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"解析版本字符串失败: {versionString}");
            }
            
            return null;
        }

        /// <summary>
        /// 根据程序集版本估算Unity版本
        /// </summary>
        private static Version EstimateUnityVersionFromAssemblyVersion(Version assemblyVersion)
        {
            try
            {
                // Unity版本与程序集版本的对应关系（这是一个估算）
                // 实际对应关系可能因Unity版本而异
                if (assemblyVersion.Major >= 2022)
                {
                    return new Version(2022, 3, 0);
                }
                else if (assemblyVersion.Major >= 2021)
                {
                    return new Version(2021, 3, 0);
                }
                else if (assemblyVersion.Major >= 2020)
                {
                    return new Version(2020, 3, 0);
                }
                else if (assemblyVersion.Major >= 2019)
                {
                    return new Version(2019, 4, 0);
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, "根据程序集版本估算Unity版本失败");
            }
            
            return null;
        }

        /// <summary>
        /// 获取版本兼容性信息
        /// </summary>
        public static string GetCompatibilityInfo()
        {
            var version = UnityVersion;
            var info = $"Unity版本: {version}";
            
            if (IsUnity2022OrHigher)
            {
                info += " (支持IL2CPP兼容性)";
            }
            else
            {
                info += " (可能需要额外兼容性处理)";
            }
            
            return info;
        }
    }
}
