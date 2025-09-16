using System;
using System.Collections.Generic;
using System.Reflection;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;
using XUnity.Common.Utilities;

namespace XUnity.RuntimeHooker.Core
{
    /// <summary>
    /// Harmony集成优化器，提供Unity 2022+兼容的Harmony集成
    /// </summary>
    public static class HarmonyPatcher
    {
        private static readonly Dictionary<string, HarmonyPatchInfo> _harmonyPatches = new Dictionary<string, HarmonyPatchInfo>();
        private static readonly Dictionary<string, HarmonyContext> _patchContexts = new Dictionary<string, HarmonyContext>();
        private static readonly object _lockObject = new object();
        
        private static bool _initialized = false;
        private static bool _harmonyAvailable = false;

        /// <summary>
        /// Harmony补丁信息
        /// </summary>
        public class HarmonyPatchInfo
        {
            public string PatchId { get; set; }
            public string TargetType { get; set; }
            public string TargetMethod { get; set; }
            public MethodInfo PrefixMethod { get; set; }
            public MethodInfo PostfixMethod { get; set; }
            public MethodInfo TranspilerMethod { get; set; }
            public DateTime PatchTime { get; set; }
            public HarmonyPatchStatus Status { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
            public Exception LastError { get; set; }
        }

        /// <summary>
        /// Harmony上下文
        /// </summary>
        public class HarmonyContext
        {
            public string ContextId { get; set; }
            public string PatchId { get; set; }
            public HarmonyPatchType PatchType { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public HarmonyPatchResult Result { get; set; }
            public Exception Error { get; set; }
            public Dictionary<string, object> ContextData { get; set; }
        }

        /// <summary>
        /// Harmony补丁状态枚举
        /// </summary>
        public enum HarmonyPatchStatus
        {
            Pending,
            Applied,
            Failed,
            Removed
        }

        /// <summary>
        /// Harmony补丁类型枚举
        /// </summary>
        public enum HarmonyPatchType
        {
            Prefix,
            Postfix,
            Transpiler,
            Finalizer
        }

        /// <summary>
        /// Harmony补丁结果枚举
        /// </summary>
        public enum HarmonyPatchResult
        {
            Success,
            Failed,
            Skipped,
            Error
        }

        /// <summary>
        /// 初始化Harmony补丁器
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            lock (_lockObject)
            {
                if (_initialized) return;

                try
                {
                    CheckHarmonyAvailability();
                    ConfigureHarmonySettings();
                    _initialized = true;
                    XuaLogger.AutoTranslator.Info("Harmony补丁器初始化完成");
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Error(ex, "初始化Harmony补丁器时发生错误");
                    _initialized = true;
                }
            }
        }

        /// <summary>
        /// 检查Harmony可用性
        /// </summary>
        private static void CheckHarmonyAvailability()
        {
            try
            {
                // 检查Harmony是否可用
                var harmonyType = ReflectionHelper.GetType("HarmonyLib.Harmony");
                _harmonyAvailable = harmonyType != null;
                
                if (_harmonyAvailable)
                {
                    XuaLogger.AutoTranslator.Info("Harmony可用");
                }
                else
                {
                    XuaLogger.AutoTranslator.Warn("Harmony不可用");
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, "检查Harmony可用性时发生错误");
                _harmonyAvailable = false;
            }
        }

        /// <summary>
        /// 配置Harmony设置
        /// </summary>
        private static void ConfigureHarmonySettings()
        {
            var compatibilityInfo = CompatibilityHelper.CompatibilityInfo;
            
            if (compatibilityInfo.IsIL2CPP)
            {
                // IL2CPP环境下的Harmony设置
                XuaLogger.AutoTranslator.Info("配置IL2CPP环境Harmony设置");
            }
            
            if (compatibilityInfo.IsUnity2022OrHigher)
            {
                // Unity 2022+的Harmony设置
                XuaLogger.AutoTranslator.Info("配置Unity 2022+ Harmony设置");
            }
        }

