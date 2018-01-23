﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

	// Stats
	int maxhealth = 100;
	int health = 100;
	int protection = 0;
	public float moveSpeed = 5;

	// Attack
	private float attackCooldown = 0.3f;
	private float attackCooldownCounter;
	private float attackDuration = 0.2f;
	private bool isAttacking = false;

	public bool madeLoudSound = false;
	private float madeLoudSoundDuration = 1f;
	private float madeLoudSoundCounter;

	// Physics
	public Rigidbody2D pRigidbody;

	// Animation
	private Animator animator;
	private bool playerMoving;
	private bool playerAttacking;
	private Vector2 lastMove;
	private static bool playerExists = false;
	private SpriteRenderer pSpriteRenderer;
	private SpriteRenderer armorSpriteRenderer;
	private SpriteRenderer pantsSpriteRenderer;
	private SpriteRenderer bootsSpriteRenderer;

	// World

	public Grid grid;

	// UI and menus
	public int aspectRatioX = 16;
	public int aspectRatioY = 9;
	private bool isPathChecking = false;

	GameObject overlay;
	AttackArc player_attack_arc;

	// Inventory
	public List<Item> inventory = new List<Item> ();

	public Item weapon = GlobalData.punch;
	public Item offhand = GlobalData.empty_offhand;
	public Item helmet = GlobalData.naked_head;
	public Item bodyarmor = GlobalData.naked_body;
	public Item pants = GlobalData.naked_legs;
	public Item boots = GlobalData.naked_feet;


//	List<Tile> waypoints;
//	Vector3 nextWaypoint;

// Health system

	public int Health {
		set {
			int newhealth = value;
			if (newhealth < health) {
				StartCoroutine (registerPlayerHit ());
				health = Mathf.Max (value, 0);
			} else {
				health = Mathf.Min (value, maxhealth);
			}
		}

		get {
			return health;
		}
	}

	public int MaxHealth {
		set {
			maxhealth = value;
		}

		get {
			return maxhealth;
		}
	}

	public int Protection {
		get {
			return protection;
		}

		set {
			if (value < 0) {
				value = 0;
			}
			protection = value;
		}

	}

	IEnumerator registerPlayerHit() {
		Image oImage = overlay.GetComponent<Image> ();
		Color c = Color.red;
		c.a = 0.2f;
		oImage.color = c;
		overlay.SetActive (true);
		yield return new WaitForSeconds (attackDuration);
		overlay.SetActive (false);
		oImage.color = Color.white;
	}


// GUI

	void DrawRelative () {
		pSpriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);

		if (bodyarmor != GlobalData.naked_body) {
			armorSpriteRenderer.enabled = true;
			armorSpriteRenderer.sortingOrder = pSpriteRenderer.sortingOrder + 1;
		} else {
			armorSpriteRenderer.enabled = false;
		}

		if (pants != GlobalData.naked_legs) {
			pantsSpriteRenderer.enabled = true;
			pantsSpriteRenderer.sortingOrder = pSpriteRenderer.sortingOrder + 1;
		} else {
			pantsSpriteRenderer.enabled = false;
		}

		if (boots != GlobalData.naked_feet) {
			bootsSpriteRenderer.enabled = true;
			bootsSpriteRenderer.sortingOrder = pSpriteRenderer.sortingOrder + 2;
		} else {
			bootsSpriteRenderer.enabled = false;
		}
	}

	public bool IsPathChecking {
		get {
			return isPathChecking;
		}

		set {
			isPathChecking = value;
			if (IsPathChecking) {
				foreach (GameObject tileobject in GameObject.FindGameObjectsWithTag("tile_go")) {
					tileobject.transform.Find("cell").gameObject.GetComponent<SpriteRenderer> ().enabled = true;
				}
			} else {
				foreach (GameObject tileobject in GameObject.FindGameObjectsWithTag("tile_go")) {
					tileobject.transform.Find("cell").gameObject.GetComponent<SpriteRenderer> ().enabled = false;
				}
			}

		}
	}

// COOLDOWN ACTIONS

	public void Cooldown () {
		attackCooldownCounter -= Time.deltaTime;
		if (attackCooldownCounter <= 0) {
			isAttacking = false;
		} else {
		}
		madeLoudSoundCounter -= Time.deltaTime;
		if (madeLoudSoundCounter < 0) {
			madeLoudSound = false;
		}
	}

