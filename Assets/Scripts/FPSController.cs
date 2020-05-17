using UnityEngine;
using System.Collections;
using System.Threading;
using UnityEngine.Rendering.Universal;

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

	[Header("Movement Settings")]
	[SerializeField] private float mass = 1.0f;
	[SerializeField] private float walkSpeed = 5.0f;
	[SerializeField] private float sprintSpeed = 7.0f;
	private float currentSpeed = 0;

	[SerializeField] private float airControlSpeed = 2.0f;

	[Tooltip("Affects jumping force.")]
	[SerializeField] private float jumpAccelerationRate = 2.0f;

	[Tooltip("Strength of force applied when bumping into rigidbodies.")]
	[SerializeField] private float bumpForce = 2.0f;

	[Tooltip("Speed ratio while mid-air (1=velocity is not affected while mid-air).")]
	[SerializeField] private float airSpeedRatio = 0.8f;

	[Tooltip("Maximum force that the player can be pushed by explosive boxes.")]
	[SerializeField] private float maxExpForce = 40;

	[Tooltip("How much the external forces dies down.")]
	[SerializeField] private float externalForceFalloff = 3.0f;

	[SerializeField] private float gravity = -20.0f;
	[SerializeField] private float jumpForce = 10.0f;
	#endregion


	#region Hidden Variables
	private float horizonal;
	private float vertical;
	private Vector3 direction;
	private Vector3 velocity;
	private Vector3 motionDirection;
	private Vector3 previousPosition;
	private Vector3 impact = Vector3.zero;
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
		Cursor.lockState = CursorLockMode.Locked;
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

	/* OLD MOVEMENT
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

		//prevents exceeding movement speed
		direction = transform.right * horizonal + transform.forward * vertical;
		direction.z = Mathf.Clamp(direction.z, -1, 1);
		direction.x = Mathf.Clamp(direction.x, -1, 1);

		// Move
		if (controller.isGrounded)
		{
			controller.Move(direction * currentSpeed * Time.deltaTime);

			if (!IsMoving())
			{
				wasMoving = false;
			}
			else
			{
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

				controller.Move(motionDirection * currentSpeed * airSpeedRatio * Time.deltaTime);
			}
			else
			{
				controller.Move(direction * currentSpeed * airSpeedRatio * Time.deltaTime);
			}
		}
		// Apply gravity
		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);

		// Explosion impact
		if (impact.magnitude > 0.2f)
		{
			// Apply impact force
			controller.Move(impact * Time.deltaTime);

			// Reduce impact force over time
			impact = Vector3.Lerp(impact, Vector3.zero, 3 * Time.deltaTime);
			wasMoving = false;
		}
	}
	*/

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
				//print("Jumped");
			}

			if (IsMoving())
				wasMoving = true;
			else
				wasMoving = false;

			newVelocity = direction * currentSpeed;		
		}
		else
		{
			Vector3 newDirection = Vector3.zero;

			if (wasMoving)
			{
				if (!IsMoving()) direction = Vector3.zero;
				motionDirection += direction * airControlSpeed * Time.deltaTime;
				motionDirection.x = Mathf.Clamp(motionDirection.x, -1, 1);
				motionDirection.z = Mathf.Clamp(motionDirection.z, -1, 1);

				newDirection = motionDirection;
			}
			else
			{
				newDirection = direction;
			}
			newVelocity = newDirection * currentSpeed * airSpeedRatio;
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
	/* Push From Point - Unused
	/// <summary>
	///	Pushes the player from a point by a force
	/// Used by the explosive box's explosion
	/// </summary>
	/// <param name="pos">Position of explosion</param>
	/// <param name="force">Force of explosion (relative to range)</param>
	public void PushFromPoint(Vector3 pos, float force)
	{
		// Direction of push-back
		Vector3 direction = transform.position - pos;
		//direction.y += 4;
		direction.Normalize();
		
		// Create impact force in this direction
		impact += direction * force / mass; //2 represents object mass

		// Clamp the impact force to stop the player being sent to space
		if (impact.magnitude > maxExpForce)
		{
			impact = Vector3.ClampMagnitude(impact, maxExpForce);
		}
	}
	*/

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
