using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;

namespace XUnity.Common.Utilities
{
    /// <summary>
    /// 快速反射辅助类，提供高性能的反射操作
    /// </summary>
    public static class FastReflectionHelper
    {
        private static readonly Dictionary<string, Delegate> _delegateCache = new Dictionary<string, Delegate>();
        private static readonly Dictionary<string, Func<object, object>> _getterCache = new Dictionary<string, Func<object, object>>();
        private static readonly Dictionary<string, Action<object, object>> _setterCache = new Dictionary<string, Action<object, object>>();
        private static readonly Dictionary<string, Func<object, object[], object>> _methodCache = new Dictionary<string, Func<object, object[], object>>();
        
        private static readonly object _lockObject = new object();
        private static bool _initialized = false;

        /// <summary>
        /// 初始化快速反射辅助类
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            lock (_lockObject)
            {
                if (_initialized) return;

                try
                {
                    ConfigureFastReflectionSettings();
                    _initialized = true;
                    XuaLogger.AutoTranslator.Info("快速反射辅助类初始化完成");
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Error(ex, "初始化快速反射辅助类时发生错误");
                    _initialized = true;
                }
            }
        }

        /// <summary>
        /// 配置快速反射设置
        /// </summary>
        private static void ConfigureFastReflectionSettings()
        {
            var compatibilityInfo = CompatibilityHelper.CompatibilityInfo;
            
            if (compatibilityInfo.IsIL2CPP)
            {
                // IL2CPP环境下的快速反射设置
                XuaLogger.AutoTranslator.Info("配置IL2CPP环境快速反射设置");
            }
            
            if (compatibilityInfo.IsUnity2022OrHigher)
            {
                // Unity 2022+的快速反射设置
                XuaLogger.AutoTranslator.Info("配置Unity 2022+快速反射设置");
            }
        }

