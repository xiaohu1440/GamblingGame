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
		[SerializeField] private GameObject relicItemPrefab;
		private List<GameObject> spawnedRelicItems = new List<GameObject>();
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIShopPanelData ?? new UIShopPanelData();
			// please add init code here
			NextLevel.onClick.AddListener(NextLevelEvent);
			var relicSystem=this.GetSystem<IRelicSystem>();
			var shopRelics = relicSystem.DrawRelicsForShop(Global.level.Value,4);
			// 生成RelicItem预制体
			GenerateRelicItems(shopRelics);
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
				if (relicItem != null)
				{
					relicItem.SetRelicData(shopRelics[i]);
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
			int ticketReward = 6; // 默认3次及以上给6枚
			if (Global.currentLevelSpinCount.Value == 0)
			{
				ticketReward = 12;
			}
			else if (Global.currentLevelSpinCount.Value == 1)
			{
				ticketReward = 10;
			}
			else if (Global.currentLevelSpinCount.Value == 2)
			{
				ticketReward = 8;
			}
			Debug.Log("ticketReward: " + ticketReward);
			Global.level.Value++;
			Global.lotteryTicket.Value += ticketReward;
			Global.currentLevelSpinCount.Value = 0;
			Time.timeScale = 1;
			this.CloseSelf();
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
