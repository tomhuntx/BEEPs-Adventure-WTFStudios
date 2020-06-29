using UnityEngine;
using System.Collections;
using System.Threading;
using UnityEngine.Rendering;
using System;

[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("First Person Player Controller")]
public class FPSController : MonoBehaviour
{
	public enum ForceType { Force, Impulse}

    #region Exposed Variables
    [SerializeField] private Transform parentTransform;
	[SerializeField] private Camera mainCam;
	private CharacterController controller;

	[Header("Camera Settings")]
	[SerializeField] private float sensitivity = 300f;
	private float xRotation = 0f;
	private float mouseX;
	private float mouseY;


	[Header("Physics")]
	[SerializeField] private float gravity = -20.0f;
	[SerializeField] private float mass = 1.0f;

	[Header("Movement Settings")]	
	[SerializeField] private float walkSpeed = 5.0f;
	[SerializeField] private float sprintSpeed = 7.0f;
	[SerializeField] private float maxAirSpeed = 10.0f;
	[SerializeField] private float airSpeedMagnitude = 3.0f;
	private float currentSpeed = 0;
	
	[Header("Other Forces")]
	[Tooltip("Strength of force applied when bumping into rigidbodies.")]
	[SerializeField] private float bumpForce = 2.0f;

	[Tooltip("Maximum force that the player can be pushed by explosive boxes.")]
	[SerializeField] private float maxExpForce = 40;

	[Tooltip("How much the external forces dies down.")]
	[SerializeField] private float externalForceFalloff = 3.0f;

	[SerializeField] private float jumpForce = 10.0f;
	#endregion


	#region Hidden Variables
	private Vector3 direction;
	private Vector3 velocity;
	private Vector3 motionDirection;
	private Vector3 groundedMotion;
	private float currentAirSpeed;
	private Vector3 previousPosition;
	private float directionTimer;
	private float previousPosTimer;
	private bool wasMoving = false;

	private Vector3 externalForce;
	private bool isCeilingHit = false;

	private bool doMoveSpeedOverride = false;
	private bool doLookSpeedOverride = false;
	private bool jumpingEnabled = true;
	private float currentLookSpeed;
	#endregion


	#region Accessors
	public Vector3 CurrentDirection { get { return motionDirection; } }
	public CharacterController Controller { get { return controller; } }
	public Camera MainCam { get { return mainCam; } set { mainCam = value; } }
	public float Mass { get { return mass; } }
	public float WalkSpeed { get { return walkSpeed; } }
	public float SprintSpeed { get { return sprintSpeed; } }
	public float CurrentSpeed { get { return currentSpeed; } }
	public float LookSensitivity { get { return sensitivity; } }
	public bool JumpingEnabled { get { return jumpingEnabled; } set { jumpingEnabled = value; } }
	#endregion



	private void Start()
	{
		controller = GetComponent<CharacterController>();
		currentSpeed = walkSpeed;
		currentLookSpeed = sensitivity;
	}

	void Update()
    {
		Look();
		Move();
		GetMotionDirection();
		ApplyPhysics();

		if (controller.isGrounded)
		{
			if (!doMoveSpeedOverride)
			{
				if (Input.GetButton("Sprint")) currentSpeed = sprintSpeed;
				else currentSpeed = walkSpeed;
			}
		}

		if (mass <= 0) 
			mass = 0.00000001f;		
	}

	void FixedUpdate()
	{
		//Look();
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


    #region Private Methods
    private void Look()
	{
		if (!doLookSpeedOverride) currentLookSpeed = sensitivity;

		mouseX = Input.GetAxis("Mouse X") * currentLookSpeed * Time.deltaTime;
		mouseY = Input.GetAxis("Mouse Y") * currentLookSpeed * Time.deltaTime;

		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);

		mainCam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		parentTransform.Rotate(Vector3.up * mouseX);
	}
	
