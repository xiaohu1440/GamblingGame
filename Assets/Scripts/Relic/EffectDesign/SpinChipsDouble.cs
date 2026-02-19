using System;
using UnityEngine;
namespace Gambling
{
    /// <summary>
    /// 转动指定类型卡片时加分
    /// </summary>
    [CreateAssetMenu(fileName = "SpinChipsDoubleEffect", menuName = "Gambling/Effects/转动次数筹码翻倍效果")]
    public class SpinChipsDouble:RelicEffect
    {
        [Header("转动检测")]
        [Tooltip("根据转动次数翻倍筹码")]
        [SerializeField] private int spinDoubleNum = 3;
        private bool isFirstExecution = true;
        private int frontSpinCount;

        public override string RelicName => "根据转动次数筹码翻倍效果";
        public override bool CanTrigger(RelicTriggerType triggerType)
        {
            return triggerType == RelicTriggerType.OnRotationEnd;
        }

        public override void ExecuteEffect(RelicEffectContext context)
        {
            if (isFirstExecution)
            {
                frontSpinCount = context.currentRotationNum-1;
                isFirstExecution = false;
                //Debug.Log("第一次执行,转动次数:"+(context.currentRotationNum-frontSpinCount));
            }
            else
            {
                if (context.currentCard != null)
                {
                    if ((context.currentRotationNum-frontSpinCount) % spinDoubleNum == 0)
                    {
                        context.gamblingGround.chipsDouble = 2;
                        //Debug.Log("根据转动次数筹码翻倍效果触发,翻倍为:"+context.gamblingGround.chipsDouble);
                    }
                    else
                    {
                        context.gamblingGround.chipsDouble = 1;
                    }
                }
            }
            Debug.Log("测试转动次数:"+(context.currentRotationNum-frontSpinCount));
            
                
        }

        public override void Reset()
        {
            isFirstExecution = true;
            frontSpinCount = 0;
            Debug.Log("🔄 SpinChipsDouble遗物已重置");
        }


    }
}
