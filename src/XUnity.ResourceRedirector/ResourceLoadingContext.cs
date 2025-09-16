using System;
using System.Collections.Generic;
using UnityEngine;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;

namespace XUnity.ResourceRedirector
{
    /// <summary>
    /// 资源加载上下文，管理Unity 2022+兼容的资源加载状态
    /// </summary>
    public class ResourceLoadingContext
    {
        private static readonly Dictionary<string, ResourceLoadingContext> _activeContexts = new Dictionary<string, ResourceLoadingContext>();
        private static readonly object _lockObject = new object();

        /// <summary>
        /// 上下文ID
        /// </summary>
        public string ContextId { get; private set; }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string ResourcePath { get; private set; }

        /// <summary>
        /// 加载类型
        /// </summary>
        public ResourceLoadingContextType LoadType { get; private set; }

        /// <summary>
        /// 加载开始时间
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// 加载结束时间
        /// </summary>
        public DateTime? EndTime { get; private set; }

        /// <summary>
        /// 是否已完成
        /// </summary>
        public bool IsCompleted => EndTime.HasValue;

        /// <summary>
        /// 加载持续时间
        /// </summary>
        public TimeSpan Duration => (EndTime ?? DateTime.Now) - StartTime;

        /// <summary>
        /// 加载的资源
        /// </summary>
        public UnityEngine.Object LoadedResource { get; private set; }

        /// <summary>
        /// 加载错误
        /// </summary>
        public Exception LoadingError { get; private set; }

        /// <summary>
        /// 是否成功加载
        /// </summary>
        public bool IsSuccessful => IsCompleted && LoadingError == null && LoadedResource != null;

        /// <summary>
        /// 加载参数
        /// </summary>
        public Dictionary<string, object> Parameters { get; private set; }

        /// <summary>
        /// Unity版本信息
        /// </summary>
        public Version UnityVersion { get; private set; }

        /// <summary>
        /// 是否为IL2CPP环境
        /// </summary>
        public bool IsIL2CPP { get; private set; }

        /// <summary>
        /// 创建新的资源加载上下文
        /// </summary>
        public static ResourceLoadingContext Create(string resourcePath, ResourceLoadingContextType loadType, Dictionary<string, object> parameters = null)
        {
            var contextId = Guid.NewGuid().ToString();
            var context = new ResourceLoadingContext
            {
                ContextId = contextId,
                ResourcePath = resourcePath,
                LoadType = loadType,
                StartTime = DateTime.Now,
                Parameters = parameters ?? new Dictionary<string, object>(),
                UnityVersion = CompatibilityHelper.CompatibilityInfo.UnityVersion,
                IsIL2CPP = CompatibilityHelper.CompatibilityInfo.IsIL2CPP
            };

            lock (_lockObject)
            {
                _activeContexts[contextId] = context;
            }

            XuaLogger.ResourceRedirector.Debug($"创建资源加载上下文: {contextId}, 路径: {resourcePath}, 类型: {loadType}");
            return context;
        }

        /// <summary>
        /// 完成加载
        /// </summary>
        public void CompleteLoading(UnityEngine.Object resource)
        {
            EndTime = DateTime.Now;
            LoadedResource = resource;
            LoadingError = null;

            XuaLogger.ResourceRedirector.Debug($"资源加载完成: {ContextId}, 耗时: {Duration.TotalMilliseconds}ms");
        }

        /// <summary>
        /// 完成加载（带错误）
        /// </summary>
        public void CompleteLoading(Exception error)
        {
            EndTime = DateTime.Now;
            LoadedResource = null;
            LoadingError = error;

            XuaLogger.ResourceRedirector.Error(error, $"资源加载失败: {ContextId}, 耗时: {Duration.TotalMilliseconds}ms");
        }

        /// <summary>
        /// 获取上下文
        /// </summary>
        public static ResourceLoadingContext GetContext(string contextId)
        {
            lock (_lockObject)
            {
                return _activeContexts.TryGetValue(contextId, out var context) ? context : null;
            }
        }

