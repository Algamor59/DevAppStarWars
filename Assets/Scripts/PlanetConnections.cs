using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlanetConnections : MonoBehaviour
{
    public List<PlanetConnection> planetConnections = new List<PlanetConnection>();
    public TextMeshProUGUI totalDistanceLabel; // Le champ où la distance totale sera affichée
    public TextMeshProUGUI distanceDetailsLabel; // Champ pour le détail


    public GameObject distanceLabelPrefab;
    public Material dottedLineMaterial;

    public GameObject asteroidPrefab;

    public Dictionary<GameObject, List<PlanetConnection>> planetGraph = new Dictionary<GameObject, List<PlanetConnection>>(); // Dictionnaire des connexions entre les planètes
    private Dictionary<(GameObject, GameObject), GameObject> distanceLabels = new();

    private List<GameObject> activeLines = new List<GameObject>();

    [System.Serializable]
    public class PlanetConnection
    {
        public GameObject planet1;
        public GameObject planet2;  
        public bool hasAsteroids = false;
        [HideInInspector] public float distance;

        public void CalculateDistance()
        {
            if (planet1 != null && planet2 != null)
            {
                float baseDistance = Vector3.Distance(planet1.transform.position, planet2.transform.position);
                distance = hasAsteroids ? baseDistance * 2f : baseDistance; //  Si astéroïdes, on double la distance
            }
        }
    }

    void Start()
    {
        foreach (var connection in planetConnections)
        {
            connection.CalculateDistance();
            AddConnectionToGraph(connection);
            CreateDottedLine(connection.planet1, connection.planet2, connection.distance, connection.hasAsteroids);
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

    // Créer une ligne pointillée entre deux planètes
    public void CreateDottedLine(GameObject planet1, GameObject planet2, float distance, bool hasAsteroids)
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

// ➕ Enregistrer dans le dictionnaire pour pouvoir le retrouver plus tard
var key = (planet1, planet2);
var reverseKey = (planet2, planet1);
if (!distanceLabels.ContainsKey(key) && !distanceLabels.ContainsKey(reverseKey))
{
    distanceLabels[key] = label;
}


       TextMeshProUGUI tmp = label.GetComponentInChildren<TextMeshProUGUI>();
tmp.text = distance.ToString("F1") + " AL";

//  Si la ligne a des astéroïdes, passe le texte en orange
if (hasAsteroids)
{
    tmp.color = new Color(1f, 0.4f, 0f); // Plus orange (hex: #FF6600)
}
else
{
    tmp.color = Color.yellow; // Sinon texte classique en blanc
}

        
        label.AddComponent<FaceCamera>(); // Texte toujours face caméra

        // Si cette connexion a des astéroïdes, ajoutons des astéroïdes visuels
        if (hasAsteroids && asteroidPrefab != null)
            {
                int asteroidGroupCount = 2; // Crée plusieurs champs d'astéroïdes
                for (int i = 0; i < asteroidGroupCount; i++)
                {
                    float t = (i + 1) / (float)(asteroidGroupCount + 1);
                    Vector3 position = Vector3.Lerp(planet1.transform.position, planet2.transform.position, t);
                    position += Random.insideUnitSphere * 0.3f;

                    GameObject asteroidGroup = Instantiate(asteroidPrefab, position, Random.rotation);
                    asteroidGroup.transform.SetParent(line.transform); 
                    asteroidGroup.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);
                }
            }

    }

    // Cacher toutes les connexions
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

    // Afficher toutes les lignes
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

    // Cacher tous les labels de distance
    public void HideAllDistanceLabels()
    {
        foreach (var label in distanceLabels.Values)
        {
            if (label != null)
                label.SetActive(false);
        }
    }

    // Afficher tous les labels de distance
    public void ShowAllDistanceLabels()
    {
        foreach (var label in distanceLabels.Values)
        {
            if (label != null)
                label.SetActive(true);
        }
        totalDistanceLabel.text="";
        distanceDetailsLabel.text="";

    }

    // Afficher le label pour une connexion spécifique
    public void ShowLabelForConnection(GameObject planetA, GameObject planetB)
    {
        var key = (planetA, planetB);
        var reverseKey = (planetB, planetA);

        if (distanceLabels.ContainsKey(key))
            distanceLabels[key].SetActive(true);
        else if (distanceLabels.ContainsKey(reverseKey))
            distanceLabels[reverseKey].SetActive(true);
    }

    // Obtenir toutes les lignes
    public List<GameObject> GetAllLines()
    {
        return activeLines;
    }

    // Obtenir le graphe des planètes
    public Dictionary<GameObject, List<PlanetConnection>> GetPlanetGraph()
    {
        return planetGraph;
    }
    
    
}
