using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 按钮音效自动设置器 - 自动为场景中的按钮添加音效管理器
/// </summary>
public class ButtonSoundAutoSetup : MonoBehaviour
{
    [Header("自动设置")]
    public bool autoSetupOnStart = true; // 是否在Start时自动设置
    public bool setupOnEnable = false; // 是否在OnEnable时设置

    [Header("默认音效设置")]
    public string defaultHoverSound = "ButtonHover"; // 默认悬停音效
    public string defaultClickSound = "ButtonClick"; // 默认点击音效
    public float defaultHoverVolume = 0.8f; // 默认悬停音量
    public float defaultClickVolume = 1.0f; // 默认点击音量

    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupAllButtons();
        }
    }

    private void OnEnable()
    {
        if (setupOnEnable)
        {
            SetupAllButtons();
        }
    }

    /// <summary>
    /// 为场景中的所有按钮设置音效管理器
    /// </summary>
    public void SetupAllButtons()
    {
        // 查找场景中所有的Button组件
        Button[] buttons = FindObjectsOfType<Button>();
        
        Debug.Log($"ButtonSoundAutoSetup: 找到 {buttons.Length} 个按钮");

        foreach (Button button in buttons)
        {
            SetupButtonSound(button);
        }
    }

    /// <summary>
    /// 为指定按钮设置音效管理器
    /// </summary>
    /// <param name="button">要设置的按钮</param>
    public void SetupButtonSound(Button button)
    {
        if (button == null) return;

        // 检查按钮是否已经有ButtonSoundManager组件
        ButtonSoundManager existingManager = button.GetComponent<ButtonSoundManager>();
        if (existingManager != null)
        {
            Debug.Log($"ButtonSoundAutoSetup: 按钮 {button.name} 已经有音效管理器，跳过");
            return;
        }

        // 添加ButtonSoundManager组件
        ButtonSoundManager soundManager = button.gameObject.AddComponent<ButtonSoundManager>();
        
        // 设置默认音效
        soundManager.hoverSoundName = defaultHoverSound;
        soundManager.clickSoundName = defaultClickSound;
        soundManager.hoverVolume = defaultHoverVolume;
        soundManager.clickVolume = defaultClickVolume;

        Debug.Log($"ButtonSoundAutoSetup: 为按钮 {button.name} 添加了音效管理器");
    }

    /// <summary>
    /// 为指定GameObject的按钮设置音效管理器
    /// </summary>
    /// <param name="targetObject">目标GameObject</param>
    public void SetupButtonSoundOnObject(GameObject targetObject)
    {
        if (targetObject == null) return;

        Button button = targetObject.GetComponent<Button>();
        if (button != null)
        {
            SetupButtonSound(button);
        }
        else
        {
            Debug.LogWarning($"ButtonSoundAutoSetup: GameObject {targetObject.name} 没有Button组件");
        }
    }

    /// <summary>
    /// 移除指定按钮的音效管理器
    /// </summary>
    /// <param name="button">要移除音效管理器的按钮</param>
    public void RemoveButtonSound(Button button)
    {
        if (button == null) return;

        ButtonSoundManager soundManager = button.GetComponent<ButtonSoundManager>();
        if (soundManager != null)
        {
            DestroyImmediate(soundManager);
            Debug.Log($"ButtonSoundAutoSetup: 移除了按钮 {button.name} 的音效管理器");
        }
    }

    /// <summary>
    /// 移除场景中所有按钮的音效管理器
    /// </summary>
    public void RemoveAllButtonSounds()
    {
        ButtonSoundManager[] managers = FindObjectsOfType<ButtonSoundManager>();
        
        foreach (ButtonSoundManager manager in managers)
        {
            if (manager != null)
            {
                DestroyImmediate(manager);
            }
        }

        Debug.Log($"ButtonSoundAutoSetup: 移除了 {managers.Length} 个音效管理器");
    }
} 