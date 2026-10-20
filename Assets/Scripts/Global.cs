using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace Gambling
{
    public class Global : Architecture<Global>
    {

        public static BindableProperty<int> lotteryTicket = new BindableProperty<int>(10);
        public static BindableProperty<int> level = new BindableProperty<int>(1);
        public static BindableProperty<int> levelScore = new BindableProperty<int>(10);

        /// <summary>
        /// 进入本关时的总分基线。死亡线 / 強欲线按「当前总分 - 本基线」判定，
        /// 历史总分不重置（后续可作为货币消耗）。
        /// </summary>
        public static BindableProperty<int> levelStartScore = new BindableProperty<int>(0);

        /// <summary>
        /// 彩蛋保底充能百分比（0–100）。只存百分比不存次数，跨关保留；
        /// 新开局由 <c>GamblingGround.Init()</c> 归零。规则计算见 Gambling.Core.EasterEggChargeRule。
        /// </summary>
        public static BindableProperty<float> easterEggProgress = new BindableProperty<float>(0f);

        /// <summary>
        /// 遗物累计减免的转动次数 D，实际保底次数 N_eff = max(1, N - D)。
        /// 多个遗物累加（各减 1 次即 D=2）；新开局归零。
        /// </summary>
        public static BindableProperty<int> easterEggSpinsReduction = new BindableProperty<int>(0);

        /// <summary>
        /// 彩蛋保底开关，默认开启。遗物通过 EasterEggChargeRuleEvent 关闭后，
        /// 充能不再累加、不强制落点、进度条隐藏（已积累百分比保留）；新开局恢复 true。
        /// </summary>
        public static BindableProperty<bool> easterEggEnabled = new BindableProperty<bool>(true);

        public static BindableProperty<int> chips = new BindableProperty<int>(5);
        public static BindableProperty<int> currentLevelSpinCount = new BindableProperty<int>(0);
        public static BindableProperty<int> ticketReward=new BindableProperty<int>(12);
        public static BindableProperty<int> greedlevelScore=new BindableProperty<int>(30);
        protected override void Init()
        {
            this.RegisterSystem<IRelicSystem>(new RelicSystem());
            this.RegisterSystem<IEnchantmentSystem>(new EnchantmentSystem());
        }
    }
}

