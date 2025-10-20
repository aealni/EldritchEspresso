using UnityEngine;

public class Ingredient : Item
{
    [Header("Ingredient Properties")]
    [Range(1, 64)]
    public int quantity = 1;
    
    /// <summary>
    /// Override maxStackSize for Ingredient items (default 64)
    /// </summary>
    public override int maxStackSize => 64;

    private void Awake()
    {
        itemType = ItemType.Ingredient;
        _maxStackSize = 64;
    }
    
    /// <summary>
    /// Determines if this Ingredient item can stack with another Ingredient item.
    /// All properties must match exactly for stacking to occur.
    /// </summary>
    /// <param name="other">Item to compare against</param>
    /// <returns>True if all Ingredient properties match exactly</returns>
    public override bool CanStackWith(Item other)
    {
        if (!base.CanStackWith(other)) return false;
        
        if (!(other is Ingredient otherIngredient)) return false;
        
        // Compare all Ingredient-specific properties
        return quantity == otherIngredient.quantity;
    }
}
