using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundRandomizer : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;
    private AudioSource source;
    [SerializeField] private bool enablePitchRandomization;

    [Tooltip("Keep doing this task and disable destroy on end.")]
    [SerializeField] private bool repeatOnEnd = false;

    // Start is called before the first frame update
    void Start()
    {
        source = gameObject.GetComponent<AudioSource>();
        PickClip();
    }

    // Update is called once per frame
    void Update()
    {
        if (!source.isPlaying)
        {
            if (repeatOnEnd)
            {
                PickClip();
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void PickClip()
    {
        int i = Random.Range(0, clips.Length);
        source.clip = clips[i];
        if (enablePitchRandomization) source.pitch = Random.Range(0.8f, 1.1f);
        source.Play();
    }
}
