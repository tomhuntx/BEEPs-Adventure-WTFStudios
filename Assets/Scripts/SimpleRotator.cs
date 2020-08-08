using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotator : MonoBehaviour
{
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSpeed = 3.0f;

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(this.transform.up, rotationSpeed * Time.deltaTime);
    }
}
