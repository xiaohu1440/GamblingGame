using System;

namespace Gambling.Core
{
    /// <summary>
    /// 关卡进度规则：死亡线 / 強欲线只统计「进入本关之后」获得的分数。
    /// 历史总分单独累计（后续可作为货币消耗），两者不能混用。
    ///
    /// 注意：needed 的数值仍由 <c>Global.levelScore</c> / <c>Global.greedlevelScore</c> 决定，
    /// 这里只负责「拿哪个分数去比」——要调难度改那两个值，不要改这里。
    /// </summary>
    public static class LevelScoreRule
    {
        /// <summary>
        /// 本关已获得的分数 = 当前总分 - 进入本关时的总分。
        /// 跨关不重置总分，所以必须靠 levelStartScore 做基线；基线缺失时返回 0 而不是负数。
        /// </summary>
        public static int LevelProgress(int totalScore, int levelStartScore)
        {
            int progress = totalScore - levelStartScore;
            return progress > 0 ? progress : 0;
        }

        /// <summary>本关进度是否达到某个门槛（死亡线 / 強欲线）。</summary>
        public static bool Reached(int totalScore, int levelStartScore, int required)
        {
            if (required <= 0)
            {
                return true;
            }

            return LevelProgress(totalScore, levelStartScore) >= required;
        }

        /// <summary>
        /// 进入新关卡时要把基线推到当前总分；返回新基线，便于调用方直接赋值。
        /// </summary>
        public static int BeginLevel(int totalScore)
        {
            return totalScore;
        }
    }
}
