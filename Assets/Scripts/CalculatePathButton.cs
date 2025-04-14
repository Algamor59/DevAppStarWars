using System.Collections.Generic;
using UnityEngine;

public class CalculatePathButton : MonoBehaviour
{
    public Material optimalPathMaterial;
    private PlanetConnections planetConnections;

    void Start()
    {
        planetConnections = FindObjectOfType<PlanetConnections>();
    }

    public void CalculatePath()
    {
        if (PlanetClick.startPoint == null || PlanetClick.endPoint == null)
        {
            Debug.LogWarning("Sélectionnez une planète de départ et une d'arrivée.");
            return;
        }

        // Réinitialiser tous les chemins visuels
        planetConnections.HideAllConnections();

        // Créer l'algo et calculer le chemin optimal
        DijkstraAlgorithm algo = new DijkstraAlgorithm(planetConnections.GetPlanetGraph());
        List<GameObject> path = algo.FindShortestPath(PlanetClick.startPoint, PlanetClick.endPoint);

        if (path.Count < 2)
        {
            Debug.LogWarning("Chemin non trouvé.");
            return;
        }

        // Afficher le chemin optimal visuellement
        for (int i = 0; i < path.Count - 1; i++)
{
    GameObject a = path[i];
    GameObject b = path[i + 1];

    // Trouve la ligne qui correspond à ce lien
    foreach (var line in planetConnections.GetAllLines())
    {
        if (line.TryGetComponent<LineRenderer>(out var lr))
        {
            Vector3 pos1 = lr.GetPosition(0);
            Vector3 pos2 = lr.GetPosition(1);

            // Vérifie si les positions correspondent (dans un sens ou dans l’autre)
            bool match = 
                (ApproximatelyEqual(pos1, a.transform.position) && ApproximatelyEqual(pos2, b.transform.position)) ||
                (ApproximatelyEqual(pos1, b.transform.position) && ApproximatelyEqual(pos2, a.transform.position));

            if (match)
            {
                lr.enabled = true;
                lr.material = optimalPathMaterial;
                break;
            }
        }
    }
}


    }
    private bool ApproximatelyEqual(Vector3 a, Vector3 b, float tolerance = 0.1f)
{
    return Vector3.Distance(a, b) < tolerance;
}

}
