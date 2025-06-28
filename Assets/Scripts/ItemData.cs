using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    public int itemId;
    public string itemName;
    public List<Vector2> correctPositions;
    public Quaternion rotation;
    public Vector3 scale;
    public Sprite itemSprite;
    public float moveSpeed;
    public float settleTime;


    [Header("前置条件")]
    public int prerequisiteItemId = -1; // 前置物品ID，-1表示无前置条件


    [Header("台词设置")]
    public string dialogue = ""; // 物品的台词
}