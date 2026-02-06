using System;
using UnityEngine;
using QFramework;

namespace Gambling
{
	public partial class GameManager : ViewController
	{
		private RewardList rewardList;
		private ResLoader mResLoader=null;

		private void Awake()
		{
			ResKit.Init();
		}

		private void Start()
		{
			mResLoader = ResLoader.Allocate();
			rewardList= mResLoader.LoadSync<RewardList>("gamblinglevel_asset", "GamblingLevel");
			rewardList.ResetAll();
			
		}

		private void OnDestroy()
		{
			mResLoader.Recycle2Cache();
			mResLoader = null;
		}
	}
}
