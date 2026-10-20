# GamblingGame — Agent 工作说明

> 放在工程根目录，DSH 会自动加载。以下事实来自对本工程的实际扫描（Unity 2022.3.62f3 / QFramework v1.0.236）。
> 改了版本、目录或构建方式，请同步改这份文件，否则后续判断会基于过期信息。

## 0. 环境事实

- Unity：`2022.3.62f3`，Editor：`D:\Unity\unity\2022.3.62f3\Editor\Unity.exe`
- 工程根：`D:\GitProject\GamblingGame`
- 当前分支：`日本版本`；远端还有 `main`（origin/HEAD）、`Dev`、`Test_word`
- 渲染：内置管线 + 2D（`com.unity.feature.2d`）；UI 用 uGUI + TextMeshPro
- 主场景：`Assets/Scenes/GamblingMain.unity`（Build Settings 第 1 项，共 5 个启用场景）
- 框架：QFramework `v1.0.236`（`Architecture` / `System` / `ViewController` / UIKit / ResKit / BindableProperty）、UniRx、Unidux
- 程序集：游戏代码在默认 `Assembly-CSharp`；纯逻辑在 `Assets/Scripts/Core`（`Gambling.Core`，`noEngineReferences`，测试可直接引用）；编辑器工具在 `Assets/Editor`（默认 `Assembly-CSharp-Editor`）
- 测试：`com.unity.test-framework 1.1.33`；`Assets/Tests/EditMode`（程序集 `Gambling.Tests.EditMode`）引用 `Gambling.Core`，当前 33 个 EditMode 用例
- 构建：入口 `Assets/Editor/AgentCI.cs`（`AgentTools.Editor.AgentCI.BuildWindows`），产物 `Builds/StandaloneWindows64/Game.exe`（`Builds/` 已在 .gitignore）
- 包管理：`Packages/manifest.json`，无 URP/HDRP
- MCP 桥：`com.coplaydev.unity-mcp` 10.3.0；Editor 内 HTTP 服务 `http://127.0.0.1:8080/mcp`（49 个工具，`/health` 探活）。DSH 侧用 `transport: streamable-http` 接入，详见 `Docs/Unity-MCP-Workflow.md`

## 1. 验证命令（改完必跑）

```powershell
# 编译检查（必须先关掉 Unity Editor）
powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-ci.ps1 -Task check

# EditMode 测试（33 个用例，退出码 0=全绿；2 只表示环境问题）
powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-ci.ps1 -Task test -TestPlatform EditMode

# 构建（入口 Assets/Editor/AgentCI.cs，产物 Builds/StandaloneWindows64/Game.exe）
powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-ci.ps1 -Task build

# QFramework 合规检查（核心玩法改完必跑，见 §3.1）
powershell -ExecutionPolicy Bypass -File Tools\Agent\qframework-lint.ps1
```

退出码：`0` 通过 / `1` 失败（编译错、测试红、构建失败）/ `2` 环境问题（找不到 Unity、工程被占用、没有测试用例）。

Editor 开着时不能跑 batchmode —— 那种情况下改用 MCP 桥（`mcp__unity__*` 工具）看 Console、跑测试、截图。
MCP 桥自检（不依赖 AI 客户端）：

```powershell
powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-mcp.ps1 -Action health
powershell -ExecutionPolicy Bypass -File Tools\Agent\unity-mcp.ps1 -Action tools
```

