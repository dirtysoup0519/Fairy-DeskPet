# 授权范围

本仓库包含不同来源的文件，不能把一个组件的许可证视为全部内容的授权。

| 范围 | 当前状态 | 核对依据 |
| --- | --- | --- |
| `assets/frames/000.png`～`119.png` | 上游代码生成的视觉衍生文件，保留 Apache-2.0 声明 | `assets/NOTICE`、`licenses/Apache-2.0.txt`、上游归属与商标声明 |
| 新增 C# 程序、工具、项目文档与公开精简人设 | 尚未由项目所有者指定整体许可证 | 未代所有者添加统一授权或作者署名 |
| GPT-SoVITS 运行源码 | 外部可选依赖，不随本仓库分发 | 上游固定版本的 MIT LICENSE；由用户自行配置 |
| `ScreenCapture.cs` 的变化评分与关键帧管线参考部分 | 参考 dsh-see-now 的 Apache-2.0 设计，保留来源与改写说明 | `licenses/dsh-see-now-LICENSE.txt`、`licenses/dsh-see-now-NOTICE.txt`；不覆盖应用整体 |
| 模型、权重、参考录音、游戏语音、训练集 | 禁止提交或上传 | `.gitignore` 和提交候选检查脚本 |
| 角色、游戏名称、标识、剧情与其他第三方内容 | 不由代码许可证授予额外权利 | 上游 `TRADEMARKS.md`；不声称官方授权或背书 |

动画并非从游戏提取的官方位图；它由上游的角色式几何代码生成。保留代码授权和归属，不等于已经取得角色形象或商标的单独授权。

上游动画模块还包含其他 MIT / Apache 范围的插件、音效与工具，本仓库没有打包那些模块，不能据其许可证推定当前整个应用已获 MIT 或 Apache 授权。

原始声明请阅读 [Apache 文本](licenses/Apache-2.0.txt)、[上游 NOTICE](licenses/Fairy-DSH-NOTICE.txt)、[商标与内容边界](licenses/Fairy-DSH-TRADEMARKS.md) 和 [第三方来源](THIRD_PARTY_NOTICES.md)。这些原始法律文本保留原文。
