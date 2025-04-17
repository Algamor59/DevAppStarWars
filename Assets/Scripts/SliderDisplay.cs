using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class SliderDisplay : MonoBehaviour
{
    public Slider slider;
    public TMP_Text valueText; 

    void Start()
    {
        UpdateValue(slider.value); // afficher la valeur au démarrage
        slider.onValueChanged.AddListener(UpdateValue);
    }

    void UpdateValue(float value)
    {
        valueText.text = Mathf.RoundToInt(value).ToString(); // ou value.ToString("0.0") pour 1 décimale
    }
}
