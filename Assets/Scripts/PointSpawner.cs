using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float interval = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("DoSpawn");
    }

    private IEnumerator DoSpawn()
    {
        while(true)
        {
            Instantiate(prefab, this.transform.position, this.transform.rotation);
            yield return new WaitForSeconds(interval);
        }
    }
}
