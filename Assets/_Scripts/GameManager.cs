using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityTileMap;
 
public class GameManager : MonoBehaviour
{

	public AudioManager audioManager;
	public ProjectileManager projectileManager;

	public TileMapBehaviour tileMapTerrain;
	public TileMapBehaviour tileMapFOW;

	public GameObject actorVizPrefab;

	private GameObject spritePC;
	private Sprite[] texturesPC;
	private Sprite[] texturesNPC;
	public Sprite[] texturesItem;
	private Sprite[] texturesItemDecor;
	public Sprite[] texturesProjectile;
	
	private Camera camera;

	private Map[] levels;
	private int currentLevel = 0;
	private int previousLevel = -1;
	public Map map;
	
	public enum GameState
	{
		EnemiesShooting,
		Initializing,
		Inventory,
		LevelTransition,
		TurnPlayer,
		TurnPlayerInProgress,
		TurnEnemy,
		TurnEnemyInProgress,
		PlayerDeath
	}
	
	public GameState gameState;
	
	public PlayerCharacter pc;
	private int pcSpriteId;
	private TileMapBehaviour tileMapCharacters;
	private bool pcIsFlipped = false;
	private bool pcIsPathfinding = false;
	Stack<AStar.Node> pathPC;
	
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
	
	public List<Enemy> enemies = new List<Enemy> ();
	private List<GameObject> enemySprites = new List<GameObject> ();
	private List<Enemy>[] levelEnemies;
	private List<Type>[] levelEnemyTypes;
	
	private List<Item> items = new List<Item> ();
	private List<GameObject> itemSprites = new List<GameObject> ();
	private List<Item>[] levelItems;
	private List<Type>[] levelItemTypes;
	
	int LEVEL_COUNT = 5;
	int ITEMS_PER_LEVEL_COUNT = 10;
	int ENEMIES_PER_LEVEL_COUNT = 10;
	
	//tilemap constants
	int[] mapSpriteVariations;
	int SPRITE_VARIATIONS_COUNT = 3;
	int SPRITE_WIDTH = 24;

	int TILE_FLOORDEFAULT;
	public int TILE_DOORCLOSED;
	int TILE_DOOROPEN;
	int TILE_STAIRSUP;
	int TILE_STAIRSDOWN;
	int TILE_SOLIDBLACK;
	
	//Fog of war viibility percent
	int TILE_FOW_VIS0;
	int TILE_FOW_VIS50;
	int TILE_FOW_VIS100;
	
	//z levels
	int Z_TERRAIN = 8;
	int Z_DECOR = 6;
	int Z_ITEMS = 4;
	public int Z_PROJECTILE = 3;
	int Z_ACTORS = 2;
	int Z_FOW = 0;
	

	
	//chance in 100
	int CHANCE_CHEST_IS_MIMIC = 10;
	int CHANCE_ENEMY_DROPS_LOOT = 80;
	
	void Start ()
	{
		NewGame ();
	}
	
