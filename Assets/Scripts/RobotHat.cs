using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotHat : MonoBehaviour
{
	public Task robotHat;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Box")
		{
			if (!other.transform.GetComponent<Rigidbody>().isKinematic)
			{
				robotHat.Contribute();
			}
			
		}
    }
}
