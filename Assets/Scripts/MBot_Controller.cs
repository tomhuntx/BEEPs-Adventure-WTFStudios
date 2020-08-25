using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class MBot_Controller : MonoBehaviour
{
	private Vector3 startPosition;
	private NavMeshAgent agent;


	// This Manager's Hat
	public GameObject hardhat;
	public GameObject hatdhatStartLoc;

	// Events
	public UnityEvent onDisturb;

	// The Bot's Animator (all 3 are required unfortunately)
	public Animator anim;
	public Animator animFace;
	public Animator animScreen;

	// Player
	public Player player;

	private bool lookingForHat = false;
	private bool nearHat = true;

	// Start is called before the first frame update
	void Start()
    {
		startPosition = transform.position;
		agent = GetComponent<NavMeshAgent>();
		agent.SetDestination(startPosition);
	}

    // Update is called once per frame
    void Update()
    {
		if (Vector3.Distance(transform.position, hardhat.transform.position) < 2.0f)
		{
			nearHat = true;

			if (lookingForHat)
			{
				TryGrabHat();
				RotateTo(hardhat.transform.position);
			}
		}
		else
		{
			nearHat = false;
			agent.SetDestination(hardhat.transform.position);
			RotateTo(Quaternion.identity);
		}

		// Detect if at start position or not
		if (!lookingForHat && Vector3.Distance(transform.position, startPosition) < 0.5f)
		{
			if (!anim.GetBool("isHome"))
			{ // Nested if ensures bools are only set once (performance reasons)
				anim.SetBool("isHome", true);
				animFace.SetBool("isHome", true);
				animScreen.SetBool("isHome", true);
			}
		}
		else 
		{
			if (anim.GetBool("isHome"))
			{
				anim.SetBool("isHome", false);
				animFace.SetBool("isHome", false);
				animScreen.SetBool("isHome", false);
			}
		}
    }

	public void HatMoved()
	{
		// Start angry pause
		StartCoroutine(AngryPause());

		// Angry animation
		anim.SetTrigger("hatGrabbed");
		animFace.SetTrigger("hatGrabbed");
		animScreen.SetTrigger("hatGrabbed");
	}

	private void RotateTo(Vector3 target)
	{
		GameObject child = this.transform.GetChild(0).gameObject;
		Vector3 direction = (target - transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(direction);
		child.transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);
	}

	private void RotateTo(Quaternion target)
	{
		GameObject child = this.transform.GetChild(0).gameObject;
		child.transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 2f);
	}

	private void TryGrabHat()
	{
		if (nearHat)
		{
			// Start interact animation
			anim.SetBool("isPickingUp", true);
			animFace.SetBool("isPickingUp", true);
			animScreen.SetBool("isPickingUp", true);

			// Stop agent before waiting
			agent.isStopped = true;

			StartCoroutine(GrabPause(2.0f));
		}
	}

	// Bot successfully grabbed hat
	private void GrabHat()
	{
		// Return to base with hat
		agent.SetDestination(startPosition);
		lookingForHat = false;

		// Remove hat from hands
		if (true) // INSERT PREVENTION OF REMOVING NON-HARDHAT OBJECTS
		{
			player.RemoveGrabbedObject();
			hardhat.GetComponent<GrabbableObject>().DetachFromParent();
		}

		// Put hat back on head
		hardhat.transform.SetParent(hatdhatStartLoc.transform);
		hardhat.transform.position = hatdhatStartLoc.transform.position;
		hardhat.transform.rotation = hatdhatStartLoc.transform.rotation;
		Rigidbody rb = hardhat.GetComponent<Rigidbody>();
		rb.isKinematic = true;
	}

	private IEnumerator GrabPause(float seconds)
	{
		yield return new WaitForSeconds(seconds);

		agent.isStopped = false;

		// Start interact animation
		anim.SetBool("isPickingUp", false);
		animFace.SetBool("isPickingUp", false);
		animScreen.SetBool("isPickingUp", false);

		// If found hat, grab it
		if (nearHat)
		{
			GrabHat();
		}
	}


	// Pause and then start looking for hat - prevents instant attempts at grabbing
	private IEnumerator AngryPause()
	{
		agent.isStopped = true;

		yield return new WaitForSeconds(1.0f);

		agent.isStopped = false;
		lookingForHat = true;
		agent.SetDestination(hardhat.transform.position);
	}
}
