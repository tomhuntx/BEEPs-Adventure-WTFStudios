using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SliderSetup
{    
    public static void SetupSlider(float minValue, float maxValue, float initialValue, Slider slider)
    {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = initialValue;
    }

    public static void SetupSlider(int minValue, int maxValue, int initialValue, Slider slider)
    {
        slider.wholeNumbers = true;
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = initialValue;
    }
}
