# 开发团队分工（Agent Teams）

> 组织：**主策划（你）→ 技术负责人 Lead（我）→ 程序 / 美术 / 执行策划**。
> 这份文档是角色契约；具体工程规则仍以 `AGENTS.md` 为准，玩法代码规则见 `AGENTS.md §3.1`。

## 角色与写范围

| 角色 | 负责什么 | 可写范围（硬边界） | 不碰 |
|---|---|---|---|
| **主策划**（人类） | 决定做什么、优先级、验收标准；拍板玩法取舍 | 只通过 Lead 下达，不直接改文件 | — |
| **Lead**（技术负责人） | 拆任务、定范围、派发、串行化 Editor、审核 diff、跑验收、汇总 | `Tools/`、`.dsh/`、`AGENTS.md`、`Docs/`、跨范围兜底 | 不替执行者写其范围内的实现 |
| **程序** | 玩法逻辑、System、UI 面板逻辑、编辑器工具、测试 | `Assets/Scripts/**`、`Assets/Tests/**`、`Assets/Editor/**` | `Assets/Art/**`、`Resources` 数值资产 |
| **美术** | UI 布局、prefab/场景结构、导入设置、图标材质、视觉验收 | `Assets/Art/**`（含 UI prefab/场景）、`Assets/Resources/Prefab/**` | C# 代码、数值资产 |
| **执行策划** | 把设计落成配置：遗物/附魔/关卡/奖励数值、文案、图标需求清单 | `Assets/Resources/Relics/**`、`Data/**`、`Enchantment/**`、`EasterEggData/**`、`Docs/Design/**` | C# 代码、prefab |

写范围重叠时 **Lead 仲裁**：要么拆成两个任务串行，要么指定唯一写者。

## 协作协议

1. **任务板**：Lead 用 `team_task_create` 建任务，写清验收标准和 `write_scopes`；执行者 claim（拿最新 revision）→ 干活 → complete。
2. **消息**：派活用 `send_message`；完成后回报「改了什么 / 怎么验证的 / 有没有遗留」。
3. **Unity Editor 串行**：同一时间只允许一个人操作 Editor（Play、prefab stage、资产编辑）。
   谁要动 Editor 先在回报里声明占用，用完声明释放；Lead 在派任务时排好顺序。
   多人同时 Play/改 prefab 会互相踩，这是硬约束。
4. **变更记录**：改代码后按 `code-change-log` skill 生成记录；改资产/数值的在任务回报里附「旧值 → 新值」对照表。
5. **验收由 Lead 统一跑**：编译（`refresh_unity` + Console）、EditMode 测试、`qframework-lint`、必要时构建；不通过打回给原执行者。
6. **不确定就上报**：执行者不自行扩大范围；需要改别人范围的文件时，停下来找 Lead 拆任务。

## 一次需求的流水线

```
主策划提需求
  → Lead 确认设计意图、拆成 2–4 个任务（写清验收）
    → 执行策划：数值/文案/资产清单
    → 美术：UI/图标/布局
    → 程序：逻辑实现 + 测试
  → Lead 验证：编译 / 测试 / lint / 构建
  → 主策划验收（或进下一迭代）
```

前后端有依赖时用任务依赖（`blocked_by`）串行，例如「程序实现」等「执行策划给完数值」。

## 什么时候不用团队

- 单文件小修（< 20 行）、纯问答、排查问题 → Lead 直接做，团队协作的开销比收益大。
- 只有一个人能判断的事（比如必须先由主策划拍板的玩法取舍）→ 先问，再排期。

## 角色各自的「必须做」

- **程序**：读 `AGENTS.md` + `.dsh/skills/qframework-gameplay/SKILL.md`；提交前跑 `qframework-lint`，逻辑改动补 EditMode 测试。
- **美术**：prefab 改动必须走 Unity Editor API（MCP），不手改 YAML；改完 **重建 AssetBundle**（否则运行时看不到），并截图自检。
- **执行策划**：数值改动附「旧值 → 新值 + 影响范围」；新增资物/附魔要连同 `Resources` 资产和图标需求一起交付。
- **Lead**：每次合并前看 diff、跑验证、确认分支（`日本版本`）、确认变更记录已生成。
