using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;



[RequireComponent(typeof(PlayerCharacterController))]
public class Player : MonoBehaviour
{
    public static Player Instance;


 //   [Header("Prototype Tasks")]
	//public Task stackBox;
	//public Task knockHat;
	//bool boxStacking = false;

	//public GameObject controls;
	//public GameObject controlsObject;
	//public Material controlsHighlight;
	//private GameObject highlight;

	public GameObject hand;
	private Animator handAnim;
    	

    #region Exposed Variables
    [Space]
    public Graphic crosshair;
    [SerializeField] private UnityEventsHandler groundCheck;



    [Header("Box Handling Properties")]
    [SerializeField] private float punchDamage = 3.0f;

    [Tooltip("Strength of impulse force applied upon throwing.")]
    [SerializeField] private float throwForce = 20.0f;

	[Tooltip("Strength of impulse force applied upon dropping a non-box object.")]
	[SerializeField] private float dropForce = 10.0f;

    [SerializeField] private Transform thirdPersonObjectOffset;
    [SerializeField] private Transform firstPersonObjectOffset;
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

    [SerializeField] private Material materialHighlight;
    #endregion

    #region Hidden Variables
    private PlayerCharacterController controller;
    private RaycastHit hitInfo;
    private bool isRaycastHit = false;
    private GameObject heavyBox;
    private Rigidbody heavyBoxRB;
    private GrabbableObject grabbedObject;
    private Transform previousRaycastTarget;

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
        //handAnim = hand.GetComponent<Animator>();
	}

    private void Update()
    {
        //Place crosshair management to avoid jittering
        //Only adjust crosshair position if deadzone exist
        //if (crosshair != null) ManageCrosshair();
    }

    // Update is called once per frame
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
                if (Input.GetButtonDown("Throw Object")) ThrowGrabbedObject();
                //if (Input.GetButtonDown("Drop Object")) DropGrabbedObject();
            }
            else
            {
                if (Input.GetButtonDown("Grab Object")) GrabObject();
                if (Input.GetButtonDown("Punch"))
                {
                    PunchObject();
                    //handAnim.SetBool("isPunching", true);
                }
                else
                {
                    //handAnim.SetBool("isPunching", false);
                }
                HighlightTargetObject();
                DragHeavyBox();
            }
        }


        //Debug raycast
        if (Input.GetKeyDown(KeyCode.P)) EditorApplication.isPaused = true;
        //if (isRaycastHit)
        //{
        //    Debug.DrawLine(raycastOrigin.position, hitInfo.point, Color.green);
        //    print(hitInfo.transform);
        //}
        //else
        //{
        //    Debug.DrawRay(raycastOrigin.position, raycastOrigin.forward * raycastDistance, Color.cyan);
        //}
        
    }

    #region Compact
    private void HighlightTargetObject()
    {
        if (isRaycastHit)
        {
            InteractableObject interactable = hitInfo.transform.GetComponentInChildren<InteractableObject>();

            if (interactable == null &&
                previousRaycastTarget != null)
            {
                previousRaycastTarget.GetComponentInChildren<InteractableObject>().ShowHighlighter(false);
            }

            if (interactable != null)
            {
                interactable.ShowHighlighter(true);
                previousRaycastTarget = hitInfo.transform;
            }
        }
        else if (!isRaycastHit &&
                 previousRaycastTarget != null)
        {
            previousRaycastTarget.GetComponentInChildren<InteractableObject>().ShowHighlighter(false);
        }
    }

    private void VisualizeHighlighter()
    {
        if (isRaycastHit)
        {
            bool isBox = IsGameObjectBox(grabbedObject.gameObject);


            if (isBox &&
                IsGameObjectBox(hitInfo.transform.gameObject))
            {
                grabbedObject.ManagePlacementHighlighter(true,
                                                         hitInfo.transform.position + hitInfo.normal,
                                                         hitInfo.transform.rotation);
            }
            else if (!isBox ||
                     !IsGameObjectBox(hitInfo.transform.gameObject))
            {
                grabbedObject.ManagePlacementHighlighter(true,
                                                         hitInfo.point + hitInfo.normal / 2,
                                                         this.transform.rotation);
            }
        }
        else
        {
            grabbedObject.HidePlacementHighlighter();
        }
    }

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
                        hitInfo.transform.GetComponent<Rigidbody>().isKinematic = false;
                        hitInfo.transform.GetComponent<Rigidbody>().AddForce(controller.CharacterCam.transform.forward * throwForce, ForceMode.Impulse);
                        hitInfo.transform.parent = null;
                        //knockHat.Contribute();
                        break;

                    case "Bot":
                    case "ManagerBot":
                        hitInfo.transform.GetComponent<Robot>().GetPunched(this.PlayerMovementControls.CharacterCam.transform.forward);
                        break;
                }
            }
        }
    }

    private void GrabObject()
    {
        if (isRaycastHit &&
            hitInfo.transform.GetComponent<GrabbableObject>() != null)
        {
            grabbedObject = hitInfo.transform.GetComponent<GrabbableObject>();
            Transform objectParent = thirdPersonObjectOffset;
            if (controller.IsFirstPerson) objectParent = firstPersonObjectOffset;
            grabbedObject.GrabObject(objectParent);
        }
    }

    private void PlaceGrabbedObject()
    {
        if (isRaycastHit &&
            grabbedObject.interactionComponent.HighlighterInstance.activeSelf &&
            grabbedObject.PlaceObject())
        {
            grabbedObject = null;
        }
    }

    private void ThrowGrabbedObject()
    {
        if (isRaycastHit && Vector3.Distance(hitInfo.point, this.transform.position) >= 2.5 ||
            !isRaycastHit)
        {
            Direction throwDirection = new Direction(thrownObjectOffsetPos,
                                                     thrownObjectOffsetPos + raycastOrigin.forward.normalized);
            grabbedObject.ThrowObject(throwDirection.worldDirection * throwForce, ForceMode.Impulse);
            grabbedObject = null;
        }
    }

    private void DropGrabbedObject()
    {
        Direction checkerDirection = new Direction(this.transform.position + thrownObjectOffsetPos,
                                                   this.transform.position + this.transform.forward);
        if (!Physics.Raycast(checkerDirection.relativePosition, checkerDirection.worldDirection, checkerDirection.localScaledDirection.magnitude) &&
            !Physics.SphereCast(new Ray(checkerDirection.relativePosition, checkerDirection.worldDirection), 1.5f))
        {
            grabbedObject.DropObject(this.transform.position + thrownObjectOffsetPos);
            grabbedObject = null;
        }
    }

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
                heavyBox.transform.parent = this.transform;
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
                controller.JumpingEnabled = true;
            }
            else
            {
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
            if (hitInfo.transform.GetComponentInChildren<InteractableObject>() != null)
            {
                crosshair.transform.position = controller.CharacterCam.WorldToScreenPoint(hitInfo.transform.position);
            }
            else
            {
                crosshair.transform.position = controller.CharacterCam.WorldToScreenPoint(hitInfo.point);
            }
        }
        else
        {
            //crosshair.transform.position = controller.CharacterCam.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0.5f));
            Vector3 rayPoint = raycastOrigin.position + raycastOrigin.forward * raycastDistance;
            crosshair.transform.position = controller.CharacterCam.WorldToScreenPoint(rayPoint);
        }

        //manage crosshair opacity
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
            Color newColor = crosshair.color;
            newColor.a = 0.5f;
            crosshair.color = newColor;
            isTargetBehindSometing = true;
        }
        else if (!isPlayerRayHit && !isObjectRayHit)
        {
            //make crosshair opaque
            Color newColor = crosshair.color;
            newColor.a = 1;
            crosshair.color = newColor;
            isTargetBehindSometing = false;
        }

        //Debug.DrawLine(toCamera.relativePosition, toCamera.worldScaledDirection, Color.yellow);
    }

    /// <summary>
    /// Do a raycast from the given raycast origin transform.
    /// </summary>
    /// <param name="hitInfo">Raycast output</param>
    /// <returns>Returns true if the raycast hits a collider.</returns>
    private bool DoRaycast(out RaycastHit hitInfo)
    {
        return Physics.Raycast(raycastOrigin.transform.position, raycastOrigin.transform.forward, out hitInfo, raycastDistance);
    }

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
}
