using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [Header("Customer Settings")]
    public GameObject customer_prefab; // changes to non static so we can drag in
    public static float spawn_delay = 1.5F;
    static float timer;

    /*
    1 are walls
    -1 are seats
    -2 are entrances
    */
    static int[,] grid;
    static int[,] start_grid;

    [Header("Grid Settings")]

    static readonly Vector2 grid_offset = Vector2.zero; // Bottom Left Corner
    static readonly int grid_square_size = 1;
    static readonly int padding = 2;    


    static List<Vector2Int> seats = new();
    static readonly List<Vector2Int> entrances = new();
    static List<Vector2Int> spawn_locations = new();


    static int id_count = 2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateGrid();
        ParseGrid();
    }

    void SpawnCustomer()
    {
        Vector2Int new_spawn_pos = FindSpawnLocation();
        Vector2Int seat_target = FindSeat();
        if (new_spawn_pos == new Vector2Int(-1, -1) || seat_target == new Vector2Int(-1, -1))
        {
            return;
        }

        GameObject new_customer = Instantiate(customer_prefab);
        Customer new_customer_script = new_customer.GetComponent<Customer>();

        int new_id = id_count++;

        grid[new_spawn_pos.x, new_spawn_pos.y] = new_id;
        new_customer.transform.position = GridToWorld(new_spawn_pos);
        new_customer_script.Activate(new_id, new_spawn_pos, seat_target);
    }

    Vector2Int FindSpawnLocation()
    {
        spawn_locations = spawn_locations.OrderBy(x => UnityEngine.Random.value).ToList();

        for (int i = 0; i < spawn_locations.Count(); i++)
        {
            Vector2Int pos = spawn_locations[i];
            if (grid[pos.x, pos.y] == 0)
            {
                return pos;
            }
        }

        return new Vector2Int(-1, -1);
    }

    Vector2Int FindSeat()
    {
        if (seats.Count == 0)
        {
            // Wait for seat TODO
            Debug.LogWarning("No seats.");
            return new Vector2Int(-1, -1);
        }

        Vector2Int ans = seats[UnityEngine.Random.Range(0, seats.Count)];
        seats.Remove(ans);
        return ans;
    }

    public static void ReturnSeat(Vector2Int pos)
    {
        seats.Add(pos);
    }

    public static Vector2Int FindEntrance()
    {
        return entrances[UnityEngine.Random.Range(0, entrances.Count)];
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawn_delay)
        {
            SpawnCustomer();
            timer = 0;
        }
    }

    public static Vector2 GridToWorld(Vector2Int pos)
    {
        return (pos * grid_square_size) + grid_offset + Vector2.one * (grid_square_size / 2F);
    }

    // TODO
    public static Vector2Int WorldToGrid(Vector2 pos)
    {
        Vector2 swp = pos - (grid_offset + Vector2.one * (grid_square_size / 2F));
        return new Vector2Int((int)swp.x, (int)swp.y) / grid_square_size;
    }

    public static void SetSquare(Vector2Int pos, int curr_id)
    {
        grid[pos.x, pos.y] = curr_id;
    }
    public static int GetSquare(Vector2Int pos)
    {
        return grid[pos.x, pos.y];
    }
    public static void ResetSquare(Vector2Int curr_pos)
    {
        grid[curr_pos.x, curr_pos.y] = start_grid[curr_pos.x, curr_pos.y];
    }

    public static Vector2Int Astar(Vector2Int start, Vector2Int target, int id, Vector2Int target_pos)
    {
        if (start == target)
        {
            return target;
        }

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

        Dictionary<Vector2Int, Vector2Int> parent_lookup = new() { { start, start } };
        Dictionary<Vector2Int, float> path_length = new() { { start, 0 } };
        SortedSet<Tuple<Vector2Int, float>> heap = new(Comparer<Tuple<Vector2Int, float>>.Create(sort_func)) { new Tuple<Vector2Int, float>(start, 0) };

        while (heap.Count != 0)
        {
            Tuple<Vector2Int, float> curr = heap.First();
            heap.Remove(curr);

            if (curr.Item1 == target) //Reach end
            {
                break;
            }

            // Add all 8
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    //already exists
                    Vector2Int child = new Vector2Int(i, j) + curr.Item1;
                    if (parent_lookup.ContainsKey(child))
                    {
                        continue;
                    }

                    //fail cases
                    if (child.x < 0 || child.y < 0 || child.x >= grid.GetLength(0) || child.y >= grid.GetLength(1))
                    {
                        continue;
                    }

                    //obstacles or higher priority customers
                    if (grid[child.x, child.y] > 0 && grid[child.x, child.y] < id)
                    {
                        continue;
                    }

                    //certain entrances should be avoided only if we are leaving
                    if (start_grid[child.x, child.y] == start_grid[target_pos.x,target_pos.y] && child != target_pos)
                    {
                        continue;
                    }
                    
                    //seats should not be crossed unless it's the customer's designated seat
                    if (start_grid[child.x,child.y] == -1 && child != target_pos)
                    {
                        continue;
                    }

                    //diagonal check
                    if (i != 0 && j != 0)
                    {
                        int block_1 = grid[curr.Item1.x, child.y];
                        int block_2 = grid[child.x, curr.Item1.y];
                        //failed diagonal
                        if (block_1 > 0 && block_1 < id && block_2 > 0 && block_2 < id)
                        {
                            continue;
                        }
                    }

                    path_length[child] = path_length[curr.Item1] + 1;
                    parent_lookup[child] = curr.Item1;
                    heap.Add(new(child, path_length[child] + (target - child).magnitude));
                }
            }
        }

        List<Vector2Int> path = new();
        Vector2Int backtrack = target;
        if (!parent_lookup.ContainsKey(target))
        {
            Debug.Log("No path found");
            return start;
        }

        while (backtrack != start)
        {
            path.Add(backtrack);
            backtrack = parent_lookup[backtrack];
        }

        Debug.Log(path[path.Count - 1]);

        return path[path.Count - 1];
    }

    static void CreateGrid()
    {
        int[,] data = {
            {1, 1, 1, -2, -2, -2, -2, -2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
        };

        grid = new int[data.GetLength(0) + 2 * padding, data.GetLength(1) + 2 * padding];
        start_grid = new int[data.GetLength(0) + 2 * padding, data.GetLength(1) + 2 * padding];

        for (int i = 0; i < data.GetLength(0); i++)
        {
            for (int j = 0; j < data.GetLength(1); j++)
            {
                grid[i + padding, j + padding] = data[i, j];
                start_grid[i + padding, j + padding] = data[i, j];
            }
        }

    }

    static void ParseGrid()
    {
        for (int x = 0; x < grid.GetLength(0); x++) //Rows
        {
            for (int y = 0; y < grid.GetLength(1); y++) //Cols
            {
                Vector2Int pos = new(x, y);
                if (grid[x, y] == -1) //Mark seats
                {
                    seats.Add(pos);
                }

                if (grid[x, y] == -2)
                {
                    entrances.Add(pos);
                }

                if ((x < padding || x >= (grid.GetLength(0) - padding)) && (y < padding || y >= (grid.GetLength(1) - padding)))
                {
                    spawn_locations.Add(pos);
                }
            }
        }
    }

    void print_grid(int[,] grid)
    {
        string s = "";
        for (int i = grid.GetLength(1) - 1; i >= 0; i--)
        {
            for (int j = 0; j < grid.GetLength(0); j++)
            {
                s += grid[j, i] + "\t";
            }

            s += "\n";
        }

        Debug.Log(s);
    }


    void OnDrawGizmos()
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j] == -2)
                {
                    Gizmos.color = Color.red;
                }
                if (grid[i, j] == -1) //Mark seats
                {
                    Gizmos.color = Color.blue;
                }
                if (grid[i, j] == 0) //Mark seats
                {
                    Gizmos.color = Color.white;
                }
                if (grid[i, j] == 1)
                {
                    Gizmos.color = Color.black;
                }
                // if ((i < padding || i >= (grid.GetLength(0) - padding)) && (j < padding || j >= (grid.GetLength(1) - padding)))
                // {
                //     Gizmos.color = Color.grey;
                // }
                if (grid[i, j] >= 2)
                {
                    Gizmos.color = Color.green;
                }
                Gizmos.DrawWireCube(GridToWorld(new Vector2Int(i, j)), new Vector2(grid_square_size, grid_square_size) / 2);
            }
        }
    }
}
