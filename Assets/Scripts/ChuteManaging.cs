using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChuteManaging : MonoBehaviour
{
	GameObject[] bots;
	GameObject[] mbots;
	float interval = 0;
	int chutesDestroyed = 0;

	public UnityEvent destroyedOneChute;
	public UnityEvent destroyedTwoChutes;
	public UnityEvent destroyedThreeChutes;

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
			if (mbot != null)
			{
				MBot_Controller rob = mbot.GetComponent<MBot_Controller>();
				if (rob != null)
				{
					rob.AngryForever();
				}
			}
		}
	}

	private float RandomFloat()
	{
		return Random.Range(0.0f, 1.8f);
	}

	public void DestroyedChute()
	{
		chutesDestroyed++;

		switch (chutesDestroyed)
		{
			case 1:
				destroyedOneChute.Invoke();
				break;
			case 2:
				destroyedTwoChutes.Invoke();
				break;
			case 3:
				destroyedThreeChutes.Invoke();
				break;
		}
	}
}
