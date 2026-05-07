using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Gambling
{
    public  class CardItem : MonoBehaviour
    {
        public int index;//索引
        public RewardData rewardData;
        public enum CardType
        {
            水果,
            赌博,
            王,
            彩蛋,
            其它
        }
        public CardType cardType;
        public void Init(int index, RewardData data)
        {
            this.index = index;
            this.rewardData = data;
            cardType = data.runtimeCardType;
            // 更新 UI 表现
            UpdateUI();
        }

        public void UpdateUI()
        {
            //GetComponentInChildren<Text>().text = myData.RewardName;
            transform.GetChild(1).GetComponent<Image>().sprite = rewardData.runtimeRewardIcon;
        }

        

        public int OnPlayerLand()
        {
            return rewardData.runtimeGoldValue.Value;
        }

        public int OnPlayerChips()
        {
            return rewardData.runtimeRewardTicket.Value;
        }
        public Image GetCardBaseImage()
        {
            if (transform.childCount > 0)
            {
                return transform.GetChild(0).GetComponent<Image>();
            }
            return null;
        }

        public Image GetBorderImage()
        {
            if (transform.childCount > 0)
            {
                return transform.GetChild(2).GetComponent<Image>();
            }
            return null;
        }
        
    }

}
