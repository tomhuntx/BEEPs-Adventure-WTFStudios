using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager GManager;
	public GameObject controls;
	public GameObject feedback;
	public GameObject popup;
	private MenuManager mm;

	// DATA
	public int thisLevel;
	public int feedbackCount;
	public long feedbackTimeBinary;

	[SerializeField] private GameObject pauseMenu;

	private bool feedbackAllowed;
	private int feedbackDelayMins = 10;
	private double difference;

	[Header("FPS Management")]
    public int targetFPS = 60;
    public bool displayFPS = false;
    public bool capFPS = true;
    [SerializeField] private TextMeshProUGUI FPSDisplay;

	//
	bool moveControls = false;
	RectTransform rect;
	//

	private void Awake()
    {
        GManager = this;
		mm = FindObjectOfType<MenuManager>();

		GetFeedback();

		if (Waited())
		{
			feedbackAllowed = true;
		}
		else
		{
			feedbackAllowed = false;
		}

		// Should only occur if game is started from the level scene
		if (Cursor.lockState != CursorLockMode.Locked)
		{
			Cursor.lockState = CursorLockMode.Locked;
		}

		// Save current level progress
		Save();
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

		//// TEMP Controls Icon & movement
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
		////

        //Simple pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu != null)
            {
                if (pauseMenu.activeSelf)
                {
                    Time.timeScale = 1;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    pauseMenu.SetActive(false);
                }
                else
                {
                    Time.timeScale = 0;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    pauseMenu.SetActive(true);
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
            Application.targetFrameRate = 2147483647;
        }

		// Limit feedback count
		if (feedbackAllowed && feedbackCount >= 3)
		{
			feedbackAllowed = false;
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

	public void LoadScene(int scene)
	{
		Time.timeScale = 0;
		mm.LoadScene(scene);
	}
}
