using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityStandardAssets.Cameras;

public class ClawMachine : MonoBehaviour
{
    #region Variables
    [Header("Claw Rails")]
    [SerializeField] private Transform horizontalRail;
    [SerializeField] private Transform verticalRail;

    [Header("Properties")]
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float shaftMoveSpeed = 5.0f;
    [SerializeField] private float grabDelay = 0.5f;
    private float grabDelayTimer;

    [Tooltip("The size of the detection area from the grabbable object's center. " +
             "Only accounts for x and z axes.")]
    [SerializeField] private float targetDetectionThreshold = 0.3f;

    [Header("Camera Settings")]
    [SerializeField] private Camera clawCam;
    private Transform lookTRS;

    [Header("Broken Line Settings")]
    [SerializeField] private BrokenLines lines;
    [Tooltip("The offset from the claw head's position.")]
    [SerializeField] private Vector3 lineStartOffset = Vector3.zero;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color interactableColor;
    [SerializeField] private Color placementColor;

    [Header("References")]
    [SerializeField] private Transform grabbedObjectOffset;
    [SerializeField] private Transform lightTRS;
    [SerializeField] private Transform clawHeadTRS;
    [SerializeField] private Transform animatableTRS;
    [SerializeField] private UnityEventsHandler objectDetector;
    private ClawGrabbable grabbedObject;
    private ClawGrabbable highlightedObject;
    private bool controlsEnabled = true;
    private bool doGrab = false;
    private bool doPlace = false;
    private bool isResetting = false;
    private RaycastHit raycastHit;
    private ClawMachineAI clawAI;

    [Header("Movement Limiters")]
    [Tooltip("0 = min, 1 = max")]
    [SerializeField] private Transform[] horizontalLimiters = new Transform[2];
    [Tooltip("0 = min, 1 = max")]
    [SerializeField] private Transform[] verticalLimiters = new Transform[2];

    [Header("Events")]
    public UnityEvent onObjectGrab;
    public UnityEvent onObjectDrop;
    public UnityEvent onObjectPlace;
	public UnityEvent onNPCGrab;
	[Space(20)]
    public UnityEvent onPlayerKick;
    public UnityEvent onPlayerLeave;
    #endregion




    private void OnEnable()
    {
        //Reset claw to idle state when doing something else
        isResetting = animatableTRS.localPosition.y < 0;
        if (isResetting)
        {
            controlsEnabled = false;
            doGrab = false;
            doPlace = true;
            highlightedObject = null;            
        }

        //Get grabbed object if any
        if (grabbedObjectOffset.childCount > 0)
        {
            grabbedObject = grabbedObjectOffset.GetChild(0).GetComponentInChildren<ClawGrabbable>();
        }
    }

    private void Start()
    {
        onObjectGrab.AddListener(ClawClamp);
        onObjectPlace.AddListener(ClawRelease);
        onObjectDrop.AddListener(ClawRelease);

        lookTRS = new GameObject("ClawCamTRS").transform;
        lookTRS.parent = this.transform;
        lookTRS.position = clawCam.transform.position;
        lookTRS.rotation = clawCam.transform.rotation;

        clawAI = this.GetComponent<ClawMachineAI>();
    }

