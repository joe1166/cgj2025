using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    public int itemId;
    public string itemName;
    public Vector2 correctPosition;
    public Sprite itemSprite;
    public float moveSpeed;
    public float settleTime;
}