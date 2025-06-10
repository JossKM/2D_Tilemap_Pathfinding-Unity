using UnityEditor;
using UnityEngine;

public class MyPathfindingRunner : MonoBehaviour
{
    //Graph representing the game level
    DirectedGraph graph;

    public TilemapGameLevel level;

    public void Awake()
    {
        level.onTilemapUpdated.AddListener(GenerateGraph);
    }

    //From game level data, produce a graph representation of it.
    public void GenerateGraph()
    {
        BoundsInt boundaries = level.GetBounds();

        graph = new DirectedGraph();

        //Iterate through each coordinate, by column, then by row, starting from 0,0 then 0,1, then 0,2 etc.
        for (int x = boundaries.xMin; x < boundaries.xMax; x++)
        {
            for (int y = boundaries.yMin; y < boundaries.yMax; y++)
            {
                if (level.IsTraversible(x, y))
                {
                    float costPerTile = Random.Range(0.1f, 1f);

                    Vector2Int vertex = new Vector2Int(x, y);
                    //Up
                    if (level.IsTraversible(x, y + 1))
                    {
                        graph.AddEdge(vertex, new Vector2Int(x, y + 1), costPerTile);
                    }
                    //Down
                    if (level.IsTraversible(x, y - 1))
                    {
                        graph.AddEdge(vertex, new Vector2Int(x, y - 1), costPerTile);
                    }
                    //Left
                    if (level.IsTraversible(x - 1, y))
                    {
                        graph.AddEdge(vertex, new Vector2Int(x - 1, y), costPerTile);
                    }
                    //Right
                    if (level.IsTraversible(x + 1, y))
                    {
                        graph.AddEdge(vertex, new Vector2Int(x + 1, y), costPerTile);
                    }
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        if (graph == null) return;

        foreach (var adjacencyList in graph.adjacencies)
        {
            Vector3 vertexPosWorldspace = level.GetTileCenter(adjacencyList.Key.x, adjacencyList.Key.y);
            //Handles.Label(vertexPosWorldspace, adjacencyList.Key.ToString());

            if (adjacencyList.Value == null) continue;

            foreach(var vertex in adjacencyList.Value)
            {
                Gizmos.color = Color.green * 0.8f;

                Vector3 connectedVertexPosWorldspace = level.GetTileCenter(vertex.Item1.x, vertex.Item1.y);
                Vector3 lineEndPoint = Vector3.Lerp(vertexPosWorldspace, connectedVertexPosWorldspace, 0.4f);

                Gizmos.DrawLine(vertexPosWorldspace, lineEndPoint);


                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.green * 0.5f;

                Vector3 quarterPoint = Vector3.Lerp(vertexPosWorldspace, connectedVertexPosWorldspace, 0.25f);
                Handles.Label(quarterPoint, vertex.Item2.ToString("f2"), style);
            }
        }
    }
}
