using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;
using System.Threading;
using XUnity.AutoTranslator.Plugin.Core.UI;
using XUnity.AutoTranslator.Plugin.Core.Translation;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.BepInEx
{
    /// <summary>
    /// Unity 2022+ BepInEx集成插件
    /// </summary>
    [BepInPlugin("XUnity.AutoTranslator", "XUnity Auto Translator", "1.0.0")]
    [BepInDependency("XUnity.ResourceRedirector", BepInDependency.DependencyFlags.HardDependency)]
    public class Unity2022AutoTranslatorPlugin : BaseUnityPlugin
    {
        private static Unity2022AutoTranslatorPlugin _instance;
        private bool _initialized = false;

        // 配置项
        private ConfigEntry<bool> _enableTranslation;
        private ConfigEntry<string> _defaultProvider;
        private ConfigEntry<string> _targetLanguage;
        private ConfigEntry<bool> _enableUICaching;
        private ConfigEntry<int> _maxConcurrentRequests;
        private ConfigEntry<int> _requestDelay;
        private ConfigEntry<bool> _enableDebugLogging;

        /// <summary>
        /// 插件实例
        /// </summary>
        public static Unity2022AutoTranslatorPlugin Instance => _instance;

        /// <summary>
        /// 插件初始化
        /// </summary>
        private void Awake()
        {
            _instance = this;
            
            try
            {
                InitializeConfiguration();
                InitializePlugin();
                
                Logger.LogInfo("Unity 2022+ Auto Translator 插件初始化完成");
            }
            catch (Exception ex)
            {
                Logger.LogError($"插件初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化配置
        /// </summary>
        private void InitializeConfiguration()
        {
            // 基础设置
            _enableTranslation = Config.Bind("General", "EnableTranslation", true, "启用自动翻译功能");
            _defaultProvider = Config.Bind("General", "DefaultProvider", "GoogleTranslate", "默认翻译提供者");
            _targetLanguage = Config.Bind("General", "TargetLanguage", "zh", "目标语言");
            _enableDebugLogging = Config.Bind("General", "EnableDebugLogging", false, "启用调试日志");

            // 性能设置
            _enableUICaching = Config.Bind("Performance", "EnableUICaching", true, "启用UI组件缓存");
            _maxConcurrentRequests = Config.Bind("Performance", "MaxConcurrentRequests", 5, "最大并发请求数");
            _requestDelay = Config.Bind("Performance", "RequestDelay", 1000, "请求延迟(毫秒)");

            // 配置变更事件
            _enableTranslation.SettingChanged += OnEnableTranslationChanged;
            _defaultProvider.SettingChanged += OnDefaultProviderChanged;
            _targetLanguage.SettingChanged += OnTargetLanguageChanged;
            _maxConcurrentRequests.SettingChanged += OnMaxConcurrentRequestsChanged;
            _requestDelay.SettingChanged += OnRequestDelayChanged;
        }

        /// <summary>
        /// 初始化插件
        /// </summary>
        private void InitializePlugin()
        {
            if (_initialized) return;

            try
            {
                // 初始化兼容性检测
                var compatibilityInfo = CompatibilityHelper.CompatibilityInfo; // This will initialize if not already initialized

                // 初始化UI适配器
                Unity2022UIAdapter.Initialize();

                // 初始化UI组件翻译管理器
                UIComponentTranslationManager.Initialize();

                // 初始化翻译引擎
                var translationEngine = Unity2022TranslationEngine.Instance;
                translationEngine.Initialize();

                // 应用配置
                ApplyConfiguration();

                // 注册Unity事件
                RegisterUnityEvents();

                _initialized = true;
                Logger.LogInfo("插件核心组件初始化完成");
            }
            catch (Exception ex)
            {
                Logger.LogError($"插件核心组件初始化失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 应用配置
        /// </summary>
        private void ApplyConfiguration()
        {
            try
            {
                var translationEngine = Unity2022TranslationEngine.Instance;

                // 设置翻译引擎配置
                translationEngine.SetDefaultProvider(_defaultProvider.Value);
                translationEngine.SetMaxConcurrentRequests(_maxConcurrentRequests.Value);
                translationEngine.SetRequestDelay(_requestDelay.Value);

                Logger.LogInfo($"应用配置完成 - 提供者: {_defaultProvider.Value}, 目标语言: {_targetLanguage.Value}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"应用配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 注册Unity事件
        /// </summary>
        private void RegisterUnityEvents()
        {
            try
            {
                // 监听场景加载事件
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

                // 监听应用程序事件
#if UNITY_2018_1_OR_NEWER
                UnityEngine.Application.quitting += OnApplicationQuitting;
#else
                // For older Unity versions, we'll use the application's update loop to check for quit
                var go = new GameObject("QuitMonitor");
                UnityEngine.Object.DontDestroyOnLoad(go);
                var monitor = go.AddComponent<QuitMonitor>();
                monitor.OnQuit += OnApplicationQuitting;
#endif

                Logger.LogInfo("Unity事件注册完成");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Unity事件注册失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 场景加载完成事件
        /// </summary>
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (!_enableTranslation.Value) return;

            try
            {
                Logger.LogInfo($"场景加载完成: {scene.name}");

                // 异步翻译场景
                TranslateSceneAsync(scene);
            }
            catch (Exception ex)
            {
                Logger.LogError($"处理场景加载事件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 异步翻译场景
        /// </summary>
        private void TranslateSceneAsync(UnityEngine.SceneManagement.Scene scene)
        {
            try
            {
                var textComponents = UIComponentTranslationManager.GetTextComponentsInScene(scene.name);
                Logger.LogInfo($"发现 {textComponents.Length} 个文本组件");

                int totalComponents = textComponents.Length;
                int translatedCount = 0;
                var translationEngine = Unity2022TranslationEngine.Instance;

                foreach (var component in textComponents)
                {
                    try
                    {
                        var originalText = Unity2022UIAdapter.GetText(component);
                        if (!string.IsNullOrEmpty(originalText))
                        {
                            translationEngine.TranslateAsync(
                                originalText, 
                                "auto", 
                                _targetLanguage.Value, 
                                null,
                                result => 
                                {
                                    try
                                    {
                                        if (result.Success && !string.IsNullOrEmpty(result.TranslatedText))
                                        {
                                            UIComponentTranslationManager.TranslateComponent(component, result.TranslatedText, originalText);
                                            Interlocked.Increment(ref translatedCount);
                                        }

                                        // 检查是否所有组件都已处理
                                        if (Interlocked.CompareExchange(ref translatedCount, 0, 0) + Interlocked.CompareExchange(ref totalComponents, 0, 0) == textComponents.Length)
                                        {
                                            Logger.LogInfo($"场景 {scene.name} 翻译完成，成功翻译 {translatedCount} 个组件");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.LogWarning($"处理翻译结果时出错: {ex.Message}");
                                    }
                                });
                        }
                        else
                        {
                            Interlocked.Increment(ref totalComponents);
                        }
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref totalComponents);
                        Logger.LogWarning($"翻译组件失败: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"异步翻译场景失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 应用程序退出事件
        /// </summary>
        private void OnApplicationQuitting()
        {
            // This method is called when the application is quitting
            // Cleanup code here will run in both editor and player
            try
            {
                Logger.LogInfo("应用程序退出，清理资源");
                
                // 清理缓存
                UIComponentTranslationManager.ClearCache();
                Unity2022TranslationEngine.Instance.ClearCache();
            }
            catch (Exception ex)
            {
                Logger.LogError($"清理资源失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 启用翻译配置变更事件
        /// </summary>
        private void OnEnableTranslationChanged(object sender, EventArgs e)
        {
            Logger.LogInfo($"翻译功能 {((sender as ConfigEntry<bool>)?.Value == true ? "启用" : "禁用")}");
        }

        /// <summary>
        /// 默认提供者配置变更事件
        /// </summary>
        private void OnDefaultProviderChanged(object sender, EventArgs e)
        {
            var provider = (sender as ConfigEntry<string>)?.Value;
            Unity2022TranslationEngine.Instance.SetDefaultProvider(provider);
            Logger.LogInfo($"默认翻译提供者变更为: {provider}");
        }

        /// <summary>
        /// 目标语言配置变更事件
        /// </summary>
        private void OnTargetLanguageChanged(object sender, EventArgs e)
        {
            var language = (sender as ConfigEntry<string>)?.Value;
            Logger.LogInfo($"目标语言变更为: {language}");
        }

        /// <summary>
        /// 最大并发请求数配置变更事件
        /// </summary>
        private void OnMaxConcurrentRequestsChanged(object sender, EventArgs e)
        {
            var maxRequests = (sender as ConfigEntry<int>)?.Value ?? 5;
            Unity2022TranslationEngine.Instance.SetMaxConcurrentRequests(maxRequests);
            Logger.LogInfo($"最大并发请求数变更为: {maxRequests}");
        }

        /// <summary>
        /// 请求延迟配置变更事件
        /// </summary>
        private void OnRequestDelayChanged(object sender, EventArgs e)
        {
            var delay = (sender as ConfigEntry<int>)?.Value ?? 1000;
            Unity2022TranslationEngine.Instance.SetRequestDelay(delay);
            Logger.LogInfo($"请求延迟变更为: {delay}ms");
        }

        /// <summary>
        /// 获取插件统计信息
        /// </summary>
        public PluginStatistics GetStatistics()
        {
            try
            {
                var uiStats = UIComponentTranslationManager.GetStatistics();
                var translationStats = Unity2022TranslationEngine.Instance.GetStatistics();
                var compatibilityInfo = CompatibilityHelper.CompatibilityInfo;

                return new PluginStatistics
                {
                    PluginVersion = "1.0.0",
                    IsInitialized = _initialized,
                    TranslationEnabled = _enableTranslation.Value,
                    DefaultProvider = _defaultProvider.Value,
                    TargetLanguage = _targetLanguage.Value,
                    UICacheEnabled = _enableUICaching.Value,
                    UITotalComponents = uiStats.TotalComponents,
                    UITranslatedComponents = uiStats.TranslatedComponents,
                    UITotalScenes = uiStats.TotalScenes,
                    TranslationProviders = translationStats.TotalProviders,
                    TranslationCacheSize = translationStats.CacheSize,
                    UnityVersion = compatibilityInfo.UnityVersion.ToString(),
                    IsIL2CPP = compatibilityInfo.IsIL2CPP,
                    IsUnity2022OrHigher = compatibilityInfo.IsUnity2022OrHigher
                };
            }
            catch (Exception ex)
            {
                Logger.LogError($"获取统计信息失败: {ex.Message}");
                return new PluginStatistics();
            }
        }

        /// <summary>
        /// 获取兼容性建议
        /// </summary>
        public string GetCompatibilityAdvice()
        {
            var advice = new List<string>();

            try
            {
                // 收集各模块的兼容性建议
                var recommendations = CompatibilityHelper.GetCompatibilityRecommendations();
                advice.Add(string.Join("; ", recommendations));
                advice.Add(Unity2022UIAdapter.GetCompatibilityAdvice());
                advice.Add(UIComponentTranslationManager.GetCompatibilityAdvice());
                advice.Add(Unity2022TranslationEngine.Instance.GetCompatibilityAdvice());
            }
            catch (Exception ex)
            {
                Logger.LogError($"获取兼容性建议失败: {ex.Message}");
                advice.Add($"获取兼容性建议时发生错误: {ex.Message}");
            }

            return advice.Count > 0 ? string.Join("; ", advice.ToArray()) : "插件兼容性良好";
        }

        /// <summary>
        /// 手动翻译文本
        /// </summary>
        public void TranslateTextAsync(string text, string fromLanguage, string toLanguage, Action<string> callback)
        {
            if (!_enableTranslation.Value)
            {
                callback?.Invoke(text);
                return;
            }

            try
            {
                var targetLang = toLanguage ?? _targetLanguage.Value;
                Unity2022TranslationEngine.Instance.TranslateAsync(
                    text, 
                    fromLanguage, 
                    targetLang,
                    null,
                    result => 
                    {
                        try
                        {
                            callback?.Invoke(result.Success ? result.TranslatedText : text);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"处理翻译结果时出错: {ex.Message}");
                            callback?.Invoke(text);
                        }
                    });
            }
            catch (Exception ex)
            {
                Logger.LogError($"手动翻译失败: {ex.Message}");
                callback?.Invoke(text);
            }
        }

        /// <summary>
        /// 手动翻译文本（同步版本）
        /// </summary>
        public string TranslateText(string text, string fromLanguage = "auto", string toLanguage = null)
        {
            if (!_enableTranslation.Value)
            {
                return text;
            }

            try
            {
                var targetLang = toLanguage ?? _targetLanguage.Value;
                var result = new TranslationResult();
                var resetEvent = new System.Threading.ManualResetEvent(false);

                Unity2022TranslationEngine.Instance.TranslateAsync(
                    text,
                    fromLanguage,
                    targetLang,
                    null,
                    r =>
                    {
                        result = r;
                        resetEvent.Set();
                    });

                // 等待翻译完成（最多5秒）
                resetEvent.WaitOne(5000);

                return result.Success && !string.IsNullOrEmpty(result.TranslatedText) 
                    ? result.TranslatedText 
                    : text;
            }
            catch (Exception ex)
            {
                Logger.LogError($"同步翻译失败: {ex.Message}");
                return text;
            }
        }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        public void ReloadConfiguration()
        {
            try
            {
                Config.Reload();
                ApplyConfiguration();
                Logger.LogInfo("配置重新加载完成");
            }
            catch (Exception ex)
            {
                Logger.LogError($"重新加载配置失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 用于在旧版Unity中检测应用退出
    /// </summary>
    internal class QuitMonitor : MonoBehaviour
    {
        public event Action OnQuit;
        private bool _isQuitting = false;

        private void Update()
        {
            if (!_isQuitting && (UnityEngine.Application.isEditor || UnityEngine.Application.isPlaying))
            {
                if (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.F4) || Input.GetKey(KeyCode.Q))
                {
                    _isQuitting = true;
                    StartCoroutine(DelayedQuit());
                }
            }
        }

        private System.Collections.IEnumerator DelayedQuit()
        {
            yield return new WaitForSeconds(0.1f);
            if (OnQuit != null)
            {
                OnQuit();
                OnQuit = null;
            }
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 插件统计信息
    /// </summary>
    public class PluginStatistics
    {
        public string PluginVersion { get; set; }
        public bool IsInitialized { get; set; }
        public bool TranslationEnabled { get; set; }
        public string DefaultProvider { get; set; }
        public string TargetLanguage { get; set; }
        public bool UICacheEnabled { get; set; }
        public int UITotalComponents { get; set; }
        public int UITranslatedComponents { get; set; }
        public int UITotalScenes { get; set; }
        public int TranslationProviders { get; set; }
        public int TranslationCacheSize { get; set; }
        public string UnityVersion { get; set; }
        public bool IsIL2CPP { get; set; }
        public bool IsUnity2022OrHigher { get; set; }
    }
}
