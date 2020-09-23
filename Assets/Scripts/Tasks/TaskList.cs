using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// An objective that is considered as done if the required
/// number of contributions has met.
/// </summary>
[System.Serializable]
public struct Task : System.IEquatable<Task>
{
    public enum Type { Main, Optional, Ignore }
    public enum ResetType { None, InProgressOnly, OnDoneOnly, Persistent }

    #region Exposed Variables
    [Header("Task Properties")]
    [Tooltip("The type of this task is.")]
    public Type taskType;

    [Tooltip("Which type of reset can be done to this task.")]
    [SerializeField] private ResetType resetType;

    [Tooltip("The name of this task.")]
    public string taskName;

    [Tooltip("The required amount for this task to be considered as done.")]
    public float requiredContributions;    

    [Header("Task Unity Events")]
    public UnityEvent onTaskContribute;
    public UnityEvent onTaskDone;
    public UnityEvent onTaskCreate;
    public UnityEvent onTaskReset;

    public static Task invalidTask = new Task(false);
	#endregion


	#region Accessors
	/// <summary>
	/// The current number of contributions this task had accumulated.
	/// </summary>
	public float currentContributions { get; private set; }

    /// <summary>
    /// If this task is done or not.
    /// </summary>
    public bool isTaskDone { get; set; }
    #endregion


    #region Constructors
    /// <summary>
    /// An objective that is considered as done if the required
    /// number of contributions has met.
    /// </summary>
    /// <param name="thisTaskType">The type of this task.</param>
    /// <param name="name">The name of this task.</param>
    /// <param name="required">The required amount of contributions for this task to be considered as done.</param>
    /// <param name="current">The initial amount of contributions for this task.</param>
    /// <param name="thisResetType">Which type of reset can be done to this task.</param>
    public Task (Type thisTaskType, string name, float required, float current, ResetType thisResetType = ResetType.None)
    {
        //Error check
        if (name == "" ||
            name == string.Empty)
        {
            Debug.LogError(string.Format("[{0}] This task has no name given or is empty, please put a name before playing!", nameof(Task)));
        }

        taskType = thisTaskType;
        resetType = thisResetType;
        taskName = name;
        requiredContributions = required;
        currentContributions = current;
        isTaskDone = currentContributions >= requiredContributions;


        onTaskCreate = new UnityEvent();
        onTaskContribute = new UnityEvent();
        onTaskDone = new UnityEvent();
        onTaskReset = new UnityEvent();

        onTaskCreate.Invoke();
    }

    /// <summary>
    /// An objective that is considered as done if the required
    /// number of contributions has met.
    /// </summary>
    /// <param name="thisTaskType">The type of this task.</param>
    /// <param name="name">The name of this task.</param>
    /// <param name="required">The required amount of contributions for this task to be considered as done.</param>
    /// <param name="thisResetType">Which type of reset can be done to this task.</param>
    public Task (Type thisTaskType, string name, float required, ResetType thisResetType = ResetType.None)
    {
        //Error check
        if (name == "" ||
            name == string.Empty)
        {
            Debug.LogError(string.Format("[{0}] This task has no name given or is empty, please put a name before playing!", nameof(Task)));
        }

        taskType = thisTaskType;
        resetType = thisResetType;
        taskName = name;
        requiredContributions = required;
        currentContributions = 0;
        isTaskDone = currentContributions >= requiredContributions;

        onTaskCreate = new UnityEvent();
        onTaskContribute = new UnityEvent();
        onTaskDone = new UnityEvent();
        onTaskReset = new UnityEvent();

        onTaskCreate.Invoke();
    }

