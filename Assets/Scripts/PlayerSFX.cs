using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSFX : MonoBehaviour
{
    private AudioSource source;
    private PlayerCharacterController controller;
    private float originalPitch;
    private float sprintPitch;
    private bool isLanded = true;
    private float heightDisplacement;
    private bool isGamePaused = false;
    
    [Tooltip("How much pitch will be increase from the current pitch when moving.")]
    [SerializeField] private float pitchMult = 0.5f;

    [Tooltip("How much distance displacement before playing the effect.")]
    [SerializeField] private float landSFXThreshold = 2.5f;


    [SerializeField] private GameObject jumpSFXPrefab;
    [SerializeField] private GameObject landSFXPrefab;

    public AudioSource SFXSource { get { return source; } set { source = value; } }

    // Start is called before the first frame update
    void Start()
    {
        source = this.GetComponent<AudioSource>();
        controller = this.GetComponent<PlayerCharacterController>();
        originalPitch = source.pitch;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale > 0)
        {
            if (isGamePaused)
            {
                source.Play();
                isGamePaused = false;
            }
        }
        else
        {
            source.Stop();
            isGamePaused = true;
        }

        //doesn't work if in controller is grounded for some reason
        //current workaround
        if (isLanded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                Instantiate(jumpSFXPrefab, this.transform.position, this.transform.rotation, this.transform);
            }

            if (Input.GetButton("Sprint"))
            {
                sprintPitch += (controller.SprintSpeed - controller.WalkSpeed) * Time.deltaTime;
            }
            else
            {
                sprintPitch -= (controller.SprintSpeed - controller.WalkSpeed) * Time.deltaTime;
            }
            sprintPitch = Mathf.Clamp(sprintPitch,
                                      0,
                                      originalPitch + pitchMult);
        }

        if (controller.Controller.isGrounded)
        {
            if (!isLanded)
            {
                if (heightDisplacement > landSFXThreshold)
                {
                    Instantiate(landSFXPrefab, this.transform.position, this.transform.rotation);
                }
                heightDisplacement = 0;
                sprintPitch = 0;
                isLanded = true;
            }

            //Get movement direction
            Vector3 movement = (Input.GetAxis("Horizontal") * this.transform.right) +
                               (Input.GetAxis("Vertical") * this.transform.forward);
            movement = Vector3.ClampMagnitude(movement, 1);

            //Play and pitch up sound when moving
            if (movement.magnitude > 0)
            {
                if (!source.loop) source.loop = true;
                if (!source.isPlaying) source.Play();

                
                float newPitch = movement.magnitude + originalPitch;
                float min = originalPitch - pitchMult;
                float max = originalPitch + pitchMult;
                newPitch = Mathf.Clamp(newPitch, min, max);
                source.pitch = newPitch + sprintPitch;
            }
            //Pitch down then stop when not moving
            else
            {
                if (source.pitch > 0)
                {
                    if (source.isPlaying) source.pitch -= 4 * Time.deltaTime;
                }
                else
                {
                    source.pitch = originalPitch;
                    source.Stop();
                }
            }
        }     
        else
        {
            isLanded = false;

            //Pitch down when mid-air
            if (source.pitch > 0)
            {
                if (source.isPlaying) source.pitch -= Time.deltaTime;
            }
            else
            {
                source.pitch = originalPitch;
                source.Stop();
            }

            //Check highest point before losing height
            RaycastHit hitInfo;
            if (Physics.Raycast(this.transform.position, -this.transform.up, out hitInfo, Mathf.Infinity))
            {
                if (hitInfo.distance > heightDisplacement)
                    heightDisplacement = hitInfo.distance;
            }
        }
    }
}
