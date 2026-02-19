using System;
using UnityEngine;
namespace Gambling
{
    /// <summary>
    /// 转动指定类型卡片时加分
    /// </summary>
    [CreateAssetMenu(fileName = "CardBetsDoubleEffect", menuName = "Gambling/Effects/单图案押注翻倍效果")]
    public class CardBetsDouble:RelicEffect
    {
        [Header("增加倍率")]
        [Tooltip("图案超过进度条时增加的倍率")]
        [SerializeField] private int CardDoubleNum = 1;
        private bool isTrigger = false;

        public override string RelicName => "根据转动次数筹码翻倍效果";
        
        public override bool CanTrigger(RelicTriggerType triggerType)
        {
            return triggerType == RelicTriggerType.Passive;
        }

        public override void ExecuteEffect(RelicEffectContext context)
        {
            if (!isTrigger)
            { 
                context.gamblingGround.betMultiplierSystem.SetMultiplierRelic(CardDoubleNum);
                isTrigger = true;
                //Debug.Log($"触发翻倍效果，倍率：{CardDoubleNum}");
            }
        }

        public override void Reset()
        {
            isTrigger = false;
        }


    }
}
