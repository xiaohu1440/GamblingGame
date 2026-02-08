using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gambling
{
    /// <summary>
    /// 赌博彩蛋效果：当停在彩蛋卡片时，自动触发所有赌博类型卡片
    /// </summary>
    [CreateAssetMenu(fileName = "GamblingEasterEggEffect", menuName = "Gambling/EasterEgg/GamblingEffect")]
    public class GamblingEasterEggEffect : EasterEggEffect
    {
        [Header("赌博彩蛋配置")]
        [SerializeField] private bool includeDoubleStar = true;
        [SerializeField] private bool includeSevenSeven = true;
        [SerializeField] private bool includeWatermelon = true;
        
        public override string EasterEggName => "赌博狂潮彩蛋";

        public override bool CanTrigger(RewardData currentCard, List<CardItem> allCards)
        {
            return currentCard.runtimeCardType == CardItem.CardType.彩蛋;
        }

        public override List<CardItem> GetTargetCards(List<CardItem> allCards)
        {
            List<CardItem> targetCards = new List<CardItem>();
            
            foreach (var card in allCards)
            {
                if (card.rewardData.runtimeCardType == CardItem.CardType.赌博 && 
                    !card.rewardData.runTimeisMini)
                {
                    string cardName = card.rewardData.runtimeRewardName.Value;
                    
                    bool shouldInclude = false;
                    if (includeWatermelon && cardName.Contains("西瓜")) shouldInclude = true;
                    else if (includeDoubleStar && cardName.Contains("双星")) shouldInclude = true;
                    else if (includeSevenSeven && cardName.Contains("77")) shouldInclude = true;
                    
                    if (shouldInclude)
                    {
                        targetCards.Add(card);
                    }
                }
            }
            
            Debug.Log($"赌博彩蛋找到 {targetCards.Count} 个目标卡片");
            return targetCards;
        }

        public override IEnumerator ExecuteEffect(GamblingGround gamblingGround, int currentIndex, List<CardItem> targetCards,Dictionary<string, int> savedButtonCounts = null)
        {
            bool shouldResetAtEnd = true;
            Debug.Log("🎰 赌博彩蛋效果开始执行！");
            
            // 1. 当前彩蛋位置闪烁1秒
            yield return EasterEggEffectHelper.FlashCurrentPosition(gamblingGround, 1f);

            GameObject lastSelectBox = gamblingGround.SelectBox;
            
            // 2. 依次触发每个赌博卡片
            for (int i = 0; i < targetCards.Count; i++)
            {
                var targetCard = targetCards[i];
                Debug.Log($"🎰 正在处理第 {i + 1}/{targetCards.Count} 个赌博卡片: {targetCard.rewardData.runtimeRewardName.Value}");
                
                // 创建新的框选框从彩蛋位置出发
                GameObject newSelectBox = EasterEggEffectHelper.CreateNewSelectBox(gamblingGround, currentIndex);
                shouldResetAtEnd = (i == targetCards.Count - 1);
                // 按索引依次移动到目标卡片
                yield return EasterEggEffectHelper.MoveSelectBoxToTarget(
                    newSelectBox, gamblingGround, currentIndex, targetCard.index,shouldResetAtEnd);
                
                // 触发卡片效果
                EasterEggEffectHelper.TriggerCardEffect(gamblingGround, targetCard,savedButtonCounts
                );
                
                // 更新最后的框选框引用
                lastSelectBox = newSelectBox;
                
                // 每个卡片之间稍微间隔一下
                yield return new WaitForSeconds(0.2f);
            }

            // 3. 设置最后生成的框选框为当前框选框
            if (targetCards.Count > 0)
            {
                int finalIndex = targetCards[targetCards.Count - 1].index;
                gamblingGround.SetCurrentSelectBox(lastSelectBox, finalIndex);
                Debug.Log($"🎰 赌博彩蛋效果完成！框选框停留在索引 {finalIndex}");
            }
            else
            {
                //gamblingGround.SetEasterEggExecuting(false);
                Debug.Log("🎰 赌博彩蛋效果完成，但没有找到目标卡片");
            }
        }
    }
}