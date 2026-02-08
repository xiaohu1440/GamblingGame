using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gambling
{
    /// <summary>
    /// 彩蛋效果基类
    /// </summary>
    public abstract class EasterEggEffect:ScriptableObject
    {
        [Header("基础配置")]
        [SerializeField] protected float triggerWeight = 1.0f; // 触发权重，用于随机选择
        [SerializeField] protected bool isEnabled = true; // 是否启用该彩蛋效果

        public abstract string EasterEggName { get; }
        /// <summary>
        /// 触发权重（用于随机选择）
        /// </summary>
        public float TriggerWeight => isEnabled ? triggerWeight : 0f;
        
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled => isEnabled;

        public abstract bool CanTrigger(RewardData currentCard, List<CardItem> allCards);
        public abstract List<CardItem> GetTargetCards(List<CardItem> allCards);

        public abstract IEnumerator ExecuteEffect(GamblingGround gamblingGround, int currentIndex, List<CardItem> targetCards,Dictionary<string, int> savedButtonCounts = null);
        
    }

}
