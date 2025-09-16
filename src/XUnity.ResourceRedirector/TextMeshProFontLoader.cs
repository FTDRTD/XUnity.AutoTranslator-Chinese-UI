using System;
using System.Collections.Generic;
using UnityEngine;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;

#if IL2CPP
using Il2CppInterop.Runtime;
#endif

namespace XUnity.ResourceRedirector
{
    /// <summary>
    /// TextMeshPro字体加载器，专门处理TextMeshPro字体的加载和兼容性
    /// </summary>
    public static class TextMeshProFontLoader
    {
        private static readonly Dictionary<string, UnityEngine.Object> _tmpFontCache = new Dictionary<string, UnityEngine.Object>();
        private static readonly Dictionary<string, TMPFontLoadContext> _loadingContexts = new Dictionary<string, TMPFontLoadContext>();
        private static readonly object _lockObject = new object();

        /// <summary>
        /// TextMeshPro字体加载上下文
        /// </summary>
        public class TMPFontLoadContext
        {
            public string FontPath { get; set; }
            public UnityEngine.Object FontAsset { get; set; }
            public DateTime LoadTime { get; set; }
            public TMPFontLoadMethod LoadMethod { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
            public Exception LoadError { get; set; }
            public bool IsSuccessful => FontAsset != null && LoadError == null;
        }

        /// <summary>
        /// TextMeshPro字体加载方法枚举
        /// </summary>
        public enum TMPFontLoadMethod
        {
            FromAssetBundle,
            FromResources,
            FromFile,
            FromMemory,
            SystemFont,
            FallbackFont
        }

        /// <summary>
        /// 加载TextMeshPro字体资源
        /// </summary>
        public static UnityEngine.Object LoadTMPFontAsset(string fontPath, TMPFontLoadMethod loadMethod = TMPFontLoadMethod.FromAssetBundle)
        {
            if (string.IsNullOrEmpty(fontPath))
            {
                XuaLogger.ResourceRedirector.Warn("TextMeshPro字体路径为空");
                return null;
            }

            lock (_lockObject)
            {
                // 检查缓存
                if (_tmpFontCache.TryGetValue(fontPath, out var cachedFont))
                {
                    XuaLogger.ResourceRedirector.Debug($"从缓存获取TextMeshPro字体: {fontPath}");
                    return cachedFont;
                }

                try
                {
                    UnityEngine.Object fontAsset = null;
                    var context = new TMPFontLoadContext
                    {
                        FontPath = fontPath,
                        LoadTime = DateTime.Now,
                        LoadMethod = loadMethod,
                        Metadata = new Dictionary<string, object>()
                    };

                    switch (loadMethod)
                    {
                        case TMPFontLoadMethod.FromAssetBundle:
                            fontAsset = LoadTMPFontFromAssetBundle(fontPath);
                            break;
                        case TMPFontLoadMethod.FromResources:
                            fontAsset = LoadTMPFontFromResources(fontPath);
                            break;
                        case TMPFontLoadMethod.FromFile:
                            fontAsset = LoadTMPFontFromFile(fontPath);
                            break;
                        case TMPFontLoadMethod.FromMemory:
                            fontAsset = LoadTMPFontFromMemory(fontPath);
                            break;
                        case TMPFontLoadMethod.SystemFont:
                            fontAsset = LoadTMPSystemFont(fontPath);
                            break;
                        case TMPFontLoadMethod.FallbackFont:
                            fontAsset = LoadTMPFallbackFont(fontPath);
                            break;
                    }

                    context.FontAsset = fontAsset;
                    context.LoadError = fontAsset == null ? new Exception("TextMeshPro字体加载失败") : null;

                    if (fontAsset != null)
                    {
                        _tmpFontCache[fontPath] = fontAsset;
                        _loadingContexts[fontPath] = context;
                        XuaLogger.ResourceRedirector.Info($"成功加载TextMeshPro字体: {fontPath}");
                    }
                    else
                    {
                        XuaLogger.ResourceRedirector.Error($"TextMeshPro字体加载失败: {fontPath}");
                    }

                    return fontAsset;
                }
                catch (Exception ex)
                {
                    XuaLogger.ResourceRedirector.Error(ex, $"加载TextMeshPro字体时发生异常: {fontPath}");
                    return null;
                }
            }
        }

