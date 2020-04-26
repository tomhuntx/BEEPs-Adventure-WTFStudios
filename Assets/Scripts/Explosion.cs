using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	// Timeframe that explosion effects surroundings
	private float explosionTime = 1;
	private float time;

	// Time before this object is removed for cleanup reasons
	public float destroyTime = 5;

	void Start()
	{
		// Destroy this object after the set time
		Destroy(gameObject, destroyTime);
	}

	private void Update()
	{
		time += Time.deltaTime;
	}

	void OnTriggerStay(Collider other)
	{
		if (time < explosionTime)
		{
			if (other.transform.tag == "Player")
			{
				Debug.Log("Player in range of explosion");
			}
			if (other.transform.tag == "Box")
			{
				Debug.Log("Box in range of explosion");
			}
		}
	}
}
