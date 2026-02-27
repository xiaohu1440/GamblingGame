using System.Collections.Generic;
using UnityEngine;
using QFramework;
namespace Gambling
{
    [CreateAssetMenu(fileName = "BlueEnchantmentEffect", menuName = "Gambling/Effects/黄色附魔")]
    public class YellowChipsEnchantment:RelicEffect
    {
        public override string RelicName => "黄色附魔->增加筹码";
        private bool isTrigger = false;
        public EnchantmentEffect EnchantmentToApply;
        public override bool CanTrigger(RelicTriggerType triggerType)
        {
            return triggerType == RelicTriggerType.Passive;
        }

        public override void ExecuteEffect(RelicEffectContext context)
        {
            if (!isTrigger)
            {
                var enchantmentSystem = context.gamblingGround.GetSystem<IEnchantmentSystem>();
                var systemImpl = (EnchantmentSystem)enchantmentSystem;
                enchantmentSystem.ApplyEnchantment(
                    context.gamblingGround, 
                    EnchantmentToApply, 
                    card => ((EnchantmentSystem)enchantmentSystem).FilterCorners(card)
                );
                isTrigger = true;
            }
        }

        public override void Reset()
        {
            isTrigger = false;
        }
    }
}