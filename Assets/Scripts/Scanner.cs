using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Scanner : MonoBehaviour
{
	[SerializeField] private Light scanLight;
	[SerializeField] private GameObject scanObj;
	[SerializeField] private GameObject scanObj2;
	private Material scanMat;
	private Material scanMat2;
	private bool alarm = false;

	[Header("Events")]
	public UnityEvent onScanApprove;
	public UnityEvent onScanWarning;
	public UnityEvent onScanError;


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
		scanMat2 = scanObj2.GetComponent<Renderer>().materials[1];

		scanLight.color = Color.cyan;
		scanMat.SetColor("_EmissionColor", Color.cyan);
	}

	// Change light based on given colour
	public void ChangeLight(lightColour col)
	{
		if (!alarm)
		{
			switch (col)
			{
				case lightColour.aqua:
					scanLight.color = Color.cyan;
					scanMat.SetColor("_EmissionColor", Color.cyan);
					scanMat2.SetColor("_EmissionColor", Color.cyan);

					break;
				case lightColour.green:
					scanLight.color = Color.green;
					scanMat.SetColor("_EmissionColor", Color.green);
					scanMat2.SetColor("_EmissionColor", Color.green);

					// PLAY SOUND HERE
					onScanApprove.Invoke();

					StartCoroutine(LightTimer(1));
					break;
				case lightColour.yellow:
					scanLight.color = Color.yellow;
					scanMat.SetColor("_EmissionColor", Color.yellow);
					scanMat2.SetColor("_EmissionColor", Color.yellow);

					// PLAY SOUND HERE
					onScanWarning.Invoke();

					StartCoroutine(LightTimer(1.5f));
					break;
				case lightColour.red:
					scanLight.color = Color.red;
					scanMat.SetColor("_EmissionColor", Color.red);
					scanMat2.SetColor("_EmissionColor", Color.red);

					// PLAY SOUND & TRIGGER ERROR HERE
					onScanError.Invoke();

					// High priority - stop other scanning
					StopAllCoroutines();
					alarm = true;
					break;
			}
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
		else if (other.transform.tag == "Hardhat" || other.transform.tag == "Banana")
		{
			ChangeLight(lightColour.red);
		}
		else
		{
			if (other.tag == "Outline") return;
			ChangeLight(lightColour.yellow);
		}
	}
}
