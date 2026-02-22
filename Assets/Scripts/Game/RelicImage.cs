using System;
using UnityEngine;
using QFramework;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gambling
{
	public partial class RelicImage : ViewController,IController,IPointerEnterHandler,IPointerExitHandler
	{
		public IArchitecture GetArchitecture() => Global.Interface;
		public string relicDesc;
		[SerializeField] private GameObject descriptionPanelPrefab; // DescriptionPanel预制体
		private GameObject currentDescriptionPanel; // 当前显示的描述面板实例

		private void Awake()
		{
			descriptionPanelPrefab = Resources.Load<GameObject>("Prefab/UI/RelicShowPanel");

		}
		

        public void OnPointerEnter(PointerEventData eventData)
        {
	        if (relicDesc == null || descriptionPanelPrefab == null) return;
    
	        // 使用QFramework的UIRoot获取Canvas
	        Canvas canvas = UIRoot.Instance.Canvas;
	        if (canvas == null) return;

	        // 在Canvas根节点下生成DescriptionPanel
	        currentDescriptionPanel = Instantiate(descriptionPanelPrefab, canvas.transform);

	        // 设置面板位置：以鼠标位置作为面板的左上角，然后添加偏移
	        RectTransform rectTransform = currentDescriptionPanel.GetComponent<RectTransform>();
    
	        if (rectTransform != null)
	        {
		        // 设置面板的轴心点为左上角
		        rectTransform.pivot = new Vector2(0, 1);
        
		        // 将鼠标屏幕坐标转换为Canvas的本地坐标
		        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
		        Vector2 localPoint;
		        RectTransformUtility.ScreenPointToLocalPointInRectangle(
			        canvasRect,
			        Input.mousePosition,
			        canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
			        out localPoint
		        );
        
		        // 添加偏移：向右+200，向下-100
		        //localPoint += new Vector2(200, -100);
        
		        rectTransform.anchoredPosition = localPoint;
	        }
            Transform relicDescText = currentDescriptionPanel.transform.Find("RelicDescription");
            if (relicDescText != null)
            {
                Text descText = relicDescText.GetComponent<Text>();
                if (descText != null)
                    descText.text = "遗物效果:"+relicDesc;
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
