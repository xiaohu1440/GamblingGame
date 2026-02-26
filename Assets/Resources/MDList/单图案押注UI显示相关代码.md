根据您的需求，我设计了一个解耦的押注倍率系统。以下是完整的实现方案：

---

### 1. 创建独立的倍率系统类

首先创建一个新文件 `BetMultiplierSystem.cs`，用于管理押注倍率逻辑：

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gambling
{
    /// <summary>
    /// 押注倍率配置
    /// </summary>
    [Serializable]
    public class BetMultiplierConfig
    {
        [Tooltip("达到此筹码数量时触发倍率")]
        public int threshold;
        
        [Tooltip("对应的倍率")]
        public int multiplier;
        
        public BetMultiplierConfig(int threshold, int multiplier)
        {
            this.threshold = threshold;
            this.multiplier = multiplier;
        }
    }

    /// <summary>
    /// 押注倍率系统 - 负责计算押注倍率和进度
    /// </summary>
    public class BetMultiplierSystem
    {
        // 倍率配置列表（按阈值从小到大排序）
        private List<BetMultiplierConfig> multiplierConfigs;

        /// <summary>
        /// 构造函数 - 使用默认配置
        /// </summary>
        public BetMultiplierSystem()
        {
            // 默认配置：>10筹码2倍，>20筹码4倍，>30筹码8倍
            multiplierConfigs = new List<BetMultiplierConfig>
            {
                new BetMultiplierConfig(10, 2),
                new BetMultiplierConfig(20, 4),
                new BetMultiplierConfig(30, 8)
            };
        }

        /// <summary>
        /// 构造函数 - 使用自定义配置
        /// </summary>
        /// <param name="configs">自定义倍率配置</param>
        public BetMultiplierSystem(List<BetMultiplierConfig> configs)
        {
            multiplierConfigs = configs ?? new List<BetMultiplierConfig>();
            // 按阈值排序
            multiplierConfigs.Sort((a, b) => a.threshold.CompareTo(b.threshold));
        }

        /// <summary>
        /// 根据押注数量计算倍率
        /// </summary>
        /// <param name="betCount">押注筹码数量</param>
        /// <returns>对应的倍率</returns>
        public int GetMultiplier(int betCount)
        {
            int multiplier = 1; // 默认1倍
            
            // 从后往前遍历，找到最高的满足条件的倍率
            for (int i = multiplierConfigs.Count - 1; i >= 0; i--)
            {
                if (betCount > multiplierConfigs[i].threshold)
                {
                    multiplier = multiplierConfigs[i].multiplier;
                    break;
                }
            }
            
            return multiplier;
        }

        /// <summary>
        /// 获取当前押注的进度信息（用于UI进度条显示）
        /// </summary>
        /// <param name="betCount">当前押注筹码数量</param>
        /// <returns>进度信息</returns>
        public BetProgressInfo GetProgressInfo(int betCount)
        {
            BetProgressInfo info = new BetProgressInfo
            {
                currentBetCount = betCount,
                currentMultiplier = GetMultiplier(betCount)
            };

            // 查找下一个倍率阈值
            for (int i = 0; i < multiplierConfigs.Count; i++)
            {
                if (betCount <= multiplierConfigs[i].threshold)
                {
                    info.nextThreshold = multiplierConfigs[i].threshold;
                    info.nextMultiplier = multiplierConfigs[i].multiplier;
                    info.hasNextLevel = true;
                    
                    // 计算进度百分比
                    int previousThreshold = i > 0 ? multiplierConfigs[i - 1].threshold : 0;
                    int range = info.nextThreshold - previousThreshold;
                    int progress = betCount - previousThreshold;
                    info.progressPercent = range > 0 ? Mathf.Clamp01((float)progress / range) : 0f;
                    
                    break;
                }
            }

            // 如果已经超过所有阈值
            if (!info.hasNextLevel && multiplierConfigs.Count > 0)
            {
                info.progressPercent = 1f;
                info.nextThreshold = multiplierConfigs[multiplierConfigs.Count - 1].threshold;
                info.nextMultiplier = multiplierConfigs[multiplierConfigs.Count - 1].multiplier;
            }

            return info;
        }

        /// <summary>
        /// 获取所有倍率配置（用于UI显示）
        /// </summary>
        public List<BetMultiplierConfig> GetAllConfigs()
        {
            return new List<BetMultiplierConfig>(multiplierConfigs);
        }
    }

    /// <summary>
    /// 押注进度信息 - 用于UI进度条显示
    /// </summary>
    public class BetProgressInfo
    {
        /// <summary>当前押注数量</summary>
        public int currentBetCount;
        
        /// <summary>当前倍率</summary>
        public int currentMultiplier;
        
        /// <summary>下一个阈值</summary>
        public int nextThreshold;
        
        /// <summary>下一个倍率</summary>
        public int nextMultiplier;
        
        /// <summary>是否还有下一级倍率</summary>
        public bool hasNextLevel;
        
        /// <summary>当前进度百分比（0-1）</summary>
        public float progressPercent;

        /// <summary>
        /// 获取进度描述文本
        /// </summary>
        public string GetProgressText()
        {
            if (hasNextLevel)
            {
                return $"{currentBetCount}/{nextThreshold} (x{currentMultiplier} → x{nextMultiplier})";
            }
            else
            {
                return $"{currentBetCount} (x{currentMultiplier} MAX)";
            }
        }
    }
}
```

---

### 2. 修改 GamblingGround.cs

在 `GamblingGround` 类中集成倍率系统：

#### 2.1 添加字段（在类的开头字段区域添加）

```csharp
[Header("押注倍率系统")]
[SerializeField] private List<BetMultiplierConfig> customMultiplierConfigs = new List<BetMultiplierConfig>
{
    new BetMultiplierConfig(10, 2),
    new BetMultiplierConfig(20, 4),
    new BetMultiplierConfig(30, 8)
};

