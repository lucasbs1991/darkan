using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour {

	public string area;

	void OnTriggerEnter2D(Collider2D coll)
    {
    	coll.gameObject.GetComponent<PlayerBehaviour>().changeArea(area);
    }
}
