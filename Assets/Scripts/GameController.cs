using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityTileMap;
 
public class GameController : MonoBehaviour
{

	private TileMapBehaviour tileMapTerrain;
	private TileMapBehaviour tileMapFOW;

	private GameObject spritePC;
	private Sprite[] texturesPC;
	private Sprite[] texturesNPC;
	private Sprite[] texturesItem;
	
	private Camera camera;

	private Map[] levels;
	private int currentLevel = 0;
	private int previousLevel = -1;
	private Map map;
	
	private enum GameState
	{
		Initializing
		,
		Inventory
		,
		LevelTransition
		,
		TurnPlayer
		,
		TurnEnemyInProgress
		,
		TurnEnemy
		,
		PlayerDeath
	}
	
	private GameState gameState;
	
	private PlayerCharacter pc;
	private int pcSpriteId;
	private TileMapBehaviour tileMapCharacters;
	private bool pcIsFlipped = false;
	
	private UnityEngine.UI.Text txtMessages;
	List<string> messages = new List<string> ();
	int MAX_MESSAGE_COUNT = 5;
	
	private GameObject pnlTransition;
	private UnityEngine.UI.Text txtTransition;
	
	private GameObject pnlInventory;
	
	private float SECONDS_BETWEEN_TURNS = 0.1f;
	
	private UnityEngine.UI.Image health1, health2, health3, health4;
	public Sprite spriteHeartFull;
	public Sprite spriteHeartHalf;
	public Sprite spriteHeartEmpty;

	private int mapWidth = 35, mapHeight = 35;
	
	private int cameraScaleFactor = 24;
	
	public float VOLUME = 1.0f;
	public AudioClip audioStep;
	public AudioClip audioDoor;
	public AudioClip audioHitEnemy;
	public AudioClip audioHitPlayer;
	public AudioClip audioWhiff;
	public AudioClip audioDieEnemy;
	public AudioClip audioLoot;
	
	private List<Enemy> enemies = new List<Enemy> ();
	private List<GameObject> enemySprites = new List<GameObject> ();
	private List<Enemy>[] levelEnemies;
	private List<Type>[] levelEnemyTypes;
	
	private List<Item> items = new List<Item> ();
	private List<GameObject> itemSprites = new List<GameObject> ();
	private List<Item>[] levelItems;
	private List<Type>[] levelItemTypes;
	
	int LEVEL_COUNT = 5;
	int ITEMS_PER_LEVEL_COUNT = 1;
	int ENEMIES_PER_LEVEL_COUNT = 10;
	
	//tilemap constants
	int[] mapSpriteVariations;
	int SPRITE_VARIATIONS_COUNT = 3;
	int SPRITE_WIDTH = 24;

	int TILE_FLOORDEFAULT;
	int TILE_DOORCLOSED;
	int TILE_DOOROPEN;
	int TILE_STAIRSUP;
	int TILE_STAIRSDOWN;
	int TILE_SOLIDBLACK;
	
	//Fog of war viibility percent
	int TILE_FOW_VIS0;
	int TILE_FOW_VIS50;
	int TILE_FOW_VIS100;
	
	int Z_TERRAIN = 4;
	int Z_DECORATION = 3;
	int Z_ITEMS = 2;
	int Z_ACTORS = 1;
	int Z_FOW = 0;
	
	void Start ()
	{
		NewGame ();
	}
	
