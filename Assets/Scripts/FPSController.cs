﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("First Person Player Controller")]
public class FPSController : MonoBehaviour
{
	[SerializeField] private Transform parentTransform;
	[SerializeField] private Camera mainCam;
	private CharacterController controller;

	[Header("Camera Settings")]
	[SerializeField] private float sensitivity = 300f;
	private float xRotation = 0f;
	private float mouseX;
	private float mouseY;
	
	[Header("Movement Settings")]
	[SerializeField] private float walkSpeed = 5.0f;
	[SerializeField] private float sprintSpeed = 7.0f;
	private float currentSpeed = 0;

	[SerializeField] private float airControlSpeed = 2.0f;

	[Tooltip("Affects jumping force.")]
	[SerializeField] private float jumpAccelerationRate = 2.0f;

	[Tooltip("Strength of force applied when bumping into rigidbodies.")]
	[SerializeField] private float bumpForce = 2.0f;

	//[Tooltip("Value below this will enable full speed mid-air control.")]
	//[SerializeField] private float jumpAccelerationThreshold = 0.5f;

	[SerializeField] private float gravity = -9f;
	[SerializeField] private float jumpHeight = 3f;


	private float horizonal;
	private float vertical;
	private Vector3 direction;
	private Vector3 velocity;
	private Vector3 motionDirection;
	private Vector3 previousPosition;
	private float directionTimer;
	private float previousPosTimer;
	private bool wasMoving = false;
	//private float currentAcceleration;

	/// <summary>
	/// Used for scaled direction calculation
	/// </summary>
	private Vector3 initialPos;


	public Vector3 CurrentDirection { get { return motionDirection; } }
	public CharacterController Controller { get { return controller; } }
	public Camera MainCam { get { return mainCam; } set { mainCam = value; } }


	private void Start()
	{
		controller = GetComponent<CharacterController>();
		Cursor.lockState = CursorLockMode.Locked;
		currentSpeed = walkSpeed;
	}

	void Update()
    {
		Look();
		Move();
		GetMotionDirection();

		if (controller.isGrounded)
		{
			if (Input.GetButton("Sprint")) currentSpeed = sprintSpeed;
			else currentSpeed = walkSpeed;
		}
	}


	private void Look()
	{
		mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
		mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);

		mainCam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		parentTransform.Rotate(Vector3.up * mouseX);
	}

	private void Move()
	{
		// Reset velocity when on ground
		if (controller.isGrounded && velocity.y < 0)
		{
			velocity.y = -2f;
		}

		// Jump
		if (Input.GetButtonDown("Jump") && controller.isGrounded)
		{
			velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
		}

		// Set movement direction
		horizonal = Input.GetAxis("Horizontal");
		vertical = Input.GetAxis("Vertical");
		direction = transform.right * horizonal + transform.forward * vertical;

		// Move
		if (controller.isGrounded)
		{
			controller.Move(direction * currentSpeed * Time.deltaTime);

			if (!IsMoving())
			{
				wasMoving = false;
				//currentAcceleration = 0;
			}
			else
			{
				//Acceleration
				//currentAcceleration += jumpAccelerationRate * Time.deltaTime;
				//currentAcceleration = Mathf.Clamp(currentAcceleration, 0, 1);

				//if (currentAcceleration <= jumpAccelerationThreshold)
				//	wasMoving = false;
				//else
				//	wasMoving = true;

				wasMoving = true;
			}
		}
		else
		{
			if (wasMoving)
			{
				if (!IsMoving()) direction = Vector3.zero;
				motionDirection += direction * airControlSpeed * Time.deltaTime;
				motionDirection.x = Mathf.Clamp(motionDirection.x, -1, 1);
				motionDirection.z = Mathf.Clamp(motionDirection.z, -1, 1);

				//controller.Move(motionDirection * currentAcceleration * moveSpeed * Time.deltaTime);
				controller.Move(motionDirection * currentSpeed * Time.deltaTime);
			}
			else
			{
				controller.Move(direction * currentSpeed * Time.deltaTime);
			}
		}

		// Apply gravity
		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
	}

	/// <summary>
	/// Only calculate movement direction when grounded.
	/// </summary>
	private void GetMotionDirection()
	{
		if (controller.isGrounded)
		{
			if (directionTimer < Time.time)
			{
				directionTimer = 0.02f + Time.time;
				motionDirection = Vector3.Normalize(this.transform.position - previousPosition);
				
			}

			if (previousPosTimer < Time.time)
			{
				previousPosTimer = 0.01f + Time.time;
				previousPosition = this.transform.position;
			}
		}
		else
		{
			directionTimer = Time.time;
			previousPosTimer = Time.time;
		}
	}

	/// <summary>
	/// Checks for movement input activity.
	/// </summary>
	private bool IsMoving()
	{
		if (Input.GetAxis("Horizontal") != 0 ||
			Input.GetAxis("Vertical") != 0)
			return true;

		else if (Input.GetAxis("Horizontal") == 0 &&
			Input.GetAxis("Vertical") == 0)
			return false;

		return false;
	}

	/// <summary>
	/// Push the rigidbodies of all boxes that the player touches
	/// </summary>
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody otherRB = hit.collider.attachedRigidbody;

		// Return null for objects without rigidbodies or if object is below player
		if (otherRB == null || otherRB.isKinematic || hit.moveDirection.y < -0.3f)
			return;

		// Only bump boxes
		if (otherRB.tag == "Box")
		{
			// Bump
			otherRB.velocity = transform.forward * bumpForce;
		}
	}
}
