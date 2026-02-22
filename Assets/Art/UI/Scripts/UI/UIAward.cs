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
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIAwardData ?? new UIAwardData();
			// please add init code here
			AwardBtn.onClick.AddListener(OpenShopPanel);
			
		}

		private void OpenShopPanel()
		{
			var gamblingGround = FindObjectOfType<GamblingGround>();
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