        /// <summary>
        /// 从AssetBundle加载TextMeshPro字体
        /// </summary>
        private static UnityEngine.Object LoadTMPFontFromAssetBundle(string fontPath)
        {
            try
            {
                // 这里需要实际的AssetBundle实例
                // 由于这是静态方法，我们需要通过其他方式获取AssetBundle
                // 或者这个方法需要接收AssetBundle参数
                
                XuaLogger.ResourceRedirector.Debug($"尝试从AssetBundle加载TextMeshPro字体: {fontPath}");
                
                // 临时实现：返回null，需要在实际使用时完善
                return null;
            }
            catch (Exception ex)
            {
                XuaLogger.ResourceRedirector.Error(ex, $"从AssetBundle加载TextMeshPro字体失败: {fontPath}");
                return null;
            }
        }

        /// <summary>
        /// 从Resources加载TextMeshPro字体
        /// </summary>
        private static UnityEngine.Object LoadTMPFontFromResources(string fontPath)
        {
            try
            {
                var fontAsset = Resources.Load(fontPath);
                if (fontAsset != null)
                {
                    XuaLogger.ResourceRedirector.Info($"从Resources加载TextMeshPro字体成功: {fontPath}");
                }
                else
                {
                    XuaLogger.ResourceRedirector.Warn($"从Resources加载TextMeshPro字体失败: {fontPath}");
                }
                return fontAsset;
            }
            catch (Exception ex)
            {
                XuaLogger.ResourceRedirector.Error(ex, $"从Resources加载TextMeshPro字体异常: {fontPath}");
                return null;
            }
        }

        /// <summary>
        /// 从文件加载TextMeshPro字体
        /// </summary>
        private static UnityEngine.Object LoadTMPFontFromFile(string fontPath)
        {
            try
            {
                // TextMeshPro字体通常不能直接从文件加载
                // 需要通过AssetBundle或Resources加载
                XuaLogger.ResourceRedirector.Warn($"TextMeshPro字体不支持直接从文件加载: {fontPath}");
                return null;
            }
            catch (Exception ex)
            {
                XuaLogger.ResourceRedirector.Error(ex, $"从文件加载TextMeshPro字体失败: {fontPath}");
                return null;
            }
        }

        /// <summary>
        /// 从内存加载TextMeshPro字体
        /// </summary>
        private static UnityEngine.Object LoadTMPFontFromMemory(string fontPath)
        {
            try
            {
                // TextMeshPro字体通常不能直接从内存加载
                // 需要通过AssetBundle或Resources加载
                XuaLogger.ResourceRedirector.Warn($"TextMeshPro字体不支持直接从内存加载: {fontPath}");
                return null;
            }
            catch (Exception ex)
            {
                XuaLogger.ResourceRedirector.Error(ex, $"从内存加载TextMeshPro字体失败: {fontPath}");
                return null;
            }
        }

        /// <summary>
        /// 加载TextMeshPro系统字体
        /// </summary>
        private static UnityEngine.Object LoadTMPSystemFont(string fontPath)
        {
            try
            {
                // 尝试加载系统字体作为TextMeshPro字体
                // 这通常需要创建TMP_FontAsset
                
                XuaLogger.ResourceRedirector.Debug($"尝试加载TextMeshPro系统字体: {fontPath}");
                
                // 临时实现：返回null，需要在实际使用时完善
                return null;
            }
            catch (Exception ex)
            {
                XuaLogger.ResourceRedirector.Error(ex, $"加载TextMeshPro系统字体失败: {fontPath}");
                return null;
            }
        }

        /// <summary>
        /// 加载TextMeshPro后备字体
        /// </summary>
        private static UnityEngine.Object LoadTMPFallbackFont(string fontPath)
        {
            try
            {
                // 尝试加载后备字体
                // 这通常用于当主要字体加载失败时的备选方案
                
                XuaLogger.ResourceRedirector.Debug($"尝试加载TextMeshPro后备字体: {fontPath}");
                
                // 临时实现：返回null，需要在实际使用时完善
                return null;
            }
            catch (Exception ex)
            {
                XuaLogger.ResourceRedirector.Error(ex, $"加载TextMeshPro后备字体失败: {fontPath}");
                return null;
            }
        }

        /// <summary>
        /// 从AssetBundle加载TextMeshPro字体（带AssetBundle参数）
        /// </summary>
        public static UnityEngine.Object LoadTMPFontFromAssetBundle(AssetBundle bundle, string fontName)
        {
            if (bundle == null || string.IsNullOrEmpty(fontName))
            {
                XuaLogger.ResourceRedirector.Warn("AssetBundle或TextMeshPro字体名称为空");
                return null;
            }

            try
            {
                var fontAsset = bundle.LoadAsset(fontName);
                if (fontAsset != null)
                {
                    // 缓存字体资源
                    var cacheKey = $"bundle:{bundle.name}:{fontName}";
                    lock (_lockObject)
                    {
                        _tmpFontCache[cacheKey] = fontAsset;
                    }
                    
                    XuaLogger.ResourceRedirector.Info($"从AssetBundle加载TextMeshPro字体成功: {fontName}");
                }
                else
                {
                    XuaLogger.ResourceRedirector.Warn($"从AssetBundle加载TextMeshPro字体失败: {fontName}");
                }
                return fontAsset;
            }
            catch (Exception ex)
            {
                XuaLogger.ResourceRedirector.Error(ex, $"从AssetBundle加载TextMeshPro字体异常: {fontName}");
                return null;
            }
        }