// CONTROLS 

	public void AltHighLight () {

		if (GlobalData.debugmode) {

			if (Input.GetKeyDown (KeyCode.LeftAlt)) {
				IsPathChecking = true;
			}

			if (Input.GetKeyUp (KeyCode.LeftAlt)) {
				IsPathChecking = false;
			}
		}
	}

	// TODO remove/hide the below function which currently serve to give items for free, for testing purposes

	public void testFreeStuff () { 
		if (Input.GetKeyDown (KeyCode.H)) {
			Item newItem = new Item(Item.Type.Weapon, "Baseball bat");// actually gives baseball bat
			Item newItem2 = new Item(Item.Type.Weapon, "Sword");// actually gives sword
			Item newItem3 = new Item(Item.Type.Offhand, "Riot shield");// actually gives riot shield
			Item newItem4 = new Item(Item.Type.Helmet, "Army helmet");// actually gives army helmet
			Item newItem5 = new Item(Item.Type.Bodyarmor, "Kevlar vest");// actually gives kevlar vest
			Item newItem6 = new Item(Item.Type.Consumable, "MedKit");//GlobalData.MedKit;
			Item newItem7 = new Item(Item.Type.Pants, "Camo pants");// actually gives camo pants
			Item newItem8 = new Item(Item.Type.Boots, "Leather boots");// actually gives leather boots
			getItem (newItem);
			getItem (newItem2);
			getItem (newItem3);
			getItem (newItem4);
			getItem (newItem5);
			getItem (newItem6);
			getItem (newItem7);
			getItem (newItem8);
		}
	}

	public void listInventory () {
		if (Input.GetKeyDown (KeyCode.K)) {
			if (inventory.Count > 0) {
				for (int i = 0; i < inventory.Count; i++) {
					Debug.Log ("Inventory object in slot " + i + ": " + inventory [i].name);
				}
			} else {
				Debug.Log ("Inventory is empty");
			}
			Debug.Log ("Protection value is: " + protection);
		}
	}


	public void KeyboardControls () {
		// Extra in-game real-time keyboard controls will go here
	}

	public void MouseControls() {
		if (Input.GetKeyDown (KeyCode.Mouse0)) {
			if (attackCooldownCounter < 0) {
				Vector2 mousePosition = new Vector2 (Input.mousePosition.x / Screen.width * aspectRatioX, Input.mousePosition.y / Screen.height * aspectRatioY);
				Vector2 centerPosition = new Vector2 (8.0f, 4.5f);
				Vector3 spawnPosition = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
				Quaternion direction = Quaternion.LookRotation (Vector3.forward, centerPosition - mousePosition);
				Vector3 offset = direction * (new Vector3 (0f, 0f, 0f));
				//Debug.Log (mousePosition.x + " " + mousePosition.y); // checking mouse position at click

				AttackArc sweep = Instantiate (player_attack_arc, spawnPosition - offset, direction);

				sweep.origin = "player";
				sweep.TTL = 0.1f;
				sweep.damage = Random.Range (weapon.mindamage, weapon.maxdamage + 1);
				sweep.stunfactor = weapon.stunfactor;
				sweep.isAOE = weapon.isAOE;
				attackCooldownCounter = attackCooldown;
				isAttacking = true;
			}
		}

		if (Input.GetKeyDown (KeyCode.Mouse1)) {
			madeLoudSound = true;
			madeLoudSoundCounter = madeLoudSoundDuration;
		}
	}

