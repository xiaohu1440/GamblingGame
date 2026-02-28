using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace Gambling
{
	public class UIAwardData : UIPanelData
	{
	}
	public partial class UIAward : UIPanel,IController
	{
		public IArchitecture GetArchitecture() => Global.Interface;
		private GamblingGround gamblingGround;
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIAwardData ?? new UIAwardData();
			gamblingGround = FindObjectOfType<GamblingGround>();
			// please add init code here
			AwardBtn.onClick.AddListener(OpenShopPanel);
			AwardAppleBtn.onClick.AddListener(AppleOpenShopPanel);
			AwardOrangeBtn.onClick.AddListener(OrangeOpenShopPanel);
			
		}

		private void OrangeOpenShopPanel()
		{
			gamblingGround.totalWeight.Value = 0;
			foreach (var card in gamblingGround.rewardDataArray)
			{
				if (card.runtimeRewardName.Value == "橘子")
				{
					card.runtimeChanceWeight.Value *= 2.25f;
				}
			}

			foreach (var card in gamblingGround.cardItems)
			{
				gamblingGround.totalWeight.Value += card.rewardData.runtimeChanceWeight.Value;
			}
			this.CloseSelf();
			UIKit.OpenPanel<UIShopPanel>();
		}

		private void AppleOpenShopPanel()
		{
			gamblingGround.totalWeight.Value = 0;
			foreach (var card in gamblingGround.rewardDataArray)
			{
				if (card.runtimeRewardName.Value == "苹果")
				{
					card.runtimeChanceWeight.Value *= 2.25f;
				}
			}

			foreach (var card in gamblingGround.cardItems)
			{
				gamblingGround.totalWeight.Value += card.rewardData.runtimeChanceWeight.Value;
			}
			this.CloseSelf();
			UIKit.OpenPanel<UIShopPanel>();
		}

		private void OpenShopPanel()
		{
			gamblingGround.globalDoubleNum.Value++;
			this.CloseSelf();
			UIKit.OpenPanel<UIShopPanel>();
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
