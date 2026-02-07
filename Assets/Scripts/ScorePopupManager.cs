using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Gambling
{
    /// <summary>
    /// 积分弹出UI对象池管理器
    /// </summary>
    public class ScorePopupManager : MonoBehaviour
    {
        [Header("配置")] [SerializeField] private ScorePopueUI scorePopupPrefab; // 积分弹出UI预制体
        [SerializeField] private Transform uiParent; // UI父级容器
        [SerializeField] private int poolSize = 5; // 对象池初始大小
        [SerializeField] private int maxPoolSize = 10; // 对象池最大大小

        // 对象池
        private Queue<ScorePopueUI> availablePopups = new Queue<ScorePopueUI>();
        private List<ScorePopueUI> activePopups = new List<ScorePopueUI>();

        void Awake()
        {
            InitializePool();
        }

        /// <summary>
        /// 初始化对象池
        /// </summary>
        private void InitializePool()
        {
            if (scorePopupPrefab == null)
            {
                Debug.LogError("ScorePopueUI预制体未设置！");
                return;
            }

            if (uiParent == null)
            {
                uiParent = transform; // 如果没有设置父级，使用自身作为父级
            }

            // 预创建指定数量的UI对象
            for (int i = 0; i < poolSize; i++)
            {
                CreateNewPopupUI();
            }

            Debug.Log($"积分弹出UI对象池初始化完成，预创建 {poolSize} 个对象");
        }

        /// <summary>
        /// 创建新的弹出UI对象
        /// </summary>
        /// <returns></returns>
        private ScorePopueUI CreateNewPopupUI()
        {
            GameObject popupObj = Instantiate(scorePopupPrefab.gameObject, uiParent);
            ScorePopueUI popupUI = popupObj.GetComponent<ScorePopueUI>();

            if (popupUI == null)
            {
                Debug.LogError("积分弹出UI预制体缺少ScorePopueUI组件！");
                Destroy(popupObj);
                return null;
            }

            // 设置回调，动画完成时回收对象
            popupUI.SetOnCompleteCallback(() => ReturnToPool(popupUI));

            popupObj.SetActive(false);
            availablePopups.Enqueue(popupUI);

            return popupUI;
        }

        /// <summary>
        /// 从对象池获取一个可用的弹出UI
        /// </summary>
        /// <returns></returns>
        private ScorePopueUI GetPopupFromPool()
        {
            ScorePopueUI popup = null;

            // 如果没有可用的对象，尝试创建新的
            if (availablePopups.Count == 0)
            {
                if (activePopups.Count + availablePopups.Count < maxPoolSize)
                {
                    popup = CreateNewPopupUI();
                    if (popup != null)
                    {
                        availablePopups.Dequeue(); // 移除刚加入的对象
                    }
                }
                else
                {
                    Debug.LogWarning("积分弹出UI对象池已达到最大容量！");
                    return null;
                }
            }
            else
            {
                popup = availablePopups.Dequeue();
            }

            if (popup != null)
            {
                activePopups.Add(popup);
            }

            return popup;
        }

        /// <summary>
        /// 将对象回收到对象池
        /// </summary>
        /// <param name="popup"></param>
        private void ReturnToPool(ScorePopueUI popup)
        {
            if (popup == null) return;

            // 从活跃列表中移除
            activePopups.Remove(popup);

            // 重置对象状态
            popup.gameObject.SetActive(false);
            popup.transform.SetAsLastSibling(); // 重置层级

            // 回收到可用队列
            availablePopups.Enqueue(popup);

            Debug.Log($"积分弹出UI已回收到对象池，当前可用对象数：{availablePopups.Count}");
        }

        /// <summary>
        /// 显示积分弹出动画
        /// </summary>
        /// <param name="score">积分数值</param>
        /// <param name="startPosition">起始位置</param>
        /// <param name="delay">延迟时间（用于错开多个动画）</param>
        public void ShowScorePopup(int score, Vector3 startPosition, float delay = 0f)
        {
            ScorePopueUI popup = GetPopupFromPool();

            if (popup == null)
            {
                Debug.LogWarning("无法获取积分弹出UI对象！");
                return;
            }

            // 如果有延迟，使用DOTween延迟执行
            if (delay > 0f)
            {
                DOVirtual.DelayedCall(delay, () =>
                {
                    if (popup != null && popup.gameObject != null)
                    {
                        popup.ShowScore(score, startPosition);
                    }
                });
            }
            else
            {
                popup.ShowScore(score, startPosition);
            }

            Debug.Log($"显示积分弹出动画：+{score} 分，延迟 {delay} 秒");
        }

        /// <summary>
        /// 批量显示多个积分弹出动画
        /// </summary>
        /// <param name="scoreData">积分数据列表（积分值，起始位置）</param>
        /// <param name="delayBetween">每个动画之间的间隔时间</param>
        public void ShowMultipleScorePopups(List<(int score, Vector3 position)> scoreData, float delayBetween = 0.2f)
        {
            for (int i = 0; i < scoreData.Count; i++)
            {
                var data = scoreData[i];
                float delay = i * delayBetween;
                ShowScorePopup(data.score, data.position, delay);
            }

            Debug.Log($"批量显示 {scoreData.Count} 个积分弹出动画，间隔 {delayBetween} 秒");
        }

        /// <summary>
        /// 立即停止所有动画并回收对象
        /// </summary>
        public void StopAllAnimations()
        {
            // 停止所有活跃的动画
            for (int i = activePopups.Count - 1; i >= 0; i--)
            {
                if (activePopups[i] != null)
                {
                    activePopups[i].StopCurrentAnimation();
                    // StopCurrentAnimation 会触发回调，自动回收对象
                }
            }

            Debug.Log("所有积分弹出动画已停止");
        }

        void OnDestroy()
        {
            StopAllAnimations();
        }
    }
}

