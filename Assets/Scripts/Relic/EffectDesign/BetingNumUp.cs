using System;
using UnityEngine;
namespace Gambling
{
    [CreateAssetMenu(fileName = "BetingNumUpEffect", menuName = "Gambling/Effects/增加押注获得的筹码")]
    public class BetingNumUp:RelicEffect
    {
        [Header("增加筹码")]
        [Tooltip("增加筹码数值")]
        [SerializeField] private int addTicketNum = 5;
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
                context.gamblingGround.chipsAddNum = addTicketNum;
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
