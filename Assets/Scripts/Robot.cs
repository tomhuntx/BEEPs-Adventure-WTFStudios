using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

	//private Player thePlayer;
	private GameObject[] robots;
	public GameObject boxProcessor;

	// Model for maintaining position
	private GameObject model;

	// The Bot's Animator (Different based on type)
	// If anyone can find a different method, that would be great
	public Animator anim;
	public Animator animFace;
	public Animator animScreen;

	//public MatDetector matDetector;
	public UnityEventsHandler matDetector;
	private float punchTime = 0f;

	// Manager is unique variant
	[SerializeField] private bool isManagerBot = false;

	[Header("Unity Events")]
	public UnityEvent onPlayerPunch;
	public UnityEvent onGetExploded;
	public UnityEvent onGetAnnoyed;

	void Start()
	{
		//thePlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		robots = GameObject.FindGameObjectsWithTag("Bot");

		model = transform.GetChild(0).gameObject;
		originalDirection = model.transform.forward;
		originalPosition = model.transform.position;
	}

	void FixedUpdate()
    {
		if (patience >= patienceLimit)
		{
			//anim.SetBool("isAngry", true);
			//animFace.SetBool("isAngry", true);
			//animScreen.SetBool("isAngry", true);

			SetAnimationState("isAngry", true);
		}
		else
		{
			//anim.SetBool("isAngry", false);
			//animFace.SetBool("isAngry", false);
			//animScreen.SetBool("isAngry", false);

			SetAnimationState("isAngry", false);
		}

		// Tell animator when an assembly box exists
		if (!isManagerBot && 
			matDetector.ObjectsInTrigger.Count > 0)
		{
			//anim.SetBool("assemblyBox", true);
			//animFace.SetBool("assemblyBox", true);
			//animScreen.SetBool("assemblyBox", true);

			SetAnimationState("assemblyBox", true);
		}
		else if (!isManagerBot)
		{
			//anim.SetBool("assemblyBox", false);
			//animFace.SetBool("assemblyBox", false);
			//animScreen.SetBool("assemblyBox", false);

			SetAnimationState("assemblyBox", false);
		}

		if (lookAtPlayer)
		{
			//anim.SetBool("isDisturbed", true);
			//animFace.SetBool("isDisturbed", true);
			//animScreen.SetBool("isDisturbed", true);

			SetAnimationState("isDisturbed", true);
		}
		else
		{
			//anim.SetBool("isDisturbed", false);
			//animFace.SetBool("isDisturbed", false);
			//animScreen.SetBool("isDisturbed", false);

			SetAnimationState("isDisturbed", false);
		}

		// Look at the player
		if (lookAtPlayer && Player.Instance != null)
		{
			Vector3 relativePos = Player.Instance.transform.position - transform.position;
			Quaternion rotateTo = Quaternion.LookRotation(relativePos);
			rotateTo.x = 0;
			rotateTo.z = 0;
			model.transform.rotation = Quaternion.Lerp(model.transform.rotation, rotateTo, lookSpeed * Time.deltaTime);

			if (Vector3.Distance(transform.position, Player.Instance.transform.position) > lookRange || lookTime < 0)
			{
				lookAtPlayer = false;
				lookTime = 3f;
				patience = 3;

				//anim.SetBool("isAngry", false);
				//animFace.SetBool("isAngry", false);
				//animScreen.SetBool("isAngry", false);

				//anim.SetBool("isDisturbed", false);
				//animFace.SetBool("isDisturbed", false);
				//animScreen.SetBool("isDisturbed", false);

				SetAnimationState("isAngry", false);
				SetAnimationState("isDisturbed", false);

				if (boxProcessor != null && 
					!isManagerBot)
				{
					boxProcessor.SetActive(true);
				}
			}
			lookTime -= Time.deltaTime;
		}
		// Or return to looking at original position
		else if (Player.Instance != null)
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

		if (patience >= patienceLimit && !isManagerBot)
		{
			if (robots.Length > 0)
			{
				foreach (GameObject robot in robots)
				{
					Robot currentBot = robot.GetComponent<Robot>();
					if (currentBot != null) currentBot.lookAtPlayer = true;
				}
			}

			if (boxProcessor != null)
			{
				boxProcessor.SetActive(false);
			}
		}
	}


    #region Private Methods
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
    #endregion


    #region Public Methods
    public void GetPunched(Vector3 direction)
	{
		lookAtPlayer = true;
		patience++;
		lookTime = 3f;

		if (canBePunched)
		{
			model.transform.position += direction * punchDistance;
			canBePunched = false;

			onPlayerPunch.Invoke();
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

			onGetExploded.Invoke();
		}
	}

	public void GetAnnoyed()
	{
		lookAtPlayer = true;
		patience = patienceLimit;
		lookTime = 3f;

		onGetAnnoyed.Invoke();
	}
    #endregion
}
