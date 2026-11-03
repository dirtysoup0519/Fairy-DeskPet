# 本地语音配置

语音模型、权重、基础模型、参考音频、训练集和游戏语音一律不提交、不上传。`.gitignore` 与 `tools/check-repository.ps1` 均按此规则处理。

## 复用已有服务

如果本机 `http://127.0.0.1:9880/fairy/health` 返回 `service=dsh-fairy-tts`、`ready=true`，桌宠直接复用。

需要自动启动已有环境时，将 `runtime.example.json` 复制为 `runtime.local.json`，将 `LauncherScript` 填为现有启动脚本路径、`ReferenceDir` 填为参考音频目录。`ReferenceDir` 应包含 `fairy_ref.wav` 和对应的 `fairy_ref.txt`。路径可相对仓库，也可使用本机绝对路径；不要把该配置提交到 Git。启动脚本使用相同的本地启动锁，避免同时启动多个服务。

## 新机器配置

1. 自行准备兼容 GPT-SoVITS v2 的 Python 3.11 环境与运行源码，放在 `runtime/venv/`、`runtime/GPT-SoVITS/` 或通过本地配置指定已有路径。
2. 所需基础模型、训练后的 GPT / SoVITS 权重、参考音频、NLTK 资源均由用户本地准备。不要复制进可提交目录；推荐 `runtime/private/`。
3. 按 `runtime/tts.example.yaml` 的结构创建本地 TTS 配置，填入实际路径，保存到 `runtime/private/tts.yaml`。YAML 中路径由 GPT-SoVITS 解释，通常应使用本机绝对路径。
4. 将 `runtime.example.json` 复制为 `runtime.local.json`，填写 Python、源码、TTS 配置和参考目录；新环境的 `LauncherScript` 留空。
5. 执行 `runtime/start-fairy-tts.ps1` 检查启动。日志位于 `%LOCALAPPDATA%/CodexFairyPet/tts-server*.log`。再打开桌宠。

`runtime/requirements-tested.txt` 是本机已验证环境的 Python 依赖快照，不包含 PyTorch/CUDA，不保证在任意硬件下直接安装成功。本机验证过 Python 3.11、PyTorch / torchaudio 2.7.1 + CUDA 11.8；请为目标机器独立选择兼容环境，不上传虚拟环境。

运行源码参考 GPT-SoVITS 上游，本机来源版本为 `b4570b876f5c086819ba84e3215472733c6a7b70`。仓库仅提供服务适配脚本，不内嵌或自动下载模型。

## 导入本地原声包

源目录应包含 `catalog.json` 和对应的 32 kHz / 单声道 / PCM16 little-endian `.pcm` 文件。可使用以下命令导入自己本地持有的语音：

```powershell
.\tools\import-local-idle.ps1 -SourceDirectory '<local-audio-directory>'
```

导入前校验 ID、音频 SHA-256、时长和字幕完整性，已有内容先备份到被忽略的 `runtime/private/import-backups/`，不删除源文件。目标目录 `assets/idle-original/` 永远不属于提交清单。更新原声后重新打开桌宠。

清单示例：

```json
{
  "Source": "local-user-provided",
  "Clips": [{
    "Id": "local-sample",
    "Text": "这是用户自备的一段语音。",
    "Duration": 4.0,
    "Sha256": "REPLACE_WITH_THE_AUDIO_SHA256",
    "Cues": [{ "Start": 0.0, "Text": "这是用户自备的一段语音。" }]
  }]
}
```

示例的时长与哈希必须替换为实际值。未导入原声包时，随机待机使用程序中的短句交给本机语音服务合成；未配置服务时，状态栏会提示缺少环境。

## 本地人设覆盖

可把自备人设保存到 `runtime/private/fairy-persona.txt`。它优先于公开精简人设，目录始终被 Git 忽略。调用 `node tools/sync-persona.cjs <local-persona-module>` 也只导入到本地目录，不会覆盖公开人设。不要将游戏语料、用户关系、对话或计划任务记录写进公开默认文件。
