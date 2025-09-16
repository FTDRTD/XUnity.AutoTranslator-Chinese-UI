using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;

namespace XUnity.Common.Utilities
{
    /// <summary>
    /// Unity 2022+兼容的反射辅助类
    /// </summary>
    public static class ReflectionHelper
    {
        private static readonly Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>();
        private static readonly Dictionary<string, PropertyInfo> _propertyCache = new Dictionary<string, PropertyInfo>();
        private static readonly Dictionary<string, FieldInfo> _fieldCache = new Dictionary<string, FieldInfo>();
        private static readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();
        
        private static readonly object _lockObject = new object();
        private static bool _initialized = false;

        /// <summary>
        /// 初始化反射辅助类
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            lock (_lockObject)
            {
                if (_initialized) return;

                try
                {
                    ConfigureReflectionSettings();
                    _initialized = true;
                    XuaLogger.AutoTranslator.Info("反射辅助类初始化完成");
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Error(ex, "初始化反射辅助类时发生错误");
                    _initialized = true;
                }
            }
        }

        /// <summary>
        /// 配置反射设置
        /// </summary>
        private static void ConfigureReflectionSettings()
        {
            var compatibilityInfo = CompatibilityHelper.CompatibilityInfo;
            
            if (compatibilityInfo.IsIL2CPP)
            {
                // IL2CPP环境下的反射设置
                XuaLogger.AutoTranslator.Info("配置IL2CPP环境反射设置");
            }
            
            if (compatibilityInfo.IsUnity2022OrHigher)
            {
                // Unity 2022+的反射设置
                XuaLogger.AutoTranslator.Info("配置Unity 2022+反射设置");
            }
        }

        /// <summary>
        /// 获取类型（带缓存）
        /// </summary>
        public static Type GetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            Initialize();

            lock (_lockObject)
            {
                if (_typeCache.TryGetValue(typeName, out var cachedType))
                {
                    return cachedType;
                }

                Type type = null;
                try
                {
                    if (CompatibilityHelper.CompatibilityInfo.IsIL2CPP)
                    {
                        type = GetTypeIL2CPP(typeName);
                    }
                    else
                    {
                        type = GetTypeMono(typeName);
                    }

                    if (type != null)
                    {
                        _typeCache[typeName] = type;
                    }
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, $"获取类型时发生错误: {typeName}");
                }

