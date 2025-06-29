using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(SpriteRenderer))]
public class Level0Item : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemData ItemData; // 物品数据ScriptableObject
    public float SnapRange = 0.5f; // 吸附范围
    public bool IsSnapped = false;  // 正确吸附属性
    public bool IsDragging = false;

    [Header("按钮设置")]
    public Button targetButton; // 要显示的按钮
    public float buttonAnimationDuration = 1f; // 按钮动画持续时间

    [Header("移动设置")]
    private float moveSpeed = 2f;
    private Vector2 moveDirection;
    private float settleTimer = 0;
    private float changeDirTimer;
    private float minChangeDirTime = 1f;
    private float maxChangeDirTime = 3f;

    private Vector3 _offset;
    private SpriteRenderer spriteRenderer; // 缓存SpriteRenderer组件
    private Vector2 buttonOriginalPosition; // 按钮原始位置（UI坐标）

    // 碰撞检测相关
    public float LeftExtent { get; private set; }
    public float RightExtent { get; private set; }
    public float TopExtent { get; private set; }
    public float BottomExtent { get; private set; }

    public void Start()
    {
        // 记录按钮原始位置（UI坐标）
        if (targetButton != null)
        {
            RectTransform buttonRect = targetButton.GetComponent<RectTransform>();
            buttonOriginalPosition = buttonRect.anchoredPosition;
            // 初始时隐藏按钮
            targetButton.gameObject.SetActive(false);
        }

        Init();
    }

    public virtual void Init()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError($"错误: GameObject {gameObject.name} 没有SpriteRenderer组件");
            return;
        }

        if (ItemData == null)
        {
            Debug.LogError($"错误: GameObject {gameObject.name} 的ItemData为空");
            return;
        }

        if (ItemData.itemSprite == null)
        {
            Debug.LogError($"错误: ItemData {ItemData.name} 的itemSprite为空");
            return;
        }

        // 设置sprite
        spriteRenderer.sprite = ItemData.itemSprite;

        // 应用ItemData中的旋转和缩放
        if (ItemData.rotation != Quaternion.identity)
        {
            transform.rotation = ItemData.rotation;
        }
        if (ItemData.scale != Vector3.one)
        {
            transform.localScale = ItemData.scale;
        }

        transform.position = ItemData.correctPositions[0];
        SnapRange = ItemData.SnapRange;

        // 根据图片轮廓自动设置多边形碰撞器
        SetupPolygonCollider();

        // 初始化移动相关参数
        InitMovement();
    }

    /// <summary>
    /// 初始化移动相关参数
    /// </summary>
    private void InitMovement()
    {
        moveSpeed = ItemData.moveSpeed;

        // 计算碰撞检测变量
        var bounds = GetComponent<Collider2D>().bounds;
        Vector2 center = bounds.center;
        LeftExtent = center.x - bounds.min.x;
        RightExtent = bounds.max.x - center.x;
        TopExtent = center.y - bounds.min.y;
        BottomExtent = bounds.max.y - center.y;

        PickNewDirection();
    }

    void Update()
    {
        // 处理移动逻辑
        HandleMovement();

        // 处理settle逻辑
        HandleSettle();
    }

    /// <summary>
    /// 处理移动逻辑
    /// </summary>
    private void HandleMovement()
    {
        if (CanMove())
        {
            // 移动
            Vector2 newPos = moveDirection * moveSpeed * Time.deltaTime;
            transform.Translate(newPos);

            // 碰撞检测
            HandleScreenEdgeBounce();

            // 改变移动方向
            changeDirTimer -= Time.deltaTime;
            if (changeDirTimer <= 0)
            {
                PickNewDirection();
            }
        }
    }

    /// <summary>
    /// 处理settle逻辑
    /// </summary>
    private void HandleSettle()
    {
        if (IsSettling())
        {
            settleTimer = Math.Max(settleTimer - Time.deltaTime, 0);

            // 当settle time归零时，重置状态
            if (settleTimer <= 0)
            {
                ReleasePosition();
            }
        }
    }

    /// <summary>
    /// 检查是否可以移动
    /// </summary>
    public virtual bool CanMove()
    {
        bool unmovableConditions = (IsDragging || IsSnapped);
        bool canMove = !unmovableConditions;
        return canMove;
    }

    /// <summary>
    /// 检查屏幕边界碰撞
    /// </summary>
    public List<bool> CheckScreenEdgeBounce()
    {
        // 先获取边界位置（世界坐标）
        Vector3 screenMin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, transform.position.z));
        Vector3 screenMax = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, transform.position.z));

        Vector3 pos = transform.position;

        List<bool> bounced = new List<bool> { false, false, false, false };
        if (pos.x - LeftExtent <= screenMin.x)
        {
            bounced[0] = true;
        }
        if (pos.x + RightExtent >= screenMax.x)
        {
            bounced[1] = true;
        }
        if (pos.y - BottomExtent <= screenMin.y)
        {
            bounced[2] = true;
        }
        if (pos.y + TopExtent >= screenMax.y)
        {
            bounced[3] = true;
        }

        return bounced;
    }

    /// <summary>
    /// 处理屏幕边界碰撞并反弹
    /// </summary>
    void HandleScreenEdgeBounce()
    {
        bool bounced = false;
        List<bool> bouncedCheck = CheckScreenEdgeBounce();
        if (bouncedCheck[0] || bouncedCheck[1])
        {
            moveDirection.x = -moveDirection.x;
            bounced = true;
        }
        if (bouncedCheck[2] || bouncedCheck[3])
        {
            moveDirection.y = -moveDirection.y;
            bounced = true;
        }

        if (bounced)
        {
            moveDirection = moveDirection.normalized;
            changeDirTimer = UnityEngine.Random.Range(minChangeDirTime, maxChangeDirTime);
        }
    }

    /// <summary>
    /// 选择新的移动方向
    /// </summary>
    void PickNewDirection()
    {
        float angle = UnityEngine.Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        changeDirTimer = UnityEngine.Random.Range(minChangeDirTime, maxChangeDirTime);
    }

    /// <summary>
    /// 开始settle状态
    /// </summary>
    public void Settle()
    {
        settleTimer = ItemData.settleTime;
    }

    /// <summary>
    /// 检查是否在settling状态
    /// </summary>
    bool IsSettling()
    {
        return (settleTimer > 0);
    }

    /// <summary>
    /// 释放位置并重置物品状态
    /// </summary>
    private void ReleasePosition()
    {
        // 重置IsSnapped状态
        ResetSnapState();

        Debug.Log($"物品 {ItemData?.itemName} 已释放位置，重新开始移动");
    }

    private void SetupPolygonCollider()
    {
        // 添加多边形碰撞器
        var polygonCollider = gameObject.AddComponent<PolygonCollider2D>();

        // 获取sprite的轮廓路径
        var sprite = spriteRenderer.sprite;
        if (sprite != null)
        {
            // 设置多边形碰撞器的路径为sprite的轮廓
            polygonCollider.pathCount = sprite.GetPhysicsShapeCount();

            for (int i = 0; i < polygonCollider.pathCount; i++)
            {
                var points = new List<Vector2>();
                sprite.GetPhysicsShape(i, points);
                polygonCollider.SetPath(i, points.ToArray());
            }
        }
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag called!");
        // 防止二次拖拽或已吸附物品的拖拽
        if (IsDragging || IsSnapped) return;

        IsDragging = true;
        _offset = transform.position - Camera.main.ScreenToWorldPoint(eventData.position);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        Vector3 cursorPoint = Camera.main.ScreenToWorldPoint(eventData.position);
        cursorPoint.z = transform.position.z;
        transform.position = cursorPoint + _offset;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag called!");
        IsDragging = false;

        // 检查物品是否在正确位置范围内
        if (ItemData != null && ItemData.correctPositions != null && ItemData.correctPositions.Count > 0)
        {
            bool foundCorrectPosition = false;

            // 遍历物品的正确位置
            foreach (Vector2 correctPos in ItemData.correctPositions)
            {
                float distance = Vector2.Distance(transform.position, correctPos);
                if (distance <= SnapRange)
                {
                    // 吸附到正确位置
                    transform.position = correctPos;
                    IsSnapped = true;
                    foundCorrectPosition = true;

                    // 禁用碰撞器，防止继续拖拽
                    var collider = GetComponent<Collider2D>();
                    if (collider != null)
                    {
                        collider.enabled = false;
                    }

                    Debug.Log($"物品 {ItemData.itemName} 成功放置到正确位置");

                    // 开始settle倒计时
                    Settle();

                    // 显示按钮动画
                    ShowButtonWithAnimation();

                    break;
                }
            }

            if (!foundCorrectPosition)
            {
                Debug.Log("物品未放置在正确位置");
                IsSnapped = false;
            }
        }
        else
        {
            Debug.LogError("ItemData或correctPositions为空！");
            IsSnapped = false;
        }
    }

    /// <summary>
    /// 显示按钮并播放从左侧滑入的动画
    /// </summary>
    private void ShowButtonWithAnimation()
    {
        if (targetButton == null) return;

        // 激活按钮
        targetButton.gameObject.SetActive(true);

        // 将按钮移动到屏幕左侧外面（原位置往左2000像素）
        RectTransform buttonRect = targetButton.GetComponent<RectTransform>();
        Vector2 leftOffscreenPosition = buttonOriginalPosition;
        leftOffscreenPosition.x = buttonOriginalPosition.x - 1000f; // 直接使用像素偏移
        buttonRect.anchoredPosition = leftOffscreenPosition;

        // 启动协程播放滑入动画
        StartCoroutine(SlideButtonIn());
    }

    /// <summary>
    /// 按钮滑入动画协程
    /// </summary>
    private IEnumerator SlideButtonIn()
    {
        float elapsedTime = 0f;
        RectTransform buttonRect = targetButton.GetComponent<RectTransform>();
        Vector2 startPosition = buttonRect.anchoredPosition;

        while (elapsedTime < buttonAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / buttonAnimationDuration;

            // 使用缓动函数让动画更平滑

            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);


            buttonRect.anchoredPosition = Vector2.Lerp(startPosition, buttonOriginalPosition, smoothProgress);
            yield return null;
        }

        // 确保按钮最终位置准确
        buttonRect.anchoredPosition = buttonOriginalPosition;
    }

    /// <summary>
    /// 重置吸附状态（用于外部调用）
    /// </summary>
    public void ResetSnapState()
    {
        IsSnapped = false;
        IsDragging = false;

        // 重新启用碰撞器，允许拖拽
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }
}
