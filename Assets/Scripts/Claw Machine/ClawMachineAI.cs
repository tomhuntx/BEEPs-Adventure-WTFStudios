using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Panda;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PandaBehaviour))]
public class ClawMachineAI : MonoBehaviour
{
    public static ClawMachineAI Instance;

    #region Exposed Variables
    [SerializeField] private float moveSpeed = 5.0f;

    [Header("Claw Rails")]
    [SerializeField] private Transform horizontalRail;
    [SerializeField] private Transform verticalRail;

    [Header("References")]
    [SerializeField] private UnityEventsHandler colliderEvents;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private Transform grabbedBoxOffset;

    [Header("Positions")]
    [SerializeField] private Transform boxSource;
    [SerializeField] private Transform cardboardChute;
    [SerializeField] private Transform explosiveChute;
    [SerializeField] private Transform heavyChute;

    [Header("Events")]
    [SerializeField] public UnityEvent onComponentDisable;
    [SerializeField] public UnityEvent onComponentEnable;
    #endregion


    #region Private Variables
    private Animator animator;
    private Box targetBox;
    private Box grabbedBox;
    private PandaBehaviour pandaBehaviour;
    private RaycastHit raycastHit;
    private bool isRaycastHit;

    private bool hasArrived = true;
    private Vector3 newLocation = Vector3.negativeInfinity;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        pandaBehaviour = this.GetComponent<PandaBehaviour>();
        Instance = this;
    }

    private void Update()
    {
        //Only move if there's a set valid location
        if (!hasArrived &&
            newLocation != Vector3.negativeInfinity)
        {
            //Move to location
            this.transform.position = Vector3.Lerp(this.transform.position, newLocation, moveSpeed * Time.deltaTime);

            //Negate y value then get distance
            float distance = VectorFlat3D.GetFlattenedDistance(this.transform.position, newLocation, VectorFlat3D.Axis.y);
            if (distance < 0.1f)
            {
                hasArrived = true;
                this.transform.position = newLocation;
                newLocation = Vector3.negativeInfinity;
            }
        }

        isRaycastHit = DoRaycast();
    }

    private void OnEnable()
    {
        onComponentDisable.Invoke();
    }

    private void OnDisable()
    {
        onComponentEnable.Invoke();
    }



    private void GoToLocation(Vector3 location)
    {
        newLocation = location;
        hasArrived = false;
    }

    private void GrabBox()
    {
        GrabbableObject.AttachToParent(targetBox.transform, grabbedBoxOffset, false, true);
    }

    private void DropBox()
    {
        GrabbableObject.DetachFromParent(grabbedBox.transform);
    }

    private bool DoRaycast()
    {
        Ray ray = new Ray(raycastOrigin.position, -raycastOrigin.up);
        return Physics.Raycast(ray, out raycastHit);
    }

    public void ToggleAI(bool state)
    {
        pandaBehaviour.enabled = state;
        animator.enabled = state;
        this.enabled = state;
        SetAnimation("Release");
    }

    public void SetAnimation(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    #region PandaBT Methods
    bool BoxInTrigger()
    {
        foreach (GameObject gameObject in colliderEvents.ObjectsInTrigger)
        {
            Box box = gameObject.GetComponentInChildren<Box>();
            if (box != null)
            {
                targetBox = box;
                return true;
            }
        }
        return false;
    }

    bool HasBox()
    {
        return grabbedBox != null;
    }

    bool HasArrived()
    {
        return hasArrived;
    }

    bool TargetBoxMismatch()
    {
        return targetBox.transform != raycastHit.transform &&
               raycastHit.transform.GetComponentInChildren<Box>() == null;
    }

    void GoToBoxSource()
    {
        SetLocation(boxSource.position);
    }

    void GoToCardboardChute()
    {
        SetLocation(cardboardChute.position);
    }

    void GoToExplosiveChute()
    {
        SetLocation(explosiveChute.position);
    }

    void GoToHeavyChute()
    {
        SetLocation(heavyChute.position);
    }

    void SetLocation(Vector3 position)
    {
        newLocation = position;
        animator.enabled = false;
    }
    #endregion
}
