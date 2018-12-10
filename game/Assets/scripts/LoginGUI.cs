using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using Pomelo.DotNetClient;
using UnityEngine.UI;

public class LoginGUI : MonoBehaviour {
	public string host;
	public static string hostStatic;
	public static bool connected = false;

	public static string userName = "";
	public static string channel = "1";
	public static JsonObject users = null;
	
	public static PomeloClient pc = null;
	
	public Text username;
	
	//When quit, release resource
	void Update(){
		if(Input.GetKey(KeyCode.Escape)) {
			if (pc != null) {
				pc.disconnect();
			}
			Application.Quit();
		}

		if (connected) {
			connected = false;
			SceneManager.LoadScene (1);
		}
	}
	
	//When quit, release resource
	void OnApplicationQuit(){
		if (pc != null) {
			pc.disconnect();
		}
	}
	
	//Login the chat application and new PomeloClient.
	public void Login() {
		if (username.text != "") {
			hostStatic = host;
			userName = username.text;

			int port = 3014;

			pc = new PomeloClient();

			pc.NetWorkStateChangedEvent += (state) =>
			{
				Debug.Log(state);
			};
				
			pc.initClient(host, port, () =>
			{
				pc.connect(null, data =>
				{
					Debug.Log("on data back");
					Debug.Log(data.ToString());
					JsonObject msg = new JsonObject();
					msg["uid"] = userName;
					pc.request("gate.gateHandler.queryEntry", msg, OnQuery);
				});
			});
		}
	}
	
	public static void OnQuery(JsonObject result)
	{
		if (Convert.ToInt32(result["code"]) == 200)
		{
			pc.disconnect();

			string host = (string)result["host"];
			int port = Convert.ToInt32(result["port"]);
			pc = new PomeloClient();

			pc.NetWorkStateChangedEvent += (state) =>
			{
				Debug.Log(state);
			};

			pc.initClient(host, port, () =>
			{
				pc.connect(null, (data) =>
				{
					Debug.Log("on connect to connector!");

					//Login
					JsonObject msg = new JsonObject();
					msg["username"] = userName;
					msg["rid"] = channel;

					pc.request("connector.entryHandler.enter", msg, OnEnter);
				});
			});
		}
	}

	public static void OnEnter(JsonObject result)
	{
		Console.WriteLine("on login " + result.ToString());
		Debug.Log("on login " + result.ToString());

		users = result;
		connected = true;
	}
 }