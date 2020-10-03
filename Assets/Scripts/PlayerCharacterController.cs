using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityStandardAssets.Cameras;


[System.Serializable]
public struct TransformPreset
{
    #region Variables
    public Vector3 localPosition;
    public Vector3 localEulerAngles;
    public Vector3 localScale;
    #endregion



    #region Constructors
    public TransformPreset(Vector3 thisPosition, 
                           Vector3 thisEulerRotation, 
                           Vector3 thisScale)
    {
        localPosition = thisPosition;
        localEulerAngles = thisEulerRotation;
        localScale = thisScale;
    }

    public TransformPreset(Vector3 thisPosition,
                            Vector3 thisEulerRotation)
    {
        localPosition = thisPosition;
        localEulerAngles = thisEulerRotation;
        localScale = Vector3.one;
    }

    public TransformPreset(Vector3 thisPosition)
    {
        localPosition = thisPosition;
        localEulerAngles = Vector3.zero;
        localScale = Vector3.one;
    }
    #endregion



    #region Public Methods
    public void UpdateTransform(Transform targetTransform, 
                                bool applyPosition = true, 
                                bool applyRotation = true, 
                                bool applyScaling = true)
    {
        if (applyPosition) targetTransform.localPosition = localPosition;
        if (applyRotation) targetTransform.localEulerAngles = localEulerAngles;
        if (applyScaling) targetTransform.localScale = localScale;
    }

    public void UpdateTransformPosition(Transform targetTransform)
    {
        UpdateTransform(targetTransform, true, false, false);
    }

    public void UpdateTransformRotation(Transform targetTransform)
    {
        UpdateTransform(targetTransform, false, true, false);
    }

    public void UpdateTransformScale(Transform targetTransform)
    {
        UpdateTransform(targetTransform, false, false, true);
    }
    #endregion
}



[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ProtectCameraFromWallClip))]
[RequireComponent(typeof(GraphicFader))]
[AddComponentMenu("Third Person Player Controller")]
public class PlayerCharacterController : MonoBehaviour
{
    public enum ForceType { Force, Impulse }

    private CharacterController controller;
    private Rigidbody rb;
    private ProtectCameraFromWallClip antiCamClip;
    private GraphicFader graphicFader;

    [Header("Transform References")]
    [SerializeField] Transform parentTransform;
    [SerializeField] Transform characterHead;
    [SerializeField] Transform characterBody;

    [Header("Camera Settings")]
    [SerializeField] private Camera characterCam;
    /// <summary>
    /// Camera responsible for preventing clipping objects when in first person.
    /// </summary>
    private Camera fpAntiClipCam;
    [Range(-90, 0)] [SerializeField] private float minCamAngleX = -90;
    public float originalMinCamAngleX { get; private set; }
    [Range(0, 90)] [SerializeField] private float maxCamAngleX = 90;
    public float originalMaxCamAngleX { get; private set; }
    [Range(GameManager.MIN_CAM_FOV, GameManager.MAX_CAM_FOV)] [SerializeField] private float thirdPersonFOV = 60.0f;
    [Range(GameManager.MIN_CAM_FOV, GameManager.MAX_CAM_FOV)] [SerializeField] private float firstPersonFOV = 90.0f;
    [SerializeField] private float lookSensitivity = 150.0f;
    [SerializeField] private float camTransitionSpeed = 5.0f;
    [Range(0.1f, 5.0f)] [SerializeField] private float camFlipDistanceDeadzone = 1.5f;
    private float camFlipDistanceTreshold = 0.5f;
    [SerializeField] private TransformPreset[] camOffsets;
    private int currentCamOffsetIndex = 0;
    private bool doTransition = true;
    private bool isAutoFlipDone = false;
    private bool isCamFlipped = false;
    private bool isFirstPerson;
    private Vector3 currentCamPos;
    private Vector2 mouseVector;
    private float headRotX;
    [SerializeField] private GameObject[] firstPersonGameObjects;
    [SerializeField] private GameObject[] thirdPersonGameObjects;

