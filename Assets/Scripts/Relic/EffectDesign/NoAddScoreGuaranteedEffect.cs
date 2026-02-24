using UnityEngine;
using QFramework;
namespace Gambling
{
    [CreateAssetMenu(fileName = "NoAddScoreGuaranteedEffect", menuName = "Gambling/Effects/未得分时获得分数")]
    public class NoAddScoreGuaranteedEffect:RelicEffect
    {
        [SerializeField] private int scoreAdd = 5;
        public override string RelicName => "未得分时获得分数";
        public override bool CanTrigger(RelicTriggerType triggerType)
        {
            return triggerType == RelicTriggerType.OnRotationEnd;
        }

        public override void ExecuteEffect(RelicEffectContext context)
        {
            if (context.gamblingGround.checkFinalScore <= 0)
            {
                context.gamblingGround.Score.Value+=scoreAdd;
            }
        }

        public override void Reset()
        {
            
        }
    }
}