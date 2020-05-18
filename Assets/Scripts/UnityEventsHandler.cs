using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventsHandler : MonoBehaviour
{
    #region Exposed Variables
    [Tooltip("Tags of gameobjects that will be ignored. Leave blank if everything will be detected.")]
    [SerializeField] private List<string> ignoreTags = new List<string>();

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
        if (!DoIgnore(collision))
        {
            onCollisionEnter.Invoke();

            if (!objectsOnCollider.Contains(collision.gameObject))
                objectsOnCollider.Add(collision.gameObject);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!DoIgnore(collision)) onCollisionStay.Invoke();
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!DoIgnore(collision)) onCollisionExit.Invoke();

        if (objectsOnCollider.Contains(collision.gameObject)) 
            objectsOnCollider.Remove(collision.gameObject);
    }
    #endregion



    #region Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!DoIgnore(other))
        {
            onTriggerEnter.Invoke();

            if (!objectsInTrigger.Contains(other.gameObject))
                objectsInTrigger.Add(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!DoIgnore(other)) onTriggerStay.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!DoIgnore(other)) onTriggerExit.Invoke();

        if (objectsInTrigger.Contains(other.gameObject))
            objectsInTrigger.Remove(other.gameObject);
    }
    #endregion



    #region Private Methods
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
