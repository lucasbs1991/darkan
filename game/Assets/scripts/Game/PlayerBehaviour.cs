using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJson;
using Pomelo.DotNetClient;
using CnControls;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerBehaviour : MonoBehaviour {

	public int layerMask = ~( (1 << 8) | (1 << 12) ), projectileLayerMask = ~( (1 << 8) | (1 << 9) | (1 << 10) | (1 << 11) );

	private PomeloClient pclient = LoginGUI.pc;
	private string userName = LoginGUI.userName;
	public Feedback feedback;

	public float speed = 1.4f;
	public bool moving = false;

	private string lastDir, area = "1";
	private bool dead = false;
	private Vector3 dir, toPos;

	private Animator anim;
	private SpriteRenderer spriteRenderer;

	Vector3 dirNormalized, target;

	public TextMeshPro playerNameText;

	// Use this for initialization
	void Start () {
		SetPlayerName (userName);
		anim = GetComponent<Animator> ();
		spriteRenderer = GetComponent<SpriteRenderer> ();

		// another player moved
		pclient.on("onMove", (data)=> {
			updatePlayerPos(data);
		});
	}

	// update position from others
	void updatePlayerPos(JsonObject messge) {
		System.Object posx = null, posy = null, toDir = null, fromName = null, targetName = null;
		if (messge.TryGetValue("posx", out posx) && messge.TryGetValue("posy", out posy) && messge.TryGetValue("dir", out toDir) && messge.TryGetValue("from", out fromName) && messge.TryGetValue("target", out targetName)) {
			if(fromName.ToString() != LoginGUI.userName)
				UnityMainThreadDispatcher.Instance().Enqueue(UpdatePlayerPos(fromName.ToString (), posx.ToString(), posy.ToString(), toDir.ToString())); 
		}
	}

	public IEnumerator UpdatePlayerPos(string user, string posx, string posy, string toDir) {
		GameObject go = GameObject.Find(user);
		if (go)
			go.GetComponent<OtherPlayer> ().Move (float.Parse (posx), float.Parse (posy), toDir);
		yield return null;
	}

	void FixedUpdate()
	{
		if (moving || dead)
			return;

		Move ();
	}

	void Move(){
		if (moving) { // moving, just save the desired direction
			if (Input.GetKey (KeyCode.W) || CnInputManager.GetAxis("Vertical") > 0.5f)
				lastDir = "up";
			else if (Input.GetKey (KeyCode.D) || CnInputManager.GetAxis("Horizontal") > 0.5f)
				lastDir = "right";
			else if (Input.GetKey (KeyCode.S) || CnInputManager.GetAxis("Vertical") < -0.5f)
				lastDir = "down";
			else if (Input.GetKey (KeyCode.A) || CnInputManager.GetAxis("Horizontal") < -0.5f)
				lastDir = "left";
			return;
		}

		toPos = Vector3.zero;
		if (Input.GetKey (KeyCode.W) || CnInputManager.GetAxis("Vertical") > 0.5f || lastDir == "up") {
			anim.enabled = true;
			moving = true;
			dir = new Vector3 (0, 1, 0);
			toPos += dir;
			SetAnim ("Up");
			lastDir = "up";
			//network.OnPlayerMove ("up");
		} else if (Input.GetKey (KeyCode.D) || CnInputManager.GetAxis("Horizontal") > 0.5f || lastDir == "right") {
			anim.enabled = true;
			moving = true;
			dir = new Vector3 (1, 0, 0);
			toPos += dir;
			SetAnim ("Side");

			if (spriteRenderer.flipX)
				spriteRenderer.flipX = false;

			lastDir = "right";
			//network.OnPlayerMove ("right");
		} else if (Input.GetKey (KeyCode.S) || CnInputManager.GetAxis("Vertical") < -0.5f || lastDir == "down") {
			anim.enabled = true;
			moving = true;
			dir = new Vector3 (0, -1, 0);
			toPos += dir;
			SetAnim ("Down");
			lastDir = "down";
			//network.OnPlayerMove ("down");
		} else if (Input.GetKey (KeyCode.A) || CnInputManager.GetAxis("Horizontal") < -0.5f || lastDir == "left") {
			anim.enabled = true;
			moving = true;
			dir = new Vector3 (-1, 0, 0);
			toPos += dir;
			SetAnim ("Side");

			if (!spriteRenderer.flipX)
				spriteRenderer.flipX = true;
			
			lastDir = "left";
			//network.OnPlayerMove ("left");
		} else if(lastDir != "") {
			print ("nao esta apertando nada");
			SetAnim ("Idle");

			anim.enabled = false;

			/*if (lastDir == "up")
				spriteRenderer.sprite = sprites[0];
			else if (lastDir == "left") {
				spriteRenderer.sprite = sprites[2];
			} else if (lastDir == "right") {
				spriteRenderer.sprite = sprites[2];
			} else if (lastDir == "down")
				spriteRenderer.sprite = sprites[1];*/

			lastDir = "";
		}

		if (moving)
			VerifyMove ();
	}

	// verify if there is an obstacle
	void VerifyMove(){
		// before walk
		RaycastHit2D hit;
		hit = Physics2D.Raycast (transform.position + dir, dir / 2, 0.1f, layerMask);
		if (hit.collider != null) {
			//print ("hit.collider != null: " + hit.collider.name);
			if (hit.collider.tag == "Wall" || hit.collider.tag == "Player" || hit.collider.tag == "Monster" || hit.collider.tag == "Projectile") {
				feedback.ShowText ("can't move, there is a obstacle");
				moving = false;
				anim.enabled = false;
				lastDir = "";
				toPos = transform.position;

				return;
			}

		}

		// send to server
		SendPosition (area, transform.position.x.ToString (), transform.position.y.ToString (), lastDir);

		StartCoroutine (Moving ());
	}

	IEnumerator Moving(){
		target = new Vector3 (transform.position.x + toPos.x, transform.position.y + toPos.y, 0);
		dirNormalized = (target - transform.position).normalized;

		while(Vector3.Distance(target, transform.position) >= 0.1f){
			transform.position = transform.position + dirNormalized * speed * Time.deltaTime;
			yield return null;
		}

		transform.position = new Vector3(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y), 0);
		moving = false;
		anim.enabled = false;

		/*if (lastDir == "up")
			spriteRenderer.sprite = sprites[0];
		else if (lastDir == "left") {
			spriteRenderer.sprite = sprites[2];
		} else if (lastDir == "right") {
			spriteRenderer.sprite = sprites[2];
		} else if (lastDir == "down")
			spriteRenderer.sprite = sprites[1];*/

		lastDir = "";
	}

	void SetAnim(string name){
		anim.SetBool ("Up", false);
		anim.SetBool ("Side", false);
		anim.SetBool ("Down", false);
		anim.SetBool ("Idle", false);

		anim.SetBool (name, true);
	}

	public void SetPlayerName(string name){
		playerNameText.SetText (name);
	}

	// ========================================== SEND TO SERVER ==========================================

	void SendPosition(string target, string posx, string posy, string lastdir){
		JsonObject message = new JsonObject();
		message.Add("rid", LoginGUI.channel);
		message.Add("posx", posx);
		message.Add("posy", posy);
		message.Add("dir", lastdir);
		message.Add("from", LoginGUI.userName);
		message.Add("target", target);

		pclient.request("area.areaHandler.move", message, (data) => {
			
		});
	}

	public void changeChannel(string newChannel){
		area = newChannel;
		LoginGUI.channel = newChannel;

		JsonObject message = new JsonObject();
		message.Add("area", area);

		//pclient.notify("area.areaHandler.enterScene", message);

		SceneManager.LoadScene (int.Parse(area));
	}
}