/*	public void ClickToPath (Grid grid) {

		if (Input.GetKeyDown (KeyCode.Mouse1)) {
			Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
			Vector3 mouseInWorld = Camera.main.ScreenToWorldPoint (mousePosition);
			//Debug.Log (mouseInWorld.x + " and " + mouseInWorld.y);

			int playerX = (int)Mathf.Floor (transform.position.x);
			int playerY = (int)Mathf.Floor (transform.position.y);

			int mouseX = (int)Mathf.Floor (mouseInWorld.x);
			int mouseY = (int)Mathf.Floor (mouseInWorld.y);

			AStar astar = new AStar ();
			waypoints = astar.FindPath(grid.GetTileAt(playerX, playerY), grid.GetTileAt(mouseX, mouseY), grid, false);
			//waypoints.Remove (waypoints [0]);

		}
	}*/

	public void MovePlayerToPosition (Vector3 position) {
			Quaternion direction = Quaternion.LookRotation (Vector3.forward, position - transform.position);
			pRigidbody.velocity = direction * Vector3.up * moveSpeed;
	}

	public void MovementControls () {
		playerMoving = false;

		if (!isAttacking) {
			if (Input.GetAxisRaw ("Horizontal") > 0.5f || Input.GetAxisRaw ("Horizontal") < -0.5f) {
				//transform.Translate(new Vector3(Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime, 0f, 0f));
				// We use physics velocity for movement to avoid "jerky bump" into colliders
				pRigidbody.velocity = new Vector2 (Input.GetAxisRaw ("Horizontal") * moveSpeed, pRigidbody.velocity.y);
				playerMoving = true;
				lastMove = new Vector2 (Input.GetAxisRaw ("Horizontal"), 0f);
			}

			if (Input.GetAxisRaw ("Vertical") > 0.5f || Input.GetAxisRaw ("Vertical") < -0.5f) {
				//transform.Translate(new Vector3(0f, Input.GetAxisRaw("Vertical") * moveSpeed * Time.deltaTime, 0f));
				// We use physics velocity for movement to avoid "jerky bump" into colliders
				pRigidbody.velocity = new Vector2 (pRigidbody.velocity.x, Input.GetAxisRaw ("Vertical") * moveSpeed);
				playerMoving = true;
				lastMove = new Vector2 (0f, Input.GetAxisRaw ("Vertical"));
			}

			if (Input.GetAxisRaw ("Horizontal") < 0.5f && Input.GetAxisRaw ("Horizontal") > -0.5) {
				pRigidbody.velocity = new Vector2 (0f, pRigidbody.velocity.y);
			}

			if (Input.GetAxisRaw ("Vertical") < 0.5f && Input.GetAxisRaw ("Vertical") > -0.5) {
				pRigidbody.velocity = new Vector2 (pRigidbody.velocity.x, 0f);
			}
		} else {
			pRigidbody.velocity = new Vector2 (0f, 0f); // Do not move while attacking
		}

		animator.SetFloat("MoveX", Input.GetAxisRaw("Horizontal"));
		animator.SetFloat("MoveY", Input.GetAxisRaw("Vertical"));
		animator.SetBool("IsMoving", playerMoving);
		animator.SetFloat("LastMoveX", lastMove.x);
		animator.SetFloat("LastMoveY", lastMove.y);

	}

	public void PauseControls () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			GlobalData.paused = !GlobalData.paused;
			GlobalData.inventoryON = !GlobalData.inventoryON;
			GlobalData.groundSlotOffset = 0;
		}
	}

// Grid interaction
	public Tile GetPlayerTile (Grid grid) {
		int x = (int)Mathf.Floor (transform.position.x);
		int y = (int)Mathf.Floor (transform.position.y);

		return grid.GetTileAt (x, y);		
	}

