using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemData ItemData; // 物品数据ScriptableObject
    public float SnapRange = 0.5f; // 吸附范围
    public bool IsSnapped = false;  // 正确吸附属性

    private bool _isDragging = false;
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
        }
        else
        {
            Debug.LogError("error: ItemData is null");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag called!");
        // 防止二次拖拽
        if (_isDragging) return;
        _isDragging = true;

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
        _isDragging = false;
        // 判断是否在吸附范围内
        if (ItemData != null && Vector2.Distance(transform.position, ItemData.correctPosition) <= SnapRange)
        {
            transform.position = ItemData.correctPosition;
            IsSnapped = true;
        }
        else
        {
            IsSnapped = false;
        }
    }
}
