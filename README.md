
## IL2CPP 支持
此插件提供完整的 IL2CPP 支持，包括 Unity 2021.3.19f1 及更高版本。

### 支持的功能
- ✅ 完整的文本翻译功能
- ✅ 所有内置翻译服务（Google、DeepL、Bing等）
- ✅ UI 界面翻译
- ✅ 下拉菜单完整显示（已修复显示不全问题）
- ✅ 翻译聚合器界面
- ✅ 资源重定向器
- ✅ 纹理翻译功能

### Unity 2022+ IL2CPP 特别说明
对于 Unity 2022 或更高版本，使用 IL2CPP 构建时请：
1. 使用 BepInEx 6.0 或更高版本
2. 下载 `XUnity.AutoTranslator-BepInEx-IL2CPP` 包
3. 确保安装了 IL2CPP 运行时

### 已知限制
- TextGetterCompatibilityMode 在 IL2CPP 中不可用
- 插件特定的翻译功能有限
- IMGUI 翻译功能有限

### 修复的历史问题
- **v6.1.6**: 修复 IL2CPP 下拉菜单只显示部分选项的问题
- **v6.1.6**: 优化发布包结构，BepInEx 包只包含必要的 BepInEx 文件夹
- **v6.1.6**: 改善 IL2CPP 环境下的 UI 稳定性