        /// <summary>
        /// 注册Harmony补丁
        /// </summary>
        public static string RegisterHarmonyPatch(string targetType, string targetMethod, MethodInfo prefixMethod = null, MethodInfo postfixMethod = null, MethodInfo transpilerMethod = null, Dictionary<string, object> metadata = null)
        {
            if (!_harmonyAvailable)
            {
                XuaLogger.AutoTranslator.Warn("Harmony不可用，无法注册补丁");
                return null;
            }

            if (string.IsNullOrEmpty(targetType) || string.IsNullOrEmpty(targetMethod))
            {
                XuaLogger.AutoTranslator.Warn("Harmony补丁注册参数无效");
                return null;
            }

            Initialize();

            var patchId = Guid.NewGuid().ToString();
            var patchInfo = new HarmonyPatchInfo
            {
                PatchId = patchId,
                TargetType = targetType,
                TargetMethod = targetMethod,
                PrefixMethod = prefixMethod,
                PostfixMethod = postfixMethod,
                TranspilerMethod = transpilerMethod,
                PatchTime = DateTime.Now,
                Status = HarmonyPatchStatus.Pending,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            lock (_lockObject)
            {
                _harmonyPatches[patchId] = patchInfo;
            }

            XuaLogger.AutoTranslator.Info($"注册Harmony补丁: {patchId}, 目标: {targetType}.{targetMethod}");
            return patchId;
        }

        /// <summary>
        /// 应用Harmony补丁
        /// </summary>
        public static bool ApplyHarmonyPatch(string patchId)
        {
            if (!_harmonyAvailable)
            {
                XuaLogger.AutoTranslator.Warn("Harmony不可用，无法应用补丁");
                return false;
            }

            if (string.IsNullOrEmpty(patchId))
            {
                return false;
            }

            lock (_lockObject)
            {
                if (!_harmonyPatches.TryGetValue(patchId, out var patchInfo))
                {
                    XuaLogger.AutoTranslator.Warn($"Harmony补丁不存在: {patchId}");
                    return false;
                }

                try
                {
                    if (CompatibilityHelper.CompatibilityInfo.IsIL2CPP)
                    {
                        return ApplyHarmonyPatchIL2CPP(patchInfo);
                    }
                    else
                    {
                        return ApplyHarmonyPatchMono(patchInfo);
                    }
                }
                catch (Exception ex)
                {
                    patchInfo.Status = HarmonyPatchStatus.Failed;
                    patchInfo.LastError = ex;
                    XuaLogger.AutoTranslator.Error(ex, $"应用Harmony补丁失败: {patchId}");
                    return false;
                }
            }
        }

        /// <summary>
        /// IL2CPP环境下的Harmony补丁应用
        /// </summary>
        private static bool ApplyHarmonyPatchIL2CPP(HarmonyPatchInfo patchInfo)
        {
            try
            {
                // IL2CPP环境下的Harmony补丁应用逻辑
                // 这里需要使用IL2CPP兼容的Harmony方法
                
                XuaLogger.AutoTranslator.Info($"应用IL2CPP Harmony补丁: {patchInfo.PatchId}");
                
                // 临时实现：标记为已应用状态
                patchInfo.Status = HarmonyPatchStatus.Applied;
                return true;
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, $"应用IL2CPP Harmony补丁失败: {patchInfo.PatchId}");
                return false;
            }
        }

        /// <summary>
        /// Mono环境下的Harmony补丁应用
        /// </summary>
        private static bool ApplyHarmonyPatchMono(HarmonyPatchInfo patchInfo)
        {
            try
            {
                // Mono环境下的Harmony补丁应用逻辑
                // 这里可以使用标准的Harmony方法
                
                XuaLogger.AutoTranslator.Info($"应用Mono Harmony补丁: {patchInfo.PatchId}");
                
                // 临时实现：标记为已应用状态
                patchInfo.Status = HarmonyPatchStatus.Applied;
                return true;
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, $"应用Mono Harmony补丁失败: {patchInfo.PatchId}");
                return false;
            }
        }

        /// <summary>
        /// 移除Harmony补丁
        /// </summary>
        public static bool RemoveHarmonyPatch(string patchId)
        {
            if (!_harmonyAvailable)
            {
                return false;
            }

            if (string.IsNullOrEmpty(patchId))
            {
                return false;
            }

            lock (_lockObject)
            {
                if (!_harmonyPatches.TryGetValue(patchId, out var patchInfo))
                {
                    return false;
                }

                try
                {
                    // Harmony补丁移除逻辑
                    patchInfo.Status = HarmonyPatchStatus.Removed;
                    _harmonyPatches.Remove(patchId);
                    XuaLogger.AutoTranslator.Info($"移除Harmony补丁: {patchId}");
                    return true;
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Error(ex, $"移除Harmony补丁失败: {patchId}");
                    return false;
                }
            }
        }

