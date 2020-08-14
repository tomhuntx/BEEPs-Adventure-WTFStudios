using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
	public int levelProgress = 0;

	// Loading Screen
	[SerializeField] private GameObject loadingScreen;

	// Level Select Menu (Main Menu ONLY)
	[SerializeField] private GameObject levelSelect;

	void Awake()
    {
		Time.timeScale = 1;

		try
		{
			Load();
		}
		catch (Exception)
		{
			Debug.Log("Creating new file...");
			Data data = DataSaver.NewData();
		}
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

		// Load tutorial level
		LoadScene(1);
	}

	public void Play()
	{
		if (levelProgress > 0)
		{
			// Open level select screen
			levelSelect.SetActive(true);
		}
		else
		{
			NewGame();
		}
	}

	public void Settings()
	{

	}

	public void Quit()
	{
		Application.Quit();
	}

	public void LoadScene(int scene)
	{
		StartCoroutine(LoadNewScene(scene));
	}

	IEnumerator LoadNewScene(int scene)
	{
		Instantiate(loadingScreen, FindObjectOfType<Canvas>().transform);
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		// Wait at least 2 seconds for loading (waits 1 second plus time to load the scene)
		// Realtime to not be affected by timescale
		yield return new WaitForSecondsRealtime(1);

		// Load the passed scene
		AsyncOperation async = SceneManager.LoadSceneAsync(scene);

		// Wait until the scene is loaded
		while (!async.isDone)
		{
			yield return null;
		}
	}
}
