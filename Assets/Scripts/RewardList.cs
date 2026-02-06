using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRewardList", menuName = "Gambling/RewardList")]
public class RewardList : ScriptableObject
{
    public List<RewardData> Rewards; // 所有的奖品池

    public void ResetAll()
    {
        if (Rewards == null)
        {
            return;
        }

        foreach (var reward in Rewards)
        {
            if (reward != null)
            {
                reward.Reset();
            }
        }
    }
}

