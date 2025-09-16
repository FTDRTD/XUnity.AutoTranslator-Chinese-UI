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
    /// AssetBundle缓存管理器，提供Unity 2022+兼容的缓存机制
    /// </summary>
    public static class AssetBundleCache
    {
        private static readonly Dictionary<string, CachedAssetBundle> _bundleCache = new Dictionary<string, CachedAssetBundle>();
        private static readonly Dictionary<string, CachedAsset> _assetCache = new Dictionary<string, CachedAsset>();
        private static readonly object _lockObject = new object();
        
        private static bool _initialized = false;
        private static DateTime _lastCleanupTime = DateTime.Now;
        private static readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

        /// <summary>
        /// 初始化缓存系统
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            lock (_lockObject)
            {
                if (_initialized) return;

                try
                {
                    // 根据Unity版本和IL2CPP环境配置缓存策略
                    ConfigureCacheStrategy();
                    
                    _initialized = true;
                    XuaLogger.ResourceRedirector.Info("AssetBundle缓存系统初始化完成");
                }
                catch (Exception ex)
                {
                    XuaLogger.ResourceRedirector.Error(ex, "初始化AssetBundle缓存系统时发生错误");
                    _initialized = true; // 标记为已初始化，避免重复尝试
                }
            }
        }

        /// <summary>
        /// 配置缓存策略
        /// </summary>
        private static void ConfigureCacheStrategy()
        {
            var compatibilityInfo = CompatibilityHelper.CompatibilityInfo;
            
            if (compatibilityInfo.IsIL2CPP)
            {
                // IL2CPP环境下的缓存策略
                XuaLogger.ResourceRedirector.Info("配置IL2CPP环境缓存策略");
            }
            
            if (compatibilityInfo.IsUnity2022OrHigher)
            {
                // Unity 2022+的缓存策略
                XuaLogger.ResourceRedirector.Info("配置Unity 2022+缓存策略");
            }
        }

        /// <summary>
        /// 缓存AssetBundle
        /// </summary>
        public static void CacheAssetBundle(string path, AssetBundle bundle, Dictionary<string, object> metadata = null)
        {
            if (string.IsNullOrEmpty(path) || bundle == null)
            {
                return;
            }

            Initialize();

            var cachedBundle = new CachedAssetBundle
            {
                Path = path,
                Bundle = bundle,
                CacheTime = DateTime.Now,
                LastAccessTime = DateTime.Now,
                AccessCount = 0,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            lock (_lockObject)
            {
                if (_bundleCache.ContainsKey(path))
                {
                    var existing = _bundleCache[path];
                    existing.LastAccessTime = DateTime.Now;
                    existing.AccessCount++;
                }
                else
                {
                    _bundleCache[path] = cachedBundle;
                }
            }

            XuaLogger.ResourceRedirector.Debug($"缓存AssetBundle: {path}");
        }

        /// <summary>
        /// 获取缓存的AssetBundle
        /// </summary>
        public static AssetBundle GetCachedAssetBundle(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            Initialize();

            if (_bundleCache.TryGetValue(path, out var cachedBundle))
            {
                cachedBundle.LastAccessTime = DateTime.Now;
                cachedBundle.AccessCount++;
                
                XuaLogger.ResourceRedirector.Debug($"从缓存获取AssetBundle: {path}");
                return cachedBundle.Bundle;
            }

            return null;
        }

        /// <summary>
        /// 缓存资源
        /// </summary>
        public static void CacheAsset(string assetPath, UnityEngine.Object asset, Dictionary<string, object> metadata = null)
        {
            if (string.IsNullOrEmpty(assetPath) || asset == null)
            {
                return;
            }

            Initialize();

            var cachedAsset = new CachedAsset
            {
                AssetPath = assetPath,
                Asset = asset,
                CacheTime = DateTime.Now,
                LastAccessTime = DateTime.Now,
                AccessCount = 0,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            lock (_lockObject)
            {
                if (_assetCache.ContainsKey(assetPath))
                {
                    var existing = _assetCache[assetPath];
                    existing.LastAccessTime = DateTime.Now;
                    existing.AccessCount++;
                }
                else
                {
                    _assetCache.Add(assetPath, cachedAsset);
                }
            }

            XuaLogger.ResourceRedirector.Debug($"缓存资源: {assetPath}");
        }

        /// <summary>
        /// 获取缓存的资源
        /// </summary>
        public static UnityEngine.Object GetCachedAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            Initialize();

            if (_assetCache.TryGetValue(assetPath, out var cachedAsset))
            {
                cachedAsset.LastAccessTime = DateTime.Now;
                cachedAsset.AccessCount++;
                
                XuaLogger.ResourceRedirector.Debug($"从缓存获取资源: {assetPath}");
                return cachedAsset.Asset;
            }

            return null;
        }

        /// <summary>
        /// 检查AssetBundle是否已缓存
        /// </summary>
        public static bool IsAssetBundleCached(string path)
        {
            return !string.IsNullOrEmpty(path) && _bundleCache.ContainsKey(path);
        }

        /// <summary>
        /// 检查资源是否已缓存
        /// </summary>
        public static bool IsAssetCached(string assetPath)
        {
            return !string.IsNullOrEmpty(assetPath) && _assetCache.ContainsKey(assetPath);
        }

        /// <summary>
        /// 移除AssetBundle缓存
        /// </summary>
        public static bool RemoveAssetBundleCache(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            lock (_lockObject)
            {
                if (_bundleCache.ContainsKey(path))
                {
                    var cachedBundle = _bundleCache[path];
                    _bundleCache.Remove(path);
                    
                    // 卸载AssetBundle
                    if (cachedBundle.Bundle != null)
                    {
                        cachedBundle.Bundle.Unload(false);
                    }
                    
                    XuaLogger.ResourceRedirector.Debug($"移除AssetBundle缓存: {path}");
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// 移除资源缓存
        /// </summary>
        public static bool RemoveAssetCache(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            lock (_lockObject)
            {
                if (_assetCache.ContainsKey(assetPath))
                {
                    _assetCache.Remove(assetPath);
                    XuaLogger.ResourceRedirector.Debug($"移除资源缓存: {assetPath}");
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
            var maxCacheAge = maxAge ?? TimeSpan.FromMinutes(30);
            
            // 清理过期的AssetBundle缓存
            var expiredBundles = new List<string>();
            foreach (var kvp in _bundleCache)
            {
                if (now - kvp.Value.LastAccessTime > maxCacheAge)
                {
                    expiredBundles.Add(kvp.Key);
                }
            }
            
            foreach (var path in expiredBundles)
            {
                RemoveAssetBundleCache(path);
            }

            // 清理过期的资源缓存
            var expiredAssets = new List<string>();
            foreach (var kvp in _assetCache)
            {
                if (now - kvp.Value.LastAccessTime > maxCacheAge)
                {
                    expiredAssets.Add(kvp.Key);
                }
            }
            
            foreach (var assetPath in expiredAssets)
            {
                RemoveAssetCache(assetPath);
            }

            if (expiredBundles.Count > 0 || expiredAssets.Count > 0)
            {
                XuaLogger.ResourceRedirector.Info($"清理了 {expiredBundles.Count} 个AssetBundle缓存和 {expiredAssets.Count} 个资源缓存");
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
                var bundleCount = _bundleCache.Count;
                var assetCount = _assetCache.Count;

                // 卸载所有AssetBundle
                foreach (var kvp in _bundleCache)
                {
                    if (kvp.Value.Bundle != null)
                    {
                        kvp.Value.Bundle.Unload(false);
                    }
                }

                _bundleCache.Clear();
                _assetCache.Clear();

                XuaLogger.ResourceRedirector.Info($"清除了所有缓存: {bundleCount} 个AssetBundle, {assetCount} 个资源");
            }
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        public static CacheStatistics GetCacheStatistics()
        {
            var statistics = new CacheStatistics
            {
                AssetBundleCount = _bundleCache.Count,
                AssetCount = _assetCache.Count,
                TotalMemoryUsage = CalculateMemoryUsage()
            };

            // 计算访问统计
            foreach (var kvp in _bundleCache)
            {
                statistics.TotalAssetBundleAccesses += kvp.Value.AccessCount;
            }

            foreach (var kvp in _assetCache)
            {
                statistics.TotalAssetAccesses += kvp.Value.AccessCount;
            }

            return statistics;
        }

        /// <summary>
        /// 计算内存使用量（估算）
        /// </summary>
        private static long CalculateMemoryUsage()
        {
            // 这是一个简化的内存使用量计算
            // 实际实现可能需要更复杂的计算方式
            return (_bundleCache.Count * 1024) + (_assetCache.Count * 512);
        }

        /// <summary>
        /// 获取缓存建议
        /// </summary>
        public static string GetCacheAdvice()
        {
            var statistics = GetCacheStatistics();
            var advice = new List<string>();

            if (statistics.AssetBundleCount > 100)
            {
                advice.Add("AssetBundle缓存数量较多，考虑清理过期缓存");
            }

            if (statistics.AssetCount > 1000)
            {
                advice.Add("资源缓存数量较多，考虑清理过期缓存");
            }

            if (statistics.TotalMemoryUsage > 100 * 1024 * 1024) // 100MB
            {
                advice.Add("缓存内存使用量较高，考虑清理缓存");
            }

            return advice.Count > 0 ? string.Join("; ", advice.ToArray()) : "缓存状态良好";
        }
    }

    /// <summary>
    /// 缓存的AssetBundle信息
    /// </summary>
    public class CachedAssetBundle
    {
        public string Path { get; set; }
        public AssetBundle Bundle { get; set; }
        public DateTime CacheTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public int AccessCount { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    /// <summary>
    /// 缓存的资源信息
    /// </summary>
    public class CachedAsset
    {
        public string AssetPath { get; set; }
        public UnityEngine.Object Asset { get; set; }
        public DateTime CacheTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public int AccessCount { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    /// <summary>
    /// 缓存统计信息
    /// </summary>
    public class CacheStatistics
    {
        public int AssetBundleCount { get; set; }
        public int AssetCount { get; set; }
        public int TotalAssetBundleAccesses { get; set; }
        public int TotalAssetAccesses { get; set; }
        public long TotalMemoryUsage { get; set; }
    }
}
