using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchArm : MonoBehaviour
{

    public Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Punch"))
        {
            anim.SetBool("isPunching", true);
        }
        else
        {
            anim.SetBool("isPunching", false);
        }
    }
}
