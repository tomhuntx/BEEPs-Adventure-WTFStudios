using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawMachineSFXManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource railsAudio;
    [SerializeField] private float railsPitchSpeed = 1.5f;
    [Space()]
    [SerializeField] private AudioSource shaftAudio;
    [SerializeField] private float shaftPitchSpeed = 1.5f;
    [Space]
    [SerializeField] private Animator clawAnim;
    [SerializeField] private AudioSource clawAudio;
    [SerializeField] private float clawPitchSpeed = 3.0f;
    [SerializeField] private float clawTimeOffset = 0.3f;
    private float clawTimer;

    [Header("Other Properties")]
    [SerializeField] private Transform shaftTRS;
    [SerializeField] private GameObject lockingSFX;

    private Vector3 previousPos = Vector3.negativeInfinity;
    private Vector3 currentPos = Vector3.negativeInfinity;
    private Direction moveDir;


    private bool isMovingHorizontal = false;
    private bool isMovingVertical = false;

    private bool triggeredRailsLockSFX = false;
    private bool triggeredShaftLockSFX = false;
    private bool triggeredClawSFX = false;
    private bool previousClawState;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CalculateDirection());
        previousClawState = !clawAnim.GetBool("DoClamp");
    }

    // Update is called once per frame
    void Update()
    {
        CheckMovement();
        ManageRailsAudio();
        ManageShaftAudio();
        ManageClawAudio();
    }

    private void FixedUpdate()
    {
        currentPos = new Vector3(this.transform.position.x,
                                 shaftTRS.position.y,
                                 this.transform.position.z);        
    }

    private IEnumerator CalculateDirection()
    {
        while(true)
        {
            previousPos = currentPos;
            yield return new WaitForSeconds(Time.fixedDeltaTime);

            if (currentPos != Vector3.negativeInfinity &&
                previousPos != Vector3.negativeInfinity)
            {
                moveDir = new Direction(previousPos, currentPos);
            }
        }
    }

    private void CheckMovement()
    {
        Vector3 horizontalVector = new Vector3(moveDir.localDirection.x, 0, moveDir.localDirection.z);
        Vector3 vecticalVector = new Vector3(0, moveDir.localDirection.y, 0);

        isMovingHorizontal = horizontalVector.magnitude > 0;
        isMovingVertical = vecticalVector.magnitude > 0;
    }

    private void ManageRailsAudio()
    {
        if (isMovingHorizontal)
        {
            railsAudio.pitch += railsPitchSpeed * Time.deltaTime;
            triggeredRailsLockSFX = true;
        }
        else
        {
            railsAudio.pitch -= railsPitchSpeed * Time.deltaTime;
            if (triggeredRailsLockSFX)
            {
                Instantiate(lockingSFX, railsAudio.transform.position, Quaternion.identity);
                triggeredRailsLockSFX = false;
            }
        }
        railsAudio.pitch = Mathf.Clamp(railsAudio.pitch, 0, 1);

        
    }

    private void ManageShaftAudio()
    {
        if (isMovingVertical)
        {
            shaftAudio.pitch += shaftPitchSpeed * Time.deltaTime;
            triggeredShaftLockSFX = true;
        }
        else
        {
            shaftAudio.pitch -= shaftPitchSpeed * Time.deltaTime;
            if (triggeredShaftLockSFX)
            {
                Instantiate(lockingSFX, shaftTRS.position, Quaternion.identity);
                triggeredShaftLockSFX = false;
            }
        }
        shaftAudio.pitch = Mathf.Clamp(shaftAudio.pitch, 0, 1);
    }

    private void ManageClawAudio()
    {    
        //if (previousClawState != clawAnim.GetBool("DoClamp"))
        //{
        //    if (clawTimer < Time.time)
        //    {
        //        clawTimer = clawTimeOffset + Time.time;
        //        triggeredClawSFX = false;
        //        previousClawState = clawAnim.GetBool("DoClamp");
        //    }
        //    else
        //    {
        //        triggeredClawSFX = true;
        //    }
        //}
        //else
        //{
        //    triggeredClawSFX = true;
        //}

        //if (!triggeredClawSFX)
        //{
        //    if (previousClawState == true)
        //    {
        //        clawAudio.pitch += clawPitchSpeed * Time.deltaTime;
        //    }
        //    else
        //    {
        //        clawAudio.pitch -= clawPitchSpeed * Time.deltaTime;
        //    }
        //}
        //else
        //{
        //    clawAudio.pitch -= clawPitchSpeed * Time.deltaTime;
        //}
        //clawAudio.pitch = Mathf.Clamp(clawAudio.pitch, 0, 1);

        if (previousClawState != clawAnim.GetBool("DoClamp"))
        {
            previousClawState = clawAnim.GetBool("DoClamp");
            triggeredClawSFX = false;
            //clawAudio.Stop();
            //clawAudio.pitch = 0;
            //clawAudio.Play();
        }

        if (!triggeredClawSFX)
        {
            clawTimer = clawTimeOffset + Time.time;
            triggeredClawSFX = true;
        }
        else
        {
            if (clawTimer > Time.time)
            {
                clawAudio.pitch += clawPitchSpeed * Time.deltaTime;
            }
            else
            {
                clawAudio.pitch -= clawPitchSpeed * Time.deltaTime * 2;
            }
        }
        clawAudio.pitch = Mathf.Clamp(clawAudio.pitch, 0, 1);
    }
}
