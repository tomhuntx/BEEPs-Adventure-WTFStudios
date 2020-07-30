using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanner : MonoBehaviour
{
	[SerializeField] private Light scanLight;
	[SerializeField] private GameObject scanObj;
	private Material scanMat;

	public enum lightColour
	{
		aqua,
		green,
		yellow,
		red
	}

	private void Start()
	{
		scanMat = scanObj.GetComponent<Renderer>().material;

		scanLight.color = Color.cyan;
		scanMat.SetColor("_EmissionColor", Color.cyan);
	}

	// Change light based on given colour
	public void ChangeLight(lightColour col)
	{
		switch (col) {
			case lightColour.aqua:
				scanLight.color = Color.cyan;
				scanMat.SetColor("_EmissionColor", Color.cyan);

				break;
			case lightColour.green:
				scanLight.color = Color.green;
				scanMat.SetColor("_EmissionColor", Color.green);

				// PLAY SOUND HERE

				StartCoroutine(LightTimer(1));
				break;
			case lightColour.yellow:
				scanLight.color = Color.yellow;
				scanMat.SetColor("_EmissionColor", Color.yellow);

				// PLAY SOUND HERE

				StartCoroutine(LightTimer(1.5f));
				break;
			case lightColour.red:
				scanLight.color = Color.red;
				scanMat.SetColor("_EmissionColor", Color.red);

				// PLAY SOUND & TRIGGER ERROR HERE

				// High priority - stop other scanning
				StopAllCoroutines();
				StartCoroutine(LightTimer(5));
				break;
		}
	}

	// Change light back to aqua after x seconds
	IEnumerator LightTimer(float seconds)
	{
		yield return new WaitForSeconds(seconds);

		ChangeLight(lightColour.aqua);
	}

	// Trigger changes on trigger collision with objects
	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.tag == "Box")
		{
			ChangeLight(lightColour.green);
		}
		else if (other.transform.tag == "Hardhat")
		{
			ChangeLight(lightColour.red);
		}
		else
		{
			ChangeLight(lightColour.yellow);
		}
	}
}
