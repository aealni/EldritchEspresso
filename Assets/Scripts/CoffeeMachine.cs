using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Coffee machine container that can brew coffee from beans
/// </summary>
public class CoffeeMachine : Container
{
    [Header("Coffee Machine Settings")]
    [SerializeField] private Item coffeeBeanItem;
    [SerializeField] private Item coffeeItem;
    [SerializeField] private float brewingTime = 3f;
    [SerializeField] private int maxBeans = 10;
    [SerializeField] private int maxCoffee = 5;
    
    [Header("Current State")]
    [SerializeField] private int currentBeans = 0;
    [SerializeField] private int currentCoffee = 0;
    [SerializeField] private bool isBrewing = false;
    
    private Coroutine brewingCoroutine;
    
    protected override void Start()
    {
        base.Start();
        containerName = "Coffee Machine";
    }
    
    /// <summary>
    /// Called when player interacts with the coffee machine
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
        
        // If player has coffee beans, try to add them
        if (player.Inventory.HasItem(coffeeBeanItem))
        {
            int beansToAdd = Mathf.Min(player.Inventory.GetTotalItemCount(coffeeBeanItem), maxBeans - currentBeans);
            if (beansToAdd > 0)
            {
                player.Inventory.RemoveItem(coffeeBeanItem, beansToAdd);
                currentBeans += beansToAdd;
                Debug.Log($"Added {beansToAdd} coffee beans to machine. Total beans: {currentBeans}");
            }
        }
        
        // If we have beans and not brewing, start brewing
        if (currentBeans > 0 && !isBrewing && currentCoffee < maxCoffee)
        {
            StartBrewing();
        }
        
        // If we have coffee, try to give it to player
        if (currentCoffee > 0)
        {
            int coffeeToGive = Mathf.Min(currentCoffee, 1); // Give one coffee at a time
            if (player.Inventory.CanAddItem(coffeeItem, coffeeToGive))
            {
                player.Inventory.AddItem(coffeeItem, coffeeToGive);
                currentCoffee -= coffeeToGive;
                Debug.Log($"Gave {coffeeToGive} coffee to player. Remaining coffee: {currentCoffee}");
            }
            else
            {
                Debug.Log("Player inventory is full, cannot give coffee");
            }
        }
    }
    
    /// <summary>
    /// Gets all available items in the coffee machine
    /// </summary>
    /// <returns>List of available items</returns>
    public override List<Item> GetAvailableItems()
    {
        List<Item> items = new List<Item>();
        
        if (currentCoffee > 0 && coffeeItem != null)
        {
            for (int i = 0; i < currentCoffee; i++)
            {
                items.Add(coffeeItem);
            }
        }
        
        return items;
    }
    
    /// <summary>
    /// Transfers an item from the coffee machine to the player
    /// </summary>
    /// <param name="item">Item to transfer</param>
    /// <param name="quantity">Quantity to transfer</param>
    /// <param name="playerInventory">Player's inventory</param>
    /// <returns>Number of items actually transferred</returns>
    public override int TransferItemToPlayer(Item item, int quantity, Inventory playerInventory)
    {
        if (item == null || playerInventory == null) return 0;
        
        // Only coffee can be transferred from coffee machine
        if (item != coffeeItem) return 0;
        
        int availableCoffee = currentCoffee;
        int coffeeToTransfer = Mathf.Min(quantity, availableCoffee);
        
        if (coffeeToTransfer > 0 && playerInventory.CanAddItem(item, coffeeToTransfer))
        {
            playerInventory.AddItem(item, coffeeToTransfer);
            currentCoffee -= coffeeToTransfer;
            Debug.Log($"Transferred {coffeeToTransfer} coffee to player");
            return coffeeToTransfer;
        }
        
        return 0;
    }
    
    /// <summary>
    /// Gets the quantity of a specific item in the coffee machine
    /// </summary>
    /// <param name="item">Item to count</param>
    /// <returns>Quantity of the item</returns>
    public override int GetItemQuantity(Item item)
    {
        if (item == coffeeItem)
        {
            return currentCoffee;
        }
        return 0;
    }
    
    /// <summary>
    /// Starts the brewing process
    /// </summary>
    public void StartBrewing()
    {
        if (isBrewing || currentBeans <= 0) return;
        
        isBrewing = true;
        brewingCoroutine = StartCoroutine(BrewingProcess());
        Debug.Log("Started brewing coffee...");
    }
    
    /// <summary>
    /// Completes the brewing process
    /// </summary>
    public void CompleteBrewing()
    {
        if (currentBeans > 0 && currentCoffee < maxCoffee)
        {
            currentBeans--;
            currentCoffee++;
            Debug.Log($"Brewing complete! Coffee: {currentCoffee}, Beans: {currentBeans}");
        }
    }
    
    /// <summary>
    /// Checks if the coffee machine can brew
    /// </summary>
    /// <returns>True if can brew</returns>
    public bool CanBrew()
    {
        return currentBeans > 0 && !isBrewing && currentCoffee < maxCoffee;
    }
    
    /// <summary>
    /// Coroutine for the brewing process
    /// </summary>
    private IEnumerator BrewingProcess()
    {
        yield return new WaitForSeconds(brewingTime);
        
        CompleteBrewing();
        isBrewing = false;
        
        // Continue brewing if we have more beans and space
        if (CanBrew())
        {
            StartBrewing();
        }
    }
    
    /// <summary>
    /// Gets the current status of the coffee machine
    /// </summary>
    /// <returns>Status string</returns>
    public string GetStatus()
    {
        if (isBrewing)
        {
            return $"Brewing... ({currentBeans} beans, {currentCoffee} coffee)";
        }
        else if (currentCoffee > 0)
        {
            return $"Ready! ({currentCoffee} coffee available)";
        }
        else if (currentBeans > 0)
        {
            return $"Has beans ({currentBeans}), ready to brew";
        }
        else
        {
            return "Empty - needs coffee beans";
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw additional gizmos for coffee machine state
        Gizmos.color = isBrewing ? Color.red : (currentCoffee > 0 ? Color.green : Color.yellow);
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one * 0.3f);
    }

    /// <summary>
    /// Exposes the configured brewed coffee item so other systems can discover menu items.
    /// </summary>
    public Item CoffeeItem => coffeeItem;
}