	void NewGame ()
	{
		gameState = GameState.Initializing;
		camera = GameObject.Find ("Camera").GetComponent<Camera> ();
		
		VOLUME = float.Parse (PlayerPrefs.GetString ("soundLevel"));
		
		tileMapTerrain = (TileMapBehaviour)GameObject.Find ("TileMapTerrain").GetComponent<TileMapBehaviour> ();
		tileMapFOW = (TileMapBehaviour)GameObject.Find ("TileMapFOW").GetComponent<TileMapBehaviour> ();
		var settings = new TileMeshSettings (mapWidth, mapHeight, SPRITE_WIDTH);
		tileMapTerrain.MeshSettings = settings;
		tileMapFOW.MeshSettings = settings;
		TILE_FOW_VIS0 = tileMapFOW.TileSheet.Lookup ("vision0");
		TILE_FOW_VIS50 = tileMapFOW.TileSheet.Lookup ("vision50");
		TILE_FOW_VIS100 = tileMapFOW.TileSheet.Lookup ("vision100");
		
		MoveGameObjectToZLevel (tileMapTerrain.gameObject, Z_TERRAIN);
		MoveGameObjectToZLevel (tileMapFOW.gameObject, Z_FOW);
		
		
		txtMessages = GameObject.Find ("txtMessages").GetComponent<UnityEngine.UI.Text> ();
		
		pnlTransition = GameObject.Find ("pnlTransition");
		txtTransition = GameObject.Find ("txtTransition").GetComponent<UnityEngine.UI.Text> ();
		pnlTransition.SetActive (false);
		
		pnlInventory = GameObject.Find ("pnlInventory");
		pnlInventory.SetActive (false);
		
		health1 = GameObject.Find ("health1").GetComponent<UnityEngine.UI.Image> ();
		health2 = GameObject.Find ("health2").GetComponent<UnityEngine.UI.Image> ();
		health3 = GameObject.Find ("health3").GetComponent<UnityEngine.UI.Image> ();
		health4 = GameObject.Find ("health4").GetComponent<UnityEngine.UI.Image> ();
		levels = new Map[LEVEL_COUNT];
		
		InitTextures ();
		
		mapSpriteVariations = new int[LEVEL_COUNT];
		for (int i=0; i<LEVEL_COUNT; i++) {
			levels [i] = new Map (mapWidth, mapHeight);
			mapSpriteVariations [i] = UnityEngine.Random.Range (0, SPRITE_VARIATIONS_COUNT);
		}
		
		levelEnemies = new List<Enemy>[LEVEL_COUNT];

		levelItems = new List<Item>[LEVEL_COUNT];
			
		DisplayMessage ("Welcome.");
		CreatePlayerCharacter ();
		currentLevel = -1;
		MoveToLevel (0);
	}
	
	void MoveGameObjectToZLevel (GameObject obj, int zLevel)
	{
		obj.transform.position = new Vector3 (obj.transform.position.x, obj.transform.position.y, zLevel);
	}
	
	void InitTextures ()
	{
		texturesPC = Resources.LoadAll<Sprite> ("Textures/Player");
		texturesNPC = Resources.LoadAll<Sprite> ("Textures/NPC");
		texturesItem = Resources.LoadAll<Sprite> ("Textures/Item");
	}
	
	Sprite FindSpriteInTextures (string spriteName, Sprite[] textures)
	{
		string[] names = new string[textures.Length];
		for (int i=0; i<names.Length; i++) {
			if (textures [i].name == spriteName) {
				return textures [i];
				break;
			} 
		}
		return null;
	}
	
	private void MoveToLevel (int level)
	{
		gameState = GameState.LevelTransition;
		txtTransition.text = "Level " + (level + 1).ToString ();
		pnlTransition.SetActive (true);
		StartCoroutine (WaitAndMove (level));
	}
	
	IEnumerator WaitAndMove (int level)
	{
		yield return new WaitForSeconds (1.0f);
		SaveLevelState ();
		previousLevel = currentLevel;
		currentLevel = level;
		InitMap ();
		InitPlayerCharacter ();
		InitEnemies ();
		InitItems ();
		UpdateHud ();
		pnlTransition.SetActive (false);
		gameState = GameState.TurnPlayer;
	}

	private void SaveLevelState ()
	{
		if (currentLevel != -1) {
			levelEnemies [currentLevel] = enemies;
			DestroyEnemySprites ();
			levelItems [currentLevel] = items;
			DestroyItemSprites ();
		}
	}
	
	void InitItems ()
	{
		if (levelItems [currentLevel] != null) {
			items = levelItems [currentLevel];
			MakeItemSprites ();
		} else {
			items = new List<Item> ();
			for (int i=0; i<ITEMS_PER_LEVEL_COUNT; i++) {
				//pick an item from the list
				Item item = Factory.GetItemForLevel (currentLevel);
				item.Location = map.GetRandomCell (true);
				AddItem (item);
			}
		}
		Debug.Log (items.Count + "items");
		Debug.Log (itemSprites.Count + "itemSprites");
	}

