using UnityEngine;
using QFramework;

namespace Gambling
{
    [CreateAssetMenu(fileName = "OrangeScoreAddEnchantment", menuName = "Gambling/Effects/橘子附魔")]
    public class OrangeScoreAddEnchantment : RelicEffect
    {
        public override string RelicName => "橘子附魔";
        [SerializeField]private string _filterType = "橘子";
        private bool isTrigger = false;
        public EnchantmentEffect EnchantmentToApply; // 拖入刚才创建的 ScoreBoostEnchantment

        public override bool CanTrigger(RelicTriggerType triggerType)
        {
            // 购买后立即生效
            return triggerType == RelicTriggerType.Passive;
        }

        public override void ExecuteEffect(RelicEffectContext context)
        {
            if (!isTrigger)
            {
                // 获取附魔系统
                var enchantmentSystem = context.gamblingGround.GetSystem<IEnchantmentSystem>();
                
                enchantmentSystem.ApplyEnchantment(
                    context.gamblingGround, 
                    EnchantmentToApply, 
                    card => ((EnchantmentSystem)enchantmentSystem).FilterByType(card, _filterType)
                );
                isTrigger = true;
            }
            
        }

        public override void Reset() {isTrigger=false; }
    }
}