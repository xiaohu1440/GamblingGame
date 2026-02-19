using System.Collections.Generic;
using QFramework;

namespace Gambling
{
    public interface IRelicSystem:ISystem
    {
        List<RelicData> DrawRelicsForShop(int currentLevel, int relicNum = 4);
        bool BuyRelic(RelicData relicData);
        void TriggerRelicEffect(RelicTriggerType triggerType,RelicEffectContext context);
        List<RelicData> GetOwnedRelicDatas();
        void ClearOwnedRelicDatas();
        void ResetAllRelicEffect();
    }
}