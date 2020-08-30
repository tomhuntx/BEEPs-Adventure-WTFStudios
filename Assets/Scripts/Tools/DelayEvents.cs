using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Delays a list of events then triggers them
public class DelayEvents : MonoBehaviour
{
	public float delay = 0;
	public UnityEvent eventsToTrigger;

	// Update is called once per frame
	public void WaitThenTrigger()
    {
		StartCoroutine(Delay());
    }

	// Pause and then start looking for hat - prevents instant attempts at grabbing
	private IEnumerator Delay()
	{
		yield return new WaitForSeconds(delay);

		eventsToTrigger.Invoke();
	}
}
