using System;
using UnityEngine;
namespace Gambling
{
    /// <summary>
    /// 转动指定类型卡片时加分
    /// </summary>
    [CreateAssetMenu(fileName = "CardSpinScoreEffect", menuName = "Gambling/Effects/CardSpinScoreAdd")]
    public class CardSpinScoreAdd:RelicEffect
    {
        [Header("卡片配置")]
        [Tooltip("触发效果的卡片")]
        public RewardData triggerCard;
        [Header("分数配置")]
        [Tooltip("每次转动增加的分数")]
        [SerializeField] private int scorePerSpin = 1;

        public override string RelicName => "转动指定类型卡片时加分";
        public override bool CanTrigger(RelicTriggerType triggerType)
        {
            return triggerType == RelicTriggerType.OnRotationEnd;
        }

        public override void ExecuteEffect(RelicEffectContext context)
        {
            foreach (var card in context.gamblingGround.rewardDataArray)
            {
                if (card.runtimeRewardName.Value == triggerCard.RewardName)
                {
                    card.runtimeGoldValue.Value+=scorePerSpin;
                }
                    
            }
                
        }

        public override void Reset()
        {
            
        }
    }
}
