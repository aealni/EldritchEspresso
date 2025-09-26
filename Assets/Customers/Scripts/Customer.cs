using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    // [x, y]
    // 0 is empty
    // -1 is seats
    // 1 are obstacles
    // else: id of customer
    [SerializeField]
    static int[,] grid = {
        {0, 1, 0, 0, 0, 0, 0},
        {0, 1, 0, 1, 0, 0, 0},
        {0, 1, 0, 1, 0, 0, 0},
        {0, 1, 0, 1, 0, 0, 0},
        {0, 1, 0, 1, 0, 0, 0},
        {0, 0, 0, 1, 0, 0, -1}
    };

    [SerializeField] Vector2Int grid_offset = Vector2Int.zero; // Bottom Left Corner
    [SerializeField] int grid_square_size = 1;

    //true = available, false = not 
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

    static int id_count = 2;

    [SerializeField] int curr_id;

    void Start()
    {
        // Entrance needs to be passed in before init of customer currently
        grid[entrance_pos.x, entrance_pos.y] = 0;
        curr_pos = entrance_pos;
        curr_id = id_count++;
        transform.position = GridToWorld(curr_pos);
        
        ParseGrid();
        Seating();
    }

    void ParseGrid()
    {
        for (int x = 0; x < grid.GetLength(0); x++) //Rows
        {
            for (int y = 0; y < grid.GetLength(1); y++) //Cols
            {
                if (grid[x, y] == -1) //Mark seats
                {
                    seats.Add(new Vector2Int(x, y), true);
                }
            }
        }
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

        if (available_seats.Count == 0)
        {
            // Wait for seat TODO
            Debug.LogError("No seats. Implement Waiting for seats.");
        }

        target_pos = available_seats[UnityEngine.Random.Range(0, available_seats.Count)];
        seats[target_pos] = false;
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

    void TakeOrder()
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
        target_pos = entrance_pos;
        seats[curr_pos] = true;
    }

    // TODO
    Vector2 GridToWorld(Vector2Int pos)
    {
        return (pos * grid_square_size) + grid_offset + Vector2.one * (grid_square_size / 2F);
    }

    // TODO
    Vector2Int WorldToGrid(Vector2 pos)
    {
        Vector2 swp = pos - (grid_offset + Vector2.one * (grid_square_size / 2F));
        return new Vector2Int((int)swp.x, (int)swp.y) / grid_square_size;
    }

    // TODo
    void UpdateGridPosition()
    {
        Vector2Int grid_pos = WorldToGrid(transform.position);

        // Edits the grid wrong currently. Messes up placed obstacles.
        // grid[curr_pos.x, curr_pos.y] = 0;

        
        // grid[grid_pos.x, grid_pos.y] = curr_id;
        curr_pos = grid_pos;

    }

    // TODO
    void Move()
    {
        if (next_move_pos != new Vector2Int(-1, -1))
        {
            transform.position = Vector2.MoveTowards(transform.position, GridToWorld(next_move_pos), movement_speed * Time.deltaTime);
            if ((GridToWorld(next_move_pos) - (Vector2)transform.position).magnitude > 0.01)
            {
                return;
            }
        }
        //Astar
        next_move_pos = Astar();
    }

    Vector2Int Astar()
    {
        int sort_func(Tuple<Vector2Int, float> a, Tuple<Vector2Int, float> b)
        {
            int x1 = a.Item2.CompareTo(b.Item2);
            int x2 = a.Item1.x.CompareTo(b.Item1.x);
            int x3 = a.Item1.y.CompareTo(b.Item1.y);

            if (x1 == 0)
            {
                if (x2 == 0)
                {
                    return x3;
                }

                return x2;
            }

            return x1;
        }

        Dictionary<Vector2Int, Vector2Int> parent_lookup = new(){ {curr_pos, curr_pos } };
        Dictionary<Vector2Int, float> path_length = new() { {curr_pos, 0 } };
        SortedSet<Tuple<Vector2Int, float>> heap = new(Comparer<Tuple<Vector2Int, float>>.Create(sort_func)) { new Tuple<Vector2Int, float>(curr_pos, 0) };

        while (heap.Count != 0)
        {
            Tuple<Vector2Int, float> curr = heap.First();
            heap.Remove(curr);

            if (curr.Item1 == target_pos) //Reach end
            {
                break;
            }

            // Add all 8
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    Vector2Int child = new Vector2Int(i, j) + curr.Item1;
                    if (parent_lookup.ContainsKey(child))
                    {
                        continue;
                    }
                    if (child.x < 0 || child.y < 0 || child.x >= grid.GetLength(0) || child.y >= grid.GetLength(1) || (grid[child.x,child.y] > 0 && grid[child.x,child.y] < curr_id))
                    {
                        continue;
                    }

                    path_length[child] = path_length[curr.Item1] + 1;
                    parent_lookup[child] = curr.Item1;
                    heap.Add(new(child, path_length[child] + (target_pos - child).magnitude));
                }
            }
        }

        List<Vector2Int> path = new();
        Vector2Int backtrack = target_pos;
        if (!parent_lookup.ContainsKey(target_pos))
        {
            Debug.Log("No path found");
            return new Vector2Int(-1,-1);
        }

        while (backtrack != curr_pos)
        {
            path.Add(backtrack);
            backtrack = parent_lookup[backtrack];
        }
        Debug.Log(path[path.Count - 1]);
        
        return path[path.Count - 1];
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
                target_pos = new Vector2Int(-1, -1);
                if (state == STATE.SEATING)
                {
                    Ordering();
                }
                else if (state == STATE.LEAVING)
                {
                    //Kill self
                }
            }
            Move();
            UpdateGridPosition();
        }
    }
}
