using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityStandardAssets.Cameras;

public class ClawMachine : MonoBehaviour
{
    #region Variables
    public ProtectCameraFromWallClip antiClipCam;
    public Vector3 offset;
    [Header("Claw Rails")]
    [SerializeField] private Transform horizontalRail;
    [SerializeField] private Transform verticalRail;

    [Header("Properties")]
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 2.5f;
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
    [SerializeField] private Transform lightTRS;
    [SerializeField] private Transform clawHeadTRS;
    [SerializeField] private Transform animatableTRS;
    [SerializeField] private UnityEventsHandler objectDetector;
    private ClawGrabbable grabbedObject;
    private ClawGrabbable highlightedObject;
    private bool controlsEnabled = true;
    private bool doGrab = false;
    private bool doPlace = false;
    private RaycastHit raycastHit;

    [Header("Movement Limiters")]
    [Tooltip("0 = min, 1 = max")]
    [SerializeField] private Transform[] horizontalLimiters = new Transform[2];
    [Tooltip("0 = min, 1 = max")]
    [SerializeField] private Transform[] verticalLimiters = new Transform[2];

    [Header("Events")]
    public UnityEvent onObjectGrab;
    public UnityEvent onObjectDrop;
    public UnityEvent onObjectPlace;
    #endregion



    private void Start()
    {
        onObjectGrab.AddListener(ClawClamp);
        onObjectPlace.AddListener(ClawRelease);
        onObjectDrop.AddListener(ClawRelease);

        lookTRS = new GameObject("ClawCamTRS").transform;
        lookTRS.parent = this.transform;
        lookTRS.position = clawCam.transform.position;
        lookTRS.rotation = clawCam.transform.rotation;

        antiClipCam.cameraOffset = offset;
    }

    private void FixedUpdate()
    {
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

                if (Input.GetButtonDown("Drop Object"))
                {
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
                if (DoRaycast(clawHeadTRS))
                {
                    Debug.DrawLine(clawHeadTRS.position, raycastHit.point, Color.cyan);
                }

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
                    animatableTRS.position -= animatableTRS.up * moveSpeed * Time.deltaTime;

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
                    animatableTRS.position += animatableTRS.up * moveSpeed * Time.deltaTime;

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
                animatableTRS.position -= animatableTRS.up * moveSpeed * Time.deltaTime;

                if (grabbedObject.rigidbodyComponent.SweepTest(-clawHeadTRS.up, out RaycastHit sweepHit) &&
                    sweepHit.distance <= 0.3f)
                {
                    PlaceObject();
                }
            }
            //Upwards motion
            else
            {
                animatableTRS.position += animatableTRS.up * moveSpeed * Time.deltaTime;

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



    #region Private Methods
    private void MoveClaw()
    {
        Vector3 camForward = clawCam.transform.forward;
        camForward.y = 0;

        Vector3 camRight = clawCam.transform.right;
        camRight.y = 0;

        Vector3 movementVector = camRight * Input.GetAxis("Horizontal") +
                                 camForward * Input.GetAxis("Vertical");
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
            if (target.GetComponentInChildren<ClawGrabbable>() != null)
            {
                GrabObject();
                grabDelayTimer = grabDelay + Time.time;
                onObjectGrab.Invoke();
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
        highlightedObject.AttachToParent(clawHeadTRS);
        highlightedObject.transform.localPosition = Vector3.zero;
        grabbedObject = highlightedObject;
        grabbedObject.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
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
            lightTRS.gameObject.SetActive(highlightedObject != null);
            if (lightTRS.gameObject.activeSelf)
            {
                lightTRS.position = raycastHit.transform.position + raycastHit.normal * 1.5f;
            }
        }
        else
        {
            highlightedObject = null;
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