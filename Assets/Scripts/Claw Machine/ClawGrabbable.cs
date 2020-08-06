﻿using System.Collections;
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


    public void AttachToParent(Transform parentTransform)
    {
        onGrab.Invoke();

        //Grabbable object case
        GrabbableObject grabbable = this.GetComponent<GrabbableObject>();
        if (grabbable != null)
        {
            grabbable.GrabObject(parentTransform);
            return;
        }

        //Robot object case
        Robot robot = this.GetComponent<Robot>();
        if (robot != null)
        {
            this.transform.parent = parentTransform;
            //robot.SetAnimationState("NAME", true); //set whatever future animations here
            return;
        }

        //Default case
        this.transform.parent = parentTransform;


        //Disable any rigidbody component
        if (rigidbodyComponent != null)
        {
            rigidbodyComponent.isKinematic = true;
            rigidbodyComponent.useGravity = false;
        }
    }

    public void DetachFromParent()
    {
        //Grabbable object case
        GrabbableObject grabbable = this.GetComponent<GrabbableObject>();
        if (grabbable != null)
        {
            grabbable.DetachFromParent();
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

        //Enable any rigidbody component
        if (rigidbodyComponent != null)
        {
            rigidbodyComponent.isKinematic = false;
            rigidbodyComponent.useGravity = true;
        }
    }
}