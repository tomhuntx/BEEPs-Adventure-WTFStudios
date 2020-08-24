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
		if (Vector3.Distance(transform.position, hardhat.transform.position) < 2.5f)
		{
			nearHat = true;

			if (lookingForHat)
			{
				TryGrabHat();
				RotateTo(hardhat.transform.position);
			}
			else
			{
				RotateTo(Quaternion.identity);
			}
		}
		else
		{
			nearHat = false;
			agent.SetDestination(hardhat.transform.position);
		}


		if (!lookingForHat && Vector3.Distance(transform.position, startPosition) < 0.1f)
		{
			anim.SetTrigger("backHome");
			animFace.SetTrigger("backHome");
			animScreen.SetTrigger("backHome");
		}
    }

	public void HatMoved()
	{
		lookingForHat = true;
		agent.SetDestination(hardhat.transform.position);
		anim.SetTrigger("hatGrabbed");
		animFace.SetTrigger("hatGrabbed");
		animScreen.SetTrigger("hatGrabbed");

		// Stop interact animation
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


			// Stop agent before waiting
			agent.isStopped = true;

			StartCoroutine(GrabPause(2.0f));
		}
	}

	// Return to base with hat
	private void GrabHat()
	{
		agent.SetDestination(startPosition);

		// Put hat back on head
		hardhat.transform.parent = null;
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

		// If found hat, grab it
		if (nearHat)
		{
			GrabHat();
		}
	}
}
