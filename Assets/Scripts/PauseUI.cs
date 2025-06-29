using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 暂停界面控制器
/// </summary>
public class PauseUI : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI timeText; // 显示当前用时的文本
    public Slider volumeSlider; // 音量控制滑块

    private void Start()
    {
        // 初始化音量滑块
        InitializeVolumeSlider();
    }

    private void Update()
    {
        // 实时更新时间显示
        if (timeText != null && GameManager.Instance != null)
        {
            timeText.text = $"当前用时: {GameManager.Instance.GetFormattedTime()}";
        }
    }

    /// <summary>
    /// 初始化音量滑块
    /// </summary>
    private void InitializeVolumeSlider()
    {
        if (volumeSlider != null && AudioManager.Instance != null)
        {
            // 设置滑块的范围
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;

            // 设置当前音量值
            volumeSlider.value = AudioManager.Instance.GetMasterVolume();

            // 添加事件监听
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    /// <summary>
    /// 音量滑块值改变时调用
    /// </summary>
    /// <param name="volume">新的音量值</param>
    public void OnVolumeChanged(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(volume);
        }
    }

    private void OnDestroy()
    {
        // 清理事件监听，避免内存泄漏
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }
    }
}