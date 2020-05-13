using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
	//public int totalTasks;
	//public Task[] tasks;

	GameObject player;
	public Task enterSpace;

	void Start()
    {
		//tasks = FindObjectsOfType<Task>();
		//totalTasks = tasks.Length;

		player = GameObject.FindGameObjectWithTag("Player");
	}

	void Update()
	{
		// (Prototype) Detect if the player reaches above y 8
		if (player.transform.position.y > 8)
		{
			enterSpace.Contribute();
		}
	}

	// Called by unity events
	public void ContributeToTask(GameObject task)
	{
		task.GetComponent<Task>().Contribute();
	}
}
