using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverController : MonoBehaviour
{
	public Animator lever1;
	public Animator lever2;

	public void SetBools(bool on)
	{
		lever1.SetBool("managerOnPodium", on);
		lever2.SetBool("managerOnPodium", on);
	}

	public void SetSpeed(float speed)
	{
		lever1.speed = speed;
		lever2.speed = speed;
	}
}
