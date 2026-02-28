using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace Gambling
{
    [CreateAssetMenu(fileName = "PurpleBetEnchantment", menuName = "Gambling/Enchantments/紫色押注附魔")]
    public class PurpleBetEnchantment:EnchantmentEffect
    {
        public override string EnchantmentName => "紫色附魔";
        public override bool NeedScoreCheck => false;
        [SerializeField] private int scoreCounts = 1;
        
        public override void Execute(CardItem cardItem, GamblingGround gamblingGround)
        {
            // 1. 获取当前触发附魔图案的类别标识 (例如 "orange")
            string targetCategory = gamblingGround.GetRewardNameCategory(cardItem.rewardData.runtimeRewardName.Value);
            
            if (string.IsNullOrEmpty(targetCategory))
            {
                Debug.LogWarning($"[附魔] 紫色附魔无法识别图案类别: {cardItem.rewardData.runtimeRewardName.Value}");
                return;
            }

            // 2. 增加该类别的押注进度（即增加按钮点击计数）
            if (gamblingGround.extraBetPerClick.ContainsKey(targetCategory))
            {
                gamblingGround.extraBetPerClick[targetCategory] += scoreCounts;
                Debug.Log($"[紫色附魔] {targetCategory} 现在每次点击将额外增加 {scoreCounts} 点进度。");
            }
            else
            {
                Debug.LogWarning($"[附魔] GamblingGround 中不存在类别: {targetCategory} 的计数器");
            }
        }
    }
}
