using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Feedback : MonoBehaviour {

	public GameObject feedback;
	public Text feedbackText;

	public void ShowText(string text){
		StartCoroutine (FeedbackTimeout (text));
	}

	IEnumerator FeedbackTimeout(string text){
		feedback.SetActive (true);
		feedbackText.text = text;
		yield return new WaitForSeconds (3);
		feedback.SetActive (false);
	}
}
