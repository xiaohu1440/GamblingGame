using System.Collections.Generic;
using UnityEngine;
using QFramework;
namespace Gambling
{
    [CreateAssetMenu(fileName = "XiaoSanYuanEffect", menuName = "Gambling/Effects/图案概率修改")]
    public class XiaoSanYuanEffect:RelicEffect
    {
        public override string RelicName => "小三元图案概率+1";
        private bool isTrigger = false;
        [Header("图案配置")]
        [Tooltip("需要增加权重的图案名称列表（默认：橘子、蓝莓、铃铛）")]
        [SerializeField] private List<string> targetPatterns = new List<string> { "橘子", "蓝莓", "铃铛" };
        
        [Header("权重倍数")]
        [Tooltip("权重增加的倍数")]
        [SerializeField] private float weightMultiplier = 2f;
        private Dictionary<RewardData, float> originalWeights = new Dictionary<RewardData, float>();
        public override bool CanTrigger(RelicTriggerType triggerType)
        {
            return triggerType == RelicTriggerType.Passive;
        }

        public override void ExecuteEffect(RelicEffectContext context)
        {
            if (!isTrigger && context.gamblingGround != null)
            {
                context.gamblingGround.totalWeight.Value = 0;
                // 遍历所有卡牌
                foreach (var cardItem in context.gamblingGround.cardItems)
                {
                    if(cardItem != null && cardItem.rewardData != null) 
                    {
                        string cardName = cardItem.rewardData.runtimeRewardName.Value;
                        
                        // 检查卡牌名称是否包含目标图案
                        bool isTargetPattern = false;
                        foreach (var pattern in targetPatterns)
                        {
                            if (cardName.Contains(pattern))
                            {
                                isTargetPattern = true;
                                break;
                            }
                        }
                        // ✅ 关键修改：只在第一次遇到该 RewardData 时修改权重
                        if (isTargetPattern && !originalWeights.ContainsKey(cardItem.rewardData))
                        {
                            // 保存原始权重
                            originalWeights[cardItem.rewardData] = cardItem.rewardData.runtimeChanceWeight.Value;
                            
                            // 修改权重（只执行一次）
                            cardItem.rewardData.runtimeChanceWeight.Value *= weightMultiplier;
                        }
                        

                        context.gamblingGround.totalWeight.Value += cardItem.rewardData.runtimeChanceWeight.Value;
                    }
                }
                
                context.gamblingGround.CalculateCardWeights();
                
                isTrigger = true;
                Debug.Log($"小三元效果已触发，目标图案：{string.Join("、", targetPatterns)}，权重倍数：×{weightMultiplier}");
            }
        }

        public override void Reset()
        {
            // 恢复所有修改过的权重
            foreach (var kvp in originalWeights)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.runtimeChanceWeight.Value = kvp.Value;
                }
            }
            
            originalWeights.Clear();
            isTrigger = false;
            Debug.Log("🔄 小三元效果已重置");
        }
    }
}