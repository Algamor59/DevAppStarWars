using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;

public class PlanetConnections : MonoBehaviour
{
    public List<PlanetConnection> planetConnections = new List<PlanetConnection>();
    public TextMeshProUGUI totalDistanceLabel; // Le champ où la distance totale sera affichée
    public TextMeshProUGUI distanceDetailsLabel; // Champ pour le détail
[SerializeField]
private float maxConnectionDistance = 8f;

    public GameObject distanceLabelPrefab;
    public Material dottedLineMaterial;
    private bool cantHide = false;
    public bool isHidden = false;

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
        // Génération aléatoire des connexions
        GenerateRandomConnections();

        // Traitement des connexions générées
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

    // Générer des connexions aléatoires entre les planètes existantes
   void GenerateRandomConnections()
{
    float maxDistance = maxConnectionDistance;
    GameObject[] planets = GameObject.FindGameObjectsWithTag("Planet");
    List<GameObject> unconnected = new List<GameObject>(planets);
    List<GameObject> connected = new List<GameObject>();

    if (unconnected.Count == 0) return;

    GameObject first = unconnected[Random.Range(0, unconnected.Count)];
    connected.Add(first);
    unconnected.Remove(first);

    while (unconnected.Count > 0)
    {
        GameObject from = connected[Random.Range(0, connected.Count)];
        GameObject to = unconnected[Random.Range(0, unconnected.Count)];

        float dist = Vector3.Distance(from.transform.position, to.transform.position);
        if (dist <= maxDistance && !ConnectionExists(from, to))
        {
            CreateConnection(from, to);
            connected.Add(to);
            unconnected.Remove(to);
        }
    }

    // Connexions supplémentaires
    for (int i = 0; i < planets.Length; i++)
    {
        for (int j = i + 1; j < planets.Length; j++)
        {
            float dist = Vector3.Distance(planets[i].transform.position, planets[j].transform.position);
            if (Random.Range(0, 5) == 0 && dist <= maxDistance && !ConnectionExists(planets[i], planets[j]))
            {
                CreateConnection(planets[i], planets[j]);
            }
        }
    }

    // Vérifier que chaque planète a au moins une connexion
    foreach (var planet in planets)
    {
        bool hasConnection = planetConnections.Exists(c => c.planet1 == planet || c.planet2 == planet);
        if (!hasConnection)
        {
            GameObject other = planets[Random.Range(0, planets.Length)];
            int maxAttempts = 50;
            int attempts = 0;

            while ((other == planet || ConnectionExists(planet, other) || Vector3.Distance(planet.transform.position, other.transform.position) > maxDistance) && attempts < maxAttempts)
            {
                other = planets[Random.Range(0, planets.Length)];
                attempts++;
            }

            if (attempts < maxAttempts)
            {
                CreateConnection(planet, other);
                Debug.LogWarning($"Planète {planet.name} était isolée. Connexion ajoutée avec {other.name}");
            }
            else
            {
                Debug.LogWarning($"Impossible de connecter la planète {planet.name} après {maxAttempts} tentatives.");
            }
        }
    }

    bool ConnectionExists(GameObject a, GameObject b)
    {
        return planetConnections.Exists(c =>
            (c.planet1 == a && c.planet2 == b) ||
            (c.planet1 == b && c.planet2 == a));
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




        // S'assurer que chaque planète a AU MOINS une connexion
        foreach (var planet in planets)
        {
            if (!connected.Contains(planet))
            {
                // Choisir une autre planète aléatoire différente pour connecter
                GameObject other = planets[Random.Range(0, planets.Length)];
                while (other == planet) other = planets[Random.Range(0, planets.Length)];

                PlanetConnection fallbackConnection = new PlanetConnection
                {
                    planet1 = planet,
                    planet2 = other,
                    hasAsteroids = Random.Range(0, 4) == 0
                };

                planetConnections.Add(fallbackConnection);
                Debug.LogWarning($"Planète {planet.name} était isolée. Connexion ajoutée avec {other.name}");
            }
        }
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
            int asteroidGroupCount = 5; // Crée plusieurs champs d'astéroïdes
    for (int i = 0; i < asteroidGroupCount; i++)
    {
        float t = (i + 1) / (float)(asteroidGroupCount + 1);
        Vector3 position = Vector3.Lerp(planet1.transform.position, planet2.transform.position, t);
        position += Random.insideUnitSphere * 0.3f;

        GameObject asteroidGroup = Instantiate(asteroidPrefab, position, Random.rotation);
        asteroidGroup.transform.SetParent(line.transform); // Garde le tout sous la ligne
        asteroidGroup.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);
    }
            tmp.color = new Color(1f, 0.4f, 0f); // Plus orange (hex: #FF6600)
        }
        else
        {
            tmp.color = Color.yellow; // Sinon texte classique en blanc
        }

        label.AddComponent<FaceCamera>(); // Texte toujours face caméra
    }

    // Cacher toutes les connexions
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
        
        if(!isHidden &&!cantHide){
           foreach (var line in activeLines)
        {
            if (line.TryGetComponent<LineRenderer>(out var lr))
            {
                isHidden = true;
                lr.enabled = false;
            }
        } 
        }
        else{
            foreach (var line in activeLines)
        {
            if (line.TryGetComponent<LineRenderer>(out var lr))
            {   
                isHidden=false;
                lr.enabled = true;
            }
        }
        }
        
    }

    // Afficher toutes les lignes
    public void ShowAllLines()
    {
        cantHide=false;
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
        totalDistanceLabel.text = "";
        distanceDetailsLabel.text = "";
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