	void MakeItemSprites ()
	{
		for (int i=0; i<items.Count; i++) {
			AddItemSprite (items [i]);
		}
	}
	
	void AddItem (Item item)
	{
		items.Add (item);
		AddItemSprite (item);
	}
	
	void AddItemSprite (Item item)
	{
		GameObject sprite = new GameObject ();
		SpriteRenderer sr = sprite.AddComponent<SpriteRenderer> ();
		sr.sprite = FindSpriteInTextures (item.SpriteName, texturesItem);
		MoveGameObjectToZLevel (sprite, Z_ITEMS);
		MoveSpriteTo (sprite, item.Location.x, item.Location.y);
		itemSprites.Add (sprite);
	}
	
	void RemoveItem (int itemIndex)
	{
		items.RemoveAt (itemIndex);
		GameObject.Destroy (itemSprites [itemIndex]);
		itemSprites.RemoveAt (itemIndex);
	}
	
	void DestroyItemSprites ()
	{
		while (itemSprites.Count>0) {
			GameObject.Destroy (itemSprites [0]);
			itemSprites.RemoveAt (0);	
		}
	}

	void InitMap ()
	{
		map = levels [currentLevel];
		RenderMap ();
	}
	
	void CreatePlayerCharacter ()
	{
		string className = PlayerPrefs.GetString ("className");
		bool isMale;
		PlayerCharacter.ClassType classType = PlayerCharacter.ClassType.None;
		string[] classInfo = className.Split ('_');
		if (classInfo [1] == "m") {
			isMale = true;
		} else {
			isMale = false;
		}
		switch (classInfo [0]) {
		case "cleric":
			classType = PlayerCharacter.ClassType.Cleric;
			break;
		case "fighter":
			classType = PlayerCharacter.ClassType.Fighter;
			break;
		case "rogue":
			classType = PlayerCharacter.ClassType.Rogue;
			break;
		case "wizard":
			classType = PlayerCharacter.ClassType.Wizard;
			break;
		default:
			break;
		}
		pc = new PlayerCharacter (classType, isMale);
		InitCharacterSprite ();
	}
	
	void InitPlayerCharacter ()
	{
		if (currentLevel > previousLevel) {
			//coming down stairs
			MovePlayerTo (map.entranceLocation);
		} else {
			//going up stairs
			MovePlayerTo (map.exitLocation);
		}
		SeeTilesFlood ();
	}
	
	void InitEnemies ()
	{
		if (levelEnemies [currentLevel] != null) {
			enemies = levelEnemies [currentLevel];
		} else {
			enemies = new List<Enemy> ();
			for (int i=0; i<ENEMIES_PER_LEVEL_COUNT; i++) {
				Enemy enemy = Factory.GetEnemyForLevel (currentLevel);
				enemy.Location = map.GetRandomCell (true);
				map.Cells [enemy.Location.x, enemy.Location.y].Passable = false;
				if (UnityEngine.Random.Range (0, 100) <= 80) {
					enemy.Loot = Factory.GetItemForLevel (currentLevel);
				}
				enemies.Add (enemy);
			}
		}
		MakeEnemySprites ();
	}	
	
	void MakeEnemySprites ()
	{
		for (int i=0; i<enemies.Count; i++) {
			GameObject enemySprite = new GameObject ();
			SpriteRenderer sr = enemySprite.AddComponent<SpriteRenderer> ();
			sr.sprite = FindSpriteInTextures (enemies [i].SpriteName, texturesNPC);
			MoveSpriteTo (enemySprite, enemies [i].Location.x, enemies [i].Location.y);
			MoveGameObjectToZLevel (enemySprite, Z_ACTORS);
			enemySprites.Add (enemySprite);	
		}
	}
	
	void RemoveEnemy (int enemyIndex)
	{
		enemies.RemoveAt (enemyIndex);	
		GameObject.Destroy (enemySprites [enemyIndex]);
		enemySprites.RemoveAt (enemyIndex);	
	}
	
