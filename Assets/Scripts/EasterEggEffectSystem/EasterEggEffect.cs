using System.Collections;
using System.Collections.Generic;

namespace Gambling
{
    /// <summary>
    /// 彩蛋效果基类
    /// </summary>
    public abstract class EasterEggEffect
    {
        public abstract string EasterEggName { get; }
        public abstract bool CanTrigger(RewardData currentCard, List<RewardData> allCards);
        public abstract List<CardItem> GetTargetCards(List<CardItem> allCards);

        public abstract IEnumerator ExecuteEffect(GamblingGround gamblingGround, int currentIndex,
            List<CardItem> targetCards);
        
    }

}
