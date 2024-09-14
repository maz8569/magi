using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParameterType
{
    NOVELTY,
    HEIGHT,
}

public class SliderController : MonoBehaviour
{
    [SerializeField] private ParameterType type;

    public void OnValueChanged(float value)
    {
        Debug.Log(value * 0.6f + 0.3f + " " + type);
    }
}
