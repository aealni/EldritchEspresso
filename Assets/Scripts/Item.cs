using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Basic Item Information")]
    public string itemName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public ItemType itemType;
    
    [Header("Stacking Properties")]
    [SerializeField] protected int _maxStackSize = 1;
    
    /// <summary>
    /// Maximum number of identical items that can be stacked together
    /// </summary>
    public virtual int maxStackSize => _maxStackSize;
    
    /// <summary>
    /// Whether this item can be stacked with other identical items
    /// </summary>
    public bool isStackable => maxStackSize > 1;
    
    /// <summary>
    /// Compares base Item properties for exact matching
    /// </summary>
    /// <param name="other">Item to compare against</param>
    /// <returns>True if all base properties match exactly</returns>
    protected virtual bool CompareBaseProperties(Item other)
    {
        if (other == null) return false;
        
        return itemName == other.itemName &&
               description == other.description &&
               icon == other.icon &&
               itemType == other.itemType;
    }
    
    /// <summary>
    /// Utility method to compare two Item arrays for exact equality
    /// </summary>
    /// <param name="array1">First array to compare</param>
    /// <param name="array2">Second array to compare</param>
    /// <returns>True if arrays are identical (same items in same order)</returns>
    protected static bool ArraysEqual(Item[] array1, Item[] array2)
    {
        // Handle null arrays
        if (array1 == null && array2 == null) return true;
        if (array1 == null || array2 == null) return false;
        
        // Check lengths
        if (array1.Length != array2.Length) return false;
        
        // Compare each element
        for (int i = 0; i < array1.Length; i++)
        {
            if (array1[i] == null && array2[i] == null) continue;
            if (array1[i] == null || array2[i] == null) return false;
            if (!array1[i].CompareBaseProperties(array2[i])) return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Determines if this item can stack with another item.
    /// Items can only stack if ALL their properties match exactly.
    /// </summary>
    /// <param name="other">Item to compare against</param>
    /// <returns>True if items have identical properties and can be stacked</returns>
    public virtual bool CanStackWith(Item other)
    {
        if (other == null) return false;
        if (!isStackable || !other.isStackable) return false;
        if (GetType() != other.GetType()) return false;
        
        return CompareBaseProperties(other);
    }
}
