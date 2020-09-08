using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Data
{
	private int level;
	private int feedback;
	private long feedbackTime;
	//...

	// Set data to that of the current level
	public Data(GameManager gm)
	{
		if (gm.thisLevel > level)
		{
			level = gm.thisLevel;
		}

		feedback = gm.feedbackCount;
		feedbackTime = gm.feedbackTimeBinary;
		//...
	}

	// Set data to defaults (for new game)
	public Data()
	{
		level = 1;
		//...
	}

	public int GetLevel()
	{
		return level;
	}

	public void SetLevel(int set)
	{
		level = set;
	}

	public int GetFeedback()
	{
		return feedback;
	}

	public void AddFeedback()
	{
		feedback++;
	}

	public void SetFeedbackTime(long time)
	{
		feedbackTime = time;
	}

	public long GetFeedbackTime()
	{
		return feedbackTime;
	}

	public void ResetFeedback()
	{
		feedback = 0;
		feedbackTime = 0;
	}
}
