using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Component used to visualize button presses.
/// </summary>
[RequireComponent(typeof(Image))]
public class OnScreenButtonDisplay : MonoBehaviour
{
    [Tooltip("The key that this UI will react to.")]
    [SerializeField] private KeyCode buttonInput = KeyCode.E;

    [Space]
    [SerializeField] private Sprite buttonUpSprite;
    [SerializeField] private Sprite buttonDownSprite;

    [Space]
    [SerializeField] private TextMeshProUGUI buttonText;

    [Tooltip("Tick this to make the text move along with the image sprite's pivot point.")]
    [SerializeField] private bool enableOffsetChange = true;
    private Image buttonImage;



    // Start is called before the first frame update
    void Start()
    {
        buttonImage = this.transform.GetComponent<Image>();
        if (buttonText != null)
            buttonText.text = buttonInput.ToString();

        LiftButton();
    }

    private void Update()
    {
        if (Input.GetKeyDown(buttonInput))
            PressButton();
        
        else if (Input.GetKeyUp(buttonInput))
            LiftButton();
    }



    public void PressButton()
    {
        buttonImage.sprite = buttonDownSprite;
        UpdatePos();
    }

    public void LiftButton()
    {
        buttonImage.sprite = buttonUpSprite;
        UpdatePos();
    }

    private void UpdatePos()
    {
        if (!enableOffsetChange ||
            buttonText == null) return;
        buttonText.rectTransform.anchoredPosition = (buttonImage.sprite.pivot - (buttonImage.sprite.rect.size / 2)) - buttonText.rectTransform.pivot * 2;
    }
}
