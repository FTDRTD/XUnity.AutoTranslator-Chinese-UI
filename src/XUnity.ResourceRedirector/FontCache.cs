using System;
using System.Collections.Generic;
using UnityEngine;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;

#if NET35
// .NET 3.5 doesn't have ConcurrentDictionary, use Dictionary with lock
#else
using System.Collections.Concurrent;
#endif

namespace XUnity.ResourceRedirector
{
    /// <summary>
    /// 字体缓存管理器，提供高效的字体缓存和内存管理
    /// </summary>
    public static class FontCache
    {
        private static readonly Dictionary<string, CachedFontInfo> _fontCache = new Dictionary<string, CachedFontInfo>();
        private static readonly Dictionary<string, CachedFontAsset> _fontAssetCache = new Dictionary<string, CachedFontAsset>();
        private static readonly object _lockObject = new object();
        
        private static bool _initialized = false;
        private static DateTime _lastCleanupTime = DateTime.Now;
        private static readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan _maxCacheAge = TimeSpan.FromHours(1);

        /// <summary>
        /// 缓存的字体信息
        /// </summary>
        public class CachedFontInfo
        {
            public string FontPath { get; set; }
            public UnityEngine.Font Font { get; set; }
            public DateTime CacheTime { get; set; }
            public DateTime LastAccessTime { get; set; }
            public int AccessCount { get; set; }
            public long MemorySize { get; set; }
            public FontLoadMethod LoadMethod { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
            public bool IsValid => Font != null;
        }

        /// <summary>
        /// 缓存的字体资源信息
        /// </summary>
        public class CachedFontAsset
        {
            public string AssetPath { get; set; }
            public UnityEngine.Object FontAsset { get; set; }
            public DateTime CacheTime { get; set; }
            public DateTime LastAccessTime { get; set; }
            public int AccessCount { get; set; }
            public long MemorySize { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
            public bool IsValid => FontAsset != null;
        }

        /// <summary>
        /// 字体加载方法枚举
        /// </summary>
        public enum FontLoadMethod
        {
            FromFile,
            FromMemory,
            FromAssetBundle,
            FromResources,
            SystemFont,
            TextMeshPro
        }

        /// <summary>
        /// 初始化字体缓存系统
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            lock (_lockObject)
            {
                if (_initialized) return;

                try
                {
                    ConfigureCacheSettings();
                    _initialized = true;
                    XuaLogger.ResourceRedirector.Info("字体缓存系统初始化完成");
                }
                catch (Exception ex)
                {
                    XuaLogger.ResourceRedirector.Error(ex, "初始化字体缓存系统时发生错误");
                    _initialized = true;
                }
            }
        }

        /// <summary>
        /// 配置缓存设置
        /// </summary>
        private static void ConfigureCacheSettings()
        {
            var compatibilityInfo = CompatibilityHelper.CompatibilityInfo;
            
            if (compatibilityInfo.IsIL2CPP)
            {
                // IL2CPP环境下的缓存设置
                XuaLogger.ResourceRedirector.Info("配置IL2CPP环境字体缓存设置");
            }
            
            if (compatibilityInfo.IsUnity2022OrHigher)
            {
                // Unity 2022+的缓存设置
                XuaLogger.ResourceRedirector.Info("配置Unity 2022+字体缓存设置");
            }
        }