	void DestroyEnemySprites ()
	{
		while (enemySprites.Count>0) {
			GameObject.Destroy (enemySprites [0]);
			enemySprites.RemoveAt (0);	
		}
	}
	
	void MoveSpriteTo (GameObject sprite, int gridX, int gridY)
	{
		Rect rect = tileMapTerrain.GetTileBoundsLocal (gridX, gridY);
		sprite.transform.position = new Vector3 (rect.center.x, rect.center.y, sprite.transform.position.z);
	}
	
	void InitCharacterSprite ()
	{
		string sex;
		if (pc.IsMale) {
			sex = "_m";
		} else {
			sex = "_f";
		}
		string className = "";
		switch (pc.classType) {
		case PlayerCharacter.ClassType.Cleric:
			className = "cleric";
			break;
		case PlayerCharacter.ClassType.Fighter:
			className = "fighter";
			break;
		case PlayerCharacter.ClassType.Rogue:
			className = "rogue";
			break;
		case PlayerCharacter.ClassType.Wizard:
			className = "wizard";
			break;		
		}
		string spriteName = className + sex;
		
		spritePC = GameObject.Find ("SpritePC");
		spritePC.GetComponent<SpriteRenderer> ().sprite = FindSpriteInTextures (spriteName, texturesPC);
		MoveGameObjectToZLevel (spritePC, Z_ACTORS);
	}
	
	void RenderMap ()
	{
		//init "constants"
		TILE_DOORCLOSED = tileMapTerrain.TileSheet.Lookup ("doorClosed_" + mapSpriteVariations [currentLevel]);
		TILE_DOOROPEN = tileMapTerrain.TileSheet.Lookup ("doorOpen_" + mapSpriteVariations [currentLevel]);
		TILE_STAIRSUP = tileMapTerrain.TileSheet.Lookup ("stairsUp_" + mapSpriteVariations [currentLevel]);
		TILE_STAIRSDOWN = tileMapTerrain.TileSheet.Lookup ("stairsDown_" + mapSpriteVariations [currentLevel]);
		TILE_FLOORDEFAULT = tileMapTerrain.TileSheet.Lookup ("floor0_" + mapSpriteVariations [currentLevel]);
		TILE_SOLIDBLACK = tileMapTerrain.TileSheet.Lookup ("solidBlack");
		//loop through map
		for (int h=0; h<map.Height; h++) {
			for (int w = 0; w<map.Width; w++) {
				//hide the whole map to start
				tileMapTerrain [w, h] = TILE_SOLIDBLACK;
				tileMapFOW [w, h] = TILE_FOW_VIS0;
				switch (map.Cells [w, h].Type) {
				case Map.CellType.Door:
					//all doors start closed
					//tileMapTerrain.SetTile (w, h, LAYER_FLOOR_AND_WALLS, TILE_FLOORDEFAULT);
					tileMapTerrain [w, h] = TILE_DOORCLOSED;
					break;
				case Map.CellType.Entrance:
					//TODO logic here for top level
					//tileMapTerrain.SetTile (w, h, LAYER_FLOOR_AND_WALLS, TILE_FLOORDEFAULT);
					tileMapTerrain [w, h] = TILE_STAIRSUP;
					map.entranceLocation = new Address (w, h);
					break;
				case Map.CellType.Exit:
					//TODO logic here for bottom level
					//tileMapTerrain.SetTile (w, h, LAYER_FLOOR_AND_WALLS, TILE_FLOORDEFAULT);
					tileMapTerrain [w, h] = TILE_STAIRSDOWN;
					map.exitLocation = new Address (w, h);
					break;
				case Map.CellType.Floor:
					//randomize floor title a little
					int floorTag = UnityEngine.Random.Range (0, 3);
					string floorName = "floor" + floorTag;
					int floorId = tileMapTerrain.TileSheet.Lookup (floorName + "_" + mapSpriteVariations [currentLevel]);
					tileMapTerrain [w, h] = floorId;
					break;
				case Map.CellType.Wall:
					string wallName = GetWallTileName (w, h);
					int wallId = tileMapTerrain.TileSheet.Lookup (wallName + "_" + mapSpriteVariations [currentLevel]);
					tileMapTerrain [w, h] = wallId;
					break;
				default:
					tileMapTerrain [w, h] = TILE_SOLIDBLACK;
					break;
				}
			}
		}
		//tileMapCharacters.Build ();
		//tileMapTerrain.Build ();
	}

