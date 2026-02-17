using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace Gambling
{
	// Generate Id:bf2e73ce-595f-422b-9fde-6e3151b2e37b
	public partial class UIShopPanel
	{
		public const string Name = "UIShopPanel";
		
		[SerializeField]
		public RectTransform Relic;
		[SerializeField]
		public UnityEngine.UI.Button RefreshBtn;
		[SerializeField]
		public UnityEngine.UI.Text RefreshPrice;
		[SerializeField]
		public UnityEngine.UI.Text CoinText;
		[SerializeField]
		public UnityEngine.UI.Button NextLevel;
		
		private UIShopPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Relic = null;
			RefreshBtn = null;
			RefreshPrice = null;
			CoinText = null;
			NextLevel = null;
			
			mData = null;
		}
		
		public UIShopPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UIShopPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UIShopPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
