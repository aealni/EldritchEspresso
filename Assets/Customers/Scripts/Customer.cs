using System.Collections.Generic;
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
    [SerializeField] Vector2Int entrance_pos = new Vector2Int(-1, -1);
    [SerializeField] Vector2Int curr_pos = new Vector2Int(-1, -1);
    [SerializeField] Vector2Int target_pos = new Vector2Int(-1,-1);
    [SerializeField] Vector2Int next_move_pos = new Vector2Int(-1,-1);
    
    [Header("Grid")]

    [SerializeField] static int[,] grid = new int[20, 20]; // [vector.x, vector.y]
    [SerializeField] Vector2Int grid_offset = Vector2Int.zero; // Bottom Left Corner
    [SerializeField] int grid_square_size = 10;
    [SerializeField] Dictionary<Vector2Int, bool> seats = new();

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

    static int id_count = 0;

    [SerializeField] int curr_id;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid[entrance_pos.x, entrance_pos.y] = 0;
        curr_pos = entrance_pos;
        curr_id = id_count++;
        Seating();
    }

    void Seating()
    {
        state = STATE.SEATING;   
        List<Vector2Int> available_seats = new();
        foreach (KeyValuePair<Vector2Int, bool> seat in seats)
        {
            if (seat.Value)
            {
                available_seats.Add(seat.Key);
            }
        }

        target_pos = available_seats[Random.Range(0, available_seats.Count)];
        seats[target_pos] = false;
    }

    void Ordering()
    {
        state = STATE.ORDERING;
        order = menu[Random.Range(0, menu.Count)];
        timer = ordering_patience;
    }

    void Waiting()
    {
        state = STATE.WAITING;
        timer = serving_patience;
    }

    void Eating()
    {
        state = STATE.EATING;
        timer = eating_duration;
    }

    void Leaving()
    {
        state = STATE.LEAVING;
        target_pos = entrance_pos;
        seats[curr_pos] = true;
    }

    // TODO
    Vector2 GridToWorld(Vector2Int pos)
    {
        return (pos * grid_square_size) + grid_offset + Vector2.one * (grid_square_size / 2);
    }

    // TODO
    Vector2Int WorldToGrid(Vector2 pos)
    {
        Vector2 swp = pos - (grid_offset + Vector2.one * (grid_square_size / 2));
        return new Vector2Int((int)swp.x, (int)swp.y) / grid_square_size;
    }

    void UpdateGridPosition()
    {
        Vector2Int grid_pos = WorldToGrid(transform.position);
        grid[curr_pos.x, curr_pos.y] = 0;
        grid[grid_pos.x, grid_pos.y] = curr_id;

        curr_pos = grid_pos;
    }

    // TODO
    void Move()
    {
        if (next_move_pos != new Vector2Int(-1, 1))
        {
            Vector2.MoveTowards(transform.position, GridToWorld(next_move_pos), movement_speed * Time.deltaTime);
        }
        else
        {
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0 && (timer -= Time.deltaTime) > 0)
        {
            Leaving();
        }

        if (target_pos != new Vector2Int(-1, -1))
        {
            UpdateGridPosition();
            Move();
        }
    }
}
