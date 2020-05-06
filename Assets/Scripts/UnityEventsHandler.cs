using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventsHandler : MonoBehaviour
{
    [Tooltip("Tags of gameobjects that will be ignored. Leave blank if everything will be detected.")]
    [SerializeField] private List<string> ignoreTags = new List<string>();

    [Header("Events")]
    public UnityEvent onCollisionEnter;
    public UnityEvent onCollisionStay;
    public UnityEvent onCollisionExit;

    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerStay;
    public UnityEvent onTriggerExit;

    private void OnCollisionEnter(Collision collision)
    {
        if (!DoIgnore(collision)) onCollisionEnter.Invoke();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!DoIgnore(collision)) onCollisionStay.Invoke();
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!DoIgnore(collision)) onCollisionExit.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!DoIgnore(other)) onTriggerEnter.Invoke();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!DoIgnore(other)) onTriggerStay.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!DoIgnore(other)) onTriggerExit.Invoke();
    }


    private bool DoIgnore(Collision col)
    {
        if (ignoreTags.Count == 0) return false;

        if (ignoreTags.Contains(col.transform.tag))
            return true;
        else
            return false;
    }

    private bool DoIgnore(Collider col)
    {
        if (ignoreTags.Count == 0) return false;

        if (ignoreTags.Contains(col.tag))
            return true;
        else
            return false;
    }
}
