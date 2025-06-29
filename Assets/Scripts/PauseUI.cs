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
    public Slider brightnessSlider; // 亮度控制滑块

    [Header("亮度控制")]
    public Image brightnessOverlay; // 用于控制亮度的全屏覆盖层（可选）

    private void Start()
    {
        // 初始化音量滑块
        InitializeVolumeSlider();

        // 初始化亮度滑块
        InitializeBrightnessSlider();
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
    /// 初始化亮度滑块
    /// </summary>
    private void InitializeBrightnessSlider()
    {
        if (brightnessSlider != null)
        {
            // 设置滑块的范围
            brightnessSlider.minValue = 0.2f; // 最低亮度20%
            brightnessSlider.maxValue = 1f;   // 最高亮度100%

            // 从PlayerPrefs读取保存的亮度设置，默认为1（最亮）
            float savedBrightness = PlayerPrefs.GetFloat("GameBrightness", 1f);
            brightnessSlider.value = savedBrightness;

            // 应用初始亮度
            ApplyBrightness(savedBrightness);

            // 添加事件监听
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        }
    }

    /// <summary>
    /// 音量滑块值改变时调用
    /// </summary>
    /// <param name="volume">新的音量值</param>
    public void OnVolumeChanged(float volume)
    {
        Debug.Log("音量改变: " + volume);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(volume);

            // 强制更新BGM音量作为备用方案
            AudioManager.Instance.ForceUpdateBGMVolume();

            // 调试信息
            Debug.Log($"当前主音量: {AudioManager.Instance.GetMasterVolume()}");
            Debug.Log($"当前BGM音量: {AudioManager.Instance.GetBGMVolume()}");
        }
    }

    /// <summary>
    /// 亮度滑块值改变时调用
    /// </summary>
    /// <param name="brightness">新的亮度值</param>
    public void OnBrightnessChanged(float brightness)
    {
        Debug.Log("亮度改变: " + brightness);

        // 应用亮度
        ApplyBrightness(brightness);

        // 保存亮度设置
        PlayerPrefs.SetFloat("GameBrightness", brightness);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 应用亮度设置
    /// </summary>
    /// <param name="brightness">亮度值（0.2-1.0）</param>
    private void ApplyBrightness(float brightness)
    {
        // 方法1：使用覆盖层UI Image（如果有的话）
        if (brightnessOverlay != null)
        {
            Color overlayColor = brightnessOverlay.color;
            // 亮度越低，覆盖层越暗（alpha越高）
            overlayColor.a = 1f - brightness;
            brightnessOverlay.color = overlayColor;
        }

        // 方法2：调整主摄像机的颜色（通用方法）
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // 使用Camera的backgroundColor来影响整体亮度
            // 但这主要影响清屏颜色，对渲染内容影响有限
            Color cameraColor = mainCamera.backgroundColor;
            cameraColor = Color.Lerp(Color.black, Color.white, brightness);
            // 注意：这种方法效果有限，建议使用覆盖层方法
        }

        // 方法3：使用全局光照调整（如果场景中有光源）
        RenderSettings.ambientIntensity = brightness;

        Debug.Log($"亮度已调整至: {brightness * 100f}%");
    }

    private void OnDestroy()
    {
        // 清理事件监听，避免内存泄漏
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);
        }
    }
}