using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace Gambling
{
	// Generate Id:2def2542-e081-4e4c-b8a0-25e3e932855d
	public partial class UIAward
	{
		public const string Name = "UIAward";
		
		[SerializeField]
		public UnityEngine.UI.Button AwardBtn;
		[SerializeField]
		public UnityEngine.UI.Text AwardDes;
		[SerializeField]
		public UnityEngine.UI.Button AwardOrangeBtn;
		[SerializeField]
		public UnityEngine.UI.Button AwardAppleBtn;
		
		private UIAwardData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			AwardBtn = null;
			AwardDes = null;
			AwardOrangeBtn = null;
			AwardAppleBtn = null;
			
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
