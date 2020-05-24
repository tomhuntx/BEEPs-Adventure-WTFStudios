using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GraphicFader : MonoBehaviour
{
    [System.Serializable]
    private struct FadeSequence
    {
        public enum SequenceType { FadeIn, FadeOut, Flash }
        public SequenceType sequenceType;

        [Tooltip("How fast this transition is.")]
        public float fadeRate;

        [Tooltip("The duration after doing this sequence")]
        public float duration;

        [Range(0.0f, 1.0f)] public float minOpacity;
        [Range(0.0f, 1.0f)] public float maxOpacity;

        public UnityEvent onSequenceEntry;
    }


    private enum DestroyType { Disabled, ComponentOnly, WithGameObject }

    #region Exposed Variables
    [SerializeField] private Graphic targetGraphic;
    [Space]
    [Space]
    [SerializeField] private DestroyType destroyType = DestroyType.Disabled;
    [SerializeField] private float sequenceStartDelay = 0.0f;
    [SerializeField] private bool ignoreDelayOnLoop = false;
    [SerializeField] private bool loopSequence = false;
    [SerializeField] private FadeSequence[] sequence;

    [Header("Events")]
    public UnityEvent onSequenceDone;
    public UnityEvent onLoopSequence;
    public UnityEvent onComponentDestroy;
    public UnityEvent onGameObjectDestroy;
    #endregion


    #region Hidden Variables
    private Color graphicColor;
    private FadeSequence.SequenceType fadeType;
    private bool isCurrentSequenceDone = true;
    private bool isFlashing = false;
    private bool isFadingIn = false;
    private bool isFadingOut = false;
    private bool isSequenceFinished = false;
    private float currentTimer;
    private float delayTimer;
    private int currentIndex = 0;
    #endregion

    public bool LoopSequence { get { return loopSequence; } set { loopSequence = value; } }



    private void Start()
    {
        if (targetGraphic == null)
            targetGraphic = this.GetComponent<Graphic>();
        else
            graphicColor = targetGraphic.color;

        fadeType = sequence[currentIndex].sequenceType;
        delayTimer = sequenceStartDelay + Time.time;
    }

    private void Update()
    {
        if (delayTimer < Time.time)
        {
            //If reached end of sequence
            if (currentIndex >= sequence.Length)
            {
                //Restart sequence
                if (loopSequence)
                {
                    if (!ignoreDelayOnLoop)
                        delayTimer = sequenceStartDelay + Time.time;

                    currentIndex = 0;
                    isSequenceFinished = false;
                    onLoopSequence.Invoke();
                }
                else
                {
                    if (!isSequenceFinished)
                    {
                        isSequenceFinished = true;
                        onSequenceDone.Invoke();
                    }


                    //Do necessary destroy if indicated
                    switch (destroyType)
                    {
                        case DestroyType.ComponentOnly:
                            onComponentDestroy.Invoke();
                            Destroy(this);
                            break;

                        case DestroyType.WithGameObject:
                            onGameObjectDestroy.Invoke();
                            Destroy(this.gameObject);
                            break;
                    }
                }
            }
            //If sequence is not yet done
            else
            {
                //If the current sequence is done, move to the next one
                if (isCurrentSequenceDone)
                {
                    isCurrentSequenceDone = false;
                    fadeType = sequence[currentIndex].sequenceType;
                    sequence[currentIndex].onSequenceEntry.Invoke();

                    //Check for errors and swap if necessary
                    if (sequence[currentIndex].minOpacity > sequence[currentIndex].maxOpacity)
                    {
                        Debug.LogWarning(string.Format("Sequence number {0}'s min opacity > max opacity, swapping values...", currentIndex));
                        float newMax = sequence[currentIndex].minOpacity;
                        sequence[currentIndex].minOpacity = sequence[currentIndex].maxOpacity;
                        sequence[currentIndex].maxOpacity = newMax;
                    }

                    //Properly visualize change
                    switch (fadeType)
                    {
                        case FadeSequence.SequenceType.FadeIn:
                            graphicColor.a = sequence[currentIndex].minOpacity;
                            break;

                        case FadeSequence.SequenceType.FadeOut:
                            graphicColor.a = sequence[currentIndex].maxOpacity;
                            break;
                    }
                }
                //Do current sequence and check if it's done
                else
                {
                    switch (fadeType)
                    {
                        case FadeSequence.SequenceType.FadeIn:
                            isCurrentSequenceDone =
                                FadeIn(sequence[currentIndex].minOpacity,
                                       sequence[currentIndex].maxOpacity,
                                       sequence[currentIndex].fadeRate,
                                       sequence[currentIndex].duration);
                            break;

                        case FadeSequence.SequenceType.FadeOut:
                            isCurrentSequenceDone =
                                FadeOut(sequence[currentIndex].minOpacity,
                                        sequence[currentIndex].maxOpacity,
                                        sequence[currentIndex].fadeRate,
                                        sequence[currentIndex].duration);
                            break;

                        case FadeSequence.SequenceType.Flash:
                            isCurrentSequenceDone =
                                Flash(sequence[currentIndex].minOpacity,
                                      sequence[currentIndex].maxOpacity,
                                      sequence[currentIndex].fadeRate,
                                      sequence[currentIndex].duration);
                            break;
                    }
                }
            }

            //Assign alpha value to targeted graphic
            targetGraphic.color = new Color(targetGraphic.color.r,
                                             targetGraphic.color.g,
                                             targetGraphic.color.b,
                                             graphicColor.a);
        }
    }



    #region Private Methods
    private bool FadeIn(float minOpacity, float maxOpacity, float fadeRate, float duration)
    {
        //print("FADE IN");
        if (graphicColor.a >= maxOpacity)
        {
            if (isFadingIn)
            {
                currentTimer = duration + Time.time;
                isFadingIn = false;
                return false;
            }
            else
            {
                if (currentTimer < Time.time)
                {
                    currentIndex++;
                    return true;
                }
            }
            return false;

        }
        else
        {
            if (!isFadingIn)
            {
                isFadingIn = true;
                graphicColor.a = minOpacity;
            }
            else
            {
                graphicColor.a += fadeRate * Time.deltaTime;
                graphicColor.a = Mathf.Clamp(graphicColor.a, minOpacity, maxOpacity);
                currentTimer = Time.time;
            }

            return false;
        }
    }

    private bool FadeOut(float minOpacity, float maxOpacity, float fadeRate, float duration)
    {
        //print("FADE OUT");
        if (graphicColor.a <= minOpacity)
        {
            if (isFadingOut)
            {
                currentTimer = duration + Time.time;
                isFadingOut = false;
                return false;
            }
            else
            {
                if (currentTimer < Time.time)
                {
                    currentIndex++;
                    return true;
                }
            }
            return false;
        }
        else
        {
            if (!isFadingOut)
            {
                isFadingOut = true;
                graphicColor.a = maxOpacity;
            }
            else
            {
                graphicColor.a -= fadeRate * Time.deltaTime;
                graphicColor.a = Mathf.Clamp(graphicColor.a, minOpacity, maxOpacity);
                currentTimer = Time.time;
            }

            return false;
        }
    }

    private bool Flash(float minOpacity, float maxOpacity, float fadeRate, float duration)
    {
        //print("FLASHING");
        if (!isFlashing)
        {
            isFlashing = true;
            currentTimer = duration + Time.time;
            return false;
        }
        else
        {
            if (isFadingIn)
            {
                graphicColor.a += fadeRate * Time.deltaTime;
                isFadingIn = graphicColor.a <= maxOpacity;
            }
            else
            {
                graphicColor.a -= fadeRate * Time.deltaTime;
                isFadingIn = graphicColor.a <= minOpacity;
            }
            graphicColor.a = Mathf.Clamp(graphicColor.a, minOpacity, maxOpacity);

            if (currentTimer < Time.time)
            {
                isFlashing = false;
                isFadingIn = false;
                currentIndex++;
                return true;
            }
            return false;
        }
    }
    #endregion
}
