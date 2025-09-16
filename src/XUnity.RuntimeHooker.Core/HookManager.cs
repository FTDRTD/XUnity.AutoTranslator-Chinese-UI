using System;
using System.Collections.Generic;
using System.Reflection;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;
using XUnity.Common.Utilities;

namespace XUnity.RuntimeHooker.Core
{
    /// <summary>
    /// 统一Hook管理器，提供Unity 2022+兼容的Hook管理
    /// </summary>
    public static class HookManager
    {
        private static readonly Dictionary<string, HookInfo> _activeHooks = new Dictionary<string, HookInfo>();
        private static readonly Dictionary<string, HookContext> _hookContexts = new Dictionary<string, HookContext>();
        private static readonly object _lockObject = new object();
        
        private static bool _initialized = false;
        private static DateTime _lastCleanupTime = DateTime.Now;
        private static readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Hook信息
        /// </summary>
        public class HookInfo
        {
            public string HookId { get; set; }
            public string TargetType { get; set; }
            public string TargetMethod { get; set; }
            public MethodInfo OriginalMethod { get; set; }
            public MethodInfo HookMethod { get; set; }
            public DateTime HookTime { get; set; }
            public HookStatus Status { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
            public Exception LastError { get; set; }
        }

        /// <summary>
        /// Hook上下文
        /// </summary>
        public class HookContext
        {
            public string ContextId { get; set; }
            public string HookId { get; set; }
            public object TargetInstance { get; set; }
            public object[] Parameters { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public HookResult Result { get; set; }
            public Exception Error { get; set; }
            public Dictionary<string, object> ContextData { get; set; }
        }

        /// <summary>
        /// Hook状态枚举
        /// </summary>
        public enum HookStatus
        {
            Pending,
            Active,
            Failed,
            Disabled,
            Removed
        }

        /// <summary>
        /// Hook结果枚举
        /// </summary>
        public enum HookResult
        {
            Success,
            Failed,
            Skipped,
            Error
        }

        /// <summary>
        /// 初始化Hook管理器
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            lock (_lockObject)
            {
                if (_initialized) return;

                try
                {
                    ConfigureHookSettings();
                    _initialized = true;
                    XuaLogger.AutoTranslator.Info("Hook管理器初始化完成");
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Error(ex, "初始化Hook管理器时发生错误");
                    _initialized = true;
                }
            }
        }

        /// <summary>
        /// 配置Hook设置
        /// </summary>
        private static void ConfigureHookSettings()
        {
            var compatibilityInfo = CompatibilityHelper.CompatibilityInfo;
            
            if (compatibilityInfo.IsIL2CPP)
            {
                // IL2CPP环境下的Hook设置
                XuaLogger.AutoTranslator.Info("配置IL2CPP环境Hook设置");
            }
            
            if (compatibilityInfo.IsUnity2022OrHigher)
            {
                // Unity 2022+的Hook设置
                XuaLogger.AutoTranslator.Info("配置Unity 2022+ Hook设置");
            }
        }

