using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClawMachine : MonoBehaviour
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
    [SerializeField] private UnityEventsHandler objectDetector;
    [SerializeField] private Animator anim;
    private ClawGrabbable grabbedObject;
    private ClawGrabbable highlightedObject;
    private bool controlsEnabled = true;

    [Header("Events")]
    public UnityEvent onObjectGrab;
    public UnityEvent onObjectDrop;
    public UnityEvent onObjectPlace;
    #endregion



    private void FixedUpdate()
    {
        if (controlsEnabled) MoveClaw();
        UpdateClawRails();

        if (grabbedObject != null)
        {
            if (controlsEnabled && Input.GetButtonDown("Place Object"))
                SetAnimation("moveDown");    

            //Drop the object at the moment that it's near the floor
            if (anim.GetBool("moveDown"))
            {
                if (Physics.Raycast(grabbedObject.transform.position, -clawHeadTRS.up, out RaycastHit hitInfo))
                {
                    if (grabbedObject.rigidbodyComponent.SweepTest(-clawHeadTRS.up, out RaycastHit sweepHit))
                    {
                        print(sweepHit.distance);
                        if (sweepHit.distance <= 0.3f)
                        {
                            DropObject();
                            SetAnimation("moveUp");
                            objectDetector.gameObject.SetActive(true);
                            onObjectPlace.Invoke();
                        }
                    }
                }
            }

            if (controlsEnabled && Input.GetButtonDown("Drop Object"))
            {
                grabbedObject.DetachFromParent();
                grabbedObject = null;
                SetAnimationsState(false);
                objectDetector.gameObject.SetActive(true);
                onObjectDrop.Invoke();
            }
        }
        else
        {
            ManageHighlighter();
            if (highlightedObject != null)
            {
                if (controlsEnabled && Input.GetButtonDown("Grab Object"))
                    SetAnimation("moveDown");

                if (anim.GetBool("moveDown") && DetectObject())
                {
                    SetAnimation("moveUp");
                    objectDetector.gameObject.SetActive(false);
                    onObjectGrab.Invoke();
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
        foreach(GameObject target in objectDetector.ObjectsInTrigger)
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
        highlightedObject.AttachToParent(clawHeadTRS);
        grabbedObject = highlightedObject;
        highlightedObject = null;
        lightTRS.gameObject.SetActive(false);
    }

    private void DropObject()
    {
        grabbedObject.DetachFromParent();
        grabbedObject = null;
    }

    private void ManageHighlighter()
    {
        if (Physics.Raycast(clawHeadTRS.position, -clawHeadTRS.up, out RaycastHit hitInfo))
        {
            highlightedObject = hitInfo.transform.GetComponent<ClawGrabbable>();
            lightTRS.gameObject.SetActive(highlightedObject != null);
            if (lightTRS.gameObject.activeSelf)
            {
                lightTRS.position = hitInfo.transform.position + hitInfo.normal * 1.5f;
            }
        }
    }

    private void SetAnimationsState(bool state)
    {
        foreach (AnimatorControllerParameter parameter in anim.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool)
                anim.SetBool(parameter.name, state);
        }
    }
    #endregion



    #region Public Methods
    public void SetAnimation(string boolName, bool state)
    {
        //set all to false first
        SetAnimationsState(false);

        //set true to target animation
        anim.SetBool(boolName, state);
    }

    public void SetAnimation(string boolName)
    {
        SetAnimation(boolName, true);
    }
    #endregion

    


    //Do not call these methods and is solely used by animations only!
    #region Animation Trigger Events
    public void OnClawDownMotionEnter()
    {
        controlsEnabled = false;
    }

    public void OnClawDownMotionExit()
    {

    }

    public void OnClawUpMotionEnter()
    {

    }

    public void OnClawUpMotionExit()
    {
        controlsEnabled = true;        
    }
    #endregion
}