                return type;
            }
        }

        /// <summary>
        /// IL2CPP环境下的类型获取
        /// </summary>
        private static Type GetTypeIL2CPP(string typeName)
        {
            try
            {
                // 使用IL2CPPTypeResolver获取类型
                var typeContainer = IL2CPPTypeResolver.ResolveType(typeName);
                return typeContainer?.ClrType;
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"IL2CPP类型获取失败: {typeName}");
                return null;
            }
        }

        /// <summary>
        /// Mono环境下的类型获取
        /// </summary>
        private static Type GetTypeMono(string typeName)
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var type = assembly.GetType(typeName, false);
                        if (type != null)
                        {
                            return type;
                        }
                    }
                    catch
                    {
                        // 忽略错误，继续查找
                    }
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"Mono类型获取失败: {typeName}");
            }

            return null;
        }

        /// <summary>
        /// 获取方法（带缓存）
        /// </summary>
        public static MethodInfo GetMethod(string typeName, string methodName, Type[] parameterTypes = null)
        {
            if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(methodName))
            {
                return null;
            }

            Initialize();

            var cacheKey = $"{typeName}.{methodName}.{string.Join(",", parameterTypes?.Select(t => t.FullName).ToArray() ?? new string[0])}";

            lock (_lockObject)
            {
                if (_methodCache.TryGetValue(cacheKey, out var cachedMethod))
                {
                    return cachedMethod;
                }

                MethodInfo method = null;
                try
                {
                    var type = GetType(typeName);
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
                    XuaLogger.AutoTranslator.Debug(ex, $"获取方法时发生错误: {typeName}.{methodName}");
                }

                return method;
            }
        }

        /// <summary>
        /// 获取属性（带缓存）
        /// </summary>
        public static PropertyInfo GetProperty(string typeName, string propertyName)
        {
            if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            Initialize();

            var cacheKey = $"{typeName}.{propertyName}";

            lock (_lockObject)
            {
                if (_propertyCache.TryGetValue(cacheKey, out var cachedProperty))
                {
                    return cachedProperty;
                }

                PropertyInfo property = null;
                try
                {
                    var type = GetType(typeName);
                    if (type != null)
                    {
                        property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    }

                    if (property != null)
                    {
                        _propertyCache[cacheKey] = property;
                    }
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, $"获取属性时发生错误: {typeName}.{propertyName}");
                }

                return property;
            }
        }

        /// <summary>
        /// 获取字段（带缓存）
        /// </summary>
        public static FieldInfo GetField(string typeName, string fieldName)
        {
            if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(fieldName))
            {
                return null;
            }

            Initialize();

            var cacheKey = $"{typeName}.{fieldName}";

            lock (_lockObject)
            {
                if (_fieldCache.TryGetValue(cacheKey, out var cachedField))
                {
                    return cachedField;
                }

                FieldInfo field = null;
                try
                {
                    var type = GetType(typeName);
                    if (type != null)
                    {
                        field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    }

                    if (field != null)
                    {
                        _fieldCache[cacheKey] = field;
                    }
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, $"获取字段时发生错误: {typeName}.{fieldName}");
                }

                return field;
            }
        }

        /// <summary>
        /// 安全调用方法
        /// </summary>
        public static object SafeInvokeMethod(object instance, string typeName, string methodName, object[] parameters = null)
        {
            try
            {
                var method = GetMethod(typeName, methodName);
                if (method != null)
                {
                    return method.Invoke(instance, parameters);
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"安全调用方法时发生错误: {typeName}.{methodName}");
            }

            return null;
        }

        /// <summary>
        /// 安全获取属性值
        /// </summary>
        public static object SafeGetPropertyValue(object instance, string typeName, string propertyName)
        {
            try
            {
                var property = GetProperty(typeName, propertyName);
                if (property != null && property.CanRead)
                {
                    return property.GetValue(instance, null);
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"安全获取属性值时发生错误: {typeName}.{propertyName}");
            }

            return null;
        }

        /// <summary>
        /// 安全设置属性值
        /// </summary>
        public static bool SafeSetPropertyValue(object instance, string typeName, string propertyName, object value)
        {
            try
            {
                var property = GetProperty(typeName, propertyName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(instance, value, null);
                    return true;
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"安全设置属性值时发生错误: {typeName}.{propertyName}");
            }

            return false;
        }

        /// <summary>
        /// 安全获取字段值
        /// </summary>
        public static object SafeGetFieldValue(object instance, string typeName, string fieldName)
        {
            try
            {
                var field = GetField(typeName, fieldName);
                if (field != null)
                {
                    return field.GetValue(instance);
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"安全获取字段值时发生错误: {typeName}.{fieldName}");
            }

            return null;
        }

        /// <summary>
        /// 安全设置字段值
        /// </summary>
        public static bool SafeSetFieldValue(object instance, string typeName, string fieldName, object value)
        {
            try
            {
                var field = GetField(typeName, fieldName);
                if (field != null)
                {
                    field.SetValue(instance, value);
                    return true;
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"安全设置字段值时发生错误: {typeName}.{fieldName}");
            }

            return false;
        }

        /// <summary>
        /// 检查类型是否存在
        /// </summary>
        public static bool TypeExists(string typeName)
        {
            return GetType(typeName) != null;
        }

        /// <summary>
        /// 检查方法是否存在
        /// </summary>
        public static bool MethodExists(string typeName, string methodName, Type[] parameterTypes = null)
        {
            return GetMethod(typeName, methodName, parameterTypes) != null;
        }

        /// <summary>
        /// 检查属性是否存在
        /// </summary>
        public static bool PropertyExists(string typeName, string propertyName)
        {
            return GetProperty(typeName, propertyName) != null;
        }

        /// <summary>
        /// 检查字段是否存在
        /// </summary>
        public static bool FieldExists(string typeName, string fieldName)
        {
            return GetField(typeName, fieldName) != null;
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public static void ClearAllCache()
        {
            lock (_lockObject)
            {
                _methodCache.Clear();
                _propertyCache.Clear();
                _fieldCache.Clear();
                _typeCache.Clear();
                
                XuaLogger.AutoTranslator.Info("反射缓存已清除");
            }
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        public static ReflectionCacheStatistics GetCacheStatistics()
        {
            lock (_lockObject)
            {
                return new ReflectionCacheStatistics
                {
                    TypeCacheCount = _typeCache.Count,
                    MethodCacheCount = _methodCache.Count,
                    PropertyCacheCount = _propertyCache.Count,
                    FieldCacheCount = _fieldCache.Count
                };
            }
        }

        /// <summary>
        /// 获取反射兼容性建议
        /// </summary>
        public static string GetReflectionCompatibilityAdvice()
        {
            var advice = new List<string>();
            var compatibilityInfo = CompatibilityHelper.CompatibilityInfo;

            if (compatibilityInfo.IsIL2CPP)
            {
                advice.Add("IL2CPP环境下反射性能可能较低");
                advice.Add("建议使用缓存机制提高反射性能");
                advice.Add("避免频繁的动态反射调用");
            }

            if (compatibilityInfo.IsUnity2022OrHigher)
            {
                advice.Add("Unity 2022+支持新的反射API");
                advice.Add("建议使用最新的反射功能");
            }

            return advice.Count > 0 ? string.Join("; ", advice.ToArray()) : "反射兼容性良好";
        }
    }

    /// <summary>
    /// 反射缓存统计信息
    /// </summary>
    public class ReflectionCacheStatistics
    {
        public int TypeCacheCount { get; set; }
        public int MethodCacheCount { get; set; }
        public int PropertyCacheCount { get; set; }
        public int FieldCacheCount { get; set; }
        
        public int TotalCacheCount => TypeCacheCount + MethodCacheCount + PropertyCacheCount + FieldCacheCount;
    }
}
