using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using TMPro;

[RequireComponent(typeof(PlayerCharacterController))]
public class Player : MonoBehaviour
{
    public static Player Instance;

    public GameObject hand;
    private Animator handAnim;
    private GameObject heavyBoxRef;

    #region Exposed Variables
    [Space]
    [Tooltip("The dynamic crosshair to be used for pointing out interactable/grabbable objects")]
    public Graphic crosshair;

    [Tooltip("The crosshair used to visualize the current raycast hits.")]
    public Graphic crosshair2;

    [Tooltip("Used to check if the player has landed or not.")]
    [SerializeField] private UnityEventsHandler groundCheck;

    [Header("Box Handling Properties")]
    [Tooltip("How much damage will be applied to the destructible object.")]
    [SerializeField] private float punchDamage = 3.0f;

    [Tooltip("Strength of impulse force applied upon throwing.")]
    [SerializeField] private float throwForce = 20.0f;

    [Tooltip("Strength of impulse force applied upon dropping a non-box object.")]
    [SerializeField] private float dropForce = 10.0f;

    [Tooltip("The transform where the grabbed objects will be placed in third person.")]
    [SerializeField] private Transform thirdPersonObjectOffset;

    [Tooltip("The transform where the grabbed objects will be placed in first person.")]
    [SerializeField] private Transform firstPersonObjectOffset;

    [Tooltip("The offset position where the grabbed object's initial position will be before throwing.")]
    [SerializeField] private Vector3 thrownObjectOffsetPos;

    [Tooltip("The position of the grabbed object in this transform's local space.")]
    [SerializeField] private Vector3 objectOffset;

    [Tooltip("A variant of the object offset for non-box items.")]
    [SerializeField] private Vector3 otherObjectOffset;

    [Tooltip("The transform where the raycast will originate.")]
    [SerializeField] private Transform raycastOrigin;

    [Tooltip("How far the raycast is.")]
    [SerializeField] private float raycastDistance = 4f;

    [Tooltip("How far from the player's body is an object interactable.")]
    [SerializeField] private Vector3 interactionDistance = new Vector3(2, 10, 2);

    [Tooltip("The area around the player where box placement should be ignored.")]
    [SerializeField] private float boxPlacementDeadzone = 1f;

	[Tooltip("Whether or not we want a second moving crosshair.")]
	[SerializeField] private bool SecondCrosshair = false;

	[Header("Tutorial - Limit Controls")]
	[Tooltip("If it is the tutorial - limits doesnt allow punch or throw if not placed boxes.")]
	[SerializeField] private bool Tutorial = false;
	#endregion

	#region Hidden Variables
	private PlayerCharacterController controller;
    private PlayerSFX sfxController;
    private RaycastHit hitInfo;
    private bool isRaycastHit = false;
    private GameObject heavyBox;
    private Rigidbody heavyBoxRB;
    private GrabbableObject grabbedObject;
    private Transform previousRaycastTarget;
	// Tutorial control limiting
	private bool allowBox = true;
	private bool allowThrow = true;
	private bool allowPunch = true;

	[Header("Animation")]
	[Tooltip("BEEP's body's Animator.")]
	[SerializeField] private Animator bodyAnim;
	[Tooltip("BEEP's head's Animator.")]
	[SerializeField] private Animator headAnim;

	/// <summary>
	/// Checker if the raycast target is behind an object.
	/// </summary>
	private bool isTargetBehindSometing = false;
    #endregion

    public PlayerCharacterController PlayerMovementControls { get { return controller; } }

    private void Awake()
    {
        Instance = this;
	}

