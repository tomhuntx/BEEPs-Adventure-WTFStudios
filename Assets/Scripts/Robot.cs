using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
	// Look Variables
	private float lookSpeed = 2.5f;
	private float lookRange = 8f;
	private float lookTime = 3f;
	private bool lookAtPlayer = false;

	// Punch Variables
	private float punchDistance = 0.5f;
	private bool canBePunched = true;
	private int patienceLimit = 5;
	private int patience = 0;
	private float expDistance = 2f;

	// Saving Positions
	private Vector3 originalDirection;
	private Vector3 originalPosition;

	private Player thePlayer;
	public Task punchRobot;
	private GameObject[] robots;

	// Manager is unique variant
	public bool manager = false;

	void Awake()
	{
		originalDirection = transform.forward;
		originalPosition = transform.position;
		thePlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

		robots = GameObject.FindGameObjectsWithTag("Bot");
	}

	void FixedUpdate()
    {
		// Look at the player
		if (lookAtPlayer && thePlayer != null)
		{
			Vector3 relativePos = thePlayer.transform.position - transform.position;
			Quaternion rotateTo = Quaternion.LookRotation(relativePos);
			transform.rotation = Quaternion.Lerp(transform.rotation, rotateTo, lookSpeed * Time.deltaTime);

			if (Vector3.Distance(transform.position, thePlayer.transform.position) > lookRange && lookTime < 0)
			{
				lookAtPlayer = false;
				lookTime = 3f;
			}
			lookTime -= Time.deltaTime;
		}
		// Or return to looking at original position
		else if (thePlayer)
		{
			Quaternion rotateTo = Quaternion.LookRotation(originalDirection);
			transform.rotation = Quaternion.Lerp(transform.rotation, rotateTo, lookSpeed * Time.deltaTime);
		}

		// Move back to start position if it leaves
		if (Vector3.Distance(transform.position, originalPosition) > 0.01f)
		{
			transform.position = Vector3.MoveTowards(transform.position, originalPosition, 1f * Time.deltaTime);
		}
		else
		{
			canBePunched = true;
		}

		if (patience >= patienceLimit && !manager)
		{
			if (punchRobot)
			{
				punchRobot.Contribute();
			}

			foreach (GameObject robot in robots)
			{
				robot.GetComponent<Robot>().lookAtPlayer = true;
			}

			patience = 0;
		}
	}

	public void GetPunched(Vector3 direction)
	{
		lookAtPlayer = true;

		patience++;

		if (canBePunched)
		{
			transform.position += punchDistance * direction;
			canBePunched = false;
		}
	}

	public void GetBlownUp(GameObject explosion)
	{
		lookAtPlayer = true;

		patience = patienceLimit;

		// Prevents multiple explosive boxes from sending robots flying
		if (canBePunched)
		{
			Vector3 direction = this.transform.position - explosion.transform.position;
			direction.Normalize();
			transform.position += direction * expDistance;
			canBePunched = false;
		}
	}

	public void GetAnnoyed()
	{
		lookAtPlayer = true;
		patience = patienceLimit;
	}
}
