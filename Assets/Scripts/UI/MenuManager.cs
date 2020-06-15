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

		Load();

		Debug.Log("Player is currently on level = " + (levelProgress - 1));
	}

	public void Load()
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

	public void QuitGame()
	{
		Application.Quit();
	}
}
