要通过遗物触发附魔效果，为棋盘四个角落的图案施加“押注得分时增加分值”的效果，我们需要结合 `QFramework` 的架构，利用 `EnchantmentSystem`（附魔系统）来实现。

虽然之前的对话中提供了设计思路，但根据目前的工程现状，我们需要具体实现两个核心部分：**附魔效果逻辑** 和 **遗物触发逻辑**。

### 1. 定义附魔效果 (CornerScoreEnchantment)

首先，我们需要创建一个具体的附魔效果类，该效果会拦截得分逻辑或直接修改对应图案的得分。

```csharp
using UnityEngine;
using QFramework;

namespace Gambling
{
    // 增加得分的附魔效果
    [CreateAssetMenu(fileName = "ScoreBoostEnchantment", menuName = "Gambling/Enchantments/分数提升附魔")]
    public class ScoreBoostEnchantment : EnchantmentEffect
    {
        public int BonusScore = 10; // 额外增加的分数

        public override bool NeedScoreCheck => true; // 需要检测是否押注得分

        public override void Execute(CardItem target, GamblingGround ground)
        {
            // 获取当前落点图案的类别
            string category = ground.GetRewardNameCategory(target.rewardData.runtimeRewardName.Value);
            
            // 获取该类别的押注次数
            int betCount = ground.GetButtonClickCount(category);
            
            // 如果玩家有押注（得分条件），则增加分值
            if (betCount > 0)
            {
                // 这里直接增加全局分数，或者修改 ground.checkFinalScore
                ground.Score.Value += BonusScore;
                
                // 弹出 UI 提示
                ground.ShowSingleScorePopup(category, BonusScore);
                
                Debug.Log($"附魔触发：{target.rewardData.runtimeRewardName.Value} 额外获得 {BonusScore} 分");
            }
        }
    }
}
```

### 2. 创建遗物效果 (CornerEnchantRelicEffect)

创建一个遗物效果类，在购买遗物时（或特定时机）通过 `EnchantmentSystem` 给四个角落施加上述附魔。

```csharp
using UnityEngine;
using QFramework;

namespace Gambling
{
    [CreateAssetMenu(fileName = "CornerEnchantRelicEffect", menuName = "Gambling/Effects/角落附魔遗物")]
    public class CornerEnchantRelicEffect : RelicEffect
    {
        public override string RelicName => "角落祝福之书";
        
        public EnchantmentEffect EnchantmentToApply; // 拖入刚才创建的 ScoreBoostEnchantment

        public override bool CanTrigger(RelicTriggerType triggerType)
        {
            // 购买后立即生效
            return triggerType == RelicTriggerType.Passive;
        }

        public override void ExecuteEffect(RelicEffectContext context)
        {
            // 获取附魔系统
            var enchantmentSystem = context.gamblingGround.GetSystem<IEnchantmentSystem>();
            
            // 施加附魔：筛选索引为四个角落 (假设棋盘是 4xN 且总数16个)
            // 常见的四个角落索引计算（如果是 4x4）：0, 3, 12, 15
            enchantmentSystem.ApplyEnchantment(EnchantmentToApply, card => 
                card.index == 0 || 
                card.index == 3 || 
                card.index == 12 || 
                card.index == 15
            );

            Debug.Log("遗物效果：已为四个角落施加得分加成附魔。");
        }

        public override void Reset() { }
    }
}
```

### 3. 系统集成关键点

为了让这套逻辑运转起来，请确保以下 QFramework 流程：

1.  **注册系统**：在 `GamblingApp` (或对应的 Architecture 类) 中注册 `EnchantmentSystem`。
2.  **挂载组件**：在 `CardItem` 预制体上挂载 `EnchantmentComponent`，用于接收和处理附魔触发。
3.  **触发时机**：在 `GamblingGround.cs` 的 `OnSpinComplete` 方法末尾，添加附魔触发代码：
    ```csharp
    // 在 GamblingGround.cs 的 OnSpinComplete 中
    var enchantComp = card.GetComponent<EnchantmentComponent>();
    if (enchantComp != null)
    {
        enchantComp.OnTrigger(EnchantmentTriggerType.OnLand, this);
    }
    ```

### 4. 逻辑说明
*   **筛选器**：使用 `card => card.index == ...` 能够精确锁定四个角落。
*   **得分判定**：附魔效果内部通过 `ground.GetButtonClickCount(category) > 0` 判断玩家是否对该图案进行了押注，从而实现“押注得分时才增加分值”的需求。
*   **动画表现**：`EnchantmentComponent` 会自动处理 1s出现-1s停留-1s消失 的 DOTween 轮播动画。