    [Header("Physics")]
    [SerializeField] private float gravity = -20.0f;
    [SerializeField] private float mass = 1.0f;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float sprintSpeed = 7.0f;
    [SerializeField] private float maxAirSpeed = 10.0f;
    [SerializeField] private float airSpeedMagnitude = 3.0f;
    private float currentMoveSpeed;

    [Header("Other Forces")]
    [Tooltip("Strength of force applied when bumping into rigidbodies.")]
    [SerializeField] private float bumpForce = 2.0f;

    [Tooltip("Maximum force that the player can be pushed by explosive boxes.")]
    [SerializeField] private float maxExpForce = 40;

    [Tooltip("How much the external forces dies down.")]
    [SerializeField] private float externalForceFalloff = 3.0f;

    [SerializeField] private float jumpForce = 10.0f;


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
    public float lookSensitivityMultiplier { private get; set; }
    #endregion


    #region Accessors
    public Vector3 CurrentDirection { get { return motionDirection; } }
    public CharacterController Controller { get { return controller; } }
    public Rigidbody RigidbodyComponent { get { return rb; } set { rb = value; } }
    public Camera CharacterCam { get { return characterCam; } set { characterCam = value; } }
    public Transform CharacterHead { get { return characterHead; } set { characterHead = value; } }
    public float Mass { get { return mass; } }
    public float WalkSpeed { get { return walkSpeed; } }
    public float SprintSpeed { get { return sprintSpeed; } }
    public float CurrentSpeed { get { return currentMoveSpeed; } }
    public float LookSensitivity { get { return lookSensitivity; } }
    public bool JumpingEnabled { get { return jumpingEnabled; } set { jumpingEnabled = value; } }
    public bool IsFirstPerson { get { return isFirstPerson; } }
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        controller = this.GetComponent<CharacterController>();
        rb = this.GetComponent<Rigidbody>();
        antiCamClip = this.GetComponent<ProtectCameraFromWallClip>();
        graphicFader = this.GetComponent<GraphicFader>();
        
        isFirstPerson = Vector3.Distance(characterHead.position, characterCam.transform.position) <= 1;
        TransformPreset currentPreset = camOffsets[currentCamOffsetIndex];
        currentCamPos = currentPreset.localPosition;

		camFlipDistanceTreshold = currentPreset.localPosition.magnitude * camFlipDistanceDeadzone;
        
        //First person anti clip camera setup
        fpAntiClipCam = characterCam.transform.GetChild(0).GetComponent<Camera>();

        //No camera detected, create a new one...
        if (fpAntiClipCam == null)
        {
            GameObject newCam = new GameObject();
            newCam.transform.parent = characterCam.transform;
            newCam.transform.localPosition = Vector3.zero;
            fpAntiClipCam = newCam.AddComponent<Camera>();
		}

        if (isFirstPerson) 
            UpdateCamFOV(firstPersonFOV);
        else
            UpdateCamFOV(thirdPersonFOV);

        originalMinCamAngleX = minCamAngleX;
        originalMaxCamAngleX = maxCamAngleX;

