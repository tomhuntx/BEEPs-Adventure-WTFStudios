using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DestroyField : TagFilterer
{
    private UnityEventsHandler colliderEvents;
    private Rigidbody rb;
    
    [Header("Destroy Field Options")]
    [Tooltip("If an object has the DestructibleObject script attached to it, it will be forced to destroy without invoking the OnObjectDestroy event.")]
    [SerializeField] private bool forceDestroy = false;

    void Start()
    {
        if (this.GetComponent<Collider>() == null)
        {
            Debug.LogError(string.Format("There is no collider attached to {0}, please attach any collider to make this component to work!", this.transform));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!DoIgnore(other.tag))
        {
            if (!forceDestroy && 
                other.transform.GetComponent<DestructibleObject>() != null)
            {
                other.transform.GetComponent<DestructibleObject>().onObjectDestroy.Invoke();
            }
            Destroy(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!DoIgnore(collision.transform.tag))
        {
            if (!forceDestroy && 
                collision.transform.GetComponent<DestructibleObject>() != null)
            {
                collision.transform.GetComponent<DestructibleObject>().onObjectDestroy.Invoke();
            }
            Destroy(collision.gameObject); 
        }
    }
}
