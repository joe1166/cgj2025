using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 按钮音效管理器 - 为按钮添加悬停和按下音效
/// </summary>
public class ButtonSoundManager : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("音效设置")]
    public string hoverSoundName = "ButtonHover"; // 悬停音效名称
    public string clickSoundName = "ButtonClick"; // 点击音效名称
    
    [Header("音量设置")]
    public float hoverVolume = 0.8f; // 悬停音效音量
    public float clickVolume = 1.0f; // 点击音效音量

    [Header("防重复设置")]
    public float hoverCooldown = 0.1f; // 悬停音效冷却时间

    private Button button;
    private float lastHoverTime = 0f; // 上次播放悬停音效的时间

    private void Start()
    {
        // 获取Button组件
        button = GetComponent<Button>();
        
        if (button == null)
        {
            Debug.LogWarning($"ButtonSoundManager: GameObject {gameObject.name} 没有Button组件");
        }
    }

    /// <summary>
    /// 鼠标悬停时播放音效
    /// </summary>
    /// <param name="eventData">事件数据</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 只有在按钮可交互时才播放音效
        if (button != null && button.interactable)
        {
            PlayHoverSound();
        }
    }

    /// <summary>
    /// 鼠标点击时播放音效
    /// </summary>
    /// <param name="eventData">事件数据</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 只有在按钮可交互时才播放音效
        if (button != null && button.interactable)
        {
            PlayClickSound();
        }
    }

    /// <summary>
    /// 播放悬停音效
    /// </summary>
    private void PlayHoverSound()
    {
        // 检查冷却时间，避免重复播放
        if (Time.time - lastHoverTime < hoverCooldown)
        {
            return;
        }

        if (AudioManager.Instance != null && !string.IsNullOrEmpty(hoverSoundName))
        {
            AudioManager.Instance.PlaySFX(hoverSoundName, hoverVolume);
            lastHoverTime = Time.time;
        }
    }

    /// <summary>
    /// 播放点击音效
    /// </summary>
    private void PlayClickSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(clickSoundName))
        {
            AudioManager.Instance.PlaySFX(clickSoundName, clickVolume);
        }
    }

    /// <summary>
    /// 手动播放悬停音效（供外部调用）
    /// </summary>
    public void PlayHoverSoundManually()
    {
        PlayHoverSound();
    }

    /// <summary>
    /// 手动播放点击音效（供外部调用）
    /// </summary>
    public void PlayClickSoundManually()
    {
        PlayClickSound();
    }
} 