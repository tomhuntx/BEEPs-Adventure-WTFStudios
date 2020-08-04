using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioMixer masterMixer;
    [Range(-80, 20)] [SerializeField] private float masterVol;
    [Range(-80, 20)] [SerializeField] private float musicVol;
    [Range(-80, 20)] [SerializeField] private float soundEffectsVol;

    [SerializeField] private SliderInitializer master;
    private Slider masterSlider;

    [SerializeField] private SliderInitializer music;
    private Slider musicSlider;

    [SerializeField] private SliderInitializer soundEffects;
    private Slider soundEffectsSlider;

    //private float originalMasterLevels;
    //private float originalMusicLevels;
    //private float originalSFXLevels;
    public const float DEFAULT_MASTER_LEVEL = -25.0f;
    public const float DEFAULT_MUSIC_LEVEL = 0.0f;
    public const float DEFAULT_SFX_LEVEL = 0.0f;
    public const float DEFAULT_SFX_COMP_LEVEL = -15.0f;
    private float compressorDiffThreshold;


    [Header("Mouse Sensitivity")]    
    [SerializeField] private SliderInitializer mouseSensitivity;
    private Slider mouseSlider;
    public static float currentMouseSensitivity = 1;
    //public float CurrentMouseSensitivity { get { return currentMouseSensitivity; } }

    #region Prefs Keys
    public const string MOUSE_SENSITIVITY_PREFS_KEY = "_Mouse Sensitivity";
    public const string MASTER_VOL_PREFS_KEY = "_Master Volume";
    public const string MUSIC_VOL_PREFS_KEY = "_Music Volume";
    public const string SFX_VOL_PREFS_KEY = "_SFX Volume";
    public const string SFX_COMP_PREFS_KEY = "_SFX Compressor Level";
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        LoadPrefsData();

        //Setup levels
        masterMixer.GetFloat("Master_Levels", out masterVol);
        masterMixer.GetFloat("Music_Levels", out musicVol);

        masterMixer.GetFloat("SFX_CompressorThreshold", out float diff);
        masterMixer.GetFloat("SFX_Levels", out soundEffectsVol);
        compressorDiffThreshold = soundEffectsVol - diff;

        master.Initialize(-80, 20, masterVol, "Master");
        masterSlider = master.AssignedSlider;

        music.Initialize(-80, 20, musicVol, "Music");
        musicSlider = music.AssignedSlider;

        soundEffects.Initialize(-80, 20, soundEffectsVol, "Sound Effects");
        soundEffectsSlider = soundEffects.AssignedSlider;

                
        //Setup Mouse Settings
        mouseSensitivity.Initialize(0.5f, 3.0f, currentMouseSensitivity, "Mouse Sensitivity");
        mouseSlider = mouseSensitivity.AssignedSlider;
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ManageGameAudio();
        ManageSensitivity();
    }



    public void LoadPrefsData()
    {
        masterVol = DEFAULT_MASTER_LEVEL;
        musicVol = DEFAULT_MUSIC_LEVEL;
        soundEffectsVol = DEFAULT_SFX_LEVEL;
        compressorDiffThreshold = DEFAULT_SFX_COMP_LEVEL;

        LoadMouseSensitivity();
        LoadAudioLevels(masterMixer);
    }

    public static void LoadAudioLevels(AudioMixer mixer)
    {
        mixer.SetFloat("Master_Levels", PlayerPrefs.GetFloat(MASTER_VOL_PREFS_KEY, DEFAULT_MASTER_LEVEL));
        mixer.SetFloat("Music_Levels", PlayerPrefs.GetFloat(MUSIC_VOL_PREFS_KEY, DEFAULT_MUSIC_LEVEL));
        mixer.SetFloat("SFX_Levels", PlayerPrefs.GetFloat(SFX_VOL_PREFS_KEY, DEFAULT_SFX_LEVEL));
        mixer.SetFloat("SFX_CompressorThreshold", PlayerPrefs.GetFloat(SFX_COMP_PREFS_KEY, DEFAULT_SFX_COMP_LEVEL));
    }

    public static void LoadMouseSensitivity()
    {
        currentMouseSensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_PREFS_KEY, 1);
    }


    private void ManageSensitivity()
    {
        if (Time.timeScale == 0)
        {
            currentMouseSensitivity = mouseSlider.value;

            if (Player.Instance != null)
            {
                Player.Instance.PlayerMovementControls.lookSensitivityMultiplier = currentMouseSensitivity;
            }
        }
    }

    private void ManageGameAudio()
    {
        //only adjust volume when paused;
        if (Time.timeScale == 0)
        {
            masterVol = masterSlider.value;
            musicVol = musicSlider.value;
            soundEffectsVol = soundEffectsSlider.value;
        }

        UpdateAudioLevels();
    }

    private void UpdateAudioLevels()
    {
        masterMixer.SetFloat("Master_Levels", masterVol);
        masterMixer.SetFloat("Music_Levels", musicVol);
        masterMixer.SetFloat("SFX_Levels", soundEffectsVol);
        masterMixer.SetFloat("SFX_CompressorThreshold", soundEffectsVol - compressorDiffThreshold);
    }

    public void ToggleMute(bool isMuted)
    {
        if (isMuted)
        {
            masterMixer.SetFloat("Music_Sidechain_Level", -80);
            masterMixer.SetFloat("SFX_Sidechain_Level", -80);
        }
        else
        {
            masterMixer.SetFloat("Music_Sidechain_Level", 0);
            masterMixer.SetFloat("SFX_Sidechain_Level", 0);
        }
    }


    #region Reset Methods
    public void ResetLevels()
    {
        masterVol = DEFAULT_MASTER_LEVEL;
        musicVol = DEFAULT_MUSIC_LEVEL;
        soundEffectsVol = DEFAULT_SFX_LEVEL;

        UpdateAudioLevels();

        masterSlider.value = masterVol;
        musicSlider.value = musicVol;
        soundEffectsSlider.value = soundEffectsVol;

        SaveMasterVolume();
        SaveMusicVolume();
        SaveSFXVolume();
    }

    public void ResetMouseSensitivity()
    {
        currentMouseSensitivity = 1;
        mouseSensitivity.AssignedSlider.value = currentMouseSensitivity;

        SaveMouseSensitivity();
    }
    #endregion



    #region Slider Methods
    public void SaveMouseSensitivity()
    {
        currentMouseSensitivity = mouseSensitivity.AssignedSlider.value;
        PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_PREFS_KEY, currentMouseSensitivity);
    }

    public void SaveMasterVolume()
    {
        PlayerPrefs.SetFloat(MASTER_VOL_PREFS_KEY, masterVol);
    }

    public void SaveMusicVolume()
    {
        PlayerPrefs.SetFloat(MUSIC_VOL_PREFS_KEY, musicVol);
    }

    public void SaveSFXVolume()
    {
        PlayerPrefs.SetFloat(SFX_VOL_PREFS_KEY, soundEffectsVol);
        PlayerPrefs.SetFloat(SFX_COMP_PREFS_KEY, soundEffectsVol - compressorDiffThreshold);
    }
    #endregion
}
