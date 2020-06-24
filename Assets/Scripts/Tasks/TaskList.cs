﻿using System.Collections;
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
    public enum Type { Main, Optional }

    #region Exposed Variables
    [Header("Task Properties")]
    [Tooltip("The type of this task is.")]
    public Type taskType;

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
    public Task (Type thisTaskType, string name, float required, float current)
    {
        //Error check
        if (name == "" ||
            name == string.Empty)
        {
            Debug.LogError(string.Format("[{0}] This task has no name given or is empty, please put a name before playing!", nameof(Task)));
        }

        taskType = thisTaskType;
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
    public Task (Type thisTaskType, string name, float required)
    {
        //Error check
        if (name == "" ||
            name == string.Empty)
        {
            Debug.LogError(string.Format("[{0}] This task has no name given or is empty, please put a name before playing!", nameof(Task)));
        }

        taskType = thisTaskType;
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
    public Task(Type thisTaskType, string name, int required, int current)
    {
        //Error check
        if (name == "" ||
            name == string.Empty)
        {
            Debug.LogError(string.Format("[{0}] This task has no name given or is empty, please put a name before playing!", nameof(Task)));
        }

        taskType = thisTaskType;
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
    public Task(Type thisTaskType, string name, int required)
    {
        //Error check
        if (name == "" ||
            name == string.Empty)
        {
            Debug.LogError(string.Format("[{0}] This task has no name given or is empty, please put a name before playing!", nameof(Task)));
        }

        taskType = thisTaskType;
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

            if (CheckTaskStatus())
            {
                isTaskDone = true;
                onTaskDone.Invoke();
            }
            onTaskContribute.Invoke();
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

    public void ResetProgress()
    {
        isTaskDone = false;
        currentContributions = 0;
    }

    public void DecreaseContribution()
    {
        if (isTaskDone) isTaskDone = false;
        currentContributions = Mathf.Clamp(currentContributions - 1, 0, float.MaxValue);
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
    [SerializeField] public List<Task> tasks = new List<Task>();
    private int numMainTasksDone;
    private int numOptionalTasksDone;
    private int numMainTasks;
    private int numOptionalTasks;
    private List<string> taskNames = new List<string>(); //for name checks

    [Header("Task List Events")]
    public UnityEvent onTaskContribute;
    public UnityEvent onMainTasksDone;
    public UnityEvent onOptionalTasksDone;
    public UnityEvent onAllTasksDone;
    #endregion



    private void Awake()
    {
        Instance = this;
        scriptID = string.Format("([Task List] {0} - Instance: {1})",
                                 this.transform.gameObject.name,
                                 this.transform.gameObject.GetInstanceID());

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
        //Copy over to array to enable editing
        Task[] arrayTasks = tasks.ToArray();

        if (taskNames.Contains(taskName))
        {
            int taskIndex = taskNames.IndexOf(taskName);
            arrayTasks[taskIndex].Contribute(contributionAmount);

            if (arrayTasks[taskIndex].isTaskDone)
            {
                switch (arrayTasks[taskIndex].taskType)
                {
                    case Task.Type.Main:
                        numMainTasksDone++;
                        break;

                    case Task.Type.Optional:
                        numOptionalTasksDone++;
                        break;
                }
            }
        }
        else
        {
            Debug.LogWarning(string.Format("{0} No task found with the name of {1}...", scriptID, taskName));
        }



        if (numMainTasksDone >= numMainTasks)
        {
            onMainTasksDone.Invoke();
        }

        if (numOptionalTasksDone >= numOptionalTasks)
        {
            onOptionalTasksDone.Invoke();
        }

        if (numMainTasksDone >= numMainTasks &&
            numOptionalTasksDone >= numOptionalTasks)
        {
            onAllTasksDone.Invoke();
        }

        //Apply edits
        tasks = arrayTasks.ToList();
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

    public void ResetTaskProgress(string taskName)
    {
        if (TaskExist(taskName))
        {
            int taskIndex = taskNames.IndexOf(taskName);
            Task targetTask = tasks[taskIndex];

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
            targetTask.ResetProgress();
        }
    }

    public void DecreseContributionToTask(string taskName)
    {
        if (TaskExist(taskName))
        {
            int taskIndex = taskNames.IndexOf(taskName);
            tasks[taskIndex].DecreaseContribution();
        }
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// Update entire task names list.
    /// </summary>
    private void UpdateTaskNamesList()
    {
        taskNames.Clear();
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].taskName == "" ||
                    tasks[i].taskName == string.Empty)
            {
                Debug.LogError(string.Format("{0} The task on index {1} has no name, " +
                                             "please put a name before playing again!", scriptID, i));
            }
            else if (taskNames.Contains(tasks[i].taskName))
            {
                Debug.LogError(string.Format("{0} The task on index {1} has a duplicate name, " +
                                             "please replace the name before playing again!", scriptID, i));
            }
            else
            {
                taskNames.Add(tasks[i].taskName);
            }
        }
    }

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
