using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.EventSystems;

namespace Gambling
{
    public class UIRelicItem : MonoBehaviour,IController,IPointerEnterHandler,IPointerExitHandler
    {
        [SerializeField] private Image relicIcon;
        [SerializeField] private Text relicName;
        [SerializeField] private Text relicPrice;
        [SerializeField] private Text relicDesc;
        [SerializeField] private Button buyButton;
        [SerializeField] private RelicQuality relicQuality;
        [SerializeField] private GameObject descriptionPanelPrefab; // DescriptionPanel预制体
        private GameObject currentDescriptionPanel; // 当前显示的描述面板实例
        
        private RelicData relicData;
        public IArchitecture GetArchitecture() => Global.Interface;
        private void Awake()
        {
            buyButton=GetComponent<Button>();
            descriptionPanelPrefab=Resources.Load<GameObject>("Prefab/UI/DescriptionPanel");
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
            
            if (relicQuality != null)
                relicQuality = data.RelicQuality;
            
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
            gameObject.DestroySelf();
            Debug.Log($"购买遗物: {relicData.RelicName}");
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            if (relicData == null || descriptionPanelPrefab == null) return;
            // 获取Canvas根节点
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
    
            // 在Canvas根节点下生成DescriptionPanel
            currentDescriptionPanel = Instantiate(descriptionPanelPrefab, canvas.transform);
    
            // 设置面板位置为鼠标位置
            RectTransform rectTransform = currentDescriptionPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.position = Input.mousePosition;
            }
    
            // 获取子物体的Text组件并设置值
            Transform relicNameText = currentDescriptionPanel.transform.Find("RelicName");
            if (relicNameText != null)
            {
                Text nameText = relicNameText.GetComponent<Text>();
                if (nameText != null)
                    nameText.text = "遗物名称:"+relicData.RelicName;
            }
    
            Transform relicPriceText = currentDescriptionPanel.transform.Find("RelicPrice");
            if (relicPriceText != null)
            {
                Text priceText = relicPriceText.GetComponent<Text>();
                if (priceText != null)
                    priceText.text = "遗物价格"+relicData.RelicPrice.ToString();
            }
    
            Transform relicQualityText = currentDescriptionPanel.transform.Find("RelicQuality");
            if (relicQualityText != null)
            {
                Text qualityText = relicQualityText.GetComponent<Text>();
                if (qualityText != null)
                    qualityText.text = "遗物质量:"+relicData.RelicQuality.ToString();
            }
    
            Transform relicDescText = currentDescriptionPanel.transform.Find("RelicDescription");
            if (relicDescText != null)
            {
                Text descText = relicDescText.GetComponent<Text>();
                if (descText != null)
                    descText.text = "遗物效果:"+relicData.RelicDesc;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // 鼠标移开时销毁描述面板
            if (currentDescriptionPanel != null)
            {
                Destroy(currentDescriptionPanel);
                currentDescriptionPanel = null;
            }
        }
        private void OnDestroy()
        {
            if (currentDescriptionPanel != null)
            {
                Destroy(currentDescriptionPanel);
            }
        }
    }
}