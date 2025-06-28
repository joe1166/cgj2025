using UnityEngine;

/// <summary>
/// 腿部状态枚举
/// </summary>
public enum LegsState
{
    Moving,     // 移动状态 - 播放跑动动画
    Dragging,   // 拖拽状态 - 播放拖拽动画
    Settled     // 放置状态 - 播放放置动画
}

/// <summary>
/// 腿部管理器 - 使用状态机管理左右腿的动画
/// 需要和DraggableItem挂载在同一个GameObject上
/// </summary>
public class LegsManager : MonoBehaviour
{
    [Header("腿部预制体")]
    public GameObject leftLegPrefab;  // 左腿预制体
    public GameObject rightLegPrefab; // 右腿预制体

    [Header("腿部位置")]
    public Vector2 leftLegPosition = new Vector2(-0.5f, -0.3f);   // 左腿位置
    public Vector2 rightLegPosition = new Vector2(0.5f, -0.3f);   // 右腿位置

    [Header("动画参数名称")]
    public string movingAnimationParam = "IsMoving";     // 移动动画参数
    public string draggingAnimationParam = "IsDragging"; // 拖拽动画参数
    public string settledAnimationParam = "IsSettled";   // 放置动画参数

    [Header("排序层设置")]
    public string dragSortingLayer = "Drag"; // 拖拽时的排序层
    public string itemSortingLayer = "Item"; // 放置后的排序层
    public string itemMoveSortingLayer = "ItemMove"; // 移动时的排序层

    // 腿部实例
    private GameObject leftLegInstance;
    private GameObject rightLegInstance;

    // 动画控制器
    private Animator leftLegAnimator;
    private Animator rightLegAnimator;

    // 腿部渲染器
    private SpriteRenderer leftLegRenderer;
    private SpriteRenderer rightLegRenderer;

    // 状态管理
    private LegsState currentState = LegsState.Moving;
    private DraggableItem draggableItem;
    private MovableItem movableItem;

    // 状态机事件
    public System.Action<LegsState> OnStateChanged;

    private void Start()
    {
        // 获取相关组件
        draggableItem = GetComponent<DraggableItem>();
        movableItem = GetComponent<MovableItem>();

        if (draggableItem == null)
        {
            Debug.LogError($"LegsManager: GameObject {gameObject.name} 上没有找到DraggableItem组件！");
            return;
        }

        // 等待DraggableItem初始化完成后再实例化腿部
        StartCoroutine(InitializeLegs());
    }

    /// <summary>
    /// 初始化腿部的协程
    /// </summary>
    private System.Collections.IEnumerator InitializeLegs()
    {
        // 等待DraggableItem的Init方法执行完成
        yield return new WaitForEndOfFrame();

        // 检查ItemData是否存在
        if (draggableItem.ItemData == null)
        {
            Debug.LogError($"LegsManager: DraggableItem的ItemData为空！");
            yield break;
        }

        // 使用ItemData中的腿部位置（如果有的话）
        if (draggableItem.ItemData.leftLegPosition != Vector2.zero)
        {
            leftLegPosition = draggableItem.ItemData.leftLegPosition;
        }
        if (draggableItem.ItemData.rightLegPosition != Vector2.zero)
        {
            rightLegPosition = draggableItem.ItemData.rightLegPosition;
        }

        // 实例化腿部
        CreateLegs();
    }

    /// <summary>
    /// 创建腿部实例
    /// </summary>
    private void CreateLegs()
    {
        // 创建左腿
        if (leftLegPrefab != null)
        {
            Vector3 leftLegWorldPosition = transform.position + (Vector3)leftLegPosition;
            leftLegInstance = Instantiate(leftLegPrefab, leftLegWorldPosition, Quaternion.identity, transform);
            leftLegAnimator = leftLegInstance.GetComponent<Animator>();
            leftLegRenderer = leftLegInstance.GetComponent<SpriteRenderer>();

            if (leftLegAnimator == null)
            {
                Debug.LogWarning($"LegsManager: 左腿预制体上没有找到Animator组件！");
            }
        }

        // 创建右腿
        if (rightLegPrefab != null)
        {
            Vector3 rightLegWorldPosition = transform.position + (Vector3)rightLegPosition;
            rightLegInstance = Instantiate(rightLegPrefab, rightLegWorldPosition, Quaternion.identity, transform);
            rightLegAnimator = rightLegInstance.GetComponent<Animator>();
            rightLegRenderer = rightLegInstance.GetComponent<SpriteRenderer>();

            if (rightLegAnimator == null)
            {
                Debug.LogWarning($"LegsManager: 右腿预制体上没有找到Animator组件！");
            }
        }

        // 确保腿部初始可见
        ShowLegs(true);

        // 设置初始排序层
        SetLegsSortingLayer(itemMoveSortingLayer);

        // 设置初始状态
        SetState(LegsState.Moving);

        Debug.Log($"LegsManager: 成功创建腿部实例");
    }

    /// <summary>
    /// 设置腿部状态
    /// </summary>
    /// <param name="newState">新状态</param>
    public void SetState(LegsState newState)
    {
        if (currentState == newState) return;

        LegsState oldState = currentState;
        currentState = newState;

        // 更新动画
        UpdateAnimations();

        // 触发状态改变事件
        OnStateChanged?.Invoke(newState);

        Debug.Log($"LegsManager: 状态从 {oldState} 切换到 {newState}");
    }

