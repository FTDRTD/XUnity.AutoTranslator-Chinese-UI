# XUnity 自动翻译器
## 目录
- [介绍](#介绍)
- [插件框架](#插件框架)
- [安装](#安装)
- [按键映射](#按键映射)
- [翻译器](#翻译器)
- [文本框架](#文本框架)
- [配置](#配置)
- [IL2CPP 支持](#il2cpp-支持)
- [常见问题](#常见问题)
- [翻译模组](#翻译模组)
- [手动翻译](#手动翻译)
- [关于再分发](#关于再分发)
- [纹理翻译](#纹理翻译)
- [与自动翻译器集成](#与自动翻译器集成)
- [实现翻译器](#实现翻译器)
- [实现资源重定向器](#实现资源重定向器)


## 介绍
这是一款高级翻译插件，可用于自动翻译基于 Unity 的游戏，还提供手动翻译游戏所需的工具。

显然，它需要联网才能提供自动翻译功能，因此如果您对此不满，请不要使用。

如果您打算将此插件作为游戏翻译套件的一部分进行再分发，请阅读[本节](#关于再分发)和[手动翻译](#手动翻译)部分，以便了解插件的运行方式。


## 插件框架
该模组可在不依赖任何外部组件的情况下安装，也可作为以下插件管理器/模组加载器的插件安装：
- [BepInEx](https://github.com/bbepis/BepInEx)（推荐）
- [MelonLoader](https://melonwiki.xyz)
- [IPA](https://github.com/Eusth/IPA)
- UnityInjector

所有安装方法的说明如下。


## 安装
该插件可通过以下方式安装：

### 独立安装（ReiPatcher）
要求：无，本下载包已提供 ReiPatcher。
*非常重要的注意事项：使用此方法可通过两次简单点击在大多数 Unity 游戏中使插件正常工作。请注意，如果使用受支持的插件管理器，则应避免使用此安装方法，否则会导致问题。*
1. 阅读上面的“非常重要的注意事项”。
2. 从[发布页](../../releases)下载 XUnity.AutoTranslator-ReiPatcher-{版本号}.zip。
3. 直接解压到游戏目录，使“SetupReiPatcherAndAutoTranslator.exe”与其他可执行文件放在一起。
4. 运行“SetupReiPatcherAndAutoTranslator.exe”，这将正确设置 ReiPatcher。
5. 运行在现有可执行文件旁边创建的快捷方式“{游戏可执行文件名}（Patch and Run）.lnk”，这将修补并启动游戏。
6. 从现在开始，您可以直接从 {游戏可执行文件名}.exe 启动游戏。
7. 出于多种考虑，并非所有文本钩子都默认启用，因此如果您发现游戏或游戏的某些部分没有被正确翻译，不妨进入配置文件，启用一些已禁用的文本框架！配置文件会在游戏启动时创建。

文件结构应如下所示：
```
{游戏目录}/ReiPatcher/Patches/XUnity.AutoTranslator.Patcher.dll
{游戏目录}/ReiPatcher/ExIni.dll
{游戏目录}/ReiPatcher/Mono.Cecil.dll
{游戏目录}/ReiPatcher/Mono.Cecil.Inject.dll
{游戏目录}/ReiPatcher/Mono.Cecil.Mdb.dll
{游戏目录}/ReiPatcher/Mono.Cecil.Pdb.dll
{游戏目录}/ReiPatcher/Mono.Cecil.Rocks.dll
{游戏目录}/ReiPatcher/ReiPatcher.exe
{游戏目录}/{游戏可执行文件名}_Data/Managed/ReiPatcher.exe
{游戏目录}/{游戏可执行文件名}_Data/Managed/XUnity.Common.dll
{游戏目录}/{游戏可执行文件名}_Data/Managed/XUnity.ResourceRedirector.dll
{游戏目录}/{游戏可执行文件名}_Data/Managed/XUnity.AutoTranslator.Plugin.Core.dll
{游戏目录}/{游戏可执行文件名}_Data/Managed/XUnity.AutoTranslator.Plugin.ExtProtocol.dll
{游戏目录}/{游戏可执行文件名}_Data/Managed/MonoMod.RuntimeDetour.dll
{游戏目录}/{游戏可执行文件名}_Data/Managed/MonoMod.Utils.dll
{游戏目录}/{游戏可执行文件名}_Data/Managed/Mono.Cecil.dll
{游戏目录}/{游戏可执行文件名}_Data/Managed/0Harmony.dll
{游戏目录}/{游戏可执行文件名}_Data/Managed/ExIni.dll
{游戏目录}/{游戏可执行文件名}_Data/Managed/Translators/{翻译器}.dll
{游戏目录}/AutoTranslator/Translation/AnyTranslationFile.txt（这些文件将由插件自动生成！）
```

**注意**：放在 ReiPatcher 目录中的 `Mono.Cecil.dll` 文件与放在 Managed 目录中的文件不同。


### BepInEx 插件
要求：[BepInEx 插件管理器](https://github.com/BepInEx/BepInEx)（请先按照其安装说明操作！）。
1. 从[发布页](../../releases)下载 XUnity.AutoTranslator-BepInEx-{版本号}.zip。
2. 直接解压到游戏目录，使插件 DLL 文件放在 BepInEx 文件夹中。
3. 启动游戏。
4. 出于多种考虑，并非所有文本钩子都默认启用，因此如果您发现游戏或游戏的某些部分没有被正确翻译，不妨进入配置文件，启用一些已禁用的文本框架！配置文件会在游戏启动时创建。

文件结构应如下所示：
```
{游戏目录}/BepInEx/core/XUnity.Common.dll
{游戏目录}/BepInEx/plugins/XUnity.ResourceRedirector/XUnity.ResourceRedirector.dll
{游戏目录}/BepInEx/plugins/XUnity.ResourceRedirector/XUnity.ResourceRedirector.BepInEx.dll
{游戏目录}/BepInEx/plugins/XUnity.AutoTranslator/XUnity.AutoTranslator.Plugin.Core.dll
{游戏目录}/BepInEx/plugins/XUnity.AutoTranslator/XUnity.AutoTranslator.Plugin.BepInEx.dll
{游戏目录}/BepInEx/plugins/XUnity.AutoTranslator/XUnity.AutoTranslator.Plugin.ExtProtocol.dll
{游戏目录}/BepInEx/plugins/XUnity.AutoTranslator/ExIni.dll
{游戏目录}/BepInEx/plugins/XUnity.AutoTranslator/Translators/{翻译器}.dll
{游戏目录}/BepInEx/core/MonoMod.RuntimeDetour.dll
{游戏目录}/BepInEx/core/MonoMod.Utils.dll
{游戏目录}/BepInEx/core/Mono.Cecil.dll
{游戏目录}/BepInEx/Translation/AnyTranslationFile.txt（这些文件将由插件自动生成！）
```

#### BepInEx IL2CPP 插件
IL2CPP 版本的安装说明与标准版本相同，但您必须安装用于 IL2CPP 的 BepInEx 6（截至撰写本文时，仅可在[此处](https://builds.bepis.io/projects/bepinex_be)获取 bleeding edge 版本），并且必须使用本插件的 `BepInEx-IL2CPP` 包。

当前版本（5.4.0）基于 bleeding edge build 704 构建。


### MelonLoader 插件
要求：[Melon Loader](https://melonwiki.xyz)（请先按照其安装说明操作！）。
1. 从[发布页](../../releases)下载 XUnity.AutoTranslator-MelonMod-{版本号}.zip。
2. 直接解压到游戏目录，使插件 DLL 文件放在 Mods 和 UserLibs 文件夹中。
3. 启动游戏。
4. 出于多种考虑，并非所有文本钩子都默认启用，因此如果您发现游戏或游戏的某些部分没有被正确翻译，不妨进入配置文件，启用一些已禁用的文本框架！配置文件会在游戏启动时创建。

文件结构应如下所示：
```
{游戏目录}/Mods/XUnity.AutoTranslator.Plugin.MelonMod.dll
{游戏目录}/UserLibs/XUnity.Common.dll
{游戏目录}/UserLibs/XUnity.ResourceRedirector.dll
{游戏目录}/UserLibs/XUnity.AutoTranslator.Plugin.Core.dll
{游戏目录}/UserLibs/XUnity.AutoTranslator.Plugin.ExtProtocol.dll
{游戏目录}/UserLibs/ExIni.dll
{游戏目录}/UserLibs/Translators/{翻译器}.dll
{游戏目录}/AutoTranslator/Translation/AnyTranslationFile.txt（这些文件将由插件自动生成！）
```

当前版本（5.4.0）基于 v0.6.1 Open-Beta 构建。

#### MelonLoader IL2CPP 插件
IL2CPP 版本的安装说明与标准版本相同，但您必须使用本插件的 `MelonMod-IL2CPP` 包。


### IPA 插件
要求：[IPA 插件管理器](https://github.com/Eusth/IPA)（请先按照其安装说明操作！）。
1. 从[发布页](../../releases)下载 XUnity.AutoTranslator-IPA-{版本号}.zip。
2. 直接解压到游戏目录，使插件 DLL 文件放在 Plugins 文件夹中。
3. 启动游戏。
4. 出于多种考虑，并非所有文本钩子都默认启用，因此如果您发现游戏或游戏的某些部分没有被正确翻译，不妨进入配置文件，启用一些已禁用的文本框架！配置文件会在游戏启动时创建。

文件结构应如下所示：
```
{游戏目录}/Plugins/XUnity.Common.dll
{游戏目录}/Plugins/XUnity.ResourceRedirector.dll
{游戏目录}/Plugins/XUnity.AutoTranslator.Plugin.Core.dll
{游戏目录}/Plugins/XUnity.AutoTranslator.Plugin.IPA.dll
{游戏目录}/Plugins/XUnity.AutoTranslator.Plugin.ExtProtocol.dll
{游戏目录}/Plugins/MonoMod.RuntimeDetour.dll
{游戏目录}/Plugins/MonoMod.Utils.dll
{游戏目录}/Plugins/Mono.Cecil.dll
{游戏目录}/Plugins/0Harmony.dll
{游戏目录}/Plugins/ExIni.dll
{游戏目录}/Plugins/Translators/{翻译器}.dll
{游戏目录}/Plugins/Translation/AnyTranslationFile.txt（这些文件将由插件自动生成！）
```


### UnityInjector 插件
要求：UnityInjector（请先按照其安装说明操作！）。
1. 从[发布页](../../releases)下载 XUnity.AutoTranslator-UnityInjector-{版本号}.zip。
2. 直接解压到游戏目录，使插件 DLL 文件放在 UnityInjector 文件夹中。**这可能不是游戏根目录！**
3. 启动游戏。
4. 出于多种考虑，并非所有文本钩子都默认启用，因此如果您发现游戏或游戏的某些部分没有被正确翻译，不妨进入配置文件，启用一些已禁用的文本框架！配置文件会在游戏启动时创建。

文件结构应如下所示：
```
{游戏目录}/UnityInjector/XUnity.Common.dll
{游戏目录}/UnityInjector/XUnity.ResourceRedirector.dll
{游戏目录}/UnityInjector/XUnity.AutoTranslator.Plugin.Core.dll
{游戏目录}/UnityInjector/XUnity.AutoTranslator.Plugin.UnityInjector.dll
{游戏目录}/UnityInjector/XUnity.AutoTranslator.Plugin.ExtProtocol.dll
{游戏目录}/UnityInjector/0Harmony.dll
{游戏目录}/UnityInjector/Translators/{翻译器}.dll
{游戏目录}/UnityInjector/Config/Translation/AnyTranslationFile.txt（这些文件将由插件自动生成！）
```

**注意**：此安装方法不支持 MonoMod 钩子，因为 Sybaris 使用的是过时版本的 `Mono.Cecil.dll`。


## 按键映射
以下是按键映射：
- ALT + 0：切换 XUnity 自动翻译器界面（是数字 0，不是字母 O）。
- ALT + 1：切换翻译聚合器界面。
- ALT + T：在本插件提供的所有文本的翻译版本和未翻译版本之间切换。
- ALT + R：重新加载翻译文件。如果您实时更改了文本和纹理文件，此功能很有用。不保证对所有纹理都有效。
- ALT + U：手动挂钩。默认挂钩可能无法捕获所有文本。此功能将尝试手动查找。不会挂钩未启用的框架中的文本组件。
- ALT + F：如果配置了 OverrideFont，将在替代字体和默认字体之间切换。
- ALT + Q：如果插件因翻译端点连续出错而关闭，可重启插件。仅在您有理由相信问题已解决（例如更改了 VPN 端点等）时使用，否则它会再次关闭。

仅调试用按键：
- CTRL + ALT + NP9：模拟同步错误
- CTRL + ALT + NP8：模拟延迟一秒的异步错误
- CTRL + ALT + NP7：将加载的场景名称和 ID 打印到控制台
- CTRL + ALT + NP6：将整个游戏对象层次结构打印到文件 `hierarchy.txt`


## 翻译器
翻译通过翻译端点获取，翻译端点本质上是自动翻译器的插件。端点插件存储在 `Translators` 子文件夹中。

### 内置翻译器
以下是默认支持的翻译器列表：
- [GoogleTranslate](https://untrack.link/https://translate.google.com/)，基于谷歌在线翻译服务。不需要身份验证。
  - 无限制，但不稳定。
- [GoogleTranslateV2](https://untrack.link/https://translate.google.com/)，基于谷歌在线翻译服务。不需要身份验证。
  - 无限制，但不稳定。目前正在测试中。未来可能会取代原始版本，因为其官方翻译器网站已不再使用该 API。
- [GoogleTranslateCompat](https://untrack.link/https://translate.google.com/)，与上述相同，但请求在进程外处理，这在某些版本的 Unity/Mono 中是必需的。
  - 无限制，但不稳定。
- [GoogleTranslateLegitimate](https://untrack.link/https://cloud.google.com/translate/)，基于谷歌云翻译 API。需要 API 密钥。
  - 提供 1 年的试用期和 300 美元的信用额度。足够翻译 1500 万个字符。
- [BingTranslate](https://untrack.link/https://www.bing.com/translator)，基于必应在线翻译服务。不需要身份验证。
  - 无限制，但不稳定。
- [BingTranslateLegitimate](https://untrack.link/https://docs.microsoft.com/en-us/azure/cognitive-services/translator/translator-info-overview)，基于 Azure 文本翻译。需要 API 密钥。
  - 每月免费翻译 up 至 200 万个字符。
- [DeepLTranslate](https://untrack.link/https://www.deepl.com/translator)，基于 DeepL 在线翻译服务。不需要身份验证。
  - 无限制，但不稳定。翻译质量出色。
- [DeepLTranslateLegitimate](https://untrack.link/https://www.deepl.com/translator)，基于 DeepL 在线翻译服务。需要 API 密钥。
  - 每月 4.99 美元，当月翻译的字符每百万收费 20 美元。
  - 每月免费翻译 up 至 50 万个字符。
  - 目前，您必须订阅 DeepL API（面向开发者）。不支持 DeepL Pro（入门版、高级版和终极版）。
- [PapagoTranslate](https://untrack.link/https://papago.naver.com/)，基于 Naver Papago 在线翻译服务。不需要身份验证。
  - 无限制，但不稳定。
- [BaiduTranslate](https://untrack.link/https://fanyi.baidu.com/)，基于百度翻译服务。需要 AppId 和 AppSecret。
  - 注册后，每月前 5 万个字符免费（QPS=1），超出部分按 49 元/百万字符收费。如果您已通过免费身份验证，则每月前 100 万个字符免费（QPS=10），超出部分按 49 元/百万字符收费。单次请求最长为 6000 个字符。
- [YandexTranslate](https://untrack.link/https://tech.yandex.com/translate/)，基于 Yandex 翻译服务。需要 API 密钥。
  - 每天免费翻译 up 至 100 万个字符，但每月最多 1000 万个字符。
- [WatsonTranslate](https://untrack.link/https://cloud.ibm.com/apidocs/language-translator)，基于 IBM 的 Watson。需要 URL 和 API 密钥。
  - 每月免费翻译 up 至 100 万个字符。
- LecPowerTranslator15，基于 LEC 的 Power Translator。不需要身份验证，但需要安装该软件。
  - 无限制。
- ezTrans XP，基于 Changsinsoft 的日韩翻译器 ezTrans XP。不需要身份验证，但需要安装该软件和 [Ehnd](https://github.com/sokcuri/ehnd)。
  - 无限制。
- [LingoCloudTranslate](https://untrack.link/https://fanyi.caiyunapp.com/)，基于灵云在线翻译服务。仅支持中文与另外两种语言的互译：日语和英语。
  - 注册并通过免费认证后，每月前 100 万个字符免费，超出部分按 20 元/百万字符收费。官方测试令牌为 `3975l6lr5pcbvidl6jl2`，您可以在注册前试用。
- CustomTranslate。您也可以指定任何可作为翻译端点的自定义 HTTP URL（GET 请求）。它必须使用查询参数“from”“to”和“text”，并且仅返回包含结果的字符串（首先尝试不带 SSL 的 HTTP，因为 unity-mono 经常在 SSL 方面有问题）。
  - *注意：这是一个以开发者为中心的选项。您不能简单地指定“CustomTranslate”并期望它能与您在网上找到的任何翻译服务一起工作。请参见[常见问题](#常见问题)*
  - 配置示例：
    - Endpoint=CustomTranslate
    - [Custom]
    - Url=http://my-custom-translation-service.net/translate
  - 请求示例：GET http://my-custom-translation-service.net/translate?from=ja&to=en&text=こんにちは
  - 响应示例（仅正文）：Hello
  - 可与 CustomTranslate 一起使用的已知实现：
    - ezTrans：https://github.com/HelloKS/ezTransWeb

*注意：如果您使用任何不需要某种形式身份验证的在线翻译器，此插件可能会随时失效。*


### 第三方翻译器
从 3.0.0 版本开始，您也可以实现自己的翻译器。为此，请按照[此处](#实现翻译器)的说明进行操作。

以下是一些可与自动翻译器一起使用的第三方翻译插件：
- [SugoiOfflineTranslatorEndpoint](https://github.com/Vin-meido/XUnity-AutoTranslator-SugoiOfflineTranslatorEndpoint)，用于与 Sugoi 翻译器服务器配合使用。
  - 无限制。翻译质量出色。
- [LlmTranslators](https://github.com/joshfreitas1984/XUnity.AutoTranslate.LlmTranslators)，用于与 OpenAI 的 LLM 和 Ollama 模型配合使用。
  - OpenAI 需要 APIKey，按使用的令牌收费。本地托管的 Ollama 模型免费。
- [AutoChatGptTranslator](https://github.com/joshfreitas1984/XUnity.AutoChatGptTranslator)，用于 ChatGPT。已过时，请改用 LlmTranslators。
  - 需要 APIKey，按使用的令牌收费。
- [AutoLLMTranslator](https://github.com/NothingNullNull/XUnity.AutoLLMTranslator)，一个通用端点，支持许多不同的 LLM，包括 Ollama 模型。
  - 非常灵活，但需要高级手动配置。仅推荐给高级用户。

*注意：使用第三方插件需自行承担风险——它们在添加到列表时经过了检查，但可能会随时间变化。第三方插件可能会导致问题或存在安全问题。*


### 关于需要身份验证的翻译器
如果您决定使用需要身份验证的服务，*切勿分享您的密钥或密码*。如果不小心分享了，应立即撤销。

如果您想使用付费选项，请记住在付费前检查该插件是否支持您想要翻译的语言。此外，虽然该插件会尝试将发送到翻译端点的请求数量降至最低，但无法保证它会请求翻译的量，本仓库的作者/所有者不对因使用本插件而可能从您选择的翻译提供商处产生的任何费用负责。

插件如何尝试减少发送的请求数量在[此处](#防垃圾信息机制)概述。


### 防垃圾信息机制
该插件采用以下防垃圾信息机制：
1. 当它看到新文本时，总会等待一秒钟再将翻译请求加入队列，以检查该文本是否有变化。只有当文本在 1 秒内没有变化时，才会发送请求。
2. 在单次游戏会话中，它发送的请求绝不会超过 8000 个（每个请求最多 200 个字符（可配置））。
3. 一次绝不会发送多个请求（无并发！）。
4. 如果检测到排队的翻译数量不断增加（达到 4000 个），插件将关闭。
5. 如果服务连续五次请求都没有返回结果，插件将关闭。
6. 如果插件检测到游戏每帧都在排队翻译，插件将在 90 帧后关闭。
7. 如果插件检测到文本“滚动”显示，插件将关闭。这是通过检查所有排队等待翻译的请求来检测的（机制 1 通常会防止这种情况发生）。
8. 如果插件检测到每秒都在持续排队翻译，且持续时间超过 60 秒，插件将关闭。
9. 对于支持的语言，每个可翻译的行都必须通过符号检查，以检测该行是否包含源语言的字符。
10. 它绝不会尝试翻译已被视为其他内容的翻译的文本。
11. 所有排队的翻译都会被跟踪。如果两个不同的组件需要相同的翻译，并且都同时被加入翻译队列，只会发送一个请求。
12. 它采用了一个包含常用短语（仅日英互译）的手动翻译内部词典（总共约 2000 条），以避免为这些短语发送翻译请求。
13. 一些端点支持翻译批处理，因此发送的请求会少得多。这不会增加每会话的翻译总数（机制 2）。
14. 所有翻译结果都缓存在内存中并存储在磁盘上，以防止重复发送相同的翻译请求。
15. 由于其垃圾信息性质，任何来自 IMGUI 组件的文本中发现的数字都会被替换为模板（翻译后再替换回来），以防止与机制 6 相关的问题。
16. 插件将保持一个与翻译端点的 TCP 连接。如果 50 秒未使用，该连接将被正常关闭。


## 文本框架
支持以下文本框架：
- [UGUI](https://docs.unity3d.com/Manual/UISystem.html)
- [NGUI](https://assetstore.unity.com/packages/tools/gui/ngui-next-gen-ui-2413)
- [IMGUI](https://docs.unity3d.com/Manual/GUIScriptingGuide.html)（默认禁用）
- [TextMeshPro](http://digitalnativestudios.com/textmeshpro/docs/)
- [TextMesh](https://docs.unity3d.com/Manual/class-TextMesh.html)（默认禁用，通常文本漂浮在 3D 空间中）
- [FairyGUI for Unity](https://github.com/fairygui/FairyGUI-unity)
- [Utage（视觉小说游戏引擎）](http://madnesslabo.net/utage/?lang=en)


## 配置
默认配置文件如下：
```ini
[Service]
Endpoint=GoogleTranslate         ;要使用的端点。请参见[翻译器部分](#翻译器)了解有效值。
FallbackEndpoint=                ;如果主要端点对特定翻译失败，将自动 fallback 到的端点。
[General]
Language=en                      ;要翻译到的语言
FromLanguage=ja                  ;游戏的原始语言。某些端点支持“auto”，但通常不推荐
[Files]
Directory=Translation\{Lang}\Text                                   ;用于搜索缓存翻译文件的目录。可使用占位符：{GameExeName}、{Lang}
OutputFile=Translation\{Lang}\Text\_AutoGeneratedTranslations.txt   ;用于插入生成的翻译的文件。可使用占位符：{GameExeName}、{Lang}
SubstitutionFile=Translation\{Lang}\Text\_Substitutions.txt         ;包含翻译前应用的替换的文件。可使用占位符：{GameExeName}、{Lang}
PreprocessorsFile=Translation\{Lang}\Text\_Preprocessors.txt        ;包含发送文本到翻译器之前要应用的预处理器的文件。可使用占位符：{GameExeName}、{Lang}
PostprocessorsFile=Translation\{Lang}\Text\_Postprocessors.txt      ;包含从翻译器接收文本之后要应用的后处理器的文件。可使用占位符：{GameExeName}、{Lang}
[TextFrameworks]
EnableUGUI=True                  ;启用或禁用 UGUI 翻译
EnableNGUI=True                  ;启用或禁用 NGUI 翻译
EnableTextMeshPro=True           ;启用或禁用 TextMeshPro 翻译
EnableTextMesh=False             ;启用或禁用 TextMesh 翻译
EnableIMGUI=False                ;启用或禁用 IMGUI 翻译
[Behaviour]
MaxCharactersPerTranslation=200  ;每次翻译的最大字符数。最大为 2500。
IgnoreWhitespaceInDialogue=True  ;是否忽略对话键中的空白，包括换行
IgnoreWhitespaceInNGUI=True      ;是否忽略 NGUI 中的空白，包括换行
MinDialogueChars=20              ;被视为对话的文本长度
ForceSplitTextAfterCharacters=0  ;一旦翻译后的文本超过此字符数，就将文本拆分为多行
CopyToClipboard=False            ;是否将挂钩的文本复制到剪贴板
MaxClipboardCopyCharacters=450   ;一次挂钩到剪贴板的最大字符数
ClipboardDebounceTime=1.25       ;挂钩的文本到达剪贴板所需的秒数。最小值为 0.1
EnableUIResizing=True            ;插件是否应“尽力而为”地在翻译时调整 UI 组件大小
EnableBatching=True              ;指示是否应为由支持的端点启用翻译批处理
UseStaticTranslations=True       ;指示是否使用包含的静态翻译缓存中的翻译
OverrideFont=                    ;更新文本组件时用于文本的替代字体。注意：仅对 UGUI 有效
OverrideFontTextMeshPro=         ;考虑使用 FallbackFontTextMeshPro 代替。更新文本组件时用于文本的替代字体。注意：仅对 TextMeshPro 有效
FallbackFontTextMeshPro=         ;为 TextMeshPro 添加备用字体，以防特定字符不受支持。推荐使用此选项代替 OverrideFontTextMeshPro
ResizeUILineSpacingScale=        ;UI 调整大小时默认行间距应缩放的十进制值，例如：0.80。注意：仅对 UGUI 有效
ForceUIResizing=True             ;指示 UI 调整大小行为是否应应用于所有 UI 组件，无论它们是否被翻译。
IgnoreTextStartingWith=\u180e;   ;指示插件应忽略任何以特定字符开头的字符串。这是一个用“;”分隔的列表。
TextGetterCompatibilityMode=False ;指示是否启用“文本获取器兼容模式”。仅在游戏需要时启用。
GameLogTextPaths=                ;指示游戏用作“日志组件”的游戏对象的特定路径，游戏会不断向其中追加或前置文本。设置需要专业知识。这是一个用“;”分隔的列表。
RomajiPostProcessing=ReplaceMacronWithCircumflex;RemoveApostrophes;ReplaceHtmlEntities ;指示对“翻译后的”罗马音文本执行的后处理类型。在某些游戏中这可能很重要，因为所用字体不支持各种变音符号。这是一个用“;”分隔的列表。可能的值：["RemoveAllDiacritics", "ReplaceMacronWithCircumflex", "RemoveApostrophes", "ReplaceHtmlEntities"]
TranslationPostProcessing=ReplaceMacronWithCircumflex;ReplaceHtmlEntities ;指示对翻译后的文本（非罗马音）执行的后处理类型。可能的值：["RemoveAllDiacritics", "ReplaceMacronWithCircumflex", "RemoveApostrophes", "ReplaceWideCharacters", "ReplaceHtmlEntities"]
RegexPostProcessing=None         ;指示对正则表达式的捕获组执行的后处理类型。可能的值：["RemoveAllDiacritics", "ReplaceMacronWithCircumflex", "RemoveApostrophes", "ReplaceWideCharacters", "ReplaceHtmlEntities"]
CacheRegexLookups=False          ;指示正则表达式查找结果是否应输出到指定的 OutputFile
CacheWhitespaceDifferences=False ;指示空白差异是否应输出到指定的 OutputFile
CacheRegexPatternResults=False   ;指示正则表达式拆分的翻译的完整结果是否应输出到指定的 OutputFile
GenerateStaticSubstitutionTranslations=False ;指示使用替换时，插件是否应生成不带变量的翻译
GeneratePartialTranslations=False ;指示插件是否应生成部分翻译，以支持文本“滚动显示”时的翻译
EnableTranslationScoping=False   ;指示插件应解析“TARC”指令并基于这些指令确定翻译范围
EnableSilentMode=False           ;指示插件不应输出与翻译相关的成功消息
BlacklistedIMGUIPlugins=         ;如果 IMGUI 窗口的程序集/类/方法名称包含此列表中的任何字符串（不区分大小写），则该 UI 将不被翻译。需要 MonoMod 钩子。这是一个用“;”分隔的列表
OutputUntranslatableText=False   ;指示插件是否应将被视为不可翻译的文本输出到指定的 OutputFile
IgnoreVirtualTextSetterCallingRules=False; 指示在尝试设置文本组件的文本时，是否忽略虚拟方法调用规则。在某些情况下可能有助于设置顽固组件的文本
MaxTextParserRecursion=1         ;指示解析文本以使其能分不同部分翻译时允许的递归级别。这可在高级场景中与拆分器正则表达式一起使用。默认值 1 本质上意味着禁用递归。
HtmlEntityPreprocessing=True     ;将在发送翻译前预处理和解码 html 实体。某些翻译器在收到 html 实体时会失败。
HandleRichText=True              ;将启用富文本（带标记的文本）的自动处理
PersistRichTextMode=Final        ;指示解析的富文本应如何持久化。“Fragment”表示零碎地存储文本，“Final”表示存储整个翻译后的字符串（不支持替换！）
EnableTranslationHelper=False    ;指示是否启用与翻译器相关的有用日志消息。在基于重定向资源进行翻译时可能有用
ForceMonoModHooks=False          ;指示插件必须使用 MonoMod 钩子而不是 harmony 钩子
InitializeHarmonyDetourBridge=False ;指示插件应初始化 harmony detour 桥，这允许 harmony 钩子在不存在 System.Reflection.Emit 的环境中工作（通常此类设置由插件管理器处理，因此使用插件管理器时不要使用）
RedirectedResourceDetectionStrategy=AppendMongolianVowelSeparatorAndRemoveAll ;指示插件是否以及如何尝试识别重定向的资源，以防止双重翻译。可能的值：["None", "AppendMongolianVowelSeparator", "AppendMongolianVowelSeparatorAndRemoveAppended", "AppendMongolianVowelSeparatorAndRemoveAll"]
OutputTooLongText=False          ;指示插件是否应输出超过“MaxCharactersPerTranslation”的文本而不翻译它
[Texture]
TextureDirectory=Translation\{Lang}\Texture ;用于转储纹理以及加载图像的目录根。可使用占位符：{GameExeName}、{Lang}
EnableTextureTranslation=False   ;指示插件是否将尝试用 TextureDirectory 目录中的图像替换游戏中的图像
EnableTextureDumping=False       ;指示插件是否将其能够替换的纹理转储到 TextureDirectory。对性能有显著影响
EnableTextureToggling=False      ;指示使用 ALT+T 热键切换翻译时是否也会影响纹理。不保证对所有纹理都有效。对性能有显著影响
EnableTextureScanOnSceneLoad=False ;指示插件是否应在场景加载时扫描纹理。这使插件能够找到并（可能）替换更多纹理
EnableSpriteRendererHooking=False ;指示插件是否应尝试挂钩 SpriteRenderer。这是一个单独的选项，因为实际上无法正确挂钩 SpriteRenderer，所实现的解决方法在某些情况下可能会对性能产生理论上的影响
LoadUnmodifiedTextures=False     ;指示是否应加载未修改的纹理。修改是根据文件名中的哈希确定的。仅在调试时启用此选项，因为它可能会导致异常情况
TextureHashGenerationStrategy=FromImageName ;指示模组如何通过哈希识别图片。可能的值：["FromImageName", "FromImageData", "FromImageNameAndScene"]
DuplicateTextureNames=           ;指示游戏中重复的特定纹理名称。列表用“;”分隔。
DetectDuplicateTextureNames=False;指示插件是否应检测重复的纹理名称。
EnableLegacyTextureLoading=False ;指示插件应使用不同的策略加载图像，这可能与旧版游戏引擎相关
CacheTexturesInMemory=True       ;指示所有加载的纹理都应保存在内存中以获得最佳性能。禁用可减少内存使用
[ResourceRedirector]
PreferredStoragePath=Translation\{Lang}\RedirectedResources ;指示与自动翻译器相关的重定向资源的首选存储位置。可使用占位符：{GameExeName}、{Lang}
EnableTextAssetRedirector=False  ;指示是否应重定向 TextAssets
LogAllLoadedResources=False      ;指示插件是否应向控制台记录所有加载的资源。有助于确定可以挂钩的内容
EnableDumping=False              ;指示是否应转储找到的可翻译资源
CacheMetadataForAllFiles=True    ;当文件位于 PreferredStoragePath 中的 ZIP 文件中时，这些文件会在内存中建立索引，以避免加载时执行文件检查 IO。启用此选项也会对物理文件执行相同操作
[Http]
UserAgent=                       ;覆盖需要用户代理的 API 所使用的用户代理
DisableCertificateValidation=False ;指示是否应禁用 .NET API 的证书验证
[TranslationAggregator]
Width=400                        ;翻译聚合器窗口的总宽度。
Height=100                       ;翻译聚合器窗口的（每个翻译器的）宽度。
EnabledTranslators=              ;在翻译聚合器窗口中已启用的翻译端点的 ID。列表用“;”分隔。
[Google]
ServiceUrl=                      ;可选，可用于将谷歌 API 请求定向到不同的 URL。可用于规避 GFWoC
[GoogleLegitimate]
GoogleAPIKey=                    ;可选，如果配置了 GoogleTranslateLegitimate，则需要
[BingLegitimate]
OcpApimSubscriptionKey=          ;可选，如果配置了 BingTranslateLegitimate，则需要
[Baidu]
BaiduAppId=                      ;可选，如果配置了 BaiduTranslate，则需要
BaiduAppSecret=                  ;可选，如果配置了 BaiduTranslate，则需要
[Yandex]
YandexAPIKey=                    ;可选，如果配置了 YandexTranslate，则需要
[Watson]
Url=                             ;可选，如果配置了 WatsonTranslate，则需要
Key=                             ;可选，如果配置了 WatsonTranslate，则需要
[DeepL]
MinDelay=2                       ;可选，用于限制 DeepL 的速率
MaxDelay=7                       ;可选，用于限制 DeepL 的速率
[DeepLLegitimate]
ApiKey=                          ;可选，如果配置了 DeepLLegitimate，则需要
Free=False                       ;可选，如果配置了 DeepLLegitimate，则需要
[Custom]
Url=                             ;可选，如果配置了 CustomTranslated，则需要
[LecPowerTranslator15]
InstallationPath=                ;可选，如果配置了 LecPowerTranslator15，则需要
[LingoCloud]
LingoCloudToken=                 ;可选，如果配置了 LingoCloudTranslate，则需要
[Debug]
EnableConsole=False              ;启用控制台。如果其他插件（管理器）处理此功能，则不要启用
EnableLog=False                  ;启用额外的调试日志
[Migrations]
Enable=True                      ;用于启用此配置文件的自动迁移
Tag=4.15.0                        ;表示上次执行此插件的版本的标记。请勿编辑
```


### 行为配置说明
#### 空白处理
本节介绍对翻译前后的空白处理有影响的配置参数。**这些设置都不会对放置在自动生成的翻译文件中的“未翻译文本”产生影响。**

在自动翻译中，适当的空白处理确实会决定翻译的成败。控制空白处理的参数有：
- `IgnoreWhitespaceInDialogue`
- `IgnoreWhitespaceInNGUI`
- `MinDialogueChars`
- `ForceSplitTextAfterCharacters`

插件首先确定是否应执行特殊的空白删除操作。它根据参数 `IgnoreWhitespaceInDialogue`、`IgnoreWhitespaceInNGUI` 和 `MinDialogueChars` 来确定是否执行此操作：
- `IgnoreWhitespaceInDialogue`：如果文本长于 `MinDialogueChars`，则删除空白。
- `IgnoreWhitespaceInNGUI`：如果文本来自 NGUI 组件，则删除空白。

文本由配置的服务翻译后，`ForceSplitTextAfterCharacters` 用于确定插件是否应在特定字符数后将结果强制分为多行。

这种处理方式会决定翻译的成败，这实际上取决于在将源文本发送到端点之前是否从源文本中删除空白。大多数端点（如谷歌翻译）会分别考虑多行文本，这通常会导致如果包含不必要的换行，翻译结果会很糟糕。


#### 文本预处理/后处理
虽然适当的空白处理对确保更好的翻译大有帮助，但这并不总是足够的。

`PreprocessorsFile` 允许定义在将文本发送到翻译器之前修改文本的条目。

`PostprocessorsFile` 允许定义在从翻译器接收翻译后的文本之后修改文本的条目。


#### UI 调整大小
通常，对文本组件执行翻译时，结果文本比原始文本长。这通常意味着文本组件中没有足够的空间容纳结果。本节介绍通过更改文本组件的重要参数来解决此问题的方法。

默认情况下，插件会尝试一些基本的自动调整大小行为，这些行为由以下参数控制：`EnableUIResizing`、`ResizeUILineSpacingScale`、`ForceUIResizing`、`OverrideFont` 和 `OverrideFontTextMeshPro`。
- `EnableUIResizing`：翻译时调整组件大小。
- `ForceUIResizing`：始终调整所有组件的大小。
- `ResizeUILineSpacingScale`：更改调整大小的组件的行间距。仅适用于 UGUI。
- `OverrideFont`：无论 `EnableUIResizing` 和 `ForceUIResizing` 如何，更改所有文本组件的字体。仅适用于 UGUI。
- `OverrideFontTextMeshPro`：考虑使用 `FallbackFontTextMeshPro` 代替。无论 `EnableUIResizing` 和 `ForceUIResizing` 如何，更改所有文本组件的字体。仅适用于 TextMeshPro。此选项能够以两种不同方式加载字体。如果指定的字符串指示游戏文件夹中的路径，则将尝试将该文件作为资源包加载（需要 Unity 2018 或更高版本（或者专门为目标游戏构建的自定义资源包））。否则，将尝试通过 Resources API 加载它。TextMeshPro 通常分发的默认资源有：`Fonts & Materials/LiberationSans SDF` 或 `Fonts & Materials/ARIAL SDF`。
- `FallbackFontTextMeshPro`：添加 TextMesh Pro 可使用的备用字体，以防特定字符不受支持。

关于更改 TextMeshPro 的字体的额外说明：您可以在发布选项卡中下载一些为 Unity 2018 和 2019 预构建的资源包，但目前它们的测试并不充分。如果您想尝试，只需下载 .zip 文件夹并将其中一个字体资源放入游戏文件夹。然后通过在配置文件的 `OverrideFontTextMeshPro` 中写入文件名来进行配置。

UI 组件的调整大小并不是指更改其尺寸，而是指组件处理溢出的方式。插件更改溢出参数，使文本更有可能被显示。

配置 `EnableUIResizing` 和 `ForceUIResizing` 还控制是否启用手动 UI 调整大小行为。有关更多信息，请参见[本节](#ui-字体调整大小)。


#### 减少翻译请求
以下旨在减少发送到翻译端点的请求数量：
- `EnableBatching`：将多个翻译请求批处理为一个（由支持的端点）。
- `UseStaticTranslations`：启用使用各种英日术语的内部查找字典。
- `MaxCharactersPerTranslation`：指定要翻译的文本的最大长度。任何长于此的文本都会被插件忽略。不能大于 1000。**切勿将此值大于 400 的模组再分发**


#### 罗马音“翻译”
输出 `Language` 的可能值之一是“romaji”（罗马音）。如果您选择此作为语言，您会发现游戏通常难以显示翻译，因为字体不理解所使用的特殊字符，例如[长音符号](https://en.wikipedia.org/wiki/Macron_(diacritic))。

为解决此问题，当选择“romaji”作为 `Language` 时，可以对翻译应用后处理。这通过选项 `RomajiPostProcessing` 完成。此选项是一个用“;”分隔的值列表：
- `RemoveAllDiacritics`：从翻译后的文本中删除所有变音符号
- `ReplaceMacronWithCircumflex`：将长音符号替换为 circumflex（音调符号）。
- `RemoveApostrophes`：一些翻译器可能会在“n”字符后包含撇号。应用此选项会删除这些撇号。
- `ReplaceWideCharacters`：将全角日语字符替换为标准 ASCII 字符
- `ReplaceHtmlEntities`：将所有 html 实体替换为其未转义的字符

这种后处理也适用于正常翻译，但使用选项 `TranslationPostProcessing`，它可以使用相同的值。


#### MonoMod 钩子
MonoMod 钩子在运行时创建，但不通过 Harmony 依赖项。Harmony 有两个主要问题，这些钩子试图解决：
- Harmony 不能挂钩没有主体的方法。
- Harmony 不能挂钩 `netstandard2.0` API 表面下的方法，而 Unity 的更高版本可以在此 API 表面下构建。

MonoMod 解决了这两个问题。为了使用 MonoMod 钩子，库 `MonoMod.RuntimeDetours.dll`、`MonoMod.Utils.dll` 和 `Mono.Cecil.dll` 必须可供插件使用。这些是可选依赖项。

这些仅在以下包中可用：
- `XUnity.AutoTranslator-BepInEx-{版本号}.zip`（因为所有依赖项都随 BepInEx 5.x 分发）
- `XUnity.AutoTranslator-IPA-{版本号}.zip`（因为所有依赖项都包含在包中）
- `XUnity.AutoTranslator-ReiPatcher-{版本号}.zip`（因为所有依赖项都包含在包中）

它们不分布在 BepInEx 4.x 中，因为各种游戏的模组包中可能存在冲突。

以下配置控制 MonoMod 钩子：
- `ForceMonoModHooks`：强制插件使用 MonoMod 钩子而不是 Harmony 钩子。

如果不强制使用 MonoMod 钩子，则仅在可用且由于上述两个原因之一无法通过 Harmony 挂钩给定方法时才使用它们。


#### 其他选项
- `TextGetterCompatibilityMode`：此模式会让游戏认为显示的文本未被翻译。如果游戏使用显示给用户的文本来确定执行什么逻辑，则需要此模式。如果您切换翻译关闭（热键：ALT+T）时功能正常，您可以轻松确定是否需要此模式。
- `IgnoreTextStartingWith`：禁用对此“;”分隔设置中值开头的任何文本的翻译。[默认值](https://www.charbase.com/180e-unicode-mongolian-vowel-separator)是一个不占空间的不可见字符。
- `CopyToClipboard`：将待翻译文本复制到剪贴板，以支持翻译聚合器等工具。
- `ClipboardDebounceTime`：挂钩文本与复制到剪贴板之间的延迟。这是为了避免剪贴板被垃圾信息塞满。如果在此期间出现多个文本，它们将被连接起来。
- `EnableSilentMode`：指示插件不应输出与翻译相关的成功消息。
- `BlacklistedIMGUIPlugins`：如果 IMGUI 窗口的程序集/类/方法名称包含此列表中的任何字符串（不区分大小写），则该 UI 将不被翻译。需要 MonoMod 钩子。这是一个用“;”分隔的列表。
- `OutputUntranslatableText`：指示插件是否应将被视为不可翻译的文本输出到指定的 OutputFile。启用此选项可能还会向 `OutputFile` 输出大量垃圾内容，在可能再分发之前应删除这些内容。**切勿启用此选项再分发模组。**
- `IgnoreVirtualTextSetterCallingRules`：指示在尝试设置文本组件的文本时，是否忽略虚拟方法调用规则。在某些情况下可能有助于设置顽固组件的文本。
- `RedirectedResourceDetectionStrategy`：指示插件是否以及如何尝试识别重定向的资源，以防止双重翻译。可能的值：["None", "AppendMongolianVowelSeparator", "AppendMongolianVowelSeparatorAndRemoveAppended", "AppendMongolianVowelSeparatorAndRemoveAll"]
- `OutputTooLongText`：指示插件是否应输出超过“MaxCharactersPerTranslation”的文本而不翻译它


## IL2CPP 支持
虽然此插件提供一定程度的 IL2CPP 支持，但远非完整。可以观察到以下差异/缺少的功能：
- 文本挂钩能力较差
- 不支持 TextGetterCompatibilityMode
- 尚不支持插件特定的翻译（尚未）
- 不支持 IMGUI 翻译（尚未）
- 许多其他功能完全未经证实


## 常见问题
> **问：如何禁用自动翻译？**  
答：按 ALT+0 时选择空端点，或将配置参数 `Endpoint=` 设置为空。

> **问：如何完全禁用插件？**  
答：可以通过删除“{GameDirectory}\BepInEx\plugins”目录中的“XUnity.AutoTranslator”目录来实现。避免删除“XUnity.ResourceRedirector”目录，因为其他插件可能依赖它。

> **问：应用翻译时游戏停止工作。**  
答：尝试设置以下配置参数 `TextGetterCompatibilityMode=True`。

> **问：此插件可以翻译其他插件/模组吗？**  
答：很可能可以，参见[此处](#翻译模组)。

> **问：如何使用 CustomTranslate？**  
答：如果您要问这个问题，您可能无法使用。CustomTranslate 是为翻译服务的开发者设计的。他们能够公开符合 CustomTranslate API 规范的 API，而无需在此插件中实现自定义 ITranslateEndpoint。

> **问：请提供对翻译服务 X 的支持。**  
答：目前，不太可能再支持不需要某种形式身份验证的服务。但请注意，可以独立于此插件实现自定义翻译器。而且实现所需的代码非常少。


## 翻译模组
其他模组的 UI 通常通过 IMGUI 实现。如上所示，默认情况下此功能是禁用的。通过将“EnableIMGUI”值更改为“True”，它将开始翻译 IMGUI，这可能意味着其他模组的 UI 将被翻译。

也可以提供插件特定的翻译。参见下一节。


## 手动翻译
使用此插件时，您始终可以转到文件 `Translation\{Lang}\Text\_AutoGeneratedTranslations.txt`（OutputFile）来编辑任何自动生成的翻译，下次运行游戏时它们将显示出来。或者您可以按（ALT+R）立即重新加载翻译。

还值得注意的是，此插件将读取 `Translation`（Directory）中的所有文本文件（*.txt），因此如果您想提供手动翻译，只需从 `Translation\_AutoGeneratedTranslations.{lang}.txt`（OutputFile）中剪切文本并将它们放在新的文本文件中，以用手动翻译替换它们。这些文本文件也可以放在标准的 .zip 存档中。

在这种情况下，`Translation\{Lang}\Text\_AutoGeneratedTranslations.txt`（OutputFile）在读取翻译时优先级始终最低。因此，如果同一翻译出现在两个地方，将不会使用来自（OutputFile）的翻译。

在某些 ADV 引擎中，文本会缓慢“滚动”显示。实现此功能的技术各不相同，在某些情况下，如果您希望翻译后的文本而不是未翻译的文本滚动显示，可能需要设置 `GeneratePartialTranslations=True`。除非游戏需要，否则不应启用此选项。


### 插件特定的手动翻译
通常，您可能希望为其他未自然翻译的插件提供翻译。显然，如前一节所述，此插件也可以做到这一点。但是，如果您想提供特定于该插件的翻译，因为这样的翻译可能与其他插件/通用翻译冲突，该怎么办？

要添加插件特定的翻译，只需在文本翻译 `Directory` 中创建一个 `Plugins` 目录。在该目录中，您可以为每个要提供插件特定翻译的插件创建一个新目录。目录名称应与 dll 名称相同（不带扩展名 .dll）。

在该目录中，您可以像往常一样创建翻译文件。此外，您可以在这些文件中添加以下指令：
```
#enable fallback
```
这将允许插件特定的翻译 fallback 到插件提供的通用/自动翻译。此指令放在哪个翻译文件中并不重要，只需添加一次。

作为插件作者，也可以将这些翻译文件嵌入到您的插件中，并通过以下 API 通过代码注册它们：
```csharp
/// <summary>
/// 用于操作已由插件加载的翻译的入口点。
///
/// 应在插件初始化期间调用此接口上的方法。最好在 Start 回调期间。
/// </summary>
public static class TranslationRegistry
    /// <summary>
    /// 获取翻译注册表实例。
    /// </summary>
    public static ITranslationRegistry Default { get; }
/// <summary>
/// 用于操作已由插件加载的翻译的接口。
/// </summary>
public interface ITranslationRegistry
    /// <summary>
    /// 注册并加载指定的翻译包。
    /// </summary>
    /// <param name="assembly">行为应应用于的程序集。</param>
    /// <param name="package">包含翻译的包。</param>
    void RegisterPluginSpecificTranslations( Assembly assembly, StreamTranslationPackage package );
    /// <summary>
    /// 注册并加载指定的翻译包。
    /// </summary>
    /// <param name="assembly">行为应应用于的程序集。</param>
    /// <param name="package">包含翻译的包。</param>
    void RegisterPluginSpecificTranslations( Assembly assembly, KeyValuePairTranslationPackage package );
    /// <summary>
    /// 允许插件特定的翻译 fallback 到通用翻译。
    /// </summary>
    /// <param name="assembly">行为应应用于的程序集。</param>
    void EnablePluginTranslationFallback( Assembly assembly );
```


### 替换
也可以添加在创建翻译之前应用于找到的文本的替换。这由 `SubstitutionFile` 控制，该文件使用与普通翻译文本文件相同的格式，但不支持正则表达式等。

这对于替换经常被错误翻译的名称等很有用。

使用替换时，找到的匹配项将在生成的翻译中参数化，如下所示：
```
私は{{A}}=I am {{A}}
```
或者，如果使用配置 `GenerateStaticSubstitutionTranslations=True`，翻译将不会参数化。

创建手动翻译时，应像使用正则表达式一样谨慎使用此文件，因为它可能会影响性能。

*注意：如果要翻译的文本包含富文本，则目前无法对其进行参数化。*


### 正则表达式用法
文本翻译文件也支持正则表达式。请始终记住谨慎使用正则表达式，并限定其范围以避免性能问题。

正则表达式可以通过两种不同的方式应用于翻译。以下两节描述了这两种方式：


#### 标准正则表达式翻译
标准正则表达式翻译只是直接应用于可翻译文本的正则表达式（如果找不到直接查找）。
```
r:"^シンプルリング ([0-9]+)$"=Simple Ring $1
```
这些通过未翻译文本以 'r:' 开头来识别。


#### 拆分器正则表达式
有时游戏喜欢在屏幕上显示文本之前将文本组合起来。这意味着有时很难知道要添加到翻译文件中的文本是什么，因为它以多种不同的方式出现。

本节探讨通过在尝试查找指定文本之前应用正则表达式将待翻译文本拆分为单个部分来解决此问题的方法。

例如，假设某个配件（Simple Ring）将用以下行翻译 `シンプルリング=Simple Ring`。现在假设它在游戏中的多个文本框中出现，如 `01 シンプルリング` 和 `02 シンプルリング`。在翻译文件中提供标准正则表达式来处理此问题是行不通的，因为您需要为每个配件使用一个正则表达式，而且这一点也不高效。

但是，如果我们在尝试查找之前拆分翻译，我们将只需要在文件中有一个简单的翻译，如下所示：`シンプルリング=Simple Ring`。

只需在翻译文件中放置以下正则表达式：
```
sr:"^([0-9]{2}) ([\S\s]+)$"=$1 $2
```
这会将待翻译文本拆分为两部分，分别翻译它们，然后将它们重新组合起来。

这些通过未翻译文本以 'sr:' 开头来识别。

还值得注意的是，如果配置得当，这种方法可以递归使用。这意味着它允许被正则表达式拆分用于翻译的各个字符串流入另一个拆分器正则表达式，依此类推。

除了通过索引识别每个组外，还可以通过名称识别它们，这允许组完全附加。让我们看一个结合了所有这些内容的示例：
```
sr:"^\[(?<stat>[\w\s]+)(?<num_i>[\+\-]{1}[0-9]+)?\](?<after>[\s\S]+)?$"="[${stat}${num_i}]${after}"
```
在这个例子中，有 3 个命名组，其中两个是可选的（标准正则表达式语法）。替换模式通过将名称用 `${}` 括起来来识别这些命名组。

如果标识符名称以 `_i` 结尾，则意味着该字符串将不会尝试被翻译，而是按原样传递。通常这并不是真正需要的，因为插件足够智能，能够确定是否应该翻译某些内容。

那么这个正则表达式会拆分什么呢？它会拆分像这样的字符串：
```
[DEF+14][ATK+64][DEX+34][AGI]
```
组 `(?<stat>[\w]+)(?<num_i>[\+\-]{1}[0-9]+)?` 匹配 `[]` 内的文本。如您所见，有两个组。第一个是必需的，表示文本。第二个是可选的，表示后面的正负号和数字。

组 `(?<after>[\s\S]+)` 匹配后面的任何内容。因此，它会像翻译任何其他文本一样尝试翻译该文本，并且可能直接流回此拆分器正则表达式。


#### 正则表达式后处理
使用配置选项 `RegexPostProcessing`，也可以对正则表达式的组应用后处理。对于 `sr:` 正则表达式，仅将它们应用于名称以 `_i` 结尾的组。


### UI 字体调整大小
也可以手动控制文本组件的字体大小。当翻译后的文本比未翻译的文本占用更多空间时，这很有用。

您可以在翻译 `Directory` 中以 `resizer.txt` 结尾的文件中控制此设置。此文件采用简单的语法，如下所示：
```
CharaCustom/CustomControl/CanvasDraw=ChangeFontSizeByPercentage(0.5)
```
在这些文件中，等号左边表示必须调整字体大小的组件的（部分）路径。右边表示要对这些文本执行的命令的“;”分隔列表。

在所示示例中，它会将指定路径下所有文本的字体大小减小到 50%。

与任何其他翻译文件一样，这些文件也支持翻译范围，如[本节](#翻译范围)所述。

存在以下类型的命令：
- 将字体大小更改为静态大小的命令：
  - `ChangeFontSizeByPercentage(double percentage)`：百分比是原始字体大小要缩减到的百分比。
  - `ChangeFontSize(int size)`：大小是字体的新大小
  - `IgnoreFontSize()`：可用于重置在非常“非特定”路径上设置的字体大小调整行为。
- 控制自动调整大小的命令：
  - `AutoResize(bool enabled, minSize, maxSize)`：其中 enabled 控制是否应启用自动调整大小行为。最后两个参数是可选的。
    - minSize、maxSize 可能的值：[keep, none, 任何数字]
- 控制行间距的命令（仅 UGUI）：
  - `UGUI_ChangeLineSpacingByPercentage(float percentage)`
  - `UGUI_ChangeLineSpacing(float lineSpacing)`
- 控制水平溢出的命令（仅 UGUI）：
  - `UGUI_HorizontalOverflow(string mode)` - 可能的值：[wrap, overflow]
- 控制垂直溢出的命令（仅 UGUI）：
  - `UGUI_VerticalOverflow(string mode)` - 可能的值：[truncate, overflow]
- 控制溢出的命令（仅 TMP）：
  - `TMP_Overflow(string mode)` - [可能的值](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/api/TMPro.TextOverflowModes.html)
- 控制文本对齐的命令（仅 TMP）：
  - `TMP_Alignment(string mode)` - [可能的值](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/api/TMPro.TextAlignmentOptions.html)

但是您可能会问！我如何确定要使用的路径？此插件无法轻松确定这一点，但有其他插件可以让您做到这一点。

有两种方法，您可能需要同时使用它们：
- 使用 [Runtime Unity Editor](https://github.com/ManlyMarco/RuntimeUnityEditor) 来确定这些。
- 启用选项 `[Behaviour] EnableTextPathLogging=True`，这将记录所有文本被更改的文本组件的路径。


### 翻译范围
有两种选项可用于将翻译范围限定到游戏的仅一部分：

翻译文件支持以下指令：
- `#set level 1,2,3` 告诉插件，此文件中此行后面的翻译只能应用于 ID 为 1、2 或 3 的场景中。
- `#unset level 1,2,3` 告诉插件，此文件中此行后面的翻译不应应用于 ID 为 1、2 或 3 的场景中。如果未设置级别，则所有指定的翻译都是全局的。
- `#set exe game1,game2` 告诉插件，此文件中此行后面的翻译只能应用于通过名为 game1 或 game2 的可执行文件运行游戏时。
- `#unset exe game1,game2` 告诉插件，此文件中此行后面的翻译不应应用于通过名为 game1 或 game2 的可执行文件运行游戏时。如果未设置可执行文件，则所有指定的翻译都是全局的。
- `#set required-resolution height > 1280 && width > 720` 告诉插件，此文件中此行后面的翻译仅应在分辨率大于指定值时应用。当前实现仅处理游戏启动时使用的分辨率。
- `#unset required-resolution` 告诉插件忽略先前指定的 `#set required-resolution` 指令。

要使此功能生效，必须将以下配置选项设置为 `True`：
```
[Behaviour]
EnableTranslationScoping=True
```

此外，此行为在 `OutputFile` 中不可用。

您可以随时使用热键 CTRL+ALT+NP7 查看加载了哪些级别。

另一种确定翻译范围的方法是通过文件名。可以告诉插件在哪里查找翻译文件。可以使用变量 {GameExeName} 对这些路径进行参数化。

为每个可执行文件分离翻译的配置示例：
```
[Files]
Directory=Translation\{GameExeName}\{Lang}\Text
Directory=Translation\{GameExeName}\{Lang}\Text\_AutoGeneratedTranslations.txt
Directory=Translation\{GameExeName}\{Lang}\Text\_Substitutions.txt
```

那么什么时候应该确定翻译的范围呢？这取决于范围的类型：
- `level` 范围实际上只应用于避免翻译冲突
- `exe` 范围既可用于避免翻译冲突，也可用于提高性能


### 文本查找和空白处理
提供本节是为了让翻译人员了解此插件如何查找文本并提供翻译。

在最简单的形式中，插件的工作方式类似于未翻译文本字符串的字典。当插件看到它认为未翻译的文本时，它会尝试在字典中查找该文本字符串，如果找到结果，它将显示找到的翻译。

然而，情况并非总是如此简单。根据游戏使用的引擎/文本框架，在不同上下文中使用时，未翻译的文本字符串可能会略有不同。例如，对于视觉小说，在“ADV 历史”视图中出现的文本字符串可能与最初显示给用户时的文本字符串不完全相同。

**示例：**
```
「こう見えて怒っているんですよ？……失礼しますね」
「こう見えて怒っているんですよ？\n ……失礼しますね」
```

这些文本字符串不相同，如果最终翻译应该相同，却要多次翻译相同的文本，会很麻烦。

事实上，只需要其中一个翻译。原因如下（仍然非常简化）：
1. 当插件看到未翻译的文本时，它实际上会进行四次查找，而不是一次。按顺序如下：
   - 基于未修改的原始文本
   - 基于原始文本但没有前导/尾随空白。如果找到，前导/尾随空白将添加到结果翻译中
   - 基于原始文本但没有围绕换行符的内部非重复空白
   - 基于原始文本但没有前导/尾随空白以及围绕换行符的内部非重复空白。如果找到，前导/尾随空白将添加到结果翻译中

这意味着对于以下字符串 `\n 「こう見えて怒っているんですよ？\n ……失礼しますね」`，插件将进行以下查找：
```
\n 「こう見えて怒っているんですよ？\n ……失礼しますね」
「こう見えて怒っているんですよ？\n ……失礼しますね」
\n 「こう見えて怒っているんですよ？……失礼しますね」
「こう見えて怒っているんですよ？……失礼しますね」
```

2. 当插件加载（手动/自动）翻译时，它不会创建一个字典条目，而是三个。它们是：
   - 基于未修改的原始文本和原始翻译
   - 基于原始文本（无前后空白）和原始翻译（无前后空白）
   - 基于原始文本（无前后空白和围绕换行符的内部非重复空白）和原始翻译（无前后空白和围绕换行符的内部非重复空白）

这意味着对于以下字符串 `\n 「こう見えて怒っているんですよ？\n ……失礼しますね」`，插件将创建以下条目：
```
\n 「こう見えて怒っているんですよ？\n ……失礼しますね」
「こう見えて怒っているんですよ？\n ……失礼しますね」
「こう見えて怒っているんですよ？……失礼しますね」
```

这意味着您可以为这两种情况提供一个翻译。您认为哪种更好取决于您自己。

另一件需要注意的是，插件将始终在翻译文件中输出未修改的原始文本。但是，如果之后它看到另一个由于上述文本修改而与此文本字符串“兼容”的文本，默认情况下它不会输出这个新文本。

这由配置选项 `CacheWhitespaceDifferences=False` 控制。您可以将其更改为 true，它将为每个唯一文本输出一个新条目，即使唯一的差异是空白。显然，实际出现在翻译文件中的翻译对将始终优先于基于现有翻译对生成的翻译对。

*注意：无论此设置如何，与级别范围翻译相关的空白差异都不会输出。*


### 资源重定向
有时，通过直接覆盖游戏资源文件来为游戏提供翻译更容易。然而，直接覆盖游戏资源文件也有问题，因为这意味着修改可能只适用于游戏的一个版本。

为克服此问题并允许修改资源文件，此插件还具有资源重定向器模块，允许重定向游戏加载的任何类型的资源。

在深入了解此模块的细节之前，值得一提的是：
- 它不是插件。相反，它只是一个不依赖于任何插件管理器的库（它确实附带了一个与插件兼容的 BepInEx DLL，但这只是为了管理配置）。
- 它与游戏无关。
- 虽然它可以与自动翻译器一起再分发，但它完全独立于自动翻译器，可以在不安装自动翻译器的情况下使用。

资源重定向器工作所需的 DLL 是 `XUnity.Common.dll` 和 `XUnity.ResourceRedirector.dll`。就其本身而言，这些库没有任何作用。

默认情况下，自动翻译器插件附带一个用于 `TextAsset` 的资源重定向器，它基本上将原始文本资源输出到文件系统，允许单独覆盖它们。

可以为特定游戏实现更多重定向器，但这需要编程知识，有关更多信息，请参见[本节](#实现资源重定向器)。

自动翻译器具有以下特定于资源重定向器的配置：
- `PreferredStoragePath`：指示自动翻译器应存储重定向资源的位置。
- `EnableTextAssetRedirector`：指示是否启用 TextAsset 重定向器。
- `LogAllLoadedResources`：指示资源重定向器是否应将所有资源记录到控制台（也可通过资源重定向器 API 表面控制）。
- `EnableDumping`：指示重定向到自动翻译器的资源是否应被转储以便可能覆盖。
- `CacheMetadataForAllFiles`：当文件位于 PreferredStoragePath 中的 ZIP 文件中时，这些文件会在内存中建立索引，以避免加载时执行文件检查 IO。启用此选项也会对物理文件执行相同操作

放在 `PreferredStoragePath` 中的 ZIP 文件将在启动期间建立索引，允许重定向的资源被压缩和打包。当文件放在 zip 文件中时，在文件查找期间，zip 文件被视为不存在。


## 关于再分发
非常鼓励为各种游戏再分发此插件。但是，如果您这样做，请记住以下几点：
- **将 _AutoGeneratedTranslations.txt 文件与再分发一起分发，并包含尽可能多的翻译，以确保对在线翻译器的访问尽可能少。**
- **通过启用日志/控制台测试您的再分发，以确保游戏不会表现出不良行为，例如向端点发送垃圾信息。**
- 不要再分发配置了非默认翻译端点的插件，该端点来自此存储库。这意味着：
  - 不要设置 `Endpoint=DeepLTranslate` 然后再分发。
  - 但是，如果您实现了自己的端点或者该端点不是此存储库的一部分，您可以继续并将其作为默认端点再分发。
- 确保尽可能保持插件为最新版本。
- 如果您使用图像加载功能，请确保阅读[本节](#纹理翻译)。


## 纹理翻译
从 2.16.0 版本开始，此模组提供替换图像的基本功能。此功能默认禁用。不过，这些图像没有自动翻译功能。

此功能主要用于几乎没有模组支持的游戏，以实现完整翻译，而无需修改资源文件。

它由以下配置控制：
```ini
[Texture]
TextureDirectory=Translation\Texture
EnableTextureTranslation=False
EnableTextureDumping=False
EnableTextureToggling=False
EnableTextureScanOnSceneLoad=False
EnableSpriteRendererHooking=False
LoadUnmodifiedTextures=False
TextureHashGenerationStrategy=FromImageName
DuplicateTextureNames=
DetectDuplicateTextureNames=False
EnableLegacyTextureLoading=False
CacheTexturesInMemory=True
```

`TextureDirectory` 指定转储纹理和加载图像的目录。加载也会从指定目录的所有子目录中进行，因此您可以将转储的图像移动到任何您想要的文件夹结构中。

`EnableTextureTranslation` 启用纹理翻译。这基本上意味着纹理将从 `TextureDirectory` 及其子目录中加载。这些图像将替换游戏中使用的图像。

`EnableTextureDumping` 启用纹理转储。这意味着模组将把所有尚未转储的图像转储到 `TextureDirectory`。转储纹理时，可能还值得启用 `EnableTextureScanOnSceneLoad` 以更快地找到所有需要翻译的纹理。**切勿启用此选项再分发模组。**

`EnableTextureScanOnSceneLoad` 允许插件在 sceneLoad 事件上扫描纹理对象。这使插件能够以在场景加载期间（通常在加载屏幕等期间）微小的性能成本找到更多纹理。然而，由于 Unity 的工作方式，不能保证所有这些纹理都可以替换。如果您发现一个已转储但无法翻译的图像，请报告。但是，请认识到此模组主要用于替换 UI 纹理，而不是 3D 网格的纹理。

`EnableSpriteRendererHooking` 允许插件尝试挂钩 SpriteRenderer。这是一个单独的选项，因为实际上无法正确挂钩 SpriteRenderer，所实现的解决方法在某些情况下可能会对性能产生理论上的影响。

`LoadUnmodifiedTextures` 启用插件是否应加载未修改的纹理。这仅用于调试，并且可能导致各种视觉故障，特别是如果 `EnableTextureScanOnSceneLoad` 也启用的话。**切勿启用此选项再分发模组。**

`EnableTextureToggling` 启用 ALT+T 热键是否也会切换纹理。这绝不能保证
