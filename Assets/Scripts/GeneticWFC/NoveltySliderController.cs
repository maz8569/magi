using TMPro;
using UnityEngine;

public class NoveltySliderController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private GeneticWFCLevelGenerator levelGenerator;

    public void OnValueChanged(float value)
    {
        label.text = value.ToString("F2");
        levelGenerator.desiredNovelty = value;
    }
}
