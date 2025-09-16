using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;

namespace XUnity.Common.Utilities
{
    /// <summary>
    /// 方法信息缓存管理器，提供高效的方法信息缓存和查找
    /// </summary>
    public static class MethodInfoCache
    {
        private static readonly Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>();
        private static readonly Dictionary<string, MethodInfo[]> _typeMethodCache = new Dictionary<string, MethodInfo[]>();
        private static readonly Dictionary<string, MethodInfo[]> _overloadCache = new Dictionary<string, MethodInfo[]>();
        
        private static readonly object _lockObject = new object();
        private static bool _initialized = false;

        /// <summary>
        /// 方法查找结果
        /// </summary>
        public class MethodLookupResult
        {
            public MethodInfo Method { get; set; }
            public bool Found { get; set; }
            public string ErrorMessage { get; set; }
            public TimeSpan LookupTime { get; set; }
        }

        /// <summary>
        /// 初始化方法信息缓存
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            lock (_lockObject)
            {
                if (_initialized) return;

                try
                {
                    ConfigureMethodCacheSettings();
                    _initialized = true;
                    XuaLogger.AutoTranslator.Info("方法信息缓存初始化完成");
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Error(ex, "初始化方法信息缓存时发生错误");
                    _initialized = true;
                }
            }
        }

        /// <summary>
        /// 配置方法缓存设置
        /// </summary>
        private static void ConfigureMethodCacheSettings()
        {
            var compatibilityInfo = CompatibilityHelper.CompatibilityInfo;
            
            if (compatibilityInfo.IsIL2CPP)
            {
                // IL2CPP环境下的方法缓存设置
                XuaLogger.AutoTranslator.Info("配置IL2CPP环境方法缓存设置");
            }
            
            if (compatibilityInfo.IsUnity2022OrHigher)
            {
                // Unity 2022+的方法缓存设置
                XuaLogger.AutoTranslator.Info("配置Unity 2022+方法缓存设置");
            }
        }

        /// <summary>
        /// 获取方法信息（带缓存）
        /// </summary>
        public static MethodInfo GetMethod(string typeName, string methodName, Type[] parameterTypes = null)
        {
            if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(methodName))
            {
                return null;
            }

            Initialize();

            var cacheKey = CreateMethodCacheKey(typeName, methodName, parameterTypes);

