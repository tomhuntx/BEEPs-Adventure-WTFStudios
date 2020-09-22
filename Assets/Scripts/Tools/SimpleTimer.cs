using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleTimer : MonoBehaviour
{
    [SerializeField] private float duration = 3.0f;
    [SerializeField] private DestroyType destroyType = DestroyType.ComponentOnly;
    [SerializeField] private bool playTimerOnStart = false;

    [Tooltip("When ticked, the timer will not be interrupted when time scale is set to 0.")]
    [SerializeField] private bool useRealTime = false;
    public UnityEvent onTimerStart;
    public UnityEvent onTimerEnd;
    private bool hasTimerStarted = false;


    // Start is called before the first frame update
    void Start()
    {
        if (playTimerOnStart)
            StartCoroutine(DoTimer(duration));
    }

    /// <summary>
    /// Start the timer using the given duration.
    /// </summary>
    public void StartTimer()
    {
        StartTimer(duration);
    }

    /// <summary>
    /// Start the timer using a custom duration.
    /// </summary>
    /// <param name="time">How long the timer will last.</param>
    public void StartTimer(float time)
    {
        if (hasTimerStarted) return;
        StartCoroutine(DoTimer(time));
    }

    /// <summary>
    /// Interrupts the timer if it's running.
    /// </summary>
    public void StopTimer()
    {
        StopAllCoroutines();
        hasTimerStarted = false;
        onTimerEnd.Invoke();
    }


    private IEnumerator DoTimer(float time)
    {
        hasTimerStarted = true;
        onTimerStart.Invoke();

        if (useRealTime)
            yield return new WaitForSecondsRealtime(time);
        else
            yield return new WaitForSeconds(time);
        
        
        onTimerEnd.Invoke();

        switch(destroyType)
        {
            case DestroyType.ComponentOnly:
                Destroy(this);
                break;

            case DestroyType.WithGameObject:
                Destroy(this.gameObject);
                break;

            default:
                break;
        }
        hasTimerStarted = false;
    }
}
