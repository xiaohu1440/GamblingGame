using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gambling
{
    public enum RelicQuality
    {
        Common=0,
        Rare=1,
        Epic=2,
        Legendary=3
    }

    public enum RelicTriggerType
    {
        OnRotationNum,
        OnBetting,
        OnLevelUp,
        OnRandom,
        OnDeath,
        OnRotationEnd,
        Passive
    }
    [CreateAssetMenu(fileName = "NewRelic", menuName = "Gambling/RelicItem")]
    public class RelicData : ScriptableObject
    {
        [Header("基础信息")]
        public string RelicName;
        public Sprite RelicIcon;
        public int RelicId;
        [Header("品质与价格")]
        public RelicQuality RelicQuality;
        public int RelicPrice;
        [Header("描述信息")]
        [TextArea(2, 4)]
        public string RelicDesc;
        [TextArea(2, 4)]
        public string RelicTips;
        [Header("触发条件")]
        public RelicTriggerType RelicTriggerType;
        [Header("解锁条件")]
        public bool unLock=true;
        [Header("效果配置")]
        public RelicEffect relicEffect;
    }
}


