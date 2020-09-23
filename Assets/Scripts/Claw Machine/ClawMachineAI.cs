using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Panda;


//[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PandaBehaviour))]
public class ClawMachineAI : MonoBehaviour
{
    public static ClawMachineAI Instance;

    public float distanceThreshold = 0.3f;

    #region Exposed Variables
    [SerializeField] private float moveSpeed = 5.0f;

    [Header("Animators")]
    [SerializeField] private Animator shaftAnim;
    [SerializeField] private Animator clawAnim;

    [Header("Claw Rails")]
    [SerializeField] private Transform horizontalRail;
    [SerializeField] private Transform verticalRail;

    [Header("References")]
    [SerializeField] private UnityEventsHandler colliderEvents;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private Transform grabbedObjectOffset;

    [Header("Positions")]
    [SerializeField] private Transform boxSource;
    [SerializeField] private Transform cardboardChute;
    [SerializeField] private Transform explosiveChute;
    [SerializeField] private Transform heavyChute;

    [Header("Box Prefabs")]
    [SerializeField] private GameObject[] boxPrefabs;

    [Header("Events")]
    [SerializeField] public UnityEvent onComponentDisable;
    [SerializeField] public UnityEvent onComponentEnable;
    #endregion


    #region Private Variables
    private ClawGrabbable targetObject;
    private ClawGrabbable grabbedObject;
    private PandaBehaviour pandaBehaviour;
    private RaycastHit raycastHit;
    private bool isRaycastHit;
    private string prevTrigAnim;

    private bool hasArrived = false;
    private Vector3 newLocation = Vector3.negativeInfinity;
    private bool isSorting = false;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        pandaBehaviour = this.GetComponent<PandaBehaviour>();
        Instance = this;
        //SetAnimation("Idle");

