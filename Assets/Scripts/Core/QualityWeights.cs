using System;

namespace Gambling.Core
{
    /// <summary>
    /// 四档遗物品质的抽取权重。
    /// 索引顺序与 <c>Gambling.RelicQuality</c> 一致（0 Common / 1 Rare / 2 Epic / 3 Legendary）。
    /// 放在无引擎依赖的 Gambling.Core 程序集里，EditMode 测试可以直接引用，
    /// 也避免把 QFramework / MonoBehaviour 拖进测试。
    /// </summary>
    public readonly struct QualityWeights
    {
        public const int Count = 4;

        public readonly float Common;
        public readonly float Rare;
        public readonly float Epic;
        public readonly float Legendary;

        public QualityWeights(float common, float rare, float epic, float legendary)
        {
            Common = common;
            Rare = rare;
            Epic = epic;
            Legendary = legendary;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Common;
                    case 1: return Rare;
                    case 2: return Epic;
                    case 3: return Legendary;
                    default: throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        public float Total => Common + Rare + Epic + Legendary;

        public float[] ToArray() => new[] { Common, Rare, Epic, Legendary };

        /// <summary>
        /// 按关卡算权重：第 1 关纯白；之后每关把 5 点权重从白往高品质挪，
        /// 各档有上下限（白不低于 20，稀有封顶 40，史诗 25，传说 15）。
        /// 这套数值原来硬编码在 RelicSystem 里，抽出来是为了能测。
        /// </summary>
        public static QualityWeights ForLevel(int level)
        {
            if (level <= 1)
            {
                return new QualityWeights(100f, 0f, 0f, 0f);
            }

            float bonus = (level - 1) * 5f;
            return new QualityWeights(
                Clamp(60f - bonus, 20f, 100f),
                Clamp(25f + bonus * 0.5f, 0f, 40f),
                Clamp(10f + bonus * 0.3f, 0f, 25f),
                Clamp(5f + bonus * 0.2f, 0f, 15f));
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
