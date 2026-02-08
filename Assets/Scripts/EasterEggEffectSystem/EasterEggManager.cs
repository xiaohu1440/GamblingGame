using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Gambling
{
    public class EasterEggManager : MonoBehaviour
    {
        [Header("彩蛋效果配置")]
        [SerializeField]private List<EasterEggEffect> easterEggEffects =new List<EasterEggEffect>();
        [Header("随机选择设置")]
        [SerializeField] private bool useRandomSelection = true; // 是否使用随机选择


        public bool TryTriggerEasterEgg(GamblingGround gamblingGround, CardItem currentCard, List<CardItem> allCards,Dictionary<string,int>savedButtonCounts=null)
        {
            List<EasterEggEffect> availableEffects = GetAvailableEffects(currentCard.rewardData, allCards);
            if (availableEffects.Count == 0)
            {
                return false;
            }
            EasterEggEffect selectedEffect = useRandomSelection ? 
                SelectRandomEffect(availableEffects) : 
                SelectFirstEffect(availableEffects);
            var targetCards = selectedEffect.GetTargetCards(allCards);
            if (targetCards.Count > 0)
            {
                StartCoroutine(selectedEffect.ExecuteEffect(gamblingGround, currentCard.index, targetCards,savedButtonCounts));
                return true;
            }
            else
            {
                return false;
            }
            
        }
        /// <summary>
        /// 获取所有可触发的彩蛋效果
        /// </summary>
        private List<EasterEggEffect> GetAvailableEffects(RewardData currentCard, List<CardItem> allCards)
        {
            List<EasterEggEffect> availableEffects = new List<EasterEggEffect>();
            
            foreach (var effect in easterEggEffects)
            {
                // 检查效果是否启用且可以触发
                if (effect != null && effect.IsEnabled && effect.CanTrigger(currentCard, allCards))
                {
                    // 预检查是否有有效目标卡片
                    var targetCards = effect.GetTargetCards(allCards);
                    if (targetCards.Count > 0)
                    {
                        availableEffects.Add(effect);
                    }
                }
            }
            
            return availableEffects;
        }
        /// <summary>
        /// 根据权重随机选择一个彩蛋效果
        /// </summary>
        private EasterEggEffect SelectRandomEffect(List<EasterEggEffect> availableEffects)
        {
            if (availableEffects.Count == 0) return null;
            if (availableEffects.Count == 1) return availableEffects[0];
            
            // 计算总权重
            float totalWeight = availableEffects.Sum(effect => effect.TriggerWeight);
            
            if (totalWeight <= 0f)
            {
                // 如果所有权重都为0，则等概率随机选择
                int randomIndex = Random.Range(0, availableEffects.Count);
                return availableEffects[randomIndex];
            }
            
            // 根据权重进行随机选择
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var effect in availableEffects)
            {
                currentWeight += effect.TriggerWeight;
                if (randomValue <= currentWeight)
                {
                    return effect;
                }
            }
            
            // 如果出现浮点数精度问题，返回最后一个
            return availableEffects[availableEffects.Count - 1];
        }
        /// <summary>
        /// 选择第一个可用的彩蛋效果（非随机模式）
        /// </summary>
        private EasterEggEffect SelectFirstEffect(List<EasterEggEffect> availableEffects)
        {
            return availableEffects.Count > 0 ? availableEffects[0] : null;
        }

    }
}