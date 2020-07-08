using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// Responsible for handling UI effects for tasks.
/// </summary>
public class TaskUGUI : MonoBehaviour
{
    #region Variables
    [Tooltip("The affected TMP text component")]
    [SerializeField] private TextMeshProUGUI targetText;

    [Tooltip("The initial size of the target text's transform upon finishing the task.")]
    [SerializeField] private float scaleSizeOnFinish = 2.0f;

    [Tooltip("How fast the UI will ease in from its scaled up size.")]
    [SerializeField] private float scaleTransitionSpeed = 5.0f;
    
    public UnityEvent onTransitionPlay;

    private bool isTransitionDone = false;
    private bool isFinishedTriggered = false;
    #endregion



    // Start is called before the first frame update
    void Start()
    {
        //Assume the transform attached to this has
        //the required component.
        if (targetText == null)
            targetText = this.GetComponent<TextMeshProUGUI>();

        //Check for the 2nd time
        //if the component do exist
        if (targetText == null)
        {
            Debug.LogError("There is no Text Mesh Pro Text assigned to this script, " +
                           "please make sure the transform attached to this has one or " +
                           "the target text variable is asssigned manually before playing!");
        }
        //Component do exist, proceed to intialization
        else
        {
            //Throw error if not task with the given name is found
            if (GetAssignedTask() == Task.invalidTask)
                Debug.LogError(string.Format("No task found from the task list that has the name of {0}", targetText.text));

            //Add the scale up event when the task if fulfilled
            TaskList.Instance.FindTask(targetText.text).onTaskDone.AddListener(ScaleUpTransform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Transition works
        if (!isTransitionDone &&
            GetAssignedTask().isTaskDone)
        {
            isFinishedTriggered = true;
            targetText.fontStyle = FontStyles.Strikethrough;

            //Do scaling stuff here;
            if (targetText.transform.localScale.x >= 1)
            {
                targetText.transform.localScale = Vector3.Slerp(targetText.transform.localScale,
                                                               Vector3.one,
                                                               scaleTransitionSpeed * Time.deltaTime);

                //Stops the scaling after transition
                if (targetText.transform.localScale.x == 1)
                {
                    isTransitionDone = true;                    
                }
            }
        }
    }



    #region Methods
    private void ScaleUpTransform()
    {
        if (!isFinishedTriggered)
        {
            targetText.transform.localScale *= scaleSizeOnFinish;
            onTransitionPlay.Invoke();
        }
    }

    private Task GetAssignedTask()
    {
        return TaskList.Instance.FindTask(targetText.text);
    }
    #endregion
}
