using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InputFieldManager : MonoBehaviour
{
    public Slider slider;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private bool integerOnly = false;

    public void SendToSlider()
    {
        var newValue = integerOnly ? Int32.Parse(inputField.text) : float.Parse(inputField.text);
        newValue = newValue > slider.maxValue ? slider.maxValue : newValue;
        newValue = newValue < slider.minValue ? slider.minValue : newValue;
        slider.value = newValue;
    }

    private void Update()
    {
        if (!inputField.isFocused)
        {
            string preset = integerOnly ? "{0:0}" : "{0:0.00}";
            inputField.text = string.Format(preset, slider.value);
        }
    }
}
