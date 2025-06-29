using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// 条件拖拽物品 - 需要满足前置条件才能拖拽
/// </summary>
public class ConditionalDraggableItem : DraggableItem
{
    /// <summary>
    /// 检查前置条件是否满足
    /// </summary>
    /// <returns>如果满足前置条件返回true，否则返回false</returns>
    private bool CheckPrerequisite()
    {
        // 如果没有前置条件，直接返回true
        if (ItemData.prerequisiteItemId == -1)
        {
            return true;
        }

        // 查找所有DraggableItem
        DraggableItem[] allItems = FindObjectsOfType<DraggableItem>();

        foreach (DraggableItem item in allItems)
        {
            // 检查是否是前置物品
            if (item.ItemData != null && item.ItemData.itemId == ItemData.prerequisiteItemId && item.IsSnapped)
            {
                Debug.Log($"前置条件满足: 物品 {item.ItemData.itemName} (ID: {item.ItemData.itemId}) 已放置");
                return true;
            }
        }

        Debug.LogWarning($"ID为 {ItemData.prerequisiteItemId} 的前置物品未放置");
        return false;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        // 检查前置条件
        if (!CheckPrerequisite())
        {
            ShowRandomDialogue();
            return;
        }

        // 调用父类的拖拽开始方法
        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        // 如果前置条件不满足，不允许拖拽
        if (!CheckPrerequisite())
        {
            AudioManager.Instance.PlaySFX("ItemPlaceFail");
            return;
        }

        // 调用父类的拖拽方法
        base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        // 如果前置条件不满足，不允许结束拖拽
        if (!CheckPrerequisite())
        {
            return;
        }

        // 调用父类的拖拽结束方法
        base.OnEndDrag(eventData);
    }
}