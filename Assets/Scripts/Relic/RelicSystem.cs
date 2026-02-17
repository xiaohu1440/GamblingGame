using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework;
namespace Gambling
{
    public class RelicSystem:AbstractSystem,IRelicSystem
    {
        private List<RelicData> allRelicDatas = new List<RelicData>();//所有遗物
        private List<RelicData> ownedRelicDatas = new List<RelicData>();//玩家背包
        protected override void OnInit()
        {
            LoadRelicsFromResources();
        }

        private void LoadRelicsFromResources()
        {
            RelicData[] relicDatas = Resources.LoadAll<RelicData>("Relics");
            allRelicDatas.AddRange(relicDatas);
            Debug.Log($"✅ 遗物系统初始化完成，加载了 {allRelicDatas.Count} 个遗物");
        }

        public List<RelicData> DrawRelicsForShop(int currentLevel, int relicNum = 4)
        {
           var unLockedRelics = allRelicDatas.Where(r => r.unLock&&!ownedRelicDatas.Contains(r)).ToList();
           if (unLockedRelics.Count == 0)
           {
               Debug.LogWarning("⚠️ 没有可用的遗物！");
               return new List<RelicData>();
           }
           var qualityWeights = CalculateQualityWeights(currentLevel);
           List<RelicData> drawnRelics = new List<RelicData>();
           var currentdrawnRelics = Mathf.Min(relicNum, unLockedRelics.Count);
           for (int i = 0; i < currentdrawnRelics; i++)
           {
               RelicQuality quality = SelectQualityByWeight(qualityWeights);
               RelicData relic = SelectRelicByQuality(unLockedRelics, quality);
                
               // 避免重复
               if (relic != null && !drawnRelics.Contains(relic))
               {
                   drawnRelics.Add(relic);
               }
               else
               {
                   i--; // 重新抽一次
               }
           }
            
           return drawnRelics;
           
        }
        /// <summary>
        /// 计算品质权重
        /// </summary>
        private Dictionary<RelicQuality, float> CalculateQualityWeights(int level)
        {
            Dictionary<RelicQuality, float> weights = new Dictionary<RelicQuality, float>();
            
            if (level == 1)
            {
                // 第1关：100%白色
                weights[RelicQuality.Common] = 100f;
                weights[RelicQuality.Rare] = 0f;
                weights[RelicQuality.Epic] = 0f;
                weights[RelicQuality.Legendary] = 0f;
            }
            else
            {
                // 每关+5%高品质概率
                float bonus = (level - 1) * 5f;
                
                weights[RelicQuality.Common] = Mathf.Max(60f - bonus, 20f);
                weights[RelicQuality.Rare] = Mathf.Min(25f + bonus * 0.5f, 40f);
                weights[RelicQuality.Epic] = Mathf.Min(10f + bonus * 0.3f, 25f);
                weights[RelicQuality.Legendary] = Mathf.Min(5f + bonus * 0.2f, 15f);
            }
            
            return weights;
        }
        /// <summary>
        /// 根据权重选择品质
        /// </summary>
        private RelicQuality SelectQualityByWeight(Dictionary<RelicQuality, float> weights)
        {
            float total = weights.Values.Sum();
            float random = Random.Range(0f, total);
            float current = 0f;
            
            foreach (var kvp in weights)
            {
                current += kvp.Value;
                if (random <= current)
                {
                    return kvp.Key;
                }
            }
            
            return RelicQuality.Common;
        }
        /// <summary>
        /// 从指定品质中随机选择遗物
        /// </summary>
        private RelicData SelectRelicByQuality(List<RelicData> relics, RelicQuality quality)
        {
            var filtered = relics.Where(r => r.RelicQuality == quality).ToList();
            
            if (filtered.Count == 0)
            {
                // 降级选择
                return relics[Random.Range(0, relics.Count)];
            }
            
            return filtered[Random.Range(0, filtered.Count)];
        }

        public bool BuyRelic(RelicData relicData)
        {
            if (Global.lotteryTicket.Value >= (relicData.RelicPrice+2))
            {
                Global.lotteryTicket.Value -= relicData.RelicPrice;
                ownedRelicDatas.Add(relicData);
                Debug.Log($"✅ 成功购买遗物: {relicData.RelicName}");
                return true;
            }
            else
            {
                return false;
                Debug.Log($"❌ 抽选卷不足，无法购买: {relicData.RelicName}");
            }
        }

        public void TriggerRelicEffect(RelicTriggerType triggerType, RelicEffectContext context)
        {
            foreach (var relic in ownedRelicDatas)
            {
                if (relic.relicEffect != null && relic.relicEffect.IsEnabled &&
                    relic.relicEffect.CanTrigger(triggerType))
                {
                    relic.relicEffect.ExecuteEffect(context);
                }
            }
        }

        public List<RelicData> GetOwnedRelicDatas()
        {
           return ownedRelicDatas;
        }

        public void ClearOwnedRelicDatas()
        {
            ownedRelicDatas.Clear();
            Debug.Log("🗑️ 已清空背包");
        }
    }
}