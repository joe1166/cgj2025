using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemData ItemData; // 物品数据ScriptableObject
    public float SnapRange = 0.5f; // 吸附范围
    public bool IsSnapped = false;  // 正确吸附属性
    public bool IsDragging = false;

    [Header("台词设置")]
    public float minDialogueInterval = 5f; // 最小台词间隔时间
    public float maxDialogueInterval = 15f; // 最大台词间隔时间
    public float dialogueDisplayTime = 3f; // 台词显示时间

    [Header("排序层设置")]
    public string dragSortingLayer = "Drag"; // 拖拽时的排序层
    public string itemSortingLayer = "Item"; // 放置后的排序层
    public string itemMoveSortingLayer = "ItemMove"; // 移动时的排序层

    private Vector3 _offset;
    private float dialogueTimer = 0f; // 台词计时器
    private float nextDialogueTime = 0f; // 下次说台词的时间
    private bool isShowingDialogue = false; // 是否正在显示台词
    private SpriteRenderer spriteRenderer; // 缓存SpriteRenderer组件
    private LegsManager legsManager; // 腿部管理器

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

        // 创建文本显示
        CreateItemText();

        GetComponent<MovableItem>().Init();

        // 设置初始台词时间
        if (!string.IsNullOrEmpty(ItemData.dialogue))
        {
            float randomInterval = Random.Range(minDialogueInterval, maxDialogueInterval);
            nextDialogueTime = Time.time + randomInterval;
        }

        // 设置初始排序层为移动层
        SetSortingLayer(itemMoveSortingLayer);

        // 获取腿部管理器
        legsManager = GetComponent<LegsManager>();
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

    /// <summary>
    /// 为物品创建文本显示
    /// </summary>
    private void CreateItemText()
    {
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.CreateItemText(this, "");
        }
        else
        {
            Debug.LogWarning("场景中没有找到UIManager，无法创建文本显示");
        }
    }

    /// <summary>
    /// 更新物品的文本显示
    /// </summary>
    /// <param name="newText">新文本</param>
    public void UpdateText(string newText)
    {
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.UpdateItemText(this, newText);
        }
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        // Debug.Log("OnBeginDrag called!");
        // 防止二次拖拽或已吸附物品的拖拽
        if (IsDragging || IsSnapped) return;
        IsDragging = true;

        // 切换到拖拽层
        SetSortingLayer(dragSortingLayer);

        // 通知腿部管理器开始拖拽
        legsManager.OnDragStart();

        _offset = transform.position - Camera.main.ScreenToWorldPoint(eventData.position);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        // Debug.Log("OnDrag called!");
        Vector3 cursorPoint = Camera.main.ScreenToWorldPoint(eventData.position);
        cursorPoint.z = 1;
        transform.position = cursorPoint + _offset;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        // Debug.Log("OnEndDrag called!");
        IsDragging = false;

        // 通知腿部管理器结束拖拽
        legsManager.OnDragEnd();

        // 获取位置管理器
        PositionManager positionManager = FindObjectOfType<PositionManager>();
        if (positionManager == null)
        {
            Debug.LogError("找不到PositionManager！");
            return;
        }

        // 检查物品是否在自己的正确位置范围内
        if (ItemData != null && ItemData.correctPositions != null && ItemData.correctPositions.Count > 0)
        {
            Vector2 closestPosition = Vector2.zero;
            float closestDistance = float.MaxValue;
            bool foundValidPosition = false;

            // 遍历物品自己的正确位置
            foreach (Vector2 correctPos in ItemData.correctPositions)
            {
                float distance = Vector2.Distance(transform.position, correctPos);
                if (distance <= SnapRange && distance < closestDistance)
                {
                    // 检查该位置是否已被占用
                    if (!positionManager.IsPositionOccupied(correctPos, SnapRange))
                    {
                        closestPosition = correctPos;
                        closestDistance = distance;
                        foundValidPosition = true;
                    }
                }
            }

            if (foundValidPosition)
            {
                // 占用该位置
                if (positionManager.OccupyPosition(closestPosition, SnapRange))
                {
                    transform.position = closestPosition;
                    IsSnapped = true;
                    if (foundValidPosition)
                    {
                        // 占用该位置
                        if (positionManager.OccupyPosition(closestPosition, SnapRange))
                        {
                            transform.position = closestPosition;
                            IsSnapped = true;

                            // 切换到物品层
                            SetSortingLayer(itemSortingLayer);

                            // 禁用碰撞器，防止拖拽
                            var collider = GetComponent<Collider2D>();
                            if (collider != null)
                            {
                                collider.enabled = false;
                            }

                            GetComponent<MovableItem>().Settle();

                            // 通知腿部管理器物品已放置
                            legsManager.OnItemSettled();

                            Debug.Log($"物品 {ItemData.itemName} 成功放置到位置 {closestPosition}");

                            // 检查关卡是否完成
                            if (positionManager.IsLevelComplete())
                            {
                                Debug.Log("关卡完成！");
                                // 通知关卡控制器
                                LevelController levelController = FindObjectOfType<LevelController>();
                                if (levelController != null)
                                {
                                    levelController.CompleteLevel();
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("位置已被占用，无法放置");
                            IsSnapped = false;
                            SetSortingLayer(itemMoveSortingLayer);
                        }
                    }
                    else
                    {
                        Debug.Log("不在物品的正确位置范围内或所有正确位置已被占用");
                        IsSnapped = false;
                        SetSortingLayer(itemMoveSortingLayer);
                    }
                }
                else
                {
                    Debug.LogError("ItemData或correctPositions为空！");
                    IsSnapped = false;
                    SetSortingLayer(itemMoveSortingLayer);
                }
            }
        }
    }

    public virtual bool settleConditionHook()
    {
        return true;
    }

    private void Update()
    {
        // 处理台词显示逻辑
        HandleDialogue();

        UpdateAfterHook();
    }

    public virtual void UpdateAfterHook()
    {
        return;
    }

    /// <summary>
    /// 处理台词显示逻辑
    /// </summary>
    private void HandleDialogue()
    {
        // 如果物品没有台词，直接返回
        if (string.IsNullOrEmpty(ItemData?.dialogue))
            return;

        // 如果正在显示台词
        if (isShowingDialogue)
        {
            dialogueTimer += Time.deltaTime;
            if (dialogueTimer >= dialogueDisplayTime)
            {
                // 台词显示时间结束，清空文本
                ClearDialogue();
            }
        }
        else
        {
            // 检查是否到了说台词的时间
            if (Time.time >= nextDialogueTime)
            {
                // 随机说台词
                ShowRandomDialogue();
            }
        }
    }

    public virtual bool CanMove()
    {
        bool unmovableConditions = (IsDragging || IsSnapped);
        bool canMove = !unmovableConditions;
        return canMove;
    }

    /// <summary>
    /// 显示随机台词
    /// </summary>
    protected void ShowRandomDialogue()
    {
        if (ItemData != null && !string.IsNullOrEmpty(ItemData.dialogue))
        {
            // 显示台词
            UpdateText(ItemData.dialogue);
            isShowingDialogue = true;
            dialogueTimer = 0f;

            // 设置下次说台词的时间
            float randomInterval = Random.Range(minDialogueInterval, maxDialogueInterval);
            nextDialogueTime = Time.time + randomInterval;

            Debug.Log($"物品 {ItemData.itemName} 说: {ItemData.dialogue}");
        }
    }

    /// <summary>
    /// 清空台词显示
    /// </summary>
    protected void ClearDialogue()
    {
        UpdateText("");

        isShowingDialogue = false;
        dialogueTimer = 0f;
    }

    /// <summary>
    /// 设置排序层
    /// </summary>
    /// <param name="sortingLayer">排序层名称</param>
    private void SetSortingLayer(string sortingLayer)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = sortingLayer;
            Debug.Log($"物品 {ItemData?.itemName} 切换到排序层: {sortingLayer}");
        }
    }

    /// <summary>
    /// 重置吸附状态（用于外部调用）
    /// </summary>
    public void ResetSnapState()
    {
        IsSnapped = false;
        IsDragging = false;

        // 切换到移动层

        SetSortingLayer(itemMoveSortingLayer);

        // 确保腿部动画正常运行
        legsManager.OnItemReset();

        // 重新启用碰撞器，允许拖拽

        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }
}