	// Update is called once per frame
	void Update ()
	{
		switch (gameState) {
		case GameState.TurnPlayer:
			TakePlayerTurn ();
			break;
		case GameState.TurnEnemy:
			gameState = GameState.TurnEnemyInProgress;
			StartCoroutine (WaitForSecondsThenExecute (SECONDS_BETWEEN_TURNS, () => TakeEnemyTurn ()));
			break;
		}
	}
	
	public IEnumerator WaitForSecondsThenExecute (float waitTime, Action method)
	{
		yield return new  WaitForSeconds (waitTime);
		method ();
	}
	
	private void UpdateHud ()
	{
		//update hearts
		float healthFraction = ((float)pc.Stats.CurrentHealth / (float)pc.Stats.MaxHealth);
		if (healthFraction <= 0) {
			health4.sprite = spriteHeartEmpty;
		}
		if (healthFraction >= 0.125f) {
			health4.sprite = spriteHeartHalf;
		}
		if (healthFraction >= 0.25f) {
			health4.sprite = spriteHeartFull;
		}
		if (healthFraction <= 0 + 0.25f) {
			health3.sprite = spriteHeartEmpty;
		}
		if (healthFraction >= 0.125f + 0.25f) {
			health3.sprite = spriteHeartHalf;
		}
		if (healthFraction >= 0.25f + 0.25f) {
			health3.sprite = spriteHeartFull;
		}
		if (healthFraction <= 0 + 0.50f) {
			health2.sprite = spriteHeartEmpty;
		}
		if (healthFraction >= 0.125f + 0.50f) {
			health2.sprite = spriteHeartHalf;
		}
		if (healthFraction >= 0.25f + 0.50f) {
			health2.sprite = spriteHeartFull;
		}
		if (healthFraction <= 0 + 0.75f) {
			health1.sprite = spriteHeartEmpty;
		}
		if (healthFraction >= 0.125f + 0.75f) {
			health1.sprite = spriteHeartHalf;
		}
		if (healthFraction >= 0.25f + 0.75f) {
			health1.sprite = spriteHeartFull;
		}
	}
	
	private void TakePlayerTurn ()
	{
		CheckInput ();
		//draw the character at its location
		Rect pcLoc = tileMapTerrain.GetTileBoundsLocal (pc.Location.x, pc.Location.y);
		spritePC.transform.position = new Vector3 (pcLoc.center.x, pcLoc.center.y, Z_ACTORS);
		if (pcIsFlipped) {
			spritePC.transform.localScale = new Vector3 (-1, 1, 1);
		} else {
			spritePC.transform.localScale = new Vector3 (1, 1, 1);
		}
		camera.transform.position = new Vector3 (pcLoc.center.x, pcLoc.center.y, -10);
	}
	

	
	private void TakeEnemyTurn ()
	{
		for (int i=0; i<enemies.Count; i++) {
			if (map.DistanceToPlayer (enemies [i].Location) == 1) {
				CombatCheck (enemies [i], pc);
			} else {
				enemies [i].Move (map);
				MoveSpriteTo (enemySprites [i], enemies [i].Location.x, enemies [i].Location.y);
			}
		}
		UpdateHud ();
		gameState = GameState.TurnPlayer;
	}
		
