using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEventsHandler))]
[RequireComponent(typeof(Rigidbody))]
public class WorldOffsetColliderCheck : MonoBehaviour
{
    private Collider col;
    private Rigidbody rb;
    private UnityEventsHandler colliderEvents;
    [SerializeField] private Transform targetTRS;
    [SerializeField] private Vector3 worldOffset = Vector3.zero;


    // Start is called before the first frame update
    void Start()
    {
        col = this.GetComponent<Collider>();
        if (!col.isTrigger) col.isTrigger = true;

        rb = this.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        colliderEvents = this.GetComponent<UnityEventsHandler>();
        
        if (targetTRS == null &&
            this.transform.parent != null)
            targetTRS = this.transform.parent;

        Debug.Assert(this.transform.parent != null || targetTRS != null,
                     string.Format("[{0} || {1}] WorldOffsetColliderCheck component doesn't have a parent transform or doesn't have the 'targetTRS' value assigned.", 
                     this.gameObject.GetInstanceID(), this.transform.name));
        
        if (targetTRS != null)
        {
            this.transform.parent = null;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (targetTRS != null) this.transform.position = targetTRS.position + worldOffset;
        
        if (colliderEvents.ObjectsInTrigger.Count > 0 &&
            colliderEvents.ObjectsInTrigger.Contains(this.gameObject))
            colliderEvents.ObjectsInTrigger.Remove(this.gameObject);
    }
}
