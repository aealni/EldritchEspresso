using UnityEngine;

[CreateAssetMenu(fileName = "New Food", menuName = "Cafe Simulator/Food")]
public class Food : Item
{
    [Header("Food Properties")]
    [Range(1, 1000)]
    public int cost = 5;
    [Range(0f, 1f)]
    public float satisfaction = 0.5f;

    private void OnValidate()
    {
        itemType = ItemType.Food;
    }
}
