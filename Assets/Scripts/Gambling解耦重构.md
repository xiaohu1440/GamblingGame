#### 核心架构方法
- **`GetArchitecture()`**: 实现 `IController` 接口，返回全局架构实例，用于访问 QFramework 的系统和模型

#### 初始化相关方法
- **`Start()`**: Unity 生命周期方法，初始化所有系统组件
- **`Init()`**: 初始化游戏数据（分数、筹码、转动券、关卡等）
- **`InitializeCategoryButtons()`**: 建立按钮映射关系（apple、king、watermelon 等）
- **`InitializeButtonCounts()`**: 创建所有按钮的点击计数 `BindableProperty`
- **`TextUpdate()`**: 绑定按钮计数到 UI 文本显示
- **`BindButtonEvents()`**: 为所有押注按钮绑定点击事件
- **`GenerateGrid()`**: 生成转盘网格（清空、克隆、布局）
- **`ReGenerateGrid()`**: 重新生成网格（用于关卡切换）

#### 网格布局相关方法
- **`ClearCurrentGrids()`**: 清空当前所有网格项
- **`CloneGridItems()`**: 根据奖励池克隆卡片预制体
- **`ApplyDynamicLayout()`**: 应用动态布局（计算位置、间距）
- **`CalculateGridPosition(int index, float spacing)`**: 计算指定索引的网格位置
- **`GetGridCount()`**: 获取网格数量
- **`SetGridSpacing()`**: 计算网格间距

#### 押注相关方法
- **`OnRewardNameButtonClick(string rewardName)`**: 处理押注按钮点击（增加计数、扣除筹码）
- **`GetRewardNameCategory(string rewardName)`**: 根据奖励名称获取按钮类别
- **`ResetButtonCounts()`**: 重置所有按钮计数为 0
- **`GetButtonClickCount(string category)`**: 获取指定类别的点击次数
- **`SaveCurrentButtonCounts()`**: 保存当前所有按钮计数（用于彩蛋触发前）
- **`HasAnyBet()`**: 检查是否有任何押注
- **`UpdateStartButtonState()`**: 更新开始按钮的可交互状态

#### 转盘核心方法
- **`OnStartButtonClick()`**: 处理开始按钮点击（触发遗物效果、开始转动）
- **`StartSpin()`**: 开始转动（禁用按钮、随机目标、调用 `SpinToPosition`）
- **`SpinToPosition(int targetIndex)`**: 转动到指定位置（计算步数、调用 `StartContinuousMovement`）
- **`GetStepsToTarget(int fromIndex, int toIndex)`**: 计算从当前索引到目标索引的步数
- **`StartContinuousMovement(int totalSteps, int finalTargetIndex)`**: **核心动画方法**（详见下文）
- **`CalculateStepDuration(int currentStep, int totalSteps)`**: 计算每步的持续时间（加速→匀速→减速）
- **`OnSpinComplete(int selectedIndex)`**: 转动完成回调（计算得分、触发遗物效果）

#### 权重随机相关方法
- **`GetWeightedRandomIndex()`**: 根据卡片权重随机选择索引
- **`GetWeightedRandomColorIndex()`**: 根据颜色权重随机选择颜色索引
- **`CalculateCardWeights()`**: 计算所有卡片的权重

#### 按钮状态控制方法
- **`DisableAllButtons()`**: 禁用所有按钮（转动时、彩蛋执行时）
- **`EnableAllButtons()`**: 启用所有按钮（转动结束、彩蛋结束）
- **`ResetGambling()`**: 重置赌博状态（启用按钮、重置倍数）

#### 彩蛋相关方法
- **`SetEasterEggExecuting(bool executing)`**: 设置彩蛋执行状态（控制按钮、检查游戏结束）
- **`IsCurrentlyEasterEggTriggering(CardItem card)`**: 检查当前卡片是否触发彩蛋
- **`AddTemporarySelectBox(GameObject selectBox)`**: 添加临时框选框（彩蛋效果）
- **`SetCurrentSelectBox(GameObject newSelectBox, int newIndex)`**: 设置当前框选框
- **`ClearTemporarySelectBoxes()`**: 清除所有临时框选框

