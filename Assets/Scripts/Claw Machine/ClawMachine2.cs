using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClawMachine2 : MonoBehaviour
{
    #region Variables
    [Header("Claw Rails")]
    [SerializeField] private Transform horizontalRail;
    [SerializeField] private Transform verticalRail;

    [Header("Properties")]
    [SerializeField] private float moveSpeed = 2.5f;

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
    private Vector3 targetPos;
    private Vector3 originalPos;

    [Header("Events")]
    public UnityEvent onObjectGrab;
    public UnityEvent onObjectDrop;
    public UnityEvent onObjectPlace;
    #endregion


    private void Start()
    {
        originalPos = animatableTRS.position;
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
                    grabbedObject.DetachFromParent();
                    grabbedObject = null;
                    objectDetector.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            if (!doGrab) ManageHighlighter();

            if (highlightedObject != null)
            {
                if (!doGrab &&
                    controlsEnabled &&
                    Input.GetButtonDown("Grab Object") &&
                    Physics.Raycast(animatableTRS.position, -animatableTRS.up, out RaycastHit rayHit))
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
                //animatableTRS.position = Vector3.Lerp(animatableTRS.position, targetPos, moveSpeed * Time.deltaTime);
                animatableTRS.position -= animatableTRS.up * moveSpeed * Time.deltaTime;
                DetectObject();
            }
            //Upwards motion
            else
            {
                //animatableTRS.localPosition = Vector3.Lerp(animatableTRS.localPosition, Vector3.zero, moveSpeed * Time.deltaTime);
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
                //animatableTRS.localPosition = Vector3.Lerp(animatableTRS.localPosition, targetPos, moveSpeed * Time.deltaTime);
                animatableTRS.position -= animatableTRS.up * moveSpeed * Time.deltaTime;

                if (grabbedObject.rigidbodyComponent.SweepTest(-clawHeadTRS.up, out RaycastHit sweepHit) &&
                    sweepHit.distance <= 0.3f)
                {
                    DropObject();
                }
            }
            //Upwards motion
            else
            {
                //animatableTRS.localPosition = Vector3.Lerp(animatableTRS.localPosition, Vector3.zero, moveSpeed * Time.deltaTime);
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



    #region Private Methods
    private void MoveClaw()
    {
        Vector3 movementVector = this.transform.right * Input.GetAxis("Horizontal") +
                                 this.transform.forward * Input.GetAxis("Vertical");
        this.transform.Translate(movementVector * moveSpeed * Time.deltaTime);
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
                return true;
            }
        }
        return false;
    }

    private void GrabObject()
    {
        //Clear out references when an object is grabbed
        objectDetector.ObjectsInTrigger.Clear();

        highlightedObject.AttachToParent(clawHeadTRS);
        grabbedObject = highlightedObject;
        highlightedObject = null;
        lightTRS.gameObject.SetActive(false);
        objectDetector.gameObject.SetActive(false);
    }

    private void DropObject()
    {
        grabbedObject.DetachFromParent();
        grabbedObject = null;
        objectDetector.gameObject.SetActive(true);
    }

    private void ManageHighlighter()
    {
        if (Physics.Raycast(clawHeadTRS.position, -clawHeadTRS.up, out RaycastHit hitInfo) &&
            Physics.OverlapSphere(hitInfo.point, 0.5f).Length < 2)
        {
            highlightedObject = hitInfo.transform.GetComponent<ClawGrabbable>();
            lightTRS.gameObject.SetActive(highlightedObject != null);
            if (lightTRS.gameObject.activeSelf)
            {
                lightTRS.position = hitInfo.transform.position + hitInfo.normal * 1.5f;
            }
        }
        else
        {
            highlightedObject = null;
        }
    }
    #endregion

}
