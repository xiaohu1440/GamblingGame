using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace Gambling
{
    public class ProcessPachinkoTurnCommand : AbstractCommand
    {
        private readonly int mCenterIndex;

        public ProcessPachinkoTurnCommand(int centerIndex)
        {
            mCenterIndex = centerIndex;
        }

        protected override void OnExecute()
        {
            var system = this.GetSystem<IPachinsoSystem>();
            var gamblingGround = Object.FindObjectOfType<GamblingGround>();
            int totalCount = gamblingGround.cardItems.Count;

            // 1. 获取以当前选中为中心的纵向三个图案
            int top = (mCenterIndex - 1 + totalCount) % totalCount;
            int center = mCenterIndex;
            int bottom = (mCenterIndex + 1) % totalCount;

            RewardData[] currentColumn = new RewardData[3] {
                gamblingGround.cardItems[top].rewardData,
                gamblingGround.cardItems[center].rewardData,
                gamblingGround.cardItems[bottom].rewardData
            };

            // 获取当前选中的图案所属类别的押注倍数（对应你提到的橘子押注次数）
            string category = gamblingGround.GetRewardNameCategory(currentColumn[1].runtimeRewardName.Value);
            int clickCount = gamblingGround.buttonClickCount[category].Value;

            // 记录数据
            system.AddSpinResult(currentColumn);

            // 发送 UI 更新事件（显示第一行/第一列等）
            this.SendEvent(new UpdatePachinkoUIEvent 
            { 
                RowData = currentColumn, 
                RowIndex = system.SpinHistory.Count - 1 
            });

            // 2. 每三次结算一次
            if (system.SpinHistory.Count >= 3)
            {
                CalculatePachinkoScore(system,gamblingGround);
                system.ClearData();
            }
        }

        private void CalculatePachinkoScore(IPachinsoSystem system, GamblingGround ground)
        {
            var history = system.SpinHistory; // List<RewardData[3]> -> 3列数据
            int totalTicket = 0;
            // 辅助方法：获取某个图案当前的押注点击数 (clickCount)
            System.Func<RewardData, int> GetBet = (data) => {
                string category = ground.GetRewardNameCategory(data.runtimeRewardName.Value);
                return ground.buttonClickCount.ContainsKey(category) ? ground.buttonClickCount[category].Value : 0;
            };

            // 构建 3x3 矩阵 [col][row]
            // history[0] 是第一列，history[1] 第二列，history[2] 第三列

            // --- 规则 1 & 2: 纵向结算 (3次押注相加 * RewardTicket) ---
            // 每一列（纵向）只要三个 ID 一样，就按 (押注1+押注2+押注3) * Ticket 计算
            // --- 规则 1: 纵向结算 ---
            for (int col = 0; col < 3; col++)
            {
                if (history[col][0].runtimeId == history[col][1].runtimeId && 
                    history[col][1].runtimeId == history[col][2].runtimeId)
                {
                    // 此时该列三个图案相同，获取该图案的押注数
                    int currentBet = GetBet(history[col][0]);
                    totalTicket += currentBet * history[col][0].runtimeRewardTicket.Value;
                }
            }

            // --- 规则 2: 横向结算 ---
            for (int row = 0; row < 3; row++)
            {
                if (history[0][row].runtimeId == history[1][row].runtimeId && 
                    history[1][row].runtimeId == history[2][row].runtimeId)
                {
                    int currentBet = GetBet(history[0][row]);
                    // 奖励 = (基础3倍 + 押注数) * Ticket
                    totalTicket += (3 + currentBet) * history[0][row].runtimeRewardTicket.Value;
                }
            }

            // --- 规则 3: 斜向结算 ---
            // 左上到右下
            if (history[0][0].runtimeId == history[1][1].runtimeId && history[1][1].runtimeId == history[2][2].runtimeId)
                totalTicket += (3 + GetBet(history[0][0])) * history[0][0].runtimeRewardTicket.Value;
    
            // 左下到右上
            if (history[0][2].runtimeId == history[1][1].runtimeId && history[1][1].runtimeId == history[2][0].runtimeId)
                totalTicket += (3 + GetBet(history[0][2])) * history[0][2].runtimeRewardTicket.Value;

            // --- 规则 4: 9个全一样 (全盘相加 * 9) ---
            bool allSame = true;
            int firstId = history[0][0].runtimeId;
            foreach(var colData in history)
                foreach(var data in colData)
                    if(data.runtimeId != firstId) allSame = false;

            if (allSame)
            {
                totalTicket += (history[0][0].runtimeRewardTicket.Value * 9) * 9;
            }

            // 更新全局数值并通知 UI
            Global.lotteryTicket.Value += totalTicket;
            this.SendEvent(new ShowSettlementEffectEvent
            {
                TotalScore = totalTicket,
                HistoryData = new List<RewardData[]>(history)
            });

        }
    }

    public struct UpdatePachinkoUIEvent 
    { 
        public RewardData[] RowData; // 改名为 RowData 语义更清晰
        public int RowIndex;         // 0, 1, 2 分别代表第 1, 2, 3 行
    }

    public struct ShowSettlementEffectEvent
    {
        public int TotalScore;
        public List<RewardData[]> HistoryData;
    }

}