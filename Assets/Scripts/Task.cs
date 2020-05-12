using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Task : MonoBehaviour
{
	private TaskList taskList;

	[Tooltip("The task number in the list (1+).")]
	public int taskNumber;

	[Tooltip("Required contributions to complete the task.")]
	public int requiredContributes = 1;

	[Tooltip("Current total contributions.")]
	public int currentContributes = 0;

	private void Start()
	{
		taskList = FindObjectOfType<TaskList>();
	}

	public void Contribute()
	{
		if (currentContributes < requiredContributes)
		{
			Debug.Log("+1 contribution for task " + taskNumber);
			currentContributes++;

			if (currentContributes >= requiredContributes)
			{
				taskList.CompleteTask(taskNumber);
			}
			else
			{
				taskList.IncreaseContributions(taskNumber, currentContributes, requiredContributes);
			}
		}
	}
}
