using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gambling
{
    /// <summary>
    /// Configuration for bet multipliers.
    /// </summary>
    [Serializable]
    public class BetMultiplierConfig
    {
        /// <summary>Represents the limit or boundary value for a specific operation or condition.</summary>
        [Tooltip("达到此筹码数量时触发倍率")]
        public int threshold;

        /// <summary>Represents the factor by which a value is multiplied.</summary>
        [Tooltip("对应的倍率")]
        public int multiplier;

        /// <summary>
        /// Configuration class for defining the settings related to the bet multiplier system.
        /// </summary>
        public BetMultiplierConfig(int threshold, int multiplier)
        {
            this.threshold = threshold;
            this.multiplier = multiplier;
        }
    }

    /// <summary>
    /// System for managing and applying bet multipliers in a gaming context.
    /// </summary>
    public class BetMultiplierSystem
    {
        // 倍率配置列表（按阈值从小到大排序）
        /// <summary>Represents the configuration settings for multipliers used in calculations or gameplay logic.</summary>
        private List<BetMultiplierConfig> multiplierConfigs;

        /// <summary>
        /// Represents a system for managing and calculating bet multipliers.
        /// </summary>
        public BetMultiplierSystem()
        {
            // 默认配置：>10筹码2倍，>20筹码4倍，>30筹码8倍
            multiplierConfigs = new List<BetMultiplierConfig>
            {
                new BetMultiplierConfig(10, 2),
                new BetMultiplierConfig(20, 4),
                new BetMultiplierConfig(30, 8)
            };
        }

        /// <summary>
        /// Represents a system for managing and calculating bet multipliers.
        /// </summary>
        public BetMultiplierSystem(List<BetMultiplierConfig> configs)
        {
            multiplierConfigs = configs ?? new List<BetMultiplierConfig>();
            // 按阈值排序
            multiplierConfigs.Sort((a, b) => a.threshold.CompareTo(b.threshold));
        }

        /// <summary>
        /// Retrieves the multiplier value based on the provided input.
        /// </summary>
        /// <param name="inputValue">The value used to determine the multiplier.</param>
        /// <returns>A decimal representing the calculated multiplier.</returns>
        public int GetMultiplier(int betCount)
        {
            int multiplier = 1; // 默认1倍

            // 从后往前遍历，找到最高的满足条件的倍率
            for (int i = multiplierConfigs.Count - 1; i >= 0; i--)
            {
                if (betCount >= multiplierConfigs[i].threshold)
                {
                    multiplier = multiplierConfigs[i].multiplier;
                    break;
                }
            }
            
            return multiplier;
        }

        /// <summary>
        /// Retrieves the progress information for a specific task or process.
        /// </summary>
        /// <param name="taskId">The unique identifier of the task or process.</param>
        /// <returns>
        /// A string representing the progress details, such as percentage completed or status.
        /// </returns>
        public BetProgressInfo GetProgressInfo(int betCount)
        {
            BetProgressInfo info = new BetProgressInfo
            {
                currentBetCount = betCount,
                currentMultiplier = GetMultiplier(betCount)
            };

            // 查找下一个倍率阈值
            for (int i = 0; i < multiplierConfigs.Count; i++)
            {
                if (betCount <= multiplierConfigs[i].threshold)
                {
                    info.nextThreshold = multiplierConfigs[i].threshold;
                    info.nextMultiplier = multiplierConfigs[i].multiplier;
                    info.hasNextLevel = true;
                    
                    // 计算进度百分比
                    int previousThreshold = i > 0 ? multiplierConfigs[i - 1].threshold : 0;
                    int range = info.nextThreshold - previousThreshold;
                    int progress = betCount - previousThreshold;
                    info.progressPercent = range > 0 ? Mathf.Clamp01((float)progress / range) : 0f;
                    
                    break;
                }
            }

            // 如果已经超过所有阈值
            if (!info.hasNextLevel && multiplierConfigs.Count > 0)
            {
                info.progressPercent = 1f;
                info.nextThreshold = multiplierConfigs[multiplierConfigs.Count - 1].threshold;
                info.nextMultiplier = multiplierConfigs[multiplierConfigs.Count - 1].multiplier;
            }

            return info;
        }

        /// <summary>
        /// Retrieves all configurations available in the system.
        /// </summary>
        /// <returns>A collection of all configurations.</returns>
        public List<BetMultiplierConfig> GetAllConfigs()
        {
            return new List<BetMultiplierConfig>(multiplierConfigs);
        }
    }

    /// <summary>
    /// Information related to the progress of a bet
    /// </summary>
    public class BetProgressInfo
    {
        /// <summary>Represents the number of bets currently placed.</summary>
        public int currentBetCount;

        /// <summary>当前倍数值</summary>
        public int currentMultiplier;

        /// <summary>Represents the next threshold value for triggering a specific condition or action.</summary>
        public int nextThreshold;

        /// <summary>Represents the factor by which the current value should be multiplied for the next operation.</summary>
        public int nextMultiplier;

        /// <summary>Indicates whether there is a subsequent level available</summary>
        public bool hasNextLevel;

        /// <summary>Represents the completion percentage of a task or process</summary>
        public float progressPercent;

        /// <summary>
        /// Generates and returns the textual representation of the current progress.
        /// </summary>
        /// <returns>
        /// A string representing the progress in a human-readable format.
        /// </returns>
        public string GetProgressText()
        {
            if (hasNextLevel)
            {
                return $"{currentBetCount}/{nextThreshold} (x{currentMultiplier} → x{nextMultiplier})";
            }
            else
            {
                return $"{currentBetCount} (x{currentMultiplier} MAX)";
            }
        }
    }
}