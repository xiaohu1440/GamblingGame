using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace Gambling
{
    [CreateAssetMenu(fileName = "EnchantmentMasterRelic", menuName = "Gambling/Effects/附魔大师遗物")]
    public class EnchantmentMasterRelic : RelicEffect
    {
        public override string RelicName => "附魔大师";
        [SerializeField] private int EnchantmentNum = 2;
        
        // 用于记录已经贡献过倍率的格子索引，防止重复增加
        private HashSet<int> processedIndices = new HashSet<int>();

        public override bool CanTrigger(RelicTriggerType triggerType)
        {
            // 在转动开始前触发
            return triggerType == RelicTriggerType.OnRotationStart;
        }

        public override void ExecuteEffect(RelicEffectContext context)
        {
            int newlyDetectedCount = 0;
            
            // 1. 遍历棋盘上所有的格子
            foreach (var cardItem in context.gamblingGround.cardItems)
            {
                // 如果这个索引已经处理过，跳过
                if (processedIndices.Contains(cardItem.index)) continue;

                // 2. 获取格子上的附魔组件
                var enchantmentComp = cardItem.GetComponent<EnchantmentComponent>();
                
                if (enchantmentComp != null)
                {
                    // 3. 检测附魔数量是否大于等于设定值
                    if (enchantmentComp.ActiveEffects.Count >= EnchantmentNum)
                    {
                        newlyDetectedCount++;
                        // 记录该索引，确保下次不再触发
                        processedIndices.Add(cardItem.index);
                    }
                }
            }

            // 4. 只有发现“新”的满足条件的图案时，才增加全局倍率
            if (newlyDetectedCount > 0)
            {
                context.gamblingGround.globalDoubleNum.Value += newlyDetectedCount;
                
                Debug.Log($"[遗物触发] 附魔大师检测到 {newlyDetectedCount} 个新增达标图案，全局倍率永久 +{newlyDetectedCount}。当前总加成索引数: {processedIndices.Count}");
            }
        }

        public override void Reset()
        {
            // 当游戏重置或进入新关卡时（取决于你的 RelicSystem 逻辑），清空记录
            processedIndices.Clear();
        }
    }
}