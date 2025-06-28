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
}