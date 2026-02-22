using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace Gambling
{
	// Generate Id:abc6f0c1-bb3d-44fe-a4af-6cd5cd3226df
	public partial class UIAward
	{
		public const string Name = "UIAward";
		
		[SerializeField]
		public UnityEngine.UI.Button AwardBtn;
		[SerializeField]
		public UnityEngine.UI.Text AwardDes;
		
		private UIAwardData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			AwardBtn = null;
			AwardDes = null;
			
			mData = null;
		}
		
		public UIAwardData Data
		{
			get
			{
				return mData;
			}
		}
		
		UIAwardData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UIAwardData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