    /// <summary>
    /// 更新动画状态
    /// </summary>
    private void UpdateAnimations()
    {
        // 重置所有动画参数
        ResetAnimationParameters();

        // 根据当前状态设置动画参数和排序层
        switch (currentState)
        {
            case LegsState.Moving:
                SetAnimationParameter(movingAnimationParam, true);
                SetLegsSortingLayer(itemMoveSortingLayer);
                ShowLegs(true);
                break;
            case LegsState.Dragging:
                SetAnimationParameter(draggingAnimationParam, true);
                SetLegsSortingLayer(dragSortingLayer);
                ShowLegs(true);
                break;
            case LegsState.Settled:
                SetLegsSortingLayer(itemSortingLayer);
                ShowLegs(false); // 隐藏腿部
                break;
        }
    }

    /// <summary>
    /// 重置所有动画参数
    /// </summary>
    private void ResetAnimationParameters()
    {
        SetAnimationParameter(movingAnimationParam, false);
        SetAnimationParameter(draggingAnimationParam, false);
        SetAnimationParameter(settledAnimationParam, false);
    }

    /// <summary>
    /// 设置动画参数
    /// </summary>
    /// <param name="paramName">参数名称</param>
    /// <param name="value">参数值</param>
    private void SetAnimationParameter(string paramName, bool value)
    {
        if (leftLegAnimator != null)
        {
            leftLegAnimator.SetBool(paramName, value);
        }
        if (rightLegAnimator != null)
        {
            rightLegAnimator.SetBool(paramName, value);
        }
    }

    /// <summary>
    /// 显示或隐藏腿部
    /// </summary>
    /// <param name="show">是否显示</param>
    public void ShowLegs(bool show)
    {
        if (leftLegInstance != null)
        {
            leftLegInstance.SetActive(show);
        }
        if (rightLegInstance != null)
        {
            rightLegInstance.SetActive(show);
        }

        Debug.Log($"LegsManager: {(show ? "显示" : "隐藏")}腿部");
    }

    /// <summary>
    /// 开始拖拽
    /// </summary>
    public void OnDragStart()
    {
        SetState(LegsState.Dragging);
    }

    /// <summary>
    /// 结束拖拽
    /// </summary>
    public void OnDragEnd()
    {
        // 根据物品状态决定切换到哪个状态
        if (draggableItem.IsSnapped)
        {
            SetState(LegsState.Settled);
        }
        else
        {
            SetState(LegsState.Moving);
        }
    }

    /// <summary>
    /// 物品放置完成
    /// </summary>
    public void OnItemSettled()
    {
        SetState(LegsState.Settled);
    }

    /// <summary>
    /// 物品重置（重新开始移动）
    /// </summary>
    public void OnItemReset()
    {
        SetState(LegsState.Moving);
    }

    /// <summary>
    /// 设置动画速度
    /// </summary>
    /// <param name="speed">动画速度倍数</param>
    public void SetAnimationSpeed(float speed)
    {
        if (leftLegAnimator != null)
        {
            leftLegAnimator.speed = speed;
        }
        if (rightLegAnimator != null)
        {
            rightLegAnimator.speed = speed;
        }
        Debug.Log($"LegsManager: 设置腿部动画速度为 {speed}");
    }

    /// <summary>
    /// 暂停动画
    /// </summary>
    public void PauseAnimation()
    {
        if (leftLegAnimator != null)
        {
            leftLegAnimator.enabled = false;
        }
        if (rightLegAnimator != null)
        {
            rightLegAnimator.enabled = false;
        }
    }

    /// <summary>
    /// 恢复动画
    /// </summary>
    public void ResumeAnimation()
    {
        if (leftLegAnimator != null)
        {
            leftLegAnimator.enabled = true;
        }
        if (rightLegAnimator != null)
        {
            rightLegAnimator.enabled = true;
        }
    }

    /// <summary>
    /// 获取当前状态
    /// </summary>
    public LegsState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// 销毁腿部实例
    /// </summary>
    public void DestroyLegs()
    {
        if (leftLegInstance != null)
        {
            Destroy(leftLegInstance);
            leftLegInstance = null;
            leftLegAnimator = null;
            leftLegRenderer = null;
        }
        if (rightLegInstance != null)
        {
            Destroy(rightLegInstance);
            rightLegInstance = null;
            rightLegAnimator = null;
            rightLegRenderer = null;
        }
        Debug.Log("LegsManager: 销毁腿部实例");
    }

    private void OnDestroy()
    {
        // 确保在销毁时清理腿部实例
        if (leftLegInstance != null)
        {
            Destroy(leftLegInstance);
        }
        if (rightLegInstance != null)
        {
            Destroy(rightLegInstance);
        }
    }

    /// <summary>
    /// 设置腿部排序层
    /// </summary>
    /// <param name="sortingLayer">排序层名称</param>
    private void SetLegsSortingLayer(string sortingLayer)
    {
        if (leftLegRenderer != null)
        {
            leftLegRenderer.sortingLayerName = sortingLayer;
            leftLegRenderer.sortingOrder = 100;
        }
        if (rightLegRenderer != null)
        {
            rightLegRenderer.sortingLayerName = sortingLayer;
            rightLegRenderer.sortingOrder = 100;
        }

        Debug.Log($"LegsManager: 设置腿部排序层为 {sortingLayer}");
    }

    /// <summary>
    /// 手动设置腿部排序层
    /// </summary>
    /// <param name="sortingLayer">排序层名称</param>
    public void SetLegsSortingLayerPublic(string sortingLayer)
    {
        SetLegsSortingLayer(sortingLayer);
    }
}