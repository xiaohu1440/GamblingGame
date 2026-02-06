using System;
using UnityEngine;
using QFramework;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using QFramework.Example;
using Random = UnityEngine.Random;


namespace Gambling
{
	public partial class GamblingGround : ViewController
	{
		[Header("配置模块")]
		[SerializeField]private int SideCount = 7;
		[SerializeField]private float TotalWidth = 800f;
		[SerializeField]private GameObject GridItemPrefab;
		[SerializeField]private Transform GridContainer;
		[Header("转盘动画配置")]
		[SerializeField]private float moveSpeed = 0.05f; // 移动速度（每个格子的移动时间）
		[SerializeField]private int minRotations = 3;
		[SerializeField]private int maxRotations = 8;
		[SerializeField] private RewardList currentPool;
		public Dictionary<string, float> cardNameWeights = new Dictionary<string, float>();
		public Dictionary<string, string> cardNameScores = new Dictionary<string, string>();
		public BindableProperty<int> Score = new BindableProperty<int>(0);
		private Dictionary<string,BindableProperty<int>> buttonClickCount=new Dictionary<string,BindableProperty<int>>();
		public BindableProperty<float> totalWeight = new BindableProperty<float>(0);
		private List<RectTransform> gridRects = new List<RectTransform>();
		private List<CardItem> cardItems = new List<CardItem>();
		private RectTransform selectBoxRect;
		private bool isSpinning = false;
		private int currentIndex = 0; // 当前SelectBox所在的索引

		void Start()
		{
			Init();
			GenerateGrid();
			SelectBox.Show();
			//ScoreBG.Show();
			selectBoxRect = SelectBox.GetComponent<RectTransform>();
			SelectBox.GetComponent<RectTransform>().sizeDelta = new Vector2((TotalWidth-80)/SideCount, (TotalWidth-80)/SideCount);
			if (gridRects.Count > 0)
			{
				selectBoxRect.anchoredPosition = gridRects[0].anchoredPosition;
				currentIndex = 0;
			}

			StartButton.OnPointerClickEvent(OnStartButtonClick);
			// 初始化按钮计数
			InitializeButtonCounts();
    
			// 绑定按钮点击事件
			BindButtonEvents();
			TextUpdate();


		}

		private void Init()
		{
			Score.Value = 0;
			Global.chips.Value = 5;
			Global.lotteryTicket.Value = 10;
			Global.levelScore.Value = 10;
			Global.level.Value = 1;
		}

