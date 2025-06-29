using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 关卡结束控制器 - 监听游戏状态变化并控制下一关按钮行为
/// </summary>
public class LevelEnd : MonoBehaviour
{
    [Header("UI组件")]
    public Button nextLevelButton; // 下一关按钮（手动绑定）

    [Header("按钮躲避设置")]
    public float moveSpeed = 10f; // 按钮移动速度
    public float detectionRadius = 300f; // 鼠标检测半径
    public Vector2 screenBounds = new Vector2(100f, 100f); // 屏幕边界偏移

    private RectTransform buttonRectTransform; // 按钮的RectTransform
    private Vector2 originalPosition; // 按钮的原始位置
    private bool isLevelComplete = false; // 关卡是否完成
    private Canvas canvas; // Canvas组件

    private void Start()
    {
        // 获取按钮的RectTransform
        if (nextLevelButton != null)
        {
            buttonRectTransform = nextLevelButton.GetComponent<RectTransform>();
            originalPosition = buttonRectTransform.anchoredPosition;

            // 获取Canvas组件
            canvas = nextLevelButton.GetComponentInParent<Canvas>();

            // 验证Canvas模式
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogWarning("LevelEnd: Canvas不是Screen Space - Overlay模式，按钮躲避可能不会正常工作");
            }
        }
        else
        {
            Debug.LogWarning("LevelEnd: 下一关按钮未绑定！");
        }

        // 订阅游戏状态变化事件
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }
        else
        {
            Debug.LogError("LevelEnd: 找不到GameManager实例！");
        }
    }

    private void Update()
    {
        // 只有在关卡完成时才执行按钮躲避逻辑
        if (isLevelComplete && buttonRectTransform != null)
        {
            HandleButtonEvasion();
        }
    }

    /// <summary>
    /// 处理游戏状态变化
    /// </summary>
    /// <param name="newState">新的游戏状态</param>
    private void OnGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.LevelComplete)
        {
            isLevelComplete = true;
            Debug.Log("LevelEnd: 关卡完成，开始按钮躲避模式");
        }
        else
        {
            isLevelComplete = false;
            // 重置按钮位置
            if (buttonRectTransform != null)
            {
                buttonRectTransform.anchoredPosition = originalPosition;
            }
        }
    }

    /// <summary>
    /// 处理按钮躲避鼠标的逻辑
    /// </summary>
    private void HandleButtonEvasion()
    {
        // 获取鼠标在屏幕上的位置
        Vector2 mousePosition = Input.mousePosition;

        // 获取按钮在屏幕上的位置
        Vector2 buttonScreenPosition = GetButtonScreenPosition();

        // 计算鼠标和按钮之间的距离
        float distance = Vector2.Distance(mousePosition, buttonScreenPosition);


        // 计算从鼠标指向按钮的方向（远离方向）
        Vector2 direction = (buttonScreenPosition - mousePosition);

        // 如果鼠标和按钮位置几乎重合，使用随机方向
        if (direction.magnitude < 100f)
        {
            direction = Random.insideUnitCircle.normalized * 3; // 给一个随机方向
        }
        else
        {
            direction = direction.normalized;
        }

        // 计算移动距离，距离越近移动越快
        float moveMultiplier = 0;
        if (distance < detectionRadius * 5)
        {
            moveMultiplier = Mathf.Max(3f, Mathf.Sqrt(detectionRadius / distance));
        }

        // 计算新的屏幕位置
        Vector2 newScreenPosition = buttonScreenPosition + (direction * moveSpeed * moveMultiplier);

        // 将屏幕位置转换为anchoredPosition
        Vector2 newAnchoredPosition = ScreenToAnchoredPosition(newScreenPosition);

        // 限制按钮在屏幕边界内
        newAnchoredPosition = ClampPositionToScreen(newAnchoredPosition);
        Debug.Log("newAnchoredPosition: " + newAnchoredPosition);

        // 应用新位置
        buttonRectTransform.anchoredPosition = newAnchoredPosition;

    }

    /// <summary>
    /// 获取按钮在屏幕上的位置
    /// </summary>
    /// <returns>按钮的屏幕坐标</returns>
    private Vector2 GetButtonScreenPosition()
    {
        // 对于Screen Space - Overlay模式，按钮的position就是屏幕坐标
        return buttonRectTransform.position;
    }

    /// <summary>
    /// 将屏幕坐标转换为anchoredPosition
    /// </summary>
    /// <param name="screenPosition">屏幕坐标</param>
    /// <returns>anchoredPosition</returns>
    private Vector2 ScreenToAnchoredPosition(Vector2 screenPosition)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPosition,
            null,
            out localPoint);

        return localPoint;
    }

    /// <summary>
    /// 将位置限制在屏幕边界内
    /// </summary>
    /// <param name="anchoredPosition">要限制的anchoredPosition</param>
    /// <returns>限制后的位置</returns>
    private Vector2 ClampPositionToScreen(Vector2 anchoredPosition)
    {
        // 获取Canvas的RectTransform
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // 获取Canvas的尺寸
        Vector2 canvasSize = canvasRect.sizeDelta;

        // 获取按钮尺寸
        Vector2 buttonSize = buttonRectTransform.sizeDelta;

        // 计算边界（相对于Canvas中心）
        float halfCanvasWidth = canvasSize.x * 0.5f;
        float halfCanvasHeight = canvasSize.y * 0.5f;
        float halfButtonWidth = buttonSize.x * 0.5f;
        float halfButtonHeight = buttonSize.y * 0.5f;

        float minX = -halfCanvasWidth + screenBounds.x + halfButtonWidth;
        float maxX = halfCanvasWidth - screenBounds.x - halfButtonWidth;
        float minY = -halfCanvasHeight + screenBounds.y + halfButtonHeight;
        float maxY = halfCanvasHeight - screenBounds.y - halfButtonHeight;

        // 限制位置
        anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, minX, maxX);
        anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, minY, maxY);

        return anchoredPosition;
    }

    /// <summary>
    /// 重置按钮位置到原始位置
    /// </summary>
    public void ResetButtonPosition()
    {
        if (buttonRectTransform != null)
        {
            buttonRectTransform.anchoredPosition = originalPosition;
        }
    }

    /// <summary>
    /// 手动设置按钮位置
    /// </summary>
    /// <param name="position">新位置</param>
    public void SetButtonPosition(Vector2 position)
    {
        if (buttonRectTransform != null)
        {
            buttonRectTransform.anchoredPosition = position;
        }
    }

    /// <summary>
    /// 启用或禁用按钮躲避功能
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public void SetEvasionEnabled(bool enabled)
    {
        isLevelComplete = enabled;
        if (!enabled && buttonRectTransform != null)
        {
            buttonRectTransform.anchoredPosition = originalPosition;
        }
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
    }
}