    // Start is called before the first frame update
    void Start()
    {
        controller = this.GetComponent<PlayerCharacterController>();
        sfxController = this.GetComponent<PlayerSFX>();
        handAnim = hand.GetComponent<Animator>();
        heavyBoxRef = new GameObject();

		if (Tutorial)
		{
			allowBox = false;
			allowThrow = false;
			allowPunch = false;
		}

		// Temp cursor lock
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

    void LateUpdate()
    {
        //Do check if there's a hit, else no hit
        if (DoRaycast(out hitInfo))
        {
            //Get Distance from player's center to ray hit point
            Vector3 diff = hitInfo.point - this.transform.position;

            //Prevent negative values
            diff.x = Mathf.Abs(diff.x);
            diff.y = Mathf.Abs(diff.y);
            diff.z = Mathf.Abs(diff.z);

            //Check if the ray hit is within interaction specifications
            isRaycastHit = diff.x <= interactionDistance.x &&
                           diff.y <= interactionDistance.y &&
                           diff.z <= interactionDistance.z;
        }
        else
        {
            isRaycastHit = false;
        }

		// Allow grabbing boxes in tutorial after jump
		if (Input.GetButtonDown("Jump"))
		{
			allowBox = true;

			// Jump animation
			bodyAnim.SetTrigger("Jump");
			headAnim.SetTrigger("Jump");
		}

		if (Time.timeScale > 0)
        {
			ManageCrosshair();

            if (grabbedObject != null)
            {
                VisualizeHighlighter();

                if (controller.IsFirstPerson)
                {
                    grabbedObject.RenderToLayer("Grabbed Object");
                    grabbedObject.transform.parent = firstPersonObjectOffset;
                    grabbedObject.transform.position = firstPersonObjectOffset.position;
                    grabbedObject.transform.rotation = firstPersonObjectOffset.rotation;
                }
                else
                {
                    grabbedObject.RenderToLayer("Default");
                    grabbedObject.transform.parent = thirdPersonObjectOffset;
                    grabbedObject.transform.position = thirdPersonObjectOffset.position;
                    grabbedObject.transform.rotation = thirdPersonObjectOffset.rotation;

                }

                if (Input.GetButtonDown("Place Object")) PlaceGrabbedObject();
                if (Input.GetButtonDown("Throw Object") && allowThrow) ThrowGrabbedObject();
                //if (Input.GetButtonDown("Drop Object")) DropGrabbedObject();
            }
            else
            {
                if (Input.GetButtonDown("Grab Object") && allowBox) GrabObject();
                if (Input.GetButtonDown("Punch") && allowPunch)
                {
                    PunchObject();
                    handAnim.SetBool("isPunching", true);

					// Punch animation
					bodyAnim.SetTrigger("Punch");
					headAnim.SetTrigger("Punch");
				}
                else
                {
                    handAnim.SetBool("isPunching", false);
                }
                HighlightTargetObject();
                DragHeavyBox();
            }
        }


        //Debug raycast - can't build with this!
        //if (Input.GetKeyDown(KeyCode.P)) EditorApplication.isPaused = true;
		
        //if (isRaycastHit)
        //{
        //    Debug.DrawLine(raycastOrigin.position, hitInfo.point, Color.green);
        //    Debug.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal, Color.magenta);
        //    //print(hitInfo.transform);
        //}
        //else
        //{
        //    Debug.DrawRay(raycastOrigin.position, raycastOrigin.forward * raycastDistance, Color.cyan);
        //}
    }

    #region Private Methods
    /// <summary>
    /// Shows the highlighter of an interactable object.
    /// </summary>
    private void HighlightTargetObject()
    {
        InteractableObject previousInteractable = null;
        if (previousRaycastTarget != null)
        {
            previousInteractable = previousRaycastTarget.GetComponentInChildren<InteractableObject>();
        }

        if (isRaycastHit)
        {
            InteractableObject interactable = hitInfo.transform.GetComponentInChildren<InteractableObject>();

            if (interactable == null)
            {
                if (previousRaycastTarget != null &&
                    previousInteractable != null)
                {
                    //Hide the previous raycasted target's highlighter
                    //when looking at an object with no interactable object component
                    previousInteractable.ShowHighlighter(false);
                }
			}
            else
            {
				if (previousRaycastTarget != null)
                {
                    if (interactable.transform != hitInfo.transform)
                    {
                        //Hides the childed interactable object if the raycasted 
                        //transform doesn't have an interactable object component
                        interactable.ShowHighlighter(false);
                    }
                    else
                    {
                        //The previous raycasted target is the same as the current one
                        interactable.ShowHighlighter(true);
					}

					if (previousRaycastTarget != interactable.transform &&
                        previousInteractable != null)
                    {
                        //Hide the previous raycasted target's highlighter
                        //when looking at a different object
                        previousInteractable.ShowHighlighter(false);
                    }
				}
                else
                {
                    //No previous raycast target
                    //Show this interactable's highlighter
                    interactable.ShowHighlighter(true);
				}

			}
            previousRaycastTarget = hitInfo.transform;
		}
        else if (!isRaycastHit &&
                 previousRaycastTarget != null)
        {
            //No raycast hit and there's a previous raycasted target
            //Hide the previous raycasted target's highlighter if there's any
            if (previousInteractable != null)
                previousInteractable.ShowHighlighter(false);

            //Put null reference since there's not raycast hit
            previousRaycastTarget = null;
		}
	}

    /// <summary>
    /// Visualize and manages the grabbed object's highlighter - used for object placement.
    /// </summary>
    private void VisualizeHighlighter()
    {
        if (isRaycastHit)
        {
            bool isBox = IsGameObjectBox(grabbedObject.gameObject);

            //Both the raycast hit and the grabbed object are boxes
            //Apply grid like locking to highlighter
            if (isBox &&
                IsGameObjectBox(hitInfo.transform.gameObject))
            {
                grabbedObject.ManagePlacementHighlighter(true,
                                                         hitInfo.transform.position + hitInfo.normal,
                                                         hitInfo.transform.rotation);
            }
            //One or both grabbed and raycasted target aren't boxes
            //Visualize highlighter with free movement - no grid locking
            else if (!isBox ||
                     !IsGameObjectBox(hitInfo.transform.gameObject))
            {
                Renderer highlighterRenderer = grabbedObject.RendererComponent;
                Bounds bounds = highlighterRenderer.bounds;
                Vector3 contactOffset = new Vector3(bounds.extents.x - 0.5f,
                                                    bounds.extents.y - 0.5f,
                                                    bounds.extents.z - 0.5f);

                Vector3 offset = new Vector3(hitInfo.normal.x * (bounds.extents.x - contactOffset.x),
                                             hitInfo.normal.y * (bounds.extents.y - contactOffset.y),
                                             hitInfo.normal.z * (bounds.extents.z - contactOffset.z));
                Vector3 newPos = hitInfo.point + offset;
                grabbedObject.ManagePlacementHighlighter(true,
                                                         newPos,
                                                         this.transform.rotation);
            }
            //print(hitInfo.transform);
            //Debug.DrawLine(hitInfo.point, hitInfo.normal + hitInfo.point, Color.cyan);
        }
        else
        {
            //No surfaces are hit, hide the grabbed object's highlighter
            grabbedObject.HidePlacementHighlighter();
        }
    }

    /// <summary>
    /// Applied impulse force and damage to the destructible object.
    /// </summary>
    private void PunchObject()
    {
        if (isRaycastHit)
        {
            if (IsGameObjectBox(hitInfo.transform.gameObject))
            {
                DestructibleObject target = hitInfo.transform.GetComponent<DestructibleObject>();
                target.ApplyDamage(punchDamage);
                target.onPlayerPunch.Invoke();
                Rigidbody boxRB = hitInfo.transform.GetComponent<Rigidbody>();
                boxRB.AddForce(controller.CharacterCam.transform.forward * throwForce / boxRB.mass, ForceMode.Impulse);
            }
            else
            {
                DestructibleObject target = hitInfo.transform.GetComponent<DestructibleObject>();
                if (target != null)
                {
                    target.onPlayerPunch.Invoke();
                }

                switch (hitInfo.transform.tag)
                {
                    case "Hardhat":
                        hitInfo.transform.gameObject.GetComponent<Rigidbody>().isKinematic = false;
                        hitInfo.transform.gameObject.GetComponent<Rigidbody>().AddForce(controller.CharacterCam.transform.forward * throwForce, ForceMode.Impulse);
                        hitInfo.transform.parent = null;
                        break;
                    case "Bot":
					case "MiniBot":
						NPC_Controller mini = hitInfo.transform.GetComponent<NPC_Controller>();
						if (mini != null && raycastOrigin != null)
						{
							mini.GetPunched(transform.forward);
						}
						break;
                    case "ManagerBot":
                        Transform parent = SearchForParent.GetParentTransform(hitInfo.transform.gameObject, "Robot");
						Robot rob = parent.GetComponentInChildren<Robot>();
						if (rob != null && raycastOrigin != null)
						{
							rob.GetPunched(raycastOrigin.forward);
						}
						break;
					case "Generic Destructable":
						target.ApplyDamage(punchDamage);
						break;

				}
            }
        }
    }

    /// <summary>
    /// Set the grabbable object's parent to the object offset.
    /// </summary>
    private void GrabObject()
    {
        if (isRaycastHit &&
            hitInfo.transform.GetComponent<GrabbableObject>() != null)
        {
            grabbedObject = hitInfo.transform.GetComponent<GrabbableObject>();
            Transform objectParent = thirdPersonObjectOffset;
            if (controller.IsFirstPerson) objectParent = firstPersonObjectOffset;

			grabbedObject.GrabObject(objectParent);

			// Tooltips on interactable objects            
			//if (hitInfo.transform.GetComponent<InteractableObject>() != null)
			//{
			//    InteractableObject interactable = hitInfo.transform.GetComponentInChildren<InteractableObject>();
			//}
			//use grabbedObject.interactionComponent instead...

			// Grab animation
			bodyAnim.SetTrigger("PickupBox");
			headAnim.SetTrigger("PickupBox");
			bodyAnim.SetBool("isHoldingBox", true);
			headAnim.SetBool("isHoldingBox", true);
		}
    }

    /// <summary>
    /// Places object only when placement is valid.
    /// </summary>
    private void PlaceGrabbedObject()
    {
        if (isRaycastHit &&
            grabbedObject.interactionComponent.HighlighterInstance.activeSelf &&
            grabbedObject.PlaceObject())
        {
            grabbedObject = null;

			// Allow throwing
			allowThrow = true;

			// Place animation
			bodyAnim.SetTrigger("Place");
			headAnim.SetTrigger("Place");
			bodyAnim.SetBool("isHoldingBox", false);
			headAnim.SetBool("isHoldingBox", false);
		}
    }

    /// <summary>
    /// Throws the grabbed object by applying impulse force to its rigidbody.
    /// </summary>
    private void ThrowGrabbedObject()
    {
        if ((isRaycastHit && Vector3.Distance(hitInfo.point, this.transform.position) >= 2.5 ||
            !isRaycastHit) && grabbedObject)
        {
            grabbedObject.transform.localPosition = thrownObjectOffsetPos;
            grabbedObject.ThrowObject(raycastOrigin.forward * throwForce, ForceMode.Impulse);
            grabbedObject = null;

			// Allow punching
			allowPunch = true;

			// Throw animation
			bodyAnim.SetTrigger("Throw");
			headAnim.SetTrigger("Throw");
			bodyAnim.SetBool("isHoldingBox", false);
			headAnim.SetBool("isHoldingBox", false);
		}
	}

    /// <summary>
    /// Currently unused and probably still has some bugs...
    /// </summary>
    private void DropGrabbedObject()
    {
        Direction checkerDirection = new Direction(this.transform.position + thrownObjectOffsetPos,
                                                   this.transform.position + this.transform.forward);
        if (!Physics.Raycast(checkerDirection.relativePosition, checkerDirection.worldDirection, checkerDirection.localScaledDirection.magnitude) &&
            !Physics.SphereCast(new Ray(checkerDirection.relativePosition, checkerDirection.worldDirection), 1.5f))
        {
            grabbedObject.DropObject(this.transform.position + thrownObjectOffsetPos);
            grabbedObject = null;

			// Drop recognition
			bodyAnim.SetBool("isHoldingBox", false);
			headAnim.SetBool("isHoldingBox", false);
		}
    }

    /// <summary>
    /// Checks if the target game object is a 
    /// box or not by checking the game object tag.
    /// </summary>
    /// <param name="target">The target game object to be checked if it's a box or not.</param>
    /// <returns>Returns true if it's a box, otherwise, returns false.</returns>
    private bool IsGameObjectBox(GameObject target)
    {
        switch (target.tag)
        {
            case "Box":
                return true;

            case "Heavy Box":
                return true;

            default:
                return false;
        }
    }

	/// <summary>
	/// Resets all animation triggers and triggers one at a time
	/// </summary>
	private void SetTrigger(string trigger)
	{
		

	}

	/// <summary>
	/// Heavy box dragging mechanic.
	/// </summary>
	private void DragHeavyBox()
    {
        if (heavyBox == null)
        {
            if (controller.Controller.isGrounded &&
                isRaycastHit &&
                Input.GetButtonDown("Grab Object") &&
                AllowHeavyBox())
            {
                heavyBox = hitInfo.transform.gameObject;
                //heavyBox.transform.parent = heavyBoxRef.transform;
                heavyBoxRB = heavyBox.GetComponent<Rigidbody>();
                controller.JumpingEnabled = false;
            }
        }
        else
        {
            bool heavyBoxOnSight = isRaycastHit &&
                                   hitInfo.transform.gameObject == heavyBox;

            if (Input.GetButtonUp("Grab Object") ||
                !heavyBoxOnSight)
            {
                heavyBox.transform.parent = null;
                heavyBox = null;
                heavyBoxRB = null;
                controller.RevertMoveSpeed();
                controller.RevertLookSpeed();
                controller.RevertCamAngleClamp();
                controller.JumpingEnabled = true;
            }
            else
            {
                
                //Manage heavy box reference position
                Transform hbRef = heavyBoxRef.transform;
                hbRef.position = raycastOrigin.transform.position + raycastOrigin.transform.forward * interactionDistance.z;

                //Get transform references
                Transform boxTRS = heavyBox.transform;
                Transform TRS = this.transform;

                //Get all directions from the player's transform
                Vector3[] thisDirections = new Vector3[]
                {
                    TRS.right,
                    TRS.up,
                    TRS.forward,
                    -TRS.right,
                    -TRS.up,
                    -TRS.forward
                };                

                //Make the nearest box face towards the player
                float previousAngle = float.MaxValue;
                int forwardIndex = -1;
                for (int i = 0; i < thisDirections.Length; i++)
                {
                    Vector3 dir = thisDirections[i];
                    float currentAngle = Vector3.Angle(boxTRS.forward, dir);

                    if (currentAngle < previousAngle)
                    {
                        previousAngle = currentAngle;
                        forwardIndex = i;
                    }
                }
                hbRef.LookAt(boxTRS.position + thisDirections[forwardIndex]);

                //Store into new variables to declutter later calculations...
                Vector3 newPos = new Vector3(hbRef.position.x,
                                             heavyBox.transform.position.y,
                                             hbRef.position.z);
                Quaternion newRot = Quaternion.Euler(heavyBox.transform.eulerAngles.x,
                                                     hbRef.eulerAngles.y,
                                                     heavyBox.transform.eulerAngles.z);

                //Apply transform changes and smoothening
                heavyBox.transform.position = Vector3.Lerp(heavyBox.transform.position, newPos, 15 * Time.deltaTime);
                heavyBox.transform.rotation = Quaternion.Lerp(heavyBox.transform.rotation, newRot, 15 * Time.deltaTime);


                //Clamp min camera angle
                controller.UpdateCamAngleClamp(controller.originalMinCamAngleX, 70);

                float newSpeed = 0;
                if (Input.GetButtonDown("Sprint")) newSpeed = controller.SprintSpeed;
                else newSpeed = controller.WalkSpeed;
                controller.OverrideMoveSpeed(newSpeed / heavyBoxRB.mass);
                controller.OverrideLookSpeed(controller.LookSensitivity / heavyBoxRB.mass);
            }
        }
    }


    /// <summary>
    /// Move crosshair onto raycast hit. If no hits or the hit point is too close, set position to center.
    /// </summary>
    private void ManageCrosshair()
    {
        if (isRaycastHit)
        {
            if (SecondCrosshair)
            {
                InteractableObject interactable = hitInfo.transform.GetComponent<InteractableObject>();
                if (interactable != null)
                {
                    //Snap the dynamic crosshair to the interactable object's transform
                    crosshair2.transform.position = controller.CharacterCam.WorldToScreenPoint(interactable.transform.position);
                }
                else
                {
                    //Move the dynamic crosshair to the raycast point
                    crosshair2.transform.position = controller.CharacterCam.WorldToScreenPoint(hitInfo.point);
                }

                //Manage dynamic crosshair opacity
                Direction toCamera = new Direction(raycastOrigin.position + raycastOrigin.forward * raycastDistance,
                                                   controller.CharacterCam.transform.position);

                bool isPlayerRayHit = Physics.Raycast(toCamera.relativePosition, toCamera.localDirection,
                                                      out RaycastHit rayHit, toCamera.localScaledDirection.magnitude,
                                                      Physics.IgnoreRaycastLayer, QueryTriggerInteraction.Collide) &&
                                      rayHit.transform.tag == "Player";

                Vector3 rayOrigin = toCamera.relativePosition;
                if (DoRaycast(out RaycastHit toObjectHit)) rayOrigin = toObjectHit.point;
                bool isObjectRayHit = Physics.Raycast(rayOrigin, toCamera.localDirection,
                                                      toCamera.localScaledDirection.magnitude);

                if (isPlayerRayHit || isObjectRayHit)
                {
                    //Make the crosshair semi-transparent if the raycast point
                    //is behind an obstacle based on the current camera view
                    Color newColor = crosshair2.color;
                    newColor.a = 0.5f;
                    crosshair2.color = newColor;
                    isTargetBehindSometing = true;
                }
                else if (!isPlayerRayHit && !isObjectRayHit)
                {
                    //Make crosshair opaque
                    Color newColor = crosshair2.color;
                    newColor.a = 1;
                    crosshair2.color = newColor;
                    isTargetBehindSometing = false;
                }

				crosshair.transform.position = controller.CharacterCam.WorldToScreenPoint(hitInfo.point);
			}
        }
        else
        {
            Vector3 rayPoint = raycastOrigin.position + raycastOrigin.forward * raycastDistance;
            crosshair.transform.position = controller.CharacterCam.WorldToScreenPoint(rayPoint);
            
            if (SecondCrosshair)
                crosshair2.transform.position = crosshair.transform.position;
        }        
    }

    /// <summary>
    /// Do a raycast from the given raycast origin transform.
    /// </summary>
    /// <param name="hitInfo">Raycast output</param>
    /// <returns>Returns true if the raycast hits a collider.</returns>
    private bool DoRaycast(out RaycastHit hitInfo)
    {
        return Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out hitInfo, raycastDistance);
    }

    /// <summary>
    /// Returns true if the player is grounded,
    /// not stepping on the current targeted heavy box,
    /// and the current targeted game object is a heavy box.
    /// </summary>
    private bool AllowHeavyBox()
    {
        if (isRaycastHit &&
            hitInfo.transform.tag == "Heavy Box")
        {
            GameObject currentTarget = hitInfo.transform.gameObject;

            if (!groundCheck.ObjectsInTrigger.Contains(currentTarget))
                return true;
        }
        return false;
    }
    #endregion

    #region Public Methods
    public void SetEnabled (bool isEnabled)
    {
        controller.Controller.enabled = isEnabled;
        controller.RigidbodyComponent.isKinematic = !isEnabled;        
        controller.CharacterCam.gameObject.SetActive(isEnabled);
        controller.enabled = isEnabled;

        if (!isEnabled) sfxController.SFXSource.pitch = 0;
        sfxController.enabled = isEnabled;        
        
        crosshair.gameObject.SetActive(isEnabled);
        if (SecondCrosshair) crosshair2.gameObject.SetActive(isEnabled);

        this.enabled = isEnabled;
    }
    #endregion
}
