using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatDetector : MonoBehaviour
{
	public bool boxExists;

	private void OnTriggerStay(Collider other)
	{
		if (other.tag == "Box Material")
		{
			boxExists = true;
		}
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Box Material")
		{
			boxExists = false;
		}
	}
}
