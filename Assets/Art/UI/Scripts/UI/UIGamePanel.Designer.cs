using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace QFramework.Example
{
	// Generate Id:1080ff33-a9ae-4197-bc92-8b0791707aa6
	public partial class UIGamePanel
	{
		public const string Name = "UIGamePanel";
		
		[SerializeField]
		public UnityEngine.UI.Image Panel;
		[SerializeField]
		public UnityEngine.UI.Button King;
		[SerializeField]
		public UnityEngine.UI.Text KingText;
		[SerializeField]
		public UnityEngine.UI.Text KingNum;
		[SerializeField]
		public UnityEngine.UI.Button SmallKing;
		[SerializeField]
		public UnityEngine.UI.Text SmallKingText;
		[SerializeField]
		public UnityEngine.UI.Text SmallKingNum;
		[SerializeField]
		public UnityEngine.UI.Button SevenSeven;
		[SerializeField]
		public UnityEngine.UI.Text SevenSevenText;
		[SerializeField]
		public UnityEngine.UI.Text SevenSevenNum;
		[SerializeField]
		public UnityEngine.UI.Button DoubleStar;
		[SerializeField]
		public UnityEngine.UI.Text DoubleStarText;
		[SerializeField]
		public UnityEngine.UI.Text DoubleStarNum;
		[SerializeField]
		public UnityEngine.UI.Button Watermelon;
		[SerializeField]
		public UnityEngine.UI.Text WatermelonText;
		[SerializeField]
		public UnityEngine.UI.Text WatermelonNum;
		[SerializeField]
		public UnityEngine.UI.Button Bell;
		[SerializeField]
		public UnityEngine.UI.Text BellText;
		[SerializeField]
		public UnityEngine.UI.Text BellNum;
		[SerializeField]
		public UnityEngine.UI.Button Blueberry;
		[SerializeField]
		public UnityEngine.UI.Text BlueberryText;
		[SerializeField]
		public UnityEngine.UI.Text BlueberryNum;
		[SerializeField]
		public UnityEngine.UI.Button Orange;
		[SerializeField]
		public UnityEngine.UI.Text OrangeText;
		[SerializeField]
		public UnityEngine.UI.Text OrangeNum;
		[SerializeField]
		public UnityEngine.UI.Button Apple;
		[SerializeField]
		public UnityEngine.UI.Text AppleText;
		[SerializeField]
		public UnityEngine.UI.Text AppleNum;
		[SerializeField]
		public Gambling.GamblingGround GamblingGround;
		[SerializeField]
		public UnityEngine.UI.Image ScoreBG;
		[SerializeField]
		public UnityEngine.UI.Text ScoreText;
		[SerializeField]
		public UnityEngine.UI.Button StartButton;
		[SerializeField]
		public UnityEngine.UI.Text TicketText;
		[SerializeField]
		public UnityEngine.UI.Text LevelScore;
		[SerializeField]
		public UnityEngine.UI.Text Chips;
		[SerializeField]
		public UnityEngine.UI.Button NextLevelBtn;
		[SerializeField]
		public UnityEngine.UI.Text NextLevelBtnText;
		[SerializeField]
		public UnityEngine.UI.Button ScoreProbaility;
		[SerializeField]
		public UnityEngine.UI.Text ScoreProbailityText;
		[SerializeField]
		public UnityEngine.UI.Image CardProbabilityPanel;
		
		private UIGamePanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Panel = null;
			King = null;
			KingText = null;
			KingNum = null;
			SmallKing = null;
			SmallKingText = null;
			SmallKingNum = null;
			SevenSeven = null;
			SevenSevenText = null;
			SevenSevenNum = null;
			DoubleStar = null;
			DoubleStarText = null;
			DoubleStarNum = null;
			Watermelon = null;
			WatermelonText = null;
			WatermelonNum = null;
			Bell = null;
			BellText = null;
			BellNum = null;
			Blueberry = null;
			BlueberryText = null;
			BlueberryNum = null;
			Orange = null;
			OrangeText = null;
			OrangeNum = null;
			Apple = null;
			AppleText = null;
			AppleNum = null;
			GamblingGround = null;
			ScoreBG = null;
			ScoreText = null;
			StartButton = null;
			TicketText = null;
			LevelScore = null;
			Chips = null;
			NextLevelBtn = null;
			NextLevelBtnText = null;
			ScoreProbaility = null;
			ScoreProbailityText = null;
			CardProbabilityPanel = null;
			
			mData = null;
		}
		
		public UIGamePanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UIGamePanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UIGamePanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