#### UI 相关方法
- **`ShowSingleScorePopup(string category, int score)`**: 显示单个积分弹出动画
- **`GetButtonPosition(string category)`**: 获取按钮的世界坐标位置
- **`ScoreShow()`**: 显示分数（可能用于调试）

#### 倍率系统方法
- **`GetBetMultiplier(string category)`**: 获取指定类别的押注倍率
- **`GetBetProgressInfo(string category)`**: 获取指定类别的押注进度信息
- **`GetAllBetProgressInfo()`**: 获取所有类别的押注进度信息

#### 遗物系统方法
- **`TriggerRotationEndRelics(int selectedIndex, CardItem card)`**: 触发转动结束类型的遗物效果

---

### StartContinuousMovement 方法详解

#### 方法签名
```csharp
private void StartContinuousMovement(int totalSteps, int finalTargetIndex)
```

#### 参数说明
- **`totalSteps`**: 总移动步数 = 转动圈数 × 网格数量 + 到目标的步数
- **`finalTargetIndex`**: 最终停留的目标索引

#### 核心功能
这是转盘动画的核心方法，使用 **DOTween** 创建连续移动动画序列，实现转盘的视觉效果。

#### 详细执行流程

##### 1. 初始化动画序列（第379-382行）
```csharp
Sequence moveSequence = DOTween.Sequence();
int tempCurrentIndex = currentIndex;
cardItems[tempCurrentIndex].GetCardBaseImage().color = Color.white;
int previousIndex = -1; // 记录上一个格子的索引
```
- 创建 DOTween 序列对象
- 保存当前索引到临时变量
- 重置当前卡片颜色为白色
- 初始化上一个索引为 -1

##### 2. 循环创建每一步动画（第385-427行）
```csharp
for (int step = 0; step < totalSteps; step++)
{
    // 计算下一个索引（循环）
    int nextIndex = (tempCurrentIndex + 1) % gridRects.Count;
    
    // 根据剩余步数调整移动速度（最后几步减速）
    float stepDuration = CalculateStepDuration(step, totalSteps);
    
    // 添加移动到下一个位置的动画
    moveSequence.Append(selectBoxRect.DOAnchorPos(gridRects[nextIndex].anchoredPosition, stepDuration)
        .SetEase(Ease.InOutQuad));
```

**关键点：**
- **循环索引计算**: `(tempCurrentIndex + 1) % gridRects.Count` 实现环形移动
- **动态速度**: `CalculateStepDuration` 实现加速→匀速→减速效果
    - 前 30% 步数：加速阶段
    - 中间 40% 步数：匀速阶段
    - 后 30% 步数：减速阶段
- **缓动函数**: `Ease.InOutQuad` 使移动更平滑

##### 3. 颜色变化回调（第397-422行）
```csharp
int currentStepIndex = nextIndex; // 捕获当前步骤的索引
int prevStepIndex = previousIndex; // 捕获上一个步骤的索引

moveSequence.AppendCallback(() =>
{
    // 重置上一个格子的颜色为白色
    if (prevStepIndex >= 0 && prevStepIndex < cardItems.Count)
    {
        Image prevBaseImage = cardItems[prevStepIndex].GetCardBaseImage();
        if (prevBaseImage != null)
        {
            prevBaseImage.color = Color.white;
        }
    }

    // 使用加权随机改变当前格子的颜色
    if (currentStepIndex >= 0 && currentStepIndex < cardItems.Count)
    {
        Image baseImage = cardItems[currentStepIndex].GetCardBaseImage();
        if (baseImage != null)
        {
            int randomColorIndex = GetWeightedRandomColorIndex();
            baseImage.color = colorConfigs[randomColorIndex].color;
        }
    }
});
```

