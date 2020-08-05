using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_Controller : MonoBehaviour
{
	[Tooltip("Assigned object health based on percentage.")]
	public GameObject[] patrolPoints;

	private NavMeshAgent agent;
	private int currentPoint = 0;

	private void Start()
	{
		if (patrolPoints.Length == 0)
		{
			Debug.LogError("Please add at least one patrol point to this bot.");
		}
		agent = GetComponent<NavMeshAgent>();
	}

	void Update()
	{
		// If not reached location, move to it
		if (Vector3.Distance(transform.position, patrolPoints[currentPoint].transform.position) > 2.0f)
		{
			agent.SetDestination(patrolPoints[currentPoint].transform.position);
		}
		// Otherwise, wait then move to the next patrol point
		else
		{
			StartCoroutine(Wait(1));

			currentPoint++;
			if (currentPoint == patrolPoints.Length)
			{
				currentPoint = 0;
			}
		}
	}

	private IEnumerator Wait(float seconds)
	{
		// Stop agent before waiting
		agent.isStopped = true;

		yield return new WaitForSeconds(seconds);

		// Start agent after waiting
		agent.isStopped = false;
	}
}
