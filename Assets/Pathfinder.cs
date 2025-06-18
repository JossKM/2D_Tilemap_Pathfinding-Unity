using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    struct DijkstraNodeData
    {
        public Vector2Int previous; // How we got here
        public float gCost; // Total cost to reach this node via Previous

        public DijkstraNodeData(Vector2Int previous, float gCost)
        {
            this.previous = previous;
            this.gCost = gCost;
        }
    }

    private TilemapGameLevel level;

    public Vector2Int start;
    public Vector2Int end;
    public Vector2Int current;
    public List<Vector2Int> solution;
    private float iterationDelay = 0.5f;

    private List<Vector2Int> unvisited;
    private List<Vector2Int> visited;
    private Dictionary<Vector2Int, DijkstraNodeData> nodes; // All known nodes

    public void UpdateBestWayToReachTile(Vector2Int origin, Vector2Int destination, float cost)
    {
        nodes[destination] = new DijkstraNodeData(origin, cost);
    }

    public void SetAlgorithmDebuggingDelay(float delay)
    {
        iterationDelay = delay;
    }

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
        if (nodes.ContainsKey(node))
        {
            return nodes[node].gCost;
        }
        else
        {
            return float.PositiveInfinity;
        }
    }

    /// <summary>
    /// Returns the cheapest node, and the total cost to reach it
    /// </summary>
    /// <returns></returns>
    public Tuple<Vector2Int, float> GetLowestCostInUnvisited()
    {
        //Edge bestEdge = new Edge(Vector2Int.zero, Vector2Int.zero, float.PositiveInfinity);
        Vector2Int bestNode = new Vector2Int(int.MaxValue, int.MaxValue);
        float bestCost = float.PositiveInfinity;

        foreach (Vector2Int node in unvisited)
        {
            if (nodes.ContainsKey(node))
            {
                if (nodes[node].gCost < bestCost)
                {
                    bestCost = nodes[node].gCost;
                    bestNode = node;
                }
            }
        }

        return new Tuple<Vector2Int, float>(bestNode, bestCost);
    }

    bool IsSolved()
    {
        return IsVisited(end);
    }

    bool IsComplete()
    {
        return IsSolved() || GetLowestCostInUnvisited().Item2 == float.PositiveInfinity;
    }

    void MoveToVisitedSet(Vector2Int node)
    {
        unvisited.Remove(node);
        visited.Add(node);
    }

    internal void FindPathDebugging()
    {
        StopAllCoroutines();
        StartCoroutine(DijkstraSearchCoroutine(start, end));
    }

    public IEnumerator DijkstraSearchCoroutine(Vector2Int origin, Vector2Int destination)
    {
        start = origin;
        end = destination;
        solution = new List<Vector2Int>();
        nodes = new Dictionary<Vector2Int, DijkstraNodeData> ();
        UpdateBestWayToReachTile(start, start, 0);
        unvisited = new List<Vector2Int> { start }; // Place Start in Unvisited set
        visited = new List<Vector2Int>(); // Visited set starts empty

        while (!IsComplete())
        {
            current = GetLowestCostInUnvisited().Item1;

            Debug.Log("Visiting: " + current +", cost: " + nodes[current].gCost);


            DebugDrawing.DrawCircle(level.GetTileCenter(current.x, current.y),  Quaternion.AngleAxis(90, Vector3.forward), 0.6f, 16, Color.yellow, iterationDelay, false);

            foreach (Vector2Int connected in level.GetTraversibleTilesAdjacentTo(current.x, current.y))
            {
                float costToReachConnected = nodes[current].gCost + level.GetCostToEnterTile(connected.x, connected.y);

                if(!IsDiscovered(connected)) // New node! Add it to Unvisited set
                {
                    //Newly discovered!
                    Debug.Log("Discovered: " + connected + ", cost: " + costToReachConnected);
                    unvisited.Add(connected);
                    UpdateBestWayToReachTile(current, connected, costToReachConnected);
                } else // We have already discovered this node, but do we have a better path to it?
                {
                    if (costToReachConnected < nodes[connected].gCost) //If this new route is better, then...
                    {
                        //replace because the new route is better
                        UpdateBestWayToReachTile(current, connected, costToReachConnected);
                    }
                }
            }

            //Finished exploring. Move the node to Visited
            MoveToVisitedSet(current);

          //  DebugDrawDiscoveredRoutes(delay);
            yield return new WaitForSeconds(iterationDelay);
        }

        //Complete!
        if(IsSolved())
        {
            Debug.Log("Dijkstra's Algorithm Successful!");
            GenerateSolution();
        } else
        {
            Debug.Log("Dijkstra's Algorithm Failed!");
        }
    }
    
    void GenerateSolution()
    {
        if(!IsSolved())
        {
            throw new Exception("Not solved! Cannot generate solution");
        }

        solution = new List<Vector2Int>();

        //work backwards from the end...
        Vector2Int currentNode = end;
        do
        {
            solution.Add(currentNode);
            currentNode = nodes[currentNode].previous;
        } while (currentNode != start);

        //We added all the elements backwards starting from the end. We should reverse it so it is forwards!
        solution.Reverse();
    }

    //Draw debugging info
    void OnDrawGizmos()
    {
        if (level != null)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.fontStyle = FontStyle.Bold;

            Vector3 startNodeWorldspace = level.GetTileCenter(start.x, start.y);
            Vector3 endNodeWorldspace = level.GetTileCenter(end.x, end.y);

            //Draw Start node and label
            style.normal.textColor = new Color(0.05f, 0.8f, 0.05f, 1.0f);
            Handles.Label(startNodeWorldspace + Vector3.up * 0.4f, "START", style);
            DebugDrawing.DrawCircle(startNodeWorldspace, Quaternion.AngleAxis(90, Vector3.forward), 0.8f, 8, Color.green, Time.deltaTime, false);

            //Draw End node and label
            style.normal.textColor = new Color(0.8f, 0.05f, 0.05f, 1.0f);
            Handles.Label(endNodeWorldspace + Vector3.up * 0.4f, "END", style);
            DebugDrawing.DrawCircle(endNodeWorldspace, Quaternion.AngleAxis(90, Vector3.forward), 0.8f, 5, Color.red, Time.deltaTime, false);

            if (nodes != null)
            {
                style.normal.textColor = new Color(0.05f, 0.05f, 0.05f, 1.0f);


                foreach (KeyValuePair<Vector2Int, DijkstraNodeData> pair in nodes)
                {
                    Vector2Int nodePos = pair.Key;
                    Vector2Int prev = pair.Value.previous;
                    float cost = pair.Value.gCost;

                    Vector3 nodePosWorldspace = level.GetTileCenter(nodePos.x, nodePos.y);
                    Vector3 prevNodePosWorldspace = level.GetTileCenter(prev.x, prev.y);

                    //Draw each edge found in the algorithm
                    Debug.DrawLine(prevNodePosWorldspace, nodePosWorldspace, Color.HSVToRGB(cost / 10, 1, 0.8f), Time.deltaTime);

                    //Label each node with its current G-Cost
                    Handles.Label(nodePosWorldspace + Vector3.up * 0.4f, cost.ToString("f0"), style);
                }


                if (IsSolved())
                {
                        //Draw solution path
                    foreach (var node in solution)
                    {
                        DebugDrawing.DrawCircle(level.GetTileCenter(node.x, node.y), Quaternion.AngleAxis(90, Vector3.forward), 0.4f, 3, Color.green, Time.deltaTime, false);
                    }
                }
            }
        }

    }

}