        lookSensitivityMultiplier = SettingsMenu.currentMouseSensitivity;
	}

    // Update is called once per frame
    void Update()
    {
		//ManageCamera();
		LookRotation();
        Move();
        GetMotionDirection();
		ApplyPhysics();

		if (controller.isGrounded)
        {
            if (!doMoveSpeedOverride)
            {
                if (Input.GetButton("Sprint")) currentMoveSpeed = sprintSpeed;
                else currentMoveSpeed = walkSpeed;
            }
        }

        if (mass <= 0)
            mass = 0.00000001f;

        //Debug.DrawLine(characterHead.position, characterHead.position + characterHead.forward * 5, Color.cyan);
    }

    private void LateUpdate()
    {
        ManageCamera();
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
        switch(otherRB.tag)
        {
            case "Box":
            case "Heavy Box":
            case "Explosive Box":
                Vector3 moveVelocity = velocity;
                moveVelocity.y = 0;
                otherRB.velocity = (moveVelocity * bumpForce) / otherRB.mass;
                break;
        }

        /*
        if (otherRB.tag == "Box" ||
            otherRB.tag == "Heavy Box" ||
			otherRB.tag == "Bumpable")
		{
            // Bump
            //otherRB.velocity = transform.forward * bumpForce;
            Vector3 moveVelocity = velocity;
            moveVelocity.y = 0;
            otherRB.velocity = (moveVelocity * bumpForce) / otherRB.mass;
        }
        */
    }



    #region Private Methods
    private void ManageCamera()
    {
        if (camOffsets.Length > 1 && 
            Input.GetButtonDown("Cycle Camera"))
        {
            currentCamOffsetIndex++;
            if (currentCamOffsetIndex >= camOffsets.Length) currentCamOffsetIndex = 0;

            Vector3 newCamPos = camOffsets[currentCamOffsetIndex].localPosition;
            if (isCamFlipped) newCamPos.x *= -1;
            currentCamPos = newCamPos;
            camFlipDistanceTreshold = newCamPos.magnitude * camFlipDistanceDeadzone;

            doTransition = true;

            if (Vector3.Distance(characterHead.localPosition, newCamPos) <= 1.5f)
            {
                isFirstPerson = true;
                ManageRendering(isFirstPerson);
                UpdateCamFOV(firstPersonFOV);
                graphicFader.PlaySequence();
            }
            else
            {
                if (isFirstPerson)
                {
                    if (graphicFader.IsSequenceFinished)
                        graphicFader.PlaySequence();

                    antiCamClip.enabled = true;
                    isFirstPerson = false;
                    UpdateCamFOV(thirdPersonFOV);
                    ManageRendering(isFirstPerson);
                }                
            }
        }

        characterCam.transform.localEulerAngles = Vector3.Lerp(characterCam.transform.localEulerAngles, 
                                                               camOffsets[currentCamOffsetIndex].localEulerAngles, 
                                                               camTransitionSpeed * Time.deltaTime);

        //Third person camera management
        if (!isFirstPerson)
        {
            //Switch camera sides manually
            if (camOffsets[currentCamOffsetIndex].localPosition.x != 0 && 
                Input.GetButtonDown("Flip Camera"))
            {
                doTransition = true;
                isAutoFlipDone = !isAutoFlipDone;
                isCamFlipped = !isCamFlipped;
                currentCamPos.x *= -1;
            }

            //Camera transition management
            if (doTransition)
            {
                //Smoothen camera transitions in third person
                if (Vector3.Distance(antiCamClip.cameraOffset, currentCamPos) > 0.001)
                {
					antiCamClip.cameraOffset = Vector3.Lerp(antiCamClip.cameraOffset, currentCamPos, camTransitionSpeed * Time.deltaTime);
                }
                //Jump camera directly to firstperson
                else
                {
                    doTransition = false;
					antiCamClip.cameraOffset = currentCamPos;
                }
            }
            //Manage camera side switching
            else
            {
                //Check the right side of the player by default
                int multiplier = 1;

                //Check left side since that's where the side the camera is currently at
                if (characterCam.transform.localPosition.x < 0) multiplier = -1; 
                
                //Flip camera if an obstacle is within the deadzone
                Ray direction = new Ray(characterBody.position, characterBody.right * multiplier);
                bool doFlip = Physics.Raycast(direction, camFlipDistanceDeadzone);
                
                //Only enable camera flip when the transition is done
                if (isAutoFlipDone)
                {
                    if (doFlip)
                    {
                        currentCamPos.x *= -1;
                        isCamFlipped = !isCamFlipped;

                        doTransition = true;
                        isAutoFlipDone = false;
                    }
                }
                //Set auto flip to done if it's not currently switching sides
                else
                {
                    if (!doFlip)
                        isAutoFlipDone = true;
                }
            }
        }
        //First person camera management
        else
        {
            //Smoothen transition to first person
            if (Vector3.Distance(antiCamClip.cameraOffset, currentCamPos) > 1)
            {
                antiCamClip.cameraOffset = Vector3.Lerp(antiCamClip.cameraOffset, currentCamPos, camTransitionSpeed * Time.deltaTime);
            }
            //Jump to camera offset if within deadzone
            else
            {
                antiCamClip.cameraOffset = currentCamPos;
                characterCam.transform.localPosition = currentCamPos;
                antiCamClip.enabled = false;
            }
        }
    }

    private void LookRotation()
    {
        mouseVector.x = Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime * lookSensitivityMultiplier;
        mouseVector.y = Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime * lookSensitivityMultiplier;

        headRotX -= mouseVector.y;
        headRotX = Mathf.Clamp(headRotX, minCamAngleX, maxCamAngleX);

        characterHead.transform.localRotation = Quaternion.Euler(headRotX, 0f, 0f);
        parentTransform.Rotate(Vector3.up * mouseVector.x);
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

            newVelocity = direction * currentMoveSpeed;
            groundedMotion = direction;
            currentAirSpeed = currentMoveSpeed;
        }
        else
        {
            Vector3 newDirection = Vector3.zero;

            if (wasMoving)
            {
                newDirection = groundedMotion;
                currentAirSpeed += newDirection.magnitude * currentMoveSpeed * Time.deltaTime;
            }
            else
            {
                newDirection = direction;
                currentAirSpeed = direction.magnitude * currentMoveSpeed;
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
        velocity.y -= gravity * mass * Time.deltaTime;

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

    /// <summary>
    /// Switches game objects' layer masks.
    /// </summary>
    private void ManageRendering(bool doRenderFirstperson)
    {
        string thirdPersonLayer = string.Empty;
        string firstPersonLayer = string.Empty;

        if (doRenderFirstperson)
        {
            thirdPersonLayer = "Ignore Render";
            firstPersonLayer = "Grabbed Object";
        }
        else
        {
            thirdPersonLayer = "Default";
            firstPersonLayer = "Ignore Render";
        }

        if (thirdPersonGameObjects.Length > 0)
        {
            for (int i = 0; i < thirdPersonGameObjects.Length; i++)
            {
                thirdPersonGameObjects[i].gameObject.layer = LayerMask.NameToLayer(thirdPersonLayer);
            }
        }

        if (firstPersonGameObjects.Length > 0)
        {
            for (int i = 0; i < firstPersonGameObjects.Length; i++)
            {
                firstPersonGameObjects[i].gameObject.layer = LayerMask.NameToLayer(firstPersonLayer);
            }
        }
    }
    #endregion



    #region Public Methods
    /// <summary>
    /// Updates the min/max clamp values for rotating the camera in the x-axis.
    /// </summary>
    /// <param name="min">This value is clamped between -90 and 0, both inclusive.</param>
    /// <param name="max">This value is clamped between 0 and 90, both inclusive.</param>
    public void UpdateCamAngleClamp(float min, float max)
    {
        minCamAngleX = Mathf.Clamp(min, -90, 90);
        maxCamAngleX = Mathf.Clamp(max, -90, 90);
    }

    /// <summary>
    /// Reverts to the original clamping angles for the camera's rotation in the x-axis.
    /// </summary>
    public void RevertCamAngleClamp()
    {
        minCamAngleX = originalMinCamAngleX;
        maxCamAngleX = originalMaxCamAngleX;
    }

    /// <summary>
    /// Updates the camera's FoV accordingly.
    /// </summary>
    /// <param name="FoV">The new field of view value.</param>
    public void UpdateCamFOV(float FoV = float.NaN)
    {
        Camera thirdPersonCam = characterCam;

        if (FoV != float.NaN)
        {
            characterCam.fieldOfView = FoV;
            fpAntiClipCam.fieldOfView = FoV;
        }
            
    }

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
                externalForce += motion * Time.deltaTime * 0.297f;
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
    public void CeilingHit(bool isHit)
    {
        //if (velocity.y > 0 && isHit) velocity.y *= -1f;
        isCeilingHit = isHit;
    }

    public static ForceType ConvertFromForceMode(ForceMode forceMode)
    {
        switch (forceMode)
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
        currentMoveSpeed = newSpeed;
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

    public void NegateForces()
    {
        externalForce = Vector3.zero;
        velocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        //rb.inertiaTensor = Vector3.zero;
        //rb.inertiaTensorRotation = Quaternion.identity;
    }

	public void SetGravity(float newGravity)
	{
		gravity = newGravity;
	}

    #endregion
}