        /// <summary>
        /// 缓存字体
        /// </summary>
        public static void CacheFont(string fontPath, UnityEngine.Font font, FontLoadMethod loadMethod = FontLoadMethod.FromFile, Dictionary<string, object> metadata = null)
        {
            if (string.IsNullOrEmpty(fontPath) || font == null)
            {
                return;
            }

            Initialize();

            var cachedFont = new CachedFontInfo
            {
                FontPath = fontPath,
                Font = font,
                CacheTime = DateTime.Now,
                LastAccessTime = DateTime.Now,
                AccessCount = 0,
                MemorySize = EstimateFontMemorySize(font),
                LoadMethod = loadMethod,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            lock (_lockObject)
            {
                if (_fontCache.ContainsKey(fontPath))
                {
                    var existing = _fontCache[fontPath];
                    existing.LastAccessTime = DateTime.Now;
                    existing.AccessCount++;
                }
                else
                {
                    _fontCache.Add(fontPath, cachedFont);
                }
            }

            XuaLogger.ResourceRedirector.Debug($"缓存字体: {fontPath}, 内存大小: {cachedFont.MemorySize} bytes");
        }

        /// <summary>
        /// 获取缓存的字体
        /// </summary>
        public static UnityEngine.Font GetCachedFont(string fontPath)
        {
            if (string.IsNullOrEmpty(fontPath))
            {
                return null;
            }

            Initialize();

            if (_fontCache.TryGetValue(fontPath, out var cachedFont))
            {
                if (cachedFont.IsValid)
                {
                    cachedFont.LastAccessTime = DateTime.Now;
                    cachedFont.AccessCount++;
                    
                    XuaLogger.ResourceRedirector.Debug($"从缓存获取字体: {fontPath}");
                    return cachedFont.Font;
                }
                else
                {
                    // 字体已失效，移除缓存
                    lock (_lockObject)
                    {
                        if (_fontCache.ContainsKey(fontPath))
                        {
                            _fontCache.Remove(fontPath);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 缓存字体资源
        /// </summary>
        public static void CacheFontAsset(string assetPath, UnityEngine.Object fontAsset, Dictionary<string, object> metadata = null)
        {
            if (string.IsNullOrEmpty(assetPath) || fontAsset == null)
            {
                return;
            }

            Initialize();

            var cachedAsset = new CachedFontAsset
            {
                AssetPath = assetPath,
                FontAsset = fontAsset,
                CacheTime = DateTime.Now,
                LastAccessTime = DateTime.Now,
                AccessCount = 0,
                MemorySize = EstimateFontAssetMemorySize(fontAsset),
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            lock (_lockObject)
            {
                if (_fontAssetCache.ContainsKey(assetPath))
                {
                    var existing = _fontAssetCache[assetPath];
                    existing.LastAccessTime = DateTime.Now;
                    existing.AccessCount++;
                }
                else
                {
                    _fontAssetCache.Add(assetPath, cachedAsset);
                }
            }

            XuaLogger.ResourceRedirector.Debug($"缓存字体资源: {assetPath}, 内存大小: {cachedAsset.MemorySize} bytes");
        }

        /// <summary>
        /// 获取缓存的字体资源
        /// </summary>
        public static UnityEngine.Object GetCachedFontAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            Initialize();

            if (_fontAssetCache.TryGetValue(assetPath, out var cachedAsset))
            {
                if (cachedAsset.IsValid)
                {
                    cachedAsset.LastAccessTime = DateTime.Now;
                    cachedAsset.AccessCount++;
                    
                    XuaLogger.ResourceRedirector.Debug($"从缓存获取字体资源: {assetPath}");
                    return cachedAsset.FontAsset;
                }
                else
                {
                    // 字体资源已失效，移除缓存
                    lock (_lockObject)
                    {
                        if (_fontAssetCache.ContainsKey(assetPath))
                        {
                            _fontAssetCache.Remove(assetPath);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 检查字体是否已缓存
        /// </summary>
        public static bool IsFontCached(string fontPath)
        {
            return !string.IsNullOrEmpty(fontPath) && _fontCache.ContainsKey(fontPath);
        }

        /// <summary>
        /// 检查字体资源是否已缓存
        /// </summary>
        public static bool IsFontAssetCached(string assetPath)
        {
            return !string.IsNullOrEmpty(assetPath) && _fontAssetCache.ContainsKey(assetPath);
        }

        /// <summary>
        /// 移除字体缓存
        /// </summary>
        public static bool RemoveFontCache(string fontPath)
        {
            if (string.IsNullOrEmpty(fontPath))
            {
                return false;
            }

            lock (_lockObject)
            {
                if (_fontCache.ContainsKey(fontPath))
                {
                    _fontCache.Remove(fontPath);
                    XuaLogger.ResourceRedirector.Debug($"移除字体缓存: {fontPath}");
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// 移除字体资源缓存
        /// </summary>
        public static bool RemoveFontAssetCache(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            lock (_lockObject)
            {
                if (_fontAssetCache.ContainsKey(assetPath))
                {
                    _fontAssetCache.Remove(assetPath);
                    XuaLogger.ResourceRedirector.Debug($"移除字体资源缓存: {assetPath}");
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// 清理过期缓存
        /// </summary>
        public static void CleanupExpiredCache(TimeSpan? maxAge = null)
        {
            var now = DateTime.Now;
            var maxCacheAge = maxAge ?? _maxCacheAge;
            
            // 清理过期的字体缓存
            var expiredFonts = new List<string>();
            foreach (var kvp in _fontCache)
            {
                if (now - kvp.Value.LastAccessTime > maxCacheAge)
                {
                    expiredFonts.Add(kvp.Key);
                }
            }
            
            foreach (var fontPath in expiredFonts)
            {
                RemoveFontCache(fontPath);
            }

            // 清理过期的字体资源缓存
            var expiredAssets = new List<string>();
            foreach (var kvp in _fontAssetCache)
            {
                if (now - kvp.Value.LastAccessTime > maxCacheAge)
                {
                    expiredAssets.Add(kvp.Key);
                }
            }
            
            foreach (var assetPath in expiredAssets)
            {
                RemoveFontAssetCache(assetPath);
            }

            if (expiredFonts.Count > 0 || expiredAssets.Count > 0)
            {
                XuaLogger.ResourceRedirector.Info($"清理了 {expiredFonts.Count} 个字体缓存和 {expiredAssets.Count} 个字体资源缓存");
            }
        }

        /// <summary>
        /// 定期清理缓存
        /// </summary>
        public static void PerformPeriodicCleanup()
        {
            var now = DateTime.Now;
            if (now - _lastCleanupTime > _cleanupInterval)
            {
                CleanupExpiredCache();
                _lastCleanupTime = now;
            }
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public static void ClearAllCache()
        {
            lock (_lockObject)
            {
                var fontCount = _fontCache.Count;
                var assetCount = _fontAssetCache.Count;

                _fontCache.Clear();
                _fontAssetCache.Clear();

                XuaLogger.ResourceRedirector.Info($"清除了所有字体缓存: {fontCount} 个字体, {assetCount} 个字体资源");
            }
        }

        /// <summary>
        /// 估算字体内存大小
        /// </summary>
        private static long EstimateFontMemorySize(Font font)
        {
            if (font == null) return 0;
            
            // 这是一个简化的内存大小估算
            // 实际大小可能因字体复杂度和字符集而异
            return 1024 * 10; // 假设每个字体约10KB
        }

        /// <summary>
        /// 估算字体资源内存大小
        /// </summary>
        private static long EstimateFontAssetMemorySize(UnityEngine.Object fontAsset)
        {
            if (fontAsset == null) return 0;
            
            // 这是一个简化的内存大小估算
            return 1024 * 5; // 假设每个字体资源约5KB
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        public static FontCacheStatistics GetCacheStatistics()
        {
            var statistics = new FontCacheStatistics
            {
                FontCount = _fontCache.Count,
                FontAssetCount = _fontAssetCache.Count,
                TotalMemoryUsage = CalculateTotalMemoryUsage()
            };

            // 计算访问统计
            foreach (var kvp in _fontCache)
            {
                statistics.TotalFontAccesses += kvp.Value.AccessCount;
            }

            foreach (var kvp in _fontAssetCache)
            {
                statistics.TotalFontAssetAccesses += kvp.Value.AccessCount;
            }

            return statistics;
        }

        /// <summary>
        /// 计算总内存使用量
        /// </summary>
        private static long CalculateTotalMemoryUsage()
        {
            long totalMemory = 0;
            
            foreach (var kvp in _fontCache)
            {
                totalMemory += kvp.Value.MemorySize;
            }
            
            foreach (var kvp in _fontAssetCache)
            {
                totalMemory += kvp.Value.MemorySize;
            }
            
            return totalMemory;
        }

        /// <summary>
        /// 获取缓存建议
        /// </summary>
        public static string GetCacheAdvice()
        {
            var statistics = GetCacheStatistics();
            var advice = new List<string>();

            if (statistics.FontCount > 50)
            {
                advice.Add("字体缓存数量较多，考虑清理过期缓存");
            }

            if (statistics.FontAssetCount > 100)
            {
                advice.Add("字体资源缓存数量较多，考虑清理过期缓存");
            }

            if (statistics.TotalMemoryUsage > 50 * 1024 * 1024) // 50MB
            {
                advice.Add("字体缓存内存使用量较高，考虑清理缓存");
            }

            return advice.Count > 0 ? string.Join("; ", advice.ToArray()) : "字体缓存状态良好";
        }
    }

    /// <summary>
    /// 字体缓存统计信息
    /// </summary>
    public class FontCacheStatistics
    {
        public int FontCount { get; set; }
        public int FontAssetCount { get; set; }
        public int TotalFontAccesses { get; set; }
        public int TotalFontAssetAccesses { get; set; }
        public long TotalMemoryUsage { get; set; }
    }
}
