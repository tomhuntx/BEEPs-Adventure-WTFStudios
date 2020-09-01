using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistonAnimator : MonoBehaviour
{
    // Animator to transition
    public Animator anim;

    private bool boxExists = false;

    private void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Generic Destructable")
        {
            anim.SetBool("boxUnderPiston", true);
            boxExists = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Box Material" || other.tag == "Generic Destructable")
        {
            anim.SetBool("boxUnderPiston", false);
            boxExists = false;
        }
    }
}