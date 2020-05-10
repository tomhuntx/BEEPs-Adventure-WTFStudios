using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
	//public int totalTasks;

	//public Task[] tasks;

	void Start()
    {
		//tasks = FindObjectsOfType<Task>();
		//totalTasks = tasks.Length;
	}

	// Called by another script - generally as a UnityEvent
    public void ContributeToTask(GameObject task)
	{
		task.GetComponent<Task>().Contribute();
	}
}
