using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_Controller : MonoBehaviour
{
	[Tooltip("Parent gameobject with every RIGHT move point within it.")]
	public GameObject moveParentR;
	[Tooltip("Parent gameobject with every LEFT move point within it.")]
	public GameObject moveParentL;
	private List<GameObject> movePointsR;
	private List<GameObject> movePointsL;

	private NavMeshAgent agent;
	private int currentPoint = 0;
	private int newPoint = 0;
	bool moveLeft;

	private void Start()
	{
		movePointsR = new List<GameObject> { };
		movePointsL = new List<GameObject> { };

		// Get and store all the children
		Transform[] allChildrenR = moveParentR.GetComponentsInChildren<Transform>();
		foreach (Transform child in allChildrenR)
		{
			movePointsR.Add(child.gameObject);
		}
		Transform[] allChildrenL = moveParentL.GetComponentsInChildren<Transform>();
		foreach (Transform child in allChildrenL)
		{
			movePointsL.Add(child.gameObject);
		}

		// Check this worked
		if (movePointsR.Count == 0 || movePointsL.Count == 0)
		{
			Debug.LogError("Please add at the point parents to this bot.");
		}
		agent = GetComponent<NavMeshAgent>();

		

		// Randomly start with left or right points
		bool moveLeft = (Random.value > 0.5f);

		// Get a random point to start with
		if (moveLeft)
			currentPoint = Random.Range(0, movePointsL.Count);
		else
			currentPoint = Random.Range(0, movePointsR.Count);
	}

	void Update()
	{
		/* HAVE TO BRB - WILL FIX
		// If not reached location, move to it
		if (Vector3.Distance(transform.position, movePoints[currentPoint].transform.position) > 2.0f)
		{
			agent.SetDestination(movePoints[currentPoint].transform.position);
		}
		// Otherwise, wait then move to the next patrol point
		else
		{
			StartCoroutine(Wait(2));

			// Get a new random point
			newPoint = Random.Range(0, movePoints.Count);

			// Make it increadibly unlikely to get the same point twice
			if (newPoint == currentPoint)
			{
				currentPoint = Random.Range(0, movePoints.Count);
			}
			else
			{
				currentPoint = newPoint;
			}
		}
		*/
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
