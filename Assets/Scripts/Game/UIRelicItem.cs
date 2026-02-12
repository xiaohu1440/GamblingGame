using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace Gambling
{
    public class UIRelicItem : MonoBehaviour,IController
    {
        [SerializeField] private Image relicIcon;
        [SerializeField] private Text relicName;
        [SerializeField] private Text relicPrice;
        [SerializeField] private Text relicDesc;
        [SerializeField] private Button buyButton;
        
        private RelicData relicData;
        public IArchitecture GetArchitecture() => Global.Interface;
        private void Awake()
        {
            buyButton=GetComponent<Button>();
        }

        public void SetRelicData(RelicData data)
        {
            relicData = data;
            
            // 更新UI显示
            if (relicIcon != null)
                relicIcon.sprite = data.RelicIcon;
            
            if (relicName != null)
                relicName.text = data.RelicName;
            
            if (relicPrice != null)
                relicPrice.text = data.RelicPrice.ToString();
            
            if (relicDesc != null)
                relicDesc.text = data.RelicDesc;
            
            // 绑定购买按钮事件
            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(OnBuyClick);
            }
        }
        
        private void OnBuyClick()
        {
            // 这里可以添加购买逻辑
            var relicSystem=this.GetSystem<IRelicSystem>();
            relicSystem.BuyRelic(relicData);
            Debug.Log($"购买遗物: {relicData.RelicName}");
        }


    }
}