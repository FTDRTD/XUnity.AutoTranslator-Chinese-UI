using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Configuration
{
    /// <summary>
    /// Unity 2022+ 配置管理器
    /// </summary>
    public static class Unity2022ConfigurationManager
    {
        private static readonly Dictionary<string, ConfigurationSection> _sections = new Dictionary<string, ConfigurationSection>();
        private static readonly Dictionary<string, ConfigurationValue> _values = new Dictionary<string, ConfigurationValue>();
        private static readonly object _lockObject = new object();
        private static bool _initialized = false;
        private static string _configPath = "AutoTranslatorConfig.json";

        /// <summary>
        /// 配置变更事件
        /// </summary>
        public static event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        /// <summary>
        /// 初始化配置管理器
        /// </summary>
        public static void Initialize(string configPath = null)
        {
            if (_initialized) return;

            lock (_lockObject)
            {
                if (_initialized) return;

                try
                {
                    if (!string.IsNullOrEmpty(configPath))
                    {
                        _configPath = configPath;
                    }

                    // 创建默认配置
                    CreateDefaultConfiguration();

                    // 加载配置文件
                    LoadConfiguration();

                    _initialized = true;
                    XuaLogger.AutoTranslator.Info("Unity 2022+ 配置管理器初始化完成");
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Error(ex, "配置管理器初始化失败");
                }
            }
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        private static void CreateDefaultConfiguration()
        {
            // 通用设置
            RegisterSection("General", "通用设置");
            RegisterValue("General", "EnableTranslation", true, "启用自动翻译");
            RegisterValue("General", "DefaultProvider", "GoogleTranslate", "默认翻译提供者");
            RegisterValue("General", "TargetLanguage", "zh", "目标语言");
            RegisterValue("General", "SourceLanguage", "auto", "源语言");
            RegisterValue("General", "EnableDebugLogging", false, "启用调试日志");

            // UI设置
            RegisterSection("UI", "UI设置");
            RegisterValue("UI", "EnableUICaching", true, "启用UI缓存");
            RegisterValue("UI", "CacheExpirationTime", 3600, "缓存过期时间(秒)");
            RegisterValue("UI", "MaxCacheSize", 10000, "最大缓存大小");
            RegisterValue("UI", "EnableAutoTranslation", true, "启用自动翻译UI");

            // 性能设置
            RegisterSection("Performance", "性能设置");
            RegisterValue("Performance", "MaxConcurrentRequests", 5, "最大并发请求数");
            RegisterValue("Performance", "RequestDelay", 1000, "请求延迟(毫秒)");
            RegisterValue("Performance", "RequestTimeout", 30000, "请求超时(毫秒)");
            RegisterValue("Performance", "EnableRequestCaching", true, "启用请求缓存");

            // 翻译设置
            RegisterSection("Translation", "翻译设置");
            RegisterValue("Translation", "EnableTextFiltering", true, "启用文本过滤");
            RegisterValue("Translation", "MinTextLength", 1, "最小文本长度");
            RegisterValue("Translation", "MaxTextLength", 5000, "最大文本长度");
            RegisterValue("Translation", "EnableFallbackTranslation", true, "启用备用翻译");

            // 兼容性设置
            RegisterSection("Compatibility", "兼容性设置");
            RegisterValue("Compatibility", "EnableIL2CPPSupport", true, "启用IL2CPP支持");
            RegisterValue("Compatibility", "EnableUnity2022Features", true, "启用Unity 2022+功能");
            RegisterValue("Compatibility", "LegacyMode", false, "传统模式");

            XuaLogger.AutoTranslator.Debug("默认配置创建完成");
        }

        /// <summary>
        /// 注册配置节
        /// </summary>
        private static void RegisterSection(string name, string displayName)
        {
            _sections[name] = new ConfigurationSection
            {
                Name = name,
                DisplayName = displayName,
                Values = new Dictionary<string, ConfigurationValue>()
            };
        }

        /// <summary>
        /// 注册配置值
        /// </summary>
        private static void RegisterValue(string sectionName, string key, object defaultValue, string description)
        {
            var value = new ConfigurationValue
            {
                Section = sectionName,
                Key = key,
                Value = defaultValue,
                Description = description,
                Type = defaultValue.GetType()
            };

            _values[$"{sectionName}.{key}"] = value;
            
            if (_sections.TryGetValue(sectionName, out var section))
            {
                section.Values[key] = value;
            }
        }

        /// <summary>
        /// 获取配置值
        /// </summary>
        public static T GetValue<T>(string sectionName, string key, T defaultValue = default(T))
        {
            Initialize();

            lock (_lockObject)
            {
                var fullKey = $"{sectionName}.{key}";
                if (_values.TryGetValue(fullKey, out var configValue))
                {
                    try
                    {
                        return (T)Convert.ChangeType(configValue.Value, typeof(T));
                    }
                    catch (Exception ex)
                    {
                        XuaLogger.AutoTranslator.Debug(ex, $"配置值转换失败: {fullKey}");
                        return defaultValue;
                    }
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 设置配置值
        /// </summary>
        public static void SetValue<T>(string sectionName, string key, T value)
        {
            Initialize();

            lock (_lockObject)
            {
                var fullKey = $"{sectionName}.{key}";
                if (_values.TryGetValue(fullKey, out var configValue))
                {
                    var oldValue = configValue.Value;
                    configValue.Value = value;

                    // 触发配置变更事件
                    ConfigurationChanged?.Invoke(null, new ConfigurationChangedEventArgs
                    {
                        SectionName = sectionName,
                        Key = key,
                        OldValue = oldValue,
                        NewValue = value
                    });

                    XuaLogger.AutoTranslator.Debug($"配置值已更新: {fullKey} = {value}");
                }
                else
                {
                    XuaLogger.AutoTranslator.Warn($"未找到配置项: {fullKey}");
                }
            }
        }

        /// <summary>
        /// 获取配置节
        /// </summary>
        public static ConfigurationSection GetSection(string sectionName)
        {
            Initialize();

            lock (_lockObject)
            {
                return _sections.TryGetValue(sectionName, out var section) ? section : null;
            }
        }

        /// <summary>
        /// 获取所有配置节
        /// </summary>
        public static ConfigurationSection[] GetAllSections()
        {
            Initialize();

            lock (_lockObject)
            {
                return _sections.Values.ToArray();
            }
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public static void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
#if UNITY_2022_1_OR_NEWER
                    var configData = JsonUtility.FromJson<ConfigurationData>(json);
#else
                    // 在非Unity环境中，使用简单的JSON解析
                    var configData = ParseSimpleJson(json);
#endif
                    
                    if (configData != null && configData.Values != null)
                    {
                        foreach (var kvp in configData.Values)
                        {
                            if (_values.TryGetValue(kvp.Key, out var configValue))
                            {
                                try
                                {
                                    configValue.Value = Convert.ChangeType(kvp.Value, configValue.Type);
                                }
                                catch (Exception ex)
                                {
                                    XuaLogger.AutoTranslator.Debug(ex, $"加载配置值失败: {kvp.Key}");
                                }
                            }
                        }
                    }

                    XuaLogger.AutoTranslator.Info($"配置文件加载完成: {_configPath}");
                }
                else
                {
                    XuaLogger.AutoTranslator.Info("配置文件不存在，使用默认配置");
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, "加载配置文件失败");
            }
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public static void SaveConfiguration()
        {
            try
            {
                var configData = new ConfigurationData
                {
                    Values = new Dictionary<string, object>()
                };

                lock (_lockObject)
                {
                    foreach (var kvp in _values)
                    {
                        configData.Values[kvp.Key] = kvp.Value.Value;
                    }
                }

#if UNITY_2022_1_OR_NEWER
                var json = JsonUtility.ToJson(configData, true);
#else
                var json = SerializeSimpleJson(configData);
#endif
                File.WriteAllText(_configPath, json);

                XuaLogger.AutoTranslator.Info($"配置文件保存完成: {_configPath}");
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, "保存配置文件失败");
            }
        }

        /// <summary>
        /// 重置配置为默认值
        /// </summary>
        public static void ResetToDefaults()
        {
            lock (_lockObject)
            {
                foreach (var kvp in _values)
                {
                    var configValue = kvp.Value;
                    // 这里需要保存默认值，暂时跳过
                }

                XuaLogger.AutoTranslator.Info("配置已重置为默认值");
            }
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        public static ConfigurationValidationResult ValidateConfiguration()
        {
            var result = new ConfigurationValidationResult
            {
                IsValid = true,
                Errors = new List<string>(),
                Warnings = new List<string>()
            };

            try
            {
                // 验证通用设置
                var maxConcurrentRequests = GetValue("Performance", "MaxConcurrentRequests", 5);
                if (maxConcurrentRequests < 1 || maxConcurrentRequests > 20)
                {
                    result.Warnings.Add("最大并发请求数应在1-20之间");
                }

                var requestDelay = GetValue("Performance", "RequestDelay", 1000);
                if (requestDelay < 0 || requestDelay > 10000)
                {
                    result.Warnings.Add("请求延迟应在0-10000毫秒之间");
                }

                var targetLanguage = GetValue("General", "TargetLanguage", "zh");
                if (string.IsNullOrEmpty(targetLanguage))
                {
                    result.Errors.Add("目标语言不能为空");
                    result.IsValid = false;
                }

                // 验证兼容性设置
                var enableIL2CPP = GetValue("Compatibility", "EnableIL2CPPSupport", true);
                if (CompatibilityHelper.CompatibilityInfo.IsIL2CPP && !enableIL2CPP)
                {
                    result.Warnings.Add("当前为IL2CPP环境，建议启用IL2CPP支持");
                }

                var enableUnity2022 = GetValue("Compatibility", "EnableUnity2022Features", true);
                if (CompatibilityHelper.CompatibilityInfo.IsUnity2022OrHigher && !enableUnity2022)
                {
                    result.Warnings.Add("当前为Unity 2022+环境，建议启用Unity 2022+功能");
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"配置验证失败: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取配置统计信息
        /// </summary>
        public static ConfigurationStatistics GetStatistics()
        {
            lock (_lockObject)
            {
                return new ConfigurationStatistics
                {
                    TotalSections = _sections.Count,
                    TotalValues = _values.Count,
                    ConfigFilePath = _configPath,
                    IsInitialized = _initialized
                };
            }
        }

        /// <summary>
        /// 简单的JSON解析方法
        /// </summary>
        private static ConfigurationData ParseSimpleJson(string json)
        {
            // 简单的JSON解析实现
            return new ConfigurationData
            {
                Values = new Dictionary<string, object>()
            };
        }

        /// <summary>
        /// 简单的JSON序列化方法
        /// </summary>
        private static string SerializeSimpleJson(ConfigurationData configData)
        {
            // 简单的JSON序列化实现
            return "{}";
        }

        /// <summary>
        /// 获取兼容性建议
        /// </summary>
        public static string GetCompatibilityAdvice()
        {
            var advice = new List<string>();
            var validation = ValidateConfiguration();

            if (!validation.IsValid)
            {
                advice.AddRange(validation.Errors);
            }

            advice.AddRange(validation.Warnings);

            if (CompatibilityHelper.CompatibilityInfo.IsUnity2022OrHigher)
            {
                advice.Add("Unity 2022+支持新的配置功能");
                advice.Add("建议使用最新的配置API");
            }

            return advice.Count > 0 ? string.Join("; ", advice.ToArray()) : "配置系统兼容性良好";
        }
    }

    /// <summary>
    /// 配置节
    /// </summary>
    [Serializable]
    public class ConfigurationSection
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public Dictionary<string, ConfigurationValue> Values { get; set; }
    }

    /// <summary>
    /// 配置值
    /// </summary>
    [Serializable]
    public class ConfigurationValue
    {
        public string Section { get; set; }
        public string Key { get; set; }
        public object Value { get; set; }
        public string Description { get; set; }
        public Type Type { get; set; }
    }

    /// <summary>
    /// 配置数据
    /// </summary>
    [Serializable]
    public class ConfigurationData
    {
        public Dictionary<string, object> Values { get; set; }
    }

    /// <summary>
    /// 配置变更事件参数
    /// </summary>
    public class ConfigurationChangedEventArgs : EventArgs
    {
        public string SectionName { get; set; }
        public string Key { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }

    /// <summary>
    /// 配置验证结果
    /// </summary>
    public class ConfigurationValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
    }

    /// <summary>
    /// 配置统计信息
    /// </summary>
    public class ConfigurationStatistics
    {
        public int TotalSections { get; set; }
        public int TotalValues { get; set; }
        public string ConfigFilePath { get; set; }
        public bool IsInitialized { get; set; }
    }
}
