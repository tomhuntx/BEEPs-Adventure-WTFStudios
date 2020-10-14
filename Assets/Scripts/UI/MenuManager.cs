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

	// New Game Yes/No Prompt
	[SerializeField] private GameObject newGamePrompt;

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
		// If has saved progress, prompt
		if (levelProgress > 0)
		{
			newGamePrompt.SetActive(true);
		}
		// Else load the tutorial level
		else
		{
			LoadScene(1);
		}
	}

	public void Override()
	{
		DataSaver.ResetProgress();

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
		levelProgress = 3;
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

		// Maximum sorting order
		oldcanvas.GetComponent<Canvas>().sortingOrder = 10;

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
		//GameObject player = GameObject.FindGameObjectWithTag("Player");
		//if (player)
		//{
		//	// Disable the player
		//	player.GetComponent<Player>().SetEnabled(false);
		//}

		// Pause
		yield return new WaitForSeconds(0.3f);


		// Fade out
		StartCoroutine(FadeOut());
		yield return new WaitForSeconds(1.2f);

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

		//if (player)
		//{
		//	// Enable the player
		//	player.GetComponent<Player>().SetEnabled(true);
		//}

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
