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
    [SerializeField] private float interactionRange = 2f;
    private Transform playerTransform;

    public void Activate(int new_id, Vector2Int spawn_pos, Vector2Int seat)
    {
        id = new_id;
        curr_pos = spawn_pos;
        target_pos = seat;
        my_seat = seat; // Remember which seat was assigned to this customer

        state = STATE.SEATING;

        sprite_renderer = GetComponent<SpriteRenderer>();
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
        
    // Wait indefinitely for the player to take the order
    timer = -1;
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
    // After taking the order, wait indefinitely until the correct item is given
    timer = -1;
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
        if (distance > interactionRange) return;
        
        // Check for F key press
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryReceiveFood();
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
                    ScoreManager.Instance?.AddMiniGameScore("ServeCustomer", points);
                }
            }
        }
        else
        {
            Debug.Log($"Player doesn't have {order.itemName}. Customer wants: {order.itemName}");
        }
    }
}
