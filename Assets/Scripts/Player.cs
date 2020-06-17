using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


//[System.Serializable]
//public struct GrabbableObject
//{
//    //Required
//    public Transform transform { get; private set; }
//    public Renderer renderer { get; private set; }

//    //Can be null
//    public Rigidbody rigidbodyComponent { get; set; }
//    public DestructibleObject destructibleComponent { get; set; }
    
//    public GrabbableObject(Transform thisTransform,
//                           Renderer thisRenderer,
//                           Rigidbody thisRigidbody = null,
//                           DestructibleObject thisDestructible = null)
//    {
//        transform = thisTransform;
//        renderer = thisRenderer;
//        rigidbodyComponent = thisRigidbody;
//        destructibleComponent = thisDestructible;
//    }
//}


[RequireComponent(typeof(PlayerCharacterController))]
public class Player : MonoBehaviour
{
    public static Player Instance;


    [Header("Prototype Tasks")]
	public Task stackBox;
	public Task knockHat;
	bool boxStacking = false;

	public GameObject controls;
	public GameObject controlsObject;
	public Material controlsHighlight;
	private GameObject highlight;

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
    //outline
    //private GameObject grabbedObjectOutline;
    //private GrabbableObjectPlacementChecker outlineCollider;
    //private Color originalOutlineColor;
    //private Renderer outlineRenderer;

    //highlight
    //private GameObject boxHighlight;

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

                //if (outlineCollider.isPlacable)
                //{
                //    if (Input.GetButtonDown("Place Box") && (boxOutline.activeSelf)) PlaceBox();
                //}

