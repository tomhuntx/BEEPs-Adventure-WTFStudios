using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager GManager;
	public GameObject controls;

	
    [SerializeField] private GameObject pauseMenu;

    [Header("FPS Management")]
    public int targetFPS = 60;
    public bool displayFPS = false;
    public bool capFPS = true;
    [SerializeField] private TextMeshProUGUI FPSDisplay;
	private bool locked = false;

	//
	bool moveControls = false;
	RectTransform rect;
	//

	private void Awake()
    {
        GManager = this;
    }

    // Start is called before the first frame update
    void Start()
    {
		
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
		if (Input.GetMouseButtonDown(0) && Cursor.visible && !pauseMenu.activeSelf)
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			locked = true;
		}

		/////// Would move to GUI MANAGER
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
		///////

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


    public void ResumeGame()
    {
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        pauseMenu.SetActive(false);
    }

    public void RestartScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
