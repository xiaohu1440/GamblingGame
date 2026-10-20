# 代码变更记录（DescriptionOfChanges）

这里按**月份**归档 DSH（DeepSeek Harness）对工程代码的每一次改动，用来回答三个问题：
**什么时候改的、改了哪个脚本的哪段代码、为什么改**。

## 目录

```
DescriptionOfChanges/
  01/ … 12/          # 两位月份，例如 10 = 十月
  README.md
```

- 月份用 `MM` 两位数字，和脚本 `Get-Date -Format 'MM'` 一致；跨年靠**文件名里的日期**区分（`20261019-…`）。
- 空月份保留 `.gitkeep`，保证目录在 git 里存在。
- 不要在月份目录里手写/改文件，全部由脚本生成，避免格式漂移。

## 文件命名

**一天一个文件**：

```
<yyyy-MM-dd>.md
例：2026-10-19.md
```

同一天的多次改动全部追加进同一个文件，用 `---` 分隔成 `## 变更 1`、`## 变更 2`……
所以一天里改十次也只有一个文件，方便按天回看。

## 文件内容

文件头（只写一次）：

```markdown
# 代码变更记录 · 2026-10-19

> 同一天的多次改动合并在本文件，用 `---` 分隔。
> 由 `Tools/Agent/change-log.ps1` 自动生成，勿手改。
```

每次改动追加一段，固定包含：

| 块 | 内容 |
|---|---|
| 变更标题 | `## 变更 N · HH:mm:ss (+09:00)` |
| 头部表格 | 摘要、范围（工作区/提交/指定文件，几个文件）、分支 |
| 涉及脚本 | 每个文件的路径和状态（`M` 修改 / `??` 新增 / `D` 删除） |
| 说明 | 为什么改、影响范围、怎么验证的 |
| 代码片段 | 修改 = unified diff；新增 = 前 60 行摘录；删除 = 原内容 |


## 怎么生成

```powershell
# 默认：记录工作区相对 HEAD 的全部改动（含未跟踪），追加到当天的文件
powershell -ExecutionPolicy Bypass -File Tools\Agent\change-log.ps1 `
  -Summary "一句话说明改了什么" `
  -Detail "为什么改、影响什么、怎么验证"

# 只记某几个文件
powershell -ExecutionPolicy Bypass -File Tools\Agent\change-log.ps1 -Summary "…" `
  -Files "Assets/Scripts/Relic/RelicSystem.cs,Assets/Scripts/Core/QualityWeights.cs"

# 已经提交了再补记
powershell -ExecutionPolicy Bypass -File Tools\Agent\change-log.ps1 -Summary "…" -Commit <sha>
```

- 默认只记代码类文件（`.cs` `.asmdef` `.shader` `.hlsl` `.cginc` `.uss` `.uxml` `.ps1` `.json`）；
  要连文档一起记，加 `-AllFiles`。
- 自动忽略 `DescriptionOfChanges/`、`Library/`、`Logs/`、`Temp/`、`Builds/`、`obj/`。

## 自动化

两个层面，缺一不可：

1. **Skill `code-change-log`**（`.dsh/skills/code-change-log/SKILL.md`）：DSH 判断这一轮动过代码时加载它，
   按其中的步骤调用脚本。skill 目录被监听，改完下一个回合就生效，不需要重启。
2. **AGENTS.md 硬规则**：每次会话都会自动加载，保证即使 skill 没被选中，规则仍然约束行为。

> 说明：DSH 目前没有"文件保存后触发 shell"的钩子，所以「自动」= **Agent 行为约束 + 一条命令**，
> 不是后台守护进程。记录做不到"零遗漏"层面的事后补记可以用 `-Commit` 把历史提交一条条补出来。

## 和提交的关系

- **记录跟代码一起提交**：改了代码 → 生成记录 → 同一次 commit 带上 `DescriptionOfChanges/` 下的新文件。
- 不要攒成「日志提交」，也不要提交空的占位记录。
- 如果一次改动跨了很多天，按**实际生成记录的时间**归档，不按分支创建时间。
