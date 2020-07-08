using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(UnityEventsHandler))]
public class BoxDragSFX : MonoBehaviour
{
    private AudioSource source;
    private UnityEventsHandler colliderEvents;
    private Vector3 previousPos;    
    private bool isMoving = false;
    private bool isGrabbed = false;

    [Tooltip("How much time before checking for position change.")]
    [SerializeField] private float moveDeadzone = 0.1f;
    [SerializeField] private GameObject boxPlaceSFXPrefab;


    // Start is called before the first frame update
    void Awake()
    {
        source = this.GetComponent<AudioSource>();
        colliderEvents = this.GetComponent<UnityEventsHandler>();
        StartCoroutine("CheckPos");
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrabbed)
        {
            isGrabbed = false;
            previousPos = this.transform.position;
            if (boxPlaceSFXPrefab != null) 
                Instantiate(boxPlaceSFXPrefab, this.transform.position, this.transform.rotation);
        }
        else
        {
            //If it has contact and it's moving, play sfx
            if (colliderEvents.ObjectsOnCollder.Count > 0)
            {
                if (isMoving)
                {
                    if (!source.isPlaying) source.Play();
                }
                else
                {
                    if (source.isPlaying) source.Stop();
                }

            }
            //If there's no contact, stop sfx
            else
            {
                if (source.isPlaying) source.Stop();
            }
        }
    }

    private IEnumerator CheckPos()
    {
        while(true)
        {
            if (previousPos != this.transform.position)
            {
                previousPos = this.transform.position;
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }
            yield return new WaitForSeconds(moveDeadzone);            
        }
    }

    public void ToggleThis(bool enable)
    {
        if (enable)
        {
            this.enabled = true;
        }
        else
        {
            isGrabbed = true;
            if (source.isPlaying) source.Stop();
            colliderEvents.ObjectsInTrigger.Clear();
            colliderEvents.ObjectsOnCollder.Clear();
            this.enabled = false;
        }
    }
}
