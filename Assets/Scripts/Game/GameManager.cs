using System;
using UnityEngine;
using QFramework;

namespace Gambling
{
	public partial class GameManager : ViewController,IController
	{
		private RewardList rewardList;
		private ResLoader mResLoader=null;
		public IArchitecture GetArchitecture() => Global.Interface;

		private void Awake()
		{
			ResKit.Init();
		}

		private void Start()
		{
			mResLoader = ResLoader.Allocate();
			rewardList= mResLoader.LoadSync<RewardList>("gamblinglevel_asset", "GamblingLevel");
			rewardList.ResetAll();
			var relicSystem=this.GetSystem<IRelicSystem>();
			relicSystem.ResetAllRelicEffect();
			
		}

		private void OnDestroy()
		{
			mResLoader.Recycle2Cache();
			mResLoader = null;
		}

		
	}
}
