using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class TaskList : MonoBehaviour
{
	public static TaskList Instance;

	public GameObject[] taskObjects;
	public List<TextMeshProUGUI> taskTexts = new List<TextMeshProUGUI>();
	public List<bool> tasksComplete = new List<bool>();

	public string[] completeTexts;
	private int numTasksDone = 0;
	
	[Tooltip("How fast will the UI will be scaled down during emphasis.")]
	[SerializeField] private float uiTransitionSpeed = 5.0f;

	public int NumTasksDone { get { return numTasksDone; } }


	private void Awake()
	{
		//prevent other instances
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogWarning("Another TaskList instance is already existing, deleting this one...");
			Destroy(this);
		}
	}

	private void Start()
	{
		// Get task objects and text boxes
		for (int i = 0; i < taskObjects.Length; i++)
		{
			taskTexts.Add(taskObjects[i].GetComponentInChildren<TextMeshProUGUI>());
			tasksComplete.Add(false);
			taskObjects[i].GetComponent<Task>().UITransitionSpeed = uiTransitionSpeed;
		}
	}

	/// <summary>
	/// Complete a task and set its text to a crossed-out version
	/// -Crossed-out versions defined publicly in TaskList object
	/// </summary>
	/// <param name="taskNum">Current task in question</param>
	public void CompleteTask(int taskNum)
	{
		if (!tasksComplete[taskNum - 1])
		{
			// Ensure task number correlates to the correct index
			taskNum--;

			// Cross out text
			taskTexts[taskNum].SetText(completeTexts[taskNum]);

			// Record task as complete
			tasksComplete[taskNum] = true;

			//Increase count - used by sound manager
			if (numTasksDone < tasksComplete.Count)
				numTasksDone++;
		}
	}

	/// <summary>
	/// Increase the contribution text counter of a task that requires multiple contributions.
	/// </summary>
	/// <param name="taskNum">Current task in question</param>
	/// <param name="current">Current contributions out of the total</param>
	/// <param name="total">Total contributions required to complete the task</param>
	public void IncreaseContributions(int taskNum, int current, int total)
	{
		// Ensure task number correlates to the correct index
		taskNum--;

		// Create a new text
		string newText;
		if (current > 1)
		{
			newText = taskTexts[taskNum].text.Replace(" (" + (current - 1) + "/" + total + ")", " (" + current + "/" + total + ")");
		}
		else
		{
			newText = taskTexts[taskNum].text + " (" + current + "/" + total + ")";
		}

		// Set the new text
		taskTexts[taskNum].SetText(newText);
	}
}
