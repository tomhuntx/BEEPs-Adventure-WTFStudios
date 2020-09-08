using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
	[SerializeField] private TMP_Text loadingText;
	[SerializeField] private RawImage img;
	private float a = 0;
	private bool fadeIn = true;

    void Awake()
    {
		transform.SetAsLastSibling();
		StartCoroutine(LoadingText());
	}

	public void SetFade(float a)
	{
		img.color = new Color(1, 1, 1, a);
	}

	public void FadeOut()
	{
		if (fadeIn)
		{
			fadeIn = false;
			a = 0;
		}
	}

	public void FadeIn()
	{
		if (!fadeIn)
		{
			fadeIn = false;
			a = 1;
		}
	}

	IEnumerator LoadingText()
	{
		yield return new WaitForSecondsRealtime(0.15f);
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
