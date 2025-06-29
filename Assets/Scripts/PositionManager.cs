using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 位置管理器 - 管理关卡中所有位置的占用状态
/// 每个物品只能放在自己ItemData.correctPositions中的位置
/// 此管理器用于防止多个物品放置到同一位置
/// </summary>
public class PositionManager : MonoBehaviour
{
    [Header("关卡设置")]
    public List<Vector2> correctPositions = new List<Vector2>(); // 关卡中所有可能的正确位置（用于检测占用）

    [Header("物品设置")]
    public GameObject itemPrefab; // 物品预制体（需要有DraggableItem组件）
    public GameObject conditionalItemPrefab; // 条件物品预制体（需要有ConditionalDraggableItem组件）
    public GameObject shakableItemPrefab;
    public List<ItemData> levelItems = new List<ItemData>(); // 当前关卡会用到的所有ItemData
    public List<DraggableItem> levelItemsInstances = new List<DraggableItem>(); // 当前关卡所有Item(根据ItemData生成)

    private List<Vector2> occupiedPositions = new List<Vector2>(); // 已被占用的位置
    private float timer = 5f;
    // private int baseSortingOrder = 

    private void Start()
    {
        // 创建关卡中的所有物品
        CreateLevelItems();
    }

    public void HideAll()
    {
        PauseAll();
        foreach (var obj in levelItemsInstances)
        {
            obj.gameObject.SetActive(false);
        }
    }

    public void PauseAll()
    {
        foreach (var obj in levelItemsInstances)
        {
            obj.SetSortingLayer("BG");
            obj.IsPaused = true;
        }
    }

    public void UnPauseAll()
    {
        foreach (var obj in levelItemsInstances)
        {
            obj.IsPaused = false;
            // TODO 加判定
            if (obj.IsSnapped)
            {
                obj.SetSortingLayer(obj.itemSortingLayer);
            }
            else
            {
                obj.SetSortingLayer(obj.itemMoveSortingLayer);
            }
        }

    }

    /// <summary>
    /// 创建关卡中的所有物品
    /// </summary>
    private void CreateLevelItems()
    {
        if (itemPrefab == null)
        {
            Debug.LogError("ItemPrefab未设置！");
            return;
        }

        if (levelItems == null || levelItems.Count == 0)
        {
            Debug.LogWarning("LevelItems列表为空，没有物品需要创建");
            return;
        }

        var count = 0;
        foreach (ItemData itemData in levelItems)
        {
            if (itemData == null)
            {
                Debug.LogWarning("发现空的ItemData，跳过");
                continue;
            }

            GameObject itemInstance;
            if (itemData.prerequisiteItemId != -1)
            {
                // 实例化条件物品预制体
                itemInstance = Instantiate(conditionalItemPrefab, transform);
            }
            else if (itemData.needShake)
            {
                itemInstance = Instantiate(shakableItemPrefab, transform);
            }
            else
            {
                // 实例化物品预制体
                itemInstance = Instantiate(itemPrefab, transform);
            }

            // 改变物品渲染层数（保证物品和它的附件 如腿 不会出现随机遮挡）Add commentMore actions
            itemInstance.GetComponent<SpriteRenderer>().sortingOrder = count * 5;

            // 获取DraggableItem组件
            DraggableItem draggableItem = itemInstance.GetComponent<DraggableItem>();

            // 设置ItemData并初始化
            draggableItem.ItemData = itemData;
            draggableItem.Init(this);

            Debug.Log($"成功创建物品: {itemData.itemName}");
            levelItemsInstances.Add(draggableItem);
            count += 1;
        }

        Debug.Log($"关卡物品创建完成，共创建了 {levelItems.Count} 个物品");
    }

    /// <summary>
    /// 检查指定位置是否已被占用
    /// </summary>
    /// <param name="position">要检查的位置</param>
    /// <param name="snapRange">吸附范围</param>
    /// <returns>如果位置已被占用返回true，否则返回false</returns>
    public bool IsPositionOccupied(Vector2 position)
    {
        foreach (Vector2 occupiedPos in occupiedPositions)
        {
            if (Vector2.Distance(position, occupiedPos) <= 0.01f)
            {
                return true; // 位置已被占用
            }
        }
        return false; // 位置未被占用
    }

    /// <summary>
    /// 占用指定位置
    /// </summary>
    /// <param name="position">要占用的位置</param>
    /// <param name="snapRange">吸附范围</param>
    /// <returns>是否成功占用位置</returns>
    public bool OccupyPosition(Vector2 position)
    {
        foreach (Vector2 correctPos in correctPositions)
        {
            if (Vector2.Distance(position, correctPos) <= 0.01f)
            {
                // 检查该位置是否已被占用
                foreach (Vector2 occupiedPos in occupiedPositions)
                {
                    if (Vector2.Distance(correctPos, occupiedPos) <= 0.01f)
                    {
                        return false; // 位置已被占用
                    }
                }

                // 占用位置
                occupiedPositions.Add(correctPos);
                Debug.Log($"位置 {correctPos} 已放置，当前已放置物品数量: {occupiedPositions.Count}");
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 释放指定位置（不再被占用）
    /// </summary>
    /// <param name="position">要释放的位置</param>
    /// <returns>是否成功释放位置</returns>
    public bool ReleasePosition(Vector2 position)
    {
        // 检查该位置是否被占用
        for (int i = 0; i < occupiedPositions.Count; i++)
        {
            if (Vector2.Distance(position, occupiedPositions[i]) < 0.01f) // 使用很小的距离阈值
            {
                // 释放位置
                occupiedPositions.RemoveAt(i);
                Debug.Log($"位置 {position} 已释放，当前已放置物品数量: {occupiedPositions.Count}");
                return true;
            }
        }
        return false; // 位置本来就没有被占用
    }

    /// <summary>
    /// 检查关卡是否完成（所有位置都被占用）
    /// </summary>
    /// <returns>如果所有位置都被占用返回true</returns>
    public bool IsLevelComplete()
    {
        // 检查关卡中所有可能的正确位置是否都被占用
        return occupiedPositions.Count >= correctPositions.Count && correctPositions.Count > 0;
    }

    public virtual void CheckLevelCompletion()
    {
        // 检查关卡中所有可能的正确位置是否都被占用
        if (IsLevelComplete())
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
}