注意：DSH 的 `workspace-write` 沙箱只放行工程根目录写入，`unity-ci.ps1` 会因写不了 `Logs\`、`Library\` 而失败；
跑 batchmode 前先把会话切到完全权限（full access）。

**AssetBundle 模式（重要）**：Editor 当前是 QFramework **模拟模式**（`SimulateAssetBundles = True`）——
改 prefab / 资产**不需要**重建 AB，Editor 直接读 AssetDatabase，立刻能看到效果。
但 **Player 只用真机 AB**：`AgentCI.BuildWindows` 已内置「打包前先重建 AB」，改完资源直接打包不会带旧资源。
代价：模拟模式看不到 AB 打包类问题（资源没打进包、依赖缺失），需要时临时切真机模式抽查一次。

### 改完代码必跑：变更记录

每次新增/修改/删除代码后，**在回复用户之前**生成一条记录（skill `code-change-log` 描述同一件事）：

```powershell
powershell -ExecutionPolicy Bypass -File Tools\Agent\change-log.ps1 `
  -Summary "一句话说明改了什么" `
  -Detail "为什么改、影响什么、怎么验证的"
```

产物：`DescriptionOfChanges\<MM>\<yyyy-MM-dd>.md`（一天一个文件，多次改动用 `---` 分隔成 `## 变更 N`），
含时间、脚本清单、代码片段、说明。记录要和代码**同一次提交**，不要单独攒日志提交；格式细节见 `DescriptionOfChanges/README.md`。

## 2. 目录地图（真实结构）

```
Assets/
  Scripts/
    Core/                        # Gambling.Core（noEngineReferences）：纯逻辑 + 可测算法
      QualityWeights.cs          # 抽卡品质权重规则（第1关纯白，每关+5%）
      WeightedPicker.cs          # 按权重选下标（随机数外部传入，可测）
    Global.cs                    # Architecture<Global>，注册 System，放全局 BindableProperty
    CardItem.cs / RewardData.cs / RewardList.cs / BetMultiplierSystem.cs / ScorePopupManager.cs
    Game/                        # GameManager、GameUIController、GamblingGround…（ViewController + .Designer.cs）
    Relic/                       # RelicData / RelicEffect / IRelicSystem / RelicSystem
      EffectDesign/              # 每个遗物效果一个 ScriptableObject 子类
    Enchantment/                 # EnchantmentSystem + EnchantmentDesign/
    EasterEggEffectSystem/       # 彩蛋逻辑 + EggEffect/
  Art/                           # 美术资源；UI 面板场景在 Art/UI/Scenes/TestUIPanels/
  Resources/
    Relics/                      # 29 个 RelicData 资产（Resources.LoadAll<RelicData>("Relics")）
    Enchantment/  EasterEggData/  Data/（GamblingLevel、CardData）  Prefab/  MDList/
  Scenes/GamblingMain.unity
  Tests/EditMode/                # EditMode 测试（Gambling.Tests.EditMode，引用 Gambling.Core）
  Editor/AgentCI.cs              # 无界面构建入口（菜单 Tools/Agent/构建 Windows 版）
  QFramework/                    # 第三方框架（v1.0.236），不要改
DescriptionOfChanges/            # 代码变更记录，01..12 按月归档（脚本生成，勿手写）
.dsh/skills/code-change-log/     # DSH skill：改完代码自动记录（SKILL.md）
Tools/Agent/                     # unity-ci.ps1 / unity-mcp.ps1 / change-log.ps1
AssetBundles/                    # AB 产物目录
Assets/StreamingAssets/          # 运行时数据
```

## 3. 硬性规则

