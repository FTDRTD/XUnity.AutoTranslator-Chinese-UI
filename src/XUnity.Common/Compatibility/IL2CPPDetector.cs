using System;
using System.Reflection;
using XUnity.Common.Logging;

namespace XUnity.Common.Compatibility
{
    /// <summary>
    /// IL2CPP环境检测器，用于检测当前是否运行在IL2CPP环境下
    /// </summary>
    public static class IL2CPPDetector
    {
        private static bool? _isIL2CPP;
        private static bool _detectionCompleted = false;

        /// <summary>
        /// 检查当前是否运行在IL2CPP环境下
        /// </summary>
        public static bool IsIL2CPP
        {
            get
            {
                if (!_detectionCompleted)
                {
                    DetectIL2CPPEnvironment();
                }
                return _isIL2CPP ?? false;
            }
        }

        /// <summary>
        /// 检查当前是否运行在Mono环境下
        /// </summary>
        public static bool IsMono
        {
            get
            {
                return !IsIL2CPP;
            }
        }

        /// <summary>
        /// 检测IL2CPP环境
        /// </summary>
        private static void DetectIL2CPPEnvironment()
        {
            try
            {
                // 方法1: 检查IL2CPP相关的程序集
                if (CheckIL2CPPAssemblies())
                {
                    _isIL2CPP = true;
                    _detectionCompleted = true;
                    XuaLogger.AutoTranslator.Info("检测到IL2CPP环境 (通过程序集检测)");
                    return;
                }

                // 方法2: 检查IL2CPP相关的类型
                if (CheckIL2CPPTypes())
                {
                    _isIL2CPP = true;
                    _detectionCompleted = true;
                    XuaLogger.AutoTranslator.Info("检测到IL2CPP环境 (通过类型检测)");
                    return;
                }

                // 方法3: 检查运行时特性
                if (CheckRuntimeCharacteristics())
                {
                    _isIL2CPP = true;
                    _detectionCompleted = true;
                    XuaLogger.AutoTranslator.Info("检测到IL2CPP环境 (通过运行时特性检测)");
                    return;
                }

                // 方法4: 检查编译时定义
#if IL2CPP
                _isIL2CPP = true;
                _detectionCompleted = true;
                XuaLogger.AutoTranslator.Info("检测到IL2CPP环境 (通过编译时定义)");
                return;
#endif

                // 如果所有检测都失败，默认为Mono
                _isIL2CPP = false;
                _detectionCompleted = true;
                XuaLogger.AutoTranslator.Info("检测到Mono环境");
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Error(ex, "检测IL2CPP环境时发生错误");
                _isIL2CPP = false;
                _detectionCompleted = true;
            }
        }

        /// <summary>
        /// 检查IL2CPP相关的程序集
        /// </summary>
        private static bool CheckIL2CPPAssemblies()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                
                foreach (var assembly in assemblies)
                {
                    var assemblyName = assembly.GetName().Name;
                    
                    // 检查IL2CPP相关的程序集
                    if (assemblyName.Contains("Il2Cpp") || 
                        assemblyName.Contains("Il2CppInterop") ||
                        assemblyName.Contains("Il2Cppmscorlib"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, "检查IL2CPP程序集时发生错误");
            }
            
            return false;
        }

        /// <summary>
        /// 检查IL2CPP相关的类型
        /// </summary>
        private static bool CheckIL2CPPTypes()
        {
            try
            {
                // 检查Il2CppSystem命名空间下的类型
                var il2CppSystemType = Type.GetType("Il2CppSystem.Object");
                if (il2CppSystemType != null)
                {
                    return true;
                }

                // 检查Il2CppInterop命名空间下的类型
                var il2CppInteropType = Type.GetType("Il2CppInterop.Runtime.Il2CppObjectBase");
                if (il2CppInteropType != null)
                {
                    return true;
                }

                // 检查UnityEngine.Object是否为IL2CPP包装类型
                var unityObjectType = typeof(UnityEngine.Object);
                if (unityObjectType != null)
                {
                    var baseType = unityObjectType.BaseType;
                    if (baseType != null && baseType.Name.Contains("Il2Cpp"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, "检查IL2CPP类型时发生错误");
            }
            
            return false;
        }

        /// <summary>
        /// 检查运行时特性
        /// </summary>
        private static bool CheckRuntimeCharacteristics()
        {
            try
            {
                // 检查是否有IL2CPP相关的环境变量
                var il2CppEnv = Environment.GetEnvironmentVariable("IL2CPP");
                if (!string.IsNullOrEmpty(il2CppEnv))
                {
                    return true;
                }

                // 检查是否有IL2CPP相关的命令行参数
                var commandLine = Environment.CommandLine;
                if (!string.IsNullOrEmpty(commandLine) && commandLine.Contains("il2cpp"))
                {
                    return true;
                }

                // 检查当前进程名称是否包含IL2CPP相关信息
                var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                if (!string.IsNullOrEmpty(processName) && processName.ToLower().Contains("il2cpp"))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, "检查运行时特性时发生错误");
            }
            
            return false;
        }

        /// <summary>
        /// 获取IL2CPP环境信息
        /// </summary>
        public static string GetEnvironmentInfo()
        {
            var info = IsIL2CPP ? "IL2CPP" : "Mono";
            
            if (IsIL2CPP)
            {
                info += " (需要特殊处理)";
            }
            else
            {
                info += " (标准.NET环境)";
            }
            
            return info;
        }

        /// <summary>
        /// 检查特定功能是否在IL2CPP下可用
        /// </summary>
        public static bool IsFeatureAvailable(string featureName)
        {
            if (!IsIL2CPP)
            {
                return true; // Mono环境下所有功能都可用
            }

            // IL2CPP环境下的功能可用性检查
            switch (featureName.ToLower())
            {
                case "reflection":
                    return true; // IL2CPP支持反射，但有限制
                case "emission":
                    return false; // IL2CPP不支持动态代码生成
                case "unsafe":
                    return true; // IL2CPP支持unsafe代码
                case "p/invoke":
                    return true; // IL2CPP支持P/Invoke
                case "delegates":
                    return true; // IL2CPP支持委托
                default:
                    return true; // 默认可用
            }
        }

        /// <summary>
        /// 获取IL2CPP兼容性建议
        /// </summary>
        public static string GetCompatibilityAdvice()
        {
            if (!IsIL2CPP)
            {
                return "运行在Mono环境下，无需特殊处理";
            }

            var advice = "运行在IL2CPP环境下，建议：\n";
            advice += "- 避免使用动态代码生成\n";
            advice += "- 谨慎使用反射API\n";
            advice += "- 使用预编译的Hook方法\n";
            advice += "- 注意类型转换的安全性\n";
            
            return advice;
        }
    }
}
