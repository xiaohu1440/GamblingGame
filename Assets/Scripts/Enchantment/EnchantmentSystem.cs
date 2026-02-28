using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
namespace Gambling
{
    public interface IEnchantmentSystem : ISystem
    {
        void ApplyEnchantment(GamblingGround ground, EnchantmentEffect effect, System.Func<CardItem, bool> filter);
    }
    public class EnchantmentSystem:AbstractSystem,IEnchantmentSystem
    {
        
        protected override void OnInit()
        {
            
        }
        // 通过位置和类型选取目标
        public void ApplyEnchantment(GamblingGround ground,EnchantmentEffect effect, System.Func<CardItem, bool> filter)
        {
            var targets = ground.cardItems.Where(filter).ToList();

            foreach (var target in targets)
            {
                var comp = target.GetOrAddComponent<EnchantmentComponent>();
                // 克隆一份 ScriptableObject 保证实例状态独立（尤其是剩余步数）
                //comp.ActiveEffects.Add(UnityEngine.Object.Instantiate(effect));
                comp.AddEffect(UnityEngine.Object.Instantiate(effect));
            }
        }
        // 示例：选取四个角落
        public bool FilterCorners(CardItem card)
        {
            int idx = card.index;
            // 假设棋盘是 4xN 或者通过索引计算
            return idx == 0 || idx == 6 || idx == 12 || idx == 18; 
        }

        // 示例：选取所有橘子
        public bool FilterByType(CardItem card, string name)
        {
            return card.rewardData.runtimeRewardName.Value==name;
        }
        /// <summary>
        /// 在给定的索引范围内，筛选出指定类型的卡片
        /// 例如：传入棋盘上方所有索引 [0, 1, 2, 3, 4] 和 CardType.水果
        /// </summary>
        public List<CardItem> GetFilteredCards(GamblingGround ground, List<int> indices, CardItem.CardType cardType)
        {
            return ground.cardItems
                .Where(card => indices.Contains(card.index) && card.cardType == cardType)
                .ToList();
        }

        public List<CardItem> RandomFilteredCards(GamblingGround ground, List<int> indices)
        {
            return ground.cardItems
                .Where(card => indices.Contains(card.index)&& card.cardType != CardItem.CardType.彩蛋)
                .OrderBy(x => Random.value) // 随机排序
                .Take(3)                    // 取前三个（如果不足三个则取所有）
                .ToList();
        }
        
    }
}