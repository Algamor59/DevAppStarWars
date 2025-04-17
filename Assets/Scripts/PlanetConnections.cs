using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlanetConnections : MonoBehaviour
{
    public List<PlanetConnection> planetConnections = new List<PlanetConnection>();
    public TextMeshProUGUI totalDistanceLabel;
    public TextMeshProUGUI distanceDetailsLabel;

    [SerializeField] private float maxConnectionDistance = 8f;
    [SerializeField] [Range(0, 100)] private int connectionChance = 20; // Chance de connexion supplémentaire
    [SerializeField] private int maxConnectionsPerPlanet = 3; // Connexions max par planète

    public GameObject distanceLabelPrefab;
    public Material dottedLineMaterial;
    private bool cantHide = false;
    public bool isHidden = false;

    public GameObject asteroidPrefab;

    public Dictionary<GameObject, List<PlanetConnection>> planetGraph = new Dictionary<GameObject, List<PlanetConnection>>();
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
                distance = hasAsteroids ? baseDistance * 2f : baseDistance;
            }
        }
    }

    void Start()
    {
        maxConnectionsPerPlanet = PlayerPrefs.GetInt("MaxConnections");
        maxConnectionDistance = PlayerPrefs.GetFloat("MaxPathLength");
        GenerateRandomConnections();

        foreach (var connection in planetConnections)
        {
            connection.CalculateDistance();
            AddConnectionToGraph(connection);
            CreateDottedLine(connection.planet1, connection.planet2, connection.distance, connection.hasAsteroids);
        }
    }

    void AddConnectionToGraph(PlanetConnection connection)
    {
        if (!planetGraph.ContainsKey(connection.planet1))
            planetGraph[connection.planet1] = new List<PlanetConnection>();
        planetGraph[connection.planet1].Add(connection);

        if (!planetGraph.ContainsKey(connection.planet2))
            planetGraph[connection.planet2] = new List<PlanetConnection>();
        planetGraph[connection.planet2].Add(connection);
    }
void GenerateMinimumSpanningTree(GameObject[] planets)
    {
        List<(GameObject a, GameObject b, float dist)> edges = new();

        // 1. Générer toutes les paires possibles
        for (int i = 0; i < planets.Length; i++)
        {
            for (int j = i + 1; j < planets.Length; j++)
            {
                float dist = Vector3.Distance(planets[i].transform.position, planets[j].transform.position);
                edges.Add((planets[i], planets[j], dist));
            }
        }

        // 2. Trier les connexions par distance
        edges.Sort((x, y) => x.dist.CompareTo(y.dist));

        // 3. Structure Union-Find pour suivre les groupes connectés
        Dictionary<GameObject, GameObject> parent = new();
        foreach (var planet in planets)
            parent[planet] = planet;

        GameObject Find(GameObject p)
        {
            if (parent[p] != p)
                parent[p] = Find(parent[p]);
            return parent[p];
        }

        void Union(GameObject a, GameObject b)
        {
            var rootA = Find(a);
            var rootB = Find(b);
            if (rootA != rootB)
                parent[rootB] = rootA;
        }

        // 4. Appliquer Kruskal
        foreach (var edge in edges)
        {
            if (Find(edge.a) != Find(edge.b))
            {
                CreateConnection(edge.a, edge.b); // Ta fonction de connexion actuelle
                Union(edge.a, edge.b);
            }
        }
    }
   void GenerateRandomConnections()
{
        

        GameObject[] planets = GameObject.FindGameObjectsWithTag("Planet");
        GenerateMinimumSpanningTree(planets);
        List<GameObject> unconnected = new List<GameObject>(planets);
    List<GameObject> connected = new List<GameObject>();

    if (unconnected.Count == 0) return;

    GameObject first = unconnected[Random.Range(0, unconnected.Count)];
    connected.Add(first);
    unconnected.Remove(first);

        int safetyCounter = 0;
        int maxAttempts = 1000; // ou plus selon le nombre de planètes

        while (unconnected.Count > 0 && safetyCounter < maxAttempts)
        {
            safetyCounter++;

            GameObject from = connected[Random.Range(0, connected.Count)];
            GameObject to = unconnected[Random.Range(0, unconnected.Count)];

            float dist = Vector3.Distance(from.transform.position, to.transform.position);
            if (dist <= maxConnectionDistance && !ConnectionExists(from, to))
            {
                CreateConnection(from, to);
                connected.Add(to);
                unconnected.Remove(to);
            }
        }

        if (unconnected.Count > 0)
        {
            Debug.LogWarning("Certaines planètes sont trop éloignées pour être connectées.");
        }


        for (int i = 0; i < planets.Length; i++)
    {
        for (int j = i + 1; j < planets.Length; j++)
        {
            GameObject a = planets[i];
            GameObject b = planets[j];

            float dist = Vector3.Distance(a.transform.position, b.transform.position);
            if (dist <= maxConnectionDistance && !ConnectionExists(a, b))
            {
                int aCount = GetConnectionCount(a);
                int bCount = GetConnectionCount(b);
                if (aCount < maxConnectionsPerPlanet && bCount < maxConnectionsPerPlanet)
                {
                    if (Random.Range(0, 100) < connectionChance)
                    {
                        CreateConnection(a, b);
                    }
                }
            }
        }
    }

    // Nouvelle étape : si une planète n’a aucune possibilité de connexion à cause du rayon, on force une avec la plus proche
    foreach (var planet in planets)
    {
        bool isConnected = planetConnections.Exists(c => c.planet1 == planet || c.planet2 == planet);
        if (!isConnected)
        {
            GameObject closest = null;
            float closestDistance = float.MaxValue;

            foreach (var other in planets)
            {
                if (other == planet) continue;
                if (ConnectionExists(planet, other)) continue;

                float dist = Vector3.Distance(planet.transform.position, other.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closest = other;
                }
            }

            if (closest != null)
            {
                CreateConnection(planet, closest);

                // PlanetConnection forcedConnection = new PlanetConnection
                // {
                //     planet1 = planet,
                //     planet2 = closest,
                //     hasAsteroids = Random.Range(0, 4) == 0
                // };
                // planetConnections.Add(forcedConnection);
                Debug.LogWarning($"Planète {planet.name} était isolée (aucune dans le rayon). Connexion forcée avec {closest.name} à {closestDistance:F1} unités.");
            }
        }
    }

    int GetConnectionCount(GameObject planet)
    {
        return planetConnections.FindAll(c => c.planet1 == planet || c.planet2 == planet).Count;
    }

    bool ConnectionExists(GameObject a, GameObject b)
    {
        return planetConnections.Exists(c =>
            (c.planet1 == a && c.planet2 == b) ||
            (c.planet1 == b && c.planet2 == a));
    }

}

