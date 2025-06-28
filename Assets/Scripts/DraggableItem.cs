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

    private Vector3 _offset;

    public void Init()
    {
        var sr = GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            Debug.LogError($"错误: GameObject {gameObject.name} 没有SpriteRenderer组件");
        }
        else if (ItemData == null)
        {
            Debug.LogError($"错误: GameObject {gameObject.name} 的ItemData为空");
        }
        else if (ItemData.itemSprite == null)
        {
            Debug.LogError($"错误: ItemData {ItemData.name} 的itemSprite为空");
        }
        else
        {
            sr.sprite = ItemData.itemSprite;

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

            // 根据图片轮廓自动设置多边形碰撞器
            SetupPolygonCollider();

            GetComponent<MovableItem>().Init();
        }


    }

    private void SetupPolygonCollider()
    {
        // 添加多边形碰撞器
        var polygonCollider = gameObject.AddComponent<PolygonCollider2D>();

        // 获取sprite的轮廓路径

        var sprite = GetComponent<SpriteRenderer>().sprite;
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag called!");
        // 防止二次拖拽
        if (IsDragging || IsSnapped) return;
        IsDragging = true;

        _offset = transform.position - Camera.main.ScreenToWorldPoint(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag called!");
        Vector3 cursorPoint = Camera.main.ScreenToWorldPoint(eventData.position);
        cursorPoint.z = 1;
        transform.position = cursorPoint + _offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag called!");
        IsDragging = false;

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
                    GetComponent<MovableItem>().Settle();


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
                }
            }
            else
            {
                Debug.Log("不在物品的正确位置范围内或所有正确位置已被占用");
                IsSnapped = false;
            }
        }
        else
        {
            Debug.LogError("ItemData或correctPositions为空！");
            IsSnapped = false;
        }
    }
}