1. **不改 `Assets/QFramework/**`**。这是 vendored 框架（v1.0.236），升级会整体覆盖；确有必要先在方案里说明。
2. **不手改 `*.Designer.cs`**。文件头是 `// Generate Id:...`，由 QFramework UIKit 代码生成；要改绑定就改面板/生成器。
3. **不手改 `.unity` / `.prefab` / `.asset` / `.mat` 的 YAML**。改结构走 Editor API 或写 `Assets/Editor/` 脚本。
4. **新资源必须带 `.meta`**；移动资源用 `AssetDatabase.MoveAsset`，不要文件系统直接搬。
5. **全局状态只走 `Global`**（`Architecture<Global>` + `BindableProperty<T>`）。不要新增 static 单例或 `FindObjectOfType` 兜底。
6. **系统间通信走事件**：`this.SendEvent(new XxxEvent{...})` + `RegisterEvent`，事件用 `struct`（参考 `BuyRelicSuccessEvent`）。
7. **数值/配置优先放 ScriptableObject**（`Resources/Relics`、`Resources/Data`），不写死在代码里。
8. **一次一个可验证切片**（< 200 行 diff）。大改先拆串行小步。
9. **提交前确认分支**：当前是 `日本版本`，不要顺手把日语文案改动混进 `main`。
10. 不擅自升级 Unity 版本、不增删 UPM 包、不改 `ProjectSettings/`（除非任务明确要求）。
11. **改完代码必须生成变更记录**（`Tools\Agent\change-log.ps1`，见 §1）；记录跟代码同一次提交，不手写 `DescriptionOfChanges/`。
12. **核心玩法代码必须符合 QFramework 框架**（见 §3.1）：状态走 `Global.BindableProperty`、通信用 `struct` 事件、依赖用 `GetSystem<T>()`/序列化引用；提交前跑 `Tools\Agent\qframework-lint.ps1`，**不得新增命中**。
13. **需求必须先成卡再执行**（见 §9）：任何开发需求先落到 `Docs/Requirements/<ID>_<名称>.md` 并跑 `requirement-check.ps1`；缺项时**只能提问补齐**，不得用假设代替需求。
14. **需求验收通过后必须归档**（见 §9）：写 `Docs/Requirements/Archive/<ID>_<名称>.md`，四项必填——改动名称、改动时间、改动内容、后续需要增加和注意的点；归档与代码同一次提交。

## 3.1 QFramework 合规（核心玩法）

> 适用范围：分数/关卡/遗物/附魔/彩蛋/商店/UI 面板等**核心玩法**。
> 纯编辑器工具（`Assets/Editor`）、纯算法（`Gambling.Core`）、测试不受此约束。
> 机械自查：`powershell -ExecutionPolicy Bypass -File Tools\Agent\qframework-lint.ps1`

### 分层映射（本项目现状）

| 角色 | 本工程位置 | 规则 |
|---|---|---|
| Architecture | `Global : Architecture<Global>` | 唯一入口；System 在 `Init()` 注册 |
| System | `RelicSystem` / `EnchantmentSystem`（`AbstractSystem`） | 只能 `this.GetSystem<T>()`，不要 `new` |
| Model | 目前没有 | 需要存档/纯数据状态时用 `AbstractModel` + `GetModel<T>()` |
| Controller | `GamblingGround`、`UIGamePanel` 等（`ViewController, IController`） | `GetArchitecture() => Global.Interface` |
| UI | UIKit `UIPanel` + `partial` + `.Designer.cs` | 面板走 `UIKit.OpenPanel<T>()`；不手改 Designer |

### 硬约束

1. **状态**：全局状态只能放 `Global` 的 `BindableProperty`；禁止 static 字段/单例保存玩法状态。
2. **通信**：System、面板之间用 `struct XxxEvent` + `this.SendEvent(...)` / `this.RegisterEvent<T>(...)`；事件必须是 `struct`；不要互相持有实例直接调用。
3. **取依赖**：`this.GetSystem<T>()` / `this.GetModel<T>()`；禁止 `new XxxSystem()`（`Global.Init()` 里 `RegisterSystem(new Xxx())` 除外）。
4. **资源**：走 `ResLoader` / `ResKit` / UIKit；玩法代码不得新增 `Resources.Load`（存量为 6 处，见 lint）。
5. **找对象**：不得新增 `FindObjectOfType`，改用 `[SerializeField]` 引用或事件（存量为 4 处）。
6. **解绑**：所有 `Register` / `RegisterEvent` 必须 `UnRegisterWhenGameObjectDestroyed(gameObject)` 或成对 `UnRegister`，否则重开面板会重复回调。
7. **数值**：玩法数值放 ScriptableObject（`Resources/Relics`、`Resources/Data`），不写死在代码里。
8. **新状态进 `Global`**：同时给它默认值，并在 `GamblingGround.Init()` 的新开局流程里重置。