    /// <summary>
    /// An objective that is considered as done if the required
    /// number of contributions has met.
    /// </summary>
    /// <param name="thisTaskType">The type of this task.</param>
    /// <param name="name">The name of this task.</param>
    /// <param name="required">The required amount of contributions for this task to be considered as done.</param>
    /// <param name="current">The initial amount of contributions for this task.</param>
    /// <param name="thisResetType">Which type of reset can be done to this task.</param>
    public Task(Type thisTaskType, string name, int required, int current, ResetType thisResetType = ResetType.None)
    {
        //Error check
        if (name == "" ||
            name == string.Empty)
        {
            Debug.LogError(string.Format("[{0}] This task has no name given or is empty, please put a name before playing!", nameof(Task)));
        }

        taskType = thisTaskType;
        resetType = thisResetType;
        taskName = name;
        requiredContributions = required;
        currentContributions = current;
        isTaskDone = currentContributions >= requiredContributions;


        onTaskCreate = new UnityEvent();
        onTaskContribute = new UnityEvent();
        onTaskDone = new UnityEvent();
        onTaskReset = new UnityEvent();

        onTaskCreate.Invoke();
    }

    /// <summary>
    /// An objective that is considered as done if the required
    /// number of contributions has met.
    /// </summary>
    /// <param name="thisTaskType">The type of this task.</param>
    /// <param name="name">The name of this task.</param>
    /// <param name="required">The required amount of contributions for this task to be considered as done.</param>
    /// <param name="thisResetType">Which type of reset can be done to this task.</param>
    public Task(Type thisTaskType, string name, int required, ResetType thisResetType = ResetType.None)
    {
        //Error check
        if (name == "" ||
            name == string.Empty)
        {
            Debug.LogError(string.Format("[{0}] This task has no name given or is empty, please put a name before playing!", nameof(Task)));
        }

        taskType = thisTaskType;
        resetType = thisResetType;
        taskName = name;
        requiredContributions = required;
        currentContributions = 0;
        isTaskDone = currentContributions >= requiredContributions;

        onTaskCreate = new UnityEvent();
        onTaskContribute = new UnityEvent();
        onTaskDone = new UnityEvent();
        onTaskReset = new UnityEvent();

        onTaskCreate.Invoke();
    }

    /// <summary>
    /// Constructor for invalid data type reference.
    /// </summary>
    /// <param name="isDone">If this invalid type is considered as a finished task or not.</param>
    public Task(bool isDone)
    {
        taskType = Type.Optional;
        resetType = ResetType.Persistent;
        taskName = string.Empty;
        requiredContributions = float.NaN;
        currentContributions = float.NaN;
        isTaskDone = isDone;

        onTaskCreate = new UnityEvent();
        onTaskContribute = new UnityEvent();
        onTaskDone = new UnityEvent();
        onTaskReset = new UnityEvent();
    }
    #endregion


    #region Public Methods
    /// <summary>
    /// Add a number of contributions to this task.
    /// </summary>
    /// <param name="amount">The amount of contributions to be added for this task.</param>
    public void Contribute(float amount)
    {
        if (!isTaskDone)
        {
            currentContributions += amount;
            currentContributions = Mathf.Clamp(currentContributions,
                                               0, requiredContributions);
            onTaskContribute.Invoke();
            
            if (CheckTaskStatus())
            {
                isTaskDone = true;
                onTaskDone.Invoke();

                //Debug.Log(taskName + ": DONE");
            }
        }
    }

    /// <summary>
    /// Add a number of contributions to this task.
    /// </summary>
    /// <param name="amount">The amount of contributions to be added for this task.</param>
    public void Contribute(int amount)
    {
        Contribute((float)amount);
    }

    /// <summary>
    /// Add one contributions to this task.
    /// </summary>
    public void Contribute()
    {
        Contribute(1);
    }

    /// <summary>
    /// Resets this task's current progress.
    /// </summary>
    public void ResetProgress()
    {
        bool canBeReset = false;

        switch(resetType)
        {
            case ResetType.InProgressOnly:
                canBeReset = isTaskDone == false;
                break;

            case ResetType.OnDoneOnly:
                canBeReset = isTaskDone == true;
                break;

            case ResetType.Persistent:
                canBeReset = true;
                break;

            default:
                canBeReset = false;
                break;
        }

        if (canBeReset)
        {
            isTaskDone = false;
            currentContributions = 0;
            onTaskReset.Invoke();

            //Debug.Log(taskName + ": RESET");
        }
    }

