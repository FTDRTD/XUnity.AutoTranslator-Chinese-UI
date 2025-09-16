using System;
using System.Collections.Generic;
using UnityEngine;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
    /// <summary>
    /// Unity 2022+ UI组件翻译管理器
    /// </summary>
    public static class UIComponentTranslationManager
    {
        private static readonly Dictionary<Component, TranslationInfo> _translationCache = new Dictionary<Component, TranslationInfo>();
        private static readonly Dictionary<string, Component[]> _componentCache = new Dictionary<string, Component[]>();
        private static readonly object _lockObject = new object();
        private static bool _initialized = false;

        /// <summary>
        /// 初始化UI组件翻译管理器
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            lock (_lockObject)
            {
                if (_initialized) return;

                // 初始化UI适配器
                Unity2022UIAdapter.Initialize();

                // 注册Unity事件
                RegisterUnityEvents();

                _initialized = true;
                XuaLogger.AutoTranslator.Info("UI组件翻译管理器初始化完成");
            }
        }

        /// <summary>
        /// 注册Unity事件
        /// </summary>
        private static void RegisterUnityEvents()
        {
            try
            {
                // 监听场景加载事件
#if UNITY_2022_1_OR_NEWER
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
#else
                // 在旧版本Unity中，使用其他方式监听场景加载事件
#endif
                
                // 监听对象销毁事件
#if UNITY_2022_1_OR_NEWER
                Application.quitting += OnApplicationQuitting;
#else
                // 在旧版本Unity中，使用其他方式监听退出事件
#endif
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, "注册Unity事件时发生错误");
            }
        }

        /// <summary>
        /// 场景加载完成事件
        /// </summary>
        private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            try
            {
                // 清理旧场景的缓存
                CleanupSceneCache(scene.name);
                
                // 扫描新场景的UI组件
                ScanSceneComponents(scene);
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"处理场景加载事件时发生错误: {scene.name}");
            }
        }

        /// <summary>
        /// 应用程序退出事件
        /// </summary>
        private static void OnApplicationQuitting()
        {
            try
            {
                lock (_lockObject)
                {
                    _translationCache.Clear();
                    _componentCache.Clear();
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, "处理应用程序退出事件时发生错误");
            }
        }

        /// <summary>
        /// 扫描场景中的UI组件
        /// </summary>
        private static void ScanSceneComponents(UnityEngine.SceneManagement.Scene scene)
        {
            try
            {
                var allComponents = new List<Component>();
                var rootObjects = scene.GetRootGameObjects();

                foreach (var rootObject in rootObjects)
                {
                    var components = rootObject.GetComponentsInChildren<Component>(true);
                    allComponents.AddRange(components);
                }

                lock (_lockObject)
                {
                    _componentCache[scene.name] = allComponents.ToArray();
                }

                XuaLogger.AutoTranslator.Debug($"扫描场景 {scene.name} 完成，发现 {allComponents.Count} 个组件");
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"扫描场景组件时发生错误: {scene.name}");
            }
        }

        /// <summary>
        /// 清理场景缓存
        /// </summary>
        private static void CleanupSceneCache(string sceneName)
        {
            lock (_lockObject)
            {
                if (_componentCache.TryGetValue(sceneName, out var components))
                {
                    foreach (var component in components)
                    {
                        _translationCache.Remove(component);
                    }
                    _componentCache.Remove(sceneName);
                }
            }
        }

        /// <summary>
        /// 翻译UI组件文本
        /// </summary>
        public static bool TranslateComponent(Component component, string translatedText, string originalText = null)
        {
            if (component == null || string.IsNullOrEmpty(translatedText)) return false;

            Initialize();

            try
            {
                // 检查是否为文本组件
                if (!Unity2022UIAdapter.IsTextComponent(component))
                {
                    return false;
                }

                // 获取原始文本
                if (string.IsNullOrEmpty(originalText))
                {
                    originalText = Unity2022UIAdapter.GetText(component);
                }

                // 设置翻译后的文本
                bool success = Unity2022UIAdapter.SetText(component, translatedText);

                if (success)
                {
                    // 缓存翻译信息
                    lock (_lockObject)
                    {
                        _translationCache[component] = new TranslationInfo
                        {
                            Component = component,
                            OriginalText = originalText,
                            TranslatedText = translatedText,
                            TranslationTime = DateTime.Now,
                            IsTranslated = true
                        };
                    }

                    XuaLogger.AutoTranslator.Debug($"翻译UI组件成功: {component.GetType().Name} - {originalText} -> {translatedText}");
                }

                return success;
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"翻译UI组件时发生错误: {component.GetType().Name}");
                return false;
            }
        }

        /// <summary>
        /// 恢复UI组件原始文本
        /// </summary>
        public static bool RestoreComponent(Component component)
        {
            if (component == null) return false;

            Initialize();

            try
            {
                lock (_lockObject)
                {
                    if (_translationCache.TryGetValue(component, out var translationInfo))
                    {
                        bool success = Unity2022UIAdapter.SetText(component, translationInfo.OriginalText);
                        
                        if (success)
                        {
                            translationInfo.IsTranslated = false;
                            XuaLogger.AutoTranslator.Debug($"恢复UI组件原始文本: {component.GetType().Name} - {translationInfo.OriginalText}");
                        }
                        
                        return success;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"恢复UI组件时发生错误: {component.GetType().Name}");
                return false;
            }
        }

        /// <summary>
        /// 获取组件的翻译信息
        /// </summary>
        public static TranslationInfo GetTranslationInfo(Component component)
        {
            if (component == null) return null;

            Initialize();

            lock (_lockObject)
            {
                return _translationCache.TryGetValue(component, out var info) ? info : null;
            }
        }

        /// <summary>
        /// 检查组件是否已翻译
        /// </summary>
        public static bool IsComponentTranslated(Component component)
        {
            var info = GetTranslationInfo(component);
            return info?.IsTranslated ?? false;
        }

        /// <summary>
        /// 获取场景中的所有文本组件
        /// </summary>
        public static Component[] GetTextComponentsInScene(string sceneName)
        {
            Initialize();

            lock (_lockObject)
            {
                if (_componentCache.TryGetValue(sceneName, out var components))
                {
                    var textComponents = new List<Component>();
                    foreach (var component in components)
                    {
                        if (Unity2022UIAdapter.IsTextComponent(component))
                        {
                            textComponents.Add(component);
                        }
                    }
                    return textComponents.ToArray();
                }
            }

            return new Component[0];
        }

        /// <summary>
        /// 批量翻译场景中的文本组件
        /// </summary>
        public static int TranslateSceneComponents(string sceneName, Func<string, string> translator)
        {
            if (translator == null) return 0;

            Initialize();

            var textComponents = GetTextComponentsInScene(sceneName);
            int translatedCount = 0;

            foreach (var component in textComponents)
            {
                try
                {
                    var originalText = Unity2022UIAdapter.GetText(component);
                    if (!string.IsNullOrEmpty(originalText))
                    {
                        var translatedText = translator(originalText);
                        if (!string.IsNullOrEmpty(translatedText) && translatedText != originalText)
                        {
                            if (TranslateComponent(component, translatedText, originalText))
                            {
                                translatedCount++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, $"批量翻译组件时发生错误: {component.GetType().Name}");
                }
            }

            XuaLogger.AutoTranslator.Info($"场景 {sceneName} 批量翻译完成，成功翻译 {translatedCount} 个组件");
            return translatedCount;
        }

        /// <summary>
        /// 获取翻译统计信息
        /// </summary>
        public static UITranslationStatistics GetStatistics()
        {
            Initialize();

            lock (_lockObject)
            {
                var stats = new UITranslationStatistics
                {
                    TotalComponents = _translationCache.Count,
                    TranslatedComponents = 0,
                    TotalScenes = _componentCache.Count
                };

                foreach (var kvp in _translationCache)
                {
                    if (kvp.Value.IsTranslated)
                    {
                        stats.TranslatedComponents++;
                    }
                }

                return stats;
            }
        }

        /// <summary>
        /// 清理翻译缓存
        /// </summary>
        public static void ClearCache()
        {
            lock (_lockObject)
            {
                _translationCache.Clear();
                _componentCache.Clear();
            }
        }

        /// <summary>
        /// 获取兼容性建议
        /// </summary>
        public static string GetCompatibilityAdvice()
        {
            var advice = new List<string>();
            var stats = GetStatistics();

            if (stats.TotalComponents == 0)
            {
                advice.Add("未检测到UI组件");
                advice.Add("建议检查场景中是否有UI组件");
            }

            if (CompatibilityHelper.CompatibilityInfo.IsUnity2022OrHigher)
            {
                advice.Add("Unity 2022+支持新的UI翻译功能");
                advice.Add("建议使用最新的UI组件API");
            }

            return advice.Count > 0 ? string.Join("; ", advice.ToArray()) : "UI组件翻译兼容性良好";
        }
    }

    /// <summary>
    /// 翻译信息
    /// </summary>
    public class TranslationInfo
    {
        public Component Component { get; set; }
        public string OriginalText { get; set; }
        public string TranslatedText { get; set; }
        public DateTime TranslationTime { get; set; }
        public bool IsTranslated { get; set; }
    }

    /// <summary>
    /// UI翻译统计信息
    /// </summary>
    public class UITranslationStatistics
    {
        public int TotalComponents { get; set; }
        public int TranslatedComponents { get; set; }
        public int TotalScenes { get; set; }
    }
}
