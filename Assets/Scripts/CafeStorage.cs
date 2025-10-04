using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New CafeStorage", menuName = "Cafe Simulator/CafeStorage")]
public class CafeStorage : ScriptableObject
{
    [Header("Storage Information")]
    [SerializeField] private string storageName;
    [SerializeField, TextArea(2, 4)] private string description;
    
    [Header("Storage Settings")]
    [SerializeField] private int maxCapacity = -1;
    
    [Header("Items")]
    [SerializeField] private List<Item> items = new List<Item>();
    
    private void OnValidate()
    {
        if (items == null)
            items = new List<Item>();
    }
    
    /// <summary>
    /// Adds an item to the storage with validation and capacity checks
    /// </summary>
    /// <param name="item">Item to add to storage</param>
    /// <returns>True if item was added successfully</returns>
    public bool AddItem(Item item)
    {
        if (item == null) return false;
        
        if (IsAtCapacity())
        {
            Debug.LogWarning($"Cannot add {item.itemName} to {storageName}: Storage is at capacity");
            return false;
        }
        
        items.Add(item);
        Debug.Log($"Added {item.itemName} to {storageName}");
        return true;
    }
    
    /// <summary>
    /// Removes a specific item from storage
    /// </summary>
    /// <param name="item">Item to remove</param>
    /// <returns>True if item was removed successfully</returns>
    public bool RemoveItem(Item item)
    {
        if (item == null) return false;
        
        bool removed = items.Remove(item);
        if (removed)
            Debug.Log($"Removed {item.itemName} from {storageName}");
        
        return removed;
    }
    
    /// <summary>
    /// Removes item at specific index
    /// </summary>
    /// <param name="index">Index of item to remove</param>
    /// <returns>True if item was removed successfully</returns>
    public bool RemoveItemAt(int index)
    {
        if (index < 0 || index >= items.Count) return false;
        
        Item item = items[index];
        items.RemoveAt(index);
        Debug.Log($"Removed {item.itemName} from {storageName} at index {index}");
        return true;
    }
    
    /// <summary>
    /// Gets item at specific index
    /// </summary>
    /// <param name="index">Index of item to retrieve</param>
    /// <returns>Item at index or null if index is invalid</returns>
    public Item GetItem(int index)
    {
        if (index < 0 || index >= items.Count) return null;
        return items[index];
    }
    
    /// <summary>
    /// Returns a copy of all items in storage
    /// </summary>
    /// <returns>New list containing all items</returns>
    public List<Item> GetAllItems()
    {
        return new List<Item>(items);
    }
    
    /// <summary>
    /// Gets the current number of items in storage
    /// </summary>
    /// <returns>Number of items currently stored</returns>
    public int GetItemCount()
    {
        return items.Count;
    }
    
    /// <summary>
    /// Checks if a specific item exists in storage
    /// </summary>
    /// <param name="item">Item to search for</param>
    /// <returns>True if item exists in storage</returns>
    public bool HasItem(Item item)
    {
        if (item == null) return false;
        return items.Contains(item);
    }
    
    /// <summary>
    /// Finds all items of a specific type
    /// </summary>
    /// <param name="type">ItemType to filter by</param>
    /// <returns>List of items matching the specified type</returns>
    public List<Item> FindItemsByType(ItemType type)
    {
        List<Item> result = new List<Item>();
        foreach (Item item in items)
        {
            if (item != null && item.itemType == type)
                result.Add(item);
        }
        return result;
    }
    
    /// <summary>
    /// Removes all items from storage
    /// </summary>
    public void ClearStorage()
    {
        items.Clear();
        Debug.Log($"Cleared all items from {storageName}");
    }
    
    /// <summary>
    /// Checks if storage is at maximum capacity
    /// </summary>
    /// <returns>True if storage is at capacity</returns>
    public bool IsAtCapacity()
    {
        if (maxCapacity <= 0) return false; // Unlimited capacity
        return items.Count >= maxCapacity;
    }
    
    /// <summary>
    /// Finds all Food items in storage
    /// </summary>
    /// <returns>List of Food items</returns>
    public List<Item> FindFoodItems()
    {
        List<Item> result = new List<Item>();
        foreach (Item item in items)
        {
            if (item != null && item.GetComponent<Food>() != null)
                result.Add(item);
        }
        return result;
    }
    
    /// <summary>
    /// Finds all Ingredient items in storage
    /// </summary>
    /// <returns>List of Ingredient items</returns>
    public List<Item> FindIngredientItems()
    {
        List<Item> result = new List<Item>();
        foreach (Item item in items)
        {
            if (item != null && item.GetComponent<Ingredient>() != null)
                result.Add(item);
        }
        return result;
    }
    
    /// <summary>
    /// Finds item by name
    /// </summary>
    /// <param name="name">Name to search for</param>
    /// <returns>First item with matching name or null if not found</returns>
    public Item FindItemByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        
        foreach (Item item in items)
        {
            if (item != null && item.itemName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                return item;
        }
        return null;
    }
    