	private void CheckInput ()
	{
		
		#region CHEATS		
		bool cheats = true;
		if (cheats) {
		
			if (Input.GetKeyDown (KeyCode.X)) {
				MovePlayerTo (map.exitLocation);
			}
			
			if (Input.GetKeyDown (KeyCode.N)) {
				MovePlayerTo (map.entranceLocation);
			}
		
		}
	
		#endregion
		
		if (Input.GetKeyDown (KeyCode.RightBracket) && camera.orthographicSize >= 0.0) {
			camera.orthographicSize -= 1.0f; 
		}
		if (Input.GetKeyDown (KeyCode.LeftBracket) && camera.orthographicSize <= 15.0) {
			camera.orthographicSize += 1.0f;
		}
		
		if (Input.GetKeyDown (KeyCode.I)) {
			ShowInventory ();
		}
		bool isMoving = false;
		int newX = pc.Location.x, newY = pc.Location.y;
		if (Input.GetKeyDown (KeyCode.D) || Input.GetKeyDown (KeyCode.RightArrow)) {
			newX = newX + 1;
			pcIsFlipped = true;
			isMoving = true;
		} else if (Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.LeftArrow)) {
			newX = newX - 1;
			pcIsFlipped = false;
			isMoving = true;
		} else if (Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.UpArrow)) {
			newY = newY + 1;
			isMoving = true;
		} else if (Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.DownArrow)) {
			newY = newY - 1;
			isMoving = true;
		}
		if (isMoving) { 
			if (IsPassable (newX, newY)) {
				//open a closed door instead of moving in
				if (tileMapTerrain [newX, newY] == TILE_DOORCLOSED) {
					tileMapTerrain [newX, newY] = TILE_DOOROPEN;
					map.Cells [newX, newY].BlocksVision = false;
					audio.PlayOneShot (audioDoor, VOLUME);
				} else if (map.Cells [newX, newY].Type == Map.CellType.Exit && currentLevel != LEVEL_COUNT - 1) {
					MovePlayerTo (new Address (newX, newY));
					map.Cells [pc.Location.x, pc.Location.y].Passable = true;
					MoveToLevel (currentLevel + 1);
				} else if (map.Cells [newX, newY].Type == Map.CellType.Entrance && currentLevel != 0) {
					MovePlayerTo (new Address (newX, newY));
					map.Cells [pc.Location.x, pc.Location.y].Passable = true;
					MoveToLevel (currentLevel - 1);
				} else {
					MovePlayerTo (new Address (newX, newY));
					audio.PlayOneShot (audioStep, VOLUME);
				}
				SeeTilesFlood ();
			} else {
				//combat?
				//does the square contain an enemy?
				int enemyIndex = EnemyAt (new Address (newX, newY));
				if (enemyIndex != -1) {
					CombatCheck (pc, enemies [enemyIndex]);
					//TODO encapsulate
					if (enemies [enemyIndex].Stats.CurrentHealth <= 0) {
						audio.PlayOneShot (audioDieEnemy, VOLUME);
						//drop loot
						Item loot = enemies [enemyIndex].Loot;
						if (loot != null) {
							loot.Location = new Address (enemies [enemyIndex].Location.x, enemies [enemyIndex].Location.y);
							AddItem (loot);
							DisplayMessage (enemies [enemyIndex].Name + " drops " + loot.Name);
						}
						//remove enemy
						map.Cells [enemies [enemyIndex].Location.x, enemies [enemyIndex].Location.y].Passable = true;
						RemoveEnemy (enemyIndex);	
					}
				}
			}
			UpdateHud ();
			gameState = GameState.TurnEnemy;
		}
	}
	
	private void GetLoot (Address location)
	{
		//any loot to pick up?
		int itemIndex = ItemAt (new Address (location.x, location.y));
		if (itemIndex != -1) {
			if (pc.AddToInventory (items [itemIndex])) {
				RemoveItem (itemIndex);
				audio.PlayOneShot (audioLoot, VOLUME);
				DisplayMessage ("Picked up " + pc.Inventory [pc.Inventory.Count - 1].Name);
			}
		}
	}
	
	private int EnemyAt (Address location)
	{
		int enemyIndex = -1;
		for (int i=0; i<enemies.Count; i++) {
			if (enemies [i].Location.x == location.x && enemies [i].Location.y == location.y) {
				enemyIndex = i;
				break;
			}
		}	
		return enemyIndex;
	}
	
	private int ItemAt (Address location)
	{
		int itemIndex = -1;
		for (int i=0; i<items.Count; i++) {
			if (items [i].Location.x == location.x && items [i].Location.y == location.y) {
				itemIndex = i;
				break;
			}
		}	
		return itemIndex;
	}
	
	private void MovePlayerTo (Address newLocation)
	{
		//old cell can now be walked through
		map.Cells [pc.Location.x, pc.Location.y].Passable = true;
		pc.Location = newLocation;
		//block character's current location
		map.Cells [pc.Location.x, pc.Location.y].Passable = false;
		map.pcLocation = newLocation;
		GetLoot (newLocation);
	}
	
	private void CombatCheck (Actor attacker, Actor defender)
	{
		//TODO make actual combat system
		DisplayMessage (attacker.Name + " attacks " + defender.Name + "...");
		if (UnityEngine.Random.Range (1, 10) > 4) {
			//hit
			int damage = UnityEngine.Random.Range (1, attacker.Stats.AttackMaxDamage + 1);
			DisplayMessage (defender.Name + " is hit for " + damage + " damage!");
			defender.Stats.CurrentHealth -= damage;
			if (attacker.GetType ().ToString () == "PlayerCharacter") {
				audio.PlayOneShot (audioHitEnemy, VOLUME);
			} else {
				audio.PlayOneShot (audioHitPlayer, VOLUME);
			}
		} else {
			//miss
			DisplayMessage (attacker.Name + " misses!");
			audio.PlayOneShot (audioWhiff, 0.3f);
		}
	}
	
	private void DisplayMessage (string messageText)
	{
		//TODO eventually this will be displayed in the HUD
		Debug.Log (messageText);
		messages.Add (messageText);
		if (messages.Count > MAX_MESSAGE_COUNT) {
			messages.RemoveAt (0);
		}
		txtMessages.text = "";
		for (int i = 0; i<messages.Count; i++) {
			txtMessages.text += "\n" + messages [i];
		}
	}
	
	private bool IsPassable (int newX, int newY)
	{
		if (map.Cells [newX, newY].Passable) {
			return true;
		}
		return false;
	}
	
	private string GetWallTileName (int x, int y)
	{
		int goesN, goesE, goesS, goesW;
		//check each direction
		goesN = WallGoes (x, y + 1); //1
		goesE = WallGoes (x + 1, y);//2
		goesS = WallGoes (x, y - 1);//4
		goesW = WallGoes (x - 1, y);//8
		int wallNum = goesN + (goesE * 2) + (goesS * 4) + (goesW * 8);
		switch (wallNum) {
		case 0:
			return "pillar0";
		case 1:
			return "wallN";
		case 2:
			return "wallE";
		case 3:
			return "wallNE";
		case 4:
			return "wallS";
		case 5:
			return "wallNS";
		case 6:
			return "wallES";
		case 7:
			return "wallNES";
		case 8:
			return "wallW";
		case 9:
			return "wallNW";
		case 10:
			return "wallEW";
		case 11:
			return "wallNEW";
		case 12:
			return "wallSW";
		case 13:
			return "wallNSW";
		case 14:
			return "wallESW";
		case 15:
			return "wallNESW";			
		default:
			return "pillar1";			
		}
		
	}
	
	private int WallGoes (int x, int y)
	{
		if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) {
			return 0;
		} else if (map.Cells [x, y].Type == Map.CellType.Wall) {
			return 1;
		} else {
			return 0; 
		}
	}
	
	private void SeeTilesFlood ()
	{
		List<Address> vTiles = map.findVisibleCellsFlood (new Address (pc.Location.x, pc.Location.y), pc.Stats.VisionRange);
		foreach (Address a in vTiles) {
			if (!map.Cells [a.x, a.y].Visited) {
				map.Cells [a.x, a.y].Visited = true;
				tileMapFOW [a.x, a.y] = TILE_FOW_VIS100;
			}
		}
	}
	
	public void ShowInventory ()
	{
		gameState = GameState.Inventory;
		pnlInventory.SetActive (true);
		InventoryController ic = (InventoryController)pnlInventory.GetComponentInChildren<InventoryController> ();
		ic.RenderInventory (pc.Inventory);
	}
	
	public void HideInventory ()
	{
		pnlInventory.SetActive (false);
		gameState = GameState.TurnPlayer;
	}

	
	
	
}
