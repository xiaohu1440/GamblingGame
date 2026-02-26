using UnityEngine;
using QFramework;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Gambling
{
    // 附魔触发时机
    public enum EnchantmentTriggerType
    {
        OnLand,        // 转动停止落位时
        OnSpinStart    // 转动开始时
    }
    /// <summary>
    /// 附魔效果基类
    /// </summary>
    public abstract class EnchantmentEffect:ScriptableObject
    {
        public abstract string EnchantmentName { get; }
        public string Description;
        public Sprite Icon;
        public int RemainingTurns = -1; // -1 表示永久，>0 表示有限次数
        public abstract bool NeedScoreCheck { get; } // 是否需要检测押注得分
        public abstract void Execute(CardItem cardItem, GamblingGround gamblingGround);
        // 减少次数并检查是否结束
        public bool Tick()
        {
            if (RemainingTurns > 0)
            {
                RemainingTurns--;
                return RemainingTurns == 0;
            }
            return false;
        }
    }
}