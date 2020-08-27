﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MetalButton : MonoBehaviour
{
	public Animator anim;
	private bool pressing;
	private bool complete;

	public UnityEvent onPressed;

	float pressTime = 0.0f;
	float pressDuration = 1.5f;

    // Update is called once per frame
    void Update()
    {
		if (!complete)
		{
			if (pressing)
			{
				pressTime += Time.deltaTime;
				if (pressTime > pressDuration)
				{
					anim.SetBool("isComplete", true);
					complete = true;

					onPressed.Invoke();
				}
			}
			else
			{
				pressTime = 0;
			}
		}
    }

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.tag == "Heavy Box" && pressing == false)
		{
			anim.SetBool("isPressing", true);
			pressing = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.transform.tag == "Heavy Box" && pressing == true)
		{
			anim.SetBool("isPressing", false);
			pressing = false;
		}
	}
}
