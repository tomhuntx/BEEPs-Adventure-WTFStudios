using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupBox : MonoBehaviour
{
	[SerializeField] private GameObject hands;
	[SerializeField] private GameObject playerCam;

	public KeyCode pickUpBox = KeyCode.E;
	public int throwKey = 0;

	private float throwForce = 600;
	private float reach = 2f;

	private float boxDistance; 

	private Rigidbody boxRB;
	private Vector3 boxPosition;
	private bool holdingBox = false;

	private void Start()
	{
		playerCam = GameObject.FindGameObjectWithTag("MainCamera");
		hands = GameObject.Find("Hands");
		boxRB = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		boxDistance = Vector3.Distance(transform.position, hands.transform.position);

		// Drop box if it goes out of hands
		if (boxDistance >= reach /2)
		{
			holdingBox = false;	
		}

		// Freeze box when holding
		if (holdingBox)
		{
			boxRB.velocity = Vector3.zero;
			boxRB.angularVelocity = Vector3.zero;
			boxRB.MovePosition(hands.transform.position);

			// Throw on left-click
			if (Input.GetMouseButtonDown(throwKey))
			{
				boxRB.AddForce(playerCam.transform.forward * throwForce);
				holdingBox = false;
			}
		}
		// Drop when not holding
		else
		{
			boxPosition = transform.position;
			transform.SetParent(null);
			boxRB.useGravity = true;
			transform.position = boxPosition;
		}

		// When the player presses E
		if (Input.GetKeyDown(pickUpBox))
		{
			// Drop box if already holding it
			if (holdingBox)
			{
				holdingBox = false;
			}
			// Pick up box if within reach
			else if (boxDistance <= reach)
			{
				holdingBox = true;
				boxRB.useGravity = false;
				boxRB.detectCollisions = true;

				// Move box to hands
				transform.position = hands.transform.position;
				transform.SetParent(hands.transform);
			}
		}
	}
}