        /// <summary>
        /// 检查TextMeshPro字体是否已缓存
        /// </summary>
        public static bool IsTMPFontCached(string fontPath)
        {
            lock (_lockObject)
            {
                return _tmpFontCache.ContainsKey(fontPath);
            }
        }

        /// <summary>
        /// 获取缓存的TextMeshPro字体
        /// </summary>
        public static UnityEngine.Object GetCachedTMPFont(string fontPath)
        {
            lock (_lockObject)
            {
                return _tmpFontCache.TryGetValue(fontPath, out var font) ? font : null;
            }
        }

        /// <summary>
        /// 移除TextMeshPro字体缓存
        /// </summary>
        public static bool RemoveTMPFontCache(string fontPath)
        {
            lock (_lockObject)
            {
                var removed = _tmpFontCache.Remove(fontPath);
                _loadingContexts.Remove(fontPath);
                
                if (removed)
                {
                    XuaLogger.ResourceRedirector.Debug($"移除TextMeshPro字体缓存: {fontPath}");
                }
                
                return removed;
            }
        }

        /// <summary>
        /// 清除所有TextMeshPro字体缓存
        /// </summary>
        public static void ClearAllTMPFontCache()
        {
            lock (_lockObject)
            {
                var count = _tmpFontCache.Count;
                _tmpFontCache.Clear();
                _loadingContexts.Clear();
                
                XuaLogger.ResourceRedirector.Info($"清除了 {count} 个TextMeshPro字体缓存");
            }
        }

        /// <summary>
        /// 获取TextMeshPro字体加载统计信息
        /// </summary>
        public static TMPFontLoadStatistics GetTMPFontLoadStatistics()
        {
            lock (_lockObject)
            {
                var statistics = new TMPFontLoadStatistics
                {
                    CachedFontCount = _tmpFontCache.Count,
                    LoadingContextCount = _loadingContexts.Count
                };

                foreach (var context in _loadingContexts.Values)
                {
                    if (context.IsSuccessful)
                    {
                        statistics.SuccessfulLoads++;
                    }
                    else
                    {
                        statistics.FailedLoads++;
                    }
                }

                return statistics;
            }
        }

        /// <summary>
        /// 获取TextMeshPro字体兼容性建议
        /// </summary>
        public static string GetTMPFontCompatibilityAdvice()
        {
            var advice = new List<string>();
            var compatibilityInfo = CompatibilityHelper.CompatibilityInfo;

            if (compatibilityInfo.IsIL2CPP)
            {
                advice.Add("IL2CPP环境下TextMeshPro字体加载可能需要特殊处理");
                advice.Add("建议使用预加载的TextMeshPro字体资源");
            }

            if (compatibilityInfo.IsUnity2022OrHigher)
            {
                advice.Add("Unity 2022+支持新的TextMeshPro功能");
                advice.Add("建议使用最新的TextMeshPro API");
            }

            return advice.Count > 0 ? string.Join("; ", advice.ToArray()) : "TextMeshPro字体加载兼容性良好";
        }

        /// <summary>
        /// 验证TextMeshPro字体资源
        /// </summary>
        public static bool ValidateTMPFontAsset(UnityEngine.Object fontAsset)
        {
            if (fontAsset == null)
            {
                return false;
            }

            try
            {
                // 检查是否为TextMeshPro字体资源
                var typeName = fontAsset.GetType().Name;
                return typeName.Contains("TMP_FontAsset") || typeName.Contains("FontAsset");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取TextMeshPro字体资源类型
        /// </summary>
        public static string GetTMPFontAssetType(UnityEngine.Object fontAsset)
        {
            if (fontAsset == null)
            {
                return "Unknown";
            }

            return fontAsset.GetType().Name;
        }
    }

    /// <summary>
    /// TextMeshPro字体加载统计信息
    /// </summary>
    public class TMPFontLoadStatistics
    {
        public int CachedFontCount { get; set; }
        public int LoadingContextCount { get; set; }
        public int SuccessfulLoads { get; set; }
        public int FailedLoads { get; set; }
        
        public int TotalLoads => SuccessfulLoads + FailedLoads;
        public double SuccessRate => TotalLoads > 0 ? (double)SuccessfulLoads / TotalLoads : 0;
    }
}
