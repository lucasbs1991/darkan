using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using Pomelo.DotNetClient;

public class Game : MonoBehaviour
{
	private string userName = LoginGUI.userName;
	private PomeloClient pclient = LoginGUI.pc;

	private string playerName;

	private ArrayList userList = null; 

	private GameObject player;
	private Transform players;

	void Start() 
	{
		player = Resources.Load ("Player") as GameObject;
		players = GameObject.Find ("Players").transform;

		userList = new ArrayList();

		InitPlayers();

		pclient.on("onAdd", (data) => {
			print("onAdd");
			RefreshPlayers("add", data);
		});

		pclient.on("onLeave", (data) => {
			print("onLeave");
			RefreshPlayers("leave", data);
		});
	}

	void InitPlayers(){
		JsonObject jsonObject = LoginGUI.users;
		System.Object users = null;
		if (jsonObject.TryGetValue("users", out users)) {
			string u = users.ToString();
			string [] initUsers = u.Substring(1,u.Length-2).Split(new Char[] { ',' });
			int length = initUsers.Length;
			for(int i = 0; i < length; i++) {
				string s = initUsers[i];
				string name = s.Substring (1, s.Length - 2);
				userList.Add(name);
				if (userName != name) {
					GameObject go = Instantiate (player, players) as GameObject;
					go.name = name;
					go.GetComponent<OtherPlayer> ().SetPlayerName (name);
				}
			}
		}
	}

	//Update the userlist.
	void RefreshPlayers(string flag,JsonObject msg){
		System.Object user = null;
		if(msg.TryGetValue("user", out user)) {
			if (flag == "add") {
				this.userList.Add(user.ToString());
				UnityMainThreadDispatcher.Instance().Enqueue(InstantiatePlayer(user.ToString())); 
			} else if (flag == "leave") {
				this.userList.Remove(user.ToString());
				UnityMainThreadDispatcher.Instance().Enqueue(DestroyPlayer(user.ToString())); 
			}
		}
	}

	public IEnumerator InstantiatePlayer(string user) {
		GameObject go = Instantiate (player, players) as GameObject;
		go.name = user;
		go.GetComponent<OtherPlayer> ().SetPlayerName (user);
		yield return null;
	}

	public IEnumerator DestroyPlayer(string user) {
		Destroy (GameObject.Find (user));
		yield return null;
	}

	//When quit, release resource
	void Update(){
		if(Input.GetKey(KeyCode.Escape) || Input.GetKey("escape")) {
			if (pclient != null) {
				pclient.disconnect();
			}
			Application.Quit();
		}
	}

	//When quit, release resource
	void OnApplicationQuit(){
		if (pclient != null) {
			pclient.disconnect();
		}
	}

}


