using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Gambling
{
    public static class EasterEggEffectHelper
    {
            /// <summary>
            /// 从彩蛋位置根据索引依次移动到目标卡片位置
            /// </summary>
            /// <param name="selectBox">要移动的框选框</param>
            /// <param name="gamblingGround">游戏场景</param>
            /// <param name="fromIndex">起始索引（彩蛋位置）</param>
            /// <param name="targetIndex">目标索引</param>
            /// <returns></returns>
            public static IEnumerator MoveSelectBoxToTarget(GameObject selectBox, GamblingGround gamblingGround, int fromIndex, int targetIndex,bool shouldResetAtEnd = false)
            {
                var rectTransform = selectBox.GetComponent<RectTransform>();
                var gridRects = gamblingGround.gridRects; // 直接访问public字段
                
                // 计算需要移动的步数（顺时针方向）
                int totalSteps = GetStepsToTarget(fromIndex, targetIndex, gridRects.Count);
                
                if (totalSteps == 0)
                {
                    // 如果已经在目标位置，直接返回
                    yield break;
                }
                
                // 创建移动序列
                Sequence moveSequence = DOTween.Sequence();
                int currentIndex = fromIndex;
                
                // 为每一步创建移动动画
                for (int step = 0; step < totalSteps; step++)
                {
                    // 计算下一个索引（循环）
                    int nextIndex = (currentIndex + 1) % gridRects.Count;
                    
                    // 彩蛋移动比正常转盘快一些，使用较短的移动时间
                    float stepDuration = gamblingGround.moveSpeed * 0.5f; // 比正常转盘快
                    
                    // 添加移动到下一个位置的动画
                    moveSequence.Append(rectTransform.DOAnchorPos(gridRects[nextIndex].anchoredPosition, stepDuration)
                        .SetEase(Ease.InOutQuad));
                    
                    currentIndex = nextIndex;
                }
                
                // 等待移动完成
                yield return moveSequence.WaitForCompletion();
                //gamblingGround.ResetButtonCounts();
                if (shouldResetAtEnd)
                {
                    gamblingGround.ResetButtonCounts();
                    gamblingGround.SetEasterEggExecuting(false);
                }
                

                
            }
            
            /// <summary>
            /// 计算从起始索引到目标索引需要的步数（顺时针方向）
            /// </summary>
            /// <param name="fromIndex">起始索引</param>
            /// <param name="toIndex">目标索引</param>
            /// <param name="totalCount">总卡片数量</param>
            /// <returns>需要的步数</returns>
            private static int GetStepsToTarget(int fromIndex, int toIndex, int totalCount)
            {
                if (toIndex >= fromIndex)
                {
                    return toIndex - fromIndex;
                }
                else
                {
                    return totalCount - fromIndex + toIndex;
                }
            }
            /// <summary>
            /// 闪烁辅助功能
            /// </summary>
            /// <param name="gamblingGround"></param>
            /// <param name="duration"></param>
            /// <returns></returns>
            
            public static IEnumerator FlashCurrentPosition(GamblingGround gamblingGround, float duration)
            {
                var selectBox = gamblingGround.SelectBox;
                var originalColor = selectBox.GetComponent<Image>().color;
                
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    float alpha = 0.3f + (Mathf.Sin(elapsed * 8f) * 0.4f + 0.4f);
                    var flashColor = originalColor;
                    flashColor.a = alpha;
                    selectBox.GetComponent<Image>().color = flashColor;
                    
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                
                selectBox.GetComponent<Image>().color = originalColor;
            }
            /// <summary>
            /// 创建新框选框
            /// </summary>
            /// <param name="gamblingGround"></param>
            /// <param name="fromIndex"></param>
            /// <returns></returns>
            public static GameObject CreateNewSelectBox(GamblingGround gamblingGround, int fromIndex)
            {
                var originalSelectBox = gamblingGround.SelectBox;
                var newSelectBox = Object.Instantiate(originalSelectBox, originalSelectBox.transform.parent);
                
                // 设置初始位置为彩蛋位置
                newSelectBox.GetComponent<RectTransform>().anchoredPosition = gamblingGround.gridRects[fromIndex].anchoredPosition;
                
                // 添加到临时框选框列表中（需要在GamblingGround中添加这个方法）
                gamblingGround.AddTemporarySelectBox(newSelectBox);
                
                return newSelectBox;
            }
            
            public static int TriggerCardEffect(GamblingGround gamblingGround, CardItem targetCard,Dictionary<string, int> savedButtonCounts = null)
            {
                // 获取卡片基础分值
                int baseScore = targetCard.OnPlayerLand();
                string landedRewardName = targetCard.rewardData.runtimeRewardName.Value;
                    
                // 根据 RewardName 确定所属类别
                string category = gamblingGround.GetRewardNameCategory(landedRewardName);
                    
                if (string.IsNullOrEmpty(category))
                {
                    Debug.LogWarning($"彩蛋效果：未找到 RewardName '{landedRewardName}' 对应的按钮类别");
                    return 0;
                }
                int clickCount = 0;
                if (savedButtonCounts != null && savedButtonCounts.ContainsKey(category))
                {
                    clickCount = savedButtonCounts[category];
                    Debug.Log($"🔍 使用保存的 {category} 点击次数: {clickCount}");
                }
                else
                {
                    clickCount = gamblingGround.GetButtonClickCount(category);
                    Debug.Log($"🔍 使用当前的 {category} 点击次数: {clickCount}");
                }

                // 计算最终得分：基础分值 × 按钮点击次数
                int finalScore = baseScore * clickCount;
                // 只有在按钮被点击过时才获得分数
                if (clickCount > 0)
                {
                    gamblingGround.Score.Value += finalScore;
                        
                    Debug.Log($"🎉 彩蛋触发卡片: {landedRewardName} (类别: {category})");
                    Debug.Log($"📊 基础分值: {baseScore} × 点击次数: {clickCount} = 最终得分: {finalScore}");
                        
                    // 显示积分弹出动画
                    //gamblingGround.ShowSingleScorePopup(category, finalScore);
                }
                else
                {
                    Debug.Log($"⚠️ 彩蛋触发卡片: {landedRewardName} (类别: {category})，但按钮点击次数为0，未获得分数");
                }
                    
                return finalScore;
            }

            
        }

}