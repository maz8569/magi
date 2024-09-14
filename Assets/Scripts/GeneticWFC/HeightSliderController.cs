using TMPro;
using UnityEngine;

public class HeightSliderController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private GeneticWFCLevelGenerator levelGenerator;

    public void OnValueChanged(float value)
    {
        label.text = ((int)value).ToString();
        levelGenerator.SetYSize((int)value);
    }
}
