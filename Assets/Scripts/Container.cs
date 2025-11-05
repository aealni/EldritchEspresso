using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Abstract base class for all containers that can hold and transfer items
/// </summary>
public abstract class Container : MonoBehaviour
{
    [Header("Container Settings")]
    [SerializeField] protected string containerName = "Container";
    [SerializeField] protected float interactionRange = 2f;
    [SerializeField] protected bool isInteractable = true;
    
    [Header("Visual Feedback")]
    [SerializeField] protected GameObject interactionPrompt;
    
    protected bool playerInRange = false;
    protected Transform playerTransform;
    
    /// <summary>
    /// Gets the name of this container
    /// </summary>
    public string ContainerName => containerName;
    
    /// <summary>
    /// Gets the interaction range of this container
    /// </summary>
    public float InteractionRange => interactionRange;
    
    /// <summary>
    /// Gets whether this container is currently interactable
    /// </summary>
    public bool IsInteractable => isInteractable;
    
    /// <summary>
    /// Gets whether the player is currently in range
    /// </summary>
    public bool PlayerInRange => playerInRange;
    
    protected virtual void Start()
    {
        // Initialize container
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    protected virtual void Update()
    {
        // Check for player proximity
        CheckPlayerProximity();
    }
    
    /// <summary>
    /// Checks if the player is within interaction range
    /// </summary>
    protected virtual void CheckPlayerProximity()
    {
        if (playerTransform == null) return;
        
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;
        
        if (playerInRange && !wasInRange)
        {
            OnPlayerEnterRange();
        }
        else if (!playerInRange && wasInRange)
        {
            OnPlayerExitRange();
        }
    }
    
    /// <summary>
    /// Sets the player transform for proximity detection
    /// </summary>
    /// <param name="player">Player transform</param>
    public virtual void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }
    
    /// <summary>
    /// Called when player enters interaction range
    /// </summary>
    protected virtual void OnPlayerEnterRange()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
        
        Debug.Log($"Player entered range of {containerName}");
    }
    
    /// <summary>
    /// Called when player exits interaction range
    /// </summary>
    protected virtual void OnPlayerExitRange()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        Debug.Log($"Player exited range of {containerName}");
    }
    
    /// <summary>
    /// Checks if the player can interact with this container
    /// </summary>
    /// <param name="player">Player transform</param>
    /// <returns>True if player can interact</returns>
    public virtual bool CanInteract(Transform player)
    {
        if (!isInteractable) return false;
        if (player == null) return false;
        
        float distance = Vector2.Distance(transform.position, player.position);
        return distance <= interactionRange;
    }
    
    /// <summary>
    /// Gets the distance to the player
    /// </summary>
    /// <param name="player">Player transform</param>
    /// <returns>Distance to player</returns>
    public virtual float GetInteractionDistance(Transform player)
    {
        if (player == null) return float.MaxValue;
        return Vector2.Distance(transform.position, player.position);
    }
    
    /// <summary>
    /// Called when player interacts with this container
    /// </summary>
    /// <param name="playerTransform">Player transform</param>
    public abstract void OnInteract(Transform playerTransform);
    
    /// <summary>
    /// Gets all available items in this container
    /// </summary>
    /// <returns>List of available items</returns>
    public abstract List<Item> GetAvailableItems();
    
    /// <summary>
    /// Transfers an item from this container to the player
    /// </summary>
    /// <param name="item">Item to transfer</param>
    /// <param name="quantity">Quantity to transfer</param>
    /// <param name="playerInventory">Player's inventory</param>
    /// <returns>Number of items actually transferred</returns>
    public abstract int TransferItemToPlayer(Item item, int quantity, Inventory playerInventory);
    
    /// <summary>
    /// Gets the first available item of a specific type
    /// </summary>
    /// <param name="itemType">Type of item to find</param>
    /// <returns>First matching item or null if not found</returns>
    public virtual Item GetFirstItemOfType(ItemType itemType)
    {
        List<Item> items = GetAvailableItems();
        foreach (Item item in items)
        {
            if (item != null && item.itemType == itemType)
            {
                return item;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Gets the quantity of a specific item in this container
    /// </summary>
    /// <param name="item">Item to count</param>
    /// <returns>Quantity of the item</returns>
    public abstract int GetItemQuantity(Item item);
    
    /// <summary>
    /// Checks if this container has any items
    /// </summary>
    /// <returns>True if container has items</returns>
    public virtual bool HasItems()
    {
        List<Item> items = GetAvailableItems();
        return items != null && items.Count > 0;
    }
    
    /// <summary>
    /// Enables or disables interaction with this container
    /// </summary>
    /// <param name="interactable">Whether container should be interactable</param>
    public virtual void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        if (!interactable && playerInRange)
        {
            OnPlayerExitRange();
        }
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        // Draw interaction range in scene view
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireCircle(transform.position, interactionRange);
    }
}
