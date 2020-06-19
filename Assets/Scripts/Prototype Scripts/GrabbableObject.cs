using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

[RequireComponent(typeof(InteractableObject))]
public class GrabbableObject : DestructibleObject
{
    #region Variables
    public InteractableObject interactionComponent { get; private set; }
    private GrabbableObjectPlacementChecker placementChecker;

    [Header("Grabbed Object Events")]
    public UnityEvent onObjectGrab;
    public UnityEvent onObjectDrop;
    public UnityEvent onObjectThrow;
    public UnityEvent onObjectPlace;
    #endregion


    private void Start()
    {
        interactionComponent = this.GetComponent<InteractableObject>();
        interactionComponent.HighlighterBasis = TargetGameObject;
        interactionComponent.SetupHighlighter();
        placementChecker = interactionComponent.HighlighterInstance.AddComponent<GrabbableObjectPlacementChecker>();
    }



    #region Public Methods
    public void GrabObject(Transform parentTransform)
    {
        onObjectGrab.Invoke();
        AttachToParent(parentTransform);
    }

    public void DropObject(Vector3 dropPosition)
    {
        onObjectDrop.Invoke();
        DetachFromParent();
        this.transform.position = dropPosition;
    }

    public void ThrowObject(Vector3 force, ForceMode forceType)
    {
        onObjectThrow.Invoke();
        DetachFromParent();
        RigidbodyComponent.AddForce(force, forceType);

    }

    public bool PlaceObject(Vector3 newPos)
    {
        if (placementChecker.isPlacable)
        {
            onObjectPlace.Invoke();
            this.transform.position = newPos;
            DetachFromParent();

            return true;
        }
        else
        {
            return false;
        }
    }

    public bool PlaceObject()
    {
        if (placementChecker.isPlacable)
        {
            onObjectPlace.Invoke();
            Transform highlighterTransform = interactionComponent.HighlighterInstance.transform;
            this.transform.position = highlighterTransform.position;
            this.transform.rotation = highlighterTransform.rotation;
            DetachFromParent();

            return true;
        }
        else
        {
            return false;
        }
    }

    public void ManagePlacementHighlighter(bool doShow, Vector3 customPosition, Quaternion customRotation)
    {
        interactionComponent.ShowHighlighter(doShow);
        GameObject highlighter = interactionComponent.HighlighterInstance;
        
        if (doShow)
        {
            highlighter.transform.parent = null;            
        }
        else
        {
            highlighter.transform.parent = interactionComponent.HighlighterBasis.transform;
        }
        highlighter.transform.position = customPosition;
        highlighter.transform.rotation = customRotation;
    }

    public void HidePlacementHighlighter()
    {
        interactionComponent.ShowHighlighter(false);
        GameObject highlighter = interactionComponent.HighlighterInstance;
        highlighter.transform.parent = interactionComponent.HighlighterBasis.transform;
        highlighter.transform.localPosition = Vector3.zero;
        highlighter.transform.localRotation = Quaternion.identity;
    }


    public void RenderToLayer(string layerName)
    {
        TargetGameObject.layer = LayerMask.NameToLayer(layerName);
    }
    #endregion



    #region Private Methods
    public void DetachFromParent()
    {
        this.transform.parent = null;
        RenderToLayer("Default");

        foreach(Collider collider in ColliderComponents)
        {
            collider.enabled = true;
        }
        RigidbodyComponent.isKinematic = false;

        interactionComponent.ShowHighlighter(false);
        interactionComponent.ResetHighlighter();
    }

    public void AttachToParent(Transform parentTransform)
    {
        this.transform.parent = parentTransform;
        this.transform.localRotation = parentTransform.rotation;
        this.transform.localPosition = parentTransform.position;

        foreach (Collider collider in ColliderComponents)
        {
            collider.enabled = false;
        }
        RigidbodyComponent.isKinematic = true;

        interactionComponent.ShowHighlighter(false);
    }
    #endregion
}
