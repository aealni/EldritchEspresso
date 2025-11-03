using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Player controller that handles movement, inventory, and container interaction
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 6f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 lastMoveDirection = Vector2.down;
    
    [Header("Inventory")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private int selectedSlot = 0;
    
    [Header("Container Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask containerLayerMask = -1;
    private List<Container> nearbyContainers = new List<Container>();
    private Container nearestContainer;
    
    [Header("Input")]
    [SerializeField] private PlayerInputHandler inputHandler;
    
    // Events
    public System.Action<int> OnSlotSelected;
    public System.Action<Container> OnContainerEnterRange;
    public System.Action<Container> OnContainerExitRange;
    
    // Properties
    public Inventory Inventory => inventory;
    public int SelectedSlot => selectedSlot;
    public Container NearestContainer => nearestContainer;
    public List<Container> NearbyContainers => new List<Container>(nearbyContainers);
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (inventory == null)
            inventory = GetComponent<Inventory>();
        if (inputHandler == null)
            inputHandler = GetComponent<PlayerInputHandler>();
    }
    
    private void Start()
    {
        // Initialize inventory
        if (inventory == null)
        {
            inventory = gameObject.AddComponent<Inventory>();
        }
        
        // Initialize input handler
        if (inputHandler == null)
        {
            inputHandler = gameObject.AddComponent<PlayerInputHandler>();
        }
        
        // Set up input callbacks
        if (inputHandler != null)
        {
            inputHandler.OnSlot1Pressed += () => SelectSlot(0);
            inputHandler.OnSlot2Pressed += () => SelectSlot(1);
            inputHandler.OnSlot3Pressed += () => SelectSlot(2);
            inputHandler.OnSlot4Pressed += () => SelectSlot(3);
            inputHandler.OnEmptySlotPressed += EmptySelectedSlot;
            inputHandler.OnAddFromContainerPressed += AddItemFromContainer;
            inputHandler.OnInteractPressed += InteractWithNearestContainer;
        }
    }
    
    private void Update()
    {
        HandleMovement();
        UpdateNearbyContainers();
    }
    
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
    
    /// <summary>
    /// Handles player movement input
    /// </summary>
    private void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        movement = new Vector2(moveX, moveY).normalized;
        
        // Update animator
        if (animator != null)
        {
            bool isMoving = movement.sqrMagnitude > 0.01f;
            
            if (isMoving)
            {
                // Store the last direction when moving
                lastMoveDirection = movement;
            }
            
            // Set the isMoving bool FIRST
            animator.SetBool("isMoving", isMoving);
            
            // Always update animator with current or last direction
            // Vertical movement ALWAYS overrides horizontal
            if (Mathf.Abs(lastMoveDirection.y) > 0.01f)
            {
                // Moving/facing vertically - ALWAYS prioritize this
                animator.SetFloat("moveX", 0);
                animator.SetFloat("moveY", lastMoveDirection.y);
            }
            else
            {
                // Moving/facing horizontally only when no vertical movement
                animator.SetFloat("moveX", lastMoveDirection.x);
                animator.SetFloat("moveY", 0);
                
                // Don't flip - let the animator handle left/right animations
                // The animator has separate playerleft and playerright animations
            }
        }
    }
    
    /// <summary>
    /// Updates the list of nearby containers
    /// </summary>
    private void UpdateNearbyContainers()
    {
        // Find all containers in range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRange, containerLayerMask);
        
        List<Container> currentContainers = new List<Container>();
        
        foreach (Collider2D collider in colliders)
        {
            Container container = collider.GetComponent<Container>();
            if (container != null)
            {
                currentContainers.Add(container);
                container.SetPlayerTransform(transform);
                
                if (!nearbyContainers.Contains(container))
                {
                    OnContainerEnterRange?.Invoke(container);
                }
            }
        }
        
        // Check for containers that are no longer in range
        foreach (Container container in nearbyContainers)
        {
            if (!currentContainers.Contains(container))
            {
                OnContainerExitRange?.Invoke(container);
            }
        }
        
        nearbyContainers = currentContainers;
        
        // Update nearest container
        UpdateNearestContainer();
    }
    
    /// <summary>
    /// Updates the nearest container
    /// </summary>
    private void UpdateNearestContainer()
    {
        Container newNearest = null;
        float closestDistance = float.MaxValue;
        
        foreach (Container container in nearbyContainers)
        {
            float distance = container.GetInteractionDistance(transform);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                newNearest = container;
            }
        }
        
        nearestContainer = newNearest;
    }
    
    /// <summary>
    /// Selects an inventory slot
    /// </summary>
    /// <param name="slotIndex">Slot index (0-3)</param>
    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.GetSlotCount())
        {
            Debug.LogWarning($"Invalid slot index: {slotIndex}");
            return;
        }
        
        selectedSlot = slotIndex;
        OnSlotSelected?.Invoke(selectedSlot);
        Debug.Log($"Selected inventory slot {selectedSlot}");
    }
    
    /// <summary>
    /// Empties the selected inventory slot
    /// </summary>
    public void EmptySelectedSlot()
    {
        if (inventory == null) return;
        
        Item item = inventory.GetItem(selectedSlot);
        if (item != null)
        {
            int quantity = inventory.GetSlotQuantity(selectedSlot);
            inventory.RemoveItemFromSlot(selectedSlot, 1); // Remove one item
            Debug.Log($"Emptied 1 {item.itemName} from slot {selectedSlot}");
        }
        else
        {
            Debug.Log($"Slot {selectedSlot} is already empty");
        }
    }
    
    /// <summary>
    /// Adds an item from the nearest container to the selected slot
    /// </summary>
    public void AddItemFromContainer()
    {
        if (nearestContainer == null)
        {
            Debug.Log("No container nearby");
            return;
        }
        
        if (inventory == null)
        {
            Debug.Log("No inventory found");
            return;
        }
        
        // Get the first available item from the container
        List<Item> availableItems = nearestContainer.GetAvailableItems();
        if (availableItems.Count == 0)
        {
            Debug.Log($"{nearestContainer.ContainerName} is empty");
            return;
        }
        
        Item itemToAdd = availableItems[0];
        int transferred = nearestContainer.TransferItemToPlayer(itemToAdd, 1, inventory);
        
        if (transferred > 0)
        {
            Debug.Log($"Added {transferred} {itemToAdd.itemName} from {nearestContainer.ContainerName}");
        }
        else
        {
            Debug.Log($"Could not add item from {nearestContainer.ContainerName} - inventory full or item not available");
        }
    }
    
    /// <summary>
    /// Interacts with the nearest container
    /// </summary>
    public void InteractWithNearestContainer()
    {
        if (nearestContainer == null)
        {
            Debug.Log("No container nearby to interact with");
            return;
        }
        
        if (nearestContainer.CanInteract(transform))
        {
            nearestContainer.OnInteract(transform);
        }
    }
    
    /// <summary>
    /// Finds the nearest container
    /// </summary>
    /// <returns>Nearest container or null if none found</returns>
    public Container FindNearestContainer()
    {
        return nearestContainer;
    }
    
    /// <summary>
    /// Gets the distance to the nearest container
    /// </summary>
    /// <returns>Distance to nearest container or float.MaxValue if none found</returns>
    public float GetDistanceToNearestContainer()
    {
        if (nearestContainer == null) return float.MaxValue;
        return nearestContainer.GetInteractionDistance(transform);
    }
    
    /// <summary>
    /// Checks if the player is near any container
    /// </summary>
    /// <returns>True if near a container</returns>
    public bool IsNearContainer()
    {
        return nearestContainer != null;
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        // Draw interaction range
        // Gizmos.color = Color.blue;
        // Gizmos.DrawWireCircle(transform.position, interactionRange);
        
        // Draw line to nearest container
        if (nearestContainer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nearestContainer.transform.position);
        }
    }
}
