using System;
using System.Collections.Generic;
using System.Linq;
using Gambling;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.EventSystems;
using System.Linq;

namespace Gambling
{
	public class UIGamePanelData : UIPanelData
	{
	}
	public partial class UIGamePanel : UIPanel,IController
	{
		private IUnRegister mInputRegister;
		
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGamePanelData ?? new UIGamePanelData();
			AwardBackGround.Hide();
			NextLevelBtn.GetComponent<Button>().interactable = false;
			CardProbabilityPanel.Hide();
			// please add init code here
			GamblingGround.Score.RegisterWithInitValue(score =>
			{
				GamblingGround.ScoreText.text = "Score:" + score;
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<BuyRelicSuccessEvent>(OnBuyRelicSuccess)
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			GamblingGround.Score.Register(score =>
			{
				if (score >= Global.levelScore.Value)
				{
					if (!GamblingGround.isEasterEggExecuting)
					{
						NextLevelBtn.GetComponent<Button>().interactable = true;
					}
					else
					{
						
					}
				}


			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			GamblingGround.currentDoubleNum.RegisterWithInitValue(doubleNum =>
			{
				DoubleNumText.text = (doubleNum*GamblingGround.globalDoubleNum.Value).ToString();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			GamblingGround.globalDoubleNum.RegisterWithInitValue(globalDoubleNum =>
			{
				DoubleNumText.text = (globalDoubleNum*GamblingGround.currentDoubleNum.Value).ToString();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			GamblingGround.currentDoubleNum.Register(doubleNum =>
			{
				
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			//NextLevelBtn.OnPointerClickEvent(NextLevelEvent);
			Global.chips.RegisterWithInitValue(chips =>
			{
				Chips.text = "筹码:" + chips;
				
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			Global.chips.Register(chips =>
			{
				if (Global.chips.Value <= 0)
				{
					Button[] allButtons = Panel.GetComponentsInChildren<Button>();
					foreach (Button button in allButtons)
					{
						button.interactable = false;
					}
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			AwardBtn.onClick.AddListener(OpenAwardPanel);
			NextLevelBtn.onClick.AddListener(NextLevelEvent);
			Global.level.Register(level =>
			{
				if (level <= 5)
				{ 
					Global.levelScore.Value += 30*(level-1);
				}
				else
				{
					Global.levelScore.Value += 50*level-1;
				}
				Global.greedlevelScore.Value += 20*(level-1)*level;
				Global.chips.Value = this.GamblingGround.chipsGlobalAddNum;
				GamblingGround.EnableAllButtons();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			DoubleBetBtn.onClick.AddListener(DoubleBetEvent);
			Global.levelScore.RegisterWithInitValue(levelScore =>
			{
				LevelScore.text = "死线" + Global.level.Value  +":"+ levelScore + "分";
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			Global.greedlevelScore.RegisterWithInitValue(greedlevelScore =>
			{
				GreedLevelScore.text = "贪婪死线" + Global.level.Value +":"+ greedlevelScore + "分";
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			Global.lotteryTicket.RegisterWithInitValue(lotteryticket =>
			{
				TicketText.text = "旋转票数:" + lotteryticket;
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			Global.lotteryTicket.Register(lotteryticket =>
			{
				if (lotteryticket <= 2)
				{
					DoubleBetBtn.interactable=false;
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			Global.ticketReward.RegisterWithInitValue(ticketReward =>
			{
				LotteyTicket.text = "通关奖励票数:" + ticketReward;
				AwardTicket.text = "转动卷:+" + ticketReward;
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			GamblingGround.totalWeight.RegisterWithInitValue(totalweight =>
			{
				GamblingGround.CalculateCardWeights();
				UpdateCardProbabilityUI();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			Global.currentLevelSpinCount.RegisterWithInitValue(levelspincount =>
			{
				if (levelspincount == 0)
				{
					Global.ticketReward.Value = 12;
				}
				else if (levelspincount == 1)
				{
					Global.ticketReward.Value = 10;
				}
				else if (levelspincount == 2)
				{
					Global.ticketReward.Value = 8;
				}
				else
				{
					Global.ticketReward.Value = 6;
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			GamblingGround.totalColorWeight.RegisterWithInitValue(totalcolorweight =>
			{
				UpdateColorProbabilityUI();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			ScoreProbaility.OnPointerClickEvent(ProbailityUIStartEvent);
			ColorProbaility.OnPointerClickEvent(ProbailityUIStartEvent);
			mInputRegister = ActionKit.OnUpdate.Register(() =>
			{
				if (Input.GetMouseButtonDown(0))
				{
					CheckAndClosePanel();
				}
			});


		}

		private void OpenAwardPanel()
		{
			AwardBackGround.Hide();
			if (GamblingGround.Score.Value >= Global.greedlevelScore.Value)
			{
				UIKit.OpenPanel<UIAward>();
			}
			else
			{
				UIKit.OpenPanel<UIShopPanel>();
			}
		}

		private void DoubleBetEvent()
		{
			if (Global.lotteryTicket.Value > 2)
			{
				Global.chips.Value += 5+this.GamblingGround.chipsAddNum;
				Global.lotteryTicket.Value -= 2;
				if (Global.lotteryTicket.Value <= 2)
				{ 
					DoubleBetBtn.interactable = false;
				}
			}

		}

		private void CheckAndClosePanel()
		{
			// 检查CardProbabilityPanel是否激活
			if (CardProbabilityPanel != null)
			{
				// 检查点击是否在Panel外部
				if (!IsPointerOverUIElement(CardProbabilityPanel))
				{
					CardProbabilityPanel.Hide();
				}

				if (!IsPointerOverUIElement(ColorProbabilityPanel))
				{
					ColorProbabilityPanel.Hide();
				}
			}
		}
		private bool IsPointerOverUIElement(Image panel)
		{
			// 创建PointerEventData
			PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
			eventDataCurrentPosition.position = Input.mousePosition;

			// 执行射线检测
			List<RaycastResult> results = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

			// 检查射线检测结果中是否包含目标Panel
			foreach (RaycastResult result in results)
			{
				if (result.gameObject.transform.IsChildOf(panel.transform) || result.gameObject == panel)
				{
					return true;
				}
			}

			return false;
		}



		private void ProbailityUIStartEvent(PointerEventData obj)
		{
			if (obj.pointerPress.name == "ScoreProbaility")
			{
				CardProbabilityPanel.Show();
			}

			if (obj.pointerPress.name == "ColorProbaility")
			{
				ColorProbabilityPanel.Show();
			}
		}

		private void NextLevelEvent()
		{
			//NextLevelBtn.GetComponent<Button>().interactable = false;
			GamblingGround.ResetGambling();
			Debug.Log("ticketReward: " + Global.ticketReward.Value);
			Global.lotteryTicket.Value += Global.ticketReward.Value;
			Time.timeScale = 0;
			AwardBackGround.Show();
			//UIKit.OpenPanel<UIAward>();
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

		private void UpdateCardProbabilityUI()
		{
			if (CardProbabilityPanel == null) return;
			// 遍历 CardProbabilityPanel 下的所有子物体
			for (int i = 0; i < CardProbabilityPanel.transform.childCount; i++)
			{
				Transform childTransform = CardProbabilityPanel.transform.GetChild(i);
				string childName = childTransform.name;

				// 检查 cardNameWeights 字典中是否包含该子物体名称
				if  (GamblingGround.cardNameWeights.TryGetValue(childName, out float probability))
				{
					// 使用 TryGetValue 确保分数字典中也存在该键
					if (GamblingGround.cardNameScores.TryGetValue(childName, out string scoreUIText))
					{
						Text probabilityText = childTransform.GetChild(0).GetComponent<Text>();
						Text scoreText = childTransform.GetChild(1).GetComponent<Text>();
        
						if (probabilityText != null)
							probabilityText.text = probability.ToString("F2") + "%";
            
						if (scoreText != null)
							scoreText.text = scoreUIText;
					}
					else 
					{
						// 如果权重有但分数没有，可以设置默认值或隐藏
						childTransform.GetChild(1).GetComponent<Text>().text = "0"; 
					}
				}
			}
		}

		private void UpdateColorProbabilityUI()
		{
			if (ColorProbabilityPanel == null) return;
			for (int i = 0; i < ColorProbabilityPanel.transform.childCount; i++)
			{
				Transform childTransform = ColorProbabilityPanel.transform.GetChild(i);
				string childName = childTransform.name;
				var target = GamblingGround.colorConfigs.FirstOrDefault(item => item.colorName == childName);
				if (target.colorName != null)
				{
					Color color = target.color;
					childTransform.GetComponent<Image>().color = color;
					float probability = target.weight;
					//Debug.Log("颜色权重"+probability);
					Text probabilityText = childTransform.GetChild(0).GetComponent<Text>();
					probabilityText.text = (probability/GamblingGround.totalColorWeight.Value*100).ToString("F2") + "%";
					int score = target.score;
					Text scoreText = childTransform.GetChild(1).GetComponent<Text>();
					scoreText.text = score.ToString();
				}
				
			}
		}
		private void OnBuyRelicSuccess(BuyRelicSuccessEvent evt)
		{
			// 加载 RelicImage 预制体
			GameObject relicImagePrefab = Resources.Load<GameObject>("Prefab/UI/RelicImage");
			relicImagePrefab.GetComponent<RelicImage>().relicDesc = evt.RelicData.RelicDesc;
			if (relicImagePrefab != null && RelicList != null)
			{
				// 在 RelicList 下实例化预制体
				GameObject relicImageObj = Instantiate(relicImagePrefab, RelicList);
        
				// 获取 Image 组件并设置图标
				Image relicImage = relicImageObj.GetComponent<Image>();
				if (relicImage != null)
				{
					relicImage.sprite = evt.RelicIcon;
				}
        
				Debug.Log($"✅ 已在 RelicList 中添加遗物图标: {evt.RelicData.RelicName}");
			}
			else
			{
				Debug.LogError("❌ 无法加载 RelicImage 预制体或 RelicList 为空");
			}
		}

		public IArchitecture GetArchitecture() => Global.Interface;

	}
}
