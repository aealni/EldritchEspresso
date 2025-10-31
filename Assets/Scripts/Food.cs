using UnityEngine;

public class Food : Item
{
    [Header("Food Properties")]
    [Range(1, 1000)]
    public int cost = 5;
    [Range(0f, 1f)]
    public float satisfaction = 0.5f;
    public Item[] ingredients_used;
    public bool isDeadly = false;
    public bool eldritchPreferred = false;
    
    /// <summary>
    /// Override maxStackSize for Food items (default 10)
    /// </summary>
    public override int maxStackSize => 10;

    private void Awake()
    {
        itemType = ItemType.Food;
        _maxStackSize = 1;
    }
    
    /// <summary>
    /// Determines if this Food item can stack with another Food item.
    /// All properties must match exactly for stacking to occur.
    /// </summary>
    /// <param name="other">Item to compare against</param>
    /// <returns>True if all Food properties match exactly</returns>
    public override bool CanStackWith(Item other)
    {
        if (!base.CanStackWith(other)) return false;
        
        if (!(other is Food otherFood)) return false;
        
        // Compare all Food-specific properties with exact matching
        const float epsilon = 0.0001f; // For float comparison
        
        return cost == otherFood.cost &&
               Mathf.Abs(satisfaction - otherFood.satisfaction) < epsilon &&
               isDeadly == otherFood.isDeadly &&
               eldritchPreferred == otherFood.eldritchPreferred &&
               ArraysEqual(ingredients_used, otherFood.ingredients_used);
    }
}
