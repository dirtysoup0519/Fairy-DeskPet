# Fairy DeskPet

Windows 悬浮桌宠，提供 Fairy 开场与退场动画、HDD 风格字幕、快捷输入，以及 DeepSeek / Codex 双入口。支持按任务分开对话和长期记忆，也可开启看屏待机、工作协作或游戏陪伴。文字对话与本机语音可以分开配置。

**完整源码可构建桌宠；先完成第 1～3 步即可使用文字对话，需要朗读和待机语音时再完成第 4 步。** 仓库不包含语音模型、权重或原作录音，下载源码不等于已经安装语音环境。

## 1. 准备环境和源码

| 使用方式 | 需要准备 |
| --- | --- |
| 所有方式 | Windows x64、.NET Framework 4.8、PowerShell、完整项目文件 |
| DeepSeek 文字对话 | 可用的 DeepSeek API Key、账户可调用的模型、网络 |
| Codex 文字对话 | Windows 原生 `codex.exe`、已完成的 ChatGPT 登录、相应账户使用权限及网络 |
| 合成语音 | 能正常推理的 GPT-SoVITS v2 本地环境、兼容权重与基础模型、参考音频及对应文字 |
| 原声试听 | 用户本地提供的语音包；不依赖 TTS 模型 |

构建桌宠本体无需 Visual Studio、Python、Node.js 或旁边的 DSH 工程。Python 仅在配置本机 TTS 时需要；Node.js 仅在使用可选人设导入工具时需要。

已有本地项目文件时，直接在包含 `build.ps1` 的目录打开 PowerShell，不必重新克隆。获取已发布源码可使用：

```powershell
git clone https://github.com/dirtysoup0519/Fairy-DeskPet.git
Set-Location Fairy-DeskPet
```

克隆或解压后先确认当前目录存在 `build.ps1`。若远端仍为空、未发布源码或只下载了部分文件，这些步骤不能完成构建，需要先取得完整源码。

检查系统编译器：

```powershell
Test-Path "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
```

应返回 `True`；否则先安装或修复 [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48)。使用 ZIP 源码也可以，解压后保留 `assets/`、`licenses/`、`chat-workspace/` 等子目录。

## 2. 构建并打开桌宠

