using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XUnity.Common.Logging;
using XUnity.Common.Compatibility;

namespace XUnity.ResourceRedirector
{
    /// <summary>
    /// 字体辅助类，提供Unity 2022+兼容的字体加载和处理
    /// </summary>
    public static class FontHelper
    {
        private static readonly Dictionary<string, UnityEngine.Font> _fontCache = new Dictionary<string, UnityEngine.Font>();
        private static readonly Dictionary<string, FontLoadContext> _loadingContexts = new Dictionary<string, FontLoadContext>();
        private static readonly object _lockObject = new object();

        /// <summary>
        /// 字体加载上下文
        /// </summary>
        public class FontLoadContext
        {
            public string FontPath { get; set; }
            public UnityEngine.Font Font { get; set; }
            public DateTime LoadTime { get; set; }
            public FontLoadMethod LoadMethod { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
            public Exception LoadError { get; set; }
            public bool IsSuccessful => Font != null && LoadError == null;
        }

        /// <summary>
        /// 字体加载方法枚举
        /// </summary>
        public enum FontLoadMethod
        {
            FromFile,
            FromMemory,
            FromAssetBundle,
            FromResources,
            SystemFont
        }

        /// <summary>
        /// 加载字体文件
        /// </summary>
        public static UnityEngine.Font LoadFontFromFile(string fontPath, bool useUnity2022API = true)
        {
            if (string.IsNullOrEmpty(fontPath) || !File.Exists(fontPath))
            {
                XuaLogger.ResourceRedirector.Warn($"字体文件不存在: {fontPath}");
                return null;
            }

            lock (_lockObject)
            {
                // 检查缓存
                if (_fontCache.TryGetValue(fontPath, out var cachedFont))
                {
                    XuaLogger.ResourceRedirector.Debug($"从缓存获取字体: {fontPath}");
                    return cachedFont;
                }

                try
                {
                    UnityEngine.Font font = null;
                    var context = new FontLoadContext
                    {
                        FontPath = fontPath,
                        LoadTime = DateTime.Now,
                        LoadMethod = FontLoadMethod.FromFile,
                        Metadata = new Dictionary<string, object>()
                    };

                    if (CompatibilityHelper.CompatibilityInfo.IsUnity2022OrHigher && useUnity2022API)
                    {
                        font = LoadFontFromFileUnity2022(fontPath);
                    }
                    else
                    {
                        font = LoadFontFromFileLegacy(fontPath);
                    }

                    context.Font = font;
                    context.LoadError = font == null ? new Exception("字体加载失败") : null;

                    if (font != null)
                    {
                        _fontCache[fontPath] = font;
                        _loadingContexts[fontPath] = context;
                        XuaLogger.ResourceRedirector.Info($"成功加载字体: {fontPath}");
                    }
                    else
                    {
                        XuaLogger.ResourceRedirector.Error($"字体加载失败: {fontPath}");
                    }

                    return font;
                }
                catch (Exception ex)
                {
                    XuaLogger.ResourceRedirector.Error(ex, $"加载字体时发生异常: {fontPath}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Unity 2022+的字体加载实现
        /// </summary>
        private static UnityEngine.Font LoadFontFromFileUnity2022(string fontPath)
        {
            try
            {
                // Unity 2022+可能有新的字体加载API
                // 这里使用标准API，但添加了额外的错误处理
                var font = Font.CreateDynamicFontFromOSFont(fontPath, 16);
                
                if (font == null)
                {
                    // 尝试其他加载方式
                    font = LoadFontFromFileLegacy(fontPath);
                }

                return font;
            }
            catch (Exception ex)
            {
                XuaLogger.ResourceRedirector.Error(ex, $"Unity 2022+字体加载失败: {fontPath}");
                return null;
            }
        }

        /// <summary>
        /// 传统字体加载实现
        /// </summary>
        private static UnityEngine.Font LoadFontFromFileLegacy(string fontPath)
        {
            try
            {
                // 读取字体文件数据
                var fontData = File.ReadAllBytes(fontPath);
                
                // 创建字体
                var font = Font.CreateDynamicFontFromOSFont(fontPath, 16);
                
                return font;
            }
            catch (Exception ex)
            {
                XuaLogger.ResourceRedirector.Error(ex, $"传统字体加载失败: {fontPath}");
                return null;
            }
        }

        /// <summary>
        /// 从内存加载字体
        /// </summary>
        public static UnityEngine.Font LoadFontFromMemory(byte[] fontData, string fontName = null)
        {
            if (fontData == null || fontData.Length == 0)
            {
                XuaLogger.ResourceRedirector.Warn("字体数据为空");
                return null;
            }

            try
            {
                // 创建临时文件
                var tempPath = Path.GetTempFileName();
                File.WriteAllBytes(tempPath, fontData);

                try
                {
                    var font = LoadFontFromFile(tempPath);
                    if (font != null && !string.IsNullOrEmpty(fontName))
                    {
                        font.name = fontName;
                    }
                    return font;
                }
                finally
                {
                    // 清理临时文件
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    {
                        // 忽略清理错误
                    }
                }
            }
            catch (Exception ex)
            {
                XuaLogger.ResourceRedirector.Error(ex, "从内存加载字体失败");
                return null;
            }
        }

        /// <summary>
        /// 从AssetBundle加载字体
        /// </summary>
        public static UnityEngine.Font LoadFontFromAssetBundle(AssetBundle bundle, string fontName)
        {
            if (bundle == null || string.IsNullOrEmpty(fontName))
            {
                XuaLogger.ResourceRedirector.Warn("AssetBundle或字体名称为空");
                return null;
            }

            try
            {
                var font = bundle.LoadAsset<Font>(fontName);
                if (font != null)
                {
                    XuaLogger.ResourceRedirector.Info($"从AssetBundle加载字体成功: {fontName}");
                }
                else
                {
                    XuaLogger.ResourceRedirector.Warn($"从AssetBundle加载字体失败: {fontName}");
                }
                return font;
            }
            catch (Exception ex)
            {
                XuaLogger.ResourceRedirector.Error(ex, $"从AssetBundle加载字体异常: {fontName}");
                return null;
            }
        }

        /// <summary>
        /// 从Resources加载字体
        /// </summary>
        public static UnityEngine.Font LoadFontFromResources(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                XuaLogger.ResourceRedirector.Warn("资源路径为空");
                return null;
            }

            try
            {
                var font = Resources.Load<Font>(resourcePath);
                if (font != null)
                {
                    XuaLogger.ResourceRedirector.Info($"从Resources加载字体成功: {resourcePath}");
                }
                else
                {
                    XuaLogger.ResourceRedirector.Warn($"从Resources加载字体失败: {resourcePath}");
                }
                return font;
            }
            catch (Exception ex)
            {
                XuaLogger.ResourceRedirector.Error(ex, $"从Resources加载字体异常: {resourcePath}");
                return null;
            }
        }

        /// <summary>
        /// 获取系统字体
        /// </summary>
        public static UnityEngine.Font GetSystemFont(string fontName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(fontName))
                {
                    fontName = "Arial"; // 默认字体
                }

                var font = Font.CreateDynamicFontFromOSFont(fontName, 16);
                if (font != null)
                {
                    XuaLogger.ResourceRedirector.Debug($"获取系统字体成功: {fontName}");
                }
                else
                {
                    XuaLogger.ResourceRedirector.Warn($"获取系统字体失败: {fontName}");
                }
                return font;
            }
            catch (Exception ex)
            {
                XuaLogger.ResourceRedirector.Error(ex, $"获取系统字体异常: {fontName}");
                return null;
            }
        }

        /// <summary>
        /// 检查字体是否已缓存
        /// </summary>
        public static bool IsFontCached(string fontPath)
        {
            lock (_lockObject)
            {
                return _fontCache.ContainsKey(fontPath);
            }
        }

        /// <summary>
        /// 获取缓存的字体
        /// </summary>
        public static UnityEngine.Font GetCachedFont(string fontPath)
        {
            lock (_lockObject)
            {
                return _fontCache.TryGetValue(fontPath, out var font) ? font : null;
            }
        }

        /// <summary>
        /// 移除字体缓存
        /// </summary>
        public static bool RemoveFontCache(string fontPath)
        {
            lock (_lockObject)
            {
                var removed = _fontCache.Remove(fontPath);
                _loadingContexts.Remove(fontPath);
                
                if (removed)
                {
                    XuaLogger.ResourceRedirector.Debug($"移除字体缓存: {fontPath}");
                }
                
                return removed;
            }
        }

        /// <summary>
        /// 清除所有字体缓存
        /// </summary>
        public static void ClearAllFontCache()
        {
            lock (_lockObject)
            {
                var count = _fontCache.Count;
                _fontCache.Clear();
                _loadingContexts.Clear();
                
                XuaLogger.ResourceRedirector.Info($"清除了 {count} 个字体缓存");
            }
        }

        /// <summary>
        /// 获取字体加载统计信息
        /// </summary>
        public static FontLoadStatistics GetFontLoadStatistics()
        {
            lock (_lockObject)
            {
                var statistics = new FontLoadStatistics
                {
                    CachedFontCount = _fontCache.Count,
                    LoadingContextCount = _loadingContexts.Count
                };

                foreach (var context in _loadingContexts.Values)
                {
                    if (context.IsSuccessful)
                    {
                        statistics.SuccessfulLoads++;
                    }
                    else
                    {
                        statistics.FailedLoads++;
                    }
                }

                return statistics;
            }
        }

        /// <summary>
        /// 获取字体兼容性建议
        /// </summary>
        public static string GetFontCompatibilityAdvice()
        {
            var advice = new List<string>();
            var compatibilityInfo = CompatibilityHelper.CompatibilityInfo;

            if (compatibilityInfo.IsIL2CPP)
            {
                advice.Add("IL2CPP环境下字体加载可能需要特殊处理");
                advice.Add("建议使用预加载的字体资源");
            }

            if (compatibilityInfo.IsUnity2022OrHigher)
            {
                advice.Add("Unity 2022+支持新的字体加载API");
                advice.Add("建议使用Font.CreateDynamicFontFromOSFont方法");
            }

            return advice.Count > 0 ? string.Join("; ", advice.ToArray()) : "字体加载兼容性良好";
        }

        /// <summary>
        /// 验证字体文件
        /// </summary>
        public static bool ValidateFontFile(string fontPath)
        {
            if (string.IsNullOrEmpty(fontPath) || !File.Exists(fontPath))
            {
                return false;
            }

            try
            {
                var fileInfo = new FileInfo(fontPath);
                var extension = fileInfo.Extension.ToLower();
                
                // 检查是否为支持的字体格式
                var supportedExtensions = new[] { ".ttf", ".otf", ".ttc" };
                return Array.Exists(supportedExtensions, ext => ext == extension);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取支持的字体格式
        /// </summary>
        public static string[] GetSupportedFontFormats()
        {
            return new[] { ".ttf", ".otf", ".ttc" };
        }
    }

    /// <summary>
    /// 字体加载统计信息
    /// </summary>
    public class FontLoadStatistics
    {
        public int CachedFontCount { get; set; }
        public int LoadingContextCount { get; set; }
        public int SuccessfulLoads { get; set; }
        public int FailedLoads { get; set; }
        
        public int TotalLoads => SuccessfulLoads + FailedLoads;
        public double SuccessRate => TotalLoads > 0 ? (double)SuccessfulLoads / TotalLoads : 0;
    }
}
