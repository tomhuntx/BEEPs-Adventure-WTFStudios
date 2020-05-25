using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
	// Look Variables
	private float lookSpeed = 2.5f;
	private float lookRange = 12f;
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
	public GameObject boxProcessor;

	// Model for maintaining position
	private GameObject model;

	// The Bot's Animator (Different based on type)
	public Animator anim;

	public MatDetector matDetector;
	private float punchTime = 0f;

	// Manager is unique variant
	public bool manager = false;

	void Start()
	{
		thePlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

		robots = GameObject.FindGameObjectsWithTag("Bot");

		model = transform.GetChild(0).gameObject;
		originalDirection = model.transform.forward;
		originalPosition = model.transform.position;
	}

	void FixedUpdate()
    {
		if (patience >= patienceLimit)
		{
			anim.SetBool("isAngry", true);
		} 
		else
		{
			anim.SetBool("isAngry", false);
		}

		// Tell animator when an assembly box exists
		if (!manager && matDetector.boxExists)
		{
			anim.SetBool("assemblyBox", true);
		}
		else if (!manager)
		{
			anim.SetBool("assemblyBox", false);
		}

		if (lookAtPlayer)
		{
			anim.SetBool("isDisturbed", true);
		}
		else
		{
			anim.SetBool("isDisturbed", false);
		}

		// Look at the player
		if (lookAtPlayer && thePlayer != null)
		{
			Vector3 relativePos = thePlayer.transform.position - transform.position;
			Quaternion rotateTo = Quaternion.LookRotation(relativePos);
			rotateTo.x = 0;
			rotateTo.z = 0;
			model.transform.rotation = Quaternion.Lerp(model.transform.rotation, rotateTo, lookSpeed * Time.deltaTime);

			if (Vector3.Distance(transform.position, thePlayer.transform.position) > lookRange || lookTime < 0)
			{
				lookAtPlayer = false;
				lookTime = 3f;
				patience = 3;

				anim.SetBool("isAngry", false);
				anim.SetBool("isDisturbed", false);

				if (boxProcessor && !manager)
				{
					boxProcessor.SetActive(true);
				}
			}
			lookTime -= Time.deltaTime;
		}
		// Or return to looking at original position
		else if (thePlayer)
		{
			Quaternion rotateTo = Quaternion.LookRotation(originalDirection);
			model.transform.rotation = Quaternion.Lerp(model.transform.rotation, rotateTo, lookSpeed * Time.deltaTime);
		}

		// Move back to start position if it leaves
		if (Vector3.Distance(model.transform.position, originalPosition) > 0.1f)
		{
			model.transform.position = Vector3.MoveTowards(model.transform.position, originalPosition, 2f * Time.deltaTime);
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

			if (boxProcessor)
			{
				boxProcessor.SetActive(false);
			}
		}
	}

	public void GetPunched(Vector3 direction)
	{
		lookAtPlayer = true;

		patience++;

		lookTime = 3f;

		if (canBePunched)
		{
			model.transform.position += direction * punchDistance;

			canBePunched = false;
		}
	}

	public void GetBlownUp(GameObject explosion)
	{
		lookAtPlayer = true;

		patience = patienceLimit;

		lookTime = 3f;

		// Prevents multiple explosive boxes from sending robots flying
		if (canBePunched)
		{
			Vector3 direction = transform.position - explosion.transform.position;
			direction.Normalize();
			model.transform.position += direction * expDistance;
			canBePunched = false;
		}
	}

	public void GetAnnoyed()
	{
		lookAtPlayer = true;
		patience = patienceLimit;
		lookTime = 3f;
	}
}
