using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Slider numberOfPlanetsSlider;
    public Slider spawnRadiusSlider;
    public Slider maxConnectionsSlider;
    public Slider maxPathLengthSlider;

    public InputField numberOfPlanetsInput;

    private void Start()
    {
        // Initialise l'input avec la valeur du slider au démarrage
        numberOfPlanetsInput.text = Mathf.RoundToInt(numberOfPlanetsSlider.value).ToString();

        // Quand le slider change, mets à jour l'input field
        numberOfPlanetsSlider.onValueChanged.AddListener((value) => {
            numberOfPlanetsInput.text = Mathf.RoundToInt(value).ToString();
        });

        // Quand l'utilisateur entre une valeur dans l'input field
        numberOfPlanetsInput.onEndEdit.AddListener((text) => {
            if (int.TryParse(text, out int parsedValue))
            {
                // Clamp pour rester dans les limites du slider
                parsedValue = Mathf.Clamp(parsedValue, (int)numberOfPlanetsSlider.minValue, (int)numberOfPlanetsSlider.maxValue);
                numberOfPlanetsSlider.value = parsedValue;
                numberOfPlanetsInput.text = parsedValue.ToString(); // pour recaler si la valeur est clampée
            }
        });
    }

    public void OnStartSimulation()
    {   
        if(Mathf.RoundToInt(numberOfPlanetsSlider.value)>20000){
            numberOfPlanetsSlider.value=20000;
        }
        PlayerPrefs.SetInt("NumberOfPlanets", Mathf.RoundToInt(numberOfPlanetsSlider.value));
        PlayerPrefs.SetFloat("SpawnRadius", spawnRadiusSlider.value);
        PlayerPrefs.SetInt("MaxConnections", Mathf.RoundToInt(maxConnectionsSlider.value));
        PlayerPrefs.SetFloat("MaxPathLength", maxPathLengthSlider.value);

        SceneManager.LoadScene("Gameplay");
    }
}
