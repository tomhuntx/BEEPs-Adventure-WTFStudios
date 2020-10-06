using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingParticle : MonoBehaviour
{
    // Animator to transition
    public GameObject CraftingBurstParticle;
    public float ParticleDelay;

    //detect when a flat box is on the conveyor
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Box Material")
        {
            Invoke("Crafty", ParticleDelay);
            Debug.Log("i love programming");
        }
    }

    //spawn craft burst particle when the new boxes are created
    private void Crafty()
    {
        GameObject BoxDetector = Instantiate(CraftingBurstParticle, transform.position, Quaternion.identity);
        Destroy(BoxDetector, 2.0f);

    }
}