    /// <summary>
    /// Decreases this task's current contribution according to the given number.
    /// </summary>
    /// <param name="amount">How much contribution will be decreased from this task.</param>
    public void DecreaseContribution(float amount)
    {
        if (IsEditAllowed())
        {
            currentContributions -= amount;
            currentContributions = Mathf.Clamp(currentContributions,
                                               0, requiredContributions);

            isTaskDone = CheckTaskStatus();
        }

        //Debug.Log(taskName + ": DECREASE");
    }

    /// <summary>
    /// Decreases this task's current contribution according to the given number.
    /// </summary>
    /// <param name="amount">How much contribution will be decreased from this task.</param>
    public void DecreaseContribution(int amount)
    {
        DecreaseContribution((float)amount);
    }

    /// <summary>
    /// Decreases this task's current contribution according by one.
    /// </summary>
    public void DecreaseContribution()
    {
        DecreaseContribution(1);
    }

    public bool IsEditAllowed()
    {
        bool doUpdate = false;
        switch (resetType)
        {
            case ResetType.InProgressOnly:
                doUpdate = !isTaskDone;
                break;

            case ResetType.OnDoneOnly:
                doUpdate = isTaskDone;
                break;

            case ResetType.Persistent:
                doUpdate = true;
                break;
        }
        return doUpdate;
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// Checks if this task is done or not.
    /// </summary>
    /// <returns>Returns true if the current contributions had exceeded or met the required amount.</returns>
    private bool CheckTaskStatus()
    {
        return currentContributions >= requiredContributions;
    }
    #endregion


    #region Overrides
    public override int GetHashCode()
    {
        return taskName.GetHashCode() ^ taskType.GetHashCode();
    }

    public bool Equals(Task otherTask)
    {
        return string.Compare(taskName, otherTask.taskName, true) == 0;
    }

    public override bool Equals(object obj)
    {
        return obj is Task &&
               Equals((Task) obj);
    }

    public static bool operator !=(Task a, Task b)
    {
        return !a.Equals(b);
    }

    public static bool operator ==(Task a, Task b)
    {
        return a.Equals(b);
    }
    #endregion
}


public class TaskList : MonoBehaviour
{
    public static TaskList Instance;
    private string scriptID = "";

	#region Variables
	[SerializeField] private List<Task> tasks = new List<Task>();
    private int numMainTasksDone;
    private int numOptionalTasksDone;
    private int numMainTasks;
    private int numOptionalTasks;
    private List<string> taskNames = new List<string>(); //for name checks
    private bool isMainTasksDone = false;
    private bool isOptionalTasksDone = false;
    private bool isAllTasksDone = false;
	private bool contribute = false;
	private Animator anim;

	[Header("Task List Events")]
    public UnityEvent onTaskContribute;
	public UnityEvent onTaskComplete;
	public UnityEvent onMainTasksDone;
    public UnityEvent onOptionalTasksDone;
    public UnityEvent onAllTasksDone;
    #endregion

    #region Accessors
    public int NumMainTasksDone { get { return numMainTasksDone; } }
    public int NumOptionalTasksDone { get { return numOptionalTasksDone; } }
    public int NumMainTasks { get { return numMainTasks; } }
    public int NumOptionalTasks { get { return numOptionalTasks; } }
    public bool IsMainTasksDone { get { return IsMainTasksDone; } }
    public bool IsOptionalTasksDone { get { return isOptionalTasksDone; } }
    public bool IsAllTasksDone { get { return isAllTasksDone; } }
    #endregion



    private void Start()
    {
        Instance = this;
        
        //Initialize script error text format
        scriptID = string.Format("([Task List] {0} - Instance: {1})",
                                 this.transform.gameObject.name,
                                 this.transform.gameObject.GetInstanceID());

        //Task is empty, throw an error
        if (tasks.Count <= 0)
            Debug.LogError(string.Format("{0} The task list is empty, " +
                                         "please put at least one (1) task for this script to work!", scriptID));

        //Task list initialization
        for (int i = 0; i < tasks.Count; i++)
        {
            //Count main and optional tasks
            switch (tasks[i].taskType)
            {
                case Task.Type.Main:
                    numMainTasks++;
                    break;

                case Task.Type.Optional:
                    numOptionalTasks++;
                    break;
            }
        }

		anim = GetComponent<Animator>();
		if (anim == null)
		{
			anim = GameObject.FindGameObjectWithTag("TaskList").GetComponent<Animator>();
			if (anim == null)
			{
				Debug.LogWarning("This tasklist does not have an Animator. Please fix this. Thank.");
			}
		}

        //Check for name errors
        UpdateTaskNamesList();
    }



