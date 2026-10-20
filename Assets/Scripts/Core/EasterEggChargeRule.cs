using System;

namespace Gambling.Core
{
    /// <summary>
    /// 彩蛋保底规则（纯计算，无引擎依赖）：把「转动次数」换算成百分比充能。
    ///
    /// 需求见 Docs/Requirements/SYS-001_彩蛋保底.md §4 / §8。这里只回答三件事：
    /// 一次普通转动涨多少、什么时候算满格、触发后回到什么状态；
    /// 随机数、UI、事件、跨关保留都不在这个类里。
    /// </summary>
    public static class EasterEggChargeRule
    {
        /// <summary>充能下限（百分比）。</summary>
        public const float MinProgress = 0f;

        /// <summary>充能上限（百分比）。</summary>
        public const float MaxProgress = 100f;

        /// <summary>
        /// 满格判定阈值。浮点累加会有误差（如 16.666… × 6 = 99.999），
        /// 用 99.99 做容差，避免差一点点永远不触发。
        /// </summary>
        public const float FullThreshold = 99.99f;

        /// <summary>
        /// 实际保底次数 N_eff = max(1, N - D)。
        /// N 小于 1 时按 1 处理（配置错误也不让充能出现除零/负步长）；
        /// D 小于 0 视为 0；N_eff 最小为 1。
        /// </summary>
        public static int EffectiveSpins(int requiredSpins, int spinsReduction)
        {
            int n = requiredSpins < 1 ? 1 : requiredSpins;
            int d = spinsReduction < 0 ? 0 : spinsReduction;
            int effective = n - d;
            return effective < 1 ? 1 : effective;
        }

        /// <summary>每次普通转动的充能增量（百分比）= 100 / N_eff。</summary>
        public static float Step(int requiredSpins, int spinsReduction)
        {
            return MaxProgress / EffectiveSpins(requiredSpins, spinsReduction);
        }

        /// <summary>把百分比夹到 [0, 100]；NaN 视为 0，避免脏数据把进度条写坏。</summary>
        public static float Clamp(float progress)
        {
            if (float.IsNaN(progress))
            {
                return MinProgress;
            }

            if (progress < MinProgress)
            {
                return MinProgress;
            }

            if (progress > MaxProgress)
            {
                return MaxProgress;
            }

            return progress;
        }

        /// <summary>
        /// 一次普通转动后的新进度：先夹住旧值再加步长，最后再夹一次。
        /// 用「加完就夹到 100」，这样最后一转必定落在 100 而不是 99.999…
        /// </summary>
        public static float AddProgress(float progress, int requiredSpins, int spinsReduction)
        {
            return Clamp(Clamp(progress) + Step(requiredSpins, spinsReduction));
        }

        /// <summary>是否已经满格（带容差）。</summary>
        public static bool IsFull(float progress)
        {
            return Clamp(progress) >= FullThreshold;
        }

        /// <summary>
        /// 下一次转动是否要强制落到彩蛋卡：保底被关闭时不强制。
        /// </summary>
        public static bool ShouldForceEasterEgg(bool enabled, float progress)
        {
            return enabled && IsFull(progress);
        }

        /// <summary>
        /// 遗物减免次数是「增量」，多个遗物自然累加（各 -1 次合计 -2 次）。
        /// 累加结果不允许为负，避免把 N_eff 反向抬到大于 N。
        /// </summary>
        public static int ApplySpinsReductionDelta(int currentReduction, int delta)
        {
            int safeCurrent = currentReduction < 0 ? 0 : currentReduction;
            int next = safeCurrent + delta;
            return next < 0 ? 0 : next;
        }
    }

    /// <summary>
    /// 彩蛋保底的初始状态：0% / D=0 / 保底开启。
    /// 单独抽出来是为了让「新开局重置」有唯一事实来源，也能被 EditMode 测试直接断言。
    /// </summary>
    public readonly struct EasterEggChargeState
    {
        public readonly float Progress;
        public readonly int SpinsReduction;
        public readonly bool Enabled;

        public EasterEggChargeState(float progress, int spinsReduction, bool enabled)
        {
            Progress = EasterEggChargeRule.Clamp(progress);
            SpinsReduction = spinsReduction < 0 ? 0 : spinsReduction;
            Enabled = enabled;
        }

        public static EasterEggChargeState Initial => new EasterEggChargeState(0f, 0, true);
    }
}
