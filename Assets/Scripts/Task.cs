using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public struct TaskObject
{
	[Header("Task Specifications")]
	[Tooltip("The number of times this task has to be repeated.")]
	public int requirementCount;
	private int currentCount;

	[Tooltip("Tick this to mark this task done.")]
	public bool isDone;

	[Tooltip("Tick this to mark this task as a main task.")]
	public bool isMainTask;


	[Header("Events")]
	public UnityEvent onTaskContribute;
	public UnityEvent onTaskDone;


	/// <summary>
	/// Marks this task as done if it exceeds the required contribution amount, otherwise marks as not done.
	/// </summary>
	private void CheckContribution()
	{
		if (currentCount >= requirementCount)
		{
			isDone = true;
			onTaskDone.Invoke();
		}
		else
		{
			isDone = false;
		}
	}

	/// <summary>
	/// Add a set number of contribution/s.
	/// </summary>
	/// <param name="numContribution">Number of contributions to be added.</param>
	public void AddContribution(int numContribution)
	{
		if (!isDone)
		{
			currentCount += numContribution;
			onTaskContribute.Invoke();
			CheckContribution();
		}
	}

	/// <summary>
	/// Add one contribution.
	/// </summary>
	public void AddContribution()
	{
		AddContribution(1);
	}

	/// <summary>
	/// Subtracts the current contribution by a given number.
	/// </summary>
	/// <param name="num">Number of contributions to be subtracted.</param>
	public void SubtractContribution(int num)
	{
		currentCount -= num;
		CheckContribution();
	}

	/// <summary>
	/// Subtracts the current contribution by 1.
	/// </summary>
	public void SubtractContribution()
	{
		SubtractContribution(1);
	}
}


public class Task : MonoBehaviour
{
	[SerializeField] private TaskObject thisTask;

	private TaskList taskList;

	[Tooltip("The task number in the list (1+).")]
	public int taskNumber;

	[Tooltip("Required contributions to complete the task.")]
	public int requiredContributes = 1;

	[Tooltip("Current total contributions.")]
	public int currentContributes = 0;

	private float uiTransitionSpeed;


	[Header("Events")]
	public UnityEvent onTaskDone;


	public float UITransitionSpeed { set { uiTransitionSpeed = value; } }	
	
	
	private void Start()
	{
		taskList = FindObjectOfType<TaskList>();
	}

	private void Update()
	{
		//scale transition
		if (this.transform.localScale.x < 1)
		{
			this.transform.localScale = Vector3.one;
		}
		else if (this.transform.localScale.x > 1)
		{
			this.transform.localScale = Vector3.Slerp(this.transform.localScale, Vector3.one, uiTransitionSpeed * Time.deltaTime);
		}
	}

	public void Contribute()
	{
		if (currentContributes < requiredContributes)
		{
			this.transform.localScale *= 2; //emphasize UI
			Debug.Log("+1 contribution for task " + taskNumber);
			currentContributes++;

			if (currentContributes >= requiredContributes)
			{
				taskList.CompleteTask(taskNumber);				
				onTaskDone.Invoke();
			}
			else
			{
				taskList.IncreaseContributions(taskNumber, currentContributes, requiredContributes);
			}
		}

		if (!thisTask.isDone)
		{

		}
	}
}
