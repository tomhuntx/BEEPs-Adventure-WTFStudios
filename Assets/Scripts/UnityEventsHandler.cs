using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventsHandler : TagFilterer
{
    #region Exposed Variables
    [Header("Events")]
    public UnityEvent onCollisionEnter;
    public UnityEvent onCollisionStay;
    public UnityEvent onCollisionExit;

    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerStay;
    public UnityEvent onTriggerExit;
    #endregion


    #region Hidden Variables
    private List<GameObject> objectsOnCollider = new List<GameObject>();
    private List<GameObject> objectsInTrigger = new List<GameObject>();
    #endregion


    #region Accessors
    public List<GameObject> ObjectsOnCollder { get { return objectsOnCollider; } }
    public List<GameObject> ObjectsInTrigger { get { return objectsInTrigger; } }
    #endregion



    private void Start()
    {
        StartCoroutine("UpdateLists");
    }



    #region Collider
    private void OnCollisionEnter(Collision collision)
    {
        if (!DoIgnore(collision.transform.tag))
        {
            onCollisionEnter.Invoke();

            if (!objectsOnCollider.Contains(collision.gameObject))
                objectsOnCollider.Add(collision.gameObject);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!DoIgnore(collision.transform.tag)) onCollisionStay.Invoke();
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!DoIgnore(collision.transform.tag)) onCollisionExit.Invoke();

        if (objectsOnCollider.Contains(collision.gameObject)) 
            objectsOnCollider.Remove(collision.gameObject);
    }
    #endregion



    #region Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!DoIgnore(other.tag))
        {
            onTriggerEnter.Invoke();

            if (!objectsInTrigger.Contains(other.gameObject))
                objectsInTrigger.Add(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!DoIgnore(other.tag)) onTriggerStay.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!DoIgnore(other.tag)) onTriggerExit.Invoke();

        if (objectsInTrigger.Contains(other.gameObject))
            objectsInTrigger.Remove(other.gameObject);
    }
    #endregion



    #region Private Methods
    /// <summary>
    /// Update lists to prevent null reference
    /// </summary>
    private IEnumerator UpdateLists()
    {
        while (true)
        {            
            for (int i = 0; i < objectsOnCollider.Count; i++)
            {
                if (objectsOnCollider[i] == null)
                {
                    objectsOnCollider.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < objectsInTrigger.Count; i++)
            {
                if (objectsInTrigger[i] == null)
                {
                    objectsInTrigger.RemoveAt(i);
                    i--;
                }
            }

            //repeat every 3 seconds
            yield return new WaitForSeconds(3);
        }
    }
    #endregion
}
