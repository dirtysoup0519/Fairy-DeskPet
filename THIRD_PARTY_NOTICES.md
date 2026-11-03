# 第三方来源与范围

## 动画

`assets/frames/` 的 120 帧为本地动画模块按 Fairy-DSH 的几何代码生成的深色动画衍生文件。来源、改动和输出范围保留在 `assets/NOTICE`。

- 视觉上游：[Fairy-DSH 固定版本](https://github.com/Chengzhibense/Fairy-DSH/tree/d639887a386b0de6fef2e61c97dc268631854591)。
- 核对过的 [Apache-2.0 LICENSE](https://github.com/Chengzhibense/Fairy-DSH/blob/d639887a386b0de6fef2e61c97dc268631854591/LICENSE)、[NOTICE](https://github.com/Chengzhibense/Fairy-DSH/blob/d639887a386b0de6fef2e61c97dc268631854591/NOTICE) 和 [TRADEMARKS](https://github.com/Chengzhibense/Fairy-DSH/blob/d639887a386b0de6fef2e61c97dc268631854591/TRADEMARKS.md)。
- 本地动画生成源版本：`7d826d50ae8c648a24f4e5f60d6b2b35634ba7e5`。这是来源追踪标识，不声明它属于上述上游仓库。

分发这些动画时保留 `assets/NOTICE` 与 `licenses/`。上游代码许可不包含角色、商标或官方游戏素材的额外授权；本项目不声称官方合作或背书。

## 人设

公开的 `assets/fairy-persona.txt` 是本项目编写的精简助手风格说明，不含游戏台词摘录、游戏语料统计、用户姓名关系或任务记录。内容哈希与说明保存在 `assets/fairy-persona-source.json`。

从本机 DSH 导入的详细人设可能包含个人配置或第三方语料，只允许保存在被忽略的 `runtime/private/`。不把本地未提交模块当作上游已公开授权的文件。

## 可选语音运行时

GPT-SoVITS 由用户另外配置，未内嵌其运行源码、发行版或模型。核对参考：[固定版本 MIT LICENSE](https://github.com/RVC-Boss/GPT-SoVITS/blob/b4570b876f5c086819ba84e3215472733c6a7b70/LICENSE)。`runtime/serve_fairy.py` 仅为连接该外部 API 的小型适配脚本。

第三方运行时、依赖和模型分别受其自身条款约束。运行时代码的许可不能推定模型、声音或训练数据可再分发。

## 不分发的内容

模型、权重、基础模型、声码器、ASR / 分离模型、训练集、参考录音、游戏语音、视频音轨及其处理结果均不进入 Git，也不通过 LFS、Release 或压缩包上传。

Fairy 与《绝区零》相关名称和形象归相应权利人。本项目是非官方桌面适配。新增自有代码的整体许可证尚未指定；全部文件的具体范围见 [LICENSES.md](LICENSES.md)。
