using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistonParticle : MonoBehaviour
{
    // Animator to transition
    public GameObject PuffParticleForward;
    public GameObject PuffParticleBackward;
    public float ParticleDelay = 1f;

    private bool boxExists = false;

    private void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Generic Destructable")
        {
            Invoke("Puff", ParticleDelay);
            //Puff();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Box Material" || other.tag == "Generic Destructable")
        {
            
        }
    }
    private void Puff()
    {
        GameObject PistonParticleForward = Instantiate(PuffParticleForward, transform.position, Quaternion.identity);
        GameObject PistonParticleBackward = Instantiate(PuffParticleBackward, transform.position, Quaternion.identity);

        Destroy(PistonParticleForward, 2.0f);
        Destroy(PistonParticleBackward, 2.0f);

    }
}
