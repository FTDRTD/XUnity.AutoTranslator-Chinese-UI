using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;
using XUnity.Common.Constants;

#if IL2CPP
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#endif

namespace XUnity.Common.Utilities
{
    /// <summary>
    /// IL2CPP类型解析器，专门处理IL2CPP环境下的类型查找和缓存
    /// </summary>
    public static class IL2CPPTypeResolver
    {
        private static readonly Dictionary<string, TypeContainer> _typeCache = new Dictionary<string, TypeContainer>();
        private static readonly Dictionary<string, IntPtr> _classPointerCache = new Dictionary<string, IntPtr>();
        private static readonly Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>();
        private static readonly Dictionary<string, PropertyInfo> _propertyCache = new Dictionary<string, PropertyInfo>();
        private static readonly Dictionary<string, FieldInfo> _fieldCache = new Dictionary<string, FieldInfo>();
        
        private static bool _initialized = false;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// 初始化IL2CPP类型解析器
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            lock (_lockObject)
            {
                if (_initialized) return;

                try
                {
                    if (CompatibilityHelper.CompatibilityInfo.IsIL2CPP)
                    {
                        InitializeIL2CPP();
                    }
                    else
                    {
                        InitializeMono();
                    }

                    _initialized = true;
                    XuaLogger.AutoTranslator.Info("IL2CPP类型解析器初始化完成");
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Error(ex, "初始化IL2CPP类型解析器时发生错误");
                    _initialized = true; // 标记为已初始化，避免重复尝试
                }
            }
        }

        /// <summary>
        /// 初始化IL2CPP环境
        /// </summary>
        private static void InitializeIL2CPP()
        {
#if IL2CPP
            try
            {
                // 预加载常用的IL2CPP类型
                PreloadCommonTypes();
                
                // 建立类型映射
                BuildTypeMappings();
                
                XuaLogger.AutoTranslator.Info("IL2CPP环境初始化完成");
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, "初始化IL2CPP环境时发生错误");
            }
#else
            XuaLogger.AutoTranslator.Warn("尝试在非IL2CPP环境下初始化IL2CPP类型解析器");
#endif
        }

        /// <summary>
        /// 初始化Mono环境
        /// </summary>
        private static void InitializeMono()
        {
            try
            {
                // Mono环境下的初始化
                PreloadCommonTypes();
                XuaLogger.AutoTranslator.Info("Mono环境初始化完成");
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, "初始化Mono环境时发生错误");
            }
        }

        /// <summary>
        /// 预加载常用类型
        /// </summary>
        private static void PreloadCommonTypes()
        {
            var commonTypes = new[]
            {
                "UnityEngine.Object",
                "UnityEngine.GameObject",
                "UnityEngine.Component",
                "UnityEngine.Transform",
                "UnityEngine.UI.Text",
                "UnityEngine.UI.Image",
                "TMPro.TMP_Text",
                "TMPro.TextMeshProUGUI",
                "UnityEngine.AssetBundle",
                "UnityEngine.Texture2D",
                "UnityEngine.Sprite"
            };

            foreach (var typeName in commonTypes)
            {
                try
                {
                    ResolveType(typeName);
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, $"预加载类型失败: {typeName}");
                }
            }
        }

        /// <summary>
        /// 建立类型映射
        /// </summary>
        private static void BuildTypeMappings()
        {
#if IL2CPP
            try
            {
                // 建立IL2CPP类型到托管类型的映射
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            if (type.Name.StartsWith("Il2Cpp") || type.Namespace?.StartsWith("Il2Cpp") == true)
                            {
                                // 处理IL2CPP类型映射
                                ProcessIL2CPPType(type);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        XuaLogger.AutoTranslator.Debug(ex, $"处理程序集类型时发生错误: {assembly.FullName}");
                    }
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, "建立类型映射时发生错误");
            }
