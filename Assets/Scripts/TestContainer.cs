using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple test container for immediate testing of the container system
/// </summary>
public class TestContainer : Container
{
    [Header("Test Container Settings")]
    [SerializeField] private List<Item> testItems = new List<Item>();
    [SerializeField] private int maxItems = 10;
    
    private List<Item> currentItems = new List<Item>();
    
    protected override void Start()
    {
        base.Start();
        containerName = "Test Container";
        
        // Initialize with some test items if provided
        if (testItems.Count > 0)
        {
            currentItems.AddRange(testItems);
        }
    }
    
    /// <summary>
    /// Called when player interacts with the test container
    /// </summary>
    /// <param name="playerTransform">Player transform</param>
    public override void OnInteract(Transform playerTransform)
    {
        if (!CanInteract(playerTransform)) return;
        
        Debug.Log($"Interacting with {containerName}");
        
        // Find player components
        PlayerController player = playerTransform.GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.Log("No PlayerController found on player");
            return;
        }
        
        // Simple interaction - just log available items
        List<Item> availableItems = GetAvailableItems();
        Debug.Log($"Available items in {containerName}: {availableItems.Count}");
        
        foreach (Item item in availableItems)
        {
            Debug.Log($"- {item.itemName} (x{GetItemQuantity(item)})");
        }
        
        // Try to give player a random item if available
        if (availableItems.Count > 0 && player.Inventory != null)
        {
            Item randomItem = availableItems[Random.Range(0, availableItems.Count)];
            int transferred = TransferItemToPlayer(randomItem, 1, player.Inventory);
            
            if (transferred > 0)
            {
                Debug.Log($"Gave {transferred} {randomItem.itemName} to player");
            }
            else
            {
                Debug.Log("Could not give item to player - inventory full or item not available");
            }
        }
        else
        {
            Debug.Log($"{containerName} is empty");
        }
    }
    
    /// <summary>
    /// Gets all available items in the test container
    /// </summary>
    /// <returns>List of available items</returns>
    public override List<Item> GetAvailableItems()
    {
        return new List<Item>(currentItems);
    }
    
    /// <summary>
    /// Transfers an item from the test container to the player
    /// </summary>
    /// <param name="item">Item to transfer</param>
    /// <param name="quantity">Quantity to transfer</param>
    /// <param name="playerInventory">Player's inventory</param>
    /// <returns>Number of items actually transferred</returns>
    public override int TransferItemToPlayer(Item item, int quantity, Inventory playerInventory)
    {
        if (item == null || playerInventory == null) return 0;
        
        // Check if we have the item
        int availableQuantity = GetItemQuantity(item);
        if (availableQuantity == 0) return 0;
        
        // Check if player inventory can accept the item
        if (!playerInventory.CanAddItem(item, quantity))
        {
            Debug.Log("Player inventory cannot accept this item");
            return 0;
        }
        
        // Transfer the item
        int transferred = 0;
        for (int i = 0; i < quantity && i < availableQuantity; i++)
        {
            if (currentItems.Contains(item))
            {
                currentItems.Remove(item);
                playerInventory.AddItem(item);
                transferred++;
            }
        }
        
        Debug.Log($"Transferred {transferred} {item.itemName} to player");
        return transferred;
    }
    
    /// <summary>
    /// Gets the quantity of a specific item in the test container
    /// </summary>
    /// <param name="item">Item to count</param>
    /// <returns>Quantity of the item</returns>
    public override int GetItemQuantity(Item item)
    {
        if (item == null) return 0;
        
        int count = 0;
        foreach (Item currentItem in currentItems)
        {
            if (currentItem != null && currentItem.CanStackWith(item))
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Adds an item to the test container
    /// </summary>
    /// <param name="item">Item to add</param>
    /// <returns>True if item was added successfully</returns>
    public bool AddItem(Item item)
    {
        if (item == null) return false;
        if (currentItems.Count >= maxItems)
        {
            Debug.Log($"{containerName} is full");
            return false;
        }
        
        currentItems.Add(item);
        Debug.Log($"Added {item.itemName} to {containerName}");
        return true;
    }
    
    /// <summary>
    /// Removes an item from the test container
    /// </summary>
    /// <param name="item">Item to remove</param>
    /// <returns>True if item was removed successfully</returns>
    public bool RemoveItem(Item item)
    {
        if (item == null) return false;
        
        bool removed = currentItems.Remove(item);
        if (removed)
        {
            Debug.Log($"Removed {item.itemName} from {containerName}");
        }
        return removed;
    }
    
    /// <summary>
    /// Gets the current status of the test container
    /// </summary>
    /// <returns>Status string</returns>
    public string GetStatus()
    {
        return $"{containerName}: {currentItems.Count}/{maxItems} items";
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw additional gizmos for test container
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one * 0.3f);
        
        // Draw item count
        if (currentItems.Count > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 1f, Vector3.one * 0.2f);
        }
    }
}
