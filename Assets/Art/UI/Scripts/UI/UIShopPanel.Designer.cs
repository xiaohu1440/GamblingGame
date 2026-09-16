using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace Gambling
{
	// Generate Id:e53475f7-d1fb-4451-a0ba-544bc8b79e73
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
