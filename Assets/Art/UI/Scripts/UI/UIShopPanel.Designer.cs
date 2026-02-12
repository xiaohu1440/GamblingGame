using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace Gambling
{
	// Generate Id:42634995-ca1e-469e-9837-5e107e99be0e
	public partial class UIShopPanel
	{
		public const string Name = "UIShopPanel";
		
		[SerializeField]
		public RectTransform Relic;
		[SerializeField]
		public UnityEngine.UI.Button NextLevel;
		
		private UIShopPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Relic = null;
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
