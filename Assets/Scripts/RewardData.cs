using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gambling;
using Unity.Collections;
using Gambling;
using QFramework;

[CreateAssetMenu(fileName = "NewReward", menuName = "Gambling/RewardItem")]
public class RewardData : ScriptableObject
{ 
    public string RewardName;     // 奖品名称
    public Sprite RewardIcon;     // 奖品图标
    public int GoldValue;         // 奖励金额
    public float ChanceWeight;    // 中奖权重（用于概率控制）
    public CardItem.CardType CardType;
    public bool isMini;
    
    [HideInInspector] public BindableProperty<string> runtimeRewardName=new BindableProperty<string>("");
    [HideInInspector] public Sprite runtimeRewardIcon;
    [HideInInspector] public BindableProperty<int> runtimeGoldValue=new BindableProperty<int>(0);
    [HideInInspector] public BindableProperty<float> runtimeChanceWeight=new BindableProperty<float>(0);
    [HideInInspector] public CardItem.CardType runtimeCardType;
    [HideInInspector] public bool runTimeisMini;

    public void Reset()
    {
        runtimeRewardName.Value = RewardName;
        runtimeRewardIcon = RewardIcon;
        runtimeGoldValue.Value = GoldValue;
        runtimeChanceWeight.Value = ChanceWeight;
        runtimeCardType = CardType;
        runTimeisMini=isMini;
        Debug.Log(runtimeRewardName+" "+runtimeChanceWeight);

    }
}
