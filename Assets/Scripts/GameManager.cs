using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
	public GameObject controls;

    public const float MIN_CAM_FOV = 30.0f;
    public const float MAX_CAM_FOV = 120;

    [SerializeField] private GameObject pauseMenu;

    [Header("FPS Management")]
    public int targetFPS = 60;
    public bool displayFPS = false;
    public bool capFPS = true;
    [SerializeField] private TextMeshProUGUI FPSDisplay;
	private bool locked = false;

    [Header("Object Interaction Settings")]
    [SerializeField] private Material highlighterMaterial;
    [SerializeField] private Color normalHighlightColor;
    [SerializeField] private Color invalidHighlightColor;

	[Header("Feedback Components")]
	public GameObject feedback;
	public GameObject popup;
	private MenuManager mm;
	private bool feedbackAllowed;
	private int feedbackDelayMins = 10;
	private double difference;

	[Header("Save Data")]
	public int thisLevel;
	public int feedbackCount;
	public long feedbackTimeBinary;

	//
	bool moveControls = false;
	RectTransform rect;
	//

	private List<AudioSource> pausedAudioSources = new List<AudioSource>();

	private SettingsMenu settingsMenuComponent;
	private WindowManager windowManagerComponent;
	public SettingsMenu SettingsMenuComponent { get { return settingsMenuComponent; } }
	public WindowManager WindowManagerComponent { get { return windowManagerComponent; } }



	private void Awake()
    {
		settingsMenuComponent = Resources.FindObjectsOfTypeAll<SettingsMenu>()[0];
		windowManagerComponent = Resources.FindObjectsOfTypeAll<WindowManager>()[0];

		settingsMenuComponent.LoadPrefsData();

		Instance = this;
		mm = FindObjectOfType<MenuManager>();

		//Highlighter setup
		InteractableObject.highlighterMaterial = highlighterMaterial;
        InteractableObject.normalHighlightColor = normalHighlightColor;
        InteractableObject.invalidHighlightColor = invalidHighlightColor;
	}

    // Update is called once per frame
    void Update()
    {
        if (FPSDisplay != null)
        {
            if (displayFPS && 
                Time.timeScale != 0)
            {
                if (!FPSDisplay.gameObject.activeSelf)
                {
                    FPSDisplay.gameObject.SetActive(true);
                    StartCoroutine("CalculateFPS");
                }
            }

            if (FPSDisplay.gameObject.activeSelf != displayFPS)
                FPSDisplay.gameObject.SetActive(displayFPS);
        }

		// WEBGL Fix - Only lock cursor clicked when it's visible & not paused
		//if (Input.GetMouseButtonDown(0) && Cursor.visible && !pauseMenu.activeSelf)
		//{
		//	Cursor.visible = false;
		//	Cursor.lockState = CursorLockMode.Locked;
		//	locked = true;
		//}

		//Store controls
		if (Input.GetKeyDown(KeyCode.Return))
		{
			rect = controls.GetComponent<RectTransform>();
			moveControls = true;
		}

		if (moveControls)
		{
			if (rect.localPosition.y > -400f)
			{
				rect.localPosition += 600 * Vector3.down * Time.deltaTime;
			}
			else if (controls.activeSelf)
			{
				controls.SetActive(false);
			}
		}

        //Simple pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu != null)
            {
				//Unpause
                if (pauseMenu.activeSelf)
                {
					ResumeAllAudio();
                    Time.timeScale = 1;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    pauseMenu.SetActive(false);
					//settingsMenuComponent.ToggleMute(false);
                }
				//Pause
                else
                {
                    Time.timeScale = 0;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    pauseMenu.SetActive(true);
					//settingsMenuComponent.ToggleMute(true);
					PauseAllAudio();
				}
            }
            else
            {
                switch(Time.timeScale)
                {
                    case 0:
                        Time.timeScale = 1;
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                        break;

                    case 1:
                        Time.timeScale = 0;
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                        break;
                }
            }
        }

        if (capFPS)
        {
            if (Application.targetFrameRate != targetFPS)
            {
                Application.targetFrameRate = targetFPS;
            }
        }
        else
        {
			Application.targetFrameRate = -1;
        }
    }




    private IEnumerator CalculateFPS()
    {
        while(true)
        {
            float fps = (1 / Time.deltaTime) * Time.timeScale;
            FPSDisplay.text = string.Format("FPS: {0:0.00}", fps);
            yield return new WaitForSeconds(0.5f);
        }
    }

	private void PauseAllAudio()
    {
		pausedAudioSources = FindObjectsOfType<AudioSource>().ToList();
		//if (pausedAudioSources.Count == 0) return;

		foreach (AudioSource source in pausedAudioSources)
        {
			if (source != null) source.mute = true;
        }
	}

	private void ResumeAllAudio()
    {
		if (pausedAudioSources.Count == 0) return;

		foreach (AudioSource source in pausedAudioSources)
        {
			if (source != null) source.mute = false;
        }
		pausedAudioSources.Clear();
	}

	private void StopAllAudio()
    {
		AudioSource[] sources = FindObjectsOfType<AudioSource>();

		foreach (AudioSource source in pausedAudioSources)
		{
			source.Stop();
		}
	}




	// Feedback
	public void Feedback()
	{
		// Show feedback popup
		if (feedbackAllowed)
		{
			feedback.SetActive(true);
		}
		// Check if 10 mins has passed
		else
		{
			if (Waited())
			{
				feedbackAllowed = true;
				ResetFeedback();
			}
			else
			{
				double num = Math.Round(feedbackDelayMins - difference, 1);
				string errorText = "You must wait " + num + " minutes to provide more feedback.";

				if (popup)
				{
					GameObject error = Instantiate(popup, popup.transform.position, popup.transform.rotation) as GameObject;
					error.transform.SetParent(pauseMenu.transform, false);
					error.GetComponentInChildren<TMP_Text>().SetText(errorText);
				}
				else
				{
					Debug.LogWarning("Please attach a popup to the GameManager!");
				}
			}
		}
	}

	private bool Waited()
	{
		Data data = DataSaver.LoadData();
		DateTime oldTime = DateTime.FromBinary(data.GetFeedbackTime());
		DateTime timeNow = System.DateTime.Now;

		difference = (timeNow - oldTime).TotalMinutes;
		if (difference >= feedbackDelayMins)
		{ // Waited long enough
			return true;
		}
		else
		{ // Has not waited long enough
			return false;
		}
	}

	public void AddFeedback()
	{
		Data data = DataSaver.LoadData();
		data.AddFeedback();
		feedbackCount = data.GetFeedback();

		if (feedbackCount == 1)
		{
			DateTime time = System.DateTime.Now;
			feedbackTimeBinary = time.ToBinary();
			data.SetFeedbackTime(feedbackTimeBinary);

			Debug.Log("Time of first feedback is " + time);
		}

		Save();
	}

	public void ResetFeedback()
	{
		Data data = DataSaver.LoadData();
		data.ResetFeedback();
		feedbackCount = data.GetFeedback();
		Save();

		// Open feedback window again
		Feedback();
	}

	public void GetFeedback()
	{
		Data data = DataSaver.LoadData();
		feedbackCount = data.GetFeedback();
		feedbackTimeBinary = data.GetFeedbackTime();
	}

	// Pause Menu
	public void ResumeGame()
	{
		ResumeAllAudio();
		Time.timeScale = 1;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		pauseMenu.SetActive(false);
	}

	public void RestartScene()
	{
		LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void QuitGame()
	{
		pauseMenu.SetActive(false);
		Save();
		//Application.Quit();
		LoadScene(0);
	}

	// Data management
	public void Save()
	{
		DataSaver.SaveProgress(this);
	}

	// Load given scene and mute volume while doing it
	public void LoadScene(int scene)
	{
		// Attempt to mute all audio sources
		//AudioSource[] sources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		//for (int index = 0; index < sources.Length; ++index)
		//{
		//	sources[index].mute = true;
		//}

		AudioListener.volume = 0f;
		Time.timeScale = 0;
		//StopAllAudio();
		PauseAllAudio();
		mm.LoadScene(scene);
		AudioListener.volume = 1f;
	}
}