在项目根目录执行：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Start-Fairy.ps1
```

这里的执行策略参数仅作用于启动的 PowerShell 进程，不更改系统永久策略。构建成功会显示 `Built ...FairyPet.exe`，随后桌面出现悬浮图标。以后可直接双击 `FairyPet.exe` 或运行 `Start-Fairy.ps1`。

首次启动没有 `runtime.local.json` 时使用文字模式，默认关闭自动朗读和待机语音，也不会尝试启动未配置的语音环境。右键图标打开完整对话窗口，按下一步配置任一聊天入口。旧版本偏好会继续保留；已有偏好的用户如需纯文字模式，可手动关闭自动朗读和待机。

程序只允许一个桌宠实例。如果提示已经运行，请使用托盘中的 Fairy；需要换新版时，先从托盘退出，再打开新的 EXE。重新构建不会自动替换正在运行的实例。

## 3. 配置聊天入口（二选一，也可都配置）

### A. DeepSeek API

1. 在 [DeepSeek 开放平台](https://platform.deepseek.com/) 准备 API Key，确认账户可调用 API。
2. 在完整窗口点击 `DEEPSEEK / API`，或打开快捷输入框的「更多」，切换到 DeepSeek 后点「设置」。
3. 在“在这里粘贴 DeepSeek API Key”中填写 Key；模型名称填写账户实际可用的型号。
4. 点击“应用并返回对话”，发送一句简短问题，确认收到文字回复。

软件可能显示旧默认名称 `deepseek-v4-flash`。配置时以[官方 API 文档](https://api-docs.deepseek.com/)为准，例如当前文档列出的 `deepseek-flash`；不要把仓库里的默认值视为永久有效的型号。

Key 只在本次运行内存中保留，退出后需重新填写。程序也可读取启动进程已有的 `DEEPSEEK_API_KEY`，但不要求你创建 `.env`，不要把 Key 写进源码或 README。DeepSeek API 的账户与计费独立于 Codex 登录。

### B. Codex / ChatGPT 登录

1. 按 [OpenAI 官方 CLI 安装说明](https://learn.chatgpt.com/docs/codex/cli)安装 Windows 原生 CLI。也可使用已安装桌面客户端提供的 CLI。
2. 在能够运行该 CLI 的 PowerShell 中登录并确认状态：

   ```powershell
   codex login
   codex login status
   ```

3. 按浏览器提示完成 ChatGPT 登录，确认状态为已登录，再重新打开桌宠。
4. 选择 `CODEX / 订阅`，发送一句简短问题，确认收到回复。

`codex login` 的默认浏览器登录和 `codex login status` 的含义见[官方认证说明](https://learn.chatgpt.com/docs/auth)。使用订阅入口需要相应账户权限／可用额度；不需要在桌宠里填写 OpenAI API Key。

**本项目直接启动 `codex.exe`。** 它先寻找 `%LOCALAPPDATA%/OpenAI/Codex/bin/` 下的可执行文件，再尝试 PATH 中的 `codex.exe`。仅安装 WSL 中的 Codex，或只能找到 npm 的 `codex.cmd`，不等于满足这个条件。可检查：

```powershell
Get-Command codex.exe -ErrorAction SilentlyContinue
```

PATH 方式应能返回原生可执行文件；修改 PATH 或安装 CLI 后需重开桌宠。桌宠会关闭子进程的 shell / computer-use 工具，聊天入口不会因登录 Codex 就获得操作电脑的功能。

### 联网与对话验证

两种入口都可在「设置 → 模型与联网」中切换「允许联网搜索」。快捷输入中的「联网」和「来源」都在「更多」里。开启后可询问需要查证的公开资料，有来源时可点击「来源」。联网失败时先检查对应账户、网络和服务接口。

## 4. 配置语音

文字入口成功后再做这一步。**模型、权重、训练集、参考录音和游戏语音只能保存在本机，严禁提交或上传到 Git、LFS、Release 或归档附件。**

### A. 已有可运行的 DSH / Fairy TTS 环境

这是复用现有模型的方式，不需要重新训练。

1. 若服务已经运行，检查：

   ```powershell
   Invoke-RestMethod -Uri 'http://127.0.0.1:9880/fairy/health' -TimeoutSec 5 |
       Select-Object service,ready
   ```

   应返回 `service = dsh-fairy-tts`、`ready = True`。仅能访问普通 GPT-SoVITS `/docs` 不足以说明它兼容本项目的就绪检查。

2. 若还没有本地配置，复制模板；已配置过则直接编辑现有文件，不要覆盖：

   ```powershell
   if (-not (Test-Path .\runtime.local.json)) {
       Copy-Item .\runtime.example.json .\runtime.local.json
   }
   notepad .\runtime.local.json
   ```

3. 在 `LauncherScript` 中填写已有启动脚本的路径；在 `ReferenceDir` 中填写参考目录。可用项目相对路径；填写 Windows 绝对路径时，JSON 中使用 `/` 或转义的 `\`，不要填未转义的单反斜杠。
4. 参考目录必须同时包含 `fairy_ref.wav` 与 `fairy_ref.txt`，后者是该段参考录音对应的文字。
5. 运行 `runtime/start-fairy-tts.ps1`，确认健康检查通过，然后在界面点击“试听 Fairy”。

设置了 `LauncherScript` 后，由该脚本管理已有环境，模板中的 `PythonPath`、`SourceRoot`、`TtsConfigPath` 不参与此次启动。服务已就绪时直接复用。`ReferenceDir` 仍供合成请求使用，不能省略必要的参考文件。

### B. 新机器没有语音环境

本项目只提供语音服务适配器，不是 GPT-SoVITS 的完整安装包。请先按[GPT-SoVITS 上游安装说明](https://github.com/RVC-Boss/GPT-SoVITS)准备能正常推理的 v2 环境。当前适配所参考的源码版本为 `b4570b876f5c086819ba84e3215472733c6a7b70`；不同版本的 API 与依赖可能有差异。

按以下清单准备本地资源：

| 资源 | 要求／推荐位置 |
| --- | --- |
| Python 环境 | Python 3.11，建议独立环境；`runtime/venv/`，也可使用已有环境 |
| 推理源码 | 兼容的 GPT-SoVITS，包含 `api_v2.py`、`GPT_SoVITS/`；`runtime/GPT-SoVITS/` |
| 基础模型 | 中文 RoBERTa 与 HuBERT 的完整目录，由 TTS YAML 指定 |
| 角色权重 | 相互兼容的 GPT `.ckpt` 与 SoVITS `.pth`，版本与配置一致；本地自备 |
| 文本前处理资源 | 按相应运行时要求准备 G2PW、语言识别和 NLTK 等资源 |
| 参考音频与文本 | `runtime/private/reference/fairy_ref.wav`、`fairy_ref.txt`；清晰且内容对应 |
| TTS 配置 | `runtime/private/tts.yaml`；指定模型、设备和精度 |

本项目环境验证过 Python 3.11、PyTorch / torchaudio 2.7.1 + CUDA 11.8。`runtime/requirements-tested.txt` 是依赖快照，**不是脱离上游环境即可一键安装成功的完整锁文件**；PyTorch/CUDA、基础依赖和模型资源仍需单独准备。CPU 或其他 GPU 的环境要按实际硬件调整。

准备好可推理环境后，完成接入：

1. 建立本地配置目录，复制 TTS 模板：

   ```powershell
   New-Item -ItemType Directory -Force -Path .\runtime\private\reference | Out-Null
   if (-not (Test-Path .\runtime\private\tts.yaml)) {
       Copy-Item .\runtime\tts.example.yaml .\runtime\private\tts.yaml
   }
   notepad .\runtime\private\tts.yaml
   ```

2. 替换 YAML 中所有 `/local/...` 占位路径：`bert_base_path`、`cnhuhbert_base_path`、`t2s_weights_path`、`vits_weights_path`。这些路径由外部运行时解释，建议填写本机绝对路径。CUDA 配置需要兼容的 NVIDIA 环境；CPU 环境使用 `device: cpu`、`is_half: false`。`version` 保持与权重兼容。
3. 复制 `runtime.example.json` 为 `runtime.local.json`，按前一小节的“不覆盖已有文件”方式操作。新环境的 `LauncherScript` 留空，填写以下字段：

   | 字段 | 含义 | 默认相对路径 |
   | --- | --- | --- |
   | `PythonPath` | 已装好推理依赖的 Python 可执行文件 | `runtime/venv/Scripts/python.exe` |
   | `SourceRoot` | GPT-SoVITS 源码根目录 | `runtime/GPT-SoVITS` |
   | `TtsConfigPath` | 刚编辑好的配置 | `runtime/private/tts.yaml` |
   | `ReferenceDir` | 参考 WAV 与 TXT 所在目录 | `runtime/private/reference` |
   | `LauncherScript` | 委托外部启动脚本；本方案不使用 | 空字符串 |

4. 把参考录音和对应文字放入 `ReferenceDir`。运行时读取的文件名固定为 `fairy_ref.wav`、`fairy_ref.txt`。项目启动脚本把 NLTK 数据目录设为 `runtime/private/nltk_data`；相应语言资源也应在本机准备妥当。
5. 手动启动并检查就绪状态：

   ```powershell
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\runtime\start-fairy-tts.ps1
   Invoke-RestMethod -Uri 'http://127.0.0.1:9880/fairy/health' -TimeoutSec 5 |
       Select-Object service,ready
   ```

6. 状态就绪后重开桌宠，在「设置 → 语音」点击「试听 Fairy」并调整音量。确认能听到完整测试句后，可在对话页启用「自动朗读回复」，或在语音页启用待机语音。

配置正确后，以后打开桌宠会自动复用或启动模型，无需每次手动运行语音脚本。模型加载可能需要数分钟。此自动加载不创建 Windows 计划任务，也不设置系统登录自启动；退出桌宠不会关闭其他程序共用的服务。

### C. 可选：导入原声包

原声包与 TTS 权重是两种资源：前者用于直接播放已有录音，后者用于合成任意文字。两者都不随仓库提供。

使用自己本地持有、可合法使用的语音目录，其中应有 `catalog.json` 及清单对应的 32 kHz、单声道、PCM16 little-endian `.pcm` 文件：

```powershell
.\tools\import-local-idle.ps1 -SourceDirectory '<local-audio-directory>'
```

将占位参数替换为实际目录。导入后重新打开桌宠，点击“试听原作”。导入器会校验哈希、时长和字幕，已有包先做本地备份；文件位于被忽略的 `assets/idle-original/`。完整格式见 [语音配置说明](docs/VOICE_SETUP.md)。

没有原声包时，随机待机台词交给本机 TTS 合成；没有 TTS 环境时，应关闭这类合成朗读。已导入的录音可单独播放，但不能代替任意文本合成。

## 5. 日常操作与本地数据

### 屏幕协作（可选，默认关闭）

右键 Fairy →「屏幕协作设置…」，选定显示器后可点击「截屏并分析一次」，或开启「全屏对话与持续观察」来使用待机陪伴、工作协作或游戏协作。开启后图标旁会显示橙色状态点。停止、隐藏或切换入口会取消读取。工作和游戏协作的语音由「朗读屏幕分析结果」控制。

开启或手动分析会将选定屏幕图片发送至当前 Codex / DeepSeek 入口，可能消耗额度。DeepSeek 看图模型默认 `deepseek-flash`；Codex 当前 CLI 模型也需支持图片。每秒本地采样完整画面、最多缓存两帧，变化后按 5～300 秒冷却分析；不是逐帧视频识别，不会遮盖其他软件中的敏感内容。支持全部显示器。

游戏可选择“游戏协作”：目标每 0.25 秒采样，稳定后按事件分析，可在游戏陪伴设置中调节主动开口间隔；`Ctrl+Alt+F8` 分析当前画面，`Ctrl+Alt+F9` 本地预览（不上传）。模型与语音延迟仍取决于服务速度。游戏模式可按当前章节／区域后台搜索攻略并缓存来源；进度不确定时只查通用技巧，可手动指定章号。默认只给无后续剧情的思路提示，支持 Fairy 风格的克制鼓励。需要同时开启联网搜索与游戏攻略开关。

开启全屏对话后，普通提问会附加一张新截图。可先用本地预览确认采集范围，尤其是共享屏幕或多显示器时。采集范围、数据流和停止行为见 [屏幕协作说明](docs/SCREEN_ASSIST.md)。

### 看屏待机与语音字幕

1. 在「设置 → 语音 → 待机语音」开启「空闲时播放待机语音」，设置 1～60 分钟的间隔。
2. 在「屏幕协作」开启「全屏对话与持续观察」，选择「待机陪伴」。
3. 到点后，Fairy 截取当前画面并按人设生成一句短台词。对话或朗读进行中会顺延，实际出声还要等识图与本机合成。

看屏待机只播放合成语音。遇到画面没有合适话题或识别失败时，会从普通合成待机台词中随机选一句。关闭屏幕观察后，普通待机可在原创合成台词和已导入的原声之间随机选择。语音合成完成后弹出字幕，播放时同步翻页，播完后短暂停留。右键字幕可提前收回。

### 任务、人设与长期记忆

- 在对话页用任务下拉框切换，点「管理」或打开「设置 → 任务与记忆」新建、重命名、删除任务。编辑名称时，Enter 换行，Ctrl+Enter 确认。
- 任务列表、当前任务和各任务的长期记忆可在重启后恢复。对话、草稿和最近回复只保留在本次运行中，每个任务内的 Codex 与 DeepSeek 对话记录分别保存。删除自建任务会清除它的对话、草稿和长期记忆。默认任务可以单独清空记忆。
- 在「任务与记忆」可以查看、停用或清空当前任务的长期记忆，也可以在对话中用「记住：」主动保存一条文字。每个任务最多保存 500 条，由 Windows DPAPI 在本机加密。检索到的相关文字可能随提问发送到所选模型入口。截图不写入长期记忆。
- 在「设置 → 人设」可修改 Fairy 对你的称呼、它的自称和表达偏好。公开默认人设见 `assets/fairy-persona.txt`。本机 `runtime/private/fairy-persona.txt` 可覆盖默认人设，Node.js 人设导入工具也只写入本地忽略目录。

### 基本操作

- 单击图标打开或收起快捷输入，拖动图标可移动，右键菜单可打开完整窗口。快捷输入用 Enter 发送、Shift+Enter 换行。「更多」里有模型切换、联网、来源、朗读和设置。Esc 先收起已展开的操作，再关闭输入框。
- 完整对话用 Enter 换行、Ctrl+Enter 发送。「设置」页左侧按功能导航，语音页分别管理音量、试听和待机间隔。
- 启动或从托盘恢复时，桌宠图标会展开。隐藏或退出时会收回。动画遵循 Windows 的界面动画设置。
- 尺寸支持 32～360 px，小／中／大为 64／180／240 px；实际安全上限随工作区调整。输入框会按空间调整位置和高度。
- 偏好、日志、加密记忆和播放用临时音频位于 `%LOCALAPPDATA%/CodexFairyPet/`。请将这个目录留在本机。

## 6. 检查与故障排查

仓库审计脚本需要 Git 和 `.git` 元数据。ZIP 解压目录可以直接构建运行，但不能运行依赖 Git 的候选文件审计；需要这些检查时请使用包含源码的 Git 克隆目录。

无需聊天 Key 或语音模型的公开文件检查：

```powershell
.\tools\check-repository.ps1
.\tools\check-docs.ps1
```

可运行完整本地检查，无需聊天凭据或语音资源：

```powershell
.\tools\test-local.ps1
```

该脚本重建 EXE，运行 `--self-test`、`--render-ui` 和 `--test-screen`，再检查文档与候选文件。它会验证用户偏好没有变化，界面输出保存在被忽略的 `preview/`。`--self-test` 会记录 Codex 是否可用，基础自检无需安装 Codex 或发起真实聊天请求。

当前验证范围：在同一台 Windows 上复制公开文件，以空偏好、无私有人设／语音包／机器配置的副本验证构建、窗口加载、入口切换和界面布局；还覆盖了未安装 Codex 的基础自检分支。这不等同于所有全新 Windows 机器的实机测试，也没有代其他用户验证 API Key、账户额度或语音环境。

| 现象 | 检查方法 |
| --- | --- |
| 编译器不存在 | 检查第 1 步的 `Test-Path` 结果及 .NET Framework 安装 |
| EXE 无法重新生成 | 先从托盘退出占用它的旧实例，再构建 |
| 启动提示语音未配置 | 先关闭自动朗读和待机，使用文字对话；要发声再完成第 4 步 |
| DeepSeek 无法回复 | 检查 Key、模型名称、账户可用状态、网络，以及是否点击“应用并返回对话” |
| Codex 找不到或无法登录 | 确保是 Windows 原生 `codex.exe`，检查 `codex login status`；安装或改 PATH 后重开桌宠 |
| Codex 检查显示不可用 | 原生 CLI 支持从 PATH 解析完整路径；检查 `Get-Command codex.exe` 和登录状态，只有 `codex.cmd` 的安装方式仍不支持 |
| 9880 已被占用 | 查看健康检查是否属于本项目服务；启动器不会强制结束其他占用进程 |
| 健康检查通过但合成失败 | 检查参考 WAV/TXT、路径转义、权重版本、设备和文本前处理资源；就绪检查不代替实际合成测试 |
| 有音频生成却听不见 | 检查界面音量、系统混音器、当前耳机／扬声器与静音状态；先用“试听 Fairy”验证 |
| 到点没有听到看屏待机 | 确认待机语音和屏幕观察都开启，模式选「待机陪伴」。先用「试听 Fairy」确认本机合成可用，再检查当前入口能否完成一次截屏分析 |
| 原声按钮提示未导入 | 原声包需本地导入，仓库不附录音；确认清单与 PCM 文件完整后重开 |

新环境的服务输出日志是 `%LOCALAPPDATA%/CodexFairyPet/tts-server.log` 与 `tts-server-error.log`。使用 `LauncherScript` 委托启动时，详细日志位置由该脚本决定。

## 7. 版权、隐私与提交边界

模型、权重、原声、训练集、参考录音、私有人设、API Key、本机配置及计划任务／自动化记录不得进入提交集合。`.gitignore` 与候选检查同时覆盖这些内容；强制暂存的文件也会被检查。检查脚本需要主动运行，并非远端服务器的强制规则。

公开前请阅读 [提交前检查](docs/PUBLISHING.md)、[隐私与版权说明](docs/PRIVACY_AND_RIGHTS.md)、[第三方来源](THIRD_PARTY_NOTICES.md) 与 [授权范围](LICENSES.md)。动画保留 `assets/NOTICE` 和 `licenses/` 的上游声明；它们不授予角色、名称或商标的额外权利。新增自有代码的整体许可证尚未指定。