	void NewGame ()
	{
		gameState = GameState.Initializing;
		
		audioManager = GameObject.Find ("GameController").GetComponent<AudioManager> ();
		
		projectileManager = GameObject.Find ("GameController").GetComponent<ProjectileManager> ();
		
		camera = GameObject.Find ("Camera").GetComponent<Camera> ();
		
		audioManager.FXVOLUME = float.Parse (PlayerPrefs.GetString ("soundLevel"));
		
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
	
	public void MoveGameObjectToZLevel (GameObject obj, int zLevel)
	{
		obj.transform.position = new Vector3 (obj.transform.position.x, obj.transform.position.y, zLevel);
	}
	
	void InitTextures ()
	{
		texturesPC = Resources.LoadAll<Sprite> ("Textures/Player");
		texturesNPC = Resources.LoadAll<Sprite> ("Textures/NPC");
		texturesItem = Resources.LoadAll<Sprite> ("Textures/Item");
		texturesItemDecor = Resources.LoadAll<Sprite> ("Textures/ItemDecor");
		texturesProjectile = Resources.LoadAll<Sprite> ("Textures/projectile");
	}
	
	public Sprite FindSpriteInTextures (string spriteName, Sprite[] textures)
	{
		for (int i=0; i<textures.Length; i++) {
			if (textures [i].name == spriteName) {
				return textures [i];
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
				//Item item = Factory.GetItemForLevel (currentLevel);
				//item.Location = map.GetRandomCell (true);
				//AddItem (item);
				
				//chests
				ItemChest chest = new ItemChest ();
				chest.Location = map.GetRandomOpenArea ();
				if (chest.Location != null) {
					map.Cells [chest.Location.x, chest.Location.y].Passable = false;
					AddItem (chest);
				}
				 
				//webs
				ItemWeb web;
				Map.Direction dir;
				dir = Map.Direction.Northeast;
				web = new ItemWeb (dir);
				web.Location = map.GetRandomCorner (dir);
				if (web.Location != null) {
					AddItem (web);
				}
				dir = Map.Direction.Northwest;
				web = new ItemWeb (dir);
				web.Location = map.GetRandomCorner (dir);
				if (web.Location != null) {
					AddItem (web);
				}
				dir = Map.Direction.Southeast;
				web = new ItemWeb (dir);
				web.Location = map.GetRandomCorner (dir);
				if (web.Location != null) {
					AddItem (web);
				}
				dir = Map.Direction.Southwest;
				web = new ItemWeb (dir);
				web.Location = map.GetRandomCorner (dir);
				if (web.Location != null) {
					AddItem (web);
				}
				
			}
		}
		
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
		if (item is ItemDecor) {
			sr.sprite = FindSpriteInTextures (item.SpriteName, texturesItemDecor);
		} else {
			sr.sprite = FindSpriteInTextures (item.SpriteName, texturesItem);
		}
		MoveGameObjectToZLevel (sprite, Z_ITEMS);
		MoveSpriteTo (sprite, item.Location.x, item.Location.y);
		itemSprites.Add (sprite);
	}
	
	void SetSprite (GameObject obj, Sprite sprite)
	{
		SpriteRenderer sr = obj.GetComponent<SpriteRenderer> ();
		sr.sprite = sprite;
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
		pcIsPathfinding = false;
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
			MakeEnemySprites ();
		} else {
			enemies = new List<Enemy> ();
			for (int i=0; i<ENEMIES_PER_LEVEL_COUNT; i++) {
				Enemy enemy = EnemyFactory.GetEnemyForLevel (currentLevel);
				enemy.Location = map.GetRandomCell (true);
				map.Cells [enemy.Location.x, enemy.Location.y].Passable = false;
				if (D100 () <= CHANCE_ENEMY_DROPS_LOOT) {
					enemy.Loot = ItemFactory.GetItemForLevel (currentLevel);
				}
				AddEnemy (enemy);
			}
		}
	}	
	
	void MakeEnemySprites ()
	{
		for (int i=0; i<enemies.Count; i++) {
			AddEnemySprite (enemies [i]);
			
			/*
			GameObject enemySprite = new GameObject ();
			SpriteRenderer sr = enemySprite.AddComponent<SpriteRenderer> ();
			sr.sprite = FindSpriteInTextures (enemies [i].SpriteName, texturesNPC);
			MoveSpriteTo (enemySprite, enemies [i].Location.x, enemies [i].Location.y);
			MoveGameObjectToZLevel (enemySprite, Z_ACTORS);
			enemySprites.Add (enemySprite);	
		*/
		}
	}
	
	void AddEnemy (Enemy enemy)
	{
		enemies.Add (enemy);
		AddEnemySprite (enemy);
	}
	
