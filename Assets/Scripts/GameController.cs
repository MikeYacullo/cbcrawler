using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityTileMap;
 
public class GameController : MonoBehaviour
{

	public TileMapBehaviour tileMapTerrain;
	public TileMapBehaviour tileMapPlayers;
	public TileMapBehaviour tileMapItems;
	
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
	private List<Enemy>[] levelEnemies;
	private List<Type>[] levelEnemyTypes;
	
	private List<Item> items = new List<Item> ();
	private List<Item>[] levelItems;
	private List<Type>[] levelItemTypes;
	
	int LEVEL_COUNT = 5;
	int ITEMS_PER_LEVEL_COUNT = 1;
	int ENEMIES_PER_LEVEL_COUNT = 10;
	
	//tilemap constants
	Color COLOR_NOTHING = new Color (0, 0, 0, 0);
	int[] mapSpriteVariations;
	int SPRITE_VARIATIONS_COUNT = 2;
	int SPRITE_WIDTH = 24;
	int LAYER_FLOOR_AND_WALLS = 0;
	int LAYER_DOORS_AND_STAIRS = 1;
	int LAYER_VISIBILITY = 2;
	int TILE_FLOORDEFAULT;
	int TILE_DOORCLOSED;
	int TILE_DOOROPEN;
	int TILE_STAIRSUP;
	int TILE_STAIRSDOWN;
	int TILE_SOLIDBLACK;
	
	void Start ()
	{
		NewGame ();
	}
	
