using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    public PlanetConnections connectionsManager;

    private Dictionary<GameObject, List<(GameObject, float)>> graph = new();

    void Start()
    {
        BuildGraph();
    }

    void BuildGraph()
    {
        foreach (var connection in connectionsManager.planetConnections)
        {
            connection.CalculateDistance();

            if (!graph.ContainsKey(connection.planet1))
                graph[connection.planet1] = new();
            if (!graph.ContainsKey(connection.planet2))
                graph[connection.planet2] = new();

            graph[connection.planet1].Add((connection.planet2, connection.distance));
            graph[connection.planet2].Add((connection.planet1, connection.distance));
        }
    }

    public List<GameObject> FindShortestPath(GameObject start, GameObject end)
    {
        var distances = new Dictionary<GameObject, float>();
        var previous = new Dictionary<GameObject, GameObject>();
        var unvisited = new List<GameObject>();

        foreach (var node in graph.Keys)
        {
            distances[node] = float.MaxValue;
            unvisited.Add(node);
        }

        distances[start] = 0;

        while (unvisited.Count > 0)
        {
            unvisited.Sort((a, b) => distances[a].CompareTo(distances[b]));
            var current = unvisited[0];
            unvisited.RemoveAt(0);

            if (current == end) break;

            foreach (var (neighbor, cost) in graph[current])
            {
                float alt = distances[current] + cost;
                if (alt < distances[neighbor])
                {
                    distances[neighbor] = alt;
                    previous[neighbor] = current;
                }
            }
        }

        return ReconstructPath(previous, start, end);
    }

    List<GameObject> ReconstructPath(Dictionary<GameObject, GameObject> previous, GameObject start, GameObject end)
    {
        List<GameObject> path = new();
        GameObject current = end;

        while (current != start)
        {
            if (!previous.ContainsKey(current)) return new(); // pas de chemin
            path.Insert(0, current);
            current = previous[current];
        }

        path.Insert(0, start);
        return path;
    }
}
