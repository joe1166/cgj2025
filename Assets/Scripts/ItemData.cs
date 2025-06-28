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
    public float moveSpeed = 2f;
    public float settleTime = 20f;
    public float SnapRange = 0.5f;


    [Header("前置条件")]
    public int prerequisiteItemId = -1; // 前置物品ID，-1表示无前置条件
    public bool needShake = false;


    [Header("台词设置")]
    public string dialogue = ""; // 物品的台词

    [Header("腿设置")]
    public Vector2 leftLegPosition = Vector2.zero; // 左腿位置
    public Vector2 rightLegPosition = Vector2.zero; // 右腿位置
}