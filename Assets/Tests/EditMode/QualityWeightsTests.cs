using Gambling.Core;
using NUnit.Framework;

namespace Gambling.Tests
{
    /// <summary>
    /// 抽卡权重的行为契约。数值改动（比如调概率）会让这些用例变红，提醒同步改文档/需求。
    /// </summary>
    public class QualityWeightsTests
    {
        private const float Tol = 0.0001f;

        [Test]
        public void Level1_OnlyCommon()
        {
            var w = QualityWeights.ForLevel(1);

            Assert.AreEqual(100f, w.Common, Tol);
            Assert.AreEqual(0f, w.Rare, Tol);
            Assert.AreEqual(0f, w.Epic, Tol);
            Assert.AreEqual(0f, w.Legendary, Tol);
        }

        [Test]
        public void Level0OrNegative_TreatedAsFirstLevel()
        {
            var w = QualityWeights.ForLevel(-3);

            Assert.AreEqual(100f, w.Common, Tol);
            Assert.AreEqual(0f, w.Total - w.Common, Tol);
        }

        [Test]
        public void Level2_ShiftsFivePointsFromCommon()
        {
            var w = QualityWeights.ForLevel(2);

            Assert.AreEqual(55f, w.Common, Tol);
            Assert.AreEqual(27.5f, w.Rare, Tol);
            Assert.AreEqual(11.5f, w.Epic, Tol);
            Assert.AreEqual(6f, w.Legendary, Tol);
        }

        [Test]
        public void Level5_MatchesDesignTable()
        {
            var w = QualityWeights.ForLevel(5);

            Assert.AreEqual(40f, w.Common, Tol);
            Assert.AreEqual(35f, w.Rare, Tol);
            Assert.AreEqual(16f, w.Epic, Tol);
            Assert.AreEqual(9f, w.Legendary, Tol);
            Assert.AreEqual(100f, w.Total, Tol, "第 5 关权重应刚好凑满 100");
        }

        [Test]
        public void Level20_RespectsCaps()
        {
            var w = QualityWeights.ForLevel(20);

            Assert.AreEqual(20f, w.Common, Tol, "白装下限 20");
            Assert.AreEqual(40f, w.Rare, Tol, "稀有封顶 40");
            Assert.AreEqual(25f, w.Epic, Tol, "史诗封顶 25");
            Assert.AreEqual(15f, w.Legendary, Tol, "传说封顶 15");
        }

        [Test]
        public void CommonWeight_NeverIncreasesWithLevel()
        {
            float previous = float.MaxValue;
            for (int level = 1; level <= 30; level++)
            {
                float common = QualityWeights.ForLevel(level).Common;
                Assert.LessOrEqual(common, previous, $"第 {level} 关白装权重不该比上一关高");
                previous = common;
            }
        }

        [Test]
        public void IndexerAndToArray_AgreeWithProperties()
        {
            var w = new QualityWeights(1f, 2f, 3f, 4f);

            Assert.AreEqual(w.Common, w[0], Tol);
            Assert.AreEqual(w.Rare, w[1], Tol);
            Assert.AreEqual(w.Epic, w[2], Tol);
            Assert.AreEqual(w.Legendary, w[3], Tol);
            CollectionAssert.AreEqual(new[] { 1f, 2f, 3f, 4f }, w.ToArray());
            Assert.AreEqual(10f, w.Total, Tol);
        }
    }

    /// <summary>
    /// 权重挑选：随机数由外部传入，所以每个用例都能精确断言边界。
    /// </summary>
    public class WeightedPickerTests
    {
        private static readonly float[] Level5 = { 40f, 35f, 16f, 9f };

        [Test]
        public void SinglePositiveWeight_AlwaysWins()
        {
            var weights = new[] { 0f, 0f, 0f, 7f };

            Assert.AreEqual(3, WeightedPicker.PickIndex(weights, 0f));
            Assert.AreEqual(3, WeightedPicker.PickIndex(weights, 0.5f));
            Assert.AreEqual(3, WeightedPicker.PickIndex(weights, 0.999f));
        }

        [Test]
        public void RollZero_PicksFirstPositiveBand()
        {
            Assert.AreEqual(0, WeightedPicker.PickIndex(Level5, 0f));
        }

        [Test]
        public void RollJustBelowBoundary_StaysInFirstBand()
        {
            Assert.AreEqual(0, WeightedPicker.PickIndex(Level5, 0.3999f));
        }

        [Test]
        public void RollJustAboveBoundary_MovesToSecondBand()
        {
            Assert.AreEqual(1, WeightedPicker.PickIndex(Level5, 0.4001f));
        }

        [Test]
        public void RollNearOne_PicksLastBand()
        {
            Assert.AreEqual(3, WeightedPicker.PickIndex(Level5, 0.999f));
        }

        [Test]
        public void ZeroWeights_AreSkipped()
        {
            var weights = new[] { 0f, 25f, 75f, 0f };

            Assert.AreEqual(1, WeightedPicker.PickIndex(weights, 0f), "0 权重不该被选中");
            Assert.AreEqual(2, WeightedPicker.PickIndex(weights, 0.5f));
        }

        [Test]
        public void AllZeroOrEmpty_FallsBackToFirstIndex()
        {
            Assert.AreEqual(0, WeightedPicker.PickIndex(new[] { 0f, 0f, 0f, 0f }, 0.5f));
            Assert.AreEqual(0, WeightedPicker.PickIndex(new float[0], 0.5f));
            Assert.AreEqual(0, WeightedPicker.PickIndex(null, 0.5f));
        }

        [Test]
        public void OutOfRangeRoll_IsClamped()
        {
            Assert.AreEqual(0, WeightedPicker.PickIndex(Level5, -1f));
            Assert.AreEqual(3, WeightedPicker.PickIndex(Level5, 1f));
        }

        [Test]
        public void SameRoll_IsDeterministic()
        {
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(
                    WeightedPicker.PickIndex(Level5, 0.42f),
                    WeightedPicker.PickIndex(Level5, 0.42f));
            }
        }
    }
}
