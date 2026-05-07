using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace Gambling
{
    public interface IPachinsoSystem : ISystem
    {
        // 存储 3 次转动的结果，每次转动包含 3 个图案的 RewardData
        List<RewardData[]> SpinHistory { get; }
        // 存储 3 次转动时的押注倍数（基于对应图案按钮的点击次数）
        
        void AddSpinResult(RewardData[] columnData);
        void ClearData();
    }

    public class PachinsoSystem : AbstractSystem, IPachinsoSystem
    {
        public List<RewardData[]> SpinHistory { get; } = new List<RewardData[]>();

        protected override void OnInit() { }

        public void AddSpinResult(RewardData[] columnData)
        {
            SpinHistory.Add(columnData);
        }

        public void ClearData()
        {
            SpinHistory.Clear();
        }
    }
}