            lock (_lockObject)
            {
                if (_methodCache.TryGetValue(cacheKey, out var cachedMethod))
                {
                    return cachedMethod;
                }

                MethodInfo method = null;
                try
                {
                    var type = ReflectionHelper.GetType(typeName);
                    if (type != null)
                    {
                        if (parameterTypes == null || parameterTypes.Length == 0)
                        {
                            method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        }
                        else
                        {
                            method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, parameterTypes, null);
                        }
                    }

                    if (method != null)
                    {
                        _methodCache[cacheKey] = method;
                    }
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, $"获取方法信息时发生错误: {typeName}.{methodName}");
                }

                return method;
            }
        }

        /// <summary>
        /// 获取类型的所有方法（带缓存）
        /// </summary>
        public static MethodInfo[] GetTypeMethods(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return new MethodInfo[0];
            }

            Initialize();

            lock (_lockObject)
            {
                if (_typeMethodCache.TryGetValue(typeName, out var cachedMethods))
                {
                    return cachedMethods;
                }

                MethodInfo[] methods = new MethodInfo[0];
                try
                {
                    var type = ReflectionHelper.GetType(typeName);
                    if (type != null)
                    {
                        methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    }

                    _typeMethodCache[typeName] = methods;
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, $"获取类型方法时发生错误: {typeName}");
                }

                return methods;
            }
        }

        /// <summary>
        /// 获取方法重载（带缓存）
        /// </summary>
        public static MethodInfo[] GetMethodOverloads(string typeName, string methodName)
        {
            if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(methodName))
            {
                return new MethodInfo[0];
            }

            Initialize();

            var cacheKey = $"{typeName}.{methodName}.overloads";

            lock (_lockObject)
            {
                if (_overloadCache.TryGetValue(cacheKey, out var cachedOverloads))
                {
                    return cachedOverloads;
                }

                MethodInfo[] overloads = new MethodInfo[0];
                try
                {
                    var type = ReflectionHelper.GetType(typeName);
                    if (type != null)
                    {
                        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        var overloadList = new List<MethodInfo>();
                        
                        foreach (var method in methods)
                        {
                            if (method.Name == methodName)
                            {
                                overloadList.Add(method);
                            }
                        }
                        
                        overloads = overloadList.ToArray();
                    }

                    _overloadCache[cacheKey] = overloads;
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, $"获取方法重载时发生错误: {typeName}.{methodName}");
                }

                return overloads;
            }
        }

        /// <summary>
        /// 查找最佳匹配的方法
        /// </summary>
        public static MethodLookupResult FindBestMatchMethod(string typeName, string methodName, Type[] parameterTypes)
        {
            var result = new MethodLookupResult();
            var startTime = DateTime.Now;

            try
            {
                // 首先尝试精确匹配
                var exactMethod = GetMethod(typeName, methodName, parameterTypes);
                if (exactMethod != null)
                {
                    result.Method = exactMethod;
                    result.Found = true;
                    result.LookupTime = DateTime.Now - startTime;
                    return result;
                }

                // 如果没有精确匹配，尝试查找重载
                var overloads = GetMethodOverloads(typeName, methodName);
                if (overloads.Length > 0)
                {
                    // 查找最佳匹配的重载
                    var bestMatch = FindBestMatchingOverload(overloads, parameterTypes);
                    if (bestMatch != null)
                    {
                        result.Method = bestMatch;
                        result.Found = true;
                        result.LookupTime = DateTime.Now - startTime;
                        return result;
                    }
                }

                result.Found = false;
                result.ErrorMessage = $"未找到匹配的方法: {typeName}.{methodName}";
                result.LookupTime = DateTime.Now - startTime;
            }
            catch (Exception ex)
            {
                result.Found = false;
                result.ErrorMessage = ex.Message;
                result.LookupTime = DateTime.Now - startTime;
                XuaLogger.AutoTranslator.Debug(ex, $"查找最佳匹配方法时发生错误: {typeName}.{methodName}");
            }

            return result;
        }

        /// <summary>
        /// 查找最佳匹配的重载
        /// </summary>
        private static MethodInfo FindBestMatchingOverload(MethodInfo[] overloads, Type[] parameterTypes)
        {
            if (parameterTypes == null || parameterTypes.Length == 0)
            {
                // 查找无参数的方法
                foreach (var overload in overloads)
                {
                    if (overload.GetParameters().Length == 0)
                    {
                        return overload;
                    }
                }
                return null;
            }

            MethodInfo bestMatch = null;
            int bestScore = -1;

            foreach (var overload in overloads)
            {
                var parameters = overload.GetParameters();
                if (parameters.Length != parameterTypes.Length)
                {
                    continue;
                }

                int score = CalculateParameterMatchScore(parameters, parameterTypes);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatch = overload;
                }
            }

            return bestMatch;
        }

        /// <summary>
        /// 计算参数匹配分数
        /// </summary>
        private static int CalculateParameterMatchScore(ParameterInfo[] methodParameters, Type[] providedTypes)
        {
            int score = 0;

            for (int i = 0; i < methodParameters.Length && i < providedTypes.Length; i++)
            {
                var methodParamType = methodParameters[i].ParameterType;
                var providedType = providedTypes[i];

                if (methodParamType == providedType)
                {
                    score += 100; // 完全匹配
                }
                else if (methodParamType.IsAssignableFrom(providedType))
                {
                    score += 50; // 可赋值匹配
                }
                else if (CanConvert(providedType, methodParamType))
                {
                    score += 25; // 可转换匹配
                }
                else
                {
                    return -1; // 不匹配
                }
            }

            return score;
        }

        /// <summary>
        /// 检查类型是否可以转换
        /// </summary>
        private static bool CanConvert(Type from, Type to)
        {
            try
            {
                // 检查基本类型转换
                if (to.IsAssignableFrom(from))
                {
                    return true;
                }

                // 检查数值类型转换
                if (IsNumericType(from) && IsNumericType(to))
                {
                    return true;
                }

                // 检查字符串转换
                if (from == typeof(string) && to != typeof(string))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查是否为数值类型
        /// </summary>
        private static bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(long) || type == typeof(float) || 
                   type == typeof(double) || type == typeof(decimal) || type == typeof(short) || 
                   type == typeof(byte) || type == typeof(uint) || type == typeof(ulong) || 
                   type == typeof(ushort) || type == typeof(sbyte);
        }

        /// <summary>
        /// 创建方法缓存键
        /// </summary>
        private static string CreateMethodCacheKey(string typeName, string methodName, Type[] parameterTypes)
        {
            if (parameterTypes == null || parameterTypes.Length == 0)
            {
                return $"{typeName}.{methodName}";
            }

            var paramTypes = string.Join(",", parameterTypes.Select(t => t.FullName).ToArray());
            return $"{typeName}.{methodName}.{paramTypes}";
        }

        /// <summary>
        /// 检查方法是否存在
        /// </summary>
        public static bool MethodExists(string typeName, string methodName, Type[] parameterTypes = null)
        {
            return GetMethod(typeName, methodName, parameterTypes) != null;
        }

        /// <summary>
        /// 获取方法签名
        /// </summary>
        public static string GetMethodSignature(MethodInfo method)
        {
            if (method == null)
            {
                return "null";
            }

            var parameters = method.GetParameters();
            var paramTypes = string.Join(", ", parameters.Select(p => p.ParameterType.Name).ToArray());
            
            return $"{method.ReturnType.Name} {method.Name}({paramTypes})";
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public static void ClearAllCache()
        {
            lock (_lockObject)
            {
                _methodCache.Clear();
                _typeMethodCache.Clear();
                _overloadCache.Clear();
                
                XuaLogger.AutoTranslator.Info("方法信息缓存已清除");
            }
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        public static MethodCacheStatistics GetCacheStatistics()
        {
            lock (_lockObject)
            {
                return new MethodCacheStatistics
                {
                    MethodCacheCount = _methodCache.Count,
                    TypeMethodCacheCount = _typeMethodCache.Count,
                    OverloadCacheCount = _overloadCache.Count
                };
            }
        }

        /// <summary>
        /// 获取方法缓存兼容性建议
        /// </summary>
        public static string GetMethodCacheCompatibilityAdvice()
        {
            var advice = new List<string>();
            var compatibilityInfo = CompatibilityHelper.CompatibilityInfo;

            if (compatibilityInfo.IsIL2CPP)
            {
                advice.Add("IL2CPP环境下方法查找可能较慢");
                advice.Add("建议使用缓存机制提高方法查找性能");
                advice.Add("避免频繁的方法反射调用");
            }

            if (compatibilityInfo.IsUnity2022OrHigher)
            {
                advice.Add("Unity 2022+支持新的反射API");
                advice.Add("建议使用最新的方法查找功能");
            }

            return advice.Count > 0 ? string.Join("; ", advice.ToArray()) : "方法缓存兼容性良好";
        }
    }

    /// <summary>
    /// 方法缓存统计信息
    /// </summary>
    public class MethodCacheStatistics
    {
        public int MethodCacheCount { get; set; }
        public int TypeMethodCacheCount { get; set; }
        public int OverloadCacheCount { get; set; }
        
        public int TotalCacheCount => MethodCacheCount + TypeMethodCacheCount + OverloadCacheCount;
    }
}
