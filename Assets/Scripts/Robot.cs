﻿using System.Collections;
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
	private bool superAnnoyed = false;
	private bool keepLooking = false;

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

	[Header("Face Changing")]
	public Renderer faceRender;
	public int faceMatIndex = 0;
	public Texture normal;
	public Texture disturbed;
	public Texture angry;
	public bool changeColour = false;
	public Color normalCol;
	public Color angryCol;

	[Header("Unity Events")]
	public UnityEvent onPlayerPunch;
	public UnityEvent onGetExploded;
	public UnityEvent onGetAnnoyed;

	void Start()
	{
		//thePlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		robots = GameObject.FindGameObjectsWithTag("Bot");

		originalDirection = this.transform.forward;
		originalPosition = this.transform.position;

		faceRender.materials[faceMatIndex].EnableKeyword("_NORMALMAP");
	}

	void FixedUpdate()
    {
		if ((patience >= patienceLimit &&
			!IsTimerDone()) || superAnnoyed)
		{
			SetAnimationState("doAngry", true);
			faceRender.materials[faceMatIndex].SetTexture("_MainTex", angry);
			if (changeColour)
				faceRender.materials[faceMatIndex].SetColor("_Color", angryCol);
		}
		else
		{
			SetAnimationState("doAngry", false);
			faceRender.materials[faceMatIndex].SetTexture("_MainTex", normal);
			if (changeColour)
				faceRender.materials[faceMatIndex].SetColor("_Color", normalCol);
		}

		if (!isManagerBot)
		{
			// Tell animator when an assembly box exists
			if (matDetector != null &&
				matDetector.ObjectsInTrigger.Count > 0)
			{
				SetAnimationState("doAssembly", true);
				faceRender.materials[faceMatIndex].SetTexture("_MainTex", normal);
			}
			else
			{
				SetAnimationState("doAssembly", false);
			}
		}

		if (lookAtPlayer && !superAnnoyed)
		{
			SetAnimationState("doDisturbed", true);
			if (faceRender.materials[faceMatIndex].GetTexture("_MainTex") != angry)
			{
				faceRender.materials[faceMatIndex].SetTexture("_MainTex", disturbed);
			}
		}
		else
		{
			SetAnimationState("doDisturbed", false);
			faceRender.materials[faceMatIndex].SetTexture("_MainTex", normal);
			if (changeColour)
				faceRender.materials[faceMatIndex].SetColor("_Color", normalCol);
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
				IsTimerDone() && !keepLooking)
			{
				lookAtPlayer = false;
				//lookTime = 3f;
				ResetLookTimer(lookTime);
				patience = 3;

				SetAnimationState("doAngry", false);
				SetAnimationState("doDisturbed", false);
				faceRender.materials[faceMatIndex].SetTexture("_MainTex", normal);
				if (changeColour)
					faceRender.materials[faceMatIndex].SetColor("_Color", normalCol);

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

		if (patience >= patienceLimit && !isManagerBot && !superAnnoyed)
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
				lookAtPlayer = true;
			}
		}


		if (superAnnoyed)
		{
			SetAnimationState("doAngry", true);
			faceRender.materials[faceMatIndex].SetTexture("_MainTex", angry);
			if (changeColour)
				faceRender.materials[faceMatIndex].SetColor("_Color", angryCol);

			patience = patienceLimit;
		}
	}


    #region Private Methods
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

	// Get angry, then shaking head (completes task)
	public void GetAnnoyed()
	{
		lookAtPlayer = true;
		patience = patienceLimit;
		//lookTime = 3f;
		ResetLookTimer(lookTime);

		faceRender.materials[faceMatIndex].SetTexture("_MainTex", angry);
		if (changeColour)
			faceRender.materials[faceMatIndex].SetColor("_Color", angryCol);

		onGetAnnoyed.Invoke();
	}

	// Get angry non-stop
	public void GetSuperAnnoyed(float time)
	{
		lookAtPlayer = true;
		keepLooking = true;
		ResetLookTimer(lookTime);
		StartCoroutine(WaitThenAnnoy(time));

		if (boxProcessor)
		{
			boxProcessor.SetActive(false);
		}
	}

	IEnumerator WaitThenAnnoy(float secs)
	{
		// wait for x seconds
		yield return new WaitForSeconds(secs);

		SetAnimationState("doAngry", false);
		SetAnimationState("doDisturbed", false);
		superAnnoyed = true;
		onGetAnnoyed.Invoke();

		// Face change
		faceRender.materials[faceMatIndex].SetTexture("_MainTex", angry);
		if (changeColour)
			faceRender.materials[faceMatIndex].SetColor("_Color", angryCol);
	}

	/// <summary>
	/// Sets the body, face, and screen animator's boolean name to the given value.
	/// </summary>
	/// <param name="booleanName">The name of the boolean variable in the animator.</param>
	/// <param name="state">The state of the animation.</param>
	public void SetAnimationState(string booleanName, bool state)
	{
		anim.SetBool(booleanName, state);
		animFace.SetBool(booleanName, state);
		animScreen.SetBool(booleanName, state);
	}
	#endregion
}
