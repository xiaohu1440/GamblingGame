using UnityEngine;

namespace Gambling
{
    public class RelicEffectContext
    {
        public GamblingGround gamblingGround;
        public int currentIndex;
        public int currentRotationNum;
        public int RelicNum;
        public CardItem currentCard;
        public int currentDoubleNum;
        public ColorScoreConfig currentColorScoreConfig;

    }
    public abstract class RelicEffect:ScriptableObject
    {
        [Header("基础信息")]
        [SerializeField] protected bool isEnabled = true;
        public bool IsEnabled => isEnabled;
        public abstract string RelicName { get; }
        public abstract bool CanTrigger(RelicTriggerType triggerType);//检测是否可以触发
        public abstract void ExecuteEffect(RelicEffectContext context);


        

    }
}