using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Player Inventory - 3 Slots")]
    [SerializeField] private Item[] slots = new Item[3];

    /// Adds an item to the first available slot
    /// item: Item to add
    /// returns True if item was added successfully
    public bool AddItem(Item item)
    {
        if (item == null) return false;

        int emptySlot = GetEmptySlotIndex();
        if (emptySlot >= 0)
        {
            slots[emptySlot] = item;
            Debug.Log($"Added {item.itemName} to inventory slot {emptySlot}");
            return true;
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    /// Removes item from specific slot
    /// slotIndex: the inventory slot(0-2)
    /// returns True if item was removed
    public bool RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;
        
        if (slots[slotIndex] != null)
        {
            Debug.Log($"Removed {slots[slotIndex].itemName} from slot {slotIndex}");
            slots[slotIndex] = null;
            return true;
        }
        
        return false;
    }

    /// Gets item at specific slot
    /// slotIndex: the inventory slot(0-2)
    /// returns the Item at the slotIndex or null if empty
    public Item GetItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return null;
        return slots[slotIndex];
    }

    /// Checks if specific slot is empty
    /// slotindex: the inventory slot (0-2)
    /// returns True if slot is empty
    public bool IsSlotEmpty(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;
        return slots[slotIndex] == null;
    }

    /// Finds first empty slot
    /// returns Index of first empty slot, or -1 if inventory is full
    public int GetEmptySlotIndex()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                return i;
        }
        return -1;
    }

    /// Gets total number of slots
    /// returns Total slot count (3)
    public int GetSlotCount()
    {
        return slots.Length;
    }

    /// Checks if inventory is full
    /// returns True if all slots are occupied
    public bool IsFull()
    {
        return GetEmptySlotIndex() == -1;
    }

    /// Debug method to display current inventory contents
    public void DisplayInventory()
    {
        Debug.Log("=== INVENTORY CONTENTS ===");
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                Debug.Log($"Slot {i}: {slots[i].itemName} ({slots[i].itemType})");
            else
                Debug.Log($"Slot {i}: Empty");
        }
        Debug.Log("========================");
    }
}
