using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Scripts that contains methods that will automatically run without being attached to a game object
/// </summary>
public class GameSettingsInitializer : MonoBehaviour
{
    ////Before first Scene loaded
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //static void OnBeforeSceneLoadRuntimeMethod()
    //{
    //    Debug.Log("Before first Scene loaded");
    //}

    ////After first Scene loaded
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    //static void OnAfterSceneLoadRuntimeMethod()
    //{
    //    Debug.Log("After first Scene loaded");
    //}

    //RuntimeMethodLoad: After first Scene loaded
    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        //Debug.Log("RuntimeMethodLoad: After first Scene loaded");        
        SettingsMenu.LoadMouseSensitivity();
        WindowManager.InitializeGameWindow();

        new GameObject().AddComponent<GameSettingsInitializer>();
    }

    private void Start()
    {
        AudioMixer masterMixer = Resources.Load<AudioMixer>("Mixer Groups/Master");
        SettingsMenu.LoadAudioLevels(masterMixer);
        Destroy(this.gameObject);
    }
}
