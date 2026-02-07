using UnityEngine;
using QFramework;
using DG.Tweening;
using UnityEngine.UI;
using System;

namespace Gambling
{
	public partial class ScorePopueUI : ViewController
	{
    [Header("UI组件")]
    public Text scoreText;           
    public RectTransform rectTransform;
    
    [Header("动画配置")]
    public float popDuration = 0.5f;     
    public float stayDuration = 2.0f;    
    public float fadeDuration = 0.3f;    
    public Vector2 popOffset = new Vector2(0, 100); 
    public Ease popEase = Ease.OutBack;   
    public Ease fadeEase = Ease.InQuad;   
    
    private Sequence currentSequence; 
    private Action onCompleteCallback; // 完成回调
    
    void Awake()
    {
        if (scoreText != null)
        {
            Color textColor = scoreText.color;
            textColor.a = 0f;
            scoreText.color = textColor;
        }
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 设置动画完成回调
    /// </summary>
    /// <param name="callback"></param>
    public void SetOnCompleteCallback(Action callback)
    {
        onCompleteCallback = callback;
    }
    
    /// <summary>
    /// 显示积分弹出动画
    /// </summary>
    /// <param name="score">积分数值</param>
    /// <param name="startPosition">起始位置</param>
    public void ShowScore(int score, Vector3 startPosition)
    {
        // 如果有正在进行的动画，先停止
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }
        
        // 激活游戏对象
        gameObject.SetActive(true);
        
        // 设置积分文本和颜色
        scoreText.text = "+" + score.ToString();
        SetScoreTextColor(score); // 根据积分设置颜色
        
        // 设置初始位置和透明度
        rectTransform.position = startPosition;
        Color startColor = scoreText.color;
        startColor.a = 0f;
        scoreText.color = startColor;

        
        // 计算目标位置
        Vector3 endPosition = startPosition + (Vector3)popOffset;
        // 保存目标颜色（完全不透明）
        Color targetColor = scoreText.color;
        targetColor.a = 1f;

        
        // 创建DoTween动画序列
        currentSequence = DOTween.Sequence();
        
        // 弹出阶段：同时进行位移和淡入动画
        currentSequence.Append(rectTransform.DOMove(endPosition, popDuration).SetEase(popEase));
        currentSequence.Join(scoreText.DOColor(targetColor, popDuration).SetEase(Ease.OutQuad));

        
        // 停留阶段
        currentSequence.AppendInterval(stayDuration);
        
        // 淡出阶段
        Color fadeColor = targetColor;
        fadeColor.a = 0f;
        currentSequence.Append(scoreText.DOColor(fadeColor, fadeDuration).SetEase(fadeEase));


        // 动画完成后执行回调
        currentSequence.OnComplete(() =>
        {
            currentSequence = null;
            onCompleteCallback?.Invoke(); // 触发回收回调
        });
        
        Debug.Log($"积分弹出动画开始：+{score} 分");
    }
    
    /// <summary>
    /// 根据积分值设置文本颜色
    /// </summary>
    /// <param name="score"></param>
    private void SetScoreTextColor(int score)
    {
        if (scoreText == null) return;
        
        // 根据积分高低设置不同颜色
        if (score >= 100)
        {
            scoreText.color = Color.yellow; // 高分用金色
        }
        else if (score >= 50)
        {
            scoreText.color = Color.green;   // 中分用青色
        }
        else
        {
            scoreText.color = Color.red;  // 低分用白色
        }
    }
    
    /// <summary>
    /// 立即停止当前动画
    /// </summary>
    public void StopCurrentAnimation()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
            currentSequence = null;
        }
        gameObject.SetActive(false);
        
        // 触发回收回调
        onCompleteCallback?.Invoke();
    }
    
    void OnDestroy()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }
    }

	}
}