### 新增一个 System 的标准步骤

1. 定义接口 `IXxxSystem : ISystem`；
2. 实现 `XxxSystem : AbstractSystem, IXxxSystem`，初始化放 `OnInit()`；
3. `Global.Init()` 里 `this.RegisterSystem<IXxxSystem>(new XxxSystem())`；
4. 使用方 `this.GetSystem<IXxxSystem>()`；
5. 需要通知 UI → 发 `struct` 事件，不要在 System 里找面板。

### 提交前自查

- [ ] `qframework-lint.ps1` 没有新增命中（存量 10 处可暂不处理）
- [ ] 新事件是 `struct`，命名 `XxxEvent`
- [ ] 每个注册都有对应解绑
- [ ] 新状态在 `Global` 且 `Init()` 有默认值
- [ ] 新数值在 ScriptableObject，不在代码里

## 4. 常用配方

### 新增一个遗物（本工程最高频的改动）

1. `Assets/Scripts/Relic/EffectDesign/<Name>.cs`，继承 `RelicEffect`：
   ```csharp
   [CreateAssetMenu(fileName = "XxxEffect", menuName = "Gambling/Effects/中文名")]
   public class XxxEffect : RelicEffect
   {
       [SerializeField] private int value = 3;
       public override string RelicName => "……";
       public override bool CanTrigger(RelicTriggerType t) => t == RelicTriggerType.Passive;
       public override void ExecuteEffect(RelicEffectContext ctx) { /* 改 ctx.gamblingGround / Global */ }
       public override void Reset() { }
   }
   ```
2. 在 `Assets/Resources/Relics/` 新建或复用一个 `RelicData`，把上面的 effect 资产挂到 `relicEffect` 字段，填名称/图标/价格/品质/`unLock`。
3. 需要新的触发时机 → 扩展 `RelicTriggerType`，并在真正发生该时机的调用点调用 `relicSystem.TriggerRelicEffect(...)`。
4. 抽卡概率（品质权重）在 `RelicSystem.CalculateQualityWeights`，改概率属于玩法改动，先确认需求。

### 新增一个附魔

`Enchantment/EnchantmentSystem.cs` + `EnchantmentEffect` + `EnchantmentDesign/*`，资产放 `Assets/Resources/Enchantment`；触发链路与遗物同构。

### 新增 UI 面板

UIKit `ViewController` + `partial` 类，`.Designer.cs` 由 UIKit 生成；面板做成独立场景（参考 `Assets/Art/UI/Scenes/TestUIPanels/`）。

### 需要写单元测试时

测试框架已经接好：`Assets/Tests/EditMode`（`Gambling.Tests.EditMode`）→ 引用 `Assets/Scripts/Core`（`Gambling.Core`）。

- 新算法/纯逻辑写进 `Gambling.Core`（`noEngineReferences`，别 `using UnityEngine`），用例加在 `Assets/Tests/EditMode`；
- `Assembly-CSharp` 里的 MonoBehaviour/QFramework 代码**别为了测试搬程序集**：带 `[SerializeField]` 的类型（如 `BetMultiplierConfig`）一换程序集，场景/预制体里的序列化数据就丢；
- 现成范例：`QualityWeights` / `WeightedPicker`（16 例）、`LevelScoreRule`（6 例）、`EasterEggChargeRule`（11 例），都在 `Assets/Tests/EditMode/`；
- 跑测试：Editor 里用 MCP `run_tests`，或关掉 Editor 跑 `unity-ci.ps1 -Task test`。

## 5. 代码风格

- 命名空间 `Gambling`；文件级 `using` 在最前；tab 缩进；大括号换行（沿用现有文件）。
- UIKit 类用 `partial`，手写逻辑放主文件，绑定放 `.Designer.cs`。
- `Debug.Log` 沿用现有 emoji 风格（`✅` `⚠️` `❌`），但不要放进 `Update`。
- 注释用中文，解释“为什么”；公共 API 用 `<summary>`。
- **不要整体重排既有文件**（格式化噪音会淹没 diff）；只改需要改的行。

