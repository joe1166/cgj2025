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
        // 使用摄像机方法调整整体亮度
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // 保存摄像机原始设置（首次调用时）
            string originalBgColorKey = "OriginalCameraBgColor";
            if (!PlayerPrefs.HasKey(originalBgColorKey + "_R"))
            {
                Color originalBg = mainCamera.backgroundColor;
                PlayerPrefs.SetFloat(originalBgColorKey + "_R", originalBg.r);
                PlayerPrefs.SetFloat(originalBgColorKey + "_G", originalBg.g);
                PlayerPrefs.SetFloat(originalBgColorKey + "_B", originalBg.b);
                PlayerPrefs.SetFloat(originalBgColorKey + "_A", originalBg.a);
            }

            // 获取原始背景色
            Color originalBgColor = new Color(
                PlayerPrefs.GetFloat(originalBgColorKey + "_R"),
                PlayerPrefs.GetFloat(originalBgColorKey + "_G"),
                PlayerPrefs.GetFloat(originalBgColorKey + "_B"),
                PlayerPrefs.GetFloat(originalBgColorKey + "_A")
            );

            // 根据亮度调整背景色
            Color adjustedBgColor = originalBgColor * brightness;
            mainCamera.backgroundColor = adjustedBgColor;
        }

        // 调整全局环境光
        if (!PlayerPrefs.HasKey("OriginalAmbientIntensity"))
        {
            PlayerPrefs.SetFloat("OriginalAmbientIntensity", RenderSettings.ambientIntensity);
        }

        float originalAmbientIntensity = PlayerPrefs.GetFloat("OriginalAmbientIntensity");
        RenderSettings.ambientIntensity = originalAmbientIntensity * brightness;

        // 调整所有光源的强度
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (Light light in allLights)
        {
            string lightKey = "OriginalLightIntensity_" + light.GetInstanceID();

            // 保存原始光照强度
            if (!PlayerPrefs.HasKey(lightKey))
            {
                PlayerPrefs.SetFloat(lightKey, light.intensity);
            }

            float originalIntensity = PlayerPrefs.GetFloat(lightKey);
            light.intensity = originalIntensity * brightness;
        }

        Debug.Log($"摄像机亮度已调整至: {brightness * 100f}%");
        Debug.Log($"环境光强度: {RenderSettings.ambientIntensity}");
        Debug.Log($"摄像机背景色: {mainCamera.backgroundColor}");
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