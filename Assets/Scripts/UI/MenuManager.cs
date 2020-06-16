﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
	private int levelProgress = 1;

	// Loading Screen
	[SerializeField] private GameObject loadingScreen;

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

		// Load tutorial level
		LoadScene(1);

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		Debug.Log("Started new game!");
	}

	public void Play()
	{
		// Load level of current progress 
		LoadScene(levelProgress);

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

	public void LoadScene(int scene)
	{
		StartCoroutine(LoadNewScene(scene));
	}

	IEnumerator LoadNewScene(int scene)
	{
		Instantiate(loadingScreen, FindObjectOfType<Canvas>().transform);

		// Wait at least 2 seconds for loading (waits 2 seconds plus time to load the scene)
		// Realtime to not be affected by timescale
		yield return new WaitForSecondsRealtime(2);

		// Load the passed scene
		AsyncOperation async = SceneManager.LoadSceneAsync(scene);

		// Wait until the scene is loaded
		while (!async.isDone)
		{
			yield return null;
		}
	}
}
