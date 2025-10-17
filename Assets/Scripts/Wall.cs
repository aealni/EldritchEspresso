using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Wall : MonoBehaviour
{
    [Header("Wall Settings")]
    public GameObject wallPrefab; // Assign your wall sprite prefab
    public bool tileTop = true;
    public bool tileBottom = true;
    public bool tileLeft = true;
    public bool tileRight = true;

    [Header("Door Settings")]
    public bool doorOnTop = false;
    public bool doorOnBottom = true;
    public bool doorOnLeft = false;
    public bool doorOnRight = false;
    public int doorWidthTiles = 2;  // How many tile spaces to leave
    public float zPosition = 0f;

    [Header("Auto Generate On Play")]
    public bool generateOnStart = true;

    private void Start()
    {
        if (generateOnStart)
            TileWalls();
    }

#if UNITY_EDITOR
    [ContextMenu("Tile Walls (Editor)")]
#endif
    public void TileWalls()
    {
        if (wallPrefab == null)
        {
            Debug.LogWarning("Assign a wall prefab!");
            return;
        }

        // Delete existing children
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
        else
#endif
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("No main camera found.");
            return;
        }

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float left = cam.transform.position.x - camWidth / 2;
        float right = cam.transform.position.x + camWidth / 2;
        float top = cam.transform.position.y + camHeight / 2;
        float bottom = cam.transform.position.y - camHeight / 2;

        SpriteRenderer sr = wallPrefab.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning("Wall prefab must have a SpriteRenderer!");
            return;
        }

        float tileWidth = sr.bounds.size.x;
        float tileHeight = sr.bounds.size.y;

        // Door spacing
        float doorWidth = doorWidthTiles * tileWidth;
        float doorHeight = doorWidthTiles * tileHeight;

        // === Tile Bottom ===
        if (tileBottom)
        {
            for (float x = left; x <= right; x += tileWidth)
            {
                if (doorOnBottom && Mathf.Abs(x - cam.transform.position.x) < doorWidth / 2)
                    continue;
                InstantiateWall(x + tileWidth / 2, bottom + tileHeight / 2);
            }
        }

        // === Tile Top ===
        if (tileTop)
        {
            for (float x = left; x <= right; x += tileWidth)
            {
                if (doorOnTop && Mathf.Abs(x - cam.transform.position.x) < doorWidth / 2)
                    continue;
                InstantiateWall(x + tileWidth / 2, top - tileHeight / 2);
            }
        }

        // === Tile Left ===
        if (tileLeft)
        {
            for (float y = bottom; y <= top; y += tileHeight)
            {
                if (doorOnLeft && Mathf.Abs(y - cam.transform.position.y) < doorHeight / 2)
                    continue;
                InstantiateWall(left + tileWidth / 2, y + tileHeight / 2);
            }
        }

        // === Tile Right ===
        if (tileRight)
        {
            for (float y = bottom; y <= top; y += tileHeight)
            {
                if (doorOnRight && Mathf.Abs(y - cam.transform.position.y) < doorHeight / 2)
                    continue;
                InstantiateWall(right - tileWidth / 2, y + tileHeight / 2);
            }
        }
    }

    private void InstantiateWall(float x, float y)
    {
        GameObject go;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            go = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab);
        else
#endif
            go = Instantiate(wallPrefab);

        go.transform.position = new Vector3(x, y, zPosition);
        go.transform.parent = transform;
        go.name = wallPrefab.name;
    }
}