void CreateConnection(GameObject from, GameObject to)
    {
        PlanetConnection connection = new PlanetConnection
        {
            planet1 = from,
            planet2 = to,
            hasAsteroids = Random.Range(0, 4) == 0
        };
        planetConnections.Add(connection);
    }

    public List<PlanetConnection> GetConnections(GameObject planet)
    {
        if (planetGraph.ContainsKey(planet))
            return planetGraph[planet];
        return null;
    }

    public void CreateDottedLine(GameObject planet1, GameObject planet2, float distance, bool hasAsteroids)
    {
        GameObject line = new GameObject($"DottedLine_{planet1.name}_{planet2.name}");
        line.transform.SetParent(this.transform);

        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.material = dottedLineMaterial;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, planet1.transform.position);
        lineRenderer.SetPosition(1, planet2.transform.position);
        lineRenderer.useWorldSpace = true;
        activeLines.Add(line);

        Vector3 midPoint = (planet1.transform.position + planet2.transform.position) / 2f;
        GameObject label = Instantiate(distanceLabelPrefab, midPoint, Quaternion.identity);
        label.transform.SetParent(line.transform);
        label.transform.position = midPoint;

        var key = (planet1, planet2);
        var reverseKey = (planet2, planet1);
        if (!distanceLabels.ContainsKey(key) && !distanceLabels.ContainsKey(reverseKey))
        {
            distanceLabels[key] = label;
        }

        TextMeshProUGUI tmp = label.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = distance.ToString("F1") + " AL";

        if (hasAsteroids)
        {
            int asteroidGroupCount = 3;
            for (int i = 0; i < asteroidGroupCount; i++)
            {
                float t = (i + 1) / (float)(asteroidGroupCount + 1);
                Vector3 position = Vector3.Lerp(planet1.transform.position, planet2.transform.position, t);
                position += Random.insideUnitSphere * 0.3f;

                GameObject asteroidGroup = Instantiate(asteroidPrefab, position, Random.rotation);
                asteroidGroup.transform.SetParent(line.transform);
                asteroidGroup.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);
            }
            tmp.color = new Color(1f, 0.4f, 0f);
        }
        else
        {
            tmp.color = Color.yellow;
        }

        label.AddComponent<FaceCamera>();
    }
    


    public void HideAllConnections()
    {
        cantHide = true;
        foreach (var line in activeLines)
        {
            if (line.TryGetComponent<LineRenderer>(out var lr))
            {
                lr.enabled = false;
            }
        }
    }

    public void HideAndShowAllConnections()
    {
        if (!isHidden && !cantHide)
        {
            isHidden = true;
            foreach (var line in activeLines)
            {
                if (line.TryGetComponent<LineRenderer>(out var lr))
                {
                    lr.enabled = false;
                }
            }
            foreach (var label in distanceLabels.Values)
            {
                if (label != null)
                    label.SetActive(false);
            }
        }
        else
        {
            isHidden = false;
            foreach (var line in activeLines)
            {
                if (line.TryGetComponent<LineRenderer>(out var lr))
                {
                    lr.enabled = true;
                }
            }
            foreach (var label in distanceLabels.Values)
            {
                if (label != null)
                    label.SetActive(true);
            }
        }
    }

    public void ShowAllLines()
    {
        cantHide = false;
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
            }
        }
    }

    public void HideAllDistanceLabels()
    {
        foreach (var label in distanceLabels.Values)
        {
            if (label != null)
                label.SetActive(false);
        }
    }

    public void ShowAllDistanceLabels()
    {
        foreach (var label in distanceLabels.Values)
        {
            if (label != null)
                label.SetActive(true);
        }
        totalDistanceLabel.text = "";
        distanceDetailsLabel.text = "";
    }

    public void ShowLabelForConnection(GameObject planetA, GameObject planetB)
    {
        var key = (planetA, planetB);
        var reverseKey = (planetB, planetA);

        if (distanceLabels.ContainsKey(key))
            distanceLabels[key].SetActive(true);
        else if (distanceLabels.ContainsKey(reverseKey))
            distanceLabels[reverseKey].SetActive(true);
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