**关键点：**
- **闭包捕获**: 必须在循环外捕获 `nextIndex` 和 `previousIndex`，避免闭包问题
- **颜色重置**: 将上一个格子恢复为白色
- **随机着色**: 使用加权随机为当前格子选择颜色（白色、绿色、蓝色等）
- **视觉反馈**: 营造"转盘正在转动"的视觉效果

##### 4. 动画完成回调（第430-580行）
```csharp
moveSequence.OnComplete(() =>
{
    currentIndex = finalTargetIndex;
    isSpinning = false;
    
    // 使用加权随机选择最终颜色并设置奖励分数
    int finalColorIndex = GetWeightedRandomColorIndex();
    finalColorBonusScore = colorConfigs[finalColorIndex].score;
    
    // 设置最终停留位置的卡片底板颜色（带动画效果）
    if (finalTargetIndex >= 0 && finalTargetIndex < cardItems.Count)
    {
        Image baseImage = cardItems[finalTargetIndex].GetCardBaseImage();
        if (baseImage != null)
        {
            baseImage.DOColor(colorConfigs[finalColorIndex].color, 0.3f).SetEase(Ease.OutQuad);
        }
    }

    OnSpinComplete(finalTargetIndex);
    
    // 处理彩蛋触发和游戏结束逻辑...
});
```

**关键点：**
- **更新状态**: 更新 `currentIndex` 和 `isSpinning`
- **最终着色**: 为停留位置的卡片设置最终颜色（带 0.3 秒渐变动画）
- **颜色奖励**: 根据最终颜色设置额外分数（0-30 分）
- **触发回调**: 调用 `OnSpinComplete` 处理得分计算
- **彩蛋检测**: 检查是否触发彩蛋效果
- **游戏流程**: 处理筹码购买、游戏结束判断等

#### 动画效果总结
1. **框选框移动**: 从当前位置顺时针移动到目标位置
2. **速度变化**: 先加速、匀速、再减速，模拟真实转盘
3. **颜色闪烁**: 每经过一个格子，随机改变颜色，增强视觉效果
4. **最终高亮**: 停留位置的卡片以特定颜色高亮显示
5. **额外奖励**: 根据最终颜色获得 0-30 分的额外奖励

---

### 使用 QFramework 解耦优化建议

#### 当前问题分析

**1. 职责过重**
- `GamblingGround` 类有 1282 行代码，承担了太多职责
- 混合了 UI 控制、游戏逻辑、动画、状态管理等

**2. 耦合度高**
- 直接操作 UI 组件（Button、Image、Text）
- 直接访问 `Global` 静态类
- 动画逻辑与业务逻辑混在一起

**3. 难以测试和维护**
- 方法之间相互调用复杂
- 状态管理分散在各个方法中
- 难以单独测试某个功能

#### 优化方案：基于 QFramework 的架构重构

##### 1. 创建专门的 Model（数据层）

```csharp
// 游戏状态模型
public class GamblingGameModel : AbstractModel
{
    public BindableProperty<int> Score { get; private set; } = new BindableProperty<int>(0);
    public BindableProperty<int> Chips { get; private set; } = new BindableProperty<int>(5);
    public BindableProperty<int> LotteryTicket { get; private set; } = new BindableProperty<int>(10);
    public BindableProperty<int> CurrentLevel { get; private set; } = new BindableProperty<int>(1);
    public BindableProperty<int> LevelScore { get; private set; } = new BindableProperty<int>(10);
    public BindableProperty<bool> IsSpinning { get; private set; } = new BindableProperty<bool>(false);
    public BindableProperty<bool> IsEasterEggExecuting { get; private set; } = new BindableProperty<bool>(false);
    
    // 按钮点击计数
    public Dictionary<string, BindableProperty<int>> ButtonClickCounts { get; private set; }
    
    protected override void OnInit()
    {
        ButtonClickCounts = new Dictionary<string, BindableProperty<int>>
        {
            ["apple"] = new BindableProperty<int>(0),
            ["king"] = new BindableProperty<int>(0),
            // ... 其他按钮
        };
    }
}
```

