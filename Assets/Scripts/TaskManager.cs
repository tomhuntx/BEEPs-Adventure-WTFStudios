using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
	//public int totalTasks;
	//public Task[] tasks;

	GameObject player;

	void Start()
    {
		//tasks = FindObjectsOfType<Task>();
		//totalTasks = tasks.Length;

		player = GameObject.FindGameObjectWithTag("Player");
	}

	void Update()
	{
		
	}

	// Called by unity events
	public void ContributeToTask(GameObject task)
	{
		task.GetComponent<Task>().Contribute();
	}
}
