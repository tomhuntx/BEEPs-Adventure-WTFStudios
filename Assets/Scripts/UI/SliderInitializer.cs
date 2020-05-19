using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderInitializer : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI text;
    public Slider AssignedSlider { get { return slider; } }
    public TextMeshProUGUI AssignedText { get { return text; } }

    public void Initialize(float minVal, float maxVal, float initialVal, string sliderTitle)
    {
        UI_SliderSetup.SetupSlider(minVal, maxVal, initialVal, slider);
        text.text = sliderTitle;
    }

    public void Initialize(int minVal, int maxVal, int initialVal, string sliderTitle)
    {
        UI_SliderSetup.SetupSlider(minVal, maxVal, initialVal, slider);
        text.text = sliderTitle;
    }
}