##### 2. 创建专门的 System（业务逻辑层）

```csharp
// 转盘系统
public interface ISpinSystem : ISystem
{
    void StartSpin(int targetIndex);
    int CalculateSteps(int fromIndex, int toIndex, int rotations);
    void CompleteSpin(int finalIndex);
}

public class SpinSystem : AbstractSystem, ISpinSystem
{
    protected override void OnInit()
    {
    }
    
    public void StartSpin(int targetIndex)
    {
        var model = this.GetModel<GamblingGameModel>();
        if (model.IsSpinning.Value) return;
        
        model.IsSpinning.Value = true;
        // 发送转盘开始事件
        this.SendEvent<SpinStartEvent>();
    }
    
    public int CalculateSteps(int fromIndex, int toIndex, int rotations)
    {
        int gridCount = this.GetSystem<IGridSystem>().GetGridCount();
        int stepsToTarget = toIndex >= fromIndex 
            ? toIndex - fromIndex 
            : gridCount - fromIndex + toIndex;
        return rotations * gridCount + stepsToTarget;
    }
    
    public void CompleteSpin(int finalIndex)
    {
        var model = this.GetModel<GamblingGameModel>();
        model.IsSpinning.Value = false;
        
        // 发送转盘完成事件
        this.SendEvent(new SpinCompleteEvent { FinalIndex = finalIndex });
    }
}

// 押注系统
public interface IBetSystem : ISystem
{
    bool PlaceBet(string category);
    void ResetBets();
    int GetBetCount(string category);
    int GetBetMultiplier(string category);
}

public class BetSystem : AbstractSystem, IBetSystem
{
    private BetMultiplierSystem multiplierSystem;
    
    protected override void OnInit()
    {
        multiplierSystem = new BetMultiplierSystem(/* configs */);
    }
    
    public bool PlaceBet(string category)
    {
        var model = this.GetModel<GamblingGameModel>();
        
        // 检查筹码是否足够
        if (model.Chips.Value <= 0)
        {
            Debug.LogWarning("筹码不足！");
            return false;
        }
        
        // 扣除筹码
        model.Chips.Value -= 1;
        
        // 增加押注计数
        model.ButtonClickCounts[category].Value++;
        
        // 发送押注事件
        this.SendEvent(new BetPlacedEvent { Category = category });
        
        return true;
    }
    
    public void ResetBets()
    {
        var model = this.GetModel<GamblingGameModel>();
        foreach (var kvp in model.ButtonClickCounts)
        {
            kvp.Value.Value = 0;
        }
    }
    
    public int GetBetCount(string category)
    {
        var model = this.GetModel<GamblingGameModel>();
        return model.ButtonClickCounts[category].Value;
    }
    
    public int GetBetMultiplier(string category)
    {
        int betCount = GetBetCount(category);
        return multiplierSystem.GetMultiplier(betCount);
    }
}

// 得分系统
public interface IScoreSystem : ISystem
{
    void AddScore(int score);
    void CalculateAndAddScore(int baseScore, string category, int colorBonus);
    bool IsLevelComplete();
}

public class ScoreSystem : AbstractSystem, IScoreSystem
{
    public void AddScore(int score)
    {
        var model = this.GetModel<GamblingGameModel>();
        model.Score.Value += score;
        
        this.SendEvent(new ScoreChangedEvent { NewScore = model.Score.Value });
    }
    
    public void CalculateAndAddScore(int baseScore, string category, int colorBonus)
    {
        var betSystem = this.GetSystem<IBetSystem>();
        var model = this.GetModel<GamblingGameModel>();
        
        int betCount = betSystem.GetBetCount(category);
        int multiplier = betSystem.GetBetMultiplier(category);
        
        int finalScore = (baseScore + colorBonus) * multiplier * betCount;
        AddScore(finalScore);
    }
    
    public bool IsLevelComplete()
    {
        var model = this.GetModel<GamblingGameModel>();
        return model.Score.Value >= model.LevelScore.Value;
    }
}

// 资源管理系统（筹码、彩票）
public interface IResourceSystem : ISystem
{
    bool TryBuyChipsWithTickets();
    bool HasEnoughChips(int amount);
    bool HasEnoughTickets(int amount);
}

public class ResourceSystem : AbstractSystem, IResourceSystem
{
    private const int CHIPS_PER_PURCHASE = 5;
    private const int TICKETS_PER_PURCHASE = 2;
    
    public bool TryBuyChipsWithTickets()
    {
        var model = this.GetModel<GamblingGameModel>();
        
        if (model.Chips.Value > 0) return false;
        if (model.LotteryTicket.Value < TICKETS_PER_PURCHASE) return false;
        
        model.LotteryTicket.Value -= TICKETS_PER_PURCHASE;
        model.Chips.Value += CHIPS_PER_PURCHASE;
        
        this.SendEvent<ChipsPurchasedEvent>();
        return true;
    }
    
    public bool HasEnoughChips(int amount)
    {
        var model = this.GetModel<GamblingGameModel>();
        return model.Chips.Value >= amount;
    }
    
    public bool HasEnoughTickets(int amount)
    {
        var model = this.GetModel<GamblingGameModel>();
        return model.LotteryTicket.Value >= amount;
    }
}
```

