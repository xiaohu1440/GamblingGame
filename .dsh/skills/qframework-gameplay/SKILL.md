---
name: qframework-gameplay
description: 用 QFramework 的架构写 GamblingGame 的核心玩法代码。改动分数、关卡、遗物、附魔、彩蛋、商店、UI 面板等玩法系统时，必须按 Architecture / System / Model / Controller 分层，状态走 Global.BindableProperty，通信用 struct 事件，资源走 ResKit，并在提交前跑 qframework-lint。当任务涉及"改玩法逻辑、加系统、加事件、加全局状态、加面板"时使用。
whenToUse: 只要你在动 Assets/Scripts 或 Assets/Art/UI/Scripts 里的玩法代码；纯工具、纯算法、文档不用。
---

# QFramework 玩法开发约束

这个工程是 QFramework 架构：`Global : Architecture<Global>` 是唯一入口，System 注册在里面，全局状态是
`BindableProperty<T>`，UI 是 UIKit 面板。**核心玩法代码不得绕开这套结构**——出现 `FindObjectOfType`、
`new XxxSystem()`、static 状态、`Resources.Load`，就算功能能跑，也算不合规。

规则的上位说明在 `AGENTS.md §3.1`；本 skill 是执行细节。两者冲突时以 AGENTS.md 为准。

## 先认清四个角色

| 角色 | 本工程位置 | 写法 |
|---|---|---|
| Architecture | `Assets/Scripts/Global.cs` | `public class Global : Architecture<Global>`，`Init()` 里 `RegisterSystem<T>()` |
| System | `RelicSystem` / `EnchantmentSystem` | `AbstractSystem, IXxxSystem`；`OnInit()` 里初始化；用 `this.GetSystem<IXxxSystem>()` 取 |
| Model | 目前没有 | 需要存档/纯数据时新建 `AbstractModel`，`RegisterModel` + `GetModel<T>()` |
| Controller | `GamblingGround`、`UIGamePanel` 等 | `ViewController, IController` + `GetArchitecture() => Global.Interface` |

UI 面板：`UIPanel` + `partial` + `.Designer.cs`（生成文件，**不要手改**）；打开/关闭一律 `UIKit.OpenPanel<T>()` / `ClosePanel<T>()`。

## 三条线，别混

1. **状态线**：跨系统、跨关卡的数值 → `Global` 的 `BindableProperty`；只属于某个对象的临时状态 → 留在该对象里。
2. **通信线**：System 想通知 UI / 其他系统 → `struct` 事件（`this.SendEvent(new XxxEvent{...})`，UI 侧 `this.RegisterEvent<XxxEvent>(...)`）。**不要**在 System 里引用面板或 Controller 实例。
3. **资源线**：资产加载 → `ResLoader` / `ResKit`；面板 → UIKit；玩法代码不写 `Resources.Load`。

## 新增一个 System（照抄这个顺序）

```csharp
// 1. 接口
public interface IXxxSystem : ISystem
{
    void DoSomething(int value);
}

// 2. 实现
public class XxxSystem : AbstractSystem, IXxxSystem
{
    protected override void OnInit()
    {
        // 初始化：读配置、订阅事件都可以，但不要找 GameObject
    }

    public void DoSomething(int value)
    {
        this.SendEvent(new XxxChangedEvent { Value = value }); // 通知 UI
    }
}

// 3. 注册（Global.Init）
this.RegisterSystem<IXxxSystem>(new XxxSystem());

// 4. 使用
var system = this.GetSystem<IXxxSystem>();
```

**反例**：`var s = new XxxSystem();`、`FindObjectOfType<GamblingGround>()`、`Global.instance`。

## 事件必须是 struct

```csharp
public struct XxxChangedEvent
{
    public int Value;
}
```

注册侧一定要解绑，否则面板反复打开会重复回调：

```csharp
this.RegisterEvent<XxxChangedEvent>(OnChanged)
    .UnRegisterWhenGameObjectDestroyed(gameObject);
```

## 新增全局状态

```csharp
// Global.cs
public static BindableProperty<int> xxx = new BindableProperty<int>(0);
```

同时做两件事，缺一不可：
1. 给默认值（上面括号里）；
2. 在 `GamblingGround.Init()` 的新开局流程里重置，避免上一局残留。

## 数值不要写死

玩法数值放 ScriptableObject（`Resources/Relics`、`Resources/Data`），代码里只读不硬编码；
纯计算规则抽到 `Gambling.Core` 并配 EditMode 测试（参考 `QualityWeights` / `LevelScoreRule`）。

## 提交前必须做的检查

```powershell
powershell -ExecutionPolicy Bypass -File Tools\Agent\qframework-lint.ps1
```

- 退出码 0，且**你这次没有新增命中**（存量 10 处：`FindObjectOfType` 4、`Resources.Load` 6，可暂不处理）。
- 编译 0 error（Editor 里 `refresh_unity` 后读 Console）。
- 有逻辑改动就跑测试（`run_tests` 或 `unity-ci.ps1 -Task test`）。
- 最后按 `code-change-log` skill 生成变更记录。

## 常见错误（本工程真实踩过）

| 错误 | 后果 | 正确做法 |
|---|---|---|
| 在 `System` 里 `FindObjectOfType<GamblingGround>()` | 依赖隐式、场景一换就崩 | 发事件或把需要的值放进事件体 |
| 面板里 `ReactiveProperty.Value = x` 却不绑定 UI | 数据变了界面不动 | UI 文本在 `RegisterWithInitValue` 里更新 |
| `Register` 没解绑 | 面板重开时回调叠加、分数跳变 | `UnRegisterWhenGameObjectDestroyed` |
| 全局状态用 static 字段 | 多系统读写顺序不可控 | `Global.BindableProperty` |
| 直接 `new` 一个 System | 绕过 `OnInit`、事件收不到 | `RegisterSystem` + `GetSystem` |
| 把玩法数值写死在代码里 | 改数值要改代码 | ScriptableObject |
