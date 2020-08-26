using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ClawCanvasSpriteChange : MonoBehaviour
{

    Image myImageComponent; // Image component attached to this gameobject

    public Sprite originalSprite;
    public Sprite aPressedSprite;
    public Sprite sPressedSprite;
    public Sprite dPressedSprite;
    public Sprite wPressedSprite;

    void Start() //Lets start by getting a reference to our image component.
    {
        myImageComponent = GetComponent<Image>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            myImageComponent.sprite = aPressedSprite;
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            myImageComponent.sprite = originalSprite;
        }
        if (Input.GetKey(KeyCode.S))
        {
            myImageComponent.sprite = sPressedSprite;
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            myImageComponent.sprite = originalSprite;
        }
        if (Input.GetKey(KeyCode.D))
        {
            myImageComponent.sprite = dPressedSprite;
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            myImageComponent.sprite = originalSprite;
        }
        if (Input.GetKey(KeyCode.W))
        {
            myImageComponent.sprite = wPressedSprite;
        }
        else if (Input.GetKeyUp(KeyCode.W))
        {
            myImageComponent.sprite = originalSprite;
        }
    }
}