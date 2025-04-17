using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Diagnostics;  // Ajoute cette ligne pour utiliser Stopwatch

public class CalculatePathButton : MonoBehaviour
{
    public Material optimalPathMaterial;
    public Material AStarPathMaterial;
    private PlanetConnections planetConnections;
    public TextMeshProUGUI totalDistanceLabel; // Le champ UI pour afficher la distance totale

    public TextMeshProUGUI tempsExec; 

    public TextMeshProUGUI distanceDetailsLabel; // Champ pour le détail
    public Image EasterImage;          // Image du cockpit (AFK ou en activité)
    public Sprite updateEaster;

    void Start()
    {
        planetConnections = FindObjectOfType<PlanetConnections>();
    }

    public void CalculatePath()
    {
        if (PlanetClick.startPoint == null || PlanetClick.endPoint == null)
        {
            UnityEngine.Debug.LogWarning("Sélectionnez une planète de départ et une d'arrivée.");
            return;
        }

        EasterImage.sprite = updateEaster;
        planetConnections.HideAllConnections();
        planetConnections.HideAllDistanceLabels();

        Dictionary<GameObject, List<PlanetConnections.PlanetConnection>> graph = planetConnections.GetPlanetGraph();

        // Créer un Stopwatch pour mesurer les temps d'exécution
        Stopwatch stopwatchDijkstra = new Stopwatch();
        Stopwatch stopwatchAStar = new Stopwatch();

        // Exécution de l'algorithme Dijkstra
        stopwatchDijkstra.Start();
        DijkstraAlgorithm dijkstra = new DijkstraAlgorithm(graph);
        List<GameObject> dijkstraPath = dijkstra.FindShortestPath(PlanetClick.startPoint, PlanetClick.endPoint);
        stopwatchDijkstra.Stop();

        // Exécution de l'algorithme A*
        stopwatchAStar.Start();
        AStarAlgorithm aStar = new AStarAlgorithm(graph);
        List<GameObject> aStarPath = aStar.FindShortestPath(PlanetClick.startPoint, PlanetClick.endPoint);
        stopwatchAStar.Stop();

        tempsExec.text ="Temps d'exécution de Dijkstra: " + stopwatchDijkstra.ElapsedMilliseconds + " ms\n"+"Temps d'exécution de AStar: " + stopwatchAStar.ElapsedMilliseconds + " ms";
        UnityEngine.Debug.Log("Temps d'exécution de Dijkstra: " + stopwatchDijkstra.ElapsedMilliseconds + " ms");
        UnityEngine.Debug.Log("Temps d'exécution de AStar: " + stopwatchAStar.ElapsedMilliseconds + " ms");

        bool pathsAreIdentical = dijkstraPath.Count == aStarPath.Count;

        if (pathsAreIdentical)
        {
            for (int i = 0; i < dijkstraPath.Count; i++)
            {
                if (dijkstraPath[i] != aStarPath[i])
                {
                    pathsAreIdentical = false;
                    break;
                }
            }
        }

        int nbPlanetsDijkstra = 1;
        string detailDijkstra = "";
        float totalDistanceDijkstra = ShowPathWithMaterial(dijkstraPath, optimalPathMaterial, ref nbPlanetsDijkstra, ref detailDijkstra);

        // Uniquement si les chemins sont différents
        if (!pathsAreIdentical)
        {
            int nbPlanetsAStar = 1;
            string detailAStar = "";
            float totalDistanceAStar = ShowPathWithMaterial(aStarPath, AStarPathMaterial, ref nbPlanetsAStar, ref detailAStar);

            detailDijkstra += "\nA* → " + detailAStar;
        }

        // Affichage des labels
        totalDistanceLabel.text = "Distance : " + totalDistanceDijkstra.ToString("F1") + " AL";
        detailDijkstra += " AL - Planètes traversées : " + nbPlanetsDijkstra;
        distanceDetailsLabel.text = detailDijkstra;
    }

    private float ShowPathWithMaterial(
        List<GameObject> path,
        Material material,
        ref int nbPlanets,
        ref string detailText)
    {
        float total = 0f;

        if (path == null || path.Count < 2)
            return 0f;

        for (int i = 0; i < path.Count - 1; i++)
        {
            GameObject a = path[i];
            GameObject b = path[i + 1];

            foreach (var line in planetConnections.GetAllLines())
            {
                if (line.TryGetComponent<LineRenderer>(out var lr))
                {
                    Vector3 pos1 = lr.GetPosition(0);
                    Vector3 pos2 = lr.GetPosition(1);

                    bool match =
                        (ApproximatelyEqual(pos1, a.transform.position) && ApproximatelyEqual(pos2, b.transform.position)) ||
                        (ApproximatelyEqual(pos1, b.transform.position) && ApproximatelyEqual(pos2, a.transform.position));

                    if (match)
                    {
                        lr.enabled = true;
                        lr.material = material;
                        planetConnections.ShowLabelForConnection(a, b);

                        PlanetConnections.PlanetConnection connection = FindConnection(a, b);
                        if (connection != null)
                        {
                            float distance = connection.distance;
                            total += distance;
                            nbPlanets++;
                            detailText += $"{distance:F1} ";
                        }

                        break;
                    }
                }
            }
        }

        return total;
    }

    private bool ApproximatelyEqual(Vector3 a, Vector3 b, float tolerance = 0.1f)
    {
        return Vector3.Distance(a, b) < tolerance;
    }

    private PlanetConnections.PlanetConnection FindConnection(GameObject planet1, GameObject planet2)
    {
        foreach (var connection in planetConnections.planetConnections)
        {
            if ((connection.planet1 == planet1 && connection.planet2 == planet2) ||
                (connection.planet1 == planet2 && connection.planet2 == planet1))
            {
                return connection;
            }
        }
        return null;
    }
}