    #region Public Methods
    /// <summary>
    /// Add a number of contributions to the given task name.
    /// </summary>
    /// <param name="taskName">The name of the task.</param>
    /// <param name="contributionAmount">The number of contributions to be added to the task.</param>
    public void ContributeToTask(string taskName, float contributionAmount)
    {
        if (taskNames.Contains(taskName))
        {
            int taskIndex = taskNames.IndexOf(taskName);
            Task targetTask = tasks[taskIndex];
            bool isPreviouslyDone = targetTask.isTaskDone;
            targetTask.Contribute(contributionAmount);
            onTaskContribute.Invoke();

            if (targetTask.isTaskDone &&
                !isPreviouslyDone)
            {
				if (targetTask.taskType != Task.Type.Ignore)
				{
					// Trigger any task complete event and animations
					onTaskComplete.Invoke();
					anim.ResetTrigger("onTaskComplete");
					anim.SetTrigger("onTaskComplete");
				}

				switch (targetTask.taskType)
                {
                    case Task.Type.Main:
                        numMainTasksDone++;
                        break;

                    case Task.Type.Optional:
                        numOptionalTasksDone++;
                        break;
                }
            }

            //Apply edits
            tasks[taskIndex] = targetTask;
        }
        else
        {
            Debug.LogWarning(string.Format("{0} No task found with the name of {1}...", scriptID, taskName));
        }



        if (!isMainTasksDone &&
            numMainTasksDone >= numMainTasks)
        {
            onMainTasksDone.Invoke();
            isOptionalTasksDone = true;
        }

        if (!isOptionalTasksDone &&
            numOptionalTasksDone >= numOptionalTasks)
        {
            onOptionalTasksDone.Invoke();
            isOptionalTasksDone = true;
        }

        if (!isAllTasksDone &&
            numMainTasksDone >= numMainTasks &&
            numOptionalTasksDone >= numOptionalTasks)
        {
            onAllTasksDone.Invoke();
            isAllTasksDone = true;
        }        
    }
	
    /// <summary>
    /// Add one contribution to the given task name.
    /// </summary>
    /// <param name="taskName">The name of the task.</param>
    public void ContributeToTask(string taskName)
    {
        ContributeToTask(taskName, 1);
    }

	/// <summary>
	/// Activate a task after given seconds
	/// </summary>
	public IEnumerator ActivateAfter(string taskName, int seconds)
	{
		yield return new WaitForSeconds(seconds);

		ContributeToTask(taskName, 1);
	}


	/// <summary>
	/// Add a new task into the task list.
	/// </summary>
	/// <param name="newTask">The task that's going to be added.</param>
	public void AddTask(Task newTask)
    {
        tasks.Add(newTask);
        UpdateTaskNamesList();
    }

    /// <summary>
    /// Remove an existing task from the task list.
    /// </summary>
    /// <param name="taskName">The name of an existing task.</param>
    public void RemoveTask(string taskName)
    {
        if (taskNames.Contains(taskName))
        {
            tasks.Remove(FindTask(taskName));
            UpdateTaskNamesList();
        }
        else
        {
            Debug.LogWarning(string.Format("{0} No task found with the name of {1}...", scriptID, taskName));
        }
    }

    /// <summary>
    /// Clears the tasks list and the listed task names.
    /// </summary>
    /// <param name="doTriggerComplete">Option to invoke onAllTaskDone unity event if true.</param>
    public void ClearAllTasks(bool doTriggerComplete = false)
    {
        tasks.Clear();
        taskNames.Clear();

        if (doTriggerComplete) onAllTasksDone.Invoke();
    }

    /// <summary>
    /// Finds the task from the task list using its name.
    /// </summary>
    /// <param name="taskName">The name of an existing task in the list.</param>
    /// <returns>Returns a valid task if it exist, otherwise returns a task with invalid values.</returns>
    public Task FindTask(string taskName)
    {
        if (TaskExist(taskName))
        {
            int taskIndex = taskNames.IndexOf(taskName);
            return tasks[taskIndex];
        }
        else
        {
            return Task.invalidTask;
        }
    }

