using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FridgeDoor : MonoBehaviour
{

    public Rigidbody Door;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


        int layerMask = 12;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 10, layerMask))
        {
            if (Input.GetButtonDown("Punch"))
            {
                Debug.Log(Input.mousePosition);
            }
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            Debug.Log("Did Hit");
        }
    }
}