using System;

namespace Gambling.Core
{
    /// <summary>
    /// 按权重挑一个索引。随机数由调用方传入（<c>roll01</c> ∈ [0,1)），
    /// 这样同样的 roll 永远得到同样的结果，测试才能断言。
    /// </summary>
    public static class WeightedPicker
    {
        /// <summary>
        /// 返回被选中的下标。<paramref name="weights"/> 里的 0 和负数视为不参与抽取；
        /// 总权重为 0（或数组为空）时返回 0，调用方按「最低档」兜底。
        /// </summary>
        public static int PickIndex(float[] weights, float roll01)
        {
            if (weights == null || weights.Length == 0)
            {
                return 0;
            }

            float total = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] > 0f)
                {
                    total += weights[i];
                }
            }

            if (total <= 0f)
            {
                return 0;
            }

            float roll = roll01;
            if (float.IsNaN(roll) || roll < 0f) roll = 0f;
            if (roll >= 1f) roll = 1f;

            float target = roll * total;
            float accumulated = 0f;
            int lastPositive = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] <= 0f)
                {
                    continue;
                }

                lastPositive = i;
                accumulated += weights[i];
                if (target <= accumulated)
                {
                    return i;
                }
            }

            return lastPositive;
        }
    }
}