## 6. 已知坑

- `Resources/Relics` 里 29 个遗物资产和 effect 资产互相引用：**改 `RelicEffect` 的字段名会让所有资产丢引用**。改名前先加 `[FormerlySerializedAs("旧名")]`。
- `ResKit.Init()` 在 `GameManager.Awake`；AB 资源加载依赖 `AssetBundles/` 目录，别把 Editor 下的路径当成打包后的路径。
- **模拟模式（`SimulateAssetBundles=True`）下 Editor 读的是 AssetDatabase**：改 prefab/资产即时生效、不用重建 AB；
  但这也意味着「资源没打进 AB」这类问题在 Editor 里完全看不出来。打包只在 Player 里用真机 AB，改完资源务必走
  `AgentCI.BuildWindows`（已自动重建 AB），或在真机模式下抽查一次。
- Editor 打开时不能对同一工程跑 batchmode（会报 `another Unity instance is running`）。
- 域重载/编译期间调 MCP 会超时，等编译结束或重试一次。
- DSH 的 `workspace-write` 沙箱只放行工程根目录写入，`Tools/`、`Logs/` 等子目录会被拒（`Access to the path 'agent' is denied`）；跑 batchmode 前切完全权限。
- DSH 侧 MCP 若配成 stdio/uvx，沙箱内 `uvx` 写不了 `%LOCALAPPDATA%\uv\cache`，会静默启动失败（`failOnStartupError: false`）；直接用 8080 的 `streamable-http`。
- DSH profile 里**新增**插件必须写成 `- insert:` 列表；顶层 `- id/name/config` 是「覆盖已有条目」，id 不存在时被静默忽略（不报错、不连接），表现就是「配置看着对，工具不出来」。
- `com.coplaydev.unity-mcp` 带来的 `Newtonsoft.Json` 13.0.0 会压掉 QFramework 内置的 8.0.0（Editor.log 有 Duplicate assembly 警告）；动 JSON 序列化前先确认走的是哪个版本。
- `.idea`（4 个文件）和 `.vscode`（3 个文件）已经有内容被 git 跟踪，新增 IDE 配置不要提交。
- 构建目标以 Build Settings 为准；不要把上次构建产物路径（`D:/轮盘英雄/GamblingGame.exe`）当成固定约定。

## 7. 提交

- 沿用现有习惯：中文短句描述改了什么（如 `新增遗物`、`修复bug【转动逻辑判断】`）。
- 一个提交一件事；纯重构与功能改动分开。
- 提交前 `git status` 只应包含预期文件；出现 `Library/`、大量 `.meta` 变动先停下来排查。

## 8. 团队分工（Agent Teams）

主策划（人类）→ Lead → 程序 / 美术 / 执行策划。完整角色契约见 `Docs/Team-Roles.md`。

| 角色 | 写范围 |
|---|---|
| 程序 | `Assets/Scripts/**`、`Assets/Tests/**`、`Assets/Editor/**` |
| 美术 | `Assets/Art/**`（含 UI prefab/场景）、`Assets/Resources/Prefab/**` |
| 执行策划 | `Assets/Resources/{Relics,Data,Enchantment,EasterEggData}/**`、`Docs/Design/**` |
| Lead | `Tools/`、`.dsh/`、`AGENTS.md`、`Docs/`、跨范围兜底 |

三条硬规则：**写范围不越界**（越界先找 Lead 拆任务）、**同一时间只有一个成员操作 Unity Editor**、
**验收统一由 Lead 跑**（编译 / 测试 / `qframework-lint` / 构建）。

## 9. 需求准入（Requirement Gate）

任何开发需求（新功能 / 改规则 / 调数值 / 改 UI / 修 bug 的变更）都必须**先落成需求卡再开工**。
卡片放 `Docs/Requirements/<ID>_<名称>.md`；ID 前缀：`SYS-` 系统、`GP-` 玩法、`EX-` 表现、`QA-` 测试。

