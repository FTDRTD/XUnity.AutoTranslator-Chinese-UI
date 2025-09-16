# GitHub Actions Workflows

这个目录包含了XUnity Auto Translator项目的自动化构建和发布工作流。

## 工作流文件

### build-and-release.yml
**主构建和发布工作流**

**触发条件：**
- 推送标签时自动触发（格式：`v*`，例如 `v5.4.5`）
- 手动触发（workflow_dispatch）

**构建的版本：**
- XUnity.AutoTranslator-BepInEx (net40)
- XUnity.AutoTranslator-BepInEx-IL2CPP (net6.0)
- XUnity.AutoTranslator-Developer (net35)
- XUnity.AutoTranslator-Developer-IL2CPP (net6.0)
- XUnity.AutoTranslator-IPA (net35)
- XUnity.AutoTranslator-MelonMod (net35)
- XUnity.AutoTranslator-MelonMod-IL2CPP (net6.0)
- XUnity.AutoTranslator-UnityInjector (net35)
- XUnity.AutoTranslator-ReiPatcher (net35)

**功能：**
- 自动构建所有插件版本
- 生成ZIP压缩包
- 自动创建GitHub Release并上传所有构建产物

**手动触发参数：**
- `version`: 发布版本号（不包含v前缀），默认值为 `5.4.5`

### build-font-assets.yml
**TMP字体资源包构建工作流**

**触发条件：**
- 当 `libs/TextMesh Pro/` 或 `tools/` 目录有变更时
- 手动触发（workflow_dispatch）

**功能：**
- 构建TextMesh Pro字体资源包
- 生成7z压缩包
- 自动创建发布版本

**手动触发参数：**
- `asset_version`: 资源包版本（例如 `2025-05-12`）

## 使用方法

### 自动发布
1. 推送一个标签到仓库：
   ```bash
   git tag v5.4.6
   git push origin v5.4.6
   ```
2. 这将自动触发构建和发布流程

### 手动发布
1. 进入GitHub仓库的Actions页面
2. 选择对应的workflow
3. 点击"Run workflow"按钮
4. 填写版本号参数
5. 确认运行

### 字体资源包发布
1. 进入GitHub仓库的Actions页面
2. 选择"Build Font Assets" workflow
3. 点击"Run workflow"
4. 填写资源包版本号
5. 确认运行

## 构建产物

工作流会生成以下ZIP文件：
- `XUnity.AutoTranslator-BepInEx-{version}.zip`
- `XUnity.AutoTranslator-BepInEx-IL2CPP-{version}.zip`
- `XUnity.AutoTranslator-Developer-{version}.zip`
- `XUnity.AutoTranslator-Developer-IL2CPP-{version}.zip`
- `XUnity.AutoTranslator-IPA-{version}.zip`
- `XUnity.AutoTranslator-MelonMod-{version}.zip`
- `XUnity.AutoTranslator-MelonMod-IL2CPP-{version}.zip`
- `XUnity.AutoTranslator-UnityInjector-{version}.zip`
- `XUnity.AutoTranslator-ReiPatcher-{version}.zip`
- `TMP_Font_AssetBundles_{version}.7z` (字体资源包)

## 注意事项

- 所有构建都在Windows环境中进行，以确保兼容性
- 使用.NET 6.0和8.0 SDK进行构建
- 版本号通过MSBuild属性管理，定义在 `Directory.Build.props` 中
- 构建过程会自动复制必要的依赖文件和目录结构
- 发布版本会自动生成release notes

## 故障排除

如果构建失败，请检查：
1. .NET SDK是否正确安装
2. 依赖项是否完整
3. 构建工具（如MSBuild）是否可用
4. 必要的库文件是否存在于 `libs/` 目录中