        /// <summary>
        /// 移除上下文
        /// </summary>
        public static void RemoveContext(string contextId)
        {
            lock (_lockObject)
            {
                if (_activeContexts.Remove(contextId))
                {
                    XuaLogger.ResourceRedirector.Debug($"移除资源加载上下文: {contextId}");
                }
            }
        }

        /// <summary>
        /// 获取所有活动上下文
        /// </summary>
        public static Dictionary<string, ResourceLoadingContext> GetActiveContexts()
        {
            lock (_lockObject)
            {
                return new Dictionary<string, ResourceLoadingContext>(_activeContexts);
            }
        }

        /// <summary>
        /// 清理所有上下文
        /// </summary>
        public static void ClearAllContexts()
        {
            lock (_lockObject)
            {
                var count = _activeContexts.Count;
                _activeContexts.Clear();
                XuaLogger.ResourceRedirector.Info($"清理了 {count} 个资源加载上下文");
            }
        }

        /// <summary>
        /// 获取加载统计信息
        /// </summary>
        public static ResourceLoadingStatistics GetStatistics()
        {
            lock (_lockObject)
            {
                var statistics = new ResourceLoadingStatistics();
                
                foreach (var context in _activeContexts.Values)
                {
                    if (context.IsCompleted)
                    {
                        if (context.IsSuccessful)
                        {
                            statistics.SuccessfulLoads++;
                            statistics.TotalLoadTime += context.Duration;
                        }
                        else
                        {
                            statistics.FailedLoads++;
                        }
                    }
                    else
                    {
                        statistics.ActiveLoads++;
                    }
                }

                if (statistics.SuccessfulLoads > 0)
                {
                    statistics.AverageLoadTime = TimeSpan.FromMilliseconds(
                        statistics.TotalLoadTime.TotalMilliseconds / statistics.SuccessfulLoads);
                }

                return statistics;
            }
        }

        /// <summary>
        /// 检查是否需要特殊处理
        /// </summary>
        public bool RequiresSpecialHandling()
        {
            return IsIL2CPP || UnityVersion >= new Version(2022, 3);
        }

        /// <summary>
        /// 获取兼容性建议
        /// </summary>
        public string GetCompatibilityAdvice()
        {
            var advice = new List<string>();

            if (IsIL2CPP)
            {
                advice.Add("使用IL2CPP兼容的加载方式");
                advice.Add("注意类型转换的安全性");
            }

            if (UnityVersion >= new Version(2022, 3))
            {
                advice.Add("使用Unity 2022+的新API");
                advice.Add("注意AssetBundle加载方式的变化");
            }

            return string.Join(", ", advice.ToArray());
        }

        /// <summary>
        /// 获取上下文信息
        /// </summary>
        public string GetContextInfo()
        {
            return $"上下文ID: {ContextId}, 路径: {ResourcePath}, 类型: {LoadType}, " +
                   $"状态: {(IsCompleted ? (IsSuccessful ? "成功" : "失败") : "进行中")}, " +
                   $"耗时: {Duration.TotalMilliseconds:F2}ms";
        }
    }

    /// <summary>
    /// 资源加载上下文类型枚举
    /// </summary>
    public enum ResourceLoadingContextType
    {
        AssetBundle,
        Asset,
        Resource,
        Font,
        Texture,
        Audio,
        Other
    }

    /// <summary>
    /// 资源加载统计信息
    /// </summary>
    public class ResourceLoadingStatistics
    {
        public int ActiveLoads { get; set; }
        public int SuccessfulLoads { get; set; }
        public int FailedLoads { get; set; }
        public TimeSpan TotalLoadTime { get; set; }
        public TimeSpan AverageLoadTime { get; set; }

        public int TotalLoads => SuccessfulLoads + FailedLoads;
        public double SuccessRate => TotalLoads > 0 ? (double)SuccessfulLoads / TotalLoads : 0;
    }
}