    private void FixedUpdate()
    {
        //Enable claw controls when reset is done
        if (isResetting) /*&&
            animatableTRS.localPosition.y >= 0)*/
        {
            isResetting = false;
            controlsEnabled = true;
            //animatableTRS.localPosition = Vector3.zero;
        }
        else //if (!isResetting)
        {
            //Player leave function
            if (Input.GetKeyDown(KeyCode.E))
            {
                PlayerLeave();
                //KickPlayer();
            }
        }

        if (animatableTRS.localPosition.y >= 0)
            animatableTRS.localPosition = Vector3.zero;

        if (controlsEnabled)
        {
            MoveClaw();
            UpdateClawRails();
        }

        if (grabbedObject != null)
        {
            if (controlsEnabled)
            {
                if (!doPlace &&
                    Input.GetButtonDown("Place Object"))
                {
                    doPlace = true;
                    controlsEnabled = false;
				}

                if (Input.GetButtonDown("Throw Object"))
                {

					// Drop minibot
					if (grabbedObject.transform.tag == "MiniBot")
					{
						NPC_Controller bot = grabbedObject.GetComponent<NPC_Controller>();
						if (bot)
						{
							bot.GetDropped();
						}
					}

					grabbedObject.gameObject.layer = LayerMask.NameToLayer("Default");
                    grabbedObject.DetachFromParent();
                    grabbedObject = null;
                    objectDetector.gameObject.SetActive(true);
                    onObjectDrop.Invoke();
                }
            }
        }
        else
        {
            if (!doGrab) ManageHighlighter();

            if (highlightedObject != null)
            {
                //if (DoRaycast(clawHeadTRS))
                //{
                //    Debug.DrawLine(clawHeadTRS.position, raycastHit.point, Color.cyan);
                //}

                if (!doGrab &&
                    controlsEnabled &&
                    Input.GetButtonDown("Grab Object") &&
                    Physics.Raycast(clawHeadTRS.position, -clawHeadTRS.up))
                {
                    controlsEnabled = false;
                    doGrab = true;
                }
            }
        }

        
        if (doGrab)
        {
            //Downwards motion
            if (grabbedObject == null)
            {
                if (!DetectObject())
                {
                    animatableTRS.position -= animatableTRS.up * shaftMoveSpeed * Time.deltaTime;

                    if (highlightedObject != null &&
                        DoRaycast(objectDetector.transform) &&
                        raycastHit.transform != highlightedObject.transform)
                    {
                        doGrab = false;
                        doPlace = true;
                        highlightedObject = null;
                        lightTRS.gameObject.SetActive(false);
                    }
                }
            }
            //Upwards motion
            else
            {
                if (grabDelayTimer < Time.time)
                    animatableTRS.position += animatableTRS.up * shaftMoveSpeed * Time.deltaTime;

                if (Vector3.Distance(animatableTRS.localPosition, Vector3.zero) < 0.1f)
                {
                    controlsEnabled = true;
                    doGrab = false;
                    animatableTRS.localPosition = Vector3.zero;
                }
            }
        }
        else if (doPlace)
        {
            //Downwards motion
            if (grabbedObject != null)
            {
                animatableTRS.position -= animatableTRS.up * shaftMoveSpeed * Time.deltaTime;

                if (grabbedObject.rigidbodyComponent.SweepTest(-clawHeadTRS.up, out RaycastHit sweepHit) &&
                    sweepHit.distance <= 0.3f)
                {
                    PlaceObject();
                }
            }
            //Upwards motion
            else
            {
                animatableTRS.position += animatableTRS.up * shaftMoveSpeed * Time.deltaTime;

                if (Vector3.Distance(animatableTRS.localPosition, Vector3.zero) < 0.1f)
                {
                    controlsEnabled = true;
                    doPlace = false;
                    animatableTRS.localPosition = Vector3.zero;
                }
            }
        }
    }

    private void Update()
    {
        if (DoRaycast(clawHeadTRS))
        {
            //Debug.DrawLine(clawHeadTRS.position, rh.point, Color.green);
            lines.gameObject.SetActive(true);
            LineRenderer lr = lines.LineRendererComponent;
            lr.SetPosition(0, clawHeadTRS.position + lineStartOffset);
            lr.SetPosition(1, raycastHit.point);
            lookTRS.position = clawCam.transform.position;
            lookTRS.LookAt(raycastHit.point);
            clawCam.transform.rotation = Quaternion.Slerp(clawCam.transform.rotation, lookTRS.rotation, Time.deltaTime);

            if (lightTRS.gameObject.activeSelf)
            {
                lines.LineColor = interactableColor;
            }
            else
            {
                if (grabbedObject != null)
                {
                    lines.LineColor = placementColor;
                }
                else
                {
                    lines.LineColor = normalColor;
                }
            }
        }
        else
        {
            lines.gameObject.SetActive(false);
        }
    }


    public void ToggleClawControls(bool state)
    {
        this.enabled = state;
        clawCam.transform.parent.gameObject.SetActive(state);
        lines.gameObject.SetActive(state);
    }

    public void KickPlayer()
    {
        if (this.enabled)
        {
            onPlayerKick.Invoke();
            clawAI.ToggleAI(true);
            LeaveControls();
        }
    }

    private void PlayerLeave()
    {
        onPlayerLeave.Invoke();
        LeaveControls();
    }

    private void LeaveControls()
    {
        Player.Instance.SetEnabled(true);
        ToggleClawControls(false);
    }

