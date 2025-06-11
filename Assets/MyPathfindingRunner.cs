using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MyPathfindingRunner : MonoBehaviour
{
    public struct Edge
    {
        public Vector2Int origin;
        public Vector2Int destination;
        public float cost;

        public Edge(Vector2Int origin, Vector2Int destination, float cost)
        {
            this.origin = origin;
            this.destination = destination;
            this.cost = cost;
        }
    }

    //Graph representing the game level
   // DirectedGraph graph;

    private TilemapGameLevel level;

    private Vector2Int current;
    private List<Vector2Int> unvisited;
    private List<Vector2Int> visited;
    private Dictionary<Vector2Int, Edge> bestWayToReach = new Dictionary<Vector2Int, Edge>(); // Key is the destination node, value is the edge including previous node

    public void UpdateBestWayToReachTile(Vector2Int origin, Vector2Int destination, float cost)
    {
        if (bestWayToReach.ContainsKey(destination))
        {
            bestWayToReach[destination] = new Edge(origin, destination, cost);
        }
        else bestWayToReach.Add(destination, new Edge(origin, destination, cost));
;    }

    public bool IsDiscovered(Vector2Int node)
    {
        return unvisited.Contains(node) || visited.Contains(node);
    }

    public bool IsVisited(Vector2Int node)
    {
        return visited.Contains(node);  
    }

    public void Awake()
    {
        level = FindAnyObjectByType<TilemapGameLevel>();
    }

    public float GetTotalCostToReach(Vector2Int node)
    {
        if(bestWayToReach.ContainsKey(node))
        {
            return bestWayToReach[node].cost;
        } else
        {
            return float.PositiveInfinity;
        }
    }

    /// <summary>
    /// Returns Edge where Destination is the cheapest node, and Cost is the total cost to reach it
    /// </summary>
    /// <returns></returns>
    public Edge GetLowestCostInUnvisited()
    {
        Edge bestEdge = new Edge(Vector2Int.zero, Vector2Int.zero, float.PositiveInfinity);

        foreach (Vector2Int node in unvisited)
        {
            if(bestWayToReach.ContainsKey(node))
            {
                Edge edge = bestWayToReach[node];
                if(edge.cost < bestEdge.cost)
                {
                    bestEdge = edge;
                }
            }
        }

        return bestEdge;
    }
    
    public void DijkstraIteration()
    {
        Edge currentEdge = GetLowestCostInUnvisited();
        current = currentEdge.destination;

        foreach (Vector2Int connected in level.GetTraversibleTilesAdjacentTo(current.x, current.y))
        {
            float costToReach = currentEdge.cost + level.GetCostToEnterTile(connected.x, connected.y);
            if (!IsDiscovered(connected)) // newly discovered! add to unvisited set
            {
                unvisited.Add(connected);
                UpdateBestWayToReachTile(connected, current, costToReach);
            }
            else
            {
                //check to update lowest cost
                if(bestWayToReach.ContainsKey(connected))
                {
                    Edge bestEdge = bestWayToReach[connected];
                    if (costToReach < bestEdge.cost)
                    {
                        //replace because the new route is better
                        UpdateBestWayToReachTile(current, connected, costToReach);
                    }
                } else
                {
                    //replace because it was not evaluated before
                    UpdateBestWayToReachTile(current, connected, costToReach);
                }
                
            }
        }

        unvisited.Remove(current);
        visited.Add(current);
    }

    public void DijkstraSearch(Vector2Int start, Vector2Int end)
    {
        UpdateBestWayToReachTile(start, start, 0);
        bool isComplete;

        bestWayToReach.Clear();
        bestWayToReach[start] = new Edge { origin = start, destination = start, cost = 0};
        unvisited = new List<Vector2Int> { start }; // Place Start in Unvisited set
        visited = new List<Vector2Int>(); // Visited set starts empty

        do
        {
            DijkstraIteration();

            //Draw all routes...
            foreach(Edge edge in bestWayToReach.Values)
            {
                Debug.DrawLine(level.GetTileCenter(edge.origin.x, edge.origin.y), level.GetTileCenter(edge.destination.x, edge.destination.y), Color.HSVToRGB(edge.cost / 10, 1, 0.8f));
            }

            //DebugDrawing.DrawCircle(level.GetTileCenter(start))


            isComplete = IsVisited(end) || GetLowestCostInUnvisited().cost == float.PositiveInfinity;
        } while (!isComplete);
    }

    public IEnumerator DijkstraSearchCoroutine(Vector2Int start, Vector2Int end)
    {
        UpdateBestWayToReachTile(start, start, 0);
        bool isComplete;

        bestWayToReach.Clear();
        bestWayToReach[start] = new Edge { origin = start, destination = start, cost = 0 };
        unvisited = new List<Vector2Int> { start }; // Place Start in Unvisited set
        visited = new List<Vector2Int>(); // Visited set starts empty

        do
        {
            DijkstraIteration();
            DebugDrawing.DrawCircle(level.GetTileCenter(current.x, current.y), 0.4f, 6, Color.magenta, Time.deltaTime, false);

            while (!Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }
            ///yield return new WaitForSeconds(0.5f);

            isComplete = IsVisited(end);
        } while (!isComplete);
    }

    internal void FindPathToDebug(Vector2Int start, Vector2Int end)
    {
        StartCoroutine(DijkstraSearchCoroutine(start, end));
    }

    ////From game level data, produce a graph representation of it.
    //public void GenerateGraph()
    //{
    //    BoundsInt boundaries = level.GetBounds();

    //    graph = new DirectedGraph();

    //    //Iterate through each coordinate, by column, then by row, starting from 0,0 then 0,1, then 0,2 etc.
    //    for (int x = boundaries.xMin; x < boundaries.xMax; x++)
    //    {
    //        for (int y = boundaries.yMin; y < boundaries.yMax; y++)
    //        {
    //            if (level.IsTraversible(x, y))
    //            {
    //                float costPerTile = Random.Range(0.1f, 1f);

    //                Vector2Int vertex = new Vector2Int(x, y);
    //                //Up
    //                if (level.IsTraversible(x, y + 1))
    //                {
    //                    graph.AddEdge(vertex, new Vector2Int(x, y + 1), costPerTile);
    //                }
    //                //Down
    //                if (level.IsTraversible(x, y - 1))
    //                {
    //                    graph.AddEdge(vertex, new Vector2Int(x, y - 1), costPerTile);
    //                }
    //                //Left
    //                if (level.IsTraversible(x - 1, y))
    //                {
    //                    graph.AddEdge(vertex, new Vector2Int(x - 1, y), costPerTile);
    //                }
    //                //Right
    //                if (level.IsTraversible(x + 1, y))
    //                {
    //                    graph.AddEdge(vertex, new Vector2Int(x + 1, y), costPerTile);
    //                }
    //            }
    //        }
    //    }

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    //void OnDrawGizmos()
    //{
    //    if (graph == null) return;

    //    foreach (var adjacencyList in graph.adjacencies)
    //    {
    //        Vector3 vertexPosWorldspace = level.GetTileCenter(adjacencyList.Key.x, adjacencyList.Key.y);
    //        //Handles.Label(vertexPosWorldspace, adjacencyList.Key.ToString());

    //        if (adjacencyList.Value == null) continue;

    //        foreach(var vertex in adjacencyList.Value)
    //        {
    //            Gizmos.color = Color.green * 0.8f;

    //            Vector3 connectedVertexPosWorldspace = level.GetTileCenter(vertex.Item1.x, vertex.Item1.y);
    //            Vector3 lineEndPoint = Vector3.Lerp(vertexPosWorldspace, connectedVertexPosWorldspace, 0.4f);

    //            Gizmos.DrawLine(vertexPosWorldspace, lineEndPoint);


    //            GUIStyle style = new GUIStyle();
    //            style.normal.textColor = Color.green * 0.5f;

    //            Vector3 quarterPoint = Vector3.Lerp(vertexPosWorldspace, connectedVertexPosWorldspace, 0.25f);
    //            Handles.Label(quarterPoint, vertex.Item2.ToString("f2"), style);
    //        }
    //    }
    //}
}
