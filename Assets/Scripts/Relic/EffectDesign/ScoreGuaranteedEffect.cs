using UnityEngine;
using QFramework;
namespace Gambling
{
    [CreateAssetMenu(fileName = "ScoreGuaranteedEffect", menuName = "Gambling/Effects/筹码保留获得分数")]
    public class ScoreGuaranteedEffect:RelicEffect
    {
        public override string RelicName => "剩余筹码时获得固定的分数收益";
        [SerializeField]private int scoreAdd = 10;
        public override bool CanTrigger(RelicTriggerType triggerType)
        {
            return triggerType == RelicTriggerType.OnRotationEnd;
        }

        public override void ExecuteEffect(RelicEffectContext context)
        {
            if (Global.chips.Value > 0)
            {
                context.gamblingGround.Score.Value+=scoreAdd;
            }
        }

        public override void Reset()
        {
            
        }
    }
}