    #region Private Methods
    private void MoveClaw()
    {
        Vector3 v1 = this.transform.position;
        v1.y = 0;
        
        Vector3 v2 = clawCam.transform.position;
        v2.y = 0;

        Direction toClaw = new Direction(v2, v1);
        Vector3 movementVector = clawCam.transform.right * Input.GetAxis("Horizontal") +
                                 toClaw.localDirection * Input.GetAxis("Vertical");
        movementVector = Vector3.ClampMagnitude(movementVector, 1);
        movementVector.y = 0;
        this.transform.Translate(movementVector * moveSpeed * Time.deltaTime);
        float xPos = Mathf.Clamp(this.transform.position.x, horizontalLimiters[0].position.x, horizontalLimiters[1].position.x);
        float zPos = Mathf.Clamp(this.transform.position.z, verticalLimiters[0].position.z, verticalLimiters[1].position.z);
        this.transform.position = new Vector3(xPos, this.transform.position.y, zPos);
    }

    private void UpdateClawRails()
    {
        verticalRail.position = new Vector3(this.transform.position.x,
                                            verticalRail.position.y,
                                            verticalRail.position.z);

        horizontalRail.position = new Vector3(horizontalRail.transform.position.x,
                                              horizontalRail.transform.position.y,
                                              this.transform.position.z);
    }

    private bool DetectObject()
    {
        //Exit immediately if no objects in trigger
        if (objectDetector.ObjectsInTrigger.Count < 1) return false;

        foreach (GameObject target in objectDetector.ObjectsInTrigger)
        {
            if (target != null &&
                target.GetComponentInChildren<ClawGrabbable>() != null)
            {
                GrabObject();
                grabDelayTimer = grabDelay + Time.time;
                onObjectGrab.Invoke();

				// Grab minibot
				if (grabbedObject.transform.tag == "MiniBot")
				{
					NPC_Controller bot = grabbedObject.GetComponent<NPC_Controller>();
					if (bot)
					{
						bot.GetGrabbed();
						onNPCGrab.Invoke();
					}
				}

                return true;
            }
        }
        return false;
    }

    private void GrabObject()
    {
        //Clear out references when an object is grabbed
        objectDetector.ObjectsInTrigger.Clear();

        //Attach to this transform and adjust references
        highlightedObject.AttachToParent(grabbedObjectOffset, true, true);
        //highlightedObject.transform.localPosition = Vector3.zero;
        grabbedObject = highlightedObject;
        //grabbedObject.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        highlightedObject = null;

        //Snap grabbed object to the claw head's center
        grabbedObject.transform.localPosition = 
            new Vector3(0, grabbedObject.transform.localPosition.y, 0);

        //Hide detector and highlighter
        lightTRS.gameObject.SetActive(false);
        objectDetector.gameObject.SetActive(false);
    }

    private void PlaceObject()
    {
		// Place minibot
		if (grabbedObject.transform.tag == "MiniBot")
		{
			NPC_Controller bot = grabbedObject.GetComponent<NPC_Controller>();
			if (bot)
			{
				bot.GetPlaced();
			}
		}

		grabbedObject.gameObject.layer = LayerMask.NameToLayer("Default");
        grabbedObject.DetachFromParent();
        grabbedObject = null;
        objectDetector.gameObject.SetActive(true);
        onObjectPlace.Invoke();
	}

    private void ManageHighlighter()
    {
        if (DoRaycast(clawHeadTRS) &&
            Physics.OverlapSphere(raycastHit.point, 0.5f).Length < 2)
        {
            Vector3 targetPos2D = raycastHit.transform.position;
            targetPos2D.y = 0;

            Vector3 thisPos2D = this.transform.position;
            thisPos2D.y = 0;

            if (Vector3.Distance(targetPos2D, thisPos2D) > targetDetectionThreshold)
            {
                lightTRS.gameObject.SetActive(false);
                return;
            }

            highlightedObject = raycastHit.transform.GetComponent<ClawGrabbable>();
        }
        else
        {
            highlightedObject = null;
        }

        lightTRS.gameObject.SetActive(highlightedObject != null);
        if (lightTRS.gameObject.activeSelf)
        {
            lightTRS.position = raycastHit.transform.position + raycastHit.normal * 1.5f;
        }
    }

    private void DoClampAnimation(bool state)
    {
        animator.SetBool("DoClamp", state);
    }

    private void ClawClamp()
    {
        DoClampAnimation(true);
    }

    private void ClawRelease()
    {
        DoClampAnimation(false);
    }

    private bool DoRaycast(Transform raycastPoint)
    {
        return Physics.Raycast(raycastPoint.position, -raycastPoint.up, out raycastHit);
    }
    #endregion
}