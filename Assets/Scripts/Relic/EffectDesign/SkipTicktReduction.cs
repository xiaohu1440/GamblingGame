using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QFramework;
namespace Gambling
{
    [CreateAssetMenu(fileName = "SkipTicktReduction", menuName = "Gambling/Effects/跳过转动卷消耗效果")]
    public class SkipTicktReduction:RelicEffect
    {
        public override string RelicName => "转动时有概率跳过转动卷消耗";
        [SerializeField] private float skipTicktNum = 0.25f;
        public override bool CanTrigger(RelicTriggerType triggerType)
        {
            return triggerType == RelicTriggerType.OnRotationStart;
        }

        public override void ExecuteEffect(RelicEffectContext context)
        {
            context.gamblingGround.skipTicktReductionChance = Random.Range(0f, 1f);
            context.gamblingGround.relicskipTicktReductionNum = skipTicktNum;
        }

        public override void Reset()
        {
            
        }
    }
}