    /// <summary>
    /// Gets formatted information about the storage
    /// </summary>
    /// <returns>String containing storage details</returns>
    public string GetStorageInfo()
    {
        string capacityInfo = maxCapacity > 0 ? $"{items.Count}/{maxCapacity}" : $"{items.Count}/∞";
        return $"Storage: {storageName}\nDescription: {description}\nCapacity: {capacityInfo}";
    }
    
    /// <summary>
    /// Sorts items alphabetically by name
    /// </summary>
    public void SortItemsByName()
    {
        items.Sort((a, b) => 
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;
            return string.Compare(a.itemName, b.itemName, System.StringComparison.OrdinalIgnoreCase);
        });
    }
    
    /// <summary>
    /// Sorts items by ItemType
    /// </summary>
    public void SortItemsByType()
    {
        items.Sort((a, b) => 
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;
            return a.itemType.CompareTo(b.itemType);
        });
    }
    
    /// <summary>
    /// Gets the quantity of a specific item in storage (exact parameter matching)
    /// </summary>
    /// <param name="item">Item to count</param>
    /// <returns>Total quantity of matching items</returns>
    public int GetItemQuantity(Item item)
    {
        if (item == null) return 0;
        
        int count = 0;
        foreach (Item storageItem in items)
        {
            if (storageItem != null && storageItem.CanStackWith(item))
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Removes a specific quantity of matching items from storage (exact parameter matching)
    /// </summary>
    /// <param name="item">Item type to remove</param>
    /// <param name="quantity">Quantity to remove</param>
    /// <returns>Number of items actually removed</returns>
    public int RemoveItem(Item item, int quantity)
    {
        if (item == null || quantity <= 0) return 0;
        
        int removed = 0;
        int remaining = quantity;
        
        for (int i = items.Count - 1; i >= 0 && remaining > 0; i--)
        {
            if (items[i] != null && items[i].CanStackWith(item))
            {
                items.RemoveAt(i);
                removed++;
                remaining--;
            }
        }
        
        if (removed > 0)
        {
            Debug.Log($"Removed {removed} {item.itemName} from {storageName}");
        }
        
        return removed;
    }
    
    /// <summary>
    /// Transfers items from this storage to a player inventory with stacking support
    /// </summary>
    /// <param name="item">Item type to transfer (used for exact matching)</param>
    /// <param name="inventory">Target inventory</param>
    /// <param name="quantity">Quantity to transfer (default: 1)</param>
    /// <returns>Result of the transfer operation</returns>
    public TransferResult TransferToInventory(Item item, Inventory inventory, int quantity = 1)
    {
        // Validate parameters
        if (item == null || inventory == null)
        {
            Debug.LogWarning("TransferToInventory: Invalid parameters (null item or inventory)");
            return TransferResult.InvalidParameters;
        }
        
        if (quantity <= 0)
        {
            Debug.LogWarning("TransferToInventory: Invalid quantity");
            return TransferResult.InvalidParameters;
        }
        
        // Check if we have the requested items in storage
        int availableQuantity = GetItemQuantity(item);
        if (availableQuantity == 0)
        {
            Debug.LogWarning($"TransferToInventory: {item.itemName} not found in {storageName}");
            return TransferResult.ItemNotFound;
        }
        
        if (availableQuantity < quantity)
        {
            Debug.LogWarning($"TransferToInventory: Insufficient quantity. Requested: {quantity}, Available: {availableQuantity}");
            return TransferResult.InsufficientQuantity;
        }
        
        // Check if inventory can accept the items
        if (!inventory.CanAddItem(item, quantity))
        {
            Debug.LogWarning($"TransferToInventory: Inventory cannot accept {quantity} {item.itemName}");
            return TransferResult.InventoryFull;
        }
        
        // Execute the transfer
        int removedFromStorage = RemoveItem(item, quantity);
        int addedToInventory = inventory.AddItem(item, removedFromStorage);
        
        // Handle transfer results
        if (addedToInventory == removedFromStorage)
        {
            Debug.Log($"Successfully transferred {addedToInventory} {item.itemName} from {storageName} to inventory");
            return TransferResult.Success;
        }
        else if (addedToInventory > 0)
        {
            // Partial transfer - add back the items that couldn't be transferred
            int leftover = removedFromStorage - addedToInventory;
            for (int i = 0; i < leftover; i++)
            {
                AddItem(item); // Add back the items we couldn't transfer
            }
            Debug.LogWarning($"Partial transfer: {addedToInventory}/{quantity} {item.itemName} transferred");
            return TransferResult.PartialSuccess;
        }
        else
        {
            // Failed transfer - add back all items
            for (int i = 0; i < removedFromStorage; i++)
            {
                AddItem(item);
            }
            Debug.LogError($"Transfer failed: Could not add {item.itemName} to inventory");
            return TransferResult.InventoryFull;
        }
    }
}
