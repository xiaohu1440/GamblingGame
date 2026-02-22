using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gambling
{
    [CreateAssetMenu(fileName = "AddChipsEffect", menuName = "Gambling/Effects/增加筹码")]
    public class AddChips : RelicEffect
    {
        [Header("增加筹码")]
        [Tooltip("增加筹码数值")]
        [SerializeField] private int addTicketNum = 3;
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
                context.gamblingGround.chipsGlobalAddNum += addTicketNum;
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

