using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnoyAllMinibots : MonoBehaviour
{
	GameObject[] bots;
	GameObject[] mbots;
	float interval = 0;

	// Start is called before the first frame update
	void Start()
	{
		bots = GameObject.FindGameObjectsWithTag("MiniBot");
		mbots = GameObject.FindGameObjectsWithTag("ManagerBot");
	}

	public void AnnoyBots()
	{
		foreach (GameObject bot in bots)
		{
			if (bot != null)
			{
				NPC_Controller rob = bot.GetComponent<NPC_Controller>();
				if (rob != null)
				{
					interval = RandomFloat();
					rob.SetScaredForever(interval);
				}
			}
		}

		foreach (GameObject mbot in mbots)
		{
			MBot_Controller rob = mbot.GetComponent<MBot_Controller>();
			if (rob != null)
			{
				rob.AngryForever();
			}
		}
	}

	private float RandomFloat()
	{
		return Random.Range(0.0f, 1.8f);
	}
}
