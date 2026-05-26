using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace Gambling
{
	public class UIShopPanelData : UIPanelData
	{
	}
	public partial class UIShopPanel : UIPanel,IController
	{
		public IArchitecture GetArchitecture() => Global.Interface;
		private int refreshlotteryTicket = 4;
		[SerializeField] private GameObject relicItemPrefab;
		private List<GameObject> spawnedRelicItems = new List<GameObject>();
		private IRelicSystem relicSystem;
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIShopPanelData ?? new UIShopPanelData();
			// please add init code here
			RefreshBtn.interactable = true;
			refreshlotteryTicket = 4;
			RefreshPrice.text="刷新:"+refreshlotteryTicket.ToString();
			NextLevel.onClick.AddListener(NextLevelEvent);
			relicSystem=this.GetSystem<IRelicSystem>();
			var shopRelics = relicSystem.DrawRelicsForShop(Global.level.Value,4);
			// 生成RelicItem预制体
			if (shopRelics != null)
			{
				GenerateRelicItems(shopRelics);
			}
			Global.lotteryTicket.Register(ticket =>
			{
				if ((ticket-refreshlotteryTicket) < 2)
				{
					RefreshBtn.interactable = false;
				}
				
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			Global.lotteryTicket.RegisterWithInitValue(ticket =>
			{
				CoinText.text = "转动卷" + ticket+ "<color=red>(-2)</color>";
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			RefreshBtn.onClick.AddListener(RefreshEvent);
			
		}
		/// <summary>
		/// 根据shopRelics列表生成RelicItem预制体
		/// </summary>
		private void GenerateRelicItems(List<RelicData> shopRelics)
		{
			// 清空之前生成的物品
			ClearRelicItems();
            
			// 加载RelicItem预制体（如果没有在Inspector中赋值）
			if (relicItemPrefab == null)
			{
				relicItemPrefab = Resources.Load<GameObject>("Prefab/UI/RelicItem");
			}
            
			if (relicItemPrefab == null)
			{
				Debug.LogError("❌ RelicItem预制体未找到！");
				return;
			}
            
			// 遍历shopRelics列表，生成对应数量的预制体
			for (int i = 0; i < shopRelics.Count; i++)
			{
				// 实例化预制体
				GameObject relicItemObj = Instantiate(relicItemPrefab, Relic);
                
				// 获取UIRelicItem组件并赋值RelicData
				UIRelicItem relicItem = relicItemObj.GetComponent<UIRelicItem>();
				Image relicIcon = relicItemObj.transform.GetChild(0).GetComponent<Image>();
				Text relicPrice=relicItemObj.transform.GetChild(1).GetComponent<Text>();
				if (relicItem != null)
				{
					relicItem.SetRelicData(shopRelics[i]);
					relicIcon.sprite = shopRelics[i].RelicIcon;
					relicPrice.text = "价格:"+shopRelics[i].RelicPrice.ToString();
				}
				else
				{
					Debug.LogWarning("⚠️ RelicItem预制体上没有UIRelicItem组件！");
				}
                
				// 保存引用以便后续清理
				spawnedRelicItems.Add(relicItemObj);
			}
            
			Debug.Log($"✅ 成功生成 {shopRelics.Count} 个RelicItem");
		}

		private void RefreshShopRelics()
		{
			ClearRelicItems();
			var shopRelics = relicSystem.DrawRelicsForShop(Global.level.Value, 4);
			if (shopRelics != null)
			{
				GenerateRelicItems(shopRelics);
			}
		}
        
		/// <summary>
		/// 清空已生成的RelicItem
		/// </summary>
		private void ClearRelicItems()
		{
			foreach (var item in spawnedRelicItems)
			{
				if (item != null)
				{
					Destroy(item);
				}
			}
			spawnedRelicItems.Clear();
		}
		private void NextLevelEvent()
		{
			Global.level.Value++;
			Global.currentLevelSpinCount.Value = 9;
			Time.timeScale = 1;
			this.CloseSelf();
		}

		private void RefreshEvent()
		{
			Global.lotteryTicket.Value -= refreshlotteryTicket;
			RefreshShopRelics();
			refreshlotteryTicket *= 2;
			RefreshPrice.text="刷新:"+refreshlotteryTicket.ToString();
		}

		protected override void OnOpen(IUIData uiData = null)
		{
		}
		
		protected override void OnShow()
		{
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
		}
	}
}
