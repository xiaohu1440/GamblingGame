using UnityEngine;

namespace Gambling
{
    /// <summary>
    /// 彩蛋保底配置：基础保底次数 N。
    /// 数值归执行策划调，代码只读；实际次数 = max(1, N - 遗物减免 D)，见
    /// <see cref="Gambling.Core.EasterEggChargeRule.EffectiveSpins"/>。
    /// </summary>
    [CreateAssetMenu(fileName = "EasterEggConfig", menuName = "Gambling/EasterEgg/彩蛋保底配置")]
    public class EasterEggConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip("基础保底次数 N：多少次普通转动充满（默认 4）。遗物减免会在此基础上减，实际次数最少 1 次。")]
        private int requiredSpins = 4;

        /// <summary>基础保底次数 N；编辑器里填错（小于 1）时按 1 返回，避免除零。</summary>
        public int RequiredSpins => requiredSpins < 1 ? 1 : requiredSpins;
    }
}