#endif
        }

        /// <summary>
        /// 处理IL2CPP类型
        /// </summary>
        private static void ProcessIL2CPPType(Type type)
        {
#if IL2CPP
            try
            {
                // 使用反射来避免编译时错误
                var il2CppTypeClass = Type.GetType("Il2CppInterop.Runtime.Il2CppType, Il2CppInterop.Runtime");
                if (il2CppTypeClass != null)
                {
                    var fromMethod = il2CppTypeClass.GetMethod("From", new[] { typeof(Type) });
                    if (fromMethod != null)
                    {
                        var il2CppType = fromMethod.Invoke(null, new object[] { type });
                        if (il2CppType != null)
                        {
                            var classPointerProperty = il2CppType.GetType().GetProperty("ClassPointer");
                            if (classPointerProperty != null)
                            {
                                var classPointer = (IntPtr)classPointerProperty.GetValue(il2CppType);
                                if (classPointer != IntPtr.Zero)
                                {
                                    _classPointerCache[type.FullName] = classPointer;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"处理IL2CPP类型时发生错误: {type.FullName}");
            }
#endif
        }

        /// <summary>
        /// 解析类型
        /// </summary>
        public static TypeContainer ResolveType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            // 检查缓存
            if (_typeCache.TryGetValue(typeName, out var cachedType))
            {
                return cachedType;
            }

            TypeContainer typeContainer = null;

            try
            {
                if (CompatibilityHelper.CompatibilityInfo.IsIL2CPP)
                {
                    typeContainer = ResolveTypeIL2CPP(typeName);
                }
                else
                {
                    typeContainer = ResolveTypeMono(typeName);
                }

                // 缓存结果
                if (typeContainer != null)
                {
                    _typeCache[typeName] = typeContainer;
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"解析类型时发生错误: {typeName}");
            }

            return typeContainer;
        }

        /// <summary>
        /// IL2CPP环境下的类型解析
        /// </summary>
        private static TypeContainer ResolveTypeIL2CPP(string typeName)
        {
#if IL2CPP
            try
            {
                // 解析命名空间和类型名
                var lastDot = typeName.LastIndexOf('.');
                string @namespace = string.Empty;
                string className = typeName;

                if (lastDot != -1)
                {
                    @namespace = typeName.Substring(0, lastDot);
                    className = typeName.Substring(lastDot + 1);
                }

                // 获取IL2CPP类指针
                var classPointer = Il2CppUtilities.GetIl2CppClass(@namespace, className);
                
                // 查找托管包装类型
                Type wrapperType = null;
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var type = assembly.GetType(typeName, false);
                        if (type != null)
                        {
                            wrapperType = type;
                            break;
                        }
                    }
                    catch
                    {
                        // 忽略错误，继续查找
                    }
                }

                if (wrapperType != null)
                {
#if IL2CPP
                    var nativeType = classPointer != IntPtr.Zero ? Il2CppType.TypeFromPointer(classPointer) : Il2CppType.From(wrapperType);
                    
                    if (nativeType == null)
                    {
                        XuaLogger.AutoTranslator.Warn($"无法在IL2CPP域中找到类型: {typeName}");
                    }

                    return new TypeContainer(nativeType, wrapperType, classPointer);
#else
                    return new TypeContainer(wrapperType);
#endif
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"IL2CPP类型解析失败: {typeName}");
            }
#endif
            return null;
        }

        /// <summary>
        /// Mono环境下的类型解析
        /// </summary>
        private static TypeContainer ResolveTypeMono(string typeName)
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
#if IL2CPP
                            return new TypeContainer(null, type, IntPtr.Zero);
#else
                            return new TypeContainer(type);
#endif
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
                XuaLogger.AutoTranslator.Debug(ex, $"Mono类型解析失败: {typeName}");
            }

            return null;
        }

        /// <summary>
        /// 解析方法
        /// </summary>
        public static MethodInfo ResolveMethod(string typeName, string methodName, Type[] parameterTypes = null)
        {
            var cacheKey = $"{typeName}.{methodName}.{string.Join(",", parameterTypes?.Select(t => t.FullName).ToArray() ?? new string[0])}";
            
            if (_methodCache.TryGetValue(cacheKey, out var cachedMethod))
            {
                return cachedMethod;
            }

            MethodInfo method = null;
            var typeContainer = ResolveType(typeName);
            
            if (typeContainer?.ClrType != null)
            {
                try
                {
                    if (parameterTypes == null || parameterTypes.Length == 0)
                    {
                        method = typeContainer.ClrType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    }
                    else
                    {
                        method = typeContainer.ClrType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, parameterTypes, null);
                    }
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, $"解析方法时发生错误: {typeName}.{methodName}");
                }
            }

            if (method != null)
            {
                _methodCache[cacheKey] = method;
            }

            return method;
        }

        /// <summary>
        /// 解析属性
        /// </summary>
        public static PropertyInfo ResolveProperty(string typeName, string propertyName)
        {
            var cacheKey = $"{typeName}.{propertyName}";
            
            if (_propertyCache.TryGetValue(cacheKey, out var cachedProperty))
            {
                return cachedProperty;
            }

            PropertyInfo property = null;
            var typeContainer = ResolveType(typeName);
            
            if (typeContainer?.ClrType != null)
            {
                try
                {
                    property = typeContainer.ClrType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, $"解析属性时发生错误: {typeName}.{propertyName}");
                }
            }

            if (property != null)
            {
                _propertyCache[cacheKey] = property;
            }

            return property;
        }

        /// <summary>
        /// 解析字段
        /// </summary>
        public static FieldInfo ResolveField(string typeName, string fieldName)
        {
            var cacheKey = $"{typeName}.{fieldName}";
            
            if (_fieldCache.TryGetValue(cacheKey, out var cachedField))
            {
                return cachedField;
            }

            FieldInfo field = null;
            var typeContainer = ResolveType(typeName);
            
            if (typeContainer?.ClrType != null)
            {
                try
                {
                    field = typeContainer.ClrType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Debug(ex, $"解析字段时发生错误: {typeName}.{fieldName}");
                }
            }

            if (field != null)
            {
                _fieldCache[cacheKey] = field;
            }

            return field;
        }

        /// <summary>
        /// 获取IL2CPP类指针
        /// </summary>
        public static IntPtr GetClassPointer(string typeName)
        {
            if (_classPointerCache.TryGetValue(typeName, out var classPointer))
            {
                return classPointer;
            }

            var typeContainer = ResolveType(typeName);
            if (typeContainer != null)
            {
#if IL2CPP
                if (typeContainer.ClassPointer != IntPtr.Zero)
                {
                    _classPointerCache[typeName] = typeContainer.ClassPointer;
                    return typeContainer.ClassPointer;
                }
#else
                return IntPtr.Zero;
#endif
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public static void ClearCache()
        {
            lock (_lockObject)
            {
                _typeCache.Clear();
                _classPointerCache.Clear();
                _methodCache.Clear();
                _propertyCache.Clear();
                _fieldCache.Clear();
                
                XuaLogger.AutoTranslator.Info("IL2CPP类型解析器缓存已清除");
            }
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        public static string GetCacheStatistics()
        {
            return $"类型缓存: {_typeCache.Count}, 类指针缓存: {_classPointerCache.Count}, " +
                   $"方法缓存: {_methodCache.Count}, 属性缓存: {_propertyCache.Count}, 字段缓存: {_fieldCache.Count}";
        }
    }
}
