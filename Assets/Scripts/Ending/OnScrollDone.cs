using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnScrollDone : MonoBehaviour
{
	public UnityEvent onDone;

	private Animator anim;
	private bool done = false;

    // Start is called before the first frame update
    void Start()
    {
		anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
		// Check if anim is finished (is no longer in starting state)
		if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Credits Scroll") && !done)
		{
			done = true;

			onDone.Invoke();
		}
	}
}