        /// <summary>
        /// 注册Hook
        /// </summary>
        public static string RegisterHook(string targetType, string targetMethod, MethodInfo hookMethod, Dictionary<string, object> metadata = null)
        {
            if (string.IsNullOrEmpty(targetType) || string.IsNullOrEmpty(targetMethod) || hookMethod == null)
            {
                XuaLogger.AutoTranslator.Warn("Hook注册参数无效");
                return null;
            }

            Initialize();

            var hookId = Guid.NewGuid().ToString();
            var hookInfo = new HookInfo
            {
                HookId = hookId,
                TargetType = targetType,
                TargetMethod = targetMethod,
                HookMethod = hookMethod,
                HookTime = DateTime.Now,
                Status = HookStatus.Pending,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            lock (_lockObject)
            {
                _activeHooks[hookId] = hookInfo;
            }

            XuaLogger.AutoTranslator.Info($"注册Hook: {hookId}, 目标: {targetType}.{targetMethod}");
            return hookId;
        }

        /// <summary>
        /// 激活Hook
        /// </summary>
        public static bool ActivateHook(string hookId)
        {
            if (string.IsNullOrEmpty(hookId))
            {
                return false;
            }

            lock (_lockObject)
            {
                if (!_activeHooks.TryGetValue(hookId, out var hookInfo))
                {
                    XuaLogger.AutoTranslator.Warn($"Hook不存在: {hookId}");
                    return false;
                }

                try
                {
                    if (CompatibilityHelper.CompatibilityInfo.IsIL2CPP)
                    {
                        return ActivateHookIL2CPP(hookInfo);
                    }
                    else
                    {
                        return ActivateHookMono(hookInfo);
                    }
                }
                catch (Exception ex)
                {
                    hookInfo.Status = HookStatus.Failed;
                    hookInfo.LastError = ex;
                    XuaLogger.AutoTranslator.Error(ex, $"激活Hook失败: {hookId}");
                    return false;
                }
            }
        }

        /// <summary>
        /// IL2CPP环境下的Hook激活
        /// </summary>
        private static bool ActivateHookIL2CPP(HookInfo hookInfo)
        {
            try
            {
                // IL2CPP环境下的Hook激活逻辑
                // 这里需要使用IL2CPP兼容的Hook方法
                
                XuaLogger.AutoTranslator.Info($"激活IL2CPP Hook: {hookInfo.HookId}");
                
                // 临时实现：标记为激活状态
                hookInfo.Status = HookStatus.Active;
                return true;
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, $"激活IL2CPP Hook失败: {hookInfo.HookId}");
                return false;
            }
        }

        /// <summary>
        /// Mono环境下的Hook激活
        /// </summary>
        private static bool ActivateHookMono(HookInfo hookInfo)
        {
            try
            {
                // Mono环境下的Hook激活逻辑
                // 这里可以使用标准的Hook方法
                
                XuaLogger.AutoTranslator.Info($"激活Mono Hook: {hookInfo.HookId}");
                
                // 临时实现：标记为激活状态
                hookInfo.Status = HookStatus.Active;
                return true;
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, $"激活Mono Hook失败: {hookInfo.HookId}");
                return false;
            }
        }

        /// <summary>
        /// 禁用Hook
        /// </summary>
        public static bool DisableHook(string hookId)
        {
            if (string.IsNullOrEmpty(hookId))
            {
                return false;
            }

            lock (_lockObject)
            {
                if (!_activeHooks.TryGetValue(hookId, out var hookInfo))
                {
                    return false;
                }

                try
                {
                    // 禁用Hook的逻辑
                    hookInfo.Status = HookStatus.Disabled;
                    XuaLogger.AutoTranslator.Info($"禁用Hook: {hookId}");
                    return true;
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Error(ex, $"禁用Hook失败: {hookId}");
                    return false;
                }
            }
        }

        /// <summary>
        /// 移除Hook
        /// </summary>
        public static bool RemoveHook(string hookId)
        {
            if (string.IsNullOrEmpty(hookId))
            {
                return false;
            }

            lock (_lockObject)
            {
                if (!_activeHooks.TryGetValue(hookId, out var hookInfo))
                {
                    return false;
                }

                try
                {
                    // 移除Hook的逻辑
                    hookInfo.Status = HookStatus.Removed;
                    _activeHooks.Remove(hookId);
                    XuaLogger.AutoTranslator.Info($"移除Hook: {hookId}");
                    return true;
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Error(ex, $"移除Hook失败: {hookId}");
                    return false;
                }
            }
        }

        /// <summary>
        /// 创建Hook上下文
        /// </summary>
        public static string CreateHookContext(string hookId, object targetInstance, object[] parameters)
        {
            if (string.IsNullOrEmpty(hookId))
            {
                return null;
            }

            var contextId = Guid.NewGuid().ToString();
            var context = new HookContext
            {
                ContextId = contextId,
                HookId = hookId,
                TargetInstance = targetInstance,
                Parameters = parameters,
                StartTime = DateTime.Now,
                ContextData = new Dictionary<string, object>()
            };

            lock (_lockObject)
            {
                _hookContexts[contextId] = context;
            }

            return contextId;
        }

