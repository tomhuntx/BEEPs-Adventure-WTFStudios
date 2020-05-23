using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundRandomizer : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;
    private AudioSource source;
    [SerializeField] private bool enablePitchRandomization;

    // Start is called before the first frame update
    void Start()
    {
        source = gameObject.GetComponent<AudioSource>();
        int i = Random.Range(0, clips.Length);
        source.clip = clips[i];
        if (enablePitchRandomization) source.pitch = Random.Range(0.8f, 1.1f);   
        source.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!source.isPlaying)
        {
            Destroy(this.gameObject);
        }
    }
}
