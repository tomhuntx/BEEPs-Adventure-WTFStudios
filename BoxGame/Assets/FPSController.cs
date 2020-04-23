using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("First Person Player Controller")]
public class FPSController : MonoBehaviour
{
	[SerializeField] private Transform playerBody;
	[SerializeField] private Camera mainCam;
	CharacterController controller;

	[Header("Camera Settings")]
	[SerializeField] private float sensitivity = 300f;
	private float xRotation = 0f;
	private float mouseX;
	private float mouseY;
	
	[Header("Movement Settings")]
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float airControlSpeed = 2.0f;
	[SerializeField] private float gravity = -9f;
	[SerializeField] private float jumpHeight = 3f;

	private float currentSpeed;
	private float horizonal;
	private float vertical;
	private Vector3 direction;
	private Vector3 velocity;
	private Vector3 motionDirection;
	private Vector3 previousPosition;
	private float directionTimer;
	private float previousPosTimer;
	private bool wasMoving = false;


	private void Start()
	{
		controller = GetComponent<CharacterController>();
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update()
    {
		Look();
		Move();
		GetMotionDirection();
	}

	private void Look()
	{
		mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
		mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);

		mainCam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		playerBody.Rotate(Vector3.up * mouseX);
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
			controller.Move(direction * moveSpeed * Time.deltaTime);

			if (Input.GetAxis("Horizontal") == 0 &&
				Input.GetAxis("Vertical") == 0)
			{
				wasMoving = false;
			}
			else if (Input.GetAxis("Horizontal") != 0 ||
					 Input.GetAxis("Vertical") != 0)
			{
				wasMoving = true;
			}
		}
		else
		{
			motionDirection += direction * Time.deltaTime * airControlSpeed;
			motionDirection.x = Mathf.Clamp(motionDirection.x, -1, 1);
			motionDirection.z = Mathf.Clamp(motionDirection.z, -1, 1);

			if (wasMoving)
			{
				controller.Move(motionDirection * moveSpeed * Time.deltaTime);
			}
			else
			{
				controller.Move(direction * moveSpeed * Time.deltaTime);
			}
		}

		// Apply gravity
		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
	}

	/// <summary>
	/// Only calculate a direction when grounded.
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
}
