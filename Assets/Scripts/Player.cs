using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FPSController))]
public class Player : MonoBehaviour
{
	// Prototype Tasks
	public Task stackBox;
	bool boxStacking = false;

	public Task punchRobot;
	public Task knockHat;
	//

	public static Player Instance;    
    public Graphic crosshair;

    #region Exposed Variables
    [SerializeField] private UnityEventsHandler groundCheck;

    [Header("Box Handling Properties")]
    [SerializeField] private float punchDamage = 3.0f;

    [Tooltip("Strength of impulse force applied upon throwing.")]
    [SerializeField] private float throwForce = 20.0f;

	[Tooltip("The position of the grabbed object in this transform's local space.")]
    [SerializeField] private Vector3 objectOffset;

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
    private GameObject boxOutline;
    private BoxPlacementChecker outlineCollider;
    private Color originalOutlineColor;
    private Renderer outlineRenderer;

    //highlight
    private GameObject boxHighlight;

    private GameObject currentBox;
    private FPSController controller;
    private RaycastHit hitInfo;
    private bool isRaycastHit = false;

    private GameObject heavyBox;
    private Rigidbody heavyBoxRB;
    #endregion

    public FPSController PlayerMovementControls { get { return controller; } }


    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = this.GetComponent<FPSController>();
    }

    // Update is called once per frame
    void Update()
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

        //Only adjust crosshair position if deadzone exist
        if (crosshair != null && boxPlacementDeadzone > 0) ManageCrosshair();

        if (Time.timeScale == 1)
        {
            if (currentBox != null)
            {
                ShowBoxPlacement();

                if (outlineCollider.isPlacable)
                {
                    if (Input.GetButtonDown("Place Box") && boxOutline.activeSelf) PlaceBox();
                }

                if (Input.GetButtonDown("Throw Box")) ThrowBox();
            }
            else
            {
                if (Input.GetButtonDown("Grab Box")) GrabBox();
                if (Input.GetButtonDown("Punch")) PunchBox();
                HighlightTarget();
                DragHeavyBox();
            }
        }

        //Debug raycast
        //if (isRaycastHit) Debug.DrawLine(controller.MainCam.transform.position, hitInfo.point, Color.green);
        //else              Debug.DrawRay(controller.MainCam.transform.position, controller.MainCam.transform.forward, Color.red);
    }


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
            if (hitInfo.transform.tag == "Box")
            {
                if (boxHighlight != null)
                {
                    Destroy(boxHighlight);
                }

				currentBox = hitInfo.transform.gameObject;

                //Remove any external force appliers
                currentBox.GetComponent<DestructibleObject>().DetachForceAppliers();
                currentBox.GetComponent<BoxDragSFX>().ToggleThis(false);

                //Clone grabbed box for outline setup
                boxOutline = Instantiate(currentBox);
                boxOutline.transform.localScale += new Vector3(0.00001f, 0.00001f, 0.00001f); //prevent z-fighting

                //Set grabbed box as child of main cam
                currentBox.transform.parent = controller.MainCam.transform;
                currentBox.transform.localPosition = Vector3.zero + objectOffset;
                currentBox.transform.rotation = controller.MainCam.transform.rotation;                

                //Disable physics and collision
                currentBox.GetComponent<Rigidbody>().isKinematic = true;
				currentBox.GetComponent<Collider>().enabled = false;

                //Put grabbed box in different layer mask to prevent clipping
                currentBox.layer = LayerMask.NameToLayer("Grabbed Object");

                //Remove physics and box component
                Destroy(boxOutline.GetComponent<DestructibleObject>());
				//Destroy(boxOutline.GetComponent<Rigidbody>()); // CAUSES TRIGGERS TO NOT WORK
				boxOutline.GetComponent<Rigidbody>().isKinematic = true; 

                //Tweak collider and add collision checker
                boxOutline.layer = LayerMask.NameToLayer("Ignore Raycast"); //ignore raycast to prevent placement jittering
                BoxCollider collider = boxOutline.GetComponent<BoxCollider>();
                collider.size = new Vector3(0.99f, 0.99f, 0.99f);
                collider.isTrigger = true;
                collider.enabled = true;
                outlineCollider = boxOutline.AddComponent<BoxPlacementChecker>();

                //Set outline to semi-transparent
                outlineRenderer = boxOutline.GetComponent<Renderer>();
                RendererModeChanger.SetToTransparent(outlineRenderer);
                Color alpha = outlineRenderer.material.color;
                alpha.a = 0.5f;
                outlineRenderer.material.color = alpha;
                originalOutlineColor = outlineRenderer.material.color;

                //Remove shadows
                outlineRenderer.receiveShadows = false;
                outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                
                //Hide after setup
                boxOutline.SetActive(false);
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
            if (hitInfo.transform.tag == "Box" ||
                hitInfo.transform.tag == "Heavy Box")
            {
                if (boxHighlight == null)
                {                
                    boxHighlight = Instantiate(hitInfo.transform.gameObject);
                    
                    //Remove physics and colliders
                    Destroy(boxHighlight.GetComponent<Collider>());
                    Destroy(boxHighlight.GetComponent<DestructibleObject>());
                    Destroy(boxHighlight.GetComponent<Rigidbody>());
                    Destroy(boxHighlight.GetComponent<BoxDragSFX>());

                    //Retain mesh and replace material
                    Renderer renderer = boxHighlight.GetComponent<Renderer>();
                    renderer.material = materialHighlight;

                    //Disable shadows
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;

                    //Slight size offset just to prevent "z-fighting"
                    boxHighlight.transform.localScale += new Vector3(0.0001f, 0.0001f, 0.0001f);
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
        }
    }

    /// <summary>
    /// Grabbed box copies the outline transform before placement.
    /// </summary>
    private void PlaceBox()
    {
        currentBox.transform.parent = null;

		if (boxOutline.activeSelf)
        {
            currentBox.transform.position = boxOutline.transform.position;
            currentBox.transform.rotation = boxOutline.transform.rotation;

            // Stack box task
            if (stackBox != null && boxStacking)
            {
                stackBox.Contribute();
                //
            }
		}
        else
        {
            currentBox.transform.position = this.transform.position + this.transform.forward;
            currentBox.transform.rotation = Quaternion.identity;
		}

        //Revert Box state
        currentBox.layer = LayerMask.NameToLayer("Default");
        currentBox.GetComponent<Collider>().enabled = true;
        currentBox.GetComponent<Rigidbody>().isKinematic = false;
        currentBox.GetComponent<BoxDragSFX>().ToggleThis(true);
        currentBox = null;
        Destroy(boxOutline);

	}

    /// <summary>
    /// Applies offset position before throwing.
    /// </summary>
    public void ThrowBox()
    {
        if (isRaycastHit && Vector3.Distance(hitInfo.point, this.transform.position) >= 2.5 ||
            !isRaycastHit)
        {
            Rigidbody boxRB = currentBox.GetComponent<Rigidbody>();
            Vector3 newPos = controller.MainCam.transform.localPosition;
            newPos.z = objectOffset.z;
            currentBox.transform.localPosition = newPos;
            currentBox.transform.parent = null;
            currentBox.layer = LayerMask.NameToLayer("Default");
            currentBox.GetComponent<Collider>().enabled = true;
            currentBox.GetComponent<BoxDragSFX>().ToggleThis(true);
            boxRB.isKinematic = false;
            boxRB.AddForce(controller.MainCam.transform.forward * throwForce, ForceMode.Impulse);
            currentBox = null;
            Destroy(boxOutline);
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
                boxRB.AddForce(controller.MainCam.transform.forward * throwForce / boxRB.mass, ForceMode.Impulse);
            }

			if (hitInfo.transform.tag == "Hardhat")
			{
				hitInfo.transform.GetComponent<Rigidbody>().isKinematic = false;
				hitInfo.transform.GetComponent<Rigidbody>().AddForce(controller.MainCam.transform.forward * throwForce, ForceMode.Impulse);

				knockHat.Contribute();
			}

			if (hitInfo.transform.tag == "Bot")
			{
				punchRobot.Contribute();
			}
		}
    }

    /// <summary>
    /// Move crosshair onto raycast hit. If no hits or the hit point is too close, set position to center.
    /// </summary>
    private void ManageCrosshair()
    {
        if (isRaycastHit && 
            Vector3.Distance(hitInfo.point, this.transform.position) >= boxPlacementDeadzone)
        {
            crosshair.transform.position = controller.MainCam.WorldToScreenPoint(hitInfo.point);
        }
        else
        {
            crosshair.transform.position = controller.MainCam.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0.5f));
        }
    }

    /// <summary>
    /// Do a raycast from the player cam.
    /// </summary>
    /// <param name="hitInfo">Raycast output</param>
    /// <returns>Returns true if the raycast hits a collider.</returns>
    private bool DoRaycast (out RaycastHit hitInfo)
    {
        return Physics.Raycast(controller.MainCam.transform.position, controller.MainCam.transform.forward, out hitInfo, raycastDistance);
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
