using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class Feedback : MonoBehaviour
{
	public TMP_InputField feedbackInput;
	public GameObject thankyou;
	public static string userId;
	private string inputText = "";
	private GameManager gm;

	// URL of webhook discord bot
	string webhookURL = "https://discordapp.com/api/webhooks/724815659766382602/AI2yEaaXZQ3" 
		+ "wHDRbOZHsmfVhNHZ0Y-_51LqKtw78bpK-gSgizO6FLeUkHi6Y571VbJHp";

	// Destroy after 5 seconds
	private void Start()
	{
		gm = GameObject.FindObjectOfType<GameManager>();

		userId = AnalyticsSessionInfo.userId;

		Destroy(this.gameObject, 4.0f);
	}

	public void SubmitFeedback()
	{
		inputText = feedbackInput.text;

		if (inputText.Length < 8)
		{
			Fail("not enough characters");
		}
		else
		{
			//Success();

			gm.AddFeedback();

			ThankYouPopup();
			ExitFeedback();
		}
	}

	public void ExitFeedback()
	{
		inputText = "";
		feedbackInput.text = "";

		this.gameObject.SetActive(false);
	}

	private void Success()
	{
		WWWForm form = new WWWForm();
		form.AddField("content", $"```{userId + " says:\n"+ inputText}```");
		UnityEngine.Networking.UnityWebRequest.Post(webhookURL, form).SendWebRequest();
	}

	private void Fail(string reason)
	{
		switch (reason)
		{
			case "not enough characters":
				OtherPopup("Not enough characters.");
				break;
			case "":
				OtherPopup("Not enough characters.");
				break;
		}
	}

	private void ThankYouPopup()
	{
		GameObject ty = Instantiate(thankyou, thankyou.transform.position, thankyou.transform.rotation) as GameObject;
		ty.transform.SetParent(this.transform.parent, false);
	}

	private void OtherPopup(string text)
	{
		GameObject other = Instantiate(thankyou, thankyou.transform.position, thankyou.transform.rotation) as GameObject;
		other.transform.SetParent(this.transform.parent, false);
		other.GetComponent<Image>().color = Color.red;
		other.GetComponentInChildren<TMP_Text>().SetText(text);
	}
}
