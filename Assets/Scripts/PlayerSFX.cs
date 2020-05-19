using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSFX : MonoBehaviour
{
    private AudioSource source;
    private FPSController controller;
    private float originalPitch;
    private float sprintPitch;
    private bool isLanded = true;
    [SerializeField] private float pitchMult = 0.5f;
    [SerializeField] private float landSFXThreshold = 3.0f;
    [SerializeField] private GameObject jumpSFXPrefab;
    [SerializeField] private GameObject landSFXPrefab;


    // Start is called before the first frame update
    void Start()
    {
        source = this.GetComponent<AudioSource>();
        controller = this.GetComponent<FPSController>();
        originalPitch = source.pitch;
    }

    // Update is called once per frame
    void Update()
    {
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
                isLanded = true;
                sprintPitch = 0;

                if (controller.Controller.velocity.magnitude > landSFXThreshold)
                {
                    Instantiate(landSFXPrefab, this.transform.position, this.transform.rotation);
                }
                else
                {

                }
            }

            Vector3 movement = (Input.GetAxis("Horizontal") * this.transform.right) +
                               (Input.GetAxis("Vertical") * this.transform.forward);
            movement = Vector3.ClampMagnitude(movement, 1);

            if (movement.magnitude > 0)
            {
                if (!source.loop) source.loop = true;
                if (!source.isPlaying) source.Play();

                
                float newPitch = movement.magnitude + originalPitch;
                float min = originalPitch - pitchMult;
                float max = originalPitch + pitchMult;
                newPitch = Mathf.Clamp(newPitch, min, max);
                source.pitch = newPitch + sprintPitch;
                //print(source.isPlaying);
            }
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

            if (source.pitch > 0)
            {
                if (source.isPlaying) source.pitch -= Time.deltaTime;
            }
            else
            {
                source.pitch = originalPitch;
                source.Stop();
            }
        }
    }
}
