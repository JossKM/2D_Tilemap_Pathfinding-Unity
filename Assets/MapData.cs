using UnityEngine;
using UnityEngine.Tilemaps;

public class MapData : MonoBehaviour
{
    private Tilemap map;

    public TileBase GetTile(int x, int y)
    {
        return map.GetTile(new Vector3Int(x, y, 0));
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Search the scene for any Tilemap object.
        //If there are more than one we will not know which one this is, so make sure there is only one.
        map = FindAnyObjectByType<Tilemap>();
    }

    public bool IsTraversible(int x, int y)
    {
        TileBase tile = GetTile(x, y);
        return tile != null;
    }

    // Update is called once per frame
    void Update()
    {
        BoundsInt boundaries = map.cellBounds;

        for (int x = boundaries.xMin; x < boundaries.xMax; x++)
        {
            for (int y = boundaries.yMin; y < boundaries.yMax; y++)
            {
                Vector3Int tileCoord = new Vector3Int(x, y, 0);

                TileBase tile = GetTile(x, y);

                if (tile != null)
                {
                    // Debug.Log(x + "," + y + " - " + tile.GetType().Name);

                    GameObject instantiated = map.GetInstantiatedObject(tileCoord);

                    if (instantiated != null)
                    {
                        instantiated.GetComponentInChildren<TMPro.TextMeshProUGUI> ().text = new Vector2Int(x, y).ToString();

                        instantiated.GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(0, 1, 0, 0.5f + Mathf.Sin(Time.time * 2) * 0.5f);
                    }

                    Vector3 centerPos = map.GetCellCenterWorld(new Vector3Int(x, y, 0));

                    //Up
                    if(IsTraversible(x, y + 1))
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
            }
        }
    }
}
