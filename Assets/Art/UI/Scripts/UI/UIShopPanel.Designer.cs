using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace Gambling
{
	// Generate Id:9c1fe9a5-a39a-4027-ba41-6aa4dec3af4e
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
		public UnityEngine.UI.Button NextLevel;
		
		private UIShopPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Relic = null;
			RefreshBtn = null;
			RefreshPrice = null;
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