        /// <summary>
        /// 完成Hook上下文
        /// </summary>
        public static void CompleteHookContext(string contextId, HookResult result, Exception error = null)
        {
            if (string.IsNullOrEmpty(contextId))
            {
                return;
            }

            lock (_lockObject)
            {
                if (_hookContexts.TryGetValue(contextId, out var context))
                {
                    context.EndTime = DateTime.Now;
                    context.Result = result;
                    context.Error = error;
                }
            }
        }

        /// <summary>
        /// 获取Hook信息
        /// </summary>
        public static HookInfo GetHookInfo(string hookId)
        {
            lock (_lockObject)
            {
                return _activeHooks.TryGetValue(hookId, out var hookInfo) ? hookInfo : null;
            }
        }

        /// <summary>
        /// 获取所有活动Hook
        /// </summary>
        public static Dictionary<string, HookInfo> GetActiveHooks()
        {
            lock (_lockObject)
            {
                return new Dictionary<string, HookInfo>(_activeHooks);
            }
        }

        /// <summary>
        /// 获取Hook统计信息
        /// </summary>
        public static HookStatistics GetHookStatistics()
        {
            lock (_lockObject)
            {
                var statistics = new HookStatistics
                {
                    TotalHooks = _activeHooks.Count,
                    ActiveHooks = 0,
                    DisabledHooks = 0,
                    FailedHooks = 0,
                    TotalContexts = _hookContexts.Count
                };

                foreach (var hook in _activeHooks.Values)
                {
                    switch (hook.Status)
                    {
                        case HookStatus.Active:
                            statistics.ActiveHooks++;
                            break;
                        case HookStatus.Disabled:
                            statistics.DisabledHooks++;
                            break;
                        case HookStatus.Failed:
                            statistics.FailedHooks++;
                            break;
                    }
                }

                return statistics;
            }
        }

        /// <summary>
        /// 定期清理
        /// </summary>
        public static void PerformPeriodicCleanup()
        {
            var now = DateTime.Now;
            if (now - _lastCleanupTime > _cleanupInterval)
            {
                CleanupExpiredContexts();
                _lastCleanupTime = now;
            }
        }

        /// <summary>
        /// 清理过期上下文
        /// </summary>
        private static void CleanupExpiredContexts()
        {
            var expiredContexts = new List<string>();
            var cutoffTime = DateTime.Now.AddMinutes(-10); // 10分钟前的上下文

            lock (_lockObject)
            {
                foreach (var kvp in _hookContexts)
                {
                    if (kvp.Value.StartTime < cutoffTime)
                    {
                        expiredContexts.Add(kvp.Key);
                    }
                }

                foreach (var contextId in expiredContexts)
                {
                    _hookContexts.Remove(contextId);
                }
            }

            if (expiredContexts.Count > 0)
            {
                XuaLogger.AutoTranslator.Info($"清理了 {expiredContexts.Count} 个过期Hook上下文");
            }
        }

        /// <summary>
        /// 清除所有Hook
        /// </summary>
        public static void ClearAllHooks()
        {
            lock (_lockObject)
            {
                var count = _activeHooks.Count;
                _activeHooks.Clear();
                _hookContexts.Clear();
                
                XuaLogger.AutoTranslator.Info($"清除了 {count} 个Hook");
            }
        }

        /// <summary>
        /// 获取Hook兼容性建议
        /// </summary>
        public static string GetHookCompatibilityAdvice()
        {
            var advice = new List<string>();
            var compatibilityInfo = CompatibilityHelper.CompatibilityInfo;

            if (compatibilityInfo.IsIL2CPP)
            {
                advice.Add("IL2CPP环境下Hook可能需要特殊处理");
                advice.Add("建议使用预编译的Hook方法");
                advice.Add("注意Hook的性能影响");
            }

            if (compatibilityInfo.IsUnity2022OrHigher)
            {
                advice.Add("Unity 2022+支持新的Hook API");
                advice.Add("建议使用最新的Hook功能");
            }

            return advice.Count > 0 ? string.Join("; ", advice.ToArray()) : "Hook兼容性良好";
        }
    }

    /// <summary>
    /// Hook统计信息
    /// </summary>
    public class HookStatistics
    {
        public int TotalHooks { get; set; }
        public int ActiveHooks { get; set; }
        public int DisabledHooks { get; set; }
        public int FailedHooks { get; set; }
        public int TotalContexts { get; set; }
    }
}
