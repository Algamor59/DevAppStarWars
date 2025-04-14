using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlanetConnections : MonoBehaviour
{
    public List<PlanetConnection> planetConnections = new List<PlanetConnection>();
    public GameObject distanceLabelPrefab;
    public Material dottedLineMaterial;
    public Dictionary<GameObject, List<PlanetConnection>> planetGraph = new Dictionary<GameObject, List<PlanetConnection>>(); // Dictionnaire des connexions entre les planètes
    private List<GameObject> activeLines = new List<GameObject>();



    [System.Serializable]
    public class PlanetConnection
    {
        public GameObject planet1;
        public GameObject planet2;
        [HideInInspector] public float distance;

        public void CalculateDistance()
        {
            if (planet1 != null && planet2 != null)
                distance = Vector3.Distance(planet1.transform.position, planet2.transform.position);
        }
    }

    void Start()
    {

        foreach (var connection in planetConnections)
        {
            connection.CalculateDistance();
            AddConnectionToGraph(connection);
            CreateDottedLine(connection.planet1, connection.planet2, connection.distance);
        }
    }
     // Ajouter les connexions au graphe
    void AddConnectionToGraph(PlanetConnection connection)
    {
        if (!planetGraph.ContainsKey(connection.planet1))
        {
            planetGraph[connection.planet1] = new List<PlanetConnection>();
        }
        planetGraph[connection.planet1].Add(connection);

        if (!planetGraph.ContainsKey(connection.planet2))
        {
            planetGraph[connection.planet2] = new List<PlanetConnection>();
        }
        planetGraph[connection.planet2].Add(connection);
    }

    // Fonction pour obtenir les connexions d'une planète
    public List<PlanetConnection> GetConnections(GameObject planet)
    {
        if (planetGraph.ContainsKey(planet))
            return planetGraph[planet];
        return null;
    }

    public void CreateDottedLine(GameObject planet1, GameObject planet2, float distance)
    {
        GameObject line = new GameObject($"DottedLine_{planet1.name}_{planet2.name}");
        line.transform.SetParent(this.transform);

        Debug.Log($"Création ligne entre {planet1.name} et {planet2.name}");

        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.material = dottedLineMaterial;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, planet1.transform.position);
        lineRenderer.SetPosition(1, planet2.transform.position);
        lineRenderer.useWorldSpace = true;
        activeLines.Add(line);

        // ➕ Affichage de la distance au milieu
        Vector3 midPoint = (planet1.transform.position + planet2.transform.position) / 2f;

        GameObject label = Instantiate(distanceLabelPrefab, midPoint, Quaternion.identity);
        label.transform.SetParent(line.transform);
        label.transform.position = midPoint;

        // 🔍 Récupère le TMP depuis un enfant du prefab
        TextMeshProUGUI tmp = label.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = distance.ToString("F1") + " AL";
        
       
        label.AddComponent<FaceCamera>(); // Texte toujours face caméra
    }
    public void HideAllConnections()
{
    foreach (var line in activeLines)
    {
        if (line.TryGetComponent<LineRenderer>(out var lr))
        {
            lr.enabled = false;
        }
    }
}
 public void ShowAllLines()
{
    if (activeLines.Count == 0)
    {
        Debug.LogWarning("Aucune ligne à afficher !");
        return;
    }

    foreach (var line in activeLines)
    {
        if (line != null)
        {
            if (line.TryGetComponent<LineRenderer>(out var lr))
            {
                lr.enabled = true;
                lr.material = dottedLineMaterial;
            }
            else
            {
                Debug.LogWarning("Aucun LineRenderer trouvé pour l'objet : " + line.name);
            }
        }
    }
}


public List<GameObject> GetAllLines()
{
    return activeLines;
}

public Dictionary<GameObject, List<PlanetConnection>> GetPlanetGraph()
{
    return planetGraph;
}

}
