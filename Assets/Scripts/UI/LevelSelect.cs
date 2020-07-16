using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelect : MonoBehaviour
{
	// Button objects set in the editor (must be in chrono. order)
	public List<GameObject> buttons = new List<GameObject>();

	private int level = 0;
	private MenuManager mm;

	// Start is called before the first frame update
	void Awake()
    {
		mm = GameObject.FindObjectOfType<MenuManager>();
		level = mm.levelProgress;

		// Level 0 and 1 have the same progress (main menu and tutorial)
		if (level == 0)
		{
			level = 1;
		}

		// Activate buttons according to progress
		for (int i = 0; i < level; i++)
		{
			buttons[i].GetComponent<Button>().interactable = true;
		}
	}
}