        /// <summary>
        /// 创建快速属性获取器
        /// </summary>
        public static Func<object, object> CreateFastPropertyGetter(PropertyInfo property)
        {
            if (property == null || !property.CanRead)
            {
                return null;
            }

            Initialize();

            var cacheKey = $"getter:{property.DeclaringType.FullName}.{property.Name}";

            lock (_lockObject)
            {
                if (_getterCache.TryGetValue(cacheKey, out var cachedGetter))
                {
                    return cachedGetter;
                }

                try
                {
                    var getter = CreatePropertyGetterExpression(property);
                    if (getter != null)
                    {
                        _getterCache[cacheKey] = getter;
                    }
                    return getter;
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, $"创建快速属性获取器失败: {property.DeclaringType.FullName}.{property.Name}");
                    return null;
                }
            }
        }

        /// <summary>
        /// 创建快速属性设置器
        /// </summary>
        public static Action<object, object> CreateFastPropertySetter(PropertyInfo property)
        {
            if (property == null || !property.CanWrite)
            {
                return null;
            }

            Initialize();

            var cacheKey = $"setter:{property.DeclaringType.FullName}.{property.Name}";

            lock (_lockObject)
            {
                if (_setterCache.TryGetValue(cacheKey, out var cachedSetter))
                {
                    return cachedSetter;
                }

                try
                {
                    var setter = CreatePropertySetterExpression(property);
                    if (setter != null)
                    {
                        _setterCache[cacheKey] = setter;
                    }
                    return setter;
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, $"创建快速属性设置器失败: {property.DeclaringType.FullName}.{property.Name}");
                    return null;
                }
            }
        }

        /// <summary>
        /// 创建快速方法调用器
        /// </summary>
        public static Func<object, object[], object> CreateFastMethodInvoker(MethodInfo method)
        {
            if (method == null)
            {
                return null;
            }

            Initialize();

            var cacheKey = $"method:{method.DeclaringType.FullName}.{method.Name}.{string.Join(",", method.GetParameters().Select(p => p.ParameterType.FullName).ToArray())}";

            lock (_lockObject)
            {
                if (_methodCache.TryGetValue(cacheKey, out var cachedInvoker))
                {
                    return cachedInvoker;
                }

                try
                {
                    var invoker = CreateMethodInvokerExpression(method);
                    if (invoker != null)
                    {
                        _methodCache[cacheKey] = invoker;
                    }
                    return invoker;
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, $"创建快速方法调用器失败: {method.DeclaringType.FullName}.{method.Name}");
                    return null;
                }
            }
        }

        /// <summary>
        /// 创建属性获取器表达式
        /// </summary>
        private static Func<object, object> CreatePropertyGetterExpression(PropertyInfo property)
        {
            try
            {
                var instanceParam = Expression.Parameter(typeof(object), "instance");
                var instance = Expression.Convert(instanceParam, property.DeclaringType);
                var propertyAccess = Expression.Property(instance, property);
                var convert = Expression.Convert(propertyAccess, typeof(object));
                
                var lambda = Expression.Lambda<Func<object, object>>(convert, instanceParam);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"创建属性获取器表达式失败: {property.Name}");
                return null;
            }
        }

        /// <summary>
        /// 创建属性设置器表达式
        /// </summary>
        private static Action<object, object> CreatePropertySetterExpression(PropertyInfo property)
        {
            try
            {
                var instanceParam = Expression.Parameter(typeof(object), "instance");
                var valueParam = Expression.Parameter(typeof(object), "value");
                var instance = Expression.Convert(instanceParam, property.DeclaringType);
                var value = Expression.Convert(valueParam, property.PropertyType);
                var propertyAccess = Expression.Property(instance, property);
                // .NET 3.5 doesn't have Expression.Assign, use a different approach
                var assign = Expression.Call(instance, property.GetSetMethod(true), value);
                
                var lambda = Expression.Lambda<Action<object, object>>(assign, instanceParam, valueParam);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"创建属性设置器表达式失败: {property.Name}");
                return null;
            }
        }

        /// <summary>
        /// 创建方法调用器表达式
        /// </summary>
        private static Func<object, object[], object> CreateMethodInvokerExpression(MethodInfo method)
        {
            try
            {
                var instanceParam = Expression.Parameter(typeof(object), "instance");
                var argsParam = Expression.Parameter(typeof(object[]), "args");
                
                var instance = method.IsStatic ? null : Expression.Convert(instanceParam, method.DeclaringType);
                
                var parameters = method.GetParameters();
                var arguments = new Expression[parameters.Length];
                
                for (int i = 0; i < parameters.Length; i++)
                {
                    var argAccess = Expression.ArrayIndex(argsParam, Expression.Constant(i));
                    arguments[i] = Expression.Convert(argAccess, parameters[i].ParameterType);
                }
                
                var methodCall = method.IsStatic 
                    ? Expression.Call(method, arguments)
                    : Expression.Call(instance, method, arguments);
                
                var convert = Expression.Convert(methodCall, typeof(object));
                
                var lambda = Expression.Lambda<Func<object, object[], object>>(convert, instanceParam, argsParam);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"创建方法调用器表达式失败: {method.Name}");
                return null;
            }
        }

        /// <summary>
        /// 快速获取属性值
        /// </summary>
        public static object FastGetPropertyValue(object instance, PropertyInfo property)
        {
            if (instance == null || property == null)
            {
                return null;
            }

            try
            {
                var getter = CreateFastPropertyGetter(property);
                if (getter != null)
                {
                    return getter(instance);
                }
                else
                {
                    // 回退到标准反射
                    return property.GetValue(instance, null);
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"快速获取属性值失败: {property.Name}");
                return null;
            }
        }

        /// <summary>
        /// 快速设置属性值
        /// </summary>
        public static bool FastSetPropertyValue(object instance, PropertyInfo property, object value)
        {
            if (instance == null || property == null)
            {
                return false;
            }

            try
            {
                var setter = CreateFastPropertySetter(property);
                if (setter != null)
                {
                    setter(instance, value);
                    return true;
                }
                else
                {
                    // 回退到标准反射
                    property.SetValue(instance, value, null);
                    return true;
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"快速设置属性值失败: {property.Name}");
                return false;
            }
        }

        /// <summary>
        /// 快速调用方法
        /// </summary>
        public static object FastInvokeMethod(object instance, MethodInfo method, object[] parameters)
        {
            if (method == null)
            {
                return null;
            }

            try
            {
                var invoker = CreateFastMethodInvoker(method);
                if (invoker != null)
                {
                    return invoker(instance, parameters);
                }
                else
                {
                    // 回退到标准反射
                    return method.Invoke(instance, parameters);
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"快速调用方法失败: {method.Name}");
                return null;
            }
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public static void ClearAllCache()
        {
            lock (_lockObject)
            {
                _delegateCache.Clear();
                _getterCache.Clear();
                _setterCache.Clear();
                _methodCache.Clear();
                
                XuaLogger.AutoTranslator.Info("快速反射缓存已清除");
            }
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        public static FastReflectionCacheStatistics GetCacheStatistics()
        {
            lock (_lockObject)
            {
                return new FastReflectionCacheStatistics
                {
                    DelegateCacheCount = _delegateCache.Count,
                    GetterCacheCount = _getterCache.Count,
                    SetterCacheCount = _setterCache.Count,
                    MethodCacheCount = _methodCache.Count
                };
            }
        }

        /// <summary>
        /// 获取快速反射兼容性建议
        /// </summary>
        public static string GetFastReflectionCompatibilityAdvice()
        {
            var advice = new List<string>();
            var compatibilityInfo = CompatibilityHelper.CompatibilityInfo;

            if (compatibilityInfo.IsIL2CPP)
            {
                advice.Add("IL2CPP环境下表达式编译可能受限");
                advice.Add("建议使用预编译的委托");
                advice.Add("避免动态创建表达式树");
            }

            if (compatibilityInfo.IsUnity2022OrHigher)
            {
                advice.Add("Unity 2022+支持新的表达式API");
                advice.Add("建议使用最新的表达式功能");
            }

            return advice.Count > 0 ? string.Join("; ", advice.ToArray()) : "快速反射兼容性良好";
        }
    }

    /// <summary>
    /// 快速反射缓存统计信息
    /// </summary>
    public class FastReflectionCacheStatistics
    {
        public int DelegateCacheCount { get; set; }
        public int GetterCacheCount { get; set; }
        public int SetterCacheCount { get; set; }
        public int MethodCacheCount { get; set; }
        
        public int TotalCacheCount => DelegateCacheCount + GetterCacheCount + SetterCacheCount + MethodCacheCount;
    }
}
