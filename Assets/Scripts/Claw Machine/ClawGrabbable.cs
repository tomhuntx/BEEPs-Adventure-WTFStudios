using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClawGrabbable : MonoBehaviour
{
    #region Variables
    [Header("Events")]
    public UnityEvent onGrab;
    public UnityEvent onDrop;
    public UnityEvent onPlace;

    /// <summary>
    /// The rigidbody component attached along with this script if any, otherwise, will remain null.
    /// </summary>
    public Rigidbody rigidbodyComponent { get; private set; }
    #endregion



    private void Start()
    {
        rigidbodyComponent = this.GetComponentInChildren<Rigidbody>();
    }


    public void AttachToParent(Transform parentTransform, bool colliderState = true, bool ignoreRaycast = false)
    {
        //Disable any rigidbody component
        if (rigidbodyComponent != null)
        {
            rigidbodyComponent.isKinematic = true;
            rigidbodyComponent.useGravity = false;
        }

        onGrab.Invoke();

        //Grabbable object case
        GrabbableObject grabbable = this.GetComponent<GrabbableObject>();
        if (grabbable != null)
        {
            grabbable.GrabObject(parentTransform, colliderState);
            return;
        }

        //Heavy box case
        Box heavyBox = this.GetComponent<Box>();
        if (heavyBox != null &&
            heavyBox.TypeOf == Box.Type.Heavy)
        {
            GrabbableObject.AttachToParent(this.transform, parentTransform, colliderState, ignoreRaycast);
            return;
        }

        //Robot object case
        Robot robot = this.GetComponent<Robot>();
        if (robot != null)
        {
            this.transform.parent = parentTransform;
            if (ignoreRaycast) robot.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            //robot.SetAnimationState("NAME", true); //set whatever future animations here
            return;
        }

        //Default case
        this.transform.parent = parentTransform;        
    }

    public void DetachFromParent()
    {
        //Enable any rigidbody component
        if (rigidbodyComponent != null)
        {
            rigidbodyComponent.isKinematic = false;
            rigidbodyComponent.useGravity = true;
        }

        //Grabbable object case
        GrabbableObject grabbable = this.GetComponent<GrabbableObject>();
        if (grabbable != null)
        {
            grabbable.DetachFromParent();
            return;
        }

        //Heavy box case
        Box heavyBox = this.GetComponent<Box>();
        if (heavyBox != null &&
            heavyBox.TypeOf == Box.Type.Heavy)
        {
            GrabbableObject.DetachFromParent(this.transform);
            return;
        }

        //Robot object case
        Robot robot = this.GetComponent<Robot>();
        if (robot != null)
        {
            this.transform.parent = null;
            //robot.SetAnimationState("NAME", true); //set whatever future animations here
            return;
        }

        //Default case
        this.transform.parent = null;        
    }
}
