using UnityEngine;
using UnityEngine.UI;
using Gambling;
using DG.Tweening;

namespace Gambling
{
    /// <summary>
    /// 单个押注进度条UI组件
    /// </summary>
    public class BetProgressBarUI : MonoBehaviour
    {
        [Header("配置")]
        [Tooltip("对应的按钮类别")]
        [SerializeField] private string category;
    
        [Header("UI引用")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Text multiplierText;
    
        private GamblingGround gamblingGround;

        void Start()
        {
            gamblingGround = FindObjectOfType<GamblingGround>();
        }

        void Update()
        {
            if (gamblingGround == null) return;
        
            BetProgressInfo info = gamblingGround.GetBetProgressInfo(category);
        
            // 更新进度条
            if (fillImage != null)
            {
                fillImage.fillAmount = info.progressPercent;
                if (fillImage.fillAmount == 1)
                {
                    fillImage.fillAmount = 0f;
                }
            }
        
            // 更新倍率文本
            if (multiplierText != null)
            {
                if (info.currentMultiplier > 1)
                {
                    multiplierText.text = $"*{info.currentMultiplier}";
                    multiplierText.gameObject.SetActive(true);
                }
                else
                {
                    multiplierText.gameObject.SetActive(false);
                }
            }
        }
    }

}

