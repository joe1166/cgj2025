using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI管理器 - 管理物品的文本显示
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI设置")]
    public GameObject textPrefab; // 文本预制体（需要有TextMeshPro组件）
    public float textOffset = 0.1f; // 文本偏移倍数（相对于物品高度）

    [Header("渲染设置")]
    public string textSortingLayer = "UI"; // 文本的Sorting Layer名称
    public int textSortingOrder = 10; // 文本的Sorting Order

    private Dictionary<DraggableItem, TextMeshPro> itemTexts = new Dictionary<DraggableItem, TextMeshPro>();

    private void Update()
    {
        // 更新所有文本的位置
        UpdateTextPositions();
    }

    /// <summary>
    /// 为物品创建文本显示
    /// </summary>
    /// <param name="draggableItem">要添加文本的物品</param>
    /// <param name="initialText">初始文本</param>
    public void CreateItemText(DraggableItem draggableItem, string initialText = "")
    {
        if (textPrefab == null)
        {
            Debug.LogError("TextPrefab未设置！");
            return;
        }

        // 检查是否已经为该物品创建了文本
        if (itemTexts.ContainsKey(draggableItem))
        {
            Debug.LogWarning($"物品 {draggableItem.name} 已经有文本显示");
            return;
        }

        // 实例化文本预制体
        GameObject textInstance = Instantiate(textPrefab, transform);
        TextMeshPro textComponent = textInstance.GetComponent<TextMeshPro>();

        // 设置文本的Sort Layer，确保在物品上方显示
        MeshRenderer textRenderer = textComponent.GetComponent<MeshRenderer>();
        if (textRenderer != null)
        {
            // 使用配置的Sort Layer设置
            textRenderer.sortingLayerName = textSortingLayer;
            textRenderer.sortingOrder = textSortingOrder;
        }

        // 设置初始文本
        textComponent.text = initialText;

        // 存储引用
        itemTexts[draggableItem] = textComponent;

        Debug.Log($"为物品 {draggableItem.name} 创建了文本显示");
    }

    /// <summary>
    /// 更新物品的文本内容
    /// </summary>
    /// <param name="draggableItem">物品</param>
    /// <param name="newText">新文本</param>
    public void UpdateItemText(DraggableItem draggableItem, string newText)
    {
        if (itemTexts.ContainsKey(draggableItem))
        {
            itemTexts[draggableItem].text = newText;
        }
        else
        {
            Debug.LogWarning($"物品 {draggableItem.name} 没有文本显示，请先调用CreateItemText");
        }
    }

    /// <summary>
    /// 移除物品的文本显示
    /// </summary>
    /// <param name="draggableItem">物品</param>
    public void RemoveItemText(DraggableItem draggableItem)
    {
        if (itemTexts.ContainsKey(draggableItem))
        {
            if (itemTexts[draggableItem] != null)
            {
                Destroy(itemTexts[draggableItem].gameObject);
            }
            itemTexts.Remove(draggableItem);
            Debug.Log($"移除了物品 {draggableItem.name} 的文本显示");
        }
    }

    /// <summary>
    /// 更新所有文本的位置，使其跟随物品移动
    /// </summary>
    private void UpdateTextPositions()
    {
        foreach (var kvp in itemTexts)
        {
            DraggableItem item = kvp.Key;
            TextMeshPro text = kvp.Value;

            if (item != null && text != null)
            {
                // 计算基于物品高度的偏移
                Vector3 offset = CalculateTextOffset(item);

                // 设置文本位置
                text.transform.position = item.transform.position + offset;
            }
        }
    }

    /// <summary>
    /// 计算文本偏移量（基于物品的sprite高度）
    /// </summary>
    /// <param name="item">物品</param>
    /// <returns>偏移量</returns>
    private Vector3 CalculateTextOffset(DraggableItem item)
    {
        SpriteRenderer spriteRenderer = item.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            // 获取sprite的高度（考虑缩放）
            float spriteHeight = spriteRenderer.sprite.bounds.size.y * item.transform.localScale.y;

            // 计算偏移量
            float offsetY = spriteHeight + textOffset;

            return new Vector3(0, offsetY, 0);
        }

        // 如果没有sprite，使用默认偏移
        return new Vector3(0, 1, 0);
    }

    /// <summary>
    /// 清空所有文本显示
    /// </summary>
    public void ClearAllTexts()
    {
        foreach (var kvp in itemTexts)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value.gameObject);
            }
        }
        itemTexts.Clear();
        Debug.Log("清空了所有文本显示");
    }
}