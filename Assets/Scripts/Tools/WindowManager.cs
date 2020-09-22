using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class WindowManager : MonoBehaviour
{
    #region Variables

    #region Player Prefs Key Constants
    public const string RESOLUTION_PREF_KEY = "_ScreenResolution";
    public const string SCREENMODE_PREF_KEY = "_ScreenMode";
    #endregion

    #region Resolution
    [SerializeField] private TMP_Dropdown currentResolution;
    private Resolution[] resolutions;
    private int currentResolutionIndex = 0;
    #endregion

    #region Screen Mode
    [SerializeField] private TMP_Dropdown currentWindowMode;
    private int currentScreenModeIndex = 0;
    private static FullScreenMode[] screenModes =
    {
        FullScreenMode.ExclusiveFullScreen,
        FullScreenMode.FullScreenWindow,
    };
    #endregion

    #region Others
    [Space]
    [Tooltip("The countdown before reverting to old settings.")]
    [SerializeField] private float revertTime = 10;
    private float revertTimer;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Button discardSettingsButton;
    [SerializeField] private GameObject settingsPromptParent;
    #endregion

    #region Events
    [Header("Events")]
    public UnityEvent onGraphicsChangeOnExit;
    public UnityEvent onGraphicsUnchangedOnExit;
    #endregion

    #endregion




    // Start is called before the first frame update
    void Start()
    {
        //Setup resolutions
        resolutions = Screen.resolutions;
        currentResolutionIndex = PlayerPrefs.GetInt(RESOLUTION_PREF_KEY, resolutions.Length - 1);
        List<string> names = new List<string>();
        
        foreach(Resolution resolution in resolutions)
        {
            names.Add(string.Format("{0}x{1} @ {2}Hz", resolution.width, resolution.height, resolution.refreshRate));
        }

        currentResolution.AddOptions(names);
        currentResolution.value = currentResolutionIndex;

        //Setup window modes
        currentScreenModeIndex = PlayerPrefs.GetInt(SCREENMODE_PREF_KEY, 1);
        List<string> screens = new List<string> {"Fullscreen", "Windowed Borderless"};
        currentWindowMode.AddOptions(screens);
        currentWindowMode.value = currentScreenModeIndex;
    }

    private void Update()
    {
        if (settingsPromptParent.activeSelf)
        {
            revertTimer -= 0.015f; //Custom delta time to make it work when paused
            countdownText.text = string.Format("Settings will revert in {0}", Mathf.CeilToInt(revertTimer));
            
            if (revertTimer <= 0)
            {
                discardSettingsButton.onClick.Invoke();
            }
        }
    }


    public static void InitializeGameWindow()
    {
        Resolution[] res = Screen.resolutions;

        int appliedRes = PlayerPrefs.GetInt(RESOLUTION_PREF_KEY, res.Length - 1);
        if (appliedRes >= res.Length)
        {
            PlayerPrefs.SetInt(RESOLUTION_PREF_KEY, res.Length - 1);
            appliedRes = Mathf.Clamp(appliedRes, 0, res.Length - 1);
        }

        Resolution currentRes = res[appliedRes];

        Screen.SetResolution(currentRes.width, currentRes.height,
                             screenModes[PlayerPrefs.GetInt(SCREENMODE_PREF_KEY, 0)]);
    }

    public void StartCountdown()
    {
        revertTimer = revertTime;
    }


    public void CheckForChanges()
    {
        if (currentResolutionIndex != currentResolution.value ||
            currentScreenModeIndex != currentWindowMode.value)
        {
            onGraphicsChangeOnExit.Invoke();
        }
        else
        {
            onGraphicsUnchangedOnExit.Invoke();
        }
    }

    /// <summary>
    /// Gets values from drop down lists.
    /// </summary>
    public void ApplySettings()
    {
        Resolution currentRes = resolutions[currentResolution.value];
        Screen.SetResolution(currentRes.width,
                             currentRes.height,
                             screenModes[currentWindowMode.value]);
    }

    /// <summary>
    /// Set index values from drop down lists.
    /// </summary>
    public void  ConfirmSettings()
    {
        currentResolutionIndex = currentResolution.value;
        PlayerPrefs.SetInt(RESOLUTION_PREF_KEY, currentResolutionIndex);

        currentScreenModeIndex = currentWindowMode.value;
        PlayerPrefs.SetInt(SCREENMODE_PREF_KEY, currentScreenModeIndex);
    }

    /// <summary>
    /// Re-apply settings based on index values.
    /// </summary>
    public void DiscardSettings()
    {
        Resolution currentRes = resolutions[currentResolutionIndex];
        Screen.SetResolution(currentRes.width,
                             currentRes.height,
                             screenModes[currentScreenModeIndex]);
        
        currentResolution.value = currentResolutionIndex;
        currentWindowMode.value = currentScreenModeIndex;
    }



    public void ResetSettings()
    {
        currentResolutionIndex = resolutions.Length - 1;
        currentScreenModeIndex = 0;
        DiscardSettings();

        PlayerPrefs.SetInt(RESOLUTION_PREF_KEY, currentResolutionIndex);
        PlayerPrefs.SetInt(SCREENMODE_PREF_KEY, currentScreenModeIndex);
    }
}
