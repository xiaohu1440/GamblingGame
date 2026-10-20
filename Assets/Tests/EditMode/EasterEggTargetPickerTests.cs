using Gambling.Core;
using NUnit.Framework;

namespace Gambling.Tests
{
    /// <summary>
    /// 保底落点选择：多张彩蛋卡时按 roll 等概率选中对应索引，单张固定，0 张返回 -1。
    /// 对应 SYS-001 v1.2 §8 不变量 4 与 QA-013。
    /// </summary>
    public class EasterEggTargetPickerTests
    {
        private static readonly int[] TwoEggCards = { 9, 21 };
        private static readonly int[] ThreeEggCards = { 3, 9, 21 };

        [Test]
        public void Pick_SingleIndex_AlwaysReturnsIt()
        {
            int[] single = { 9 };

            // 单张时无论 roll 取什么（包括越界脏数据）都固定这一张
            Assert.AreEqual(9, EasterEggTargetPicker.Pick(single, 0));
            Assert.AreEqual(9, EasterEggTargetPicker.Pick(single, 5));
            Assert.AreEqual(9, EasterEggTargetPicker.Pick(single, -3));
        }

        [Test]
        public void Pick_TwoIndices_MapsRollToEachCard()
        {
            // 两张彩蛋卡：roll=0 → 9，roll=1 → 21（顺序即收集顺序）
            Assert.AreEqual(9, EasterEggTargetPicker.Pick(TwoEggCards, 0));
            Assert.AreEqual(21, EasterEggTargetPicker.Pick(TwoEggCards, 1));
        }

        [Test]
        public void Pick_ThreeIndices_MapsRollToEachCard()
        {
            Assert.AreEqual(3, EasterEggTargetPicker.Pick(ThreeEggCards, 0));
            Assert.AreEqual(9, EasterEggTargetPicker.Pick(ThreeEggCards, 1));
            Assert.AreEqual(21, EasterEggTargetPicker.Pick(ThreeEggCards, 2));
        }

        [Test]
        public void Pick_EmptyOrNull_ReturnsNoTarget()
        {
            Assert.AreEqual(EasterEggTargetPicker.NoTarget, EasterEggTargetPicker.Pick(null, 0));
            Assert.AreEqual(EasterEggTargetPicker.NoTarget, EasterEggTargetPicker.Pick(new int[0], 0));
        }

        [Test]
        public void Pick_OutOfRangeRoll_IsClamped()
        {
            Assert.AreEqual(9, EasterEggTargetPicker.Pick(TwoEggCards, -1), "负数 roll 夹到第 0 个");
            Assert.AreEqual(21, EasterEggTargetPicker.Pick(TwoEggCards, 99), "越界 roll 夹到最后一个");
        }
    }
}
