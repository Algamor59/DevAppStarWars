using System.Collections.Generic;
using UnityEngine;

public class AStarAlgorithm
{
    private Dictionary<GameObject, List<PlanetConnections.PlanetConnection>> planetGraph;

    public AStarAlgorithm(Dictionary<GameObject, List<PlanetConnections.PlanetConnection>> planetGraph)
    {
        this.planetGraph = planetGraph;
    }

    public List<GameObject> FindShortestPath(GameObject startPlanet, GameObject targetPlanet)
    {
        var openSet = new HashSet<GameObject> { startPlanet };
        var cameFrom = new Dictionary<GameObject, GameObject>();

        var gScore = new Dictionary<GameObject, float>();
        var fScore = new Dictionary<GameObject, float>();

        foreach (var planet in planetGraph.Keys)
        {
            gScore[planet] = Mathf.Infinity;
            fScore[planet] = Mathf.Infinity;
        }

        gScore[startPlanet] = 0;
        fScore[startPlanet] = Heuristic(startPlanet, targetPlanet);

        while (openSet.Count > 0)
        {
            GameObject current = GetLowestFScore(openSet, fScore);
            if (current == targetPlanet)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);

            foreach (var connection in planetGraph[current])
            {
                GameObject neighbor = connection.planet1 == current ? connection.planet2 : connection.planet1;
                float tentativeGScore = gScore[current] + connection.distance;

                if (tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, targetPlanet);

                    openSet.Add(neighbor);
                }
            }
        }

        return new List<GameObject>(); // Aucun chemin trouvé
    }

    private float Heuristic(GameObject a, GameObject b)
    {
        return Vector3.Distance(a.transform.position, b.transform.position);
    }

    private GameObject GetLowestFScore(HashSet<GameObject> openSet, Dictionary<GameObject, float> fScore)
    {
        GameObject lowest = null;
        float lowestScore = Mathf.Infinity;

        foreach (var planet in openSet)
        {
            if (fScore[planet] < lowestScore)
            {
                lowestScore = fScore[planet];
                lowest = planet;
            }
        }

        return lowest;
    }

    private List<GameObject> ReconstructPath(Dictionary<GameObject, GameObject> cameFrom, GameObject current)
    {
        var path = new List<GameObject> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }
}
