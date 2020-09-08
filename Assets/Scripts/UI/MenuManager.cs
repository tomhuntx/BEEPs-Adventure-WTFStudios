using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
	public int levelProgress = 0;
	private bool finishing = false;

	// Loading Screen
	[SerializeField] private GameObject loadingScreen;
	private LoadingScreen ls;
	private GameObject loadingObject;
	private GameObject oldcanvas;
	private bool loading = false;

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

	private void Start()
	{
		UnlockAllLevels();
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
		// Open level select screen
		levelSelect.SetActive(true);
	}

	// Unlock all levels (cheat)
	public void UnlockAllLevels()
	{
		GameManager gm = new GameManager();
		gm.thisLevel = 3;
		DataSaver.SaveProgress(gm);
	}

	public void Quit()
	{
		Application.Quit();
	}

	public void LoadScene(int scene)
	{
		if (!loading)
		{
			StartCoroutine(LoadNewScene(scene));
		}
		loading = true;
	}

	IEnumerator LoadNewScene(int scene)
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		loadingObject = Instantiate(loadingScreen, FindObjectOfType<Canvas>().transform);
		loadingObject.transform.SetAsLastSibling();
		ls = loadingObject.GetComponent<LoadingScreen>();
		oldcanvas = loadingObject.transform.parent.gameObject;

		// Activate the loading screen
		loadingObject.SetActive(true);

		StartCoroutine(FadeIn());

		// Wait before loading scene
		yield return new WaitForSecondsRealtime(2f);

		// Load the passed scene
		AsyncOperation async = SceneManager.LoadSceneAsync(scene);

		async.allowSceneActivation = true;

		DontDestroyOnLoad(oldcanvas);
		DontDestroyOnLoad(this.gameObject);


		// Wait until the scene is fully loaded
		while (!async.isDone)
		{
			// Check if the load has almost finished
			if (async.progress >= 0.9f && !finishing)
			{
				StartCoroutine(PauseAfterLoad(async));
				finishing = true;
			}

			yield return null;
		}
	}

	IEnumerator PauseAfterLoad(AsyncOperation async)
	{
		// Pause
		yield return new WaitForSeconds(0.3f);

		// Fade out
		StartCoroutine(FadeOut());
		yield return new WaitForSeconds(0.8f);

		SceneManager.MoveGameObjectToScene(oldcanvas, SceneManager.GetActiveScene());
		SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());

		loadingObject.transform.SetParent(FindObjectOfType<Canvas>().transform);
		loadingObject.transform.SetAsLastSibling();

		yield return new WaitForSeconds(0.2f);

		if (oldcanvas)
		{
			Destroy(oldcanvas);
		}

		if (loadingObject)
		{
			Destroy(loadingObject);
			loadingObject = null;
			ls = null;
		}

		// Remove this
		Destroy(gameObject);
	}

	IEnumerator FadeIn()
	{
		ls.SetFade(0);
		float time = 0;
		while (time < 1)
		{
			ls.SetFade(time);
			time += 1f * Time.unscaledDeltaTime;

			yield return null;
		}
		ls.SetFade(1);
	}

	IEnumerator FadeOut()
	{
		ls.SetFade(1);
		float time = 1;
		while (time > 0)
		{
			ls.SetFade(time);
			time -= 1f * Time.unscaledDeltaTime;

			yield return null;
		}
		ls.SetFade(0);
	}

}
