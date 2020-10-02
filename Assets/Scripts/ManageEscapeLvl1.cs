using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ManageEscapeLvl1 : MonoBehaviour
{
	private bool OptionalTasksComplete;

	private bool mainDone = false;
	private bool optionalDone = false;

	public UnityEvent onMainComplete;
	public UnityEvent onMainAndOptionalComplete;

    // Update is called once per frame
    void Update()
    {
		if (mainDone && OptionalTasksComplete && !optionalDone)
		{
			onMainAndOptionalComplete.Invoke();
			optionalDone = true;
		}
	}

	public void CompleteMain()
	{
		if (!mainDone)
		{
			onMainComplete.Invoke();
			mainDone = true;
		}
	}

	public void CompleteOptional()
	{
		OptionalTasksComplete = true;
	}
}
