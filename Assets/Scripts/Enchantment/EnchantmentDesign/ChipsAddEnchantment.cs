using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace Gambling
{
    [CreateAssetMenu(fileName = "ChipsAddEnchantment", menuName = "Gambling/Enchantments/黄色筹码附魔")]
    public class ChipsAddEnchantment:EnchantmentEffect
    {
        public override string EnchantmentName => "黄色附魔";
        public override bool NeedScoreCheck => false;
        [SerializeField] private int scoreAdd = 3;
        public override void Execute(CardItem cardItem, GamblingGround gamblingGround)
        {
            Global.chips.Value += scoreAdd;
        }
    }
}