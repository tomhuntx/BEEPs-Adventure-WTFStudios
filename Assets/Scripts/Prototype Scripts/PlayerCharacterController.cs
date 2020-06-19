﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityStandardAssets.Cameras;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ProtectCameraFromWallClip))]
[RequireComponent(typeof(GraphicFader))]
[AddComponentMenu("Third Person Player Controller")]
public class PlayerCharacterController : MonoBehaviour
{
    public enum ForceType { Force, Impulse }

    CharacterController controller;
    Rigidbody rb;
    ProtectCameraFromWallClip antiCamClip;
    GraphicFader graphicFader;

    [Header("Transform References")]
    [SerializeField] Transform sample;
    [SerializeField] Transform parentTransform;
    [SerializeField] Transform characterHead;
    [SerializeField] Transform characterBody;

    [Header("Camera Settings")]
    [SerializeField] private Camera characterCam;
    /// <summary>
    /// Camera responsible for preventing clipping objects when in first person.
    /// </summary>
    private Camera fpAntiClipCam;
    [Range(GameManager.MIN_CAM_FOV, GameManager.MAX_CAM_FOV)][SerializeField] private float thirdPersonFOV = 60.0f;
    [Range(GameManager.MIN_CAM_FOV, GameManager.MAX_CAM_FOV)][SerializeField] private float firstPersonFOV = 90.0f;
    [SerializeField] private float lookSensitivity = 150.0f;
    [SerializeField] private float camTransitionSpeed = 5.0f;
    [Range(0.0001f, 0.5f)] [SerializeField] private float camFlipDistanceDeadzone = 0.3f;
    private float camFlipDistanceTreshold = 0.5f;
    [SerializeField] Vector3[] camOffsets;
    private int currentCamOffsetIndex = 0;
    private bool doTransition = false;
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
    #endregion


    #region Accessors
    public Vector3 CurrentDirection { get { return motionDirection; } }
    public CharacterController Controller { get { return controller; } }
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
        currentCamPos = camOffsets[currentCamOffsetIndex];
        camFlipDistanceTreshold = camOffsets[currentCamOffsetIndex].magnitude * camFlipDistanceDeadzone;
        
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
    }

    // Update is called once per frame
    void Update()
    {
        ManageCamera();
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
        if (otherRB.tag == "Box" ||
            otherRB.tag == "Heavy Box")
        {
            // Bump
            otherRB.velocity = transform.forward * bumpForce;
        }
    }



    #region Private Methods
    private void ManageCamera()
    {
        if (Input.GetButtonDown("Cycle Camera"))
        {
            currentCamOffsetIndex++;
            if (currentCamOffsetIndex >= camOffsets.Length) currentCamOffsetIndex = 0;

            Vector3 newCamPos = camOffsets[currentCamOffsetIndex];
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

        if (!isFirstPerson)
        {
            if (Input.GetButtonDown("Flip Camera"))
            {
                doTransition = true;
                isAutoFlipDone = !isAutoFlipDone;
                isCamFlipped = !isCamFlipped;
                currentCamPos.x *= -1;
            }

            if (doTransition)
            {
                if (Vector3.Distance(antiCamClip.cameraOffset, currentCamPos) > 0.001)
                {
                    antiCamClip.cameraOffset = Vector3.Lerp(antiCamClip.cameraOffset, currentCamPos, camTransitionSpeed * Time.deltaTime);
                }
                else
                {
                    doTransition = false;
                    antiCamClip.cameraOffset = currentCamPos;
                }
            }
            else
            {
                float camDistance = Vector3.Distance(characterHead.position, characterCam.transform.position);
                if (isAutoFlipDone)
                {
                    if (camDistance <= camFlipDistanceTreshold)
                    {
                        currentCamPos.x *= -1;
                        isCamFlipped = !isCamFlipped;

                        doTransition = true;
                        isAutoFlipDone = false;
                    }
                }
                else
                {
                    if (camDistance > camFlipDistanceTreshold)
                        isAutoFlipDone = true;
                }
            }
        }
        else
        {
            if (Vector3.Distance(antiCamClip.cameraOffset, currentCamPos) > 1)
            {
                antiCamClip.cameraOffset = Vector3.Lerp(antiCamClip.cameraOffset, currentCamPos, camTransitionSpeed * Time.deltaTime);
            }
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
        mouseVector.x = Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
        mouseVector.y = Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;

        headRotX -= mouseVector.y;
        headRotX = Mathf.Clamp(headRotX, -90f, 90f);

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
    #endregion
}