	void NewGame ()
	{
		Debug.Log ("new game!");
		gameState = GameState.Initializing;
		camera = GameObject.Find ("Camera").GetComponent<Camera> ();
		
		VOLUME = float.Parse (PlayerPrefs.GetString ("soundLevel"));
		
		tileMapTerrain = (TileMapBehaviour)GameObject.Find ("TileMapTerrain").GetComponent<TileMapBehaviour> ();
		tileMapCharacters = (TileMapBehaviour)GameObject.Find ("TileMapCharacters").GetComponent<TileMapBehaviour> ();
		tileMapItems = (TileMapBehaviour)GameObject.Find ("TileMapItems").GetComponent<TileMapBehaviour> ();
		txtMessages = GameObject.Find ("txtMessages").GetComponent<UnityEngine.UI.Text> ();
		
		var settings = new TileMeshSettings (mapWidth, mapHeight, SPRITE_WIDTH);
		tileMapCharacters.MeshSettings = settings;
		tileMapItems.MeshSettings = settings;
		tileMapTerrain.MeshSettings = settings;
		
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
	
	private void MoveToLevel (int level)
	{
		gameState = GameState.LevelTransition;
		txtTransition.text = "Level " + level.ToString ();
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
		//Debug.Log ("init level " + currentLevel + " complete");
	}

	private void SaveLevelState ()
	{
		if (currentLevel != -1) {
			levelEnemies [currentLevel] = enemies;
		}
	}
	
	void InitItems ()
	{
		if (levelItems [currentLevel] != null) {
			items = levelItems [currentLevel];
		} else {
			for (int i=0; i<ITEMS_PER_LEVEL_COUNT; i++) {
				//pick an item from the list
				Item item = Factory.GetItemForLevel (currentLevel);
				item.Location = map.GetRandomCell (true);
				items.Add (item);
			}
		}
		RenderItems ();
	}

	
	private void RenderItems ()
	{/*
		//clear the whole thing
		for (int w=0; w<mapWidth; w++) {
			for (int h=0; h<mapHeight; h++) {
				if (map.Cells [w, h].Visited) {
					tileMapItems [w, h] = 0;
					//tileMapItems.ClearTile (w, h, 0);
				}
			}
		}
		//draw items
		for (int i=0; i<items.Count; i++) {
			if (map.Cells [items [i].Location.x, items [i].Location.y].Visited) {
				//TODO optimize this!
				int spriteId = tileMapItems.TileSheet.Lookup (items [i].SpriteName);
				tileMapItems [items [i].Location.x, items [i].Location.y] = spriteId;
				//int spriteId = tileMapItems.TileSheet.Lookup (items [i].SpriteName);
				//tileMapItems.SetTile (items [i].Location.x, items [i].Location.y, 0, spriteId);
			}
		}
		//tileMapItems.Build ();
		*/
	}

	void InitMap ()
	{
		/*
		if (previousLevel != -1) {
			levels [previousLevel] = map;
		}*/
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
		//Debug.Log (pc.classType.ToString ());
	}
	
	void InitPlayerCharacter ()
	{
		if (currentLevel > previousLevel) {
			//coming down stairs
			MovePlayerTo (map.entranceLocation);
			Debug.Log ("moving to entrance");
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
		RenderTMCharacters ();
	}	
	
	void InitCharacterSprite ()
	{
		string sex;
		if (pc.IsMale) {
			sex = "_m";
		} else {
			sex = "_f";
		}
		switch (pc.classType) {
		case PlayerCharacter.ClassType.Cleric:
			pcSpriteId = tileMapCharacters.TileSheet.Lookup ("cleric" + sex);
			break;
		case PlayerCharacter.ClassType.Fighter:
			pcSpriteId = tileMapCharacters.TileSheet.Lookup ("fighter" + sex);
			break;
		case PlayerCharacter.ClassType.Rogue:
			pcSpriteId = tileMapCharacters.TileSheet.Lookup ("rogue" + sex);
			break;
		case PlayerCharacter.ClassType.Wizard:
			pcSpriteId = tileMapCharacters.TileSheet.Lookup ("wizard" + sex);
			break;		
		}
		//Debug.Log (pcSpriteId);
	}
	
	void RenderMap ()
	{
		//tileMapTerrain = GameObject.Find ("TileMapTerrain").GetComponent<TileMapBehaviour> ();
		if (tileMapTerrain == null) {
			Debug.Log ("can't find terrain");
		}
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
					//Debug.Log ("Can't find cell type " + map.Cells [w, h].Type.ToString ());
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
		//Debug.Log ("Health:" + healthFraction);
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
		
		//clear current pc cell
		//tileMapCharacters.ClearTile (pc.Location.x, pc.Location.y, 0);
		CheckInput ();
		//draw the character at its location
		tileMapCharacters [pc.Location.x, pc.Location.y] = pcSpriteId;
		//if (pcIsFlipped) {
		//	tileMapCharacters.SetTileFlags (pc.Location.x, pc.Location.y, 0, tk2dTileFlags.FlipX);
		//}
		//tileMapCharacters.Build ();
		RenderItems ();
		
		//camera.transform.position = new Vector3 (pc.Location.x * cameraScaleFactor, pc.Location.y * cameraScaleFactor, -10);	
		Rect pcLoc = tileMapTerrain.GetTileBoundsLocal (pc.Location.x, pc.Location.y);
		camera.transform.position = new Vector3 (pcLoc.center.x, pcLoc.center.y, -10);
	}
	
	private void TakeEnemyTurn ()
	{
		for (int i=0; i<enemies.Count; i++) {
			if (map.DistanceToPlayer (enemies [i].Location) == 1) {
				CombatCheck (enemies [i], pc);
			} else {
				enemies [i].Move (map);
			}
		}
		RenderTMCharacters ();
		RenderItems ();
		UpdateHud ();
		gameState = GameState.TurnPlayer;
	}
	

	private void RenderTMCharacters ()
	{
		/*
		//clear the whole thing
		for (int w=0; w<mapWidth; w++) {
			for (int h=0; h<mapHeight; h++) {
				if (map.Cells [w, h].Visited) {
					tileMapCharacters.PaintTile (w, h, COLOR_NOTHING);
				}
			}
		}
		//draw pc
		tileMapCharacters [pc.Location.x, pc.Location.y] = pcSpriteId;
		if (pcIsFlipped) {
			//tileMapCharacters.SetTileFlags (pc.Location.x, pc.Location.y, 0, tk2dTileFlags.FlipX);
		}
		//draw enemies
		for (int i=0; i<enemies.Count; i++) {
			if (map.Cells [enemies [i].Location.x, enemies [i].Location.y].Visited) {
				//TODO optimize this!
				int spriteId = tileMapCharacters.TileSheet.Lookup (enemies [i].SpriteName);
				tileMapCharacters [enemies [i].Location.x, enemies [i].Location.y] = spriteId;
			}
		}
		*/
	}

	
	private void CheckInput ()
	{
		if (Input.GetKeyDown (KeyCode.LeftBracket) && camera.orthographicSize >= 0.0) {
			camera.orthographicSize -= 0.5f; 
		}
		if (Input.GetKeyDown (KeyCode.RightBracket) && camera.orthographicSize <= 15.0) {
			camera.orthographicSize += 0.5f;
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
							items.Add (loot);
							DisplayMessage (enemies [enemyIndex].Name + " drops " + loot.Name);
						}
						//remove enemy
						map.Cells [enemies [enemyIndex].Location.x, enemies [enemyIndex].Location.y].Passable = true;
						enemies.RemoveAt (enemyIndex);						
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
				items.RemoveAt (itemIndex);
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
		//Debug.Log ("combat check!");
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
		bool newCellsSeen = false;
		List<Address> vTiles = map.findVisibleCellsFlood (new Address (pc.Location.x, pc.Location.y), pc.Stats.VisionRange);
		foreach (Address a in vTiles) {
			//if (!map.Cells [a.x, a.y].Visited) {
			map.Cells [a.x, a.y].Visited = true;
			//tileMapTerrain.ClearTile (a.x, a.y, LAYER_VISIBILITY);
			newCellsSeen = true;
			//}
		}
		//if (newCellsSeen) {
		//tileMapTerrain.Build ();
		//}
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