	private void Move()
	{
		Vector3 newVelocity = Vector3.zero;

		//Movement direction
		direction = Input.GetAxis("Horizontal") * transform.right +
					Input.GetAxis("Vertical") * transform.forward;
		direction.z = Mathf.Clamp(direction.z, -1, 1);
		direction.x = Mathf.Clamp(direction.x, -1, 1);

		if (controller.isGrounded)
		{
			if (velocity.y < 0) velocity.y = -2.0f;

			if (jumpingEnabled && Input.GetButtonDown("Jump"))
			{
				velocity.y += jumpForce;
			}

			if (IsMoving())
				wasMoving = true;
			else
				wasMoving = false;

			newVelocity = direction * currentSpeed;
			groundedMotion = direction;
			currentAirSpeed = currentSpeed;
		}
		else
		{
			Vector3 newDirection = Vector3.zero;
			
			if (wasMoving)
			{
				newDirection = groundedMotion;
				currentAirSpeed += newDirection.magnitude * currentSpeed * Time.deltaTime;
			}
			else
			{
				newDirection = direction;
				currentAirSpeed = direction.magnitude * currentSpeed;
			}
			newVelocity = newDirection * currentAirSpeed + (direction * airSpeedMagnitude);
			newVelocity = Vector3.ClampMagnitude(newVelocity, maxAirSpeed);
		}
		velocity = new Vector3(newVelocity.x,
							   velocity.y,
							   newVelocity.z);			
	}

	private void ApplyPhysics()
	{
		//Apply gravity
		velocity.y += gravity * mass * Time.deltaTime;

		//Apply movement
		if (isCeilingHit)
		{
			if (velocity.y > 0) velocity.y *= -1f;
			if (externalForce.y > 0) externalForce.y *= -1f;
		}

		//Decay external force only when grounded
		if (controller.isGrounded)
		{
			externalForce.y = 0;
			externalForce = Vector3.Lerp(externalForce, Vector3.zero, externalForceFalloff * Time.deltaTime);
		}

		// Cap external force to prevent overshooting to space
		if (externalForce.magnitude > maxExpForce)
			externalForce = Vector3.ClampMagnitude(externalForce, maxExpForce);

		controller.Move(((velocity + externalForce) / mass) * Time.deltaTime);
	}

	/// <summary>
	/// Only calculates movement direction when grounded.
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
	#endregion

	#region Public Methods
	/// <summary>
	/// Moves the transform of the game object this FPSController attached to.
	/// </summary>
	/// <param name="motion">Direction x Force - Exclude time delta multiplication.</param>
	/// <param name="forceType">Type of force applied.</param>
	public void ApplyForce(Vector3 motion, ForceType forceType)
	{
		switch (forceType)
		{
			case ForceType.Force:
				externalForce += motion * Time.deltaTime;
				break;
			case ForceType.Impulse:
				externalForce += motion;
				break;
		}		
	}

	/// <summary>
	/// Bounce off this transform if it hits a ceiling.
	/// </summary>
	/// <param name="isHit">Reverses upward motion.</param>
	public void CeilingHit (bool isHit)
	{
		//if (velocity.y > 0 && isHit) velocity.y *= -1f;
		isCeilingHit = isHit;
	}

	public static ForceType ConvertFromForceMode(ForceMode forceMode)
	{
		switch(forceMode)
		{
            case ForceMode.Impulse:
				return ForceType.Impulse;

			case ForceMode.VelocityChange:
				return ForceType.Impulse;

			case ForceMode.Force:
				return ForceType.Force;

			case ForceMode.Acceleration:
				return ForceType.Force;

			default:
				return ForceType.Force;
		}
	}

	public void OverrideMoveSpeed(float newSpeed)
	{
		currentSpeed = newSpeed;
		doMoveSpeedOverride = true;
	}

	public void OverrideLookSpeed(float newSpeed)
	{
		currentLookSpeed = newSpeed;
		doLookSpeedOverride = true;
	}

	public void RevertMoveSpeed()
	{
		doMoveSpeedOverride = false;
	}

	public void RevertLookSpeed()
	{
		doLookSpeedOverride = false;
	}
	#endregion
}