// Inventory section //

	// Equipment buttons, remove an item from equipment and put it into inventory
	public void EquipmentButtonClicked (string slotname) {
		//Debug.Log ("Interacting with an item in slot " + slotname);
		UnequipItemBySlotname (slotname);
	}

	// Inventory buttons, remove an item from inventory and put it on (in equipment slots)
	public void InventoryButtonClicked (int index) {
		//Debug.Log ("Interacting with an index " + index);
		if (inventory [index].type == Item.Type.Consumable) {
			inventory [index].Use(this, index);
		} else {
			equipItemByIndex (index);
		}
	}

	// Red cross button next to equipment/inventory slots, drops an item onto the ground
	void DropItemOnGround (Item item) {
		Tile currenttile = GetPlayerTile(grid);
		currenttile.groundItems.Add (item);
		if (currenttile.groundItems.Count > 0) {
			GameObject.Find ("Tile_" + currenttile.X + "_" + currenttile.Y).GetComponent <SpriteRenderer> ().enabled = true;
		} else {
			GameObject.Find ("Tile_" + currenttile.X + "_" + currenttile.Y).GetComponent <SpriteRenderer> ().enabled = false;
		}
	}

	// Ground inventory handling

	// Increase/decrease of a global counter to display items on the ground, because there's only 5 slots
	public void DecreaseGroundSlotdOffset () {
		GlobalData.groundSlotOffset = Mathf.Max (0, GlobalData.groundSlotOffset - 1);
	}

	public void IncreaseGroundSlotOffset () {
		int numItemsOnGround = GetPlayerTile (grid).groundItems.Count;

		GlobalData.groundSlotOffset = Mathf.Min (numItemsOnGround-5, GlobalData.groundSlotOffset + 1);
		if (GlobalData.groundSlotOffset < 0) {
			GlobalData.groundSlotOffset = 0;
		}
	}

	// Ground slot buttons to pickup items from the ground

	public void PickupItemsByIndex (int index) {
		if (inventory.Count < GlobalData.inventorySize) {
			Tile currenttile = GetPlayerTile(grid);
			int realindex = index + GlobalData.groundSlotOffset;

			if (currenttile.groundItems.Count > realindex) {
				if (currenttile.groundItems.Count > 0) {
					inventory.Add (currenttile.groundItems [realindex]);
					currenttile.groundItems.Remove (currenttile.groundItems [realindex]);
					DecreaseGroundSlotdOffset();
				}
				if (currenttile.groundItems.Count == 0) {
					GameObject.Find ("Tile_" + currenttile.X + "_" + currenttile.Y).GetComponent <SpriteRenderer> ().enabled = false;
				}
			} else {
				Debug.LogError ("Tried to reference an index outside of number of items on the ground in PickupItemsByIndex: " + realindex + ", actual number of items on the ground: " + currenttile.groundItems.Count);
			}

		} else {
			Debug.Log ("Inventory is full!");
		}
	}

	public void dropItemByIndex (int index) {
		Debug.Log ("Dropping item: " + inventory [index].name);
		DropItemOnGround (inventory [index]);
		inventory.Remove (inventory[index]);
	}

	public void dropItemBySlotname (string slotname) {

		switch (slotname) {

		case "helmet": 
			DropItemOnGround (helmet);
			Debug.Log ("Dropping item from equipment slot: " + slotname + ", item name: " + helmet.name);
			Protection -= helmet.protection;
			helmet = GlobalData.naked_head;
			break;
		case "bodyarmor":
			DropItemOnGround (bodyarmor);
			Debug.Log ("Dropping item from equipment slot: " + slotname + ", item name: " + bodyarmor.name);
			Protection -= bodyarmor.protection;
			bodyarmor = GlobalData.naked_body;
			break;
		case "pants":
			DropItemOnGround (pants);
			Debug.Log ("Dropping item from equipment slot: " + slotname + ", item name: " + pants.name);
			Protection -= pants.protection;
			pants = GlobalData.naked_legs;
			break;
		case "boots":
			DropItemOnGround (boots);
			Debug.Log ("Dropping item from equipment slot: " + slotname + ", item name: " + boots.name);
			Protection -= boots.protection;
			boots = GlobalData.naked_feet;
			break;
		case "weapon":
			DropItemOnGround (weapon);
			Debug.Log ("Dropping item from equipment slot: " + slotname + ", item name: " + weapon.name);
			Protection -= weapon.protection;
			weapon = GlobalData.punch;
			break;
		case "offhand":
			DropItemOnGround (offhand);
			Debug.Log ("Dropping item from equipment slot: " + slotname + ", item name: " + offhand.name);
			Protection -= offhand.protection;
			offhand = GlobalData.empty_offhand;
			break;
		}
	}

	void equipItemByIndex (int index) {
		if (inventory[index].isEquippable) {
			Debug.Log ("Equipped item from inventory: " + inventory[index].name);

			Protection += inventory [index].protection;

			if (inventory [index].type == Item.Type.Weapon) {
				if (weapon != GlobalData.punch) {
					if (UnequipItem (weapon)) {
						weapon = inventory [index];
						Debug.Log ("Equipped new weapon: " + weapon.name);
						Debug.Log ("Removing item " + inventory [index].name + " from inventory");
						inventory.Remove (inventory [index]);
					}
				} else {
					weapon = inventory [index];
					Debug.Log ("Equipped new weapon: " + weapon.name);
					Debug.Log ("Removing item " + inventory [index].name + " from inventory");
					inventory.Remove (inventory [index]);
				}
			} else if (inventory [index].type == Item.Type.Offhand) {
				if (offhand != GlobalData.empty_offhand) {
					if (UnequipItem (offhand)) {
						offhand = inventory [index];
						Debug.Log ("Equipped new offhand: " + offhand.name);
						Debug.Log ("Removing item " + inventory [index].name + " from inventory");
						inventory.Remove (inventory [index]);
					}
				} else {
					offhand = inventory [index];
					Debug.Log ("Equipped new offhand: " + offhand.name);
					Debug.Log ("Removing item " + inventory [index].name + " from inventory");
					inventory.Remove (inventory [index]);
				}
			} else if (inventory [index].type == Item.Type.Helmet) {
				if (helmet != GlobalData.naked_head) {
					if (UnequipItem (helmet)) {
						helmet = inventory [index];
						Debug.Log ("Equipped new helmet: " + helmet.name);
						Debug.Log ("Removing item " + inventory [index].name + " from inventory");
						inventory.Remove (inventory [index]);
					}
				} else {
					helmet = inventory [index];
					Debug.Log ("Equipped new helmet: " + helmet.name);
					Debug.Log ("Removing item " + inventory [index].name + " from inventory");
					inventory.Remove (inventory [index]);
				}
			} else if (inventory [index].type == Item.Type.Bodyarmor) {
				if (bodyarmor != GlobalData.naked_body) {
					if (UnequipItem (bodyarmor)) {
						bodyarmor = inventory [index];
						Debug.Log ("Equipped new bodyarmor: " + bodyarmor.name);
						Debug.Log ("Removing item " + inventory [index].name + " from inventory");
						inventory.Remove (inventory [index]);
					}
				} else {
					bodyarmor = inventory [index];
					Debug.Log ("Equipped new bodyarmor: " + bodyarmor.name);
					Debug.Log ("Removing item " + inventory [index].name + " from inventory");
					inventory.Remove (inventory [index]);
				}
			} else if (inventory [index].type == Item.Type.Pants) {
				if (pants != GlobalData.naked_legs) {
					if (UnequipItem (pants)) {
						pants = inventory [index];
						Debug.Log ("Equipped new pants: " + pants.name);
						Debug.Log ("Removing item " + inventory [index].name + " from inventory");
						inventory.Remove (inventory [index]);
					}
				} else {
					pants = inventory [index];
					Debug.Log ("Equipped new pants: " + pants.name);
					Debug.Log ("Removing item " + inventory [index].name + " from inventory");
					inventory.Remove (inventory [index]);
				}
			} else if (inventory [index].type == Item.Type.Boots) {
				if (boots != GlobalData.naked_feet) {
					if (UnequipItem (boots)) {
						boots = inventory [index];
						Debug.Log ("Equipped new boots: " + boots.name);
						Debug.Log ("Removing item " + inventory [index].name + " from inventory");
						inventory.Remove (inventory [index]);
					}
				} else {
					boots = inventory [index];
					Debug.Log ("Equipped new boots: " + boots.name);
					Debug.Log ("Removing item " + inventory [index].name + " from inventory");
					inventory.Remove (inventory [index]);
				}
			} else {
				Debug.LogError ("Attempted to equip an item marked as 'equippable' and yet it's none of the known equippable item types: Weapon, Offhand, Helmet, bodyarmor, Pants, Boots. Item name: " + inventory[index].name);
			}
		} else {
			Debug.Log ("Item " + inventory[index].name + " cannot be equipped due its nature.");
		}
	}

	void getItem (Item item) {
		if (inventory.Count < GlobalData.inventorySize) {
			inventory.Add (item);
			Debug.Log ("Received an item in inventory: " + item.name);
		} else {
			Debug.Log ("Inventory is full");
			DropItemOnGround (item);
		}
	}

	bool UnequipItem (Item item) {
		bool success = false;
		if (inventory.Count < GlobalData.inventorySize) {
			Debug.Log ("Unequipping and adding to inventory: " + item.name);
			inventory.Add(item);
			Protection -= item.protection;

			if (item.type == Item.Type.Weapon) {
				weapon = GlobalData.punch;
			}

			if (item.type == Item.Type.Offhand) {
				offhand = GlobalData.empty_offhand;
			}


			if (item.type == Item.Type.Helmet) {
				helmet = GlobalData.naked_head;
			}

			if (item.type == Item.Type.Bodyarmor) {
				bodyarmor = GlobalData.naked_body;
			}

			if (item.type == Item.Type.Pants) {
				pants = GlobalData.naked_legs;
			}


			if (item.type == Item.Type.Boots) {
				boots = GlobalData.naked_feet;
			}
			success = true;
		} else {
			success = false;
			Debug.Log("Inventory is full, cannot unequip! Free up some inventory space first.");
		}
		return success;
	}

	void UnequipItemBySlotname (string slotname) {
		Item item = null;

		if (inventory.Count < GlobalData.inventorySize) {

			switch (slotname) {
			case "helmet":
				item = helmet;
				break;
			case "bodyarmor":
				item = bodyarmor;
				break;
			case "pants":
				item = pants;
				break;
			case "boots":
				item = boots;
				break;
			case "weapon":
				item = weapon;
				break;
			case "offhand":
				item = offhand;
				break;
			}

			if (item.type == Item.Type.Weapon) {
				weapon = GlobalData.punch;
				Debug.Log ("Unequipping and adding to inventory: " + item.name);
				inventory.Add (item);
			}

			if (item.type == Item.Type.Offhand) {
				offhand = GlobalData.empty_offhand;
				Debug.Log ("Unequipping and adding to inventory: " + item.name);
				inventory.Add (item);
			}


			if (item.type == Item.Type.Helmet) {
				helmet = GlobalData.naked_head;
				Debug.Log ("Unequipping and adding to inventory: " + item.name);
				inventory.Add (item);
			}

			if (item.type == Item.Type.Bodyarmor) {
				bodyarmor = GlobalData.naked_body;
				Debug.Log ("Unequipping and adding to inventory: " + item.name);
				inventory.Add (item);
			}

			if (item.type == Item.Type.Pants) {
				pants = GlobalData.naked_legs;
				Debug.Log ("Unequipping and adding to inventory: " + item.name);
				inventory.Add (item);
			}


			if (item.type == Item.Type.Boots) {
				boots = GlobalData.naked_feet;
				Debug.Log ("Unequipping and adding to inventory: " + item.name);
				inventory.Add (item);
			}


			Protection -= item.protection;
		} else {
			Debug.Log("Inventory is full, cannot unequip! Free up some inventory space first.");
		}
	}

