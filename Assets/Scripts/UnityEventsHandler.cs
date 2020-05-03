using UnityEngine;
using UnityEngine.Events;

public class UnityEventsHandler : MonoBehaviour
{
    public UnityEvent onCollisionEnter;
    public UnityEvent onCollisionStay;
    public UnityEvent onCollisionExit;

    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerStay;
    public UnityEvent onTriggerExit;

    private void OnCollisionEnter(Collision collision)
    {
        onCollisionEnter.Invoke();
    }

    private void OnCollisionStay(Collision collision)
    {
        onCollisionStay.Invoke();
    }

    private void OnCollisionExit(Collision collision)
    {
        onCollisionExit.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnter.Invoke();
    }

    private void OnTriggerStay(Collider other)
    {
        onTriggerStay.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        onTriggerExit.Invoke();
    }
}
