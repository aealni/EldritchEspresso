using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Cafe Simulator/Ingredient")]
public class Ingredient : Item
{
    [Header("Ingredient Properties")]
    [Range(1, 64)]
    public int quantity = 1;
    [Range(0f, 1f)]
    public bool isStrange = false;
    public bool isDeadly = false;

    private void OnValidate()
    {
        itemType = ItemType.Ingredient;
    }
}
