using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
	// Look Variables
	private float lookSpeed = 2.5f;
	private float lookRange = 6f;
	private float lookTime = 3f;
	private bool lookAtPlayer = false;

	// Punch Variables
	private float punchDistance = 0.5f;
	private bool canBePunched = true;
	private int patienceLimit = 5;
	private int patience = 0;

	// Saving Positions
	private Vector3 originalDirection;
	private Vector3 originalPosition;

	private Player thePlayer = null;
	public Task punchRobot;

	void Awake()
	{
		originalDirection = transform.forward;
		originalPosition = transform.position;
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

		// Move back to the position if it leaves
		if (Vector3.Distance(transform.position, originalPosition) > 0.1f)
		{
			transform.position = Vector3.MoveTowards(transform.position, originalPosition, 1f * Time.deltaTime);
		}
		else
		{
			canBePunched = true;
		}
	}

	public void GetPunched(Player player)
	{
		// ALSO WANT SFX & SLIGHT BUMP
		// & anims ofc
		thePlayer = player;
		patience++;

		if (canBePunched)
		{
			transform.position += punchDistance * player.PlayerMovementControls.MainCam.transform.forward;
			canBePunched = false;
		}

		if (patience >= patienceLimit)
		{
			punchRobot.Contribute();
			lookAtPlayer = true;
		}
	}
}
