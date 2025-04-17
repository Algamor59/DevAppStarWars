using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public Slider numberOfPlanetsSlider;
    public Slider spawnRadiusSlider;
    public Slider maxConnectionsSlider;
    public Slider maxPathLengthSlider;
    public InputField numberOfPlanetsInput;
    public Button button;
public RawImage videoRawImage;
    public VideoPlayer videoPlayer;  
    public VideoClip loadingClip; 

    private void Start()
    {
        numberOfPlanetsSlider.gameObject.SetActive(true);
    numberOfPlanetsInput.gameObject.SetActive(true);
    maxPathLengthSlider.gameObject.SetActive(true);
    maxConnectionsSlider.gameObject.SetActive(true);
    spawnRadiusSlider.gameObject.SetActive(true);
    button.gameObject.SetActive(true);

        // Initialise l'input avec la valeur du slider au démarrage
        numberOfPlanetsInput.text = Mathf.RoundToInt(numberOfPlanetsSlider.value).ToString();


        // Synchronisation slider → input
        numberOfPlanetsSlider.onValueChanged.AddListener((value) => {
            numberOfPlanetsInput.text = Mathf.RoundToInt(value).ToString();
        });

        // Synchronisation input → slider
        numberOfPlanetsInput.onEndEdit.AddListener((text) => {
            if (int.TryParse(text, out int parsedValue))
            {
                parsedValue = Mathf.Clamp(parsedValue, (int)numberOfPlanetsSlider.minValue, (int)numberOfPlanetsSlider.maxValue);
                numberOfPlanetsSlider.value = parsedValue;
                numberOfPlanetsInput.text = parsedValue.ToString();
            }
        });
    }

    public void OnStartSimulation()
    {
        // Lance la coroutine de chargement avec vidéo
        StartCoroutine(PlayLoadingAndStart());
    }

   private IEnumerator PlayLoadingAndStart()
{
    int planetCount = Mathf.RoundToInt(numberOfPlanetsSlider.value);
    if (planetCount > 20000)
    {
        planetCount = 20000;
        numberOfPlanetsSlider.value = planetCount;
    }

    PlayerPrefs.SetInt("NumberOfPlanets", planetCount);
    PlayerPrefs.SetFloat("SpawnRadius", spawnRadiusSlider.value);
    PlayerPrefs.SetInt("MaxConnections", Mathf.RoundToInt(maxConnectionsSlider.value));
    PlayerPrefs.SetFloat("MaxPathLength", maxPathLengthSlider.value);

    // Cache UI
    numberOfPlanetsSlider.gameObject.SetActive(false);
    numberOfPlanetsInput.gameObject.SetActive(false);
    maxPathLengthSlider.gameObject.SetActive(false);
    maxConnectionsSlider.gameObject.SetActive(false);
    spawnRadiusSlider.gameObject.SetActive(false);
    button.gameObject.SetActive(false);

    // Active le panneau vidéo
    videoRawImage.gameObject.SetActive(true); // ← tu dois ajouter une référence à ton RawImage


    // Joue le clip depuis l’inspecteur
    videoPlayer.clip = loadingClip;
    videoPlayer.Play();

    // Lance le chargement de la scène en async
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Gameplay");
    asyncLoad.allowSceneActivation = false;

    // Attendre que la scène soit chargée
    yield return new WaitUntil(() => asyncLoad.progress >= 0.9f);

    // Stoppe la vidéo, cache l'écran et active la scène
    videoPlayer.Stop();
    videoRawImage.gameObject.SetActive(false);

    asyncLoad.allowSceneActivation = true;
}


}