		private void TextUpdate()
		{
			buttonClickCount["apple"].RegisterWithInitValue(applenum =>
			{
				Apple.transform.GetChild(1).GetComponent<Text>().text = applenum.ToString();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			buttonClickCount["king"].RegisterWithInitValue(kingnum =>
			{
				King.transform.GetChild(1).GetComponent<Text>().text = kingnum.ToString();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			buttonClickCount["smallking"].RegisterWithInitValue(smallkingnum =>
			{
				SmallKing.transform.GetChild(1).GetComponent<Text>().text = smallkingnum.ToString();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			buttonClickCount["doublestar"].RegisterWithInitValue(doublestarnum =>
			{
				DoubleStar.transform.GetChild(1).GetComponent<Text>().text = doublestarnum.ToString();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			buttonClickCount["sevenseven"].RegisterWithInitValue(sevennum =>
			{
				SevenSeven.transform.GetChild(1).GetComponent<Text>().text = sevennum.ToString();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			buttonClickCount["watermelon"].RegisterWithInitValue(watermelonnum =>
			{
				Watermelon.transform.GetChild(1).GetComponent<Text>().text = watermelonnum.ToString();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			buttonClickCount["orange"].RegisterWithInitValue(orangenum =>
			{
				Orange.transform.GetChild(1).GetComponent<Text>().text = orangenum.ToString();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			buttonClickCount["bell"].RegisterWithInitValue(bellnum =>
			{
				Bell.transform.GetChild(1).GetComponent<Text>().text = bellnum.ToString();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			buttonClickCount["blueberry"].RegisterWithInitValue(blueberrynum =>
			{
				Blueberry.transform.GetChild(1).GetComponent<Text>().text = blueberrynum.ToString();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			
		}
		private void InitializeButtonCounts()
		{
			// 先创建所有的 BindableProperty 对象
			buttonClickCount["apple"] = new BindableProperty<int>(0);
			buttonClickCount["king"] = new BindableProperty<int>(0);
			buttonClickCount["smallking"] = new BindableProperty<int>(0);
			buttonClickCount["doublestar"] = new BindableProperty<int>(0);
			buttonClickCount["sevenseven"] = new BindableProperty<int>(0);
			buttonClickCount["watermelon"] = new BindableProperty<int>(0);
			buttonClickCount["orange"] = new BindableProperty<int>(0);
			buttonClickCount["bell"] = new BindableProperty<int>(0);
			buttonClickCount["blueberry"] = new BindableProperty<int>(0);
			
		}
		public void ResetButtonCounts()
		{
			buttonClickCount["apple"].Value = 0;
			buttonClickCount["king"].Value = 0;
			buttonClickCount["smallking"].Value = 0;
			buttonClickCount["doublestar"].Value = 0;
			buttonClickCount["sevenseven"].Value = 0;
			buttonClickCount["watermelon"].Value = 0;
			buttonClickCount["orange"].Value = 0;
			buttonClickCount["bell"].Value = 0;
			buttonClickCount["blueberry"].Value = 0;
    
			Debug.Log("转动结束，所有按钮计数已重置为0");
		}

		private void BindButtonEvents()
		{
			if (Apple != null)
				Apple.GetComponent<Button>().onClick.AddListener(() => OnRewardNameButtonClick("apple"));
			if(King!=null)
				King.GetComponent<Button>().onClick.AddListener(() => OnRewardNameButtonClick("king"));
			if(SmallKing != null)
				SmallKing.GetComponent<Button>().onClick.AddListener(() => OnRewardNameButtonClick("smallking"));
			if(DoubleStar != null)
				DoubleStar.GetComponent<Button>().onClick.AddListener(() => OnRewardNameButtonClick("doublestar"));
			if(SevenSeven != null)
				SevenSeven.GetComponent<Button>().onClick.AddListener(() => OnRewardNameButtonClick("sevenseven"));
			if(Watermelon != null)
				Watermelon.GetComponent<Button>().onClick.AddListener(() => OnRewardNameButtonClick("watermelon"));
			if(Orange != null)
				Orange.GetComponent<Button>().onClick.AddListener(() => OnRewardNameButtonClick("orange"));
			if(Bell != null)
				Bell.GetComponent<Button>().onClick.AddListener(() => OnRewardNameButtonClick("bell"));
			if(Blueberry != null)
				Blueberry.GetComponent<Button>().onClick.AddListener(() => OnRewardNameButtonClick("blueberry"));

		}
		private string GetRewardNameCategory(string rewardName)
		{
			// 检查 RewardName 包含哪个关键词
			if (rewardName.Contains("苹果")) return "apple";
			if (rewardName.Contains("大王")) return "king";
			if (rewardName.Contains("小王")) return "smallking";
			if (rewardName.Contains("双星")) return "doublestar";
			if (rewardName.Contains("77")) return "sevenseven";
			if (rewardName.Contains("西瓜")) return "watermelon";
			if (rewardName.Contains("橘子")) return "orange";
			if (rewardName.Contains("铃铛")) return "bell";
			if (rewardName.Contains("蓝莓")) return "blueberry";

    
			return ""; // 如果都不包含，返回空字符串
		}

		private void OnRewardNameButtonClick(string rewardName)
		{
			if (isSpinning) return; // 转动期间禁止点击
    
			buttonClickCount[rewardName].Value++;
			Global.chips.Value -= 1;
			Debug.Log("当前筹码:"+Global.chips.Value);
			Debug.Log($"{rewardName} 按钮被点击，当前计数：{buttonClickCount[rewardName]}");
		}




		private void OnStartButtonClick(PointerEventData obj)
		{
			if (!StartButton.GetComponent<Button>().interactable) return;
			StartSpin();
			Global.lotteryTicket.Value -= 2;
		}
		public void StartSpin()
		{
			if (isSpinning || gridRects.Count == 0)
			{
				Debug.LogWarning("正在转动或没有格子可抽取！");
				return;
			}
			
			// 禁用Start按钮，防止重复点击
				StartButton.GetComponent<Button>().interactable = false;
			// 随机选择一个格子索引
			int randomIndex = GetWeightedRandomIndex();
			//int randomIndex = Random.Range(0, gridRects.Count);
			Debug.Log($"开始抽奖！目标格子索引：{randomIndex}");
			SpinToPosition(randomIndex);
		}
		public void SpinToPosition(int targetIndex)
		{
			if (isSpinning || targetIndex < 0 || targetIndex >= gridRects.Count)
			{
				Debug.LogWarning("无效的目标索引或正在转动！");
				return;
			}
			
			isSpinning = true;
			
			// 随机转的圈数
			int rotationCount = UnityEngine.Random.Range(minRotations, maxRotations + 1);
			
			// 计算总共需要移动的步数
			int totalSteps = rotationCount * gridRects.Count + GetStepsToTarget(currentIndex, targetIndex);
			
			Debug.Log($"从索引 {currentIndex} 转 {rotationCount} 圈到索引 {targetIndex}，总共移动 {totalSteps} 步");
			
			// 开始连续移动动画
			StartContinuousMovement(totalSteps, targetIndex);
		}
		/// <summary>
		/// 计算从当前索引到目标索引需要的步数（顺时针）
		/// </summary>
		/// <param name="fromIndex">起始索引</param>
		/// <param name="toIndex">目标索引</param>
		/// <returns>需要的步数</returns>
		private int GetStepsToTarget(int fromIndex, int toIndex)
		{
			if (toIndex >= fromIndex)
			{
				return toIndex - fromIndex;
			}
			else
			{
				return gridRects.Count - fromIndex + toIndex;
			}
		}
		/// <summary>
		/// 开始连续移动动画
		/// </summary>
		/// <param name="totalSteps">总移动步数</param>
		/// <param name="finalTargetIndex">最终目标索引</param>
		private void StartContinuousMovement(int totalSteps, int finalTargetIndex)
		{
			Sequence moveSequence = DOTween.Sequence();
			int tempCurrentIndex = currentIndex;
			
			// 为每一步创建移动动画
			for (int step = 0; step < totalSteps; step++)
			{
				// 计算下一个索引（循环）
				int nextIndex = (tempCurrentIndex + 1) % gridRects.Count;
				
				// 根据剩余步数调整移动速度（最后几步减速）
				float stepDuration = CalculateStepDuration(step, totalSteps);
				
				// 添加移动到下一个位置的动画
				moveSequence.Append(selectBoxRect.DOAnchorPos(gridRects[nextIndex].anchoredPosition, stepDuration)
					.SetEase(Ease.InOutQuad));
				
				tempCurrentIndex = nextIndex;
			}
			
			// 动画完成回调
			moveSequence.OnComplete(() =>
			{
				currentIndex = finalTargetIndex;
				isSpinning = false;
				OnSpinComplete(finalTargetIndex);
				
				// 重新启用Start按钮
				if (StartButton != null)
				{
					if (Score.Value < Global.levelScore.Value && Global.lotteryTicket.Value <= 0)
					{
						UIKit.ClosePanel<UIGamePanel>();
						UIKit.OpenPanel<UIGameOverPanel>();
					}
					
					StartButton.GetComponent<Button>().interactable = true;
					ResetGambling();
					ResetButtonCounts();
					if (Score.Value >= Global.levelScore.Value && Global.lotteryTicket.Value <= 0)
					{
						StartButton.GetComponent<Button>().interactable = false;
						Button[] allButtons = Panel.GetComponentsInChildren<Button>();
						foreach (Button button in allButtons)
						{
							button.interactable = false;
						}
					}
				}
				
				Debug.Log($"抽奖完成！最终停在索引：{finalTargetIndex}");
			});
		}

		/// <summary>
		/// 重新激活下注按钮
		/// </summary>
		public void ResetGambling()
		{
			Button[] allButtons = Panel.GetComponentsInChildren<Button>();
			foreach (Button button in allButtons)
			{
				button.interactable = true;
			}

			Global.chips.Value = 5;
		}

		private float CalculateStepDuration(int currentStep, int totalSteps)
		{
			// 前30%加速，中间40%匀速，后30%减速
			float progress = (float)currentStep / totalSteps;
			
			if (progress < 0.3f)
			{
				// 加速阶段：时间从较长到较短
				float accelerationFactor = 1f - (progress / 0.3f) * 0.5f;
				return moveSpeed * accelerationFactor;
			}
			else if (progress < 0.7f)
			{
				// 匀速阶段
				return moveSpeed * 0.5f;
			}
			else
			{
				// 减速阶段：时间从较短到较长
				float decelerationProgress = (progress - 0.7f) / 0.3f;
				float decelerationFactor = 0.5f + decelerationProgress * 3f;
				return moveSpeed * decelerationFactor;
			}
		}
		private void OnSpinComplete(int selectedIndex)
		{
			var card = cardItems[selectedIndex];
			string landedRewardName = card.rewardData.runtimeRewardName.Value;
    
			// 根据 RewardName 确定所属类别
			string category = GetRewardNameCategory(landedRewardName);
    
			if (string.IsNullOrEmpty(category))
			{
				Debug.LogWarning($"未找到 RewardName '{landedRewardName}' 对应的按钮类别");
				return;
			}
    
			// 获取该类别按钮的点击次数
			BindableProperty<int> clickCount = buttonClickCount[category];
    
			// 计算得分：按钮点击次数 × 卡片分值
			int baseScore = card.OnPlayerLand();
			int finalScore = baseScore * clickCount.Value;
    
			Score.Value += finalScore;
    
			Debug.Log($"停在 '{landedRewardName}' 卡片（类别：{category}），基础分值：{baseScore}，按钮点击次数：{clickCount}，最终得分：{finalScore}");
		}
		
		private void OnValidate()
		{
			//SideCount = Mathf.Max(2, SideCount);
			//TotalWidth = Mathf.Max(1f, TotalWidth);
		}
		
		public void GenerateGrid()
		{
			ClearCurrentGrids();
			CloneGridItems();
			ApplyDynamicLayout();
			for (int i = 0; i < cardItems.Count; i++)
			{
				totalWeight.Value += cardItems[i].rewardData.runtimeChanceWeight.Value;
			}
		}

		[ContextMenu("重新生成")]
		public void ReGenerateGrid()
		{
			GenerateGrid();
		}
		/// <summary>
		/// 清除现有格子
		/// </summary>
		private void ClearCurrentGrids()
		{
			if (GridItemPrefab == null)
			{
				return;
			}

			for (int i = GridContainer.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(GridContainer.GetChild(i).gameObject);
			}
			gridRects.Clear();
		}

		private void CloneGridItems()
		{
			if(GridItemPrefab == null||GridContainer==null)
			{
				Debug.LogError("GridPrefab 或 GridContainer 未设置！");
				return;
			}
			int configCount =currentPool.Rewards.Count;
			// 计算我们逻辑上需要的格数（7x7 是 24）
			int requiredCount = (SideCount * 4) - 4;

			// 安全检查：防止配置的数据量不足
			if (configCount < requiredCount)
			{
				Debug.LogError($"配置数据不足！当前需要 {requiredCount} 个，但配置只有 {configCount} 个。");
				return;
			}

			int TotalCount = (SideCount * 4) - 4;
			for (int i = 0; i < TotalCount; i++)
			{
				GameObject gridObj = Instantiate(GridItemPrefab,GridContainer)
					.Show();
				CardItem cardItem = gridObj.GetComponent<CardItem>();
				cardItem.index = i;
				RectTransform rectTransform = gridObj.GetComponent<RectTransform>();
				rectTransform.sizeDelta = new Vector2((TotalWidth-80)/SideCount, (TotalWidth-80)/SideCount);
				RewardData data = currentPool.Rewards[i];
				cardItem.Init(i, data);
				if (rectTransform != null)
				{
					gridRects.Add(rectTransform);
					cardItems.Add(cardItem);
				}
				else
				{
					Debug.LogError($"格子预制体 {gridObj.name} 没有 RectTransform 组件！");

				}
				
			}
		}

		private void ApplyDynamicLayout()
		{
			if(gridRects.Count==0||SideCount<2) return;
			float S=TotalWidth/(SideCount - 1);
			float offset = TotalWidth / 2f;
			for (int i = 0; i < gridRects.Count; i++)
			{
				Vector2 pos = CalculateGridPosition(i, S);
				pos.x -= offset;
				pos.y -= offset;
				gridRects[i].anchoredPosition = pos;
			}
		}

		private Vector2 CalculateGridPosition(int index, float spacing)

		{
			float x = 0f;
			float y = 0f;
			if (index < SideCount)
			{
				x=index *spacing;
				y=TotalWidth;
			}
			else if (index < 2 * SideCount - 1)
			{
				int offset = index - SideCount + 1;
				x=TotalWidth;
				y=TotalWidth -(offset*spacing);
			}
			else if (index < 3 * SideCount - 2)
			{
				int offset = index - (2*SideCount-1) + 1;
				x=TotalWidth-(offset*spacing);
				y=0;
			}
			else
			{
				int offset = index - (3*SideCount-2)+1;
				x = 0;
				y=offset*spacing;
			}
			return new Vector2(x, y);
			
		}

		public int GetGridCount()
		{
			return (SideCount*4)-4;
		}

		public float SetGridSpacing()
		{
			return SideCount>1?TotalWidth/(SideCount - 1):0f;
		}
		/// <summary>
		/// 根据权重随机选择一个卡牌索引
		/// </summary>
		/// <returns>选中的卡牌索引</returns>
		private int GetWeightedRandomIndex()
		{
			// 计算总权重
			totalWeight.Value = 0f;
			float currentTotalWeight = 0f;
			for (int i = 0; i < cardItems.Count; i++)
			{
				currentTotalWeight += cardItems[i].rewardData.runtimeChanceWeight.Value;
				totalWeight.Value = currentTotalWeight;
			}
    
			// 如果总权重为0，则使用等概率随机
			if (totalWeight.Value <= 0f)
			{
				Debug.LogWarning("所有卡牌权重为0，使用等概率随机选择！");
				return UnityEngine.Random.Range(0, cardItems.Count);
			}
    
			// 生成随机数
			float randomValue = UnityEngine.Random.Range(0f, totalWeight.Value);
    
			// 根据权重选择索引
			float currentWeight = 0f;
			for (int i = 0; i < cardItems.Count; i++)
			{
				currentWeight += cardItems[i].rewardData.runtimeChanceWeight.Value;
				if (randomValue <= currentWeight)
				{
					return i;
				}
			}
    
			// 如果出现浮点数精度问题，返回最后一个索引
			return cardItems.Count - 1;
		}

		public void CalculateCardWeights()
		{
			ScoreShow();
			cardNameWeights.Clear();
			float miniCardsWeight = 0f;
			foreach (var item in cardItems)
			{
				string cardName = item.rewardData.runtimeRewardName.Value;
				float weight = item.rewardData.runtimeChanceWeight.Value;
				bool isMini = item.rewardData.runTimeisMini;
				if (isMini)
				{
					miniCardsWeight += weight;
				}
				else
				{
					if (cardNameWeights.ContainsKey(cardName))
					{
						cardNameWeights[cardName] += weight;
					}
					else
					{
						cardNameWeights[cardName] = weight;
					}
				}
			}

			if (miniCardsWeight > 0f)
			{
				cardNameWeights["mini"] = miniCardsWeight;
			}
			if (totalWeight.Value > 0)
			{
				var keys = new List<string>(cardNameWeights.Keys);
				foreach (string key in keys)
				{
					cardNameWeights[key] = (cardNameWeights[key] / totalWeight.Value) * 100f;
				}
			}

		}
		/// <summary>
		/// UI分值更新显示
		/// </summary>
		public void ScoreShow()
		{
			cardNameScores.Clear();
			int miniCardScore = 3;
			foreach (var item in cardItems)
			{
				string cardName = item.rewardData.runtimeRewardName.Value;
				float score = item.rewardData.runtimeGoldValue.Value;
				bool isMini = item.rewardData.runTimeisMini;
				if (isMini)
				{
					cardNameScores["mini"] = miniCardScore.ToString();
				}
				else
				{
					if (cardNameScores.ContainsKey(cardName))
					{
						cardNameScores[cardName] = score.ToString();
					}
					else
					{
						cardNameScores[cardName] = score.ToString();
					}
				}
				
			}

		}
		
	}
}
