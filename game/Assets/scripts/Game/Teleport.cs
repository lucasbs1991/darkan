using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour {

	public string channel;

	void OnTriggerEnter2D(Collider2D coll)
    {
    	coll.gameObject.GetComponent<PlayerBehaviour>().changeChannel(channel);
    }
}
