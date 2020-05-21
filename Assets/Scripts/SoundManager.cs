using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
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
    

    private float originalMasterLevels;
    private float originalMusicLevels;
    private float originalSFXLevels;
    private float compressorDiffThreshold;
    

    // Start is called before the first frame update
    void Start()
    {
        masterMixer.GetFloat("Master_Levels", out originalMasterLevels);
        masterMixer.GetFloat("Music_Levels", out originalMusicLevels);

        masterMixer.GetFloat("SFX_CompressorThreshold", out float diff);
        masterMixer.GetFloat("SFX_Levels", out originalSFXLevels);
        compressorDiffThreshold = originalSFXLevels - diff;

        masterVol = originalMasterLevels;
        musicVol = originalMusicLevels;
        soundEffectsVol = originalSFXLevels;

        master.Initialize(-80, 20, masterVol, "Master");
        masterSlider = master.AssignedSlider;

        music.Initialize(-80, 20, musicVol, "Music");
        musicSlider = music.AssignedSlider;

        soundEffects.Initialize(-80, 20, soundEffectsVol, "Sound Effects");
        soundEffectsSlider = soundEffects.AssignedSlider;
    }

    // Update is called once per frame
    void Update()
    {
        //only adjust volume when paused;
        if (Time.timeScale == 0)
        {
            masterVol = masterSlider.value;
            musicVol = musicSlider.value;
            soundEffectsVol = soundEffectsSlider.value;

            //Mute sounds when paused
            masterMixer.SetFloat("Music_Sidechain_Level", -80);
            masterMixer.SetFloat("SFX_Sidechain_Level", -80);
        }
        else
        {
            masterMixer.SetFloat("Music_Sidechain_Level", 0);
            masterMixer.SetFloat("SFX_Sidechain_Level", 0);
        }

        masterMixer.SetFloat("Master_Levels", masterVol);
        masterMixer.SetFloat("Music_Levels", musicVol);
        masterMixer.SetFloat("SFX_Levels", soundEffectsVol);
        masterMixer.SetFloat("SFX_CompressorThreshold", soundEffectsVol - compressorDiffThreshold);
    }

    public void ResetLevels()
    {
        masterVol = originalMasterLevels;
        musicVol = originalMusicLevels;
        soundEffectsVol = originalSFXLevels;
    }
}
