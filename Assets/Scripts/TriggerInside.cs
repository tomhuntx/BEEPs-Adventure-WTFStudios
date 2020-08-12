using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerInside : MonoBehaviour
{
	public bool inside; // is there a box in the collider?
	private bool checking = false; //only check once at a time

	IEnumerator Check()
	{
		checking = true;

		yield return new WaitForSeconds(1);

		inside = false;
		checking = false;
	}

	void OnTriggerStay(Collider other)
	{
		if (other.tag == "Box")
		{
			inside = true;
			if (!checking)
			{
				StartCoroutine(Check());
			}
		}
	}
}
