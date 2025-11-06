using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Customer : MonoBehaviour
{
    [Header("Timers")]

    [SerializeField] float ordering_patience;
    [SerializeField] float serving_patience;
    [SerializeField] float eating_duration;
    [SerializeField] float timer = -1;

    [Header("Movement")]

    [SerializeField] float movement_speed = 15;
    public Vector2Int curr_pos = new(-1, -1);
    public Vector2Int target_pos = new(-1, -1);
    [SerializeField] Vector2Int next_move_pos = new(-1, -1);

    public SpriteRenderer sprite_renderer;

    // [x, y]
    // 0 is empty
    // -1 is seats
    // -2 are spawn locations
    // 1 are obstacles
    // else: id of customer

    //true = available, false = not 

    // State
    enum STATE
    {
        SEATING,
        ORDERING,
        WAITING,
        EATING,
        LEAVING
    }

    [SerializeField] STATE state;
    [SerializeField] List<Item> menu;
    [SerializeField] Item order;

    [SerializeField] int id;
    [SerializeField] Vector2Int my_seat; // Store the seat position to return it later
    
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 5f;
    [Tooltip("Shown when player is in range. E.g., a child circle sprite under the customer.")]
    [SerializeField] private GameObject rangeIndicator;
    private Transform playerTransform;
    private bool playerInRange = false;

    private void Awake()
    {
        // Cache the SpriteRenderer at awake so it's always available
        if (sprite_renderer == null)
        {
            sprite_renderer = GetComponent<SpriteRenderer>();
        }
        // Ensure the range indicator starts hidden
        if (rangeIndicator != null) rangeIndicator.SetActive(false);
    }

    public void Activate(int new_id, Vector2Int spawn_pos, Vector2Int seat)
    {
        id = new_id;
        curr_pos = spawn_pos;
        target_pos = seat;
        my_seat = seat; // Remember which seat was assigned to this customer

        state = STATE.SEATING;

        // Redundant safety in case Awake hasn't run yet
        if (sprite_renderer == null)
        {
            sprite_renderer = GetComponent<SpriteRenderer>();
        }
    }

    void Ordering()
    {
        state = STATE.ORDERING;

    if (menu == null) menu = new List<Item>();
        
        // If no items configured in the prefab, try to auto-build a basic menu
        if (menu.Count == 0)
        {
            var dynamicMenu = new List<Item>();
            
            // Discover foods from ovens
            foreach (var oven in FindObjectsByType<Oven>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (oven != null && oven.PieItem != null && !dynamicMenu.Contains(oven.PieItem))
                    dynamicMenu.Add(oven.PieItem);
            }
            
            // Discover foods present in the scene (any Food component)
            foreach (var food in FindObjectsByType<Food>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (food != null && !dynamicMenu.Contains(food))
                    dynamicMenu.Add(food);
            }
            
            // Discover coffee from coffee machines if it's a Food
        foreach (var cm in FindObjectsByType<CoffeeMachine>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (cm != null && cm.CoffeeItem is Food coffeeFood && !dynamicMenu.Contains(coffeeFood))
            dynamicMenu.Add(coffeeFood);
            }
            
            if (dynamicMenu.Count > 0)
            {
                menu = dynamicMenu;
                Debug.Log($"Customer {id} had an empty menu; auto-filled with {menu.Count} item(s) from scene/containers.");
            }
            else
            {
                Debug.LogWarning($"Customer {id} menu is empty and no Food items were discovered in the scene. Customer will wait indefinitely.");
                timer = -1; // Wait forever
                return;
            }
        }
        
        // Randomly select an item from the menu (can be coffee, pie, or any other food)
    order = menu[UnityEngine.Random.Range(0, Mathf.Max(1, menu.Count))];
        
        // If a null slipped into the menu, try to find a non-null fallback
        if (order == null)
        {
            order = menu.FirstOrDefault(m => m != null);
        }
        
        if (order != null)
        {
            Debug.Log($"Customer {id} ordered: {order.itemName}");
        }
        else
        {
            Debug.LogWarning($"Customer {id} could not determine an order (menu contained only null entries). Waiting indefinitely.");
            timer = -1; // Wait forever
            return;
        }
        
        // Set timer based on ordering_patience (if > 0, use it; otherwise wait indefinitely)
        timer = ordering_patience > 0 ? ordering_patience : -1;
    }

    public Item TakeOrder()
    {
        if (state != STATE.ORDERING)
        {
            return null;
        }
        if (order == null)
        {
            Debug.LogWarning($"Customer {id} is ORDERING but has no order item. Aborting take.");
            return null;
        }
        state = STATE.WAITING;
        // Set timer based on serving_patience (if > 0, use it; otherwise wait indefinitely)
        timer = serving_patience > 0 ? serving_patience : -1;
        Debug.Log($"Customer {id} order taken: {order.itemName}");
        return order;
    }
    
    /// <summary>
    /// Gets the current order of this customer
    /// </summary>
    /// <returns>The food item the customer ordered, or null if not ordering</returns>
    public Item GetCurrentOrder()
    {
        return order;
    }
    
    /// <summary>
    /// Gets the current state of this customer
    /// </summary>
    /// <returns>The customer's current state as a string</returns>
    public string GetStateString()
    {
        return state.ToString();
    }

    public bool GiveOrder(Item f)
    {
        if (f == null || order == null)
        {
            Debug.LogWarning($"Customer {id} cannot receive order: provided or expected item is null.");
            return false;
        }
        if (state != STATE.WAITING)
        {
            Debug.LogWarning($"Customer {id} is not waiting for food (current state: {state})");
            return false;
        }
        if (f == order)
        {
            state = STATE.EATING;
            timer = eating_duration;
            Debug.Log($"Customer {id} received correct order: {f.itemName}. Now eating.");
            return true;
        }
        else
        {
            Debug.LogWarning($"Customer {id} received wrong order! Expected {order.itemName} but got {f.itemName}");
            return false;
        }
    }
    void Leaving()
    {
        // Check if customer is leaving without eating (lost customer = lose score)
        if (state == STATE.ORDERING || state == STATE.WAITING)
        {
            int penalty = 1; // Score penalty for losing a customer
            if (order is Food food)
            {
                penalty = Mathf.Max(1, Mathf.CeilToInt(food.cost / 2f)); // Penalty is half of food cost, rounded up, minimum 1
            }
            ScoreManager.Instance?.ApplyScoreDelta(-penalty);
            Debug.LogWarning($"Customer {id} left unsatisfied! Lost {penalty} points.");
        }
        
        state = STATE.LEAVING;
        // Don't return seat yet - wait until customer is destroyed
        target_pos = CustomerManager.FindEntrance();
    }

    // TODO
    public void Move()
    {
        if (next_move_pos != new Vector2Int(-1, -1))
        {
            if (CustomerManager.GetSquare(next_move_pos) > 0 && CustomerManager.GetSquare(next_move_pos) < id)
            {
                next_move_pos = CustomerManager.Astar(curr_pos, target_pos, id, target_pos);
                CustomerManager.SetSquare(next_move_pos, id);
            }

            transform.position = Vector2.MoveTowards(transform.position, CustomerManager.GridToWorld(next_move_pos), movement_speed * Time.deltaTime);
            if ((CustomerManager.GridToWorld(next_move_pos) - (Vector2)transform.position).magnitude > 0.01)
            {
                return;
            }


            CustomerManager.ResetSquare(curr_pos);
            curr_pos = next_move_pos;
        }

        //Astar
        next_move_pos = CustomerManager.Astar(curr_pos, target_pos, id, target_pos);
        CustomerManager.SetSquare(next_move_pos, id);
    }

    Color ChangeColor()
    {
        return state switch
        {
            STATE.SEATING => Color.white,
            STATE.ORDERING => Color.blue,
            STATE.WAITING => Color.yellow,
            STATE.EATING => Color.green,
            _ => Color.red,
        };

    }

    // Update is called once per frame
    void Update()
    {
        sprite_renderer.color = ChangeColor();

        if (timer > 0)
        {
            timer -= Time.deltaTime;

            if (timer < 0)
            {
                Leaving();
            }
        }
        
        // Check for player nearby and F key press to give food
        CheckPlayerInteraction();

        if (target_pos != new Vector2Int(-1, -1))
        {
            if (curr_pos == target_pos)
            {
                next_move_pos = new Vector2Int(-1, -1);
                if (state == STATE.SEATING)
                {
                    Ordering();
                }
                else if (state == STATE.LEAVING)
                {
                    CustomerManager.ResetSquare(curr_pos);
                    CustomerManager.ReturnSeat(my_seat); // Return seat only when actually leaving
                    // Hide range indicator when leaving
                    if (rangeIndicator != null) rangeIndicator.SetActive(false);
                    Destroy(gameObject);
                    return;
                }
            }

            Move();
        }
    }
    
    /// <summary>
    /// Checks if player is nearby and can give food
    /// </summary>
    private void CheckPlayerInteraction()
    {
        // Find player if not already found
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            return;
        }
        
        // Check if player is in range
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= interactionRange;
        
        // Update range indicator visibility (same as oven)
        UpdateRangeIndicator(inRange);
        playerInRange = inRange;
        
        // Debug: Show when player is near
        if (inRange && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log($"Customer {id}: Player in range (distance: {distance:F2}), state: {state}, trying to interact...");
            TryReceiveFood();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log($"Customer {id}: Player too far (distance: {distance:F2}, need: {interactionRange})");
        }
    }
    
    /// <summary>
    /// Tries to receive food from the player
    /// </summary>
    private void TryReceiveFood()
    {
        if (state == STATE.ORDERING)
        {
            Debug.Log($"Customer {id} hasn't ordered yet, taking order now...");
            Item takenOrder = TakeOrder();
            if (takenOrder != null)
            {
                Debug.Log($"Customer {id} order taken: {takenOrder.itemName}");
            }
            return;
        }
        
        if (state != STATE.WAITING)
        {
            Debug.Log($"Customer {id} is not waiting for food (state: {state})");
            return;
        }
        
        PlayerController player = playerTransform.GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.Log("No PlayerController found");
            return;
        }
        
        // Check if player has the ordered item (must be explicitly given with key press)
        if (player.Inventory.HasItem(order))
        {
            // Try to remove the food from player inventory
            int removed = player.Inventory.RemoveItem(order, 1);
            if (removed > 0)
            {
                // Give the food to customer
                if (GiveOrder(order))
                {
                    Debug.Log($"Successfully gave {order.itemName} to customer {id}!");
                    // Award score for a successful serve
                    int points = 10;
                    if (order is Food food)
                    {
                        points = Mathf.Max(1, food.cost);
                    }
                    ScoreManager.Instance?.ApplyScoreDelta(points);
                    Debug.Log($"Customer {id} received correct order! Gained {points} points.");
                }
            }
        }
        else
        {
            Debug.Log($"Player doesn't have {order.itemName}. Customer wants: {order.itemName}");
        }
    }
    
    /// <summary>
    /// Updates the range indicator visibility based on player proximity (same as Oven)
    /// </summary>
    private void UpdateRangeIndicator(bool inRange)
    {
        if (rangeIndicator == null)
        {
            Debug.LogWarning($"Customer {id}: Range indicator is NULL! Assign a GameObject to the Range Indicator field in the Inspector.");
            return;
        }
        
        // Show range indicator when player is in range
        if (rangeIndicator.activeSelf != inRange)
        {
            rangeIndicator.SetActive(inRange);
        }
    }
}