// 倍率系统实例
private BetMultiplierSystem betMultiplierSystem;
```

#### 2.2 在 Start() 方法中初始化倍率系统

```csharp
void Start()
{
    // ... 现有代码 ...
    
    // 初始化倍率系统
    betMultiplierSystem = new BetMultiplierSystem(customMultiplierConfigs);
    Debug.Log("押注倍率系统已初始化");
    
    // ... 现有代码 ...
}
```

#### 2.3 修改 OnSpinComplete 方法，应用倍率

找到 `OnSpinComplete` 方法（约607行），修改奖励计算部分：

```csharp
private void OnSpinComplete(int selectedIndex)
{
    var card = cardItems[selectedIndex];
    string landedRewardName = card.rewardData.runtimeRewardName.Value;
    
    if (!IsCurrentlyEasterEggTriggering(card))
    {
        string category = GetRewardNameCategory(landedRewardName);
        
        if (string.IsNullOrEmpty(category))
        {
            Debug.LogWarning($"未找到 RewardName '{landedRewardName}' 对应的按钮类别");
            return;
        }
        
        BindableProperty<int> clickCount = buttonClickCount[category];
        
        // ✅ 应用押注倍率系统
        int betMultiplier = betMultiplierSystem.GetMultiplier(clickCount.Value);
        
        // 计算得分：基础分值 × 押注次数 × 押注倍率 × 其他倍率
        int baseScore = card.OnPlayerLand();
        int finalScore = (baseScore + finalColorBonusScore) * clickCount.Value * betMultiplier * currentDoubleNum.Value;
        
        Score.Value += finalScore;
        currentDoubleNum.Value = 1;
        
        Debug.Log($"停在 '{landedRewardName}' 卡片（类别：{category}）");
        Debug.Log($"基础分值：{baseScore}，押注次数：{clickCount.Value}，押注倍率：x{betMultiplier}，颜色奖励：{finalColorBonusScore}，最终得分：{finalScore}");
        
        if (finalScore > 0 && clickCount.Value > 0)
        {
            ShowSingleScorePopup(category, finalScore);
        }
    }
    
    TriggerRotationEndRelics(selectedIndex, card);
}
```

#### 2.4 添加公共方法供UI调用

在 `GamblingGround` 类中添加以下方法：

```csharp
/// <summary>
/// 获取指定类别的押注倍率
/// </summary>
/// <param name="category">按钮类别</param>
/// <returns>当前倍率</returns>
public int GetBetMultiplier(string category)
{
    if (betMultiplierSystem == null || !buttonClickCount.ContainsKey(category))
    {
        return 1;
    }
    
    int betCount = buttonClickCount[category].Value;
    return betMultiplierSystem.GetMultiplier(betCount);
}

/// <summary>
/// 获取指定类别的押注进度信息（供UI进度条使用）
/// </summary>
/// <param name="category">按钮类别</param>
/// <returns>进度信息</returns>
public BetProgressInfo GetBetProgressInfo(string category)
{
    if (betMultiplierSystem == null || !buttonClickCount.ContainsKey(category))
    {
        return new BetProgressInfo
        {
            currentBetCount = 0,
            currentMultiplier = 1,
            progressPercent = 0f
        };
    }
    
    int betCount = buttonClickCount[category].Value;
    return betMultiplierSystem.GetProgressInfo(betCount);
}

