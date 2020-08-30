using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class NPC_Controller : MonoBehaviour
{
	[Tooltip("Parent gameobject with every RIGHT move point within it.")]
	public GameObject moveParentR;
	[Tooltip("Parent gameobject with every LEFT move point within it.")]
	public GameObject moveParentL;
	private List<GameObject> movePointsR;
	private List<GameObject> movePointsL;
	private List<GameObject> currentPoints;

	// Box child gameobject
	public GameObject boxSpawnPoint;
	public GameObject boxPrefab;
	private GameObject box;

	// Rigidbody of bot
	private Rigidbody rb;

	// DEBUG - Current point bot is moving to
	public GameObject currentPoint;

	private NavMeshAgent agent;
	private int currentIndex = 0;
	private int newPoint = 0;
	bool moveLeft = false;
	private float punchForce = 0.5f;

	// Events
	public UnityEvent onPlayerPunch;

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
		movePointsR.Remove(moveParentR);

		Transform[] allChildrenL = moveParentL.GetComponentsInChildren<Transform>();
		foreach (Transform child in allChildrenL)
		{
			movePointsL.Add(child.gameObject);
		}
		movePointsL.Remove(moveParentL);

		// Check this worked
		if (movePointsR.Count == 0 || movePointsL.Count == 0)
		{
			Debug.LogError("Please add at the point parents to this bot.");
		}

		rb = GetComponent<Rigidbody>();
		agent = GetComponent<NavMeshAgent>();

		moveLeft = (Random.value < 0.5);
		SwapSides();
	}

	void Update()
	{
		if (agent.enabled && agent.isOnNavMesh)
		{
			// If not reached location, move to it
			if (Vector3.Distance(transform.position, currentPoints[currentIndex].transform.position) > 2.0f)
			{
				agent.SetDestination(currentPoints[currentIndex].transform.position);
				currentPoint = currentPoints[currentIndex];
			}
			// Otherwise, wait then move to the next patrol point
			else
			{
				StartCoroutine(Wait(5));

				SwapSides();
			}
		}
	}

	/// <summary>
	/// Swap side in which to randomly move to one of its points
	/// </summary>
	private void SwapSides()
	{
		if (moveLeft)
		{
			currentIndex = Random.Range(0, movePointsR.Count);
			currentPoints = movePointsR;
			moveLeft = false;
		}
		else
		{
			currentIndex = Random.Range(0, movePointsL.Count);
			currentPoints = movePointsL;
			moveLeft = true;
		}
	}

	public void GetPunched(Vector3 direction)
	{
		this.transform.position += direction * punchForce;

		onPlayerPunch.Invoke(); 
	}

	public void GetBlownUp(Vector3 explosionPosition)
	{
		Vector3 direction = transform.position - explosionPosition;
		direction.Normalize();

		this.transform.position += direction * 2f;

		//onGetExploded.Invoke();
	}

	public void GetGrabbed()
	{
		agent.enabled = false;

		// Turn off box if has one
		if (box != null && box.transform.IsChildOf(this.transform))
		{
			box.transform.parent = null;
			box.GetComponent<Rigidbody>().isKinematic = false;
			box = null;
		}

		// Scared animation

	}

	public void GetPlaced()
	{
		agent.enabled = true;
		agent.isStopped = false;
	}

	public void GetDropped()
	{
		StartCoroutine(WaitAfterDrop(3));

		// Return to normal animations?
	}

	private IEnumerator Wait(float seconds)
	{
		if (agent.enabled && agent.isOnNavMesh)
		{
			// Stop agent before waiting
			agent.isStopped = true;

			if (box != null && box.transform.IsChildOf(this.transform))
			{
				box.SetActive(false);
			}
		}

		// PLAY ANIMAITON!

		yield return new WaitForSeconds(seconds);

		if (agent.enabled && agent.isOnNavMesh)
		{
			// Start agent after waiting
			agent.isStopped = false;

			if (box != null && box.transform.IsChildOf(this.transform))
			{
				box.SetActive(true);
			}
			else
			{
				box = Instantiate(boxPrefab, boxSpawnPoint.transform);
				box.transform.position = boxSpawnPoint.transform.position;
				box.GetComponent<Rigidbody>().isKinematic = true;
			}
		}
	}


	private IEnumerator WaitAfterDrop(float seconds)
	{
		yield return new WaitForSeconds(seconds);

		agent.enabled = true;

		if (agent.isOnNavMesh)
		{
			agent.isStopped = false;

			// Stop animations and return to normal
		}
		else
		{
			agent.enabled = false;

			// Otherwise continue to freak out
		}
	}
}