        //newLocation = boxSource.position;
        //hasArrived = false;
    }

    private void LateUpdate()
    {
        //Only move if there's a set valid location
        if (!hasArrived &&
            NoNewLocation())
        {
            //Move to location
            //this.transform.position = Vector3.Lerp(this.transform.position, newLocation, (moveSpeed / 100) * Time.deltaTime);

            //Negate y value then get distance
            Vector3 v1 = this.transform.position;
            v1.y = 0;

            Vector3 v2 = newLocation;   
            v2.y = 0;

            bool isValidPosition = true;
            //Vector3 dir = VectorFlat3D.FlattenVector(Vector3.Normalize(newLocation - this.transform.position), VectorFlat3D.Axis.y);
            Direction dir = new Direction(v1, v2);
            if (float.IsNaN(dir.localDirection.x) ||
                float.IsNaN(dir.localDirection.y) ||
                float.IsNaN(dir.localDirection.z))
            {
                isValidPosition = false;
            }

            if (isValidPosition)
            {
                this.transform.position += dir.localDirection * moveSpeed * Time.deltaTime;

                float distance = Vector3.Distance(v1, v2);
                if (distance <= distanceThreshold)
                {
                    hasArrived = true;
                    //Vector3 newPos = newLocation;
                    //newPos.y = this.transform.position.y;
                    //this.transform.position = newPos;
                    newLocation = Vector3.negativeInfinity;
                }
            }
        }

        isRaycastHit = DoRaycast();
        UpdateClawRails();
        
        /*
        if (grabbedObject != null &&
            !IsSorting())
        {
            Vector3 v1 = this.transform.position;
            v1.y = 0;

            Vector3 v2 = boxSource.position;
            v2.y = 0;

            float distance = Vector3.Distance(v1, v2);
            if (distance < 0.1f)
            {
                ClawGrabbable newTarget = raycastHit.transform.GetComponent<ClawGrabbable>();
                if (newTarget != null &&
                    newTarget.GetComponent<Box>() != null)
                {
                    targetObject = newTarget;
                }
                else
                {
                    targetObject = null;
                }
            }
        }
        */

        if (grabbedObject != null &&
            grabbedObjectOffset.childCount == 0)
        {
            grabbedObject.gameObject.layer = LayerMask.NameToLayer("Default");
            grabbedObject = null;
        }
    }

    //private void FixedUpdate()
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //        SetAnimation("MoveDown");

    //    if (Input.GetKeyDown(KeyCode.Alpha2))
    //        SetAnimation("Grab");

    //    if (Input.GetKeyDown(KeyCode.Alpha3))
    //        SetAnimation("MoveUp");

    //    if (Input.GetKeyDown(KeyCode.Alpha4))
    //        SetAnimation("Release");

    //    if (Input.GetKeyDown(KeyCode.Alpha0))
    //        SetAnimation("Idle");
    //}

    private void OnEnable()
    {
        onComponentEnable.Invoke();

        //Drop anything if any
        if (grabbedObjectOffset.childCount > 0)
        {
            SetClawAnimation(false);
            SetShaftAnimation(false);
            ClawGrabbable grabbed = grabbedObjectOffset.GetChild(0).GetComponent<ClawGrabbable>();
            grabbed.DetachFromParent();
        }
    }

    private void OnDisable()
    {
        onComponentDisable.Invoke();
    }


    private void UpdateClawRails()
    {
        horizontalRail.position = new Vector3(this.transform.position.x,
                                              horizontalRail.position.y,
                                              horizontalRail.position.z);

        verticalRail.position = new Vector3(verticalRail.position.x,
                                            verticalRail.position.y,
                                            this.transform.position.z);
    }

    private void GoToLocation(Vector3 location)
    {
        newLocation = location;
        hasArrived = false;
    }

    

    private bool DoRaycast()
    {
        Ray ray = new Ray(raycastOrigin.position, -raycastOrigin.up);
        Debug.DrawLine(ray.origin, ray.direction * float.PositiveInfinity, Color.cyan);
        return Physics.Raycast(ray, out raycastHit);
    }

    public void ToggleAI(bool state)
    {
        pandaBehaviour.enabled = state;
        shaftAnim.enabled = state;
        this.enabled = state;
        grabbedObject = null;
        targetObject = null;
        newLocation = Vector3.negativeInfinity;


        //Drop any grabbed object
        //SetClawAnimation(false);
        //if (grabbedObjectOffset.childCount > 0)
        //{
        //    grabbedObject = grabbedObjectOffset.GetChild(0).gameObject.GetComponent<ClawGrabbable>();

        //    if (grabbedObject != null)
        //        DropObject();
        //}

        //SetAnimation("Release");
    }

    //public void SetAnimation(string triggerName)
    //{
    //    print(triggerName);
    //    if (!animator.enabled) 
    //        animator.enabled = true;

    //    animator.ResetTrigger(prevTrigAnim);
    //    animator.SetTrigger(triggerName);
    //    prevTrigAnim = triggerName;

    //    if (triggerName == "Idle")
    //        animator.enabled = false;
    //}

    private void SetShaftAnimation(bool moveDown)
    {
        shaftAnim.SetBool("MoveDown", moveDown);
    }

    private void SetClawAnimation(bool doClamp)
    {
        clawAnim.SetBool("DoClamp", doClamp);
    }



    #region PandaBT Methods
    [Panda.Task]
    private void SpawnRandomBox()
    {
        if (targetObject == null)
        {
            GameObject newInstance = Instantiate(boxPrefabs[Random.Range(0, boxPrefabs.Length)], new Vector3(9999, 9999, 9999), Quaternion.identity);
            targetObject = newInstance.GetComponent<ClawGrabbable>();
            Panda.Task.current.Succeed();
        }
    }

    [Panda.Task]
    private void GrabObject()
    {
        targetObject.transform.parent = null;
        //GrabbableObject.AttachToParent(targetObject.transform, grabbedObjectOffset, true, true);
        targetObject.AttachToParent(grabbedObjectOffset, true, true);
        grabbedObject = targetObject;
        targetObject = null;
        Panda.Task.current.Succeed();
    }

    [Panda.Task]
    private void DropObject()
    {
        //GrabbableObject.DetachFromParent(grabbedObject.transform);
        grabbedObject.DetachFromParent();
        grabbedObject = null;
        Panda.Task.current.Succeed();
    }

    [Panda.Task]
    private bool ObjectInTrigger()
    {
        Panda.Task.current.Succeed();
        foreach (GameObject gameObject in colliderEvents.ObjectsInTrigger)
        {
            Box box = gameObject.GetComponentInChildren<Box>();
            if (box != null)
            {
                targetObject = box.GetComponent<ClawGrabbable>();
                return true;
            }
        }
        return false;
    }

    [Panda.Task]
    private bool IsShaftRetracted()
    {
        return shaftAnim.transform.localPosition.y >= 0;
    }

    [Panda.Task]
    private bool HasObjectGrabbed()
    {
        return grabbedObject != null;
    }

    [Panda.Task]
    private bool HasTargetObject()
    {
        return targetObject != null;
    }

    [Panda.Task]
    private bool HasArrived()
    {
        return hasArrived;
    }

    [Panda.Task]
    private bool TargetObjectMismatch()
    {
        return targetObject.transform != raycastHit.transform &&
               raycastHit.transform.GetComponentInChildren<Box>() == null;
    }



    #region Navigation
    [Panda.Task]
    private bool NoNewLocation()
    {
        return newLocation != Vector3.negativeInfinity ||
               newLocation != Vector3.positiveInfinity;
    }

    private void SetLocation(Vector3 position)
    {
        position = VectorFlat3D.FlattenVector(position, VectorFlat3D.Axis.y);
        position.y = this.transform.position.y;
        newLocation = position;
        //animator.enabled = false;
        hasArrived = false;
        Panda.Task.current.Succeed();
    }

    [Panda.Task]
    private void GoToBoxSource()
    {
        SetLocation(boxSource.position);
        //Panda.Task.current.Succeed();
        isSorting = false;
    }

    [Panda.Task]
    private void GoToCardboardChute()
    {
        SetLocation(cardboardChute.position);
        isSorting = true;
    }

    [Panda.Task]
    private void GoToExplosiveChute()
    {
        SetLocation(explosiveChute.position);
        isSorting = true;
    }

    [Panda.Task]
    private void GoToHeavyChute()
    {
        SetLocation(heavyChute.position);
        isSorting = true;
    }
    #endregion



    [Panda.Task]
    private void EnableDetector()
    {
        colliderEvents.enabled = true;
        Panda.Task.current.Succeed();
    }

    [Panda.Task]
    private void DisableDetector()
    {
        colliderEvents.ObjectsInTrigger.Clear();
        colliderEvents.enabled = false;
        Panda.Task.current.Succeed();
    }



    #region BoxCheckers
    [Panda.Task]
    private bool HasCardboardBox()
    {
        return grabbedObject.GetComponent<Box>().TypeOf == Box.Type.Cardboard;
    }

    [Panda.Task]
    private bool HasExplosiveBox()
    {
        return grabbedObject.GetComponent<Box>().TypeOf == Box.Type.Explosive;
    }

    [Panda.Task]
    private bool HasHeavyBox()
    {
        return grabbedObject.GetComponent<Box>().TypeOf == Box.Type.Heavy;
    }

    [Panda.Task]
    private bool IsSorting()
    {
        return isSorting;
    }
    #endregion



    #region Animation Hooks
    [Panda.Task]
    private void MoveDownAnimation()
    {
        //SetAnimation("MoveDown");
        SetShaftAnimation(true);
        Panda.Task.current.Succeed();
    }

    [Panda.Task]
    private void MoveUpAnimation()
    {
        //SetAnimation("MoveUp");
        SetShaftAnimation(false);
        Panda.Task.current.Succeed();
    }

    [Panda.Task]
    private void DoGrabAnimation()
    {
        //SetAnimation("Grab");
        SetClawAnimation(true);
        Panda.Task.current.Succeed();
    }

    [Panda.Task]
    private void DoReleaseAnimation()
    {
        //SetAnimation("Release");
        SetClawAnimation(false);
        Panda.Task.current.Succeed();
    }

    [Panda.Task]
    private void DisableAnimator()
    {
        //SetAnimation("Idle");
        Panda.Task.current.Succeed();
    }

    [Panda.Task]
    private bool AtBoxSource()
    {
        float distance = VectorFlat3D.GetFlattenedDistance(this.transform.position, boxSource.position, VectorFlat3D.Axis.y);
        return distance <= distanceThreshold;
    }
    #endregion

    #endregion
}
