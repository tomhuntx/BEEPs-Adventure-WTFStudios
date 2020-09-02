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

	// Scared timer
	private float scaredTime = 4.0f;
	private bool clawScared = false;
	private bool scaredForever = false;
	private bool scaredSpin;
	private bool scaredRun;
	private float spinruntime = 2.0f;
	private float spinSpeed = 100.0f;

	[Header("Animators")]
	public Animator anim;
	public Animator animFace;
	public Animator animScreen;

	[Header("Face Changing")]
	public Renderer faceRender;
	public int faceMatIndex = 0;
	public Texture normal;
	public Texture disturbed;
	public Texture angry;

	// Events
	public UnityEvent onPlayerPunch;

	private void Start()
	{
		faceRender.materials[faceMatIndex].EnableKeyword("_NORMALMAP");
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
		if (agent != null && agent.enabled && agent.isOnNavMesh)
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
				StartCoroutine(Wait(6));

				SwapSides();
			}
		}

		if (anim.GetBool("doAngry") && !scaredForever && !clawScared)
		{
			ScaredTimer();
		}

		// Look at the player if scared
		if (anim.GetBool("doAngry") && !clawScared && !scaredForever && Player.Instance != null)
		{
			RotateTo(Player.Instance.transform.position);
		}
		// Or return to looking at original position
		else if (!anim.GetBool("doAngry") && !anim.GetBool("doAssembly"))
		{
			RotateTo(Quaternion.identity);
		}

		// If box stolen, create a new inactive one
		if (box != null && !box.transform.IsChildOf(this.transform))
		{
			box = Instantiate(boxPrefab, boxSpawnPoint.transform);
			box.SetActive(false);
			box.transform.position = boxSpawnPoint.transform.position;
			box.GetComponent<Rigidbody>().isKinematic = true;

			SetScared();
		}

		if (scaredSpin)
		{
			Spin();
		}
		if (scaredRun)
		{
			Run();
		}
	}

	private void RotateTo(Vector3 target)
	{
		Vector3 direction = (target - transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(direction);
		lookRotation.x = 0;
		lookRotation.z = 0;
		this.transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);
	}

	// Return to normal
	private void RotateTo(Quaternion target)
	{
		GameObject child = this.transform.GetChild(0).gameObject;
		child.transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 1f);
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

		// Scared animation for a few seconds
		SetScared();
	}

	public void GetGrabbed()
	{
		agent.enabled = false;

		// Drop box if has one
		if (box != null && box.transform.IsChildOf(this.transform))
		{
			box.transform.parent = null;
			box.GetComponent<Rigidbody>().isKinematic = false;
			box = null;
		}

		// Scared animation until dropped
		SetScared();
		clawScared = true;
	}

	public void GetPlaced()
	{
		StartCoroutine(WaitAfterDrop(1));
	}

	public void GetDropped()
	{
		StartCoroutine(WaitAfterDrop(3));
	}

	// Set bots as scared
	private void SetScared()
	{
		SetAnimationState("doAngry", true);

		// Drop box if has one
		if (box != null && box.transform.IsChildOf(this.transform))
		{
			box.transform.parent = null;
			box.GetComponent<Rigidbody>().isKinematic = false;
			box = null;
		}

		if (agent.enabled && agent.isOnNavMesh && !scaredRun)
		{
			// Stop agent 
			agent.isStopped = true;
		}
	}

	// Set bots as no longer scared after set time
	private void ScaredTimer()
	{
		scaredTime -= Time.deltaTime;
		if (scaredTime < 0 && !scaredForever && !clawScared)
		{
			SetAnimationState("doAngry", false);
			scaredTime = 4;

			if (agent.enabled && agent.isOnNavMesh)
			{
				// Resume agent
				agent.isStopped = false;
			}
		}
	}

	/// <summary>
	/// Sets the body, face, and screen animator's boolean name to the given value.
	/// </summary>
	/// <param name="booleanName">The name of the boolean variable in the animator.</param>
	/// <param name="state">The state of the animation.</param>
	private void SetAnimationState(string booleanName, bool state)
	{
		anim.SetBool(booleanName, state);
		animFace.SetBool(booleanName, state);
		animScreen.SetBool(booleanName, state);
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

			// Interact animation
			SetAnimationState("doAssembly", true);
		}

		yield return new WaitForSeconds(seconds);

		if (agent.enabled && agent.isOnNavMesh && !scaredForever)
		{
			// Start agent after waiting
			agent.isStopped = false;

			// Activate box if already has one
			if (box != null && box.transform.IsChildOf(this.transform))
			{
				box.SetActive(true);
			}
			// Spawn a box if not angry
			else if (!anim.GetBool("doAngry"))
			{
				box = Instantiate(boxPrefab, boxSpawnPoint.transform);
				box.transform.position = boxSpawnPoint.transform.position;
				box.GetComponent<Rigidbody>().isKinematic = true;
			}

			// Stop interact animation
			SetAnimationState("doAssembly", false);
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
			if (clawScared) { clawScared = false; }
		}
		else
		{
			agent.enabled = false;

			// Otherwise continue to freak out
		}
	}


	public void SetScaredForever(float secs)
	{
		StartCoroutine(WaitThenScare(secs));

		if (secs < 0.7f) {
			scaredSpin = true;
			spinSpeed = Random.Range(100f, 200f);
			spinSpeed *= Random.Range(0, 2) * 2 - 1;
		}
		else
		{
			scaredRun = true;
		}
	}

	private IEnumerator WaitThenScare(float secs)
	{
		// wait for x seconds
		yield return new WaitForSeconds(secs);

		scaredForever = true;

		SetScared();
	}

	private void Spin()
	{
		transform.Rotate(0, spinSpeed * Time.deltaTime, 0, Space.World);

		if (spinSpeed > -300 && spinSpeed < 300)
		{
			spinSpeed += Random.Range(-25.0f, 20.0f);
		}
	}

	private void Run()
	{
		spinruntime -= Time.deltaTime;
		if (spinruntime < 0)
		{
			Vector3 dir = Random.insideUnitSphere * 0.5f;
			dir += transform.position;
			NavMeshHit hit;
			Vector3 finalPosition = Vector3.zero;
			if (NavMesh.SamplePosition(dir, out hit, 0.5f, 1))
			{
				finalPosition = hit.position;
			}
			agent.SetDestination(finalPosition);

			spinruntime = 2f;
		}
		agent.isStopped = false;
	}
}
