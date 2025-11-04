using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Oven that can bake pies on demand (no ingredients required)
/// Press E to bake a pie, press F to pick up a ready pie
/// </summary>
public class Oven : Container
{
    [Header("Oven Settings")]
    [SerializeField] private Food pieItem;
    [SerializeField] private float bakingTime = 5f;
    [SerializeField] private int maxPies = 5;
    [SerializeField] private bool autoBake = false; // when true, auto-bake until full
    
    [Header("Current State")]
    [SerializeField] private int currentPies = 0;
    [SerializeField] private bool isBaking = false;
    
    [Header("UI/Indicators")]
    [Tooltip("Shown when at least one pie is ready. E.g., a child with an exclamation mark sprite above the oven.")]
    [SerializeField] private GameObject readyIndicator;
    [Tooltip("Optional Animator on the oven for visual feedback. Expects bools: IsBaking, HasReady")] 
    [SerializeField] private Animator animator;
    
    private Coroutine bakingCoroutine;
    
    protected override void Start()
    {
        base.Start();
        containerName = "Oven";
        // Ensure the ready indicator starts hidden
        if (readyIndicator != null) readyIndicator.SetActive(false);
        if (animator == null) animator = GetComponent<Animator>();
        SyncAnimator();
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Check for E key to start baking
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (CanBake())
            {
                StartBaking();
            }
            else if (isBaking)
            {
                Debug.Log("Oven is already baking!");
            }
            else if (currentPies >= maxPies)
            {
                Debug.Log("Oven is full! Pick up pies first (Press F)");
            }
            else
            {
                Debug.Log("Cannot start baking: unknown state");
            }
        }
        
        // Check for F key to pick up pie
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            PickupPie();
        }

        // Update ready indicator visibility
        UpdateReadyIndicator();
        SyncAnimator();
    }
    
    /// <summary>
    /// Called when player interacts with the oven
    /// </summary>
    /// <param name="playerTransform">Player transform</param>
    public override void OnInteract(Transform playerTransform)
    {
        if (!CanInteract(playerTransform)) return;
        
        Debug.Log($"Near {containerName}. Press E to bake pie, F to pick up pie");
    }
    
    /// <summary>
    /// Picks up a pie from the oven
    /// </summary>
    private void PickupPie()
    {
        if (currentPies <= 0)
        {
            Debug.Log("No pies available to pick up!");
            return;
        }
        
        if (playerTransform == null) return;
        
        PlayerController player = playerTransform.GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.Log("No PlayerController found");
            return;
        }
        
        // Give one pie to player
        if (player.Inventory.CanAddItem(pieItem, 1))
        {
            player.Inventory.AddItem(pieItem, 1);
            currentPies--;
            Debug.Log($"Picked up 1 pie. Remaining pies in oven: {currentPies}");
            
            // Hide indicator if no more pies are ready
            UpdateReadyIndicator();
            SyncAnimator();
        }
        else
        {
            Debug.Log("Inventory is full! Cannot pick up pie");
        }
    }
    
    /// <summary>
    /// Gets all available items in the oven
    /// </summary>
    /// <returns>List of available items</returns>
    public override List<Item> GetAvailableItems()
    {
        List<Item> items = new List<Item>();
        
        if (currentPies > 0 && pieItem != null)
        {
            for (int i = 0; i < currentPies; i++)
            {
                items.Add(pieItem);
            }
        }
        
        return items;
    }
    
    /// <summary>
    /// Transfers an item from the oven to the player
    /// </summary>
    /// <param name="item">Item to transfer</param>
    /// <param name="quantity">Quantity to transfer</param>
    /// <param name="playerInventory">Player's inventory</param>
    /// <returns>Number of items actually transferred</returns>
    public override int TransferItemToPlayer(Item item, int quantity, Inventory playerInventory)
    {
        if (item == null || playerInventory == null) return 0;
        
        // Only pies can be transferred from oven
        if (item != pieItem) return 0;
        
        int availablePies = currentPies;
        int piesToTransfer = Mathf.Min(quantity, availablePies);
        
        if (piesToTransfer > 0 && playerInventory.CanAddItem(item, piesToTransfer))
        {
            playerInventory.AddItem(item, piesToTransfer);
            currentPies -= piesToTransfer;
            Debug.Log($"Transferred {piesToTransfer} pies to player");
            return piesToTransfer;
        }
        
        return 0;
    }
    
    /// <summary>
    /// Gets the quantity of a specific item in the oven
    /// </summary>
    /// <param name="item">Item to count</param>
    /// <returns>Quantity of the item</returns>
    public override int GetItemQuantity(Item item)
    {
        if (item == pieItem)
        {
            return currentPies;
        }
        return 0;
    }
    
    /// <summary>
    /// Starts the baking process
    /// </summary>
    public void StartBaking()
    {
        if (isBaking) return;
        if (currentPies >= maxPies)
        {
            Debug.Log("Oven is full; cannot start baking.");
            return;
        }
        if (pieItem == null)
        {
            Debug.LogWarning("Pie Item is not assigned on the Oven. Assign a Food prefab to bake.");
        }
        
        isBaking = true;
        bakingCoroutine = StartCoroutine(BakingProcess());
        Debug.Log($"Started baking pie... ({bakingTime} seconds)");
        SyncAnimator();
    }
    
    /// <summary>
    /// Completes the baking process
    /// </summary>
    public void CompleteBaking()
    {
        if (currentPies < maxPies)
        {
            currentPies++;
            Debug.Log($"Baking complete! Total pies: {currentPies}");
            // Show indicator now that at least one pie is ready
            UpdateReadyIndicator();
        }
    }
    
    /// <summary>
    /// Checks if the oven can bake
    /// </summary>
    /// <returns>True if can bake</returns>
    public bool CanBake()
    {
        return !isBaking && currentPies < maxPies;
    }
    
    /// <summary>
    /// Coroutine for the baking process
    /// </summary>
    private IEnumerator BakingProcess()
    {
        yield return new WaitForSeconds(bakingTime);
        
        CompleteBaking();
        isBaking = false;
        // Optionally auto-continue baking until full
        SyncAnimator();
        if (autoBake && CanBake())
        {
            StartBaking();
        }
    }
    
    /// <summary>
    /// Gets the current status of the oven
    /// </summary>
    /// <returns>Status string</returns>
    public string GetStatus()
    {
        if (isBaking)
        {
            return $"Baking... ({currentPies} pies ready)";
        }
        else if (currentPies > 0)
        {
            return $"Ready! ({currentPies} pies available - Press F to pick up)";
        }
        else
        {
            return "Ready to bake (Press E to bake pie)";
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw additional gizmos for oven state
        Gizmos.color = isBaking ? Color.red : (currentPies > 0 ? Color.green : Color.yellow);
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one * 0.3f);
    }

    /// <summary>
    /// Exposes the configured pie item so other systems (e.g., customers) can discover available menu items.
    /// </summary>
    public Food PieItem => pieItem;

    private void UpdateReadyIndicator()
    {
        if (readyIndicator == null) return;
        bool show = !isBaking && currentPies > 0;
        if (readyIndicator.activeSelf != show)
        {
            readyIndicator.SetActive(show);
        }
    }

    private void SyncAnimator()
    {
        if (animator == null) return;
        animator.SetBool("IsBaking", isBaking);
        animator.SetBool("HasReady", !isBaking && currentPies > 0);
    }
}
