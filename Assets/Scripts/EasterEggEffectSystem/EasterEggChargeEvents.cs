namespace Gambling
{
    /// <summary>
    /// 彩蛋充能变化 / 触发广播事件。UI、遗物、成就等只读监听，
    /// 不要直接读写 <c>Global.easterEggProgress</c>。
    /// </summary>
    public struct EasterEggChargeChangedEvent
    {
        /// <summary>变化后的充能百分比（0–100）。</summary>
        public float Progress;

        /// <summary>当前关卡；仅用于展示与统计，不参与保底判定。</summary>
        public int Level;

        /// <summary>true 表示本次是「落到彩蛋卡触发，充能已归 0」；false 表示普通累加。</summary>
        public bool Triggered;
    }

    /// <summary>
    /// 遗物请求修改彩蛋保底规则的事件。规则只能这样改，禁止遗物直接写
    /// <c>Global.easterEggProgress</c> / <c>Global.easterEggSpinsReduction</c>。
    ///
    /// 注意：struct 的 bool 默认值是 false。只想减少所需次数时，必须显式写
    /// <c>Enabled = true</c>，否则会被当成「关闭保底」处理。
    /// </summary>
    public struct EasterEggChargeRuleEvent
    {
        /// <summary>减免次数的增量：+1 = 少要 1 次转动；多个遗物各发一次，自然累加（各 -1 即合计 -2）。</summary>
        public int SpinsReductionDelta;

        /// <summary>保底开关：false = 关闭（不再累加、不强制落点、隐藏进度条，已积累百分比保留）。</summary>
        public bool Enabled;
    }
}
