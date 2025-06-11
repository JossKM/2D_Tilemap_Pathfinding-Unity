using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class TilemapGameLevel : MonoBehaviour
{
    //The tilemap data to work on
    private Tilemap tilemap;

    // The tile asset reference to be used when spawning floor tiles
    public TileBase floorTile;

    // When randomly generating map, this is how many tiles it takes up
    public Vector2Int mapSizeTiles = new Vector2Int(10, 10);

    public UnityEvent onTilemapUpdated;

    // When randomly generating map, this is the probability that any tile will be a generated as a walkable floor tile
    public float floorSpawnThreshold = 0.75f;
    public float perlinScale = 0.1f;

    public enum RandomizationType
    { 
        Uniform,
        Perlin
    }

    public RandomizationType randomizationType = RandomizationType.Perlin;

    public TileBase GetTile(int x, int y)
    {
        return tilemap.GetTile(new Vector3Int(x, y, 0));
    }

    public Vector3 GetTileCenter(int x, int y)
    {
        return tilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));
    }

    public BoundsInt GetBounds()
    {
        return tilemap.cellBounds;
    }

    public float GetCostToEnterTile(int x, int y)
    {
        return 1;
    }

    public List<Vector2Int> GetTraversibleTilesAdjacentTo(int x, int y)
    {
        List<Vector2Int> adjacentTiles = new List<Vector2Int>();

        //Up
        if (IsTraversible(x, y + 1))
        {
            adjacentTiles.Add(new Vector2Int(x, y + 1));
        }
        //Down
        if (IsTraversible(x, y - 1))
        {
            adjacentTiles.Add(new Vector2Int(x, y - 1));
        }
        //Left
        if (IsTraversible(x - 1, y))
        {
            adjacentTiles.Add(new Vector2Int(x - 1, y));
        }
        //Right
        if (IsTraversible(x + 1, y))
        {
            adjacentTiles.Add(new Vector2Int(x + 1, y));
        }

        return adjacentTiles;
    }

    public void SetChanceToSpawnFloor(float chance)
    {
        floorSpawnThreshold = chance;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Search the scene for any Tilemap object.
        //If there are more than one we will not know which one this is, so make sure there is only one.
        tilemap = FindAnyObjectByType<Tilemap>();

        GenerateMap();
    }

    public bool IsTraversible(int x, int y)
    {
        TileBase tile = GetTile(x, y);
        return tile != null;
    }

    public void GenerateMap()
    {
        GenerateMap(tilemap, mapSizeTiles.x, mapSizeTiles.y);
    }

    public void GenerateMap(Tilemap map, int width, int height)
    {
        map.ClearAllTiles();
        //BoundsInt boundaries = map.cellBounds;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (TileRandomizationFunction(x,y) <= floorSpawnThreshold)
                {
                    map.SetTile(new Vector3Int(x, y, 0), floorTile);
                }
            }
        }

        onTilemapUpdated.Invoke();
    }

    public float TileRandomizationFunction(int x, int y)
    {
        switch (randomizationType)
        { 
        case(RandomizationType.Uniform): return Random.value;
        case (RandomizationType.Perlin): return Mathf.PerlinNoise(Time.time + 1 + (x * perlinScale), y * perlinScale);
        }

        return 0;
    }

    //Draw connections to a particular tile
    public void DebugDrawTileConnections(int x, int y)
    {
        TileBase tile = GetTile(x, y);

        if (tile == null)
        {
            throw new System.Exception("Tried to draw connections to a null tile at: " + x + ", " + y);
        }

        Vector3 centerPos = tilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));
        //Up
        if (IsTraversible(x, y + 1))
        {
            Debug.DrawRay(centerPos, Vector3.up * 0.4f, Color.green);
        }
        //Down
        if (IsTraversible(x, y - 1))
        {
            Debug.DrawRay(centerPos, Vector3.up * -0.4f, Color.green);
        }
        //Left
        if (IsTraversible(x - 1, y))
        {
            Debug.DrawRay(centerPos, Vector3.right * -0.4f, Color.green);
        }
        //Right
        if (IsTraversible(x + 1, y))
        {
            Debug.DrawRay(centerPos, Vector3.right * 0.4f, Color.green);
        }
    }

    public void DebugDrawTileInfo(int x, int y)
    {
        //Get the tile at this coordinate
        TileBase tile = GetTile(x, y);

        //Some methods require Vector3Int. We will use 0 for z and just work on the x-y plane
        Vector3Int tileCoord = new Vector3Int(x, y, 0);
        if (tile != null)
        {
            // Debug.Log(x + "," + y + " - " + tile.GetType().Name);

            //Due to the Asset settings, GameObject prefabs are generated along with the tiles. The prefab used is "Assets/LabelCanvas" 
            //We can use references to the generated GameObject for the current tile to draw relevant information on it or change its color etc.
            GameObject instantiated = tilemap.GetInstantiatedObject(tileCoord);

            if (instantiated != null)
            {
                //Draw coordinates
                instantiated.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = new Vector2Int(x, y).ToString();
             //   instantiated.GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(0, 1, 0, 0.5f + Mathf.Sin(Time.time * 2) * 0.5f);
            }
        }
    }

    void Update()
    {
        // Get the current dimensions of the tilemap
        BoundsInt boundaries = tilemap.cellBounds;

        //Iterate through each coordinate, by column, then by row, starting from 0,0 then 0,1, then 0,2 etc.
        for (int x = boundaries.xMin; x < boundaries.xMax; x++)
        {
            for (int y = boundaries.yMin; y < boundaries.yMax; y++)
            {
                TileBase tile = GetTile(x, y);
                if (tile == null)
                {
                    continue;
                }

               // DebugDrawTileConnections(x, y);
                //DebugDrawTileInfo(x, y);
            }
        }
    }
}
