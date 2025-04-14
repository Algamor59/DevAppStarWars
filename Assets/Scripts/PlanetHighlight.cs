using UnityEngine;

public class PlanetHighlight : MonoBehaviour
{
    private Material originalMaterial;
    public Material highlightedMaterial;

    void OnMouseDown()
    {
        originalMaterial = GetComponent<Renderer>().material;
        GetComponent<Renderer>().material = highlightedMaterial;  // Applique un matériau qui change la couleur
    }

    void OnMouseUp()
    {
        GetComponent<Renderer>().material = originalMaterial;  // Restaure le matériau original
    }
}
