using System;
using System.Collections.Generic;
using System.Linq;
using Gambling;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.EventSystems;
using System.Linq;

namespace QFramework.Example
{
	public class UIGamePanelData : UIPanelData
	{
	}
	public partial class UIGamePanel : UIPanel
	{
		private IUnRegister mInputRegister;
		
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGamePanelData ?? new UIGamePanelData();
			NextLevelBtn.GetComponent<Button>().interactable = false;
			CardProbabilityPanel.Hide();
			// please add init code here
			GamblingGround.Score.RegisterWithInitValue(score =>
			{
				GamblingGround.ScoreText.text = "Score:" + score;
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			
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
				DoubleNumText.text = doubleNum.ToString();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			GamblingGround.currentDoubleNum.Register(doubleNum =>
			{
				if (Global.lotteryTicket.Value > 2)
				{
					Global.lotteryTicket.Value -= 1;
				}
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
			NextLevelBtn.onClick.AddListener(NextLevelEvent);
			Global.level.Register(level =>
			{
				Global.levelScore.Value += 10;
				GamblingGround.EnableAllButtons();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			DoubleBetBtn.onClick.AddListener(DoubleBetEvent);
			Global.levelScore.RegisterWithInitValue(levelScore =>
			{
				LevelScore.text = "第" + Global.level.Value + "关:" + levelScore + "分";
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
			GamblingGround.totalWeight.RegisterWithInitValue(totalweight =>
			{
				GamblingGround.CalculateCardWeights();
				UpdateCardProbabilityUI();
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

		private void DoubleBetEvent()
		{
			if (Global.lotteryTicket.Value > 2)
			{
				GamblingGround.currentDoubleNum.Value*=2;
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
			Time.timeScale = 0;
			UIKit.OpenPanel<UIShopPanel>();
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
				if (GamblingGround.cardNameWeights.ContainsKey(childName))
				{
					// 获取概率值
					float probability = GamblingGround.cardNameWeights[childName];
					string scoreUIText=GamblingGround.cardNameScores[childName];
					// 查找该子物体下的 Text 组件（通常在子物体中）
					Text probabilityText = childTransform.GetChild(0).GetComponent<Text>();
					Text scoreText=childTransform.GetChild(1).GetComponent<Text>();
					if (probabilityText != null)
					{
						// 显示概率，保留2位小数并添加百分号
						probabilityText.text = probability.ToString("F2") + "%";
					}
					else
					{
						Debug.LogWarning($"未找到 {childName} 子物体中的 Text 组件");
					}

					if (scoreText != null)
					{
						scoreText.text = scoreUIText;
					}
					else
					{
						Debug.LogWarning($"未找到 {childName} 子物体中的 ScoreText 组件");
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
					Debug.Log("颜色权重"+probability);
					Text probabilityText = childTransform.GetChild(0).GetComponent<Text>();
					probabilityText.text = (probability/GamblingGround.totalColorWeight.Value*100).ToString("F2") + "%";
					int score = target.score;
					Text scoreText = childTransform.GetChild(1).GetComponent<Text>();
					scoreText.text = score.ToString();
				}
				
			}
		}
	}
}
