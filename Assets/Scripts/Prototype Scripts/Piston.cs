using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Piston : MonoBehaviour
{
    private Animator anim;
    [SerializeField] private UnityEventsHandler beforePress;
    [SerializeField] private UnityEventsHandler afterPress;

    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (beforePress.ObjectsInTrigger.Count > 0)
        {
            anim.SetBool("doPress", true);
            beforePress.ClearNullReference();
        }


        if (afterPress.ObjectsInTrigger.Count > 0)
        {
            anim.SetBool("doPress", false);
            afterPress.ClearNullReference();
            print("exit");
        }
    }
}
