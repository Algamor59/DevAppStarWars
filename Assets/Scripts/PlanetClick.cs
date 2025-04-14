using UnityEngine;

public class PlanetClick : MonoBehaviour
{
    public Material highlightMaterial;  // Matériau de surbrillance pour la planète
    private Material originalMaterial;   // Matériau original de la planète

    public Material dottedLineMaterial;
    private bool isSelected = false;    // Indicateur pour savoir si la planète est sélectionnée
    private PlanetConnections planetConnections;
    public static GameObject lineBetweenPoints; 

    public static GameObject startPoint;  // Point de départ (statique pour qu'il soit accessible globalement)
    public static GameObject endPoint;    // Point d'arrivée (statique pour qu'il soit accessible globalement)
    private Vector3 initialScale; 
    void Start()
    {
        planetConnections = FindObjectOfType<PlanetConnections>();
    }


    void OnMouseDown()
    {
        if(!isSelected){
           SelectPlanet(); 
        }
}   

    void SelectPlanet()
    {
        // Si la planète est déjà sélectionnée, on la réinitialise
        // if (isSelected)
        // {
        //     ResetPlanetVisuals();
        //     return;
        // }

        // Sauvegarder le matériau original
if (!isSelected)
        {
            originalMaterial = GetComponent<Renderer>().material;
            initialScale = transform.localScale; // Sauvegarde la taille initiale
        }

        // Définir la planète comme point de départ ou d'arrivée
        if(!startPoint || !endPoint){
           SetStartOrEndPoint(); 
        }
        else{   
            Debug.Log("Les deux points sont déjà définis !");
        }
        
    }



    void SetStartOrEndPoint()
{
    // Appliquer le matériau de surbrillance
    GetComponent<Renderer>().material = highlightMaterial;

    // Agrandir la planète pour la mettre en évidence
    transform.localScale *= 1.2f;  // Agrandit de 20%

    isSelected = true;

    if (startPoint == null)
    {
        startPoint = gameObject;
        Debug.Log(gameObject.name + " est maintenant le point de départ.");
    }
    else if (endPoint == null)
    {
        endPoint = gameObject;
        Debug.Log(gameObject.name + " est maintenant le point d'arrivée.");
    }
    else
    {
        // Si les deux points sont déjà définis, réinitialiser
        Debug.Log("Les deux points sont déjà définis !");
        ResetPlanetVisuals(); // On réinitialise cette planète si les deux points sont déjà définis
    }
}

    void ResetPlanetVisuals()
    {
        // Si la planète est sélectionnée, réinitialise ses visuels
        if (isSelected)
        {
            // Réinitialiser la taille de la planète à sa taille initiale
            transform.localScale = initialScale;  // Rétablit la taille d'origine

            // Remettre le matériau original
            GetComponent<Renderer>().material = originalMaterial;

            // Réinitialiser l'état de sélection
            isSelected = false;
        }
    }




     public void ResetSelection()
{
    startPoint = null;
    endPoint = null;

    // Réinitialiser les visuels des planètes sélectionnées
    foreach (var planet in FindObjectsOfType<PlanetClick>())
    {
        planet.ResetPlanetVisuals();
    }

    // Réafficher toutes les lignes après reset
    if (planetConnections != null)
    {
        planetConnections.ShowAllLines();
    }
}


}