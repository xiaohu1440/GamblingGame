using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace Gambling
{
    [CreateAssetMenu(fileName = "BlueEnchantmentEffect", menuName = "Gambling/Effects/蓝色附魔")]
    public class BlueEnchantmentEffect:RelicEffect
    {
        public override string RelicName => "蓝色附魔";
        [SerializeField]private CardItem.CardType _filterType = CardItem.CardType.水果;
        private bool isTrigger = false;
        public EnchantmentEffect EnchantmentToApply;
        public List<int>Indices=new List<int>{0,1,2,3,4,5,6};
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
                List<CardItem> targets = systemImpl.GetFilteredCards(context.gamblingGround, Indices, _filterType);
                enchantmentSystem.ApplyEnchantment(
                    context.gamblingGround, 
                    EnchantmentToApply, 
                    card =>targets.Contains(card)
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