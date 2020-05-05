using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	// Explosion variables
	[SerializeField] private float power = 10.0f;
	[SerializeField] private float radius = 7.0f;
	[SerializeField] private float upforce = 1.0f;

	// Time before this object is removed for cleanup reasons
	public float destroyTime = 5;

	void Start()
	{
		// Destroy this object after the set time
		Destroy(gameObject, destroyTime);

		Explode();
	}

	void Explode()
	{
		// Get all colliders in range
		Collider[] collidersInRange = Physics.OverlapSphere(transform.position, radius);

		// Cycle through colliders
		foreach (Collider hit in collidersInRange)
		{
			Rigidbody rb = hit.GetComponent<Rigidbody>();

			if (rb != null)
			{
				rb.AddExplosionForce(power, transform.position, radius, upforce, ForceMode.Impulse);

				if (hit.tag == "Box")
				{
					// Deals damage to boxes based on distance
					hit.GetComponent<DestructibleObject>().ApplyDamage(5 * power / Vector3.Distance(transform.position, hit.transform.position));
				}
			}
			if (hit.tag == "Player")
			{
				// Base power on distance between player and object
				float pow = 6 * power / Vector3.Distance(transform.position, hit.transform.position);

				// Push player
				hit.GetComponent<FPSController>().PushFromPoint(transform.position, pow);
			}
		}
	}
}
