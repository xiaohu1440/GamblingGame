using System;
using UnityEngine;
namespace Gambling
{
    [CreateAssetMenu(fileName = "DoubleToGlobal", menuName = "Gambling/Effects/触发单图案翻倍时下回合全局倍率翻倍")]
    public class DoubleToGlobal:RelicEffect
    {
        [Header("全局倍率")]
        [Tooltip("根据是否触发单图案翻倍增加全局倍率")]
        [SerializeField] private int GlobalDoubleNum = 2;
        public override string RelicName=> "触发单图案翻倍时下回合全局倍率翻倍";

        public override bool CanTrigger(RelicTriggerType triggerType)
        {
            return triggerType == RelicTriggerType.OnRotationEnd;
        }

        public override void ExecuteEffect(RelicEffectContext context)
        {
                        // 检查上下文是否有效
            if (context == null || context.gamblingGround == null || context.currentCard == null)
            {
                Debug.LogWarning("DoubleToGlobal: 上下文无效，无法执行效果");
                return;
            }

            // 获取当前卡片的奖励名称
            string rewardName = context.currentCard.rewardData.runtimeRewardName.Value;
            
            // 获取卡片类别
            string category = context.gamblingGround.GetRewardNameCategory(rewardName);
            
            if (string.IsNullOrEmpty(category))
            {
                // 如果没有找到类别，可能是彩蛋卡片或其他特殊卡片，直接返回
                return;
            }
            
            // 获取当前类别的押注倍率
            int betMultiplier = context.gamblingGround.GetBetMultiplier(category);
            
            // 检测是否触发了单图案翻倍（倍率 > 1 表示触发了翻倍）
            if (betMultiplier > 1)
            {
                // 设置下一回合的全局倍率
                context.gamblingGround.currentDoubleNum.Value = GlobalDoubleNum;
                Debug.Log($"🎯 触发单图案翻倍（倍率：x{betMultiplier}），下回合全局倍率设置为：x{GlobalDoubleNum}");
            }
        }

        public override void Reset()
        {

        }
    }
}