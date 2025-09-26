using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Customer : MonoBehaviour
{
    [Header("Timers")]

    [SerializeField] float ordering_patience = 20;
    [SerializeField] float serving_patience = 20;
    [SerializeField] float eating_duration = 5;
    [SerializeField] float timer = -1;

    [Header("Movement")]

    [SerializeField] float movement_speed = 10;
    [SerializeField] Vector2Int curr_pos = new(-1, -1);
    [SerializeField] Vector2Int target_pos = new(-1, -1);
    [SerializeField] Vector2Int next_move_pos = new(-1, -1);

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
    [SerializeField] List<DummyFood> menu;
    [SerializeField] DummyFood order;

    [SerializeField] int id;

    public void Activate(int new_id, Vector2Int spawn_pos, Vector2Int seat)
    {
        id = new_id;
        curr_pos = spawn_pos;
        target_pos = seat;

        state = STATE.SEATING;
    }

    void Ordering()
    {
        state = STATE.ORDERING;

        // if (menu.Count == 0)
        // {
        //     // Wait for seat TODO
        //     Debug.LogError("No items on menu!");
        // }
        // order = menu[UnityEngine.Random.Range(0, menu.Count)];
        timer = ordering_patience;
    }

    public void TakeOrder()
    {
        Waiting();
    }

    void Waiting()
    {
        state = STATE.WAITING;
        timer = serving_patience;
    }

    void GiveOrder()
    {
        Eating();
    }

    void Eating()
    {
        state = STATE.EATING;
        timer = eating_duration;
    }

    void Leaving()
    {
        state = STATE.LEAVING;
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

            curr_pos = CustomerManager.UpdateGridPosition(curr_pos, next_move_pos, id);
        }

        //Astar
        next_move_pos = CustomerManager.Astar(curr_pos, target_pos, id);
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0 && (timer -= Time.deltaTime) < 0)
        {
            Leaving();
        }

        if (target_pos != new Vector2Int(-1, -1))
        {
            if (curr_pos == target_pos)
            {
                if (state == STATE.SEATING)
                {
                    Ordering();
                }
                else if (state == STATE.LEAVING)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Move();   
            }
        }
    }
}