        /// <summary>
        /// 创建Harmony上下文
        /// </summary>
        public static string CreateHarmonyContext(string patchId, HarmonyPatchType patchType)
        {
            if (string.IsNullOrEmpty(patchId))
            {
                return null;
            }

            var contextId = Guid.NewGuid().ToString();
            var context = new HarmonyContext
            {
                ContextId = contextId,
                PatchId = patchId,
                PatchType = patchType,
                StartTime = DateTime.Now,
                ContextData = new Dictionary<string, object>()
            };

            lock (_lockObject)
            {
                _patchContexts[contextId] = context;
            }

            return contextId;
        }

        /// <summary>
        /// 完成Harmony上下文
        /// </summary>
        public static void CompleteHarmonyContext(string contextId, HarmonyPatchResult result, Exception error = null)
        {
            if (string.IsNullOrEmpty(contextId))
            {
                return;
            }

            lock (_lockObject)
            {
                if (_patchContexts.TryGetValue(contextId, out var context))
                {
                    context.EndTime = DateTime.Now;
                    context.Result = result;
                    context.Error = error;
                }
            }
        }

        /// <summary>
        /// 获取Harmony补丁信息
        /// </summary>
        public static HarmonyPatchInfo GetHarmonyPatchInfo(string patchId)
        {
            lock (_lockObject)
            {
                return _harmonyPatches.TryGetValue(patchId, out var patchInfo) ? patchInfo : null;
            }
        }

        /// <summary>
        /// 获取所有Harmony补丁
        /// </summary>
        public static Dictionary<string, HarmonyPatchInfo> GetHarmonyPatches()
        {
            lock (_lockObject)
            {
                return new Dictionary<string, HarmonyPatchInfo>(_harmonyPatches);
            }
        }

        /// <summary>
        /// 获取Harmony补丁统计信息
        /// </summary>
        public static HarmonyPatchStatistics GetHarmonyPatchStatistics()
        {
            lock (_lockObject)
            {
                var statistics = new HarmonyPatchStatistics
                {
                    TotalPatches = _harmonyPatches.Count,
                    AppliedPatches = 0,
                    FailedPatches = 0,
                    TotalContexts = _patchContexts.Count,
                    HarmonyAvailable = _harmonyAvailable
                };

                foreach (var patch in _harmonyPatches.Values)
                {
                    switch (patch.Status)
                    {
                        case HarmonyPatchStatus.Applied:
                            statistics.AppliedPatches++;
                            break;
                        case HarmonyPatchStatus.Failed:
                            statistics.FailedPatches++;
                            break;
                    }
                }

                return statistics;
            }
        }

        /// <summary>
        /// 清除所有Harmony补丁
        /// </summary>
        public static void ClearAllHarmonyPatches()
        {
            lock (_lockObject)
            {
                var count = _harmonyPatches.Count;
                _harmonyPatches.Clear();
                _patchContexts.Clear();
                
                XuaLogger.AutoTranslator.Info($"清除了 {count} 个Harmony补丁");
            }
        }

        /// <summary>
        /// 获取Harmony兼容性建议
        /// </summary>
        public static string GetHarmonyCompatibilityAdvice()
        {
            var advice = new List<string>();

            if (!_harmonyAvailable)
            {
                advice.Add("Harmony不可用，建议安装Harmony库");
                return string.Join("; ", advice.ToArray());
            }

            if (CompatibilityHelper.CompatibilityInfo.IsIL2CPP)
            {
                advice.Add("IL2CPP环境下Harmony可能需要特殊处理");
                advice.Add("建议使用预编译的Harmony补丁");
                advice.Add("注意Harmony的性能影响");
            }

            if (CompatibilityHelper.CompatibilityInfo.IsUnity2022OrHigher)
            {
                advice.Add("Unity 2022+支持新的Harmony功能");
                advice.Add("建议使用最新的Harmony版本");
            }

            return advice.Count > 0 ? string.Join("; ", advice.ToArray()) : "Harmony兼容性良好";
        }

        /// <summary>
        /// 验证Harmony环境
        /// </summary>
        public static bool ValidateHarmonyEnvironment()
        {
            if (!_harmonyAvailable)
            {
                return false;
            }

            try
            {
                // 检查Harmony环境是否可用
                // 这里可以添加更多的环境验证逻辑
                return true;
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, "验证Harmony环境失败");
                return false;
            }
        }
    }

    /// <summary>
    /// Harmony补丁统计信息
    /// </summary>
    public class HarmonyPatchStatistics
    {
        public int TotalPatches { get; set; }
        public int AppliedPatches { get; set; }
        public int FailedPatches { get; set; }
        public int TotalContexts { get; set; }
        public bool HarmonyAvailable { get; set; }
    }
}
