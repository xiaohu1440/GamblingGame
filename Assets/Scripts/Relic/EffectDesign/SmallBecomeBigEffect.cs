using System;
using UnityEngine;
using QFramework;
using UnityEngine.UI;
namespace Gambling
{
    [CreateAssetMenu(fileName = "SmallBecomeBigEffect", menuName = "Gambling/Effects/小图案视为大图案")]
    public class SmallBecomeBigEffect:RelicEffect
    {
        public override string RelicName => "小图案视为大图案";
        private bool isTrigger = false;
        public override bool CanTrigger(RelicTriggerType triggerType)
        {
            return triggerType == RelicTriggerType.Passive;
        }

        public override void ExecuteEffect(RelicEffectContext context)
        {
            if (!isTrigger)
            {
                // 1. 遍历当前场景中所有的格子（CardItem）
                foreach (var cardItem in context.gamblingGround.cardItems)
                {
                    var data = cardItem.rewardData;
            
                    // 2. 如果当前是小图案
                    if (data.runTimeisMini)
                    {
                        // 3. 修改状态
                        data.runTimeisMini = false;
                
                        // 4. 寻找对应的大图案图标
                        // 假设命名逻辑是：小图案名字比大图案多一个 "_M" 结尾
                        string bigCardName = data.runtimeRewardName.Value.Replace("_M", "");
                
                        // 从预加载的 rewardDataArray 中寻找同名的大图案资源
                        foreach (var bigData in context.gamblingGround.rewardDataArray)
                        {
                            if (bigData.runtimeRewardName.Value == bigCardName && !bigData.isMini)
                            {
                                // 替换运行时图标
                                data.runtimeRewardIcon = bigData.runtimeRewardIcon;
                                data.runtimeRewardName.Value = bigCardName; 
                                data.runtimeGoldValue.Value = bigData.runtimeGoldValue.Value;
                                break;
                            }
                        }

                        // 5. 重要：通知 CardItem 更新 UI 表现
                        // 注意：CardItem 中的 UpdateUI 是 protected，如果无法直接调用，
                        // 建议在 CardItem 中添加一个 public 的刷新方法，或者通过反射/改权限。
                        cardItem.UpdateUI();
                    }
                }

                float newTotalWeight = 0f;
                foreach (var cardItem in context.gamblingGround.cardItems)
                {
                    newTotalWeight += cardItem.rewardData.runtimeChanceWeight.Value;
                }
                // 更新 BindableProperty 的值
                context.gamblingGround.totalWeight.Value = -114514f;
                context.gamblingGround.totalWeight.Value = newTotalWeight;
                context.gamblingGround.SmallIconProBabilityText.GetComponent<Text>().text = "0%";
                context.gamblingGround.CalculateCardWeights();
                
                isTrigger = true;
            }
        }

        public override void Reset()
        {
            isTrigger=false;
        }
    }
}