using System.Collections.Generic;
using UnityEngine;

public class DijkstraAlgorithm
{
    private Dictionary<GameObject, List<PlanetConnections.PlanetConnection>> planetGraph;

    public DijkstraAlgorithm(Dictionary<GameObject, List<PlanetConnections.PlanetConnection>> planetGraph)
    {
        this.planetGraph = planetGraph;
    }

    public List<GameObject> FindShortestPath(GameObject startPlanet, GameObject targetPlanet)
    {
        // Dictionnaires pour stocker les distances et les parents des planètes
        Dictionary<GameObject, float> distances = new Dictionary<GameObject, float>();
        Dictionary<GameObject, GameObject> previousPlanets = new Dictionary<GameObject, GameObject>();
        List<GameObject> unvisited = new List<GameObject>();

        // Initialiser les distances des planètes à l'infini
        foreach (var planet in planetGraph.Keys)
        {
            distances[planet] = Mathf.Infinity;
            unvisited.Add(planet);
        }
        distances[startPlanet] = 0;

        // Exécution de l'algorithme de Dijkstra
        while (unvisited.Count > 0)
        {
            // Trouver la planète avec la distance la plus faible
            GameObject currentPlanet = GetPlanetWithMinDistance(unvisited, distances);

            // Si la planète cible est atteinte, on peut arrêter l'algorithme
            if (currentPlanet == targetPlanet)
                break;

            unvisited.Remove(currentPlanet);

            // Explorer toutes les connexions du nœud courant
            foreach (var connection in planetGraph[currentPlanet])
            {
                GameObject neighbor = connection.planet1 == currentPlanet ? connection.planet2 : connection.planet1;
                float newDist = distances[currentPlanet] + connection.distance;

                // Si le nouveau chemin est plus court, mettre à jour la distance et le parent
                if (newDist < distances[neighbor])
                {
                    distances[neighbor] = newDist;
                    previousPlanets[neighbor] = currentPlanet;
                }
            }
        }

        // Reconstituer le chemin
        List<GameObject> path = new List<GameObject>();
        GameObject current = targetPlanet;
        while (current != null)
        {
            path.Add(current);
            previousPlanets.TryGetValue(current, out current);
        }
        path.Reverse(); // Le chemin est reconstitué à l'envers

        return path;
    }

    private GameObject GetPlanetWithMinDistance(List<GameObject> unvisited, Dictionary<GameObject, float> distances)
    {
        GameObject minPlanet = null;
        float minDistance = Mathf.Infinity;

        foreach (var planet in unvisited)
        {
            if (distances[planet] < minDistance)
            {
                minDistance = distances[planet];
                minPlanet = planet;
            }
        }

        return minPlanet;
    }
}