    /// <summary>
    /// Resets the given task's progress.
    /// </summary>
    /// <param name="taskName">The name of the task.</param>
    public void ResetTaskProgress(string taskName)
    {
        //Do this only if the task do exist
        if (TaskExist(taskName))
        {
            //Get task reference
            int taskIndex = taskNames.IndexOf(taskName);
            Task targetTask = tasks[taskIndex];

            //Check for task list's progress
            if (targetTask.isTaskDone)
            {
                switch (targetTask.taskType)
                {
                    case Task.Type.Main:
                        numMainTasksDone--;
                        break;

                    case Task.Type.Optional:
                        numOptionalTasksDone--;
                        break;
                }
            }

            //Apply edits
            targetTask.ResetProgress();
            tasks[taskIndex] = targetTask;
        }
    }

    /// <summary>
    /// Decreases the given task's current contribution.
    /// </summary>
    /// <param name="taskName">The name of the task.</param>
    public void DecreseContributionToTask(string taskName)
    {
        //Do this only if the task do exist
        if (TaskExist(taskName))
        {
            //Get task reference
            int taskIndex = taskNames.IndexOf(taskName);
            Task targetTask = tasks[taskIndex];

            //Skip this if not editable
            if (!targetTask.IsEditAllowed()) return;

            //Modify task value
            targetTask.DecreaseContribution();

            //Check for task list's progress
            if (targetTask.isTaskDone)
            {
                switch (targetTask.taskType)
                {
                    case Task.Type.Main:
                        numMainTasksDone--;
                        break;

                    case Task.Type.Optional:
                        numOptionalTasksDone--;
                        break;
                }
            }

            //Apply edits
            tasks[taskIndex] = targetTask;
        }
    }

    /// <summary>
    /// Sets a task's current contribution to its required contribution forcing it to set as done.
    /// </summary>
    /// <param name="taskName">The name of the task.</param>
    public void FinishTask(string taskName)
    {
        //Do this only if the task do exist
        if (TaskExist(taskName))
        {
            //Get task reference
            int taskIndex = taskNames.IndexOf(taskName);
            Task targetTask = tasks[taskIndex];

            //Max out the current contribution
            //This forces the task to be marked as finished
            targetTask.Contribute(targetTask.requiredContributions);

            //Apply edits
            tasks[taskIndex] = targetTask;
        }
    }
    #endregion

    //public void PrintText(string text)
    //{
    //    print(text);
    //}

    #region Private Methods
    /// <summary>
    /// Update entire task names list.
    /// </summary>
    private void UpdateTaskNamesList()
    {
        //Clear the names list
        taskNames.Clear();

        //Check the name for each task
        for (int i = 0; i < tasks.Count; i++)
        {
            //Case 1: There's no given task name or it's empty
            if (tasks[i].taskName == "" ||
                tasks[i].taskName == string.Empty)
            {
                Debug.LogError(string.Format("{0} The task on index {1} has no name, " +
                                             "please put a name before playing again!", scriptID, i));
            }
            //Case 2: There's a duplicate name
            else if (taskNames.Contains(tasks[i].taskName))
            {
                Debug.LogError(string.Format("{0} The task on index {1} has a duplicate name, " +
                                             "please replace the name before playing again!", scriptID, i));
            }
            //Case 3: The task is unique and it will be added to the task names list
            else
            {
                taskNames.Add(tasks[i].taskName);
            }
        }
    }

	/// <summary>
	/// Checks if the task do exist - displays a warning message if it doesn't.
	/// </summary>
	/// <param name="taskName">The name of the task.</param>
	/// <returns>Returns true if the task exist, otherwise, returns false.</returns>
	private bool TaskExist(string taskName)
    {
        bool doExist = false;
        doExist = taskNames.Contains(taskName);

        if (!doExist)
            Debug.LogWarning(string.Format("{0} No task found with the name of {1}...", scriptID, taskName));

        return doExist;
    }
    #endregion
}
