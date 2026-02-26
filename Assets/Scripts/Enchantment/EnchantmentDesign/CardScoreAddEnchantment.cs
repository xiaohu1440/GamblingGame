using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace Gambling
{
    [CreateAssetMenu(fileName = "ScoreBoostEnchantment", menuName = "Gambling/Enchantments/分数提升附魔")]
    public class CardScoreAddEnchantment:EnchantmentEffect
    {
        public override string EnchantmentName => "转动到后分值增加";
        public override bool NeedScoreCheck => false;
        [SerializeField] private int scoreAdd = 10;
        public override void Execute(CardItem cardItem, GamblingGround gamblingGround)
        {
            // 1. 获取当前触发位置图案的类别标识 (例如 "orange")
            string targetCategory = gamblingGround.GetRewardNameCategory(cardItem.rewardData.runtimeRewardName.Value);
            
            if (string.IsNullOrEmpty(targetCategory))
            {
                Debug.LogWarning($"[附魔] 无法识别图案类别: {cardItem.rewardData.runtimeRewardName.Value}");
                return;
            }

            // ✅ 核心逻辑：直接修改原始数据源 rewardDataArray
            // 使用 HashSet 记录已修改的实例，防止在 rewardDataArray 中存在重复引用时导致多次加分
            HashSet<RewardData> modifiedDatas = new HashSet<RewardData>();

            foreach (var data in gamblingGround.rewardDataArray)
            {
                // 通过类别匹配找到对应的数据源实例
                string dataCategory = gamblingGround.GetRewardNameCategory(data.runtimeRewardName.Value);

                if (dataCategory == targetCategory && !modifiedDatas.Contains(data))
                {
                    // 永久增加该数据源的运行时分值
                    data.runtimeGoldValue.Value += scoreAdd;
                    
                    // 标记已修改
                    modifiedDatas.Add(data);
                    
                    Debug.Log($"[附魔触发] 数据源 {data.runtimeRewardName.Value} 分值永久提升至: {data.runtimeGoldValue.Value}");
                }
            }

            // 2. 通知棋盘上所有相关的 CardItem 刷新 UI
            // 虽然数据源变了，但场景中的格子需要调用 UpdateUI 来重新读取最新的数值
            foreach (var card in gamblingGround.cardItems)
            {
                if (gamblingGround.GetRewardNameCategory(card.rewardData.runtimeRewardName.Value) == targetCategory)
                {
                    card.UpdateUI();
                }
            }
        }
    }
}