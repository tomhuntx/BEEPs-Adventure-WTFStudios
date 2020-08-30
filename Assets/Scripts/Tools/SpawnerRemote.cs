using UnityEngine;
using System.Collections;

public class SpawnerRemote : MonoBehaviour
{
	public float delay = 0;
	private bool wait = false;

	private void Awake()
	{
		if (delay > 0)
		{
			wait = true;
		}	
	}

	/// <summary>
	/// Spawns prefab to this game object as child.
	/// </summary>
	public void SpawnToObject(GameObject prefab)
    {
		Instantiate(prefab, this.transform.localPosition, this.transform.localRotation, this.transform);
    }

    /// <summary>
    /// Spawns prefab to where this game object is at.
    /// </summary>
    /// <param name="prefab"></param>
    public void SpawnToPoint(GameObject prefab)
    {
		if (wait)
		{
			StartCoroutine(Delay(prefab));
		}
		else
		{
			Instantiate(prefab, this.transform.position, this.transform.rotation);
		}
    }


	// Pause and then start looking for hat - prevents instant attempts at grabbing
	private IEnumerator Delay(GameObject prefab)
	{
		yield return new WaitForSeconds(delay);

		wait = false;
		SpawnToPoint(prefab);
	}
}
