using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OtherPlayer : MonoBehaviour {

	public float speed = 1.4f;

	Vector3 dirNormalized, target;

	bool moving = false;

	public TextMeshPro playerNameText;

	public void Move(float posx, float posy, string toDir){
		transform.position = new Vector3 (Mathf.RoundToInt(posx), Mathf.RoundToInt(posy), 0);
		posx = 0;
		posy = 0;
		print (toDir);
		if (toDir == "up")
			posy++;
		else if (toDir == "down")
			posy--;
		else if (toDir == "left")
			posx--;
		else if (toDir == "right")
			posx++;

		target = new Vector3 (transform.position.x + posx, transform.position.y + posy, 0);
		dirNormalized = (target - transform.position).normalized;
		if(!moving)
			StartCoroutine (Moving (posx, posy));
	}

	IEnumerator Moving(float posx, float posy){
		moving = true;
		while(Vector3.Distance(target, transform.position) >= 0.1f){
			transform.position = transform.position + dirNormalized * speed * Time.deltaTime;
			yield return null;
		}

		transform.position = new Vector3(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y), 0);
		moving = false;
		// after walk
		//print ("chegou");

		/*if (lastDir == "up")
			spriteRenderer.sprite = sprites[0];
		else if (lastDir == "left") {
			spriteRenderer.sprite = sprites[2];
		} else if (lastDir == "right") {
			spriteRenderer.sprite = sprites[2];
		} else if (lastDir == "down")
			spriteRenderer.sprite = sprites[1];*/
	}

	public void SetPlayerName(string name){
		playerNameText.SetText (name);
	}
}
