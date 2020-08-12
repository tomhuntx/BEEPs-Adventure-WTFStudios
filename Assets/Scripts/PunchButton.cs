using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchButton : MonoBehaviour
{
	private Animator anim;
	private SpawnerRemote spawner;
	public GameObject punchSFX;

	public GameObject chuteTrigger;
	public GameObject toSpawn;
	private TriggerInside boxDetection;
	private SpawnerRemote chuteSpawner;

	private bool punched = false;

    // Start is called before the first frame update
    void Start()
    {
		anim = GetComponent<Animator>();
		spawner = GetComponent<SpawnerRemote>();
		chuteSpawner = chuteTrigger.GetComponent<SpawnerRemote>();
		boxDetection = chuteTrigger.GetComponent<TriggerInside>();
	}

    // Update is called once per frame
    void Update()
    {
		if (punched)
		{

		}
    }

	IEnumerator Cooldown()
	{
		yield return new WaitForSeconds(2);

		punched = false;
	}

	IEnumerator PauseThenSpawn()
	{
		yield return new WaitForSeconds(0.5f);

		if (!boxDetection.inside)
		{
			chuteSpawner.SpawnToPoint(toSpawn);

		}
	}

	public void Punched()
	{
		if (!punched)
		{
			punched = true;
			anim.Play("button");

			spawner.SpawnToPoint(punchSFX);

			StartCoroutine(PauseThenSpawn());
			StartCoroutine(Cooldown());
		}
	}
}
