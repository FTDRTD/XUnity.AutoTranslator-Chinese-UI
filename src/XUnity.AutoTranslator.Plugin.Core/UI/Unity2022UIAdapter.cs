using System;
using System.Collections.Generic;
using UnityEngine;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;

#if UNITY_2022_1_OR_NEWER
using UnityEngine.UI;
using TMPro;
#endif

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
    /// <summary>
    /// Unity 2022+ UI组件适配器，提供统一的UI组件访问接口
    /// </summary>
    public static class Unity2022UIAdapter
    {
        private static readonly Dictionary<Type, UIComponentInfo> _componentCache = new Dictionary<Type, UIComponentInfo>();
        private static readonly object _lockObject = new object();

        /// <summary>
        /// 初始化UI适配器
        /// </summary>
        public static void Initialize()
        {
            lock (_lockObject)
            {
                if (_componentCache.Count > 0)
                {
                    return; // 已经初始化
                }

#if UNITY_2022_1_OR_NEWER
                // 注册Unity基础UI组件
                RegisterComponent<Text>(new UIComponentInfo
                {
                    ComponentType = typeof(Text),
                    TextProperty = "text",
                    FontProperty = "font",
                    IsTextComponent = true,
                    IsLegacyComponent = true
                });

                RegisterComponent<Image>(new UIComponentInfo
                {
                    ComponentType = typeof(Image),
                    TextProperty = null,
                    FontProperty = null,
                    IsTextComponent = false,
                    IsLegacyComponent = true
                });

                // 注册TextMeshPro组件
                RegisterComponent<TMP_Text>(new UIComponentInfo
                {
                    ComponentType = typeof(TMP_Text),
                    TextProperty = "text",
                    FontProperty = "font",
                    IsTextComponent = true,
                    IsLegacyComponent = false
                });

                RegisterComponent<TMP_InputField>(new UIComponentInfo
                {
                    ComponentType = typeof(TMP_InputField),
                    TextProperty = "text",
                    FontProperty = "fontAsset",
                    IsTextComponent = true,
                    IsLegacyComponent = false
                });
#endif

                XuaLogger.AutoTranslator.Info("Unity 2022+ UI适配器初始化完成");
            }
        }

        /// <summary>
        /// 注册UI组件信息
        /// </summary>
        private static void RegisterComponent<T>(UIComponentInfo info) where T : Component
        {
            _componentCache[typeof(T)] = info;
        }

        /// <summary>
        /// 获取UI组件信息
        /// </summary>
        public static UIComponentInfo GetComponentInfo(Type componentType)
        {
            Initialize();
            
            lock (_lockObject)
            {
                return _componentCache.TryGetValue(componentType, out var info) ? info : null;
            }
        }

        /// <summary>
        /// 检查是否为文本组件
        /// </summary>
        public static bool IsTextComponent(Component component)
        {
            if (component == null) return false;
            
            var info = GetComponentInfo(component.GetType());
            return info?.IsTextComponent ?? false;
        }

        /// <summary>
        /// 获取文本内容
        /// </summary>
        public static string GetText(Component component)
        {
            if (component == null) return null;
            
            var info = GetComponentInfo(component.GetType());
            if (info?.IsTextComponent != true) return null;

            try
            {
                var textProperty = component.GetType().GetProperty(info.TextProperty);
                return textProperty?.GetValue(component, null) as string;
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"获取文本内容失败: {component.GetType().Name}");
                return null;
            }
        }

        /// <summary>
        /// 设置文本内容
        /// </summary>
        public static bool SetText(Component component, string text)
        {
            if (component == null) return false;
            
            var info = GetComponentInfo(component.GetType());
            if (info?.IsTextComponent != true) return false;

            try
            {
                var textProperty = component.GetType().GetProperty(info.TextProperty);
                textProperty?.SetValue(component, text, null);
                return true;
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"设置文本内容失败: {component.GetType().Name}");
                return false;
            }
        }

        /// <summary>
        /// 获取字体
        /// </summary>
        public static UnityEngine.Object GetFont(Component component)
        {
            if (component == null) return null;
            
            var info = GetComponentInfo(component.GetType());
            if (info?.FontProperty == null) return null;

            try
            {
                var fontProperty = component.GetType().GetProperty(info.FontProperty);
                return fontProperty?.GetValue(component, null) as UnityEngine.Object;
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"获取字体失败: {component.GetType().Name}");
                return null;
            }
        }

        /// <summary>
        /// 设置字体
        /// </summary>
        public static bool SetFont(Component component, UnityEngine.Object font)
        {
            if (component == null || font == null) return false;
            
            var info = GetComponentInfo(component.GetType());
            if (info?.FontProperty == null) return false;

            try
            {
                var fontProperty = component.GetType().GetProperty(info.FontProperty);
                fontProperty?.SetValue(component, font, null);
                return true;
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Debug(ex, $"设置字体失败: {component.GetType().Name}");
                return false;
            }
        }

        /// <summary>
        /// 获取所有支持的文本组件类型
        /// </summary>
        public static Type[] GetSupportedTextComponentTypes()
        {
            Initialize();
            
            lock (_lockObject)
            {
                var textTypes = new List<Type>();
                foreach (var kvp in _componentCache)
                {
                    if (kvp.Value.IsTextComponent)
                    {
                        textTypes.Add(kvp.Key);
                    }
                }
                return textTypes.ToArray();
            }
        }

        /// <summary>
        /// 检查组件是否受支持
        /// </summary>
        public static bool IsComponentSupported(Type componentType)
        {
            Initialize();
            
            lock (_lockObject)
            {
                return _componentCache.ContainsKey(componentType);
            }
        }

        /// <summary>
        /// 获取适配器统计信息
        /// </summary>
        public static UIAdapterStatistics GetStatistics()
        {
            Initialize();
            
            lock (_lockObject)
            {
                var stats = new UIAdapterStatistics
                {
                    TotalComponents = _componentCache.Count,
                    TextComponents = 0,
                    LegacyComponents = 0,
                    ModernComponents = 0
                };

                foreach (var kvp in _componentCache)
                {
                    if (kvp.Value.IsTextComponent) stats.TextComponents++;
                    if (kvp.Value.IsLegacyComponent) stats.LegacyComponents++;
                    else stats.ModernComponents++;
                }

                return stats;
            }
        }

        /// <summary>
        /// 获取兼容性建议
        /// </summary>
        public static string GetCompatibilityAdvice()
        {
            var advice = new List<string>();
            var stats = GetStatistics();

            if (stats.ModernComponents == 0)
            {
                advice.Add("未检测到Unity 2022+现代UI组件");
                advice.Add("建议升级到Unity 2022+以获得更好的UI支持");
            }

            if (stats.LegacyComponents > stats.ModernComponents)
            {
                advice.Add("检测到大量传统UI组件");
                advice.Add("建议逐步迁移到TextMeshPro组件");
            }

            if (CompatibilityHelper.CompatibilityInfo.IsUnity2022OrHigher)
            {
                advice.Add("Unity 2022+支持新的UI功能");
                advice.Add("建议使用最新的UI组件API");
            }

            return advice.Count > 0 ? string.Join("; ", advice.ToArray()) : "UI组件兼容性良好";
        }
    }

    /// <summary>
    /// UI组件信息
    /// </summary>
    public class UIComponentInfo
    {
        public Type ComponentType { get; set; }
        public string TextProperty { get; set; }
        public string FontProperty { get; set; }
        public bool IsTextComponent { get; set; }
        public bool IsLegacyComponent { get; set; }
    }

    /// <summary>
    /// UI适配器统计信息
    /// </summary>
    public class UIAdapterStatistics
    {
        public int TotalComponents { get; set; }
        public int TextComponents { get; set; }
        public int LegacyComponents { get; set; }
        public int ModernComponents { get; set; }
    }
}
