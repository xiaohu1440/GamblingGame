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
            return idx == 0 || idx == 3 || idx == 12 || idx == 15; 
        }

        // 示例：选取所有橘子
        public bool FilterByType(CardItem card, string name)
        {
            return card.rewardData.runtimeRewardName.Value==name;
        }
    }
}