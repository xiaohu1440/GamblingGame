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
	/// <summary>
	/// 颜色与分数配置
	/// </summary>
	[System.Serializable]
	public class ColorScoreConfig
	{
		public string colorName;        // 颜色名称（用于Inspector显示）
		public Color color;             // 颜色值
		public int score;               // 对应积分
		[Range(0f, 100f)]
		public float weight = 1f;       // 生成权重（权重越高，出现概率越大）
	}
	public partial class GamblingGround : ViewController,IController
	{
		public IArchitecture GetArchitecture() => Global.Interface;
		[Header("配置模块")]
		[SerializeField]private int SideCount = 7;
		[SerializeField]private float TotalWidth = 800f;
		[SerializeField]private GameObject GridItemPrefab;
		[SerializeField]private Transform GridContainer;
		[Header("转盘动画配置")]
		[SerializeField]public float moveSpeed = 0.05f; // 移动速度（每个格子的移动时间）
		[SerializeField]private int minRotations = 3;
		[SerializeField]private int maxRotations = 8;
		[SerializeField] private RewardList currentPool;
		[Header("积分弹出UI管理")]
		[SerializeField]private ScorePopupManager scorePopupManager;
		[Header("彩蛋系统")]
		[SerializeField] private EasterEggManager easterEggManager;
		// 临时框选框列表，用于管理彩蛋效果产生的框选框
		private List<GameObject> temporarySelectBoxes = new List<GameObject>();
		[Header("单图案押注倍率系统")]
		[SerializeField] private List<BetMultiplierConfig> customMultiplierConfigs = new List<BetMultiplierConfig>
		{
			new BetMultiplierConfig(10, 2),
			new BetMultiplierConfig(20, 4),
			new BetMultiplierConfig(30, 8)
		};
		public Dictionary<string, float> cardNameWeights = new Dictionary<string, float>();
		public Dictionary<string, string> cardNameScores = new Dictionary<string, string>();
		public BindableProperty<int> Score = new BindableProperty<int>(0);
		public Dictionary<string,BindableProperty<int>> buttonClickCount=new Dictionary<string,BindableProperty<int>>();
		public BindableProperty<int> currentDoubleNum = new BindableProperty<int>(1);
		public BindableProperty<float> totalWeight = new BindableProperty<float>(0);
		public BindableProperty<float> totalColorWeight = new BindableProperty<float>(0);
		public List<RectTransform> gridRects = new List<RectTransform>();
		public List<CardItem> cardItems = new List<CardItem>();
		private Dictionary<string, Button> categoryButtons = new Dictionary<string, Button>();
		private RectTransform selectBoxRect;
		private bool isSpinning = false;
		private int currentIndex = 0; // 当前SelectBox所在的索引
		public bool isEasterEggExecuting = false;
		[Header("框选卡片颜色配置")]
		[SerializeField] public ColorScoreConfig[] colorConfigs = new ColorScoreConfig[7]
		{
			new ColorScoreConfig { colorName = "白色", color = Color.white, score = 0, weight =50f },
			new ColorScoreConfig { colorName = "绿色", color = Color.green, score = 5, weight =20f },
			new ColorScoreConfig { colorName = "蓝色", color = new Color(0.3f, 0.5f, 1f), score = 10, weight = 12f },
			new ColorScoreConfig { colorName = "黄色", color = Color.yellow, score = 15, weight =8f },
			new ColorScoreConfig { colorName = "紫色", color = new Color(0.8f, 0.3f, 1f), score = 20, weight = 5f },
			new ColorScoreConfig { colorName = "橙色", color = new Color(1f, 0.6f, 0f), score = 25, weight =5f },
			new ColorScoreConfig { colorName = "红色", color = Color.red, score = 30, weight = 1f }
		};

		private int[] colorScores = new int[7] { 0, 5, 10, 15, 20, 25, 30 };
		private int finalColorBonusScore = 0; // 最终颜色奖励分数
		private BetMultiplierSystem betMultiplierSystem;


		void Start()
		{
			// 检查积分弹出管理器
			if (scorePopupManager == null)
			{
				scorePopupManager = FindObjectOfType<ScorePopupManager>();
				if (scorePopupManager == null)
				{
					Debug.LogWarning("未找到ScorePopupManager！请在场景中添加该组件。");
				}
			}
			// 初始化倍率系统
			betMultiplierSystem = new BetMultiplierSystem(customMultiplierConfigs);
			Debug.Log("押注倍率系统已初始化");
			InitializeCategoryButtons();
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
			StartButton.GetComponent<Button>().onClick.AddListener(OnStartButtonClick);
			//StartButton.OnPointerClickEvent(OnStartButtonClick);
			// 初始化按钮计数
			InitializeButtonCounts();
    
			// 绑定按钮点击事件
			BindButtonEvents();
			TextUpdate();
			if (easterEggManager == null)
			{
				easterEggManager = GetComponent<EasterEggManager>();
				if (easterEggManager == null)
				{
					easterEggManager = gameObject.AddComponent<EasterEggManager>();
				}
			}
			UpdateStartButtonState();



		}

		private void InitializeCategoryButtons()
		{
			// 建立按钮映射关系
			categoryButtons["apple"] = Apple.GetComponent<Button>();
			categoryButtons["king"] = King.GetComponent<Button>();
			categoryButtons["smallking"] = SmallKing.GetComponent<Button>();
			categoryButtons["doublestar"] = DoubleStar.GetComponent<Button>();
			categoryButtons["sevenseven"] = SevenSeven.GetComponent<Button>();
			categoryButtons["watermelon"] = Watermelon.GetComponent<Button>();
			categoryButtons["orange"] = Orange.GetComponent<Button>();
			categoryButtons["bell"] = Bell.GetComponent<Button>();
			categoryButtons["blueberry"] = Blueberry.GetComponent<Button>();

		}

		private void Init()
		{
			Score.Value = 0;
			Global.chips.Value = 5;
			Global.lotteryTicket.Value = 10;
			Global.levelScore.Value = 10;
			Global.level.Value = 1;
			for (int i = 0; i < colorConfigs.Length; i++)
			{
				totalColorWeight.Value += colorConfigs[i].weight;
			}

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
			UpdateStartButtonState();
    
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
		public string GetRewardNameCategory(string rewardName)
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
			if (isSpinning||isEasterEggExecuting) return; // 转动期间禁止点击
			ClearTemporarySelectBoxes();
    
			buttonClickCount[rewardName].Value++;
			Global.chips.Value -= 1;
			UpdateStartButtonState();
			Debug.Log("当前筹码:"+Global.chips.Value);
			Debug.Log($"{rewardName} 按钮被点击，当前计数：{buttonClickCount[rewardName]}");
		}




		private void OnStartButtonClick()
		{
			NextLevelBtn.GetComponent<Button>().interactable = false;
			if (!StartButton.GetComponent<Button>().interactable||isEasterEggExecuting) return;
			ClearTemporarySelectBoxes();
			StartSpin();
			Global.lotteryTicket.Value -= 2;
			Global.currentLevelSpinCount.Value++;
		}
		public void StartSpin()
		{
			if (isEasterEggExecuting||isSpinning || gridRects.Count == 0)
			{
				Debug.LogWarning("正在转动或没有格子可抽取！");
				return;
			}
			finalColorBonusScore = 0;
			// 禁用Start按钮，防止重复点击
			DisableAllButtons();
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
			cardItems[tempCurrentIndex].GetCardBaseImage().color = Color.white;
			int previousIndex = -1; // 记录上一个格子的索引
			
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
				// ✅ 修改：每次移动时根据权重随机改变当前格子颜色
				int currentStepIndex = nextIndex; // 捕获当前步骤的索引
				int prevStepIndex = previousIndex; // 捕获上一个步骤的索引
        
				moveSequence.AppendCallback(() =>
				{
					// 重置上一个格子的颜色为白色
					if (prevStepIndex >= 0 && prevStepIndex < cardItems.Count)
					{
						Image prevBaseImage = cardItems[prevStepIndex].GetCardBaseImage();
						if (prevBaseImage != null)
						{
							prevBaseImage.color = Color.white;
						}
					}
            
					// ✅ 使用加权随机改变当前格子的颜色
					if (currentStepIndex >= 0 && currentStepIndex < cardItems.Count)
					{
						Image baseImage = cardItems[currentStepIndex].GetCardBaseImage();
						if (baseImage != null)
						{
							int randomColorIndex = GetWeightedRandomColorIndex();
							baseImage.color = colorConfigs[randomColorIndex].color;
						}
					}
				});
				
				previousIndex = nextIndex;
				tempCurrentIndex = nextIndex;
				
			}
			
			// 动画完成回调
			moveSequence.OnComplete(() =>
			{
				currentIndex = finalTargetIndex;
				isSpinning = false;
				// ✅ 修改：使用加权随机选择最终颜色并设置奖励分数
				int finalColorIndex = GetWeightedRandomColorIndex();
				finalColorBonusScore = colorConfigs[finalColorIndex].score;
				// 设置最终停留位置的卡片底板颜色（带动画效果）
				if (finalTargetIndex >= 0 && finalTargetIndex < cardItems.Count)
				{
					Image baseImage = cardItems[finalTargetIndex].GetCardBaseImage();
					if (baseImage != null)
					{
						baseImage.DOColor(colorConfigs[finalColorIndex].color, 0.3f).SetEase(Ease.OutQuad);
						Debug.Log($"最终停在索引 {finalTargetIndex}，颜色：{colorConfigs[finalColorIndex].colorName}，颜色奖励：+{finalColorBonusScore}分");
					}
				}
				OnSpinComplete(finalTargetIndex);
				
				// 重新启用Start按钮
				if (StartButton != null)
				{
					bool easterEggTriggered = false;
        
					// ✅ 只在这里处理彩蛋逻辑（唯一的彩蛋触发点）
					if (easterEggManager != null)
					{
						// 在彩蛋触发前保存当前按钮计数
						Dictionary<string, int> savedButtonCounts = SaveCurrentButtonCounts();
						easterEggTriggered = easterEggManager.TryTriggerEasterEgg(this, cardItems[finalTargetIndex], cardItems, savedButtonCounts);
            
						if (easterEggTriggered)
						{
							// 彩蛋触发时禁用所有按钮
							SetEasterEggExecuting(true);
							Debug.Log("🥚 彩蛋被触发，所有按钮已禁用");
						}
					}

					// ✅ 只有在没有触发彩蛋时才立即恢复按钮状态
					if (!easterEggTriggered)
					{
						if (Score.Value < Global.levelScore.Value && Global.lotteryTicket.Value <= 0)
						{
							UIKit.ClosePanel<UIGamePanel>();
							UIKit.OpenPanel<UIGameOverPanel>();
						}
						ResetButtonCounts();
						ResetGambling();
						Debug.Log("🎯 转盘结束，按钮状态已恢复");
					}
					if (Score.Value >= Global.levelScore.Value && Global.lotteryTicket.Value <= 0)
					{
						StartButton.GetComponent<Button>().interactable = false;
						// 禁用所有下注按钮
						foreach (var kvp in categoryButtons)
						{
							if (kvp.Value != null)
							{
								kvp.Value.interactable = false;
							}
						}
    
						// 禁用加倍按钮
						if (DoubleBetBtn != null)
						{
							DoubleBetBtn.GetComponent<Button>().interactable = false;
						}
    
						// ✅ 确保NextLevelBtn保持可用（因为分数已达标）
						if (NextLevelBtn != null)
						{
							NextLevelBtn.GetComponent<Button>().interactable = true;
						}
					}
					else
					{
						
					}
				}
				
				Debug.Log($"抽奖完成！最终停在索引：{finalTargetIndex}");
			});
		}

		public void SetEasterEggExecuting(bool executing)
		{
			isEasterEggExecuting = executing;
        
			if (executing)
			{
				// 彩蛋开始执行：禁用所有按钮
				DisableAllButtons();
				Debug.Log("🥚 彩蛋效果开始执行，所有按钮已禁用");
			}
			else
			{
				// 彩蛋执行完成：重新启用按钮
				EnableAllButtons();
				ResetGambling();
				Debug.Log("🥚 彩蛋效果执行完成，所有按钮已重新启用");
			}
		}
		// ✅ 禁用所有按钮的方法
		public void DisableAllButtons()
		{
			// 禁用Start按钮
			if (StartButton != null)
			{
				StartButton.GetComponent<Button>().interactable = false;
			}
        
			// 禁用所有押注按钮
			foreach (var kvp in categoryButtons)
			{
				if (kvp.Value != null)
				{
					kvp.Value.interactable = false;
				}
			}

			if (NextLevelBtn != null)
			{
				NextLevelBtn.GetComponent<Button>().interactable = false;
			}
			if (DoubleBetBtn != null)
			{
				DoubleBetBtn.GetComponent<Button>().interactable = false;
			}
		}
    
		// ✅ 启用所有按钮的方法
		public void EnableAllButtons()
		{
			// 启用Start按钮
			if (StartButton != null)
			{
				UpdateStartButtonState();
			}
        
			// 启用所有押注按钮
			foreach (var kvp in categoryButtons)
			{
				if (kvp.Value != null)
				{
					kvp.Value.interactable = true;
				}
			}
			if (NextLevelBtn != null)
			{
				bool shouldEnable = Score.Value >= Global.levelScore.Value;
				NextLevelBtn.GetComponent<Button>().interactable = shouldEnable;

			}

			if (DoubleBetBtn != null)
			{
				bool shouldEnableDoubleBet=Global.lotteryTicket.Value>2;
				DoubleBetBtn.GetComponent<Button>().interactable = shouldEnableDoubleBet;
			}
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

			Global.chips.Value += 5;
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
			// 检查是否触发彩蛋效果

			if (!IsCurrentlyEasterEggTriggering(card))
			{
				// 根据 RewardName 确定所属类别
				string category = GetRewardNameCategory(landedRewardName);
    
				if (string.IsNullOrEmpty(category))
				{
					Debug.LogWarning($"未找到 RewardName '{landedRewardName}' 对应的按钮类别");
					return;
				}
    
				// 获取该类别按钮的点击次数
				BindableProperty<int> clickCount = buttonClickCount[category];
				int betMultiplier = betMultiplierSystem.GetMultiplier(clickCount.Value);
				// 计算得分：按钮点击次数 × 卡片分值
				int baseScore = card.OnPlayerLand();
				int finalScore = (baseScore+finalColorBonusScore) * betMultiplier*clickCount.Value*currentDoubleNum.Value;
    
				Score.Value += finalScore;
				currentDoubleNum.Value = 1;
				Debug.Log($"停在 '{landedRewardName}' 卡片（类别：{category}），基础分值：{baseScore}，按钮点击次数：{clickCount}，颜色奖励：{finalColorBonusScore}，最终得分：{finalScore}");
				// 如果有积分获得且按钮点击次数大于0，显示积分弹出动画
				if (finalScore > 0 && clickCount.Value > 0)
				{
					ShowSingleScorePopup(category, finalScore);
				}
				//TODO:ui弹出彩蛋功能关联
			}
			TriggerRotationEndRelics(selectedIndex, card);

		}

		private bool IsCurrentlyEasterEggTriggering(CardItem card)
		{
			return card.rewardData.runtimeCardType == CardItem.CardType.彩蛋;
		}

		/// <summary>
		/// 显示单个积分弹出动画
		/// </summary>
		/// <param name="category">按钮类别</param>
		/// <param name="score">获得的积分</param>
		public void ShowSingleScorePopup(string category, int score)
		{
			if (scorePopupManager == null) return;

			Vector3 buttonPosition = GetButtonPosition(category);
			if (buttonPosition != Vector3.zero)
			{
				scorePopupManager.ShowScorePopup(score, buttonPosition);
				Debug.Log($"从 {category} 按钮处弹出积分UI，显示 +{score} 分");
			}
		}
		/// <summary>
		/// 获取按钮的世界坐标位置
		/// </summary>
		/// <param name="category">按钮类别</param>
		/// <returns>按钮位置，如果找不到返回Vector3.zero</returns>
		private Vector3 GetButtonPosition(string category)
		{
			if (!categoryButtons.ContainsKey(category) || categoryButtons[category] == null)
			{
				Debug.LogWarning($"未找到类别 '{category}' 对应的按钮！");
				return Vector3.zero;
			}

			return categoryButtons[category].transform.position;
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
		// 添加临时框选框到管理列表
		public void AddTemporarySelectBox(GameObject selectBox)
		{
			if (!temporarySelectBoxes.Contains(selectBox))
			{
				temporarySelectBoxes.Add(selectBox);
			}
		}
		// 设置当前框选框
		public void SetCurrentSelectBox(GameObject newSelectBox, int newIndex)
		{
			// 隐藏原来的框选框
			SelectBox.Hide();
        
			// 设置新的框选框为当前框选框
			SelectBox = newSelectBox;
			currentIndex = newIndex;
			selectBoxRect = newSelectBox.GetComponent<RectTransform>();
        
			Debug.Log($"彩蛋效果完成，当前框选框切换到索引: {newIndex}");
		}
		// 清理所有临时框选框
		public void ClearTemporarySelectBoxes()
		{
			foreach (var tempBox in temporarySelectBoxes)
			{
				if (tempBox != null && tempBox != SelectBox)
				{
					DestroyImmediate(tempBox);
				}
			}
			temporarySelectBoxes.Clear();
			Debug.Log("所有临时框选框已清理");
		}
		/// <summary>
		/// 获取指定类别按钮的点击次数（供彩蛋系统使用）
		/// </summary>
		/// <param name="category">按钮类别</param>
		/// <returns>点击次数</returns>
		public int GetButtonClickCount(string category)
		{
			if (buttonClickCount.ContainsKey(category))
			{
				return buttonClickCount[category].Value;
			}
			return 0;
		}
		private Dictionary<string, int> SaveCurrentButtonCounts()
		{
			Dictionary<string, int> savedCounts = new Dictionary<string, int>();
			foreach (var kvp in buttonClickCount)
			{
				savedCounts[kvp.Key] = kvp.Value.Value;
			}
			Debug.Log("已保存当前按钮计数用于彩蛋效果");
			return savedCounts;
		}
		/// <summary>
		/// 检查是否有押注
		/// </summary>
		/// <returns>如果有任何押注返回true，否则返回false</returns>
		private bool HasAnyBet()
		{
			foreach (var kvp in buttonClickCount)
			{
				if (kvp.Value.Value > 0)
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 更新StartButton的可交互状态
		/// </summary>
		public void UpdateStartButtonState()
		{
			if (StartButton != null)
			{
				Button startBtn = StartButton.GetComponent<Button>();
				if (startBtn != null)
				{
					startBtn.interactable = HasAnyBet();
				}
			}
		}
		/// <summary>
		/// 根据权重随机选择一个颜色索引
		/// </summary>
		/// <returns>选中的颜色索引</returns>
		private int GetWeightedRandomColorIndex()
		{
			totalColorWeight.Value = 0;
			for (int i = 0; i < colorConfigs.Length; i++)
			{
				totalColorWeight.Value += colorConfigs[i].weight;
			}
    
			// 如果总权重为0，则使用等概率随机
			if (totalColorWeight.Value <= 0f)
			{
				Debug.LogWarning("所有颜色权重为0，使用等概率随机选择！");
				return Random.Range(0, colorConfigs.Length);
			}
    
			// 生成随机数
			float randomValue = Random.Range(0f, totalColorWeight.Value);
    
			// 根据权重选择索引
			float currentWeight = 0f;
			for (int i = 0; i < colorConfigs.Length; i++)
			{
				currentWeight += colorConfigs[i].weight;
				if (randomValue <= currentWeight)
				{
					return i;
				}
			}
    
			// 如果出现浮点数精度问题，返回最后一个索引
			return colorConfigs.Length - 1;
		}
		private void TriggerRotationEndRelics(int selectedIndex, CardItem card)
		{
			// 获取遗物系统
			var relicSystem = this.GetSystem<IRelicSystem>();
    
			// 创建遗物效果上下文
			RelicEffectContext context = new RelicEffectContext
			{
				gamblingGround = this,
				currentIndex = selectedIndex,
				currentCard = card,
				currentDoubleNum = currentDoubleNum.Value,
				currentRotationNum = Global.currentLevelSpinCount.Value,
				RelicNum = relicSystem.GetOwnedRelicDatas().Count
			};
    
			// 触发旋转结束类型的遗物效果
			relicSystem.TriggerRelicEffect(RelicTriggerType.OnRotationEnd, context);
    
			Debug.Log($"🎲 触发旋转结束遗物效果，当前索引：{selectedIndex}，卡片：{card.rewardData.runtimeRewardName.Value}");
		}
		/// <summary>
		/// 获取指定类别的押注倍率
		/// </summary>
		/// <param name="category">按钮类别</param>
		/// <returns>当前倍率</returns>
		public int GetBetMultiplier(string category)
		{
			if (betMultiplierSystem == null || !buttonClickCount.ContainsKey(category))
			{
				return 1;
			}
    
			int betCount = buttonClickCount[category].Value;
			return betMultiplierSystem.GetMultiplier(betCount);
		}

		/// <summary>
		/// 获取指定类别的押注进度信息（供UI进度条使用）
		/// </summary>
		/// <param name="category">按钮类别</param>
		/// <returns>进度信息</returns>
		public BetProgressInfo GetBetProgressInfo(string category)
		{
			if (betMultiplierSystem == null || !buttonClickCount.ContainsKey(category))
			{
				return new BetProgressInfo
				{
					currentBetCount = 0,
					currentMultiplier = 1,
					progressPercent = 0f
				};
			}
    
			int betCount = buttonClickCount[category].Value;
			return betMultiplierSystem.GetProgressInfo(betCount);
		}

		/// <summary>
		/// 获取所有类别的押注进度信息（供UI批量更新使用）
		/// </summary>
		/// <returns>所有类别的进度信息字典</returns>
		public Dictionary<string, BetProgressInfo> GetAllBetProgressInfo()
		{
			Dictionary<string, BetProgressInfo> allProgress = new Dictionary<string, BetProgressInfo>();
    
			if (betMultiplierSystem == null) return allProgress;
    
			foreach (var kvp in buttonClickCount)
			{
				allProgress[kvp.Key] = betMultiplierSystem.GetProgressInfo(kvp.Value.Value);
			}
    
			return allProgress;
		}




		
	}
}
