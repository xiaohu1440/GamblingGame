using Gambling.Core;
using NUnit.Framework;

namespace Gambling.Tests
{
    /// <summary>
    /// 关卡进度只算「本关内」得分，历史总分继续累加——这两条不能混。
    /// </summary>
    public class LevelScoreRuleTests
    {
        [Test]
        public void FreshLevel_ProgressStartsAtZero()
        {
            Assert.AreEqual(0, LevelScoreRule.LevelProgress(0, 0));
            Assert.AreEqual(0, LevelScoreRule.LevelProgress(120, 120));
        }

        [Test]
        public void Progress_IgnoresScoreEarnedBeforeTheLevel()
        {
            // 进关时已经带着 100 分，本关只赚了 20 分
            Assert.AreEqual(20, LevelScoreRule.LevelProgress(120, 100));
            Assert.IsFalse(LevelScoreRule.Reached(120, 100, 50), "旧逻辑会误判达标");
        }

        [Test]
        public void Progress_KeepsAccumulatingWithinTheLevel()
        {
            int start = 100;
            Assert.IsFalse(LevelScoreRule.Reached(100 + 9, start, 10));
            Assert.IsTrue(LevelScoreRule.Reached(100 + 10, start, 10));
            Assert.IsTrue(LevelScoreRule.Reached(100 + 30, start, 10));
        }

        [Test]
        public void Progress_NeverGoesNegative()
        {
            // 总分被清空/回退时不应出现负进度
            Assert.AreEqual(0, LevelScoreRule.LevelProgress(90, 100));
            Assert.AreEqual(0, LevelScoreRule.LevelProgress(-5, 0));
        }

        [Test]
        public void Reached_ZeroOrNegativeRequirement_IsAlwaysSatisfied()
        {
            Assert.IsTrue(LevelScoreRule.Reached(0, 0, 0));
            Assert.IsTrue(LevelScoreRule.Reached(5, 5, -1));
        }

        [Test]
        public void NewLevel_OnlyRetainsTotalButResetsProgress()
        {
            int total = 250;
            int start = LevelScoreRule.BeginLevel(total);

            Assert.AreEqual(0, LevelScoreRule.LevelProgress(total, start), "刚进关时本关进度应为 0");
            Assert.AreEqual(total, start, "进关基线应等于当时的总分，历史分数照旧保留");

            // 本关再赚 40 分
            Assert.AreEqual(40, LevelScoreRule.LevelProgress(total + 40, start));
        }
    }
}
