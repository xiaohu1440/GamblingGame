using System.Collections.Generic;

namespace Gambling.Core
{
    /// <summary>
    /// 彩蛋保底的落点选择：从轮盘上**所有**彩蛋卡索引里等概率选一张。
    ///
    /// 轮盘允许保留多张彩蛋卡（当前 2 张，索引 9 / 21），保底触发时随机选一张，
    /// 不能固定落同一张。随机数由调用方传入，这里只做「取第几个」，方便测试。
    /// </summary>
    public static class EasterEggTargetPicker
    {
        /// <summary>没有可用彩蛋卡时的返回值，调用方据此回退权重抽取。</summary>
        public const int NoTarget = -1;

        /// <summary>
        /// 从 <paramref name="indices"/> 里选一个索引返回。
        /// 列表为空（或 null）返回 <see cref="NoTarget"/>；
        /// <paramref name="roll"/> 是 [0, count) 的整数随机数，越界时夹到合法范围，
        /// 避免异常随机源把落点算崩。
        /// </summary>
        public static int Pick(IReadOnlyList<int> indices, int roll)
        {
            if (indices == null || indices.Count == 0)
            {
                return NoTarget;
            }

            int safeRoll = roll;
            if (safeRoll < 0)
            {
                safeRoll = 0;
            }
            else if (safeRoll >= indices.Count)
            {
                safeRoll = indices.Count - 1;
            }

            return indices[safeRoll];
        }
    }
}
