using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace Gambling
{
    [CreateAssetMenu(fileName = "DoubleTriggerEnchantment", menuName = "Gambling/Enchantments/双倍触发附魔")]
    public class DoubleTriggerEnchantment:EnchantmentEffect
    {
        public override string EnchantmentName => "蓝色附魔";
        public override bool NeedScoreCheck => true;
        public override void Execute(CardItem cardItem, GamblingGround gamblingGround)
        {
            // 1. 获取基础分值
            int baseScore = cardItem.OnPlayerLand();
    
            // 2. 获取对应的倍率和押注次数 (模拟 OnSpinComplete 中的计算)
            string category = gamblingGround.GetRewardNameCategory(cardItem.rewardData.runtimeRewardName.Value);
            if (string.IsNullOrEmpty(category)) return;

            int betCount = gamblingGround.GetButtonClickCount(category); // 假设你已添加此 Helper 方法
            int betMultiplier = gamblingGround.betMultiplierSystem.GetMultiplier(betCount);
            Debug.Log("附魔 当前按压次数"+betCount);
            
            // 3. 计算并增加分数 (这里只进行第二次得分触发)
            // 注意：根据你的需求，第二次触发是否还要乘上 currentDoubleNum 取决于你想让它有多强
            int extraScore = (baseScore + gamblingGround.finalColorBonusScore) * betMultiplier * betCount * gamblingGround.globalDoubleNum.Value*gamblingGround.currentDoubleNum.Value;
            Debug.Log("附魔 当前得分"+extraScore);
            gamblingGround.Score.Value += extraScore;
            // 4. 弹出飘字 UI 表现
            if (extraScore > 0)
            {
                gamblingGround.ShowSingleScorePopup(category, extraScore,1f);
            }

            // 4. 再次触发该图案上的其他附魔
            var enchantComp = cardItem.GetComponent<EnchantmentComponent>();
            if (enchantComp != null)
            {
                foreach (var effect in enchantComp.ActiveEffects)
                {
                    // 排除自身，防止死循环；同时检查是否满足触发条件
                    if (effect != this)
                    {
                        bool canTrigger = false;
                        if (!effect.NeedScoreCheck)
                        {
                            canTrigger = true;
                        }
                        else
                        {
                            canTrigger = !gamblingGround.IsCurrentlyEasterEggTriggering(cardItem) && betCount > 0;
                        }

                        if (canTrigger)
                        {
                            effect.Execute(cardItem, gamblingGround);
                        }
                    }
                }
            }
        }
    }
}