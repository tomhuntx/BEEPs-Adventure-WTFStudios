using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(InteractableObject))]
public class GrabbableObject : DestructibleObject
{
    #region Variables
    public InteractableObject interactionComponent { get; private set; }
    private GrabbableObjectPlacementChecker placementChecker;
    private bool isOffsetApplied = false;
    private CollisionDetectionMode initialDetectionMode;

    [Header("Grabbed Object Events")]
    public UnityEvent onObjectGrab;
    public UnityEvent onObjectDrop;
    public UnityEvent onObjectThrow;
    public UnityEvent onObjectPlace;
    #endregion


    private void Start()
    {
        onImpactDamage.AddListener(UpdateHighlighterModel);

        interactionComponent = this.GetComponent<InteractableObject>();
        interactionComponent.HighlighterBasis = TargetGameObject;        

        if (TargetGameObject.transform.parent != null &&
            TargetGameObject.transform.parent == this.transform)
        {
            //GameObject mainGO = this.transform.gameObject;
            interactionComponent.SetupHighlighter(this.transform);
            placementChecker = interactionComponent.HighlighterInstance.AddComponent<GrabbableObjectPlacementChecker>();
        }
        else
        {
            interactionComponent.SetupHighlighter();
            placementChecker = interactionComponent.HighlighterInstance.AddComponent<GrabbableObjectPlacementChecker>();
        }        
    }


    #region Public Methods
    public void GrabObject(Transform parentTransform, bool colliderState = false)
    {
        onObjectGrab.Invoke();
        AttachToParent(parentTransform, colliderState);
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
            this.transform.position = newPos;
            DetachFromParent();
            onObjectPlace.Invoke();

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
            Transform highlighterTransform = interactionComponent.HighlighterInstance.transform;
            this.transform.position = highlighterTransform.position;
            this.transform.rotation = highlighterTransform.rotation;
            DetachFromParent();
            onObjectPlace.Invoke();

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
            highlighter.transform.position = customPosition;
            highlighter.transform.rotation = customRotation;
            interactionComponent.SetHighlighterInvalid(!placementChecker.isPlacable);
        }
        else
        {
            highlighter.transform.parent = this.transform;
            highlighter.SetActive(false);
        }
    }

	public void HidePlacementHighlighter()
    {
        interactionComponent.ShowHighlighter(false);
        GameObject highlighter = interactionComponent.HighlighterInstance;
        highlighter.transform.parent = this.transform;
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

        BoxDragSFX sfx = this.transform.GetComponentInChildren<BoxDragSFX>();
        if (sfx != null) sfx.ToggleThis(true);

        foreach (Collider collider in ColliderComponents)
        {
            collider.enabled = true;
        }
        RigidbodyComponent.isKinematic = false;
        RigidbodyComponent.collisionDetectionMode = initialDetectionMode;


        interactionComponent.ShowHighlighter(false);
        interactionComponent.ResetHighlighter();
    }

    public void AttachToParent(Transform parentTransform, bool colliderState = false)
    {
        this.transform.parent = parentTransform;
        this.transform.localRotation = parentTransform.rotation;
        this.transform.localPosition = Vector3.zero;

        BoxDragSFX sfx = this.transform.GetComponentInChildren<BoxDragSFX>();
        if (sfx != null) sfx.ToggleThis(false);

        foreach (Collider collider in ColliderComponents)
        {
            collider.enabled = colliderState;
        }

        initialDetectionMode = RigidbodyComponent.collisionDetectionMode;
        RigidbodyComponent.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;        
        RigidbodyComponent.isKinematic = true;

        interactionComponent.ShowHighlighter(false);
        DetachForceAppliers();
    }

	/// <summary>
	/// Used with explosive box throw to always destroy it on its next hit. 
	/// </summary>
	public void DestroyOnImpact()
	{
		GetComponent<DestructibleObject>().forceMagnitudeThreshold = 0.1f;
        RigidbodyComponent.WakeUp();
	}

    private void UpdateHighlighterModel()
    {
        MeshFilter mesh = interactionComponent.HighlighterInstance.GetComponentInChildren<MeshFilter>();
        mesh.mesh = interactionComponent.HighlighterBasis.GetComponent<MeshFilter>().mesh;
    }
    #endregion



    #region Static Methods
    public static void AttachToParent(Transform target, Transform parent, 
                                      bool colliderState = false, bool disableRaycast = false)
    {
        if (disableRaycast) 
            target.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        target.transform.parent = parent;
        target.transform.localRotation = parent.localRotation;
        target.transform.localPosition = Vector3.zero;

        BoxDragSFX sfx = target.transform.GetComponentInChildren<BoxDragSFX>();
        if (sfx != null) sfx.ToggleThis(false);

        Collider collider = target.GetComponentInChildren<Collider>();
        if (collider != null)
        {
            Rigidbody rb = collider.attachedRigidbody;
            collider.enabled = colliderState;
            
            if (rb != null)
            {
                if (rb.collisionDetectionMode != CollisionDetectionMode.ContinuousDynamic)
                    rb.isKinematic = true;

                PersistentForceRigidbody[] forceAppliers = FindObjectsOfType<PersistentForceRigidbody>();
                if (forceAppliers.Length > 0)
                {
                    foreach (PersistentForceRigidbody force in forceAppliers)
                    {
                        force.RemoveReference(rb);
                    }
                }
            }
        }        
    }

    public static void DetachFromParent(Transform target, 
                                        bool colliderState = true, bool enableRaycast = true)
    {
        if (enableRaycast) 
            target.gameObject.layer = LayerMask.NameToLayer("Default");

        target.transform.parent = null;

        BoxDragSFX sfx = target.transform.GetComponentInChildren<BoxDragSFX>();
        if (sfx != null) sfx.ToggleThis(false);

        Collider collider = target.GetComponentInChildren<Collider>();
        if (collider != null)
        {
            collider.enabled = colliderState;
            Rigidbody rb = collider.attachedRigidbody;            
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
    }
    #endregion
}
