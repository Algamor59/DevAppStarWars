// PlanetGenerator.cs
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Collections;

public class PlanetGenerator : MonoBehaviour
{
    private Texture2D[] planetTextures;

    public GameObject planetPrefab;
    public int numberOfPlanets = 300;
    public float radius = 4f;
    public float minDistanceBetweenPlanets = 0.3f;

    private List<string> planetNames = new List<string>();
    private List<Vector3> planetPositions = new List<Vector3>();

    void Start()
    {
        numberOfPlanets = PlayerPrefs.GetInt("NumberOfPlanets");
        radius = PlayerPrefs.GetFloat("SpawnRadius");

        LoadPlanetData();
        Debug.Log("Load Planet Data !");

        planetTextures = Resources.LoadAll<Texture2D>("PlanetTextures");
        Debug.Log("Textures chargées : " + planetTextures.Length);

        StartCoroutine(GeneratePlanets()); // Coroutine ici
    }

    void LoadPlanetData()
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "planetGeojson.geojson");
        Debug.Log("Chemin complet : " + jsonPath);

        if (File.Exists(jsonPath))
        {
            string jsonContent = File.ReadAllText(jsonPath);
            GeoJson geoJson = JsonConvert.DeserializeObject<GeoJson>(jsonContent);
            Debug.Log("Chargement du GeoJSON réussi !");

            foreach (var feature in geoJson.features)
            {
                if (feature == null 
                    || feature.properties == null 
                    || string.IsNullOrEmpty(feature.properties.name)
                    || feature.geometry == null 
                    || feature.geometry.coordinates == null 
                    || feature.geometry.coordinates.Count < 3)
                {
                    continue;
                }

                planetNames.Add(feature.properties.name);
                float x = (float)feature.geometry.coordinates[0];
                float y = (float)feature.geometry.coordinates[1];
                float z = (float)feature.geometry.coordinates[2];
                Vector3 position = ClampToRadius(new Vector3(x, y, z) * 5f, radius);
                if (position.z < 0) position.z = Mathf.Abs(position.z);
                planetPositions.Add(position);
            }
        }
        else
        {
            Debug.LogError("Fichier GeoJSON introuvable !");
        }
    }

    IEnumerator GeneratePlanets()
    {
        if (planetPrefab == null)
        {
            Debug.LogError("planetPrefab est NULL ! Assigne-le dans l'inspecteur.");
            yield break;
        }

        // Générer les planètes du GeoJSON
        for (int i = 0; i < planetPositions.Count && i < numberOfPlanets; i++)
        {
            Vector3 position = planetPositions[i];
            GameObject planet = Instantiate(planetPrefab, position, Quaternion.identity);

            Renderer renderer = planet.GetComponent<Renderer>();
            if (renderer != null && planetTextures.Length > 0)
            {
                int randomIndex = Random.Range(0, planetTextures.Length);
                renderer.material.mainTexture = planetTextures[randomIndex];
            }

            planet.tag = "Planet";
            planet.name = planetNames[i];

            if (i % 10 == 0) yield return null;
        }

        // Générer des planètes aléatoires supplémentaires
        for (int i = planetPositions.Count; i < numberOfPlanets; i++)
        {
            Vector3 position;
            int attempts = 0;
            const int maxAttempts = 100;
            float maxDistanceFromOthers = radius * 0.6f;

            do
            {
                Vector3 randomDirection = Random.onUnitSphere;
                randomDirection.z = Mathf.Abs(randomDirection.z);
                position = randomDirection * Random.Range(radius * 0.5f, radius);
                attempts++;
            }
            while ((!IsCloseToOthers(position, maxDistanceFromOthers) 
                   || IsTooCloseToOthers(position, minDistanceBetweenPlanets)) 
                  && attempts < maxAttempts);

            if (attempts < maxAttempts)
            {
                GameObject planet = Instantiate(planetPrefab, position, Quaternion.identity);
                Renderer renderer = planet.GetComponent<Renderer>();
                if (renderer != null && planetTextures.Length > 0)
                {
                    int randomIndex = Random.Range(0, planetTextures.Length);
                    renderer.material.mainTexture = planetTextures[randomIndex];
                }

                planet.tag = "Planet";
                planet.name = "Planète" + (i + 1);

                planetPositions.Add(position);
                planetNames.Add(planet.name);
            }
            else
            {
                Debug.LogWarning("Impossible de placer une planète après plusieurs tentatives.");
            }

            if (i % 10 == 0) yield return null;
        }

        Debug.Log("Toutes les planètes ont été générées !");

        // Appel de la méthode pour générer les connexions après génération complète
        OnPlanetsGenerated();
    }

    void OnPlanetsGenerated()
    {
        // Déclenche la génération des connexions
        FindObjectOfType<PlanetConnections>().InitializeConnections();
    }

    Vector3 ClampToRadius(Vector3 position, float maxRadius)
    {
        return position.magnitude > maxRadius ? position.normalized * maxRadius : position;
    }

    bool IsCloseToOthers(Vector3 position, float maxDistance)
    {
        foreach (var pos in planetPositions)
            if (Vector3.Distance(pos, position) <= maxDistance)
                return true;
        return false;
    }

    bool IsTooCloseToOthers(Vector3 position, float minDistance)
    {
        foreach (var pos in planetPositions)
            if (Vector3.Distance(pos, position) < minDistance)
                return true;
        return false;
    }
}

// GeoJSON classes
public class GeoJson { public List<Feature> features { get; set; } }
public class Feature { public Properties properties { get; set; } public Geometry geometry { get; set; } }
public class Properties { public string name { get; set; } }
public class Geometry { public List<double> coordinates { get; set; } }
