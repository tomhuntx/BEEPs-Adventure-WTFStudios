using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	// Explosion variables
	[SerializeField] private float power = 10.0f;
	[SerializeField] private float radius = 7.0f;
	[SerializeField] private float upforce = 1.0f;
	[SerializeField] private ForceMode forceType = ForceMode.Impulse;

	// Time before this object is removed for cleanup reasons
	public float destroyTime = 5;

	void Start()
	{
		foreach (CameraShaker shaker in CameraShaker.Instances)
		{
			shaker.DoExplosionShake(power, this.transform.position);
		}

		Explode();

		// Destroy this object after the set time
		Destroy(gameObject, destroyTime);
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
				if (hit.tag == "Box" ||
					hit.tag == "Explosive Box")
				{
					rb.AddExplosionForce(power, transform.position, radius, upforce, forceType);

					// Deals damage to boxes based on distance
					hit.GetComponent<DestructibleObject>().ApplyDamage(10 * power / Vector3.Distance(transform.position, hit.transform.position));
				}
			}
			if (hit.tag == "Player")
			{
				// Base power on distance between player and object
				float pow = power / Vector3.Distance(transform.position, hit.transform.position);

				// Push player
				//hit.GetComponent<FPSController>().PushFromPoint(transform.position, pow);
				//Player.Instance.PlayerMovementControls.ApplyForce((Player.Instance.transform.position - this.transform.position).normalized * pow, 
				//												   FPSController.ConvertFromForceMode(forceType));

				Player.Instance.PlayerMovementControls.ApplyForce((Player.Instance.transform.position - this.transform.position).normalized * pow,
																	   PlayerCharacterController.ConvertFromForceMode(forceType));
			}
			if (hit.tag == "Bot")
			{
				Robot rob = hit.gameObject.GetComponentInParent<Robot>();
				if (rob != null)
				{
					rob.GetBlownUp(this.transform.position);
				}
			}
			if (hit.tag == "MiniBot")
			{
				hit.gameObject.GetComponent<NPC_Controller>().GetBlownUp(this.transform.position);
			}
			if (hit.tag == "ManagerBot")
			{
				MBot_Controller cont = SearchForParent.GetParentTransform(hit.gameObject).GetComponentInChildren<MBot_Controller>();
				if (cont != null)
				{
					cont.GetBlownUp();
				}

				Robot rob = hit.gameObject.GetComponentInParent<Robot>();
				if (rob != null)
				{
					rob.GetBlownUp(this.transform.position);
				}
			}
		}
	}
}
