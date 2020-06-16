using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
	private int levelProgress = 1;

    void Awake()
    {
		Time.timeScale = 1;

		try
		{
			Load();
		}
		catch (Exception ex)
		{
			Debug.Log("Creating new file...");
			Data data = DataSaver.NewData();
		}

		Debug.Log("Player is currently up to level " + (levelProgress - 1));
	}

	public void Load()
	{
		Data data = DataSaver.LoadData();
		levelProgress = data.GetLevel();
		//...
	}

	public void LoadNew()
	{
		Data data = DataSaver.LoadData();
		levelProgress = data.GetLevel();
		//...
	}

	public void NewGame()
	{
		DataSaver.ResetProgress();
		SceneManager.LoadScene(1);
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		Debug.Log("Started new game!");
	}

	public void Play()
	{
		// Load level of current progress - starting with the tutorial
		SceneManager.LoadScene(levelProgress);

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void Settings()
	{

	}

	public void Quit()
	{
		Application.Quit();
	}
}
