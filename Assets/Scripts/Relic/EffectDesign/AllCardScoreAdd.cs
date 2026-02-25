using System;
using UnityEngine;
namespace Gambling
{
    [CreateAssetMenu(fileName = "AllCardScoreAdd", menuName = "Gambling/Effects/全图案分值增加")]
    public class AllCardScoreAdd:RelicEffect
    {
        [Header("全局更改")]
        [Tooltip("全图案分值增加的分值")]
        [SerializeField] private int AddNum = 10;
        private bool isTrigger = false;

        public override string RelicName => "全图案分值增加";
        public override bool CanTrigger(RelicTriggerType triggerType)
        {
            return triggerType == RelicTriggerType.Passive;
        }

        public override void ExecuteEffect(RelicEffectContext context)
        {
            if (!isTrigger)
            {
                foreach (var card in context.gamblingGround.rewardDataArray)
                {
                    card.runtimeGoldValue.Value+=AddNum;
                }
                isTrigger = true;
            }

            
        }

        public override void Reset()
        {
            isTrigger=false;
        }
    }
}