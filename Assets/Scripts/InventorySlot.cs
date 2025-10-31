using UnityEngine;

/// <summary>
/// Represents a single slot in the inventory that can hold multiple identical items
/// </summary>
[System.Serializable]
public class InventorySlot
{
    [SerializeField] private Item itemType;
    [SerializeField] private int currentStack;
    [SerializeField] private int maxStackSize;
    [SerializeField] private bool isSelected = false;

    /// <summary>
    /// Creates an empty inventory slot
    /// </summary>
    public InventorySlot()
    {
        itemType = null;
        currentStack = 0;
        maxStackSize = 0;
    }

    /// <summary>
    /// Creates an inventory slot with an initial item
    /// </summary>
    /// <param name="item">Initial item to place in slot</param>
    /// <param name="quantity">Initial quantity</param>
    public InventorySlot(Item item, int quantity = 1)
    {
        if (item != null)
        {
            itemType = item;
            maxStackSize = item.maxStackSize;
            currentStack = Mathf.Clamp(quantity, 0, maxStackSize);
        }
        else
        {
            itemType = null;
            currentStack = 0;
            maxStackSize = 0;
        }
    }

    /// <summary>
    /// Gets the item type stored in this slot
    /// </summary>
    public Item ItemType => itemType;

    /// <summary>
    /// Gets the current quantity in this slot
    /// </summary>
    public int CurrentStack => currentStack;

    /// <summary>
    /// Gets the maximum stack size for this slot
    /// </summary>
    public int MaxStackSize => maxStackSize;

    /// <summary>
    /// Checks if this slot is empty
    /// </summary>
    /// <returns>True if slot contains no items</returns>
    public bool IsEmpty()
    {
        return itemType == null || currentStack <= 0;
    }

    /// <summary>
    /// Checks if this slot is at maximum capacity
    /// </summary>
    /// <returns>True if slot cannot hold more items</returns>
    public bool IsFull()
    {
        return currentStack >= maxStackSize && maxStackSize > 0;
    }

    /// <summary>
    /// Checks if the specified item can be added to this slot
    /// </summary>
    /// <param name="item">Item to check</param>
    /// <returns>True if item can be added</returns>
    public bool CanAddItem(Item item)
    {
        if (item == null) return false;
        
        // Empty slot can accept any stackable item
        if (IsEmpty()) return item.isStackable;
        
        // Non-empty slot can only accept identical items that can stack together
        return !IsFull() && itemType.CanStackWith(item);
    }

    /// <summary>
    /// Adds items to this slot
    /// </summary>
    /// <param name="item">Item to add</param>
    /// <param name="quantity">Quantity to add</param>
    /// <returns>Number of items that couldn't be added (overflow)</returns>
    public int AddItems(Item item, int quantity)
    {
        if (item == null || quantity <= 0) return quantity;

        // Handle empty slot
        if (IsEmpty())
        {
            if (!item.isStackable) return quantity; // Can't add non-stackable items
            
            itemType = item;
            maxStackSize = item.maxStackSize;
            int unstackedAdd = Mathf.Min(quantity, maxStackSize);
            currentStack = unstackedAdd;
            return quantity - unstackedAdd;
        }

        // Handle existing items
        if (!itemType.CanStackWith(item)) return quantity; // Items don't match

        int spaceAvailable = maxStackSize - currentStack;
        int stackedAdd = Mathf.Min(quantity, spaceAvailable);
        currentStack += stackedAdd;
        return quantity - stackedAdd;
    }

    /// <summary>
    /// Removes items from this slot
    /// </summary>
    /// <param name="quantity">Quantity to remove</param>
    /// <returns>Number of items actually removed</returns>
    public int RemoveItems(int quantity)
    {
        if (IsEmpty() || quantity <= 0) return 0;

        int canRemove = Mathf.Min(quantity, currentStack);
        currentStack -= canRemove;

        // Clear slot if empty
        if (currentStack <= 0)
        {
            itemType = null;
            maxStackSize = 0;
            currentStack = 0;
        }

        return canRemove;
    }

    /// <summary>
    /// Clears this slot completely
    /// </summary>
    public void Clear()
    {
        itemType = null;
        currentStack = 0;
        maxStackSize = 0;
    }

    /// <summary>
    /// Gets remaining space in this slot
    /// </summary>
    /// <returns>Number of items that can still be added</returns>
    public int GetRemainingSpace()
    {
        if (IsEmpty()) return 0; // Empty slots don't have predetermined space
        return Mathf.Max(0, maxStackSize - currentStack);
    }
    
    // Selection-related methods
    
    /// <summary>
    /// Gets whether this slot is currently selected
    /// </summary>
    /// <returns>True if slot is selected</returns>
    public bool IsSelected()
    {
        return isSelected;
    }
    
    /// <summary>
    /// Sets the selection state of this slot
    /// </summary>
    /// <param name="selected">Whether slot should be selected</param>
    public void SetSelected(bool selected)
    {
        if (isSelected != selected)
        {
            isSelected = selected;
            OnSelectionChanged?.Invoke(this);
        }
    }
    
    /// <summary>
    /// Event triggered when selection state changes
    /// </summary>
    public System.Action<InventorySlot> OnSelectionChanged;
    
    /// <summary>
    /// Gets a description of this slot for debugging
    /// </summary>
    /// <returns>String description of the slot</returns>
    public string GetDescription()
    {
        if (IsEmpty())
        {
            return $"Slot: Empty{(isSelected ? " [SELECTED]" : "")}";
        }
        else
        {
            string stackInfo = currentStack > 1 ? $" x{currentStack}" : "";
            return $"Slot: {itemType.itemName}{stackInfo}{(isSelected ? " [SELECTED]" : "")}";
        }
    }
}