                if (Input.GetButtonDown("Place Object")) PlaceGrabbedObject();
                if (Input.GetButtonDown("Throw Object")) ThrowGrabbedObject();
                if (Input.GetButtonDown("Drop Object")) DropGrabbedObject();
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
                //DragHeavyBox();
            }
        }


        //Debug raycast
        if (Input.GetKeyDown(KeyCode.P)) EditorApplication.isPaused = true;
        if (isRaycastHit)
        {
            Debug.DrawLine(raycastOrigin.position, hitInfo.point, Color.green);
            //print(hitInfo.transform);
        }
        else
        {
            Debug.DrawRay(raycastOrigin.position, raycastOrigin.forward * raycastDistance, Color.cyan);
        }
        
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
                target.OnPlayerPunch.Invoke();
                Rigidbody boxRB = hitInfo.transform.GetComponent<Rigidbody>();
                boxRB.AddForce(controller.CharacterCam.transform.forward * throwForce / boxRB.mass, ForceMode.Impulse);
            }
            else
            {
                DestructibleObject target = hitInfo.transform.GetComponent<DestructibleObject>();
                target.OnPlayerPunch.Invoke();

                switch (hitInfo.transform.tag)
                {
                    case "Hardhat":
                        hitInfo.transform.GetComponent<Rigidbody>().isKinematic = false;
                        hitInfo.transform.GetComponent<Rigidbody>().AddForce(controller.CharacterCam.transform.forward * throwForce, ForceMode.Impulse);
                        hitInfo.transform.parent = null;
                        knockHat.Contribute();
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

    //private void GrabbedObjectToFirstPerson(bool isFirstPerson)
    //{
    //    GameObject targetGO = null;
    //    if (grabbedObject.destructibleComponent != null &&
    //        grabbedObject.destructibleComponent.TargetGameObject != null)
    //    {
    //        targetGO = grabbedObject.destructibleComponent.TargetGameObject;
    //    }
    //    else
    //    {
    //        targetGO = grabbedObject.transform.gameObject;
    //    }

    //    if (isFirstPerson)
    //        targetGO.layer = LayerMask.NameToLayer("Grabbed Object");
    //    else
    //        targetGO.layer = LayerMask.NameToLayer("Default");
    //}
    #endregion

    /*
    #region Private Methods
    private void DragHeavyBox()
    {
        if (heavyBox == null)
        {
            if (controller.Controller.isGrounded &&
                isRaycastHit &&
                Input.GetButtonDown("Grab Box") &&
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

            if (Input.GetButtonUp("Grab Box") ||
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
    /// Set the targeted box's parent to this transform then instanciates an outline.
    /// </summary>
    private void GrabBox()
    {
		if (isRaycastHit)
		{
            grabbedObject = hitInfo.transform.gameObject;

            //Put grabbed object as child of grabbed object offset transform
            grabbedObject.transform.parent = grabbedObjectOffset;
            grabbedObject.transform.localPosition = grabbedObjectOffset.localPosition;
            grabbedObject.transform.rotation = grabbedObjectOffset.rotation;


            if (hitInfo.transform.tag == "Box")
			{
				if (boxHighlight != null)
				{
					Destroy(boxHighlight);
				}

				//grabbedObject = hitInfo.transform.gameObject;                
                DestructibleObject targetBox = grabbedObject.GetComponent<DestructibleObject>();

                //Clone grabbed box for outline setup
                grabbedObjectOutline = Instantiate(grabbedObject);
                grabbedObjectOutline.tag = "Outline";
                grabbedObjectOutline.transform.localScale += new Vector3(0.00001f, 0.00001f, 0.00001f); //prevent z-fighting

                //Remove any external force appliers
                targetBox.DetachForceAppliers();
				grabbedObject.GetComponent<BoxDragSFX>().ToggleThis(false);

				//Put grabbed object as child of grabbed object offset transform
				//grabbedObject.transform.parent = grabbedObjectOffset;
				//grabbedObject.transform.localPosition = grabbedObjectOffset.localPosition;
				//grabbedObject.transform.rotation = grabbedObjectOffset.rotation;

				//Disable physics and collision
				grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
				grabbedObject.GetComponent<Collider>().enabled = false;

                //Put grabbed box in different layer mask to prevent clipping
                //if (targetBox.TargetGameObject != null)
                //{
                //    targetBox.TargetGameObject.layer = LayerMask.NameToLayer("Grabbed Object");
                //}
                //else
                //{
                //    grabbedObject.layer = LayerMask.NameToLayer("Grabbed Object");
                //}

                //Tweak collider and add collision checker
                grabbedObjectOutline.layer = LayerMask.NameToLayer("Ignore Raycast"); //ignore raycast to prevent placement jittering
				BoxCollider collider = grabbedObjectOutline.GetComponent<BoxCollider>();
				collider.size = new Vector3(0.99f, 0.99f, 0.99f);
				collider.isTrigger = true;
				collider.enabled = true;
				outlineCollider = grabbedObjectOutline.AddComponent<BoxPlacementChecker>();

                //Check if the rendered game object is childed or not
                if (grabbedObjectOutline.GetComponent<DestructibleObject>().TargetGameObject != null)
                {
                    outlineRenderer = grabbedObjectOutline.GetComponent<DestructibleObject>().TargetGameObject.GetComponent<Renderer>();
                }
                else
                {
                    outlineRenderer = grabbedObjectOutline.GetComponent<Renderer>();
                }

                //Set outline to semi-transparent
				RendererModeChanger.SetToTransparent(outlineRenderer);
				Color alpha = outlineRenderer.material.color;
				alpha.a = 0.5f;
				outlineRenderer.material.color = alpha;
				originalOutlineColor = outlineRenderer.material.color;

                //Remove shadowsoutlineRenderer.receiveShadows = false;
				outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                //Remove physics and box component
                Destroy(grabbedObjectOutline.GetComponent<DestructibleObject>());
                grabbedObjectOutline.GetComponent<Rigidbody>().isKinematic = true;

                //Hide after setup
                grabbedObjectOutline.SetActive(false);
			}

			if (hitInfo.transform.tag == "FloorControls")
			{
				controls.SetActive(true);
				controlsObject.SetActive(false);

				if (highlight != null)
				{
					Destroy(highlight);
				}
			}

			if (hitInfo.transform.tag == "Hardhat")
			{
				grabbedObject = hitInfo.transform.gameObject;

				// annoy grandparent if hat hasn't left the house yet
				if (hitInfo.transform.GetComponentInParent<Robot>())
				{
					hitInfo.transform.GetComponentInParent<Robot>().GetAnnoyed();
				}

				//Remove any external force appliers
				grabbedObject.GetComponent<DestructibleObject>().DetachForceAppliers();

				//Set grabbed box as child of main cam
				grabbedObject.transform.parent = controller.CharacterCam.transform;
				grabbedObject.transform.localPosition = Vector3.zero + otherObjectOffset;
				grabbedObject.transform.rotation = controller.CharacterCam.transform.rotation;

				// Rotate (hardhat only?)
				grabbedObject.transform.Rotate(0, 0, 0);

				//Disable physics and collision
				grabbedObject.GetComponent<Rigidbody>().isKinematic = true;

				// Get multiple colliders if they have them
				Collider[] colliders = grabbedObject.GetComponentsInChildren<Collider>();
				foreach (Collider col in colliders)
				{
					col.enabled = false;
				}

				knockHat.Contribute();

				//Put grabbed box in different layer mask to prevent clipping
				grabbedObject.layer = LayerMask.NameToLayer("Grabbed Object");
			}
		}
    }

    /// <summary>
    /// Locks on other boxes sides else just sits on top of the currently targeted surface.
    /// </summary>
    private void ShowBoxPlacement()
    {
        if (isRaycastHit)
        {
            //if within deadzone, don't render the outline
            if (hitInfo.distance >= boxPlacementDeadzone)
            {
                boxOutline.SetActive(true);
                
                //Set outline color to red if not placable
                if (outlineCollider.isPlacable)
                {
                    outlineRenderer.material.color = originalOutlineColor;
                }
                else
                {
                    outlineRenderer.material.color = new Color(1, 0, 0, 0.5f);
                }

                //Box outline positioning and rotation
                if (hitInfo.transform.tag == "Box" ||
                    hitInfo.transform.tag == "Heavy Box")
                {
                    boxOutline.transform.position = hitInfo.transform.position + hitInfo.normal;
                    boxOutline.transform.rotation = hitInfo.transform.rotation;

					boxStacking = true; ////
				}
                else if (hitInfo.transform.tag != "Player")
                {
                    boxOutline.transform.position = hitInfo.point + hitInfo.normal / 2;
                    boxOutline.transform.rotation = transform.rotation;

					boxStacking = false; ////
                }
            }
            else
            {
                boxOutline.SetActive(false);
            }    
        }
        else
        {
            boxOutline.SetActive(false);
        }
    }

    /// <summary>
    /// Instanciates a copy of the current box target then changes its color for highlight effect.
    /// </summary>
    private void HighlightTarget()
    {
		if (isRaycastHit)
		{
			// Prototype control highlight
			if (hitInfo.transform.tag == "FloorControls")
			{
				if (highlight == null)
				{
					highlight = Instantiate(hitInfo.transform.gameObject);

					Renderer renderer = highlight.GetComponent<Renderer>();
					renderer.material = controlsHighlight;

					highlight.transform.position = hitInfo.transform.position;
					highlight.transform.rotation = hitInfo.transform.rotation;

					highlight.transform.localScale += new Vector3(0.0001f, 0.0001f, 0.0001f);
				}
			}
			else
			{
				if (highlight != null)
				{
					Destroy(highlight);
				}
			}

            if (hitInfo.transform.tag == "Box" ||
                hitInfo.transform.tag == "Heavy Box")
            {
                if (previousRaycastTarget != hitInfo.transform)
                {
                    if (boxHighlight != null)
                    {
                       // Destroy(boxHighlight);
                    }
                }

                if (boxHighlight == null)
                {
                    previousRaycastTarget = hitInfo.transform;
                    boxHighlight = Instantiate(hitInfo.transform.gameObject);
                    DestructibleObject target = boxHighlight.GetComponent<DestructibleObject>();

                    //Retain mesh and replace material
                    Renderer renderer = null;
                    if (target.TargetGameObject != null)
                    {
                        renderer = target.TargetGameObject.GetComponent<Renderer>();
                    }
                    else
                    {
                        renderer = boxHighlight.GetComponent<Renderer>();
                    }
                    renderer.material = materialHighlight;

                    //Disable shadows
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;

                    //Slight size offset just to prevent "z-fighting"
                    boxHighlight.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);

                    //Remove physics and colliders
                    Destroy(boxHighlight.GetComponent<Collider>());
                    Destroy(boxHighlight.GetComponent<DestructibleObject>());
                    Destroy(boxHighlight.GetComponent<Rigidbody>());
                    Destroy(boxHighlight.GetComponent<BoxDragSFX>());
                }
                else
                {
                    if (heavyBox != null)
                    {
                        boxHighlight.transform.position = heavyBox.transform.position;
                        boxHighlight.transform.rotation = heavyBox.transform.rotation;
                    }
                    else
                    {
                        boxHighlight.transform.position = hitInfo.transform.position;
                        boxHighlight.transform.rotation = hitInfo.transform.rotation;
                    }
                }
            }
            else
            {
                if (boxHighlight != null)
                {
					Destroy(boxHighlight);
                }
            }
        }
        else
        {
            if (boxHighlight != null)
            {
                Destroy(boxHighlight);
            }

			if (highlight != null)
			{
				Destroy(highlight);
			}
		}
	}

    /// <summary>
    /// Grabbed box copies the outline transform before placement.
    /// </summary>
    private void PlaceBox()
    {
		if (grabbedObject)
		{
			grabbedObject = grabbedObject;
			grabbedObject = null;
		}

        grabbedObject.transform.parent = null;

		if (boxOutline.activeSelf)
        {
            grabbedObject.transform.position = boxOutline.transform.position;
            grabbedObject.transform.rotation = boxOutline.transform.rotation;
            grabbedObject.transform.localScale = Vector3.one;

            // Stack box task
            if (stackBox != null && boxStacking)
            {
                stackBox.Contribute();
                //
            }
		}
        else
        {
            grabbedObject.transform.position = this.transform.position + this.transform.forward;
            grabbedObject.transform.rotation = Quaternion.identity;
		}

        //Revert Box state
        DestructibleObject targetBox = grabbedObject.GetComponent<DestructibleObject>();
        if (targetBox.TargetGameObject != null)
        {
            targetBox.TargetGameObject.layer = LayerMask.NameToLayer("Default");
        }
        else
        {
            grabbedObject.layer = LayerMask.NameToLayer("Default");
        }
        grabbedObject.GetComponent<Collider>().enabled = true;
        grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
		if (grabbedObject.GetComponent<BoxDragSFX>())
		{
			grabbedObject.GetComponent<BoxDragSFX>().ToggleThis(true);
		}
        grabbedObject = null;
		if (boxOutline)
		{
			Destroy(boxOutline);
		}
	}

    /// <summary>
    /// Applies offset position before throwing.
    /// </summary>
    public void ThrowBox()
    {
        if (isRaycastHit && Vector3.Distance(hitInfo.point, this.transform.position) >= 2.5 ||
            !isRaycastHit)
        {
			Vector3 newPos;
			float force;
			if (grabbedObject)
			{
				grabbedObject = grabbedObject;
				grabbedObject = null;

				newPos = controller.CharacterCam.transform.localPosition;
				newPos.z = otherObjectOffset.z;
				newPos.y -= 2;
				force = dropForce;
			}
			else
			{
				newPos = controller.CharacterCam.transform.localPosition;
				newPos.z = objectOffset.z;
				force = throwForce;
			}
			Rigidbody boxRB = grabbedObject.GetComponent<Rigidbody>();

            grabbedObject.transform.localPosition = newPos;
            grabbedObject.transform.parent = null;

            DestructibleObject targetBox = grabbedObject.GetComponent<DestructibleObject>();
            if (targetBox.TargetGameObject != null)
            {
                targetBox.TargetGameObject.layer = LayerMask.NameToLayer("Default");
            }
            else
            {
                grabbedObject.layer = LayerMask.NameToLayer("Default");
            }
            //grabbedObject.GetComponent<Collider>().enabled = true;

            // Multiple collider check
            Collider[] colliders = grabbedObject.GetComponentsInChildren<Collider>();
			foreach (Collider col in colliders)
			{
				col.enabled = true;
			}

			if (grabbedObject.GetComponent<BoxDragSFX>())
			{
				grabbedObject.GetComponent<BoxDragSFX>().ToggleThis(true);
			}
            boxRB.isKinematic = false;
            boxRB.AddForce(controller.CharacterCam.transform.forward * force, ForceMode.Impulse);
            grabbedObject = null;

			if (boxOutline)
			{
				Destroy(boxOutline);
			}
        }
    }

    /// <summary>
    /// Applies impulse force on boxes.
    /// </summary>
    private void PunchBox()
    {
        if (isRaycastHit)
        {
            if (hitInfo.transform.tag == "Box" || 
                hitInfo.transform.tag == "Heavy Box")
            {
                DestructibleObject target = hitInfo.transform.GetComponent<DestructibleObject>();
                target.ApplyDamage(punchDamage);
                target.OnPlayerPunch.Invoke();
                Rigidbody boxRB = hitInfo.transform.GetComponent<Rigidbody>();
                boxRB.AddForce(controller.CharacterCam.transform.forward * throwForce / boxRB.mass, ForceMode.Impulse);
            }

			if (hitInfo.transform.tag == "Hardhat")
			{
				DestructibleObject target = hitInfo.transform.GetComponent<DestructibleObject>();
				target.OnPlayerPunch.Invoke();

				hitInfo.transform.GetComponent<Rigidbody>().isKinematic = false;
				hitInfo.transform.GetComponent<Rigidbody>().AddForce(controller.CharacterCam.transform.forward * throwForce, ForceMode.Impulse);
				hitInfo.transform.parent = null;

				knockHat.Contribute();
			}

			if (hitInfo.transform.tag == "Bot" || hitInfo.transform.tag == "ManagerBot")
			{
				DestructibleObject target = hitInfo.transform.GetComponent<DestructibleObject>();
				target.OnPlayerPunch.Invoke();
				hitInfo.transform.GetComponent<Robot>().GetPunched(this.PlayerMovementControls.CharacterCam.transform.forward);
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
            if (hitInfo.transform.tag == "Box")
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
    private bool DoRaycast (out RaycastHit hitInfo)
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

    private void GrabbedBoxToFirstPerson(bool isFirstPerson)
    {
        DestructibleObject targetBox = grabbedObject.GetComponent<DestructibleObject>();
        GameObject targetGO = grabbedObject;

        if (targetBox.TargetGameObject != null)
            targetGO = targetBox.TargetGameObject;

        if (isFirstPerson)
            targetGO.layer = LayerMask.NameToLayer("Grabbed Object");
        else
            targetGO.layer = LayerMask.NameToLayer("Default");        
    }
    #endregion
    */
}
