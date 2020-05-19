using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotPunch : MonoBehaviour
{
	// Speed of moving to look
	private float lookSpeed = 2.5f;

	// Max range that the bot will keep looking at the player
	private float lookRange = 6f;

	// Ensure the robot looks at the player for at least x seconds
	private float lookTime = 3f;


	private bool lookAtPlayer = false;
	private Player thePlayer = null;
	private Vector3 originalDirection;
	private Vector3 originalPosition;

	private Vector3 origPlayerPos = Vector3.zero;

	private void Start()
	{
		originalDirection = transform.forward;
		originalPosition = transform.position;
	}

	// Update is called once per frame
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
	}

	public void GetPunched(Player player)
	{
		// ALSO WANT SFX & SLIGHT BUMP
		// & anims ofc

		thePlayer = player;
		lookAtPlayer = true;
	}
}
