using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Cafe Simulator/Item")]
public abstract class Item : ScriptableObject
{
    [Header("Basic Item Information")]
    public string itemName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public ItemType itemType;
}
