using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Disable all level 1 conveyors

public class DisableConveyors : MonoBehaviour
{
	GameObject[] conveyors;

	// Start is called before the first frame update
	void Start()
    {
		conveyors = GameObject.FindGameObjectsWithTag("Conveyor Belt");

		AlarmLights[] al = FindObjectsOfType<AlarmLights>();

	}

	public void Disable()
	{
		foreach (GameObject conv in conveyors)
		{
			// Set all forces inactive
			PersistentForceRigidbody[] forces = conv.GetComponentsInChildren<PersistentForceRigidbody>();
			foreach(PersistentForceRigidbody force in forces)
			{
				force.enabled = false;
			}

			// Set all scroll textures to inactive
			ScrollTexture st = conv.GetComponent<ScrollTexture>();
			if (st)
			{
				st.enabled = false;
			}
		}

	}
}
