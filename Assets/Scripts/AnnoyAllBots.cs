using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnoyAllBots : MonoBehaviour
{
	GameObject[] bots;
	GameObject[] mbots;
	float interval = 0;

	// Start is called before the first frame update
	void Start()
    {
		bots = GameObject.FindGameObjectsWithTag("Bot");
		mbots = GameObject.FindGameObjectsWithTag("ManagerBot");
	}

	public void AnnoyBots()
	{
		foreach (GameObject bot in bots)
		{
			Robot rob = bot.GetComponent<Robot>();
			if (rob != null)
			{
				interval += RandomFloat();
				rob.GetSuperAnnoyed(interval);
			}
		}

		foreach (GameObject mbot in mbots)
		{
			Robot rob = mbot.GetComponent<Robot>();
			if (rob != null)
			{
				rob.GetSuperAnnoyed(0.1f);
			}
		}
	}

	private float RandomFloat()
	{
		return Random.Range(0.3f, 0.7f);
	}
}
