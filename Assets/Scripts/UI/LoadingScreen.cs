using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
	[SerializeField] private TMP_Text loadingText;
    void Awake()
    {
		transform.SetAsLastSibling();
		StartCoroutine(LoadingText());
	}

	IEnumerator LoadingText()
	{
		yield return new WaitForSecondsRealtime(0.2f);
		if (loadingText.text.Length >= 10)
		{
			loadingText.text = "Loading";
		}
		else
		{
			loadingText.text = loadingText.text + ".";
		}
		StartCoroutine(LoadingText());
	}
}