/// <summary>
/// 获取所有类别的押注进度信息（供UI批量更新使用）
/// </summary>
/// <returns>所有类别的进度信息字典</returns>
public Dictionary<string, BetProgressInfo> GetAllBetProgressInfo()
{
    Dictionary<string, BetProgressInfo> allProgress = new Dictionary<string, BetProgressInfo>();
    
    if (betMultiplierSystem == null) return allProgress;
    
    foreach (var kvp in buttonClickCount)
    {
        allProgress[kvp.Key] = betMultiplierSystem.GetProgressInfo(kvp.Value.Value);
    }
    
    return allProgress;
}
```

---

### 3. UI进度条使用示例

创建一个UI脚本来显示押注进度：

```csharp
using UnityEngine;
using UnityEngine.UI;
using Gambling;

public class BetProgressBar : MonoBehaviour
{
    [SerializeField] private string category; // 对应的押注类别
    [SerializeField] private Image progressFillImage; // 进度条填充图片
    [SerializeField] private Text progressText; // 进度文本
    [SerializeField] private Text multiplierText; // 倍率文本
    
    private GamblingGround gamblingGround;

    void Start()
    {
        gamblingGround = FindObjectOfType<GamblingGround>();
    }

    void Update()
    {
        if (gamblingGround == null) return;
        
        // 获取进度信息
        BetProgressInfo progressInfo = gamblingGround.GetBetProgressInfo(category);
        
        // 更新进度条
        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = progressInfo.progressPercent;
        }
        
        // 更新进度文本
        if (progressText != null)
        {
            progressText.text = progressInfo.GetProgressText();
        }
        
        // 更新倍率文本
        if (multiplierText != null)
        {
            multiplierText.text = $"x{progressInfo.currentMultiplier}";
            
            // 根据倍率改变颜色
            if (progressInfo.currentMultiplier >= 8)
                multiplierText.color = Color.red;
            else if (progressInfo.currentMultiplier >= 4)
                multiplierText.color = new Color(1f, 0.5f, 0f); // 橙色
            else if (progressInfo.currentMultiplier >= 2)
                multiplierText.color = Color.yellow;
            else
                multiplierText.color = Color.white;
        }
    }
}
```

---

### 4. 使用说明

#### 4.1 基本使用
1. 将 `BetMultiplierSystem.cs` 添加到项目中
2. 按照上述修改 `GamblingGround.cs`
3. 在Inspector中可以自定义倍率配置

#### 4.2 调整倍率配置
在Unity Inspector中找到 `GamblingGround` 组件，可以看到 `Custom Multiplier Configs` 列表，可以：
- 修改现有阈值和倍率
- 添加更多倍率档位（如40筹码16倍）
- 删除某些档位

#### 4.3 UI进度条集成
```csharp
// 在任何需要显示进度的地方调用
BetProgressInfo info = gamblingGround.GetBetProgressInfo("apple");
Debug.Log($"苹果押注进度：{info.progressPercent * 100}%");
Debug.Log($"当前倍率：x{info.currentMultiplier}");
```

---

### 5. 特性说明

✅ **完全解耦**：倍率系统独立于游戏逻辑，可单独测试和复用

✅ **配置化**：所有倍率阈值可在Inspector中调整，无需修改代码

✅ **易于扩展**：可轻松添加更多倍率档位（如40筹码16倍）

✅ **UI友好**：提供完整的进度信息，包括百分比、当前/下一级倍率等

✅ **性能优化**：计算逻辑简单高效，适合频繁调用

✅ **调试友好**：添加了详细的日志输出，便于追踪倍率计算过程

---

### 6. 测试验证

可以在 `OnRewardNameButtonClick` 方法中添加调试日志：

```csharp
private void OnRewardNameButtonClick(string rewardName)
{
    if (isSpinning || isEasterEggExecuting) return;
    ClearTemporarySelectBoxes();
    
    buttonClickCount[rewardName].Value++;
    Global.chips.Value -= 1;
    UpdateStartButtonState();
    
    // ✅ 显示当前倍率信息
    int multiplier = GetBetMultiplier(rewardName);
    BetProgressInfo info = GetBetProgressInfo(rewardName);
    Debug.Log($"{rewardName} 押注：{info.currentBetCount}个筹码，当前倍率：x{multiplier}，进度：{info.progressPercent * 100:F1}%");
    
    Debug.Log("当前筹码:" + Global.chips.Value);
}
```

这样每次点击押注按钮时，都会显示当前的倍率和进度信息。