	void AddEnemySprite (Enemy enemy)
	{
		GameObject actorViz = (GameObject)Instantiate (actorVizPrefab);
		ActorVizBehavior b = actorViz.GetComponent<ActorVizBehavior> ();
		b.sprite1 = FindSpriteInTextures (enemy.SpriteName, texturesNPC);
		b.sprite2 = FindSpriteInTextures (enemy.SpriteName + "2", texturesNPC);
		b.isHealthBarVisible = false;
		MoveSpriteTo (actorViz, enemy.Location.x, enemy.Location.y);
		MoveGameObjectToZLevel (actorViz, Z_ACTORS);
		enemySprites.Add (actorViz);
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
	

	
	public void MoveSpriteTo (GameObject sprite, int gridX, int gridY)
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
		spritePC = (GameObject)Instantiate (actorVizPrefab);
		ActorVizBehavior b = spritePC.GetComponent<ActorVizBehavior> ();
		b.sprite1 = FindSpriteInTextures (spriteName, texturesPC);
		b.sprite2 = FindSpriteInTextures (spriteName + "2", texturesPC);
		b.isHealthBarVisible = false;
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
				if (map.Cells [w, h].Visited) {
					tileMapFOW [w, h] = TILE_FOW_VIS0;
				} else {
					tileMapFOW [w, h] = TILE_FOW_VIS0;
				}
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
		default:
			//otherwise keep doing whatever you're doing
			break;
		}
	}
	
	public IEnumerator WaitForSecondsThenExecute (float waitTime, Action method)
	{
		yield return new WaitForSeconds (waitTime);
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
	
	private void ClearPCPath ()
	{
		if (pathPC != null) {
			while (pathPC.Count >0) {
				AStar.Node nextStep;
				nextStep = pathPC.Pop ();
				tileMapFOW [nextStep.x, nextStep.y] = TILE_FOW_VIS100;
			}
		}
		pcIsPathfinding = false;
	}
	
	private void MovePCOnPath ()
	{
		AStar.Node nextStep;
		nextStep = pathPC.Pop ();
		tileMapFOW [nextStep.x, nextStep.y] = TILE_FOW_VIS100;
		if (nextStep == null || !map.Cells [nextStep.x, nextStep.y].Passable) {
			//no more steps or path blocked
			ClearPCPath ();
		} else {
			TryMovePlayerTo (new Address (nextStep.x, nextStep.y));
			if (pathPC.Count == 0) {
				pcIsPathfinding = false;
			}
		}
		gameState = GameState.TurnEnemy;
	}
	
	public void EndTurnInProgress ()
	{
		Debug.Log ("ending turn in progress");
		if (gameState == GameState.TurnPlayerInProgress) {
			gameState = GameState.TurnEnemy;
		} else {
			gameState = GameState.TurnPlayer;
		}
	}
	
	private void TakePlayerTurn ()
	{
		UpdateHud ();
		if (pcIsPathfinding) {
			MovePCOnPath ();
		} else {
			CheckInput ();
		}
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
		UpdateHud ();
		projectileManager.enemiesShooting.Clear ();
		for (int i=0; i<enemies.Count; i++) {
			Enemy enemy = enemies [i];
			//flip sprite to face player if necessary
			if (map.DistanceToPlayer (enemy.Location) < enemy.Stats.VisionRange) {
				if (enemy.Location.x > pc.Location.x) {
					enemySprites [i].transform.localScale = new Vector3 (1, 1, 1);
				} 
				if (enemy.Location.x < pc.Location.x) {
					enemySprites [i].transform.localScale = new Vector3 (-1, 1, 1);
				}
			}
			if (map.DistanceToPlayer (enemy.Location) == 1) {
				CombatCheck (enemy, pc);
			} else {
				if (enemy.CurrentWeapon.IsRanged 
					&& map.Distance (enemy.Location, pc.Location) <= enemy.Stats.VisionRange 
					&& projectileManager.IsClearPath (enemy.Location, pc.Location)) {
					projectileManager.enemiesShooting.Add (enemy);
				} else {
					Address oldLoc = enemy.Location;
					enemy.Move (map);
					if (enemy.Location.x != oldLoc.x || enemy.Location.y != oldLoc.y) {
						//flip moving sprite
						if (enemy.Location.x > oldLoc.x) {
							enemySprites [i].transform.localScale = new Vector3 (-1, 1, 1);
						} 
						if (enemy.Location.x < oldLoc.x) {
							enemySprites [i].transform.localScale = new Vector3 (1, 1, 1);
						} 
						MoveSpriteTo (enemySprites [i], enemy.Location.x, enemy.Location.y);
					}
				}
			}
		}
		if (projectileManager.enemiesShooting.Count > 0) {
			projectileManager.ShootEnemyShots ();
		} else {
			gameState = GameState.TurnPlayer;
		}
	}
		
	private void CheckInput ()
	{
		
		#region CHEATS		
		bool cheats = true;
		if (cheats) {
		
			if (Input.GetKeyDown (KeyCode.X)) {
				TryMovePlayerTo (map.exitLocation);
			}
			
			if (Input.GetKeyDown (KeyCode.N)) {
				TryMovePlayerTo (map.entranceLocation);
			}
		
		}
		#endregion
		
		if (Input.GetMouseButtonDown (0)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			Vector2Int tileClicked = new Vector2Int ((int)ray.origin.x, (int)ray.origin.y);
			Map.Cell cell = map.Cells [tileClicked.x, tileClicked.y];
			//Debug.Log (tileClicked.x + "," + tileClicked.y);
			//is there a monster there?
			if (pc.CurrentWeapon.IsRanged) {
				int enemyIndex = EnemyAt (new Address (tileClicked.x, tileClicked.y));
				if (cell.Visited && enemyIndex != -1) {
					PCShootAt (enemies [enemyIndex]);
				}
			}
			if (cell.Visited && cell.Passable) {
				//find a path to that cell
				AStar astar = new AStar (map);
				pathPC = astar.GetFastestPath (pc.Location, new Address (tileClicked.x, tileClicked.y));
				foreach (AStar.Node node in pathPC) {
					tileMapFOW [node.x, node.y] = TILE_FOW_VIS50;
					pcIsPathfinding = true;
				}
			}
		}
		
		if (Input.GetKeyDown (KeyCode.RightBracket) && camera.orthographicSize > 1.0) {
			camera.orthographicSize -= 0.5f; 
		}
		if (Input.GetKeyDown (KeyCode.LeftBracket) && camera.orthographicSize <= 15.0) {
			camera.orthographicSize += 0.5f;
		}
		
		if (Input.GetKeyDown (KeyCode.P)) {
			gameState = GameState.TurnEnemy;
		}
		if (Input.GetKeyDown (KeyCode.I)) {
			ShowInventory ();
		}
		bool isMoving = false;
		int newX = pc.Location.x, newY = pc.Location.y;
		if (Input.GetKeyDown (KeyCode.D) || Input.GetKeyDown (KeyCode.RightArrow)) {
			newX = newX + 1;
			isMoving = true;
		} else if (Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.LeftArrow)) {
			newX = newX - 1;
			isMoving = true;
		} else if (Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.UpArrow)) {
			newY = newY + 1;
			isMoving = true;
		} else if (Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.DownArrow)) {
			newY = newY - 1;
			isMoving = true;
		}
		if (isMoving) { 
			TryMovePlayerTo (new Address (newX, newY));
			gameState = GameState.TurnEnemy;
		}
	}
	
	private void GetLoot (Address location)
	{
		//any loot to pick up?
		int itemIndex = ItemAt (new Address (location.x, location.y));
		if (itemIndex != -1 && !(items [itemIndex] is ItemDecor)) {
			if (pc.AddToInventory (items [itemIndex])) {
				RemoveItem (itemIndex);
				audioManager.Play1Shot (audioManager.audioLoot);
				DisplayMessage ("Picked up " + pc.Inventory [pc.Inventory.Count - 1].Name);
			}
		}
	}
	
	public int EnemyAt (Address location)
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
	
	private void TryMovePlayerTo (Address newLocation)
	{
		int newX = newLocation.x;
		int newY = newLocation.y;
		//change facing if needed
		if (newX > pc.Location.x) {
			pcIsFlipped = true;
		}
		if (newX < pc.Location.x) {
			pcIsFlipped = false;
		}
		if (IsPassable (newX, newY)) {
			//open a closed door instead of moving in
			if (tileMapTerrain [newX, newY] == TILE_DOORCLOSED) {
				tileMapTerrain [newX, newY] = TILE_DOOROPEN;
				map.Cells [newX, newY].BlocksVision = false;
				audioManager.Play1Shot (audioManager.audioDoor);
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
				audioManager.Play1Shot (audioManager.audioStep);
			}
			SeeTilesFlood ();	
		} else {
			//is there a chest there?
			int itemIndex = ItemAt (new Address (newX, newY));
			if (itemIndex != -1 && items [itemIndex] is ItemChest) {
				ItemChest chest = (ItemChest)items [itemIndex];
				if (chest.State == ItemChest.ChestState.Closed && D100 () < CHANCE_CHEST_IS_MIMIC) {
					//create mimic
					Enemy mimic = EnemyFactory.NewMimic ();
					mimic.Location = chest.Location;
					AddEnemy (mimic);
					//remove chest
					RemoveItem (itemIndex);
				} else {
					switch (chest.State) {
					case ItemChest.ChestState.Closed:
						chest.SpriteName = "chestopen";
						chest.State = ItemChest.ChestState.Open;
						audioManager.Play1Shot (audioManager.audioChestOpen);
						break;
					case ItemChest.ChestState.Open:
						chest.SpriteName = "chestempty";
						//generate loot
						Item loot = ItemFactory.GetItemForLevel (currentLevel);
						loot.Location = new Address (pc.Location.x, pc.Location.y);
						AddItem (loot);
						GetLoot (pc.Location);
						chest.State = ItemChest.ChestState.Empty;
						break;
					default:
						break;
					}
					Sprite newSprite = FindSpriteInTextures (chest.SpriteName, texturesItemDecor);
					SetSprite (itemSprites [itemIndex], newSprite);
				}
			}
			//combat?
			//does the square contain an enemy?
			int enemyIndex = EnemyAt (new Address (newX, newY));
			if (enemyIndex != -1) {
				CombatCheck (pc, enemies [enemyIndex]);
			}
		}
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
		SeeTilesFlood ();
	}
	
	

	
	private void PCShootAt (Actor defender)
	{
		//flip player if needed
		if (defender.Location.x > pc.Location.x) {
			pcIsFlipped = true;
		} 
		if (defender.Location.x < pc.Location.x) {
			pcIsFlipped = false;
		} 
		gameState = GameState.TurnPlayerInProgress;
		audioManager.Play1Shot (audioManager.audioShotBow);
		StartCoroutine (projectileManager.MovePCShot (defender));
	}
	
	public void CombatCheck (Actor attacker, Actor defender)
	{
		//TODO make actual combat system
		//combat cancels pc pathfinding (whether or not pc is attacker)
		ClearPCPath ();
		DisplayMessage (attacker.Name + " attacks " + defender.Name + "...");
		if (UnityEngine.Random.Range (1, 10) > 4) {
			ApplyHit (attacker, defender);
		} else {
			//miss
			DisplayMessage (attacker.Name + " misses!");
			audioManager.Play1Shot (audioManager.audioWhiff);
		}
	}
	
	
	
	public void ApplyHit (Actor attacker, Actor defender)
	{
		int damage = UnityEngine.Random.Range (1, attacker.Stats.AttackMaxDamage + 1);
		damage = defender.TakeDamage (damage, attacker.CurrentWeapon.DmgType);
		DisplayMessage (defender.Name + " is hit for " + damage + " " + attacker.CurrentWeapon.DmgType.ToString () + " damage!");
		Debug.Log ("currentHealth:" + defender.Stats.CurrentHealth);
		GameObject viz;
		if (defender is PlayerCharacter) {
			audioManager.Play1Shot (audioManager.audioHitPlayer);
			viz = spritePC;
			//stop pathfinding if happening
			ClearPCPath ();
		} else {
			//it's an enemy
			int enemyIndex = EnemyAt (new Address (defender.Location.x, defender.Location.y));
			Enemy enemy = enemies [enemyIndex];
			viz = enemySprites [enemyIndex];
			if (enemy.Stats.CurrentHealth > 0) {
				audioManager.Play1Shot (audioManager.audioHitEnemy);
				
				ActorVizBehavior b = viz.GetComponent<ActorVizBehavior> ();
				b.isHealthBarVisible = true;
				float newHealth = (float)defender.Stats.CurrentHealth / (float)defender.Stats.MaxHealth;
				Debug.Log ("new health " + newHealth);
				b.SetHealthVisual (newHealth);
			} else {
				audioManager.Play1Shot (audioManager.audioDieEnemy);
				//drop loot
				Item loot = enemy.Loot;
				if (loot != null) {
					loot.Location = new Address (enemy.Location.x, defender.Location.y);
					AddItem (loot);
					DisplayMessage (enemy.Name + " drops " + loot.Name);
				}
				//remove enemy
				map.Cells [enemy.Location.x, enemy.Location.y].Passable = true;
				RemoveEnemy (enemyIndex);	
			}
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
			//if (!map.Cells [a.x, a.y].Visited) {
			map.Cells [a.x, a.y].Visited = true;
			if (tileMapFOW [a.x, a.y] == TILE_FOW_VIS0) {
				tileMapFOW [a.x, a.y] = TILE_FOW_VIS100;
			}
			//}
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

	public int D100 ()
	{
		return UnityEngine.Random.Range (1, 101);
	}
	
	
}
