using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Player Inventory - 3 Slots")]
    [SerializeField] private InventorySlot[] slots = new InventorySlot[3];
    public Sprite texture;

    private void Awake()
    {
        // Initialize all inventory slots
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new InventorySlot();
        }
    }

    /// <summary>
    /// Adds an item to the inventory using stacking logic with exact parameter matching
    /// </summary>
    /// <param name="item">Item to add</param>
    /// <returns>True if item was added successfully</returns>
    public bool AddItem(Item item)
    {
        return AddItem(item, 1) > 0;
    }

    /// <summary>
    /// Adds a specific quantity of items to the inventory using stacking logic
    /// </summary>
    /// <param name="item">Item to add</param>
    /// <param name="quantity">Quantity to add</param>
    /// <returns>Number of items successfully added</returns>
    public int AddItem(Item item, int quantity)
    {
        if (item == null || quantity <= 0) return 0;

        int totalAdded = 0;
        int remaining = quantity;

        // First, try to add to existing stackable slots
        int stackableSlot = FindStackableSlot(item);
        while (stackableSlot >= 0 && remaining > 0)
        {
            int overflow = slots[stackableSlot].AddItems(item, remaining);
            int added = remaining - overflow;
            totalAdded += added;
            remaining = overflow;

            if (remaining > 0)
            {
                stackableSlot = FindStackableSlot(item);
            }
            else
            {
                break;
            }
        }

        // If there are still items to add, try empty slots
        while (remaining > 0)
        {
            int emptySlot = GetEmptySlotIndex();
            if (emptySlot < 0) break; // No more empty slots

            int overflow = slots[emptySlot].AddItems(item, remaining);
            int added = remaining - overflow;
            totalAdded += added;
            remaining = overflow;
        }

        if (totalAdded > 0)
        {
            Debug.Log($"Added {totalAdded} {item.itemName} to inventory");
        }
        if (remaining > 0)
        {
            Debug.Log($"Could not add {remaining} {item.itemName} - inventory full");
        }

        return totalAdded;
    }

    /// <summary>
    /// Finds a slot that can accept more of the specified item (exact parameter matching)
    /// </summary>
    /// <param name="item">Item to find stackable slot for</param>
    /// <returns>Index of stackable slot, or -1 if none found</returns>
    public int FindStackableSlot(Item item)
    {
        if (item == null || !item.isStackable) return -1;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].CanAddItem(item))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Removes item from specific slot
    /// </summary>
    /// <param name="slotIndex">The inventory slot (0-2)</param>
    /// <returns>True if item was removed</returns>
    public bool RemoveItem(int slotIndex)
    {
        return RemoveItemFromSlot(slotIndex, 1) > 0;
    }

    /// <summary>
    /// Removes a specific quantity of items matching the given item type (exact parameter matching)
    /// </summary>
    /// <param name="itemType">Item type to remove</param>
    /// <param name="quantity">Quantity to remove</param>
    /// <returns>Number of items actually removed</returns>
    public int RemoveItem(Item itemType, int quantity)
    {
        if (itemType == null || quantity <= 0) return 0;

        int totalRemoved = 0;
        int remaining = quantity;

        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            if (!slots[i].IsEmpty() && slots[i].ItemType.CanStackWith(itemType))
            {
                int removed = slots[i].RemoveItems(remaining);
                totalRemoved += removed;
                remaining -= removed;
            }
        }

        if (totalRemoved > 0)
        {
            Debug.Log($"Removed {totalRemoved} {itemType.itemName} from inventory");
        }

        return totalRemoved;
    }

    /// <summary>
    /// Removes specific quantity from a specific slot
    /// </summary>
    /// <param name="slotIndex">Slot index (0-2)</param>
    /// <param name="quantity">Quantity to remove</param>
    /// <returns>Number of items actually removed</returns>
    public int RemoveItemFromSlot(int slotIndex, int quantity)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length || quantity <= 0) return 0;

        int removed = slots[slotIndex].RemoveItems(quantity);
        if (removed > 0)
        {
            Debug.Log($"Removed {removed} items from slot {slotIndex}");
        }
        return removed;
    }

    /// <summary>
    /// Gets item type at specific slot
    /// </summary>
    /// <param name="slotIndex">The inventory slot (0-2)</param>
    /// <returns>The Item type at the slotIndex or null if empty</returns>
    public Item GetItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return null;
        return slots[slotIndex].ItemType;
    }

    /// <summary>
    /// Gets the quantity of items in a specific slot
    /// </summary>
    /// <param name="slotIndex">Slot index (0-2)</param>
    /// <returns>Quantity in slot, or 0 if empty/invalid</returns>
    public int GetSlotQuantity(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return 0;
        return slots[slotIndex].CurrentStack;
    }

    /// <summary>
    /// Gets the inventory slot object at the specified index
    /// </summary>
    /// <param name="slotIndex">Slot index (0-2)</param>
    /// <returns>InventorySlot object or null if invalid index</returns>
    public InventorySlot GetSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return null;
        return slots[slotIndex];
    }

    /// <summary>
    /// Checks if specific slot is empty
    /// </summary>
    /// <param name="slotIndex">The inventory slot (0-2)</param>
    /// <returns>True if slot is empty</returns>
    public bool IsSlotEmpty(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;
        return slots[slotIndex].IsEmpty();
    }

    /// <summary>
    /// Finds first empty slot
    /// </summary>
    /// <returns>Index of first empty slot, or -1 if inventory is full</returns>
    public int GetEmptySlotIndex()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty())
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Gets total number of slots
    /// </summary>
    /// <returns>Total slot count (3)</returns>
    public int GetSlotCount()
    {
        return slots.Length;
    }

    /// <summary>
    /// Checks if inventory is full (no empty slots)
    /// </summary>
    /// <returns>True if all slots are occupied</returns>
    public bool IsFull()
    {
        return GetEmptySlotIndex() == -1;
    }

    /// <summary>
    /// Gets total count of items matching the specified item type (exact parameter matching)
    /// </summary>
    /// <param name="itemType">Item type to count</param>
    /// <returns>Total quantity of matching items across all slots</returns>
    public int GetTotalItemCount(Item itemType)
    {
        if (itemType == null) return 0;

        int total = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty() && slots[i].ItemType.CanStackWith(itemType))
            {
                total += slots[i].CurrentStack;
            }
        }
        return total;
    }

    /// <summary>
    /// Checks if the specified quantity of items can be added to inventory
    /// </summary>
    /// <param name="item">Item to check</param>
    /// <param name="quantity">Quantity to check</param>
    /// <returns>True if all items can be added</returns>
    public bool CanAddItem(Item item, int quantity)
    {
        if (item == null || quantity <= 0) return false;
        if (!item.isStackable && quantity > 1) return false;

        int remaining = quantity;

        // Check existing stackable slots
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            if (slots[i].CanAddItem(item))
            {
                remaining -= slots[i].GetRemainingSpace();
            }
        }

        // Check empty slots for remaining items
        if (remaining > 0)
        {
            int emptySlots = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty()) emptySlots++;
            }

            int itemsPerSlot = item.maxStackSize;
            int slotsNeeded = Mathf.CeilToInt((float)remaining / itemsPerSlot);
            return emptySlots >= slotsNeeded;
        }

        return remaining <= 0;
    }

    /// <summary>
    /// Debug method to display current inventory contents with stack quantities
    /// </summary>
    public void DisplayInventory()
    {
        Debug.Log("=== INVENTORY CONTENTS ===");
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty())
            {
                Item item = slots[i].ItemType;
                int quantity = slots[i].CurrentStack;
                string stackInfo = quantity > 1 ? $" x{quantity}" : "";
                Debug.Log($"Slot {i}: {item.itemName} ({item.itemType}){stackInfo}");
            }
            else
            {
                Debug.Log($"Slot {i}: Empty");
            }
        }
        Debug.Log("========================");
    }
}