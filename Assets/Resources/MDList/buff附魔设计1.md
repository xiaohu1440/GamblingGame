根据您的需求，我为您设计了一个基于 **QFramework** 的 Buff 附魔系统。该系统采用了模块化设计，利用 QFramework 的事件系统和架构模式，实现了高度可扩展的附魔效果、选择逻辑以及 UI 展示。

### 核心系统设计

#### 1. 附魔数据与基类设计 (`Enchantment` & `EnchantmentEffect`)
首先，我们定义附魔效果的抽象基类。每个附魔效果可以决定是否需要检测得分、是否有持续时间（转动次数）等。

```csharp
using UnityEngine;
using System.Collections.Generic;
using QFramework;

namespace Gambling
{
    // 附魔触发时机
    public enum EnchantmentTriggerType
    {
        OnLand,        // 转动停止落位时
        OnSpinStart    // 转动开始时
    }

    // 附魔效果基类
    public abstract class EnchantmentEffect : ScriptableObject
    {
        public string EffectName;
        public string Description;
        public Sprite Icon;
        public int RemainingTurns = -1; // -1 表示永久，>0 表示有限次数

        public abstract bool NeedScoreCheck { get; } // 是否需要检测押注得分
        
        // 核心执行逻辑
        public abstract void Execute(CardItem target, GamblingGround ground);

        // 减少次数并检查是否结束
        public bool Tick()
        {
            if (RemainingTurns > 0)
            {
                RemainingTurns--;
                return RemainingTurns == 0;
            }
            return false;
        }
    }
}
```

#### 2. 附魔控制器 (`EnchantmentComponent`)
将此组件挂载到 `CardItem` (棋盘图案对象) 上，用于管理该图案上的所有附魔。

```csharp
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using QFramework;

namespace Gambling
{
    public class EnchantmentComponent : MonoBehaviour, IController
    {
        public List<EnchantmentEffect> ActiveEffects = new List<EnchantmentEffect>();
        
        // 用于显示的 UI 悬浮窗引用
        public GameObject TooltipPrefab; 
        private GameObject activeTooltip;

        // 核心：处理附魔触发
        public void OnTrigger(EnchantmentTriggerType triggerType, GamblingGround ground)
        {
            CardItem card = GetComponent<CardItem>();
            List<EnchantmentEffect> toRemove = new List<EnchantmentEffect>();

            foreach (var effect in ActiveEffects)
            {
                // 逻辑判断：是否得分等条件
                bool canTrigger = !effect.NeedScoreCheck || ground.IsCurrentlyEasterEggTriggering(card);
                
                if (canTrigger)
                {
                    effect.Execute(card, ground);
                }

                // 更新步数限制
                if (effect.Tick()) toRemove.Add(effect);
            }

            // 移除失效的附魔
            foreach (var expired in toRemove) ActiveEffects.Remove(expired);
        }

        // 处理悬浮窗 UI 的动画展示逻辑
        public void ShowEffectSequence()
        {
            if (ActiveEffects.Count == 0) return;

            Sequence seq = DOTween.Sequence();
            foreach (var effect in ActiveEffects)
            {
                seq.AppendCallback(() => {
                    // 生成/更新悬浮窗内容
                    UpdateTooltipUI(effect);
                });
                
                // 1s逐渐出现 -> 1s停留 -> 1s消失
                seq.Append(activeTooltip.GetComponent<CanvasGroup>().DOFade(1, 1f));
                seq.AppendInterval(1f);
                seq.Append(activeTooltip.GetComponent<CanvasGroup>().DOFade(0, 1f));
            }
            seq.SetLoops(-1); // 循环展示
        }

        private void UpdateTooltipUI(EnchantmentEffect effect)
        {
            // 初始化或更新 Tooltip 的 Text 和 Icon
            if (activeTooltip == null) {
                activeTooltip = Instantiate(TooltipPrefab, transform);
                activeTooltip.GetComponent<CanvasGroup>().alpha = 0;
            }
            // 设置 UI 内容逻辑...
        }

        public IArchitecture GetArchitecture() => GamblingApp.Interface;
    }
}
```

#### 3. 附魔管理器 (`EnchantmentSystem`)
负责根据规则（位置、类型）选取目标并施加附魔。

```csharp
using System.Linq;
using System.Collections.Generic;

namespace Gambling
{
    public class EnchantmentSystem : AbstractSystem
    {
        protected override void OnInit() { }

        // 通过位置和类型选取目标
        public void ApplyEnchantment(EnchantmentEffect effect, System.Func<CardItem, bool> filter)
        {
            var ground = MonoSingletonPath<GamblingGround>.Instance; // 假设通过此方式获取
            var targets = ground.cardItems.Where(filter).ToList();

            foreach (var target in targets)
            {
                var comp = target.GetOrAddComponent<EnchantmentComponent>();
                // 克隆一份 ScriptableObject 保证实例状态独立（尤其是剩余步数）
                comp.ActiveEffects.Add(UnityEngine.Object.Instantiate(effect));
            }
        }

        // 示例：选取四个角落
        public bool FilterCorners(CardItem card)
        {
            int idx = card.index;
            // 假设棋盘是 4xN 或者通过索引计算
            return idx == 0 || idx == 3 || idx == 12 || idx == 15; 
        }

        // 示例：选取所有橘子
        public bool FilterByType(CardItem card, string name)
        {
            return card.rewardData.RewardName.Contains(name);
        }
    }
}
```

### 功能点实现说明

1.  **选取系统**：使用 `EnchantmentSystem` 中的 `ApplyEnchantment` 方法，配合 Lambda 表达式（如 `card => card.index == 0`）可以极易地扩展“四个角落”或“橘子图案”等筛选逻辑。
2.  **触发逻辑**：在 `GamblingGround.OnSpinComplete` (转动停止) 时调用 `card.GetComponent<EnchantmentComponent>().OnTrigger(...)`。
3.  **得分检测**：通过 `EnchantmentEffect.NeedScoreCheck` 属性，在执行前结合 `GamblingGround` 的现有逻辑（如是否在该位置下注）进行过滤。
4.  **叠加与消失**：`EnchantmentComponent` 使用 `List` 存储效果实现叠加，通过 `Tick()` 方法维护步数计数器并在归零时移除。
5.  **UI 表现 (DOTween)**：在 `EnchantmentComponent` 中实现了一个 `Sequence`。当鼠标悬浮 (`OnPointerEnter`) 时触发该循环序列，利用 `CanvasGroup` 实现 1s 出现、1s 停留、1s 消失的轮播效果。

### QFramework 集成建议
*   将 `EnchantmentSystem` 注册到 `GamblingApp` 架构中。
*   使用 `this.SendEvent<EnchantEvent>()` 来触发附魔的添加，让系统之间解耦。
*   利用 `BindableProperty` 来观察附魔次数的变化，实时更新 UI 文本。