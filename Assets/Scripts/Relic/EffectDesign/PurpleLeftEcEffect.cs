using System.Collections.Generic;
using UnityEngine;
using QFramework;
namespace Gambling
{
    [CreateAssetMenu(fileName = "PurpleLeftEcEffect", menuName = "Gambling/Effects/左侧紫附魔")]
    public class PurpleLeftEcEffect:RelicEffect
    {
        public override string RelicName => "紫色附魔遗物_左";
        public EnchantmentEffect EnchantmentToApply;
        public List<int>Indices=new List<int>{0,18,19,20,21,22,23};
        private bool isTrigger = false;
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
                List<CardItem> targets = systemImpl.RandomFilteredCards(context.gamblingGround, Indices);
                enchantmentSystem.ApplyEnchantment(
                    context.gamblingGround,
                    EnchantmentToApply,
                    card => targets.Contains(card)
                    );
                isTrigger=true;
            }
        }

        public override void Reset()
        {
            isTrigger=false;
        }
    }
}