using System;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    [Header("Timers")]

    public float ordering_patience = 10;
    public float serving_patience = 10;
    public float eating_duration = 2;
    float timer = -1;

    [Header("Movement")]

    public float movement_speed = 15;
    Vector2Int curr_pos = new(-1, -1);
    Vector2Int target_pos = new(-1, -1);
    Vector2Int next_move_pos = new(-1, -1);

    public int id { get; private set; }

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

    STATE state;
    List<DummyFood> menu;
    DummyFood order;

    public void Activate(int new_id, Vector2Int spawn_pos, Vector2Int seat)
    {
        id = new_id;
        curr_pos = spawn_pos;
        target_pos = seat;

        state = STATE.SEATING;
    }


    /// <summary>
    /// TO-DO: RANDOMLY CHOOSE A MENU OPTION
    /// </summary>
    void Ordering()
    {
        state = STATE.ORDERING;
        timer = ordering_patience;

        // add code here
    }

    /// <summary>
    /// TO-DO: SHOULD RETURN THE CUSTOMER'S ORDER
    /// </summary>
    public void TakeOrder()
    {
        state = STATE.WAITING;
        timer = serving_patience;

        // add code here
    }

    /// <summary>
    /// TO-DO: TAKES IN A FOOD ARGUEMENT SO THAT WE CAN CHECK IF ITS THE RIGHT FOOD
    /// </summary>
    public void ServeFood()
    {
        state = STATE.EATING;
        timer = eating_duration;

        // add code here
    }

    void Leaving()
    {
        state = STATE.LEAVING;
        CustomerManager.ReturnSeat(curr_pos);
        target_pos = CustomerManager.FindEntrance();
    }

    // TODO
    void Move()
    {
        if (next_move_pos != new Vector2Int(-1, -1))
        {
            transform.position = Vector2.MoveTowards(transform.position, CustomerManager.GridToWorld(next_move_pos), movement_speed * Time.deltaTime);
            if ((CustomerManager.GridToWorld(next_move_pos) - (Vector2)transform.position).magnitude > 0.01)
            {
                return;
            }

            CustomerManager.ResetSquare(curr_pos);
            curr_pos = next_move_pos;
        }

        //Astar
        next_move_pos = CustomerManager.Astar(curr_pos, target_pos, id);
        CustomerManager.SetSquare(next_move_pos, id);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(id);
        if (timer > 0 && (timer -= Time.deltaTime) < 0)
        {
            Leaving();
        }

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
                    Destroy(gameObject);
                }
            } else
            {
                Move();
            }
        }
    }
}
