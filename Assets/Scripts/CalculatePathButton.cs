using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CalculatePathButton : MonoBehaviour
{
    public Material optimalPathMaterial;
    private PlanetConnections planetConnections;
    public TextMeshProUGUI totalDistanceLabel; // Le champ UI pour afficher la distance totale

    public TextMeshProUGUI distanceDetailsLabel; // Champ pour le détail

    public Image EasterImage;          // Image du cockpit (AFK ou en activité)
    public Sprite updateEaster;
    void Start()
    {
        planetConnections = FindObjectOfType<PlanetConnections>();
    }

    public void CalculatePath()
{int nbPlanets =1;

    if (PlanetClick.startPoint == null || PlanetClick.endPoint == null)
    {
        Debug.LogWarning("Sélectionnez une planète de départ et une d'arrivée.");
        return;
    }
    EasterImage.sprite = updateEaster;

    // Vérifier si les planètes sont dans le graphe
    Debug.Log($"Start point: {PlanetClick.startPoint.name}, End point: {PlanetClick.endPoint.name}");

    // Réinitialiser tous les chemins visuels et les labels de distance
    planetConnections.HideAllConnections();
    planetConnections.HideAllDistanceLabels();

    // Créer l'algo et calculer le chemin optimal
    DijkstraAlgorithm algo = new DijkstraAlgorithm(planetConnections.GetPlanetGraph());
    List<GameObject> path = algo.FindShortestPath(PlanetClick.startPoint, PlanetClick.endPoint);

    if (path.Count < 2)
    {
        Debug.LogWarning("Chemin non trouvé.");
        return;
    }

    float totalDistance = 0f;
    string detailText = "";

    // Afficher le chemin optimal visuellement
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
                    lr.material = optimalPathMaterial;
                    planetConnections.ShowLabelForConnection(a, b);

                    PlanetConnections.PlanetConnection connection = FindConnection(a, b);
                    if (connection == null)
                    {
                        Debug.LogWarning($"Aucune connexion trouvée entre {a.name} et {b.name}.");
                    }

                    float distance = connection != null ? connection.distance : 0f;
                    totalDistance += distance;
                    nbPlanets++;
                    // Ajouter au détail
                    detailText += distance.ToString("F1")+" ";

                    break;
                }
            }
        }
    }

    // Mise à jour des labels UI
    totalDistanceLabel.text = "Distance Totale: " + totalDistance.ToString("F1") + " Années Lumières";
    detailText+=  " AL " + "Nombre de planètes traversée : "+nbPlanets;
    distanceDetailsLabel.text = detailText;
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