// Awake/Start/update functions

	void Start () {
		// Inventory
		/*
		for (int i = 0; i < 25; i++) {
			Item emptyItem = GlobalData.nothing;
			inventory.Add(emptyItem);
			Debug.Log (emptyItem.name + " added to inventory to slot " + i);
		}

		for (int i = 0; i < 25; i++) {
			Debug.Log ("Item in inventory slot " + i + ": " + inventory [i].name);
		}*/

		// GUI
		overlay = GameObject.Find ("Hiteffect");
		overlay.SetActive(false);

		// Pathfinding
//		waypoints = new List<Tile>();

		// Attack arcs
		player_attack_arc = Resources.Load<AttackArc> ("Prefabs/UI/player-attack-arc");

		// Animation/physics
		animator = GetComponent<Animator>();
		pRigidbody = GetComponent<Rigidbody2D> ();
		pSpriteRenderer = GetComponent<SpriteRenderer> ();
		armorSpriteRenderer = transform.Find("Armor").gameObject.GetComponent <SpriteRenderer> ();
		pantsSpriteRenderer = transform.Find("Pants").gameObject.GetComponent <SpriteRenderer> ();
		bootsSpriteRenderer = transform.Find("Boots").gameObject.GetComponent <SpriteRenderer> ();

		// Do not destroy player after ot was created, do not create new players
		if (!playerExists) {
			playerExists = true;
			DontDestroyOnLoad (transform.gameObject);
		} else {
			Destroy (gameObject);
		}

		
	}

	void Update ()
	{
		DrawRelative ();
		if (health <= 0) {
			this.gameObject.SetActive(false);
		}
		/*
		if (waypoints.Count > 0) {
			nextWaypoint = new Vector3 (waypoints [0].X + 0.5f, waypoints[0].Y + 0.5f, 0f);
			if (nextWaypoint != null) {
				if (Vector3.Distance (transform.position, nextWaypoint) > 0.05) {
					MovePlayerToPosition (nextWaypoint);
				} else {
					waypoints.Remove (waypoints [0]);
					pRigidbody.velocity = new Vector2 (0f, 0f);
				}
			}
		}*/
	}

}
