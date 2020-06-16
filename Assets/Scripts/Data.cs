using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Data
{
	private int level;
	//...

	// Set data to that of the current level
	public Data(GameManager gm)
	{
		level = gm.thisLevel;
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
}