##### 3. 创建 Command（命令层）

```csharp
// 开始转动命令
public class StartSpinCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        var spinSystem = this.GetSystem<ISpinSystem>();
        var gridSystem = this.GetSystem<IGridSystem>();
        
        // 随机选择目标
        int targetIndex = gridSystem.GetWeightedRandomIndex();
        
        // 开始转动
        spinSystem.StartSpin(targetIndex);
        
        // 触发遗物效果
        this.GetSystem<IRelicSystem>().TriggerRelicEffect(
            RelicTriggerType.OnRotationStart, 
            new RelicEffectContext()
        );
    }
}

// 押注命令
public class PlaceBetCommand : AbstractCommand
{
    private string category;
    
    public PlaceBetCommand(string category)
    {
        this.category = category;
    }
    
    protected override void OnExecute()
    {
        var betSystem = this.GetSystem<IBetSystem>();
        betSystem.PlaceBet(category);
    }
}

// 完成转动命令
public class CompleteSpinCommand : AbstractCommand
{
    private int finalIndex;
    
    public CompleteSpinCommand(int finalIndex)
    {
        this.finalIndex = finalIndex;
    }
    
    protected override void OnExecute()
    {
        var spinSystem = this.GetSystem<ISpinSystem>();
        var scoreSystem = this.GetSystem<IScoreSystem>();
        var resourceSystem = this.GetSystem<IResourceSystem>();
        
        // 完成转动
        spinSystem.CompleteSpin(finalIndex);
        
        // 计算得分
        // ... 得分逻辑
        
        // 检查是否需要购买筹码
        resourceSystem.TryBuyChipsWithTickets();
        
        // 检查游戏是否结束
        this.SendCommand<CheckGameOverCommand>();
    }
}
```

##### 4. 创建 Event（事件层）

```csharp
// 转盘开始事件
public struct SpinStartEvent
{
}

// 转盘完成事件
public struct SpinCompleteEvent
{
    public int FinalIndex;
}

// 押注事件
public struct BetPlacedEvent
{
    public string Category;
}

// 得分变化事件
public struct ScoreChangedEvent
{
    public int NewScore;
}

// 筹码购买事件
public struct ChipsPurchasedEvent
{
}
```

##### 5. 重构 ViewController（视图层）

