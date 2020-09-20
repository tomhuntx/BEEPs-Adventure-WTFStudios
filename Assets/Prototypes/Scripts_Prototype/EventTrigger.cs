using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventTrigger : MonoBehaviour
{
	// Triggers unity event on collision

	public UnityEvent trigger;

	void OnTriggerEnter(Collider other)
	{
		if (other.transform.tag == "Player")
		{
			trigger.Invoke();
		}
	}
}
