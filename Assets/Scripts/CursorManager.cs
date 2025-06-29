using UnityEngine;

/// <summary>
/// 鼠标样式管理器 - 管理鼠标的默认和按下状态样式
/// </summary>
public class CursorManager : MonoBehaviour
{
    [Header("鼠标样式设置")]
    [Tooltip("默认状态的鼠标纹理")]
    public Texture2D defaultCursorTexture;

    [Tooltip("按下状态的鼠标纹理")]
    public Texture2D pressedCursorTexture;

    [Header("鼠标热点设置")]
    [Tooltip("默认状态鼠标的热点位置（像素坐标）")]
    public Vector2 defaultHotspot = Vector2.zero;

    [Tooltip("按下状态鼠标的热点位置（像素坐标）")]
    public Vector2 pressedHotspot = Vector2.zero;

    [Header("调试设置")]
    [Tooltip("是否显示调试信息")]
    public bool showDebugInfo = false;

    // 单例实例
    public static CursorManager Instance { get; private set; }

    // 当前鼠标状态
    private bool isPressed = false;

    // 鼠标模式
    private CursorMode cursorMode = CursorMode.Auto;

    void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCursor();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 设置默认鼠标样式
        SetDefaultCursor();
    }

    void Update()
    {
        // 检测鼠标按下状态
        HandleMouseInput();
    }

    /// <summary>
    /// 初始化鼠标设置
    /// </summary>
    private void InitializeCursor()
    {
        // 验证纹理是否设置
        if (defaultCursorTexture == null)
        {
            Debug.LogWarning("CursorManager: 默认鼠标纹理未设置！");
        }

        if (pressedCursorTexture == null)
        {
            Debug.LogWarning("CursorManager: 按下状态鼠标纹理未设置！");
        }

        // 自动设置热点为纹理中心（如果没有手动设置的话）
        if (defaultCursorTexture != null && defaultHotspot == Vector2.zero)
        {
            defaultHotspot = new Vector2(defaultCursorTexture.width / 2f, defaultCursorTexture.height / 2f);
        }

        if (pressedCursorTexture != null && pressedHotspot == Vector2.zero)
        {
            pressedHotspot = new Vector2(pressedCursorTexture.width / 2f, pressedCursorTexture.height / 2f);
        }

        if (showDebugInfo)
        {
            Debug.Log($"CursorManager: 默认鼠标热点 {defaultHotspot}, 按下鼠标热点 {pressedHotspot}");
        }
    }

    /// <summary>
    /// 处理鼠标输入
    /// </summary>
    private void HandleMouseInput()
    {
        // 检测鼠标左键按下
        if (Input.GetMouseButtonDown(0))
        {
            if (!isPressed)
            {
                isPressed = true;
                SetPressedCursor();

                if (showDebugInfo)
                {
                    Debug.Log("CursorManager: 鼠标按下，切换到按下状态样式");
                }
            }
        }

        // 检测鼠标左键释放
        if (Input.GetMouseButtonUp(0))
        {
            if (isPressed)
            {
                isPressed = false;
                SetDefaultCursor();

                if (showDebugInfo)
                {
                    Debug.Log("CursorManager: 鼠标释放，切换到默认状态样式");
                }
            }
        }
    }

    /// <summary>
    /// 设置默认鼠标样式
    /// </summary>
    public void SetDefaultCursor()
    {
        if (defaultCursorTexture != null)
        {
            Cursor.SetCursor(defaultCursorTexture, defaultHotspot, cursorMode);
        }
        else
        {
            // 如果没有设置纹理，使用系统默认鼠标
            Cursor.SetCursor(null, Vector2.zero, cursorMode);

            if (showDebugInfo)
            {
                Debug.LogWarning("CursorManager: 默认鼠标纹理为空，使用系统默认鼠标");
            }
        }
    }

    /// <summary>
    /// 设置按下状态鼠标样式
    /// </summary>
    public void SetPressedCursor()
    {
        if (pressedCursorTexture != null)
        {
            Cursor.SetCursor(pressedCursorTexture, pressedHotspot, cursorMode);
        }
        else
        {
            // 如果没有设置按下状态纹理，保持默认样式
            if (showDebugInfo)
            {
                Debug.LogWarning("CursorManager: 按下状态鼠标纹理为空，保持默认样式");
            }
        }
    }

    /// <summary>
    /// 强制设置鼠标样式（外部调用）
    /// </summary>
    /// <param name="texture">鼠标纹理</param>
    /// <param name="hotspot">热点位置</param>
    public void SetCustomCursor(Texture2D texture, Vector2 hotspot)
    {
        if (texture != null)
        {
            Cursor.SetCursor(texture, hotspot, cursorMode);

            if (showDebugInfo)
            {
                Debug.Log($"CursorManager: 设置自定义鼠标样式，热点位置 {hotspot}");
            }
        }
    }

    /// <summary>
    /// 重置为系统默认鼠标
    /// </summary>
    public void ResetToSystemCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
        isPressed = false;

        if (showDebugInfo)
        {
            Debug.Log("CursorManager: 重置为系统默认鼠标");
        }
    }

    /// <summary>
    /// 设置鼠标是否可见
    /// </summary>
    /// <param name="visible">是否可见</param>
    public void SetCursorVisible(bool visible)
    {
        Cursor.visible = visible;

        if (showDebugInfo)
        {
            Debug.Log($"CursorManager: 设置鼠标可见性 {visible}");
        }
    }

    /// <summary>
    /// 设置鼠标锁定模式
    /// </summary>
    /// <param name="lockState">锁定状态</param>
    public void SetCursorLockState(CursorLockMode lockState)
    {
        Cursor.lockState = lockState;

        if (showDebugInfo)
        {
            Debug.Log($"CursorManager: 设置鼠标锁定状态 {lockState}");
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // 当应用程序失去焦点时，重置鼠标状态
        if (!hasFocus && isPressed)
        {
            isPressed = false;
            SetDefaultCursor();

            if (showDebugInfo)
            {
                Debug.Log("CursorManager: 应用失去焦点，重置鼠标状态");
            }
        }
    }

    void OnDestroy()
    {
        // 销毁时重置为系统默认鼠标
        if (Instance == this)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    // 编辑器相关：在Inspector中显示当前状态信息
    void OnValidate()
    {
        // 验证热点位置是否合理
        if (defaultCursorTexture != null)
        {
            defaultHotspot.x = Mathf.Clamp(defaultHotspot.x, 0, defaultCursorTexture.width);
            defaultHotspot.y = Mathf.Clamp(defaultHotspot.y, 0, defaultCursorTexture.height);
        }

        if (pressedCursorTexture != null)
        {
            pressedHotspot.x = Mathf.Clamp(pressedHotspot.x, 0, pressedCursorTexture.width);
            pressedHotspot.y = Mathf.Clamp(pressedHotspot.y, 0, pressedCursorTexture.height);
        }
    }
}