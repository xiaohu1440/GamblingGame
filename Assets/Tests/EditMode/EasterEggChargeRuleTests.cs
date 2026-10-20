using Gambling.Core;
using NUnit.Framework;

namespace Gambling.Tests
{
    /// <summary>
    /// 彩蛋保底充能规则：百分比增量、N_eff 下限、减免累加、满格容差、clamp、新开局重置。
    /// 对应需求 Docs/Requirements/SYS-001_彩蛋保底.md §4 / §8 的不变量 1、7、8、11。
    /// </summary>
    public class EasterEggChargeRuleTests
    {
        private const float Epsilon = 0.0001f;

        [Test]
        public void Step_DefaultFourSpins_IsQuarterBar()
        {
            // N=4、无遗物：每次 +25%
            Assert.AreEqual(25f, EasterEggChargeRule.Step(4, 0), Epsilon);
        }

        [Test]
        public void Step_SixSpins_ReachesFullOnSixthSpin()
        {
            // 16.666… × 6 在浮点下等于 99.999…，靠 99.99 容差与 clamp 保证第 6 次满格
            float progress = 0f;
            for (int i = 0; i < 5; i++)
            {
                progress = EasterEggChargeRule.AddProgress(progress, 6, 0);
            }

            Assert.IsFalse(EasterEggChargeRule.IsFull(progress), "第 5 次还不该满格");
            Assert.AreEqual(83.3333f, progress, 0.01f);

            progress = EasterEggChargeRule.AddProgress(progress, 6, 0);
            Assert.AreEqual(100f, progress, Epsilon, "第 6 次必须落到 100，不能卡在 99.999");
            Assert.IsTrue(EasterEggChargeRule.IsFull(progress));
        }

        [Test]
        public void EffectiveSpins_NeverDropsBelowOne()
        {
            Assert.AreEqual(4, EasterEggChargeRule.EffectiveSpins(4, 0));
            Assert.AreEqual(2, EasterEggChargeRule.EffectiveSpins(4, 2));
            Assert.AreEqual(1, EasterEggChargeRule.EffectiveSpins(4, 4));
            Assert.AreEqual(1, EasterEggChargeRule.EffectiveSpins(4, 99), "D 再大 N_eff 也必须是 1");
        }

        [Test]
        public void EffectiveSpins_InvalidConfigIsClamped()
        {
            // N 配置错误（< 1）按 1；D 为负按 0，避免除零/负步长
            Assert.AreEqual(1, EasterEggChargeRule.EffectiveSpins(0, 0));
            Assert.AreEqual(1, EasterEggChargeRule.EffectiveSpins(-3, 0));
            Assert.AreEqual(4, EasterEggChargeRule.EffectiveSpins(4, -5));
        }

        [Test]
        public void SpinsReduction_AccumulatesAcrossRelics()
        {
            // 两个遗物各 -1 次：D 累加到 2，充能步长变成 50%
            int reduction = EasterEggChargeRule.ApplySpinsReductionDelta(0, 1);
            reduction = EasterEggChargeRule.ApplySpinsReductionDelta(reduction, 1);

            Assert.AreEqual(2, reduction);
            Assert.AreEqual(50f, EasterEggChargeRule.Step(4, reduction), Epsilon);
        }

        [Test]
        public void SpinsReductionNeverGoesNegative()
        {
            Assert.AreEqual(0, EasterEggChargeRule.ApplySpinsReductionDelta(0, -1));
            Assert.AreEqual(0, EasterEggChargeRule.ApplySpinsReductionDelta(1, -5));
            Assert.AreEqual(3, EasterEggChargeRule.ApplySpinsReductionDelta(2, 1));
        }

        [Test]
        public void IsFull_UsesTolerance()
        {
            Assert.IsFalse(EasterEggChargeRule.IsFull(99.98f));
            Assert.IsTrue(EasterEggChargeRule.IsFull(99.99f));
            Assert.IsTrue(EasterEggChargeRule.IsFull(100f));
            Assert.IsFalse(EasterEggChargeRule.IsFull(0f));
        }

        [Test]
        public void AddProgress_ClampsToHundred()
        {
            // 90% + 25% 应停在 100%，不能溢出
            Assert.AreEqual(100f, EasterEggChargeRule.AddProgress(90f, 4, 0), Epsilon);
            Assert.AreEqual(100f, EasterEggChargeRule.AddProgress(100f, 4, 0), Epsilon);
        }

        [Test]
        public void AddProgress_NeverGoesBelowZero()
        {
            Assert.AreEqual(0f, EasterEggChargeRule.Clamp(-20f), Epsilon);
            Assert.AreEqual(0f, EasterEggChargeRule.Clamp(float.NaN), Epsilon);
            // 脏数据先夹到 0 再加一步，结果不会出现负充能
            Assert.AreEqual(25f, EasterEggChargeRule.AddProgress(-99f, 4, 0), Epsilon);
        }

        [Test]
        public void ShouldForceEasterEgg_NeedsEnabledAndFull()
        {
            Assert.IsTrue(EasterEggChargeRule.ShouldForceEasterEgg(true, 100f));
            Assert.IsFalse(EasterEggChargeRule.ShouldForceEasterEgg(false, 100f), "保底关闭时不强制落点");
            Assert.IsFalse(EasterEggChargeRule.ShouldForceEasterEgg(true, 75f));
        }

        [Test]
        public void InitialState_ResetsProgressReductionAndSwitch()
        {
            // GamblingGround.Init() 用这份默认值重置：0% / D=0 / 开启
            EasterEggChargeState state = EasterEggChargeState.Initial;

            Assert.AreEqual(0f, state.Progress, Epsilon);
            Assert.AreEqual(0, state.SpinsReduction);
            Assert.IsTrue(state.Enabled);
        }
    }
}
