using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBox : MonoBehaviour
{

    public GameObject box;
    public float rotateSpeed = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        box.transform.Rotate(0f, rotateSpeed, 0);
    }
}