```csharp
public partial class GamblingGround : ViewController, IController
{
    public IArchitecture GetArchitecture() => Global.Interface;
    
    // 只保留 UI 引用
    [SerializeField] private RectTransform selectBoxRect;
    [SerializeField] private Transform GridContainer;
    // ... 其他 UI 引用
    
    private void Start()
    {
        InitializeUI();
        BindEvents();
    }
    
    private void InitializeUI()
    {
        // 只负责 UI 初始化
    }
    
    private void BindEvents()
    {
        // 监听模型变化
        this.GetModel<GamblingGameModel>().Score
            .RegisterWithInitValue(OnScoreChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        
        this.GetModel<GamblingGameModel>().Chips
            .RegisterWithInitValue(OnChipsChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        
        // 监听事件
        this.RegisterEvent<SpinStartEvent>(OnSpinStart)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        
        this.RegisterEvent<SpinCompleteEvent>(OnSpinComplete)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        
        // 绑定按钮点击
        Apple.GetComponent<Button>().onClick.AddListener(() => 
            this.SendCommand(new PlaceBetCommand("apple")));
    }
    
    private void OnScoreChanged(int newScore)
    {
        // 更新 UI 显示
    }
    
    private void OnChipsChanged(int newChips)
    {
        // 更新 UI 显示
    }
    
    private void OnSpinStart(SpinStartEvent e)
    {
        // 播放动画
        StartSpinAnimation();
    }
    
    private void OnSpinComplete(SpinCompleteEvent e)
    {
        // 停止动画
    }
}
```

##### 6. 创建独立的动画控制器

```csharp
// 转盘动画控制器
public class SpinAnimationController : MonoBehaviour
{
    [SerializeField] private RectTransform selectBoxRect;
    [SerializeField] private List<CardItem> cardItems;
    [SerializeField] private List<RectTransform> gridRects;
    
    public void PlaySpinAnimation(int totalSteps, int finalIndex, Action onComplete)
    {
        Sequence moveSequence = DOTween.Sequence();
        int tempCurrentIndex = currentIndex;
        
        // ... 动画逻辑（从 StartContinuousMovement 移过来）
        
        moveSequence.OnComplete(() => onComplete?.Invoke());
    }
    
    public void SetCardColor(int index, Color color)
    {
        if (index >= 0 && index < cardItems.Count)
        {
            cardItems[index].GetCardBaseImage().color = color;
        }
    }
}
```

#### 优化后的优势

##### 1. 职责清晰
- **Model**: 只负责数据存储
- **System**: 只负责业务逻辑
- **Command**: 只负责协调多个 System
- **Event**: 只负责通知
- **ViewController**: 只负责 UI 显示和用户交互

##### 2. 低耦合
- 各层之间通过接口通信
- 不直接访问 `Global` 静态类
- UI 和逻辑完全分离

##### 3. 易测试
- 可以单独测试每个 System
- 可以 Mock 依赖
- 不依赖 Unity 组件

##### 4. 易维护
- 修改某个功能只需修改对应的 System
- 添加新功能不影响现有代码
- 代码结构清晰，易于理解

##### 5. 可复用
- System 可以在其他场景复用
- Command 可以组合使用
- Event 可以被多个监听者订阅

#### 重构步骤建议

1. **第一步**: 创建 `GamblingGameModel`，将所有数据移到 Model 中
2. **第二步**: 创建 `BetSystem`，将押注逻辑移到 System 中
3. **第三步**: 创建 `ScoreSystem`，将得分逻辑移到 System 中
4. **第四步**: 创建 `SpinSystem`，将转盘逻辑移到 System 中
5. **第五步**: 创建 Command，替换原有的方法调用
6. **第六步**: 创建 Event，实现事件驱动
7. **第七步**: 重构 `GamblingGround`，只保留 UI 相关代码
8. **第八步**: 创建独立的动画控制器

#### 注意事项

- **渐进式重构**: 不要一次性重构所有代码，逐步迁移
- **保持测试**: 每次重构后都要测试功能是否正常
- **保留原代码**: 重构时可以先注释原代码，确保新代码正常后再删除
- **文档更新**: 及时更新代码注释和文档