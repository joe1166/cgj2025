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


    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        var sr = GetComponent<SpriteRenderer>();

        if (sr != null && ItemData != null && ItemData.itemSprite != null)
        {
            sr.sprite = ItemData.itemSprite;

            // 根据图片轮廓自动设置多边形碰撞器
            SetupPolygonCollider();
        }
        else
        {
            Debug.LogError("error: ItemData is null");
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
        // 判断是否在吸附范围内
        if (ItemData != null && Vector2.Distance(transform.position, ItemData.correctPosition) <= SnapRange)
        {
            transform.position = ItemData.correctPosition;
            IsSnapped = true;
            GetComponent<SimpleMover>().Settle();
        }
        else
        {
            IsSnapped = false;
        }
    }
}
