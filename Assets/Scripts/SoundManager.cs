using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
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

	// Whether or not level has tasks (testing or tutorial)
	[SerializeField] private bool hasTasks = true;

	private float originalMasterLevels;
    private float originalMusicLevels;
    private float originalSFXLevels;
    private float compressorDiffThreshold;

    [Header("Main Game Song Loop")]
    [Tooltip("Please arrange according to intensity where 0 is to least intense.")]
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private float fadeOutRate = 1.5f;
    [SerializeField] private float fadeInRate = 0.8f;
    private AudioSource audioSource;
    private bool fadeOutMusic = false;
    private float originalVolume;


    // Start is called before the first frame update
    void Start()
    {
        //Setup levels
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

        //Look for tasklist instance
        audioSource = this.GetComponent<AudioSource>();
        originalVolume = audioSource.volume;
    }

    // Update is called once per frame
    void Update()
    {
        ManageGameAudio();
		if (hasTasks)
		{
			ManageMainGameMusic();
		}

        if (fadeOutMusic)
        {
            if (audioSource.volume > 0)
            {
                audioSource.volume -= fadeOutRate * Time.deltaTime;
            }
            audioSource.volume = Mathf.Clamp(audioSource.volume, 0, originalVolume);
        }
        else
        {
            if (audioSource.volume < originalVolume)
            {
                audioSource.volume += fadeInRate * Time.deltaTime;
            }
            audioSource.volume = Mathf.Clamp(audioSource.volume, 0, originalVolume);
        }
    }


    private void ManageMainGameMusic()
    {
        int index = TaskList.Instance.NumMainTasksDone;
        if (index < clips.Length &&
            audioSource.clip != clips[index])
        {
            audioSource.clip = clips[index];
        }
        if (!audioSource.isPlaying) audioSource.Play();
    }

    private void ManageGameAudio()
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

    public void FadeMusic(float time)
    {
        StartCoroutine(DoFadeMusic(time));
    }

    private IEnumerator DoFadeMusic(float time)
    {
        fadeOutMusic = true;
        yield return new WaitForSeconds(time);
        fadeOutMusic = false;
    }
}
