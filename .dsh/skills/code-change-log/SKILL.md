---
name: code-change-log
description: 记录 DSH 对代码的每一次改动。任何新增/修改/删除代码（.cs、.asmdef、.shader、Tools 下的 .ps1 等）之后、回复用户之前，调用 Tools/Agent/change-log.ps1 生成一条包含时间、脚本、代码片段和修改说明的记录，落到 DescriptionOfChanges/<月>/。当任务涉及"改代码/加功能/修 bug/重构/写脚本"时使用。
whenToUse: 只要你（DSH）动过仓库里的代码文件；纯问答、纯排查、只读检查不需要。
---

# 代码变更记录（DescriptionOfChanges）

## 什么时候必须执行

**只要这一轮里你修改、新增或删除了代码文件，就必须在结束回复前跑一次记录脚本。**
"代码文件"指：`Assets/**/*.cs`、`*.asmdef`、`*.shader`、`*.hlsl`、`Tools/**/*.ps1`、`*.json`（配置类）。
文档（`*.md`）、美术资源、场景/预制体 YAML 不算代码；但如果这一轮同时改了代码和文档，按代码那次记录即可。

不要攒着以后补：**记录是改动的一部分**，和改代码同一次回答完成。

## 怎么跑

在仓库根目录执行（PowerShell 5.1 需要 `-ExecutionPolicy Bypass`）：

```powershell
powershell -ExecutionPolicy Bypass -File Tools\Agent\change-log.ps1 `
  -Summary "一句话说明改了什么" `
  -Detail "为什么改、影响哪些功能、怎么验证的（可多行）"
```

- 默认范围：**工作区相对 HEAD 的全部改动**（已暂存 + 未暂存 + 未跟踪），自动忽略 `DescriptionOfChanges/`、`Library/`、`Logs/`、`Builds/`。
- 只记某几个文件：加 `-Files Assets/Scripts/Relic/RelicSystem.cs,Assets/Scripts/Core/QualityWeights.cs`。
- 已经提交完了要补记：加 `-Commit <sha>`。
- 想把文档也带上：加 `-AllFiles`。

## 写什么

- **Summary**：动词开头的一句话，和 git 提交信息同风格（`新增遗物：紫色筹码翻倍`、`修复转动逻辑判断`）。它会变成文件名的一部分。
- **Detail**：至少写清「为什么改」和「怎么验证的」。这段是给人看的，不要复述 diff。
- **一次逻辑改动一条记录**：同一轮里改了三处但属于同一件事 → 跑一次、合并描述；确实是三件独立的事 → 跑三次，每次配 `-Files`。

## 脚本会产出什么

文件：`DescriptionOfChanges/<MM>/<yyyy-MM-dd>.md`（`MM` 是两位月份，如 `10`）。

**一天一个文件**：同一天的多次改动全部追加进同一个文件，用 `---` 分隔成 `## 变更 1`、`## 变更 2`……，
脚本会自己读出现有编号接着写，不需要你手动改。每条变更固定包含：

- 变更标题（`## 变更 N · HH:mm:ss (+09:00)`）；
- 摘要、范围、分支；
- 涉及脚本清单；
- 每个文件的**具体代码片段**：修改的文件给 unified diff，新增文件给前 60 行摘录，删除文件给原内容；
- 你的说明段落。

## 失败与边界

- 脚本退出码 `1` 且提示「没有检测到需要记录的代码改动」→ 说明这轮确实没动代码，忽略即可。
- 退出码 `1` 且提示找不到 git 仓库 → 你在错误的目录，回仓库根再跑；**不要**伪造记录文件。
- 不要在 `DescriptionOfChanges/` 里手写/改文件；只由脚本生成，避免格式漂移。
- 记录文件要跟着代码一起提交（同一次 commit），不要单独攒成"日志提交"。
