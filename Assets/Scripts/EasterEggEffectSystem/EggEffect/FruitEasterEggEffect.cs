using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using QFramework.Example;

namespace Gambling
{
    [CreateAssetMenu(fileName = "FruitEasterEggEffect", menuName = "Gambling/EasterEgg/FruitEffect")]
    public class FruitEasterEggEffect:EasterEggEffect
    {
        [Header("水果彩蛋配置-包含哪些卡片")]
        [SerializeField] private bool includeBell = true;
        [SerializeField] private bool includeOrange = true;
        [SerializeField] private bool includeBlueberry = true;

        public override string EasterEggName => "水果彩蛋";
        public override bool CanTrigger(RewardData currentCard, List<CardItem> allCards)
        {
            return currentCard.runtimeCardType == CardItem.CardType.彩蛋;
        }

        public override List<CardItem> GetTargetCards(List<CardItem> allCards)
        {
            List<CardItem> targetCards = new List<CardItem>();
            foreach (var card in allCards)
            {
                if (card.rewardData.runtimeCardType == CardItem.CardType.水果 && !card.rewardData.runTimeisMini)
                {
                    string fruitName = card.rewardData.runtimeRewardName.Value;
                    bool shouldInclude = false;
                    if (includeBell && fruitName.Contains("铃铛")) shouldInclude = true;
                    else if (includeOrange && fruitName.Contains("橘子")) shouldInclude = true;
                    else if (includeBlueberry && fruitName.Contains("蓝莓")) shouldInclude = true;
                    if (shouldInclude) targetCards.Add(card);
                }
            }
            return targetCards;

        }

        public override IEnumerator ExecuteEffect(GamblingGround gamblingGround, int currentIndex, List<CardItem> targetCards,Dictionary<string, int> savedButtonCounts = null)
        {
            bool shouldResetAtEnd = true;

            Debug.Log("🍎 水果彩蛋效果开始执行！");
            
            // 1. 当前彩蛋位置闪烁1秒
            yield return EasterEggEffectHelper.FlashCurrentPosition(gamblingGround, 1f);

            GameObject lastSelectBox = gamblingGround.SelectBox;
            
            // 2. 依次触发每个水果卡片
            for (int i = 0; i < targetCards.Count; i++)
            {
                var targetCard = targetCards[i];
                Debug.Log($"🍎 正在处理第 {i + 1}/{targetCards.Count} 个水果卡片: {targetCard.rewardData.runtimeRewardName.Value}");
                
                // 创建新的框选框从彩蛋位置出发
                GameObject newSelectBox = EasterEggEffectHelper.CreateNewSelectBox(gamblingGround, currentIndex);
                shouldResetAtEnd = (i == targetCards.Count - 1);
                // 按索引依次移动到目标卡片
                yield return EasterEggEffectHelper.MoveSelectBoxToTarget(
                    newSelectBox, gamblingGround, currentIndex, targetCard.index,shouldResetAtEnd
                );
                
                // 触发卡片效果
                EasterEggEffectHelper.TriggerCardEffect(gamblingGround, targetCard,savedButtonCounts);
                
                // 更新最后的框选框引用
                lastSelectBox = newSelectBox;
                
                // 每个卡片之间稍微间隔一下
                yield return new WaitForSeconds(0.2f);
            }
            gamblingGround.currentDoubleNum.Value = 1;
            if (gamblingGround.Score.Value < Global.levelScore.Value && Global.lotteryTicket.Value <= 0)
            {
                UIKit.ClosePanel<UIGamePanel>();
                UIKit.OpenPanel<UIGameOverPanel>();
            }

            // 3. 设置最后生成的框选框为当前框选框
            if (targetCards.Count > 0)
            {
                int finalIndex = targetCards[targetCards.Count - 1].index;
                gamblingGround.SetCurrentSelectBox(lastSelectBox, finalIndex);
                Debug.Log($"🍎 水果彩蛋效果完成！框选框停留在索引 {finalIndex}");
            }
            else
            {
                //gamblingGround.SetEasterEggExecuting(false);
                Debug.Log("🍎 水果彩蛋效果完成，但没有找到目标卡片");
            }
        }
    }
}