**按级别填写**（级别写在卡片第 1 节，检查脚本按它校验）：

| 级别 | 适用 | 必填 | 模板 |
|---|---|---|---|
| **L0 小细节** | 文案 / UI 间距 / 单个数值 / 一行 bug | 功能 ID、改什么、验收 ≥1 条、影响面、待确认 | `_TEMPLATE_L0.md` |
| **L1 功能改动** | 改一条规则 / 加按钮流程 / 加一个遗物 | §1 基本信息、§2 目的、§3 范围、§4 触发·处理·输出（≥1 条 `[MUST]`）、§9 验收 ≥3 条、§10 待确认 | `_TEMPLATE.md` 只填这几节 |
| **L2 大功能** | 新系统 / 跨模块 / 改存档·经济 | 完整 10 节 + 下面 8 项 + 边界 4 类 + 技术约束 | `_TEMPLATE.md` 全填 |

L2 的 8 个必填项：

1. 功能 ID + 版本 + 状态（Draft/Review/Approved）+ 优先级
2. 设计目的（体验目标与实现规则分开写）
3. 功能范围（本次包含 / 本次不包含 / 前置条件）
4. 需求详情（触发条件 / 输入 / 处理规则 / 输出 / 状态变化 / 数值与公式）
5. 边界与异常（无效输入 / 资源不足 / 重复触发 / 中途退出，至少 4 类）
6. 数据与技术约束（依赖系统 / 必须保持的不变量 / 禁止修改的内容）
7. 验收标准（≥ 3 条可验证的勾选项）
8. 参考与待确认事项（没有就写「无」）

```powershell
# 级别从卡片里读；也可 -Level L0/L1/L2 强制校验
powershell -ExecutionPolicy Bypass -File Tools\Agent\requirement-check.ps1 -Path Docs\Requirements\SYS-002_商店刷新.md
```

- 退出码 `0` → 允许拆任务、进入实现。
- 退出码 `1` → 把缺项清单发回需求方，**只提问不实现**；禁止把假设当正式需求执行。
- 级别判断：**新增一个系统，或改跨系统契约（事件协议、存档结构、经济循环、公共接口）→ L2**；现有系统内部加/改规则（哪怕顺带加了 Global 状态、抛了新事件）→ L1；只改表现或数值 → L0。
- **实施复杂度不等于需求级别**：改了几个文件、拆给几个角色，都不影响级别判定。
- **L0 的门槛是「验收能一句话说清」**，说不清就升 L1；L0 泛滥等于没写。

细则与提问规范见 skill `requirement-contract`；模板与填写示例见 `Docs/Requirements/需求模板_v1.docx` 与 `Docs/Requirements/README.md`。

### 9.1 需求归档（验收后必做）

需求验收通过后，**在回复用户之前**写归档，否则该需求不算收尾：

- 路径：`Docs/Requirements/Archive/<ID>_<名称>.md`（从 `Archive/_TEMPLATE.md` 复制，**所有归档放同一目录**）
- 四项必填（缺一项视为未归档）：

| # | 字段 | 写什么 |
|---|---|---|
| 1 | 改动名称 | 需求卡链接 + 一句话概括 |
| 2 | 改动时间 | 开始 / 完成 / 参与角色（程序·美术·策划·Lead） |
| 3 | 改动内容 | 实现了哪些 `[MUST]`、改了哪些文件（代码/资产/prefab/文档）、关键实现说明、验证结果 |
| 4 | 后续需要增加和注意的点 | 待补功能、坑（改了会怎样）、可调数值与位置、相关文档 |

归档里的「坑」要从**本次实际踩到的**来（如默认值陷阱、AB 必须重建、数据条数约束），不写通用套话。
归档与代码同一次提交；示例见 `Docs/Requirements/Archive/SYS-001_彩蛋保底.md`。



