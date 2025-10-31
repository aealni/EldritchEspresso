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
    [SerializeField] List<Food> menu;
    [SerializeField] Food order;

    [SerializeField] int id;

    public void Activate(int new_id, Vector2Int spawn_pos, Vector2Int seat)
    {
        id = new_id;
        curr_pos = spawn_pos;
        target_pos = seat;

        state = STATE.SEATING;

        sprite_renderer = GetComponent<SpriteRenderer>();
    }

    void Ordering()
    {
        state = STATE.ORDERING;

        if (menu.Count == 0)
        {
            // Wait for seat TODO
            // Debug.LogError("No items on menu!");
        }
        else
        {
            order = menu[UnityEngine.Random.Range(0, menu.Count)];
        }
        
        timer = ordering_patience;
    }

    public Food TakeOrder()
    {
        if (state != STATE.ORDERING)
        {
            return null;
        }
        state = STATE.WAITING;
        timer = serving_patience;
        return order;
    }

    public bool GiveOrder(Food f)
    {
        if (f == order && state == STATE.WAITING)
        {
            state = STATE.EATING;
            timer = eating_duration;
            return true;
        }

        return false;
    }
    void Leaving()
    {
        state = STATE.LEAVING;
        CustomerManager.ReturnSeat(curr_pos);
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
                    return;
                }
            }

            Move();
        }
    }
}
