using UnityEngine;
using TMPro;

public class PlanetNameDisplay : MonoBehaviour
{
    public TextMeshPro textMeshProPrefab; 
    private GameObject planetNameText;   
    private TextMeshPro planetTextMeshPro; 
    public TMP_FontAsset customFont;

    void Start()
    {
        CreatePlanetNameText();
    }

    void CreatePlanetNameText()
    {
        // Créer un objet texte pour afficher le nom de la planète
        planetNameText = new GameObject("PlanetNameText");
        planetNameText.transform.SetParent(transform); // Rendre l'objet texte enfant de la planète
        planetNameText.transform.localPosition = new Vector3(0, -0.6f, 0); // Ajuster la position du texte sous la planète

        // Ajouter un composant TextMeshPro
        planetTextMeshPro = planetNameText.AddComponent<TextMeshPro>();
        planetTextMeshPro.font = customFont;
        planetTextMeshPro.text = gameObject.name; // Assigner le nom de la planète comme texte
        planetTextMeshPro.fontSize = 2; 
        planetTextMeshPro.alignment = TextAlignmentOptions.Center; 
        planetTextMeshPro.color = Color.white;

        // // Assurer que le texte regarde toujours la caméra pour être toujours visible
        // planetTextMeshPro.transform.rotation = Quaternion.LookRotation(planetTextMeshPro.transform.position - Camera.main.transform.position);
    }

    void Update()
    {
        // S'assurer que le texte reste toujours orienté vers la caméra
        if (planetNameText != null && Camera.main != null)
        {
            planetTextMeshPro.transform.rotation = Quaternion.LookRotation(planetTextMeshPro.transform.position - Camera.main.transform.position);
        }
    }
}
