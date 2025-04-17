// PlanetConnections.cs
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlanetConnections : MonoBehaviour
{
    public List<PlanetConnection> planetConnections = new List<PlanetConnection>();
    public TextMeshProUGUI totalDistanceLabel;
    public TextMeshProUGUI distanceDetailsLabel;

    [SerializeField] private float maxConnectionDistance = 8f;
    [SerializeField] [Range(0, 100)] private int connectionChance = 20;
    [SerializeField] private int maxConnectionsPerPlanet = 3;

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
        // Connections are initialized externally
    }

    public void InitializeConnections()
    {
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
        for (int i = 0; i < planets.Length; i++)
            for (int j = i + 1; j < planets.Length; j++)
                edges.Add((planets[i], planets[j], Vector3.Distance(planets[i].transform.position, planets[j].transform.position)));
        edges.Sort((x, y) => x.dist.CompareTo(y.dist));
        var parent = new Dictionary<GameObject, GameObject>();
        foreach (var p in planets) parent[p] = p;

        GameObject Find(GameObject p)
        {
            if (parent[p] != p)
                parent[p] = Find(parent[p]);
            return parent[p];
        }

        void Union(GameObject a, GameObject b)
        {
            GameObject ra = Find(a);
            GameObject rb = Find(b);
            if (ra != rb)
                parent[rb] = ra;
        }

        foreach (var e in edges)
        {
            if (Find(e.a) != Find(e.b))
            {
                CreateConnection(e.a, e.b);
                Union(e.a, e.b);
            }
        }
    }

    void GenerateRandomConnections()
    {
        GameObject[] planets = GameObject.FindGameObjectsWithTag("Planet");
        GenerateMinimumSpanningTree(planets);

        var unconnected = new List<GameObject>(planets);
        var connected = new List<GameObject>();
        if (unconnected.Count == 0) return;

        GameObject first = unconnected[Random.Range(0, unconnected.Count)];
        connected.Add(first);
        unconnected.Remove(first);

        int safety = 0;
        int maxAtt = 1000;
        while (unconnected.Count > 0 && safety++ < maxAtt)
        {
            GameObject from = connected[Random.Range(0, connected.Count)];
            GameObject to = unconnected[Random.Range(0, unconnected.Count)];
            float d = Vector3.Distance(from.transform.position, to.transform.position);
            if (d <= maxConnectionDistance && !ConnectionExists(from, to))
            {
                CreateConnection(from, to);
                connected.Add(to);
                unconnected.Remove(to);
            }
        }

        if (unconnected.Count > 0)
            Debug.LogWarning("Certaines planètes sont trop éloignées pour être connectées.");

        for (int i = 0; i < planets.Length; i++)
        {
            for (int j = i + 1; j < planets.Length; j++)
            {
                GameObject a = planets[i];
                GameObject b = planets[j];
                float d = Vector3.Distance(a.transform.position, b.transform.position);
                if (d <= maxConnectionDistance && !ConnectionExists(a, b))
                {
                    if (GetConnectionCount(a) < maxConnectionsPerPlanet
                        && GetConnectionCount(b) < maxConnectionsPerPlanet
                        && Random.Range(0, 100) < connectionChance)
                    {
                        CreateConnection(a, b);
                    }
                }
            }
        }

        foreach (var planet in planets)
        {
            if (!planetConnections.Exists(c => c.planet1 == planet || c.planet2 == planet))
            {
                GameObject closest = null;
                float cd = float.MaxValue;
                foreach (var o in planets)
                {
                    if (o != planet && !ConnectionExists(planet, o))
                    {
                        float dd = Vector3.Distance(planet.transform.position, o.transform.position);
                        if (dd < cd)
                        {
                            cd = dd;
                            closest = o;
                        }
                    }
                }
                if (closest != null)
                {
                    CreateConnection(planet, closest);
                    Debug.LogWarning($"Planète {planet.name} isolée. Connexion forcée avec {closest.name} à {cd:F1}.");
                }
            }
        }
    }

    void CreateConnection(GameObject from, GameObject to)
    {
        planetConnections.Add(new PlanetConnection { planet1 = from, planet2 = to, hasAsteroids = Random.Range(0, 4) == 0 });
    }

    bool ConnectionExists(GameObject a, GameObject b)
    {
        return planetConnections.Exists(c => (c.planet1 == a && c.planet2 == b) || (c.planet1 == b && c.planet2 == a));
    }

    int GetConnectionCount(GameObject planet)
    {
        return planetConnections.FindAll(c => c.planet1 == planet || c.planet2 == planet).Count;
    }

    public void CreateDottedLine(GameObject planet1, GameObject planet2, float distance, bool hasAsteroids)
    {
        GameObject line = new GameObject($"DottedLine_{planet1.name}_{planet2.name}");
        line.transform.SetParent(transform);
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = dottedLineMaterial;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        lr.SetPosition(0, planet1.transform.position);
        lr.SetPosition(1, planet2.transform.position);
        activeLines.Add(line);

        Vector3 mid = (planet1.transform.position + planet2.transform.position) / 2f;
        GameObject label = Instantiate(distanceLabelPrefab, mid, Quaternion.identity, line.transform);
        TextMeshProUGUI tmp = label.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = distance.ToString("F1") + " AL";
        tmp.color = hasAsteroids ? new Color(1f, 0.4f, 0f) : Color.yellow;

        if (hasAsteroids)
        {
            for (int i = 0; i < 3; i++)
            {
                float t = (i + 1) / 4f;
                Vector3 pos = Vector3.Lerp(planet1.transform.position, planet2.transform.position, t) + Random.insideUnitSphere * 0.3f;
                GameObject ast = Instantiate(asteroidPrefab, pos, Random.rotation, line.transform);
                ast.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);
            }
        }

        label.AddComponent<FaceCamera>();
    }

    // ... Les méthodes de Hide/Show restent inchangées





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
