using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using QFramework;
using UnityEngine.EventSystems;
namespace Gambling
{
	public partial class EnchantmentComponent : ViewController,IController, IPointerEnterHandler, IPointerExitHandler
	{
		public List<EnchantmentEffect> ActiveEffects = new List<EnchantmentEffect>();
        
		[Header("UI 引用")]
		public GameObject TooltipPrefab;      // 悬浮窗预制体
		private GameObject mActiveTooltip;    // 当前生成的悬浮窗实例
        
		private Image mBorderImage;           // 当前图案的边框 Image
		private Sequence mLoopSequence;       // 负责边框动画的循环序列

		private void Start()
		{
			// 获取图案的边框 Image (假设在 CardItem 的第一个子物体上)
			mBorderImage = GetComponent<CardItem>().GetBorderImage();
			mBorderImage.Hide();
			TooltipPrefab=Resources.Load<GameObject>("Prefab/UI/EnchantmentDescriptionShowPanel");
            
			// 如果初始有附魔，开始边框轮播
			if (ActiveEffects.Count > 0)
			{
				StartBorderLoopAnimation();
			}
		}
		// 核心：处理附魔触发
		public void OnTrigger(EnchantmentTriggerType triggerType, GamblingGround ground)
		{
			CardItem card = GetComponent<CardItem>();
			List<EnchantmentEffect> toRemove = new List<EnchantmentEffect>();

			foreach (var effect in ActiveEffects)
			{
				bool canTrigger = false;

				if (!effect.NeedScoreCheck)
				{
					// 情况 A: 不需要检测得分，只要转动落位到此图案即可触发
					canTrigger = true;
				}
				else
				{
					// 情况 B: 必须押注后转动到得分图案才能触发
					// 1. 首先确保不是彩蛋图案（因为彩蛋通常不走常规得分流程）
					// 2. 其次确保该图案对应的分类有押注次数
					string category = ground.GetRewardNameCategory(card.rewardData.runtimeRewardName.Value);
					int betCount = ground.GetButtonClickCount(category);
            
					// 如果不是彩蛋且有押注，则视为“得分图案”触发
					canTrigger = !ground.IsCurrentlyEasterEggTriggering(card) && betCount > 0;
				}
                
				if (canTrigger)
				{
					effect.Execute(card, ground);
				}

				// 更新步数限制
				if (effect.Tick()) toRemove.Add(effect);
			}

			// 移除失效的附魔
			foreach (var expired in toRemove) ActiveEffects.Remove(expired);
		}
		/// <summary>
		/// 1. 边框循环动画：在图案边框上循环展示不同附魔
		/// </summary>
		public void StartBorderLoopAnimation()
		{
			if (ActiveEffects.Count == 0) return;

			// 清理旧序列
			mLoopSequence?.Kill();
			mLoopSequence = DOTween.Sequence();

			foreach (var effect in ActiveEffects)
			{
				// 逻辑：1s内边框变色/变图标 -> 停留1s -> 1s内恢复或切换
				mLoopSequence.AppendCallback(() => {
					if (mBorderImage != null && effect.Icon != null)
					{
						mBorderImage.sprite = effect.Icon; // 切换边框为附魔图标（或改变颜色）
						mBorderImage.Show();
					}
				});
                
				// 1s逐渐显现 (从透明到不透明)
				mLoopSequence.Append(mBorderImage.DOFade(1f, 1f));
				// 1s停留
				mLoopSequence.AppendInterval(1f);
				// 1s逐渐消失
				mLoopSequence.Append(mBorderImage.DOFade(0.2f, 1f)); // 保持一点点可见度
			}

			mLoopSequence.SetLoops(-1); // 无限循环
		}

		public IArchitecture GetArchitecture() => Global.Interface;

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (ActiveEffects.Count == 0 || TooltipPrefab == null) return;

			// ✅ 使用 QFramework 的 UIRoot 获取全局 Canvas
			Canvas canvas = UIRoot.Instance.Canvas;
			if (canvas == null) return;

			// 在 Canvas 下生成悬浮窗
			mActiveTooltip = Instantiate(TooltipPrefab, canvas.transform);
            
			// 设置位置：将鼠标坐标转换为 Canvas 本地坐标并设置偏移
			RectTransform rectTransform = mActiveTooltip.GetComponent<RectTransform>();
			if (rectTransform != null)
			{
				rectTransform.pivot = new Vector2(0, 1); // 设置左上角为轴心
                
				Vector2 localPoint;
				RectTransform canvasRect = canvas.GetComponent<RectTransform>();
				RectTransformUtility.ScreenPointToLocalPointInRectangle(
					canvasRect,
					Input.mousePosition,
					canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
					out localPoint
				);
                
				rectTransform.anchoredPosition = localPoint + new Vector2(20, -20); // 鼠标右下方轻微偏移
			}

			// 填充数据
			Text descText = mActiveTooltip.GetComponentInChildren<Text>();
			if (descText != null)
			{
				string fullDesc = "当前附魔效果：\n";
				foreach (var effect in ActiveEffects)
				{
					fullDesc += $"- {effect.EnchantmentName}: {effect.Description}\n";
				}
				descText.text = fullDesc;
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(mActiveTooltip.GetComponent<RectTransform>());
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (mActiveTooltip != null)
			{
				Destroy(mActiveTooltip);
				mActiveTooltip = null;
			}
		}
		// 当增加新附魔时，重启边框动画
		public void AddEffect(EnchantmentEffect effect)
		{
			ActiveEffects.RemoveAll(e => e.GetType() == effect.GetType());
			ActiveEffects.Add(effect);
			StartBorderLoopAnimation();
		}

		private void OnDestroy()
		{
			mLoopSequence?.Kill();
			if (mActiveTooltip != null) Destroy(mActiveTooltip);
		}
	}
}
