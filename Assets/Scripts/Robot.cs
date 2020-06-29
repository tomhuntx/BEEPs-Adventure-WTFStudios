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
	private float lookTimer;
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
	//private GameObject model;

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

		//model = transform.GetChild(0).gameObject;
		//originalDirection = model.transform.forward;
		//originalPosition = model.transform.position;

		originalDirection = this.transform.forward;
		originalPosition = this.transform.position;
	}

	void FixedUpdate()
    {
		if (patience >= patienceLimit &&
			!IsTimerDone())
		{
			SetAnimationState("doAngry", true);
		}
		else
		{
			SetAnimationState("doAngry", false);
		}

		if (!isManagerBot)
		{
			// Tell animator when an assembly box exists
			if (matDetector != null &&
				matDetector.ObjectsInTrigger.Count > 0)
			{
				SetAnimationState("doAssembly", true);
			}
			else
			{
				SetAnimationState("doAssembly", false);
			}
		}

		if (lookAtPlayer)
		{
			SetAnimationState("doDisturbed", true);
		}
		else
		{
			SetAnimationState("doDisturbed", false);
		}

		// Look at the player
		if (lookAtPlayer && Player.Instance != null)
		{
			Vector3 relativePos = Player.Instance.transform.position - transform.position;
			Quaternion rotateTo = Quaternion.LookRotation(relativePos);
			rotateTo.x = 0;
			rotateTo.z = 0;
			//model.transform.rotation = Quaternion.Lerp(model.transform.rotation, rotateTo, lookSpeed * Time.deltaTime);
			this.transform.rotation = Quaternion.Lerp(this.transform.rotation, rotateTo, lookSpeed * Time.deltaTime);

			if (Vector3.Distance(transform.position, Player.Instance.transform.position) > lookRange && 
				IsTimerDone())
			{
				lookAtPlayer = false;
				//lookTime = 3f;
				ResetLookTimer(lookTime);
				patience = 3;

				SetAnimationState("doAngry", false);
				SetAnimationState("doDisturbed", false);

				if (boxProcessor != null && 
					!isManagerBot)
				{
					boxProcessor.SetActive(true);
				}
			}
			//lookTime -= Time.deltaTime;
		}
		// Or return to looking at original position
		else if (Player.Instance != null)
		{
			Quaternion rotateTo = Quaternion.LookRotation(originalDirection);
			this.transform.rotation = Quaternion.Lerp(this.transform.rotation, rotateTo, lookSpeed * Time.deltaTime);
		}

		// Move back to start position if it leaves
		if (Vector3.Distance(this.transform.position, originalPosition) > 0.1f)
		{
			this.transform.position = Vector3.MoveTowards(this.transform.position, originalPosition, 2f * Time.deltaTime);
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

	private void ResetLookTimer(float time, UnityEvent invokable = null)
	{
		lookTimer = time + Time.time;
		if (invokable != null) invokable.Invoke();
	}

	private void StopLookTimer(UnityEvent invokable = null)
	{
		lookTimer = Time.time;
		if (invokable != null) invokable.Invoke();
	}

	private bool IsTimerDone()
	{
		return lookTimer < Time.time;
	}
	#endregion


	#region Public Methods
	public void GetPunched(Vector3 direction)
	{
		lookAtPlayer = true;
		patience++;
		//lookTime = 3f;
		ResetLookTimer(lookTime);

		if (patience >= patienceLimit)
        {
			GetAnnoyed();
        }

		if (canBePunched)
		{
			this.transform.position += direction * punchDistance;
			canBePunched = false;

			onPlayerPunch.Invoke();
		}
	}

	public void GetBlownUp(Vector3 explosionPosition)
	{
		//lookAtPlayer = true;
		//patience = patienceLimit;
		//lookTime = 3f;

		GetAnnoyed();

		// Prevents multiple explosive boxes from sending robots flying
		if (canBePunched)
		{
			Vector3 direction = transform.position - explosionPosition;
			direction.Normalize();
			this.transform.position += direction * expDistance;
			canBePunched = false;

			onGetExploded.Invoke();
		}
	}

	public void GetAnnoyed()
	{
		lookAtPlayer = true;
		patience = patienceLimit;
		//lookTime = 3f;
		ResetLookTimer(lookTime);

		onGetAnnoyed.Invoke();
	}
    #endregion
}
