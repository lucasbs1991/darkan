using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using Pomelo.DotNetClient;

public class Game : MonoBehaviour
{
	public static bool connected = false;

	// private string userName = LoginGUI.userName;
	private PomeloClient pclient = LoginGUI.pc;

	private string playerName;

	private ArrayList userList = null; 

	private GameObject player;
	private Transform players;

	void Start() 
	{
		pclient.removeOn(); // clear all callbacks

		JsonObject message = new JsonObject();
		message.Add("area", LoginGUI.channel);
		pclient.request("area.areaHandler.enterScene", message, (data) => { // load players from this channel
			print(data);
			LoginGUI.users = data;
			UnityMainThreadDispatcher.Instance().Enqueue(InitPlayers()); 
		});

		player = Resources.Load ("Player") as GameObject;
		players = GameObject.Find ("Players").transform;

		userList = new ArrayList();

		// other player entered
		pclient.on("onAdd", (data) => {
			print("onAdd");
			RefreshPlayers("add", data);
		});

		pclient.on("onLeave", (data) => {
			print("onLeave");
			RefreshPlayers("leave", data);
		});
	}

	IEnumerator InitPlayers(){
		JsonObject jsonObject = LoginGUI.users;
		System.Object users = null;
		if (jsonObject.TryGetValue("users", out users)) {
			string u = users.ToString();
			string [] initUsers = u.Substring(1,u.Length-2).Split('}');
			int length = initUsers.Length - 1;
			print("players qnt: "+length);
			for(int i = 0; i < length; i++) {
				string s = initUsers[i];
				string name = GetValue(s, "uid");
				userList.Add(name);
				if (LoginGUI.userName != name) {
					int posx = int.Parse(GetValue(s, "posx"));
					int posy = int.Parse(GetValue(s, "posy"));
					GameObject go = Instantiate (player, new Vector3(posx, posy, 0), Quaternion.identity, players) as GameObject;
					go.name = name;
					go.GetComponent<OtherPlayer> ().SetPlayerName (name);
				}
			}
		}

		yield return null;
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





	string GetValue( string objectString, string target){
		int targetLength = target.Length + 3;
		int foundS1 = objectString.IndexOf(target+"\":\"");
		objectString = objectString.Substring(foundS1+targetLength);
		foundS1 = objectString.IndexOf("\"");
		objectString = objectString.Substring(0,foundS1);
		return objectString;
		// string[] newString = Regex.Split(target, );

		// return newString[1];
	}
}


