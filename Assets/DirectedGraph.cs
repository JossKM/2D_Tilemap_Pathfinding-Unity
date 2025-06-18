using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For storing directed graph data. Does so in the form of an adjacency list
/// </summary>
//public class DirectedGraph
//{
//    public Dictionary<Vector2Int, List<System.Tuple<Vector2Int, float>>> adjacencies = new Dictionary<Vector2Int, List<System.Tuple<Vector2Int, float>>>();

//    public void AddEdge(Vector2Int start, Vector2Int end, float weight)
//    {
//        if(!adjacencies.ContainsKey(start))
//        {
//            adjacencies.Add(start, new List<System.Tuple<Vector2Int, float>>());
//        }

//        adjacencies[start].Add(new System.Tuple<Vector2Int, float>(end, weight));
//    }
//}

//public struct Edge
//{
//    public Vector2Int destination;
//    public float weight;

//    public Edge(Vector2Int destination, float weight)
//    {
//        this.destination = destination;
//        this.weight = weight;
//    }
//}

//public struct Vertex
//{
//    public Vertex(Vector2Int position, float value = 1)
//    {
//        this.position = position;
//    }

//    public Vertex(int x, int y, float value = 1)
//    {
//        this.position = new Vector2Int(x, y);
//    }

//    public Vector2Int position;
//}
