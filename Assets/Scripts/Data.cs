using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Data
{
	private int level;
	//...

	public Data(GameManager gm)
	{
		level = gm.thisLevel;
		//...
	}

	public int GetLevel()
	{
		return level;
	}

	// Used to reset or set level
	public void SetLevel(int set)
	{
		level = set;
	}
}
