using System;
using System.Collections.Generic;
using System.Threading;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Translation
{
    /// <summary>
    /// Unity 2022+ 翻译核心引擎
    /// </summary>
    public class Unity2022TranslationEngine
    {
        private static Unity2022TranslationEngine _instance;
        private static readonly object _lockObject = new object();

        private readonly Dictionary<string, TranslationCache> _translationCache = new Dictionary<string, TranslationCache>();
        private readonly Dictionary<string, ITranslationProvider> _providers = new Dictionary<string, ITranslationProvider>();
        private readonly List<ITranslationFilter> _filters = new List<ITranslationFilter>();
        
        private bool _initialized = false;
        private string _defaultProvider = "GoogleTranslate";
        private int _maxConcurrentRequests = 5;
        private int _requestDelay = 1000; // 1秒

        /// <summary>
        /// 单例实例
        /// </summary>
        public static Unity2022TranslationEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new Unity2022TranslationEngine();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 初始化翻译引擎
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;

            lock (_lockObject)
            {
                if (_initialized) return;

                try
                {
                    // 注册默认翻译提供者
                    RegisterDefaultProviders();

                    // 注册默认过滤器
                    RegisterDefaultFilters();

                    // 加载配置
                    LoadConfiguration();

                    _initialized = true;
                    XuaLogger.AutoTranslator.Info("Unity 2022+ 翻译引擎初始化完成");
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Error(ex, "翻译引擎初始化失败");
                }
            }
        }

        /// <summary>
        /// 注册默认翻译提供者
        /// </summary>
        private void RegisterDefaultProviders()
        {
            // 这里可以注册各种翻译服务提供者
            // 例如：Google Translate, Baidu Translate, DeepL等
            XuaLogger.AutoTranslator.Debug("注册默认翻译提供者");
        }

        /// <summary>
        /// 注册默认过滤器
        /// </summary>
        private void RegisterDefaultFilters()
        {
            // 注册文本过滤器
            _filters.Add(new EmptyTextFilter());
            _filters.Add(new NumberOnlyFilter());
            _filters.Add(new SpecialCharacterFilter());
            
            XuaLogger.AutoTranslator.Debug("注册默认过滤器");
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private void LoadConfiguration()
        {
            // 从配置文件加载设置
            // 这里可以集成现有的配置系统
            XuaLogger.AutoTranslator.Debug("加载翻译引擎配置");
        }

        /// <summary>
        /// 翻译文本
        /// </summary>
        public TranslationResult Translate(string text, string fromLanguage = "auto", string toLanguage = "zh", string provider = null)
        {
            if (!_initialized)
            {
                Initialize();
            }

            if (string.IsNullOrEmpty(text))
            {
                return new TranslationResult
                {
                    Success = false,
                    ErrorMessage = "文本为空"
                };
            }

            try
            {
                // 应用过滤器
                if (!ShouldTranslate(text))
                {
                    return new TranslationResult
                    {
                        Success = true,
                        OriginalText = text,
                        TranslatedText = text,
                        FromCache = false,
                        Filtered = true
                    };
                }

                // 检查缓存
                var cacheKey = GenerateCacheKey(text, fromLanguage, toLanguage, provider ?? _defaultProvider);
                if (_translationCache.TryGetValue(cacheKey, out var cachedResult))
                {
                    if (cachedResult.IsValid)
                    {
                        return new TranslationResult
                        {
                            Success = true,
                            OriginalText = text,
                            TranslatedText = cachedResult.TranslatedText,
                            FromCache = true,
                            TranslationTime = cachedResult.TranslationTime
                        };
                    }
                    else
                    {
                        _translationCache.Remove(cacheKey);
                    }
                }

                // 执行翻译
                var result = PerformTranslation(text, fromLanguage, toLanguage, provider);

                // 缓存结果
                if (result.Success)
                {
                    _translationCache[cacheKey] = new TranslationCache
                    {
                        OriginalText = text,
                        TranslatedText = result.TranslatedText,
                        TranslationTime = DateTime.Now,
                        IsValid = true
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, $"翻译文本时发生错误: {text}");
                return new TranslationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 异步翻译文本
        /// </summary>
        public void TranslateAsync(string text, string fromLanguage, string toLanguage, string provider, Action<TranslationResult> callback)
        {
            if (!_initialized)
            {
                Initialize();
            }

            if (string.IsNullOrEmpty(text))
            {
                callback?.Invoke(new TranslationResult
                {
                    Success = false,
                    ErrorMessage = "文本为空"
                });
                return;
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    // 应用过滤器
                    if (!ShouldTranslate(text))
                    {
                        callback?.Invoke(new TranslationResult
                        {
                            Success = true,
                            OriginalText = text,
                            TranslatedText = text,
                            FromCache = false,
                            Filtered = true
                        });
                        return;
                    }

                    // 检查缓存
                    var cacheKey = GenerateCacheKey(text, fromLanguage, toLanguage, provider ?? _defaultProvider);
                    if (_translationCache.TryGetValue(cacheKey, out var cachedResult))
                    {
                        if (cachedResult.IsValid)
                        {
                            callback?.Invoke(new TranslationResult
                            {
                                Success = true,
                                OriginalText = text,
                                TranslatedText = cachedResult.TranslatedText,
                                FromCache = true,
                                TranslationTime = cachedResult.TranslationTime
                            });
                            return;
                        }
                        else
                        {
                            _translationCache.Remove(cacheKey);
                        }
                    }

                    // 执行翻译
                    var result = PerformTranslation(text, fromLanguage, toLanguage, provider);

                    // 缓存结果
                    if (result.Success)
                    {
                        _translationCache[cacheKey] = new TranslationCache
                        {
                            OriginalText = text,
                            TranslatedText = result.TranslatedText,
                            TranslationTime = DateTime.Now,
                            IsValid = true
                        };
                    }

                    callback?.Invoke(result);
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Error(ex, $"异步翻译文本时发生错误: {text}");
                    callback?.Invoke(new TranslationResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            });
        }

        /// <summary>
        /// 执行实际翻译
        /// </summary>
        private TranslationResult PerformTranslation(string text, string fromLanguage, string toLanguage, string provider)
        {
            var selectedProvider = provider ?? _defaultProvider;
            
            if (!_providers.TryGetValue(selectedProvider, out var translationProvider))
            {
                return new TranslationResult
                {
                    Success = false,
                    ErrorMessage = $"未找到翻译提供者: {selectedProvider}"
                };
            }

            try
            {
                // 添加请求延迟
                System.Threading.Thread.Sleep(_requestDelay);

                var result = translationProvider.Translate(text, fromLanguage, toLanguage);
                
                return new TranslationResult
                {
                    Success = result.Success,
                    OriginalText = text,
                    TranslatedText = result.TranslatedText,
                    FromCache = false,
                    TranslationTime = DateTime.Now,
                    ErrorMessage = result.ErrorMessage
                };
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, $"翻译提供者 {selectedProvider} 执行失败");
                return new TranslationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 检查是否应该翻译文本
        /// </summary>
        private bool ShouldTranslate(string text)
        {
            foreach (var filter in _filters)
            {
                if (!filter.ShouldTranslate(text))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 生成缓存键
        /// </summary>
        private string GenerateCacheKey(string text, string fromLanguage, string toLanguage, string provider)
        {
            return $"{provider}:{fromLanguage}:{toLanguage}:{text.GetHashCode()}";
        }

        /// <summary>
        /// 注册翻译提供者
        /// </summary>
        public void RegisterProvider(string name, ITranslationProvider provider)
        {
            lock (_lockObject)
            {
                _providers[name] = provider;
                XuaLogger.AutoTranslator.Info($"注册翻译提供者: {name}");
            }
        }

        /// <summary>
        /// 注册过滤器
        /// </summary>
        public void RegisterFilter(ITranslationFilter filter)
        {
            lock (_lockObject)
            {
                _filters.Add(filter);
                XuaLogger.AutoTranslator.Info($"注册翻译过滤器: {filter.GetType().Name}");
            }
        }

        /// <summary>
        /// 设置默认提供者
        /// </summary>
        public void SetDefaultProvider(string providerName)
        {
            if (_providers.ContainsKey(providerName))
            {
                _defaultProvider = providerName;
                XuaLogger.AutoTranslator.Info($"设置默认翻译提供者: {providerName}");
            }
        }

        /// <summary>
        /// 设置最大并发请求数
        /// </summary>
        public void SetMaxConcurrentRequests(int maxRequests)
        {
            _maxConcurrentRequests = Math.Max(1, maxRequests);
            XuaLogger.AutoTranslator.Info($"设置最大并发请求数: {_maxConcurrentRequests}");
        }

        /// <summary>
        /// 设置请求延迟
        /// </summary>
        public void SetRequestDelay(int delayMs)
        {
            _requestDelay = Math.Max(0, delayMs);
            XuaLogger.AutoTranslator.Info($"设置请求延迟: {_requestDelay}ms");
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        public void ClearCache()
        {
            lock (_lockObject)
            {
                _translationCache.Clear();
                XuaLogger.AutoTranslator.Info("清理翻译缓存");
            }
        }

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public TranslationEngineStatistics GetStatistics()
        {
            lock (_lockObject)
            {
                var stats = new TranslationEngineStatistics
                {
                    TotalProviders = _providers.Count,
                    TotalFilters = _filters.Count,
                    CacheSize = _translationCache.Count,
                    DefaultProvider = _defaultProvider,
                    MaxConcurrentRequests = _maxConcurrentRequests,
                    RequestDelay = _requestDelay
                };

                return stats;
            }
        }

        /// <summary>
        /// 获取兼容性建议
        /// </summary>
        public string GetCompatibilityAdvice()
        {
            var advice = new List<string>();
            var stats = GetStatistics();

            if (stats.TotalProviders == 0)
            {
                advice.Add("未注册任何翻译提供者");
                advice.Add("建议注册至少一个翻译服务");
            }

            if (stats.CacheSize > 10000)
            {
                advice.Add("翻译缓存过大");
                advice.Add("建议清理缓存以释放内存");
            }

            if (CompatibilityHelper.CompatibilityInfo.IsUnity2022OrHigher)
            {
                advice.Add("Unity 2022+支持新的翻译功能");
                advice.Add("建议使用最新的翻译API");
            }

            return advice.Count > 0 ? string.Join("; ", advice.ToArray()) : "翻译引擎兼容性良好";
        }
    }

    /// <summary>
    /// 翻译结果
    /// </summary>
    public class TranslationResult
    {
        public bool Success { get; set; }
        public string OriginalText { get; set; }
        public string TranslatedText { get; set; }
        public bool FromCache { get; set; }
        public bool Filtered { get; set; }
        public DateTime TranslationTime { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 翻译缓存
    /// </summary>
    public class TranslationCache
    {
        public string OriginalText { get; set; }
        public string TranslatedText { get; set; }
        public DateTime TranslationTime { get; set; }
        public bool IsValid { get; set; }
    }

    /// <summary>
    /// 翻译引擎统计信息
    /// </summary>
    public class TranslationEngineStatistics
    {
        public int TotalProviders { get; set; }
        public int TotalFilters { get; set; }
        public int CacheSize { get; set; }
        public string DefaultProvider { get; set; }
        public int MaxConcurrentRequests { get; set; }
        public int RequestDelay { get; set; }
    }

    /// <summary>
    /// 翻译提供者接口
    /// </summary>
    public interface ITranslationProvider
    {
        TranslationResult Translate(string text, string fromLanguage, string toLanguage);
        string Name { get; }
        bool IsAvailable { get; }
    }

    /// <summary>
    /// 翻译过滤器接口
    /// </summary>
    public interface ITranslationFilter
    {
        bool ShouldTranslate(string text);
        string Name { get; }
    }

    /// <summary>
    /// 空文本过滤器
    /// </summary>
    public class EmptyTextFilter : ITranslationFilter
    {
        public string Name => "EmptyTextFilter";

        public bool ShouldTranslate(string text)
        {
            return !string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text.Trim());
        }
    }

    /// <summary>
    /// 纯数字过滤器
    /// </summary>
    public class NumberOnlyFilter : ITranslationFilter
    {
        public string Name => "NumberOnlyFilter";

        public bool ShouldTranslate(string text)
        {
            return !System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+$");
        }
    }

    /// <summary>
    /// 特殊字符过滤器
    /// </summary>
    public class SpecialCharacterFilter : ITranslationFilter
    {
        public string Name => "SpecialCharacterFilter";

        public bool ShouldTranslate(string text)
        {
            return !System.Text.RegularExpressions.Regex.IsMatch(text, @"^[^\w\s]+$");
        }
    }
}
