using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerNextLevel : MonoBehaviour
{
	GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
		gm = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.tag == "Player")
		{
			gm.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}
	}
}
