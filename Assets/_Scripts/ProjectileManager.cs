﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileManager : MonoBehaviour
{

	GameManager gameManager;
	AudioManager audioManager;
	
	GameObject spriteProjectile;
	float PROJECTILE_SECONDS_PER_TILE = 0.1f;
	public List<Enemy> enemiesShooting = new List<Enemy> ();
	List<GameObject> enemyShots = new List<GameObject> ();
	
	public void AddEnemyShot (Enemy enemy)
	{
		GameObject enemyShot = new GameObject ();
		SpriteRenderer sr = enemyShot.AddComponent<SpriteRenderer> ();
		sr.sprite = gameManager.FindSpriteInTextures (enemy.CurrentWeapon.ProjectileSprite, gameManager.texturesProjectile);
		gameManager.MoveSpriteTo (enemyShot, enemy.Location.x, enemy.Location.y);
		gameManager.MoveGameObjectToZLevel (enemyShot, gameManager.Z_PROJECTILE);
		//point shot at the pc		
		enemyShot.transform.rotation = LookAt2D (enemy.Location, gameManager.pc.Location);
		enemyShots.Add (enemyShot);
	}
	
	public Quaternion LookAt2D (Address origin, Address target)
	{
		Vector3 dir = new Vector3 (target.x - origin.x, target.y - origin.y, 0);
		//Debug.Log ("dir:" + dir.ToString ());
		float angle = Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg;
		angle -= 90; //adjust for upward-oriented sprites
		return Quaternion.AngleAxis (angle, Vector3.forward);
	}
	
	public IEnumerator MoveEnemyShots ()
	{
		float moveTime = 0;
		while (true) {
			int inactiveCount = 0;
			for (int i=0; i<enemyShots.Count; i++) {
				GameObject spriteShot = enemyShots [i];
				if (!spriteShot.activeSelf) {
					inactiveCount++;
				} else {
					Address originTile = enemiesShooting [i].Location;
					Address targetTile = gameManager.pc.Location;
					
					float moveDuration = gameManager.map.Distance (originTile, targetTile) * PROJECTILE_SECONDS_PER_TILE;
					
					//get screen coordinates of origin and target
					Rect rectOrigin = gameManager.tileMapTerrain.GetTileBoundsLocal (originTile.x, originTile.y);
					Rect rectTarget = gameManager.tileMapTerrain.GetTileBoundsLocal (targetTile.x, targetTile.y);
					
					// store the starting position of the object this script is attached to as well as the target position
					Vector3 oldPos = new Vector3 (rectOrigin.center.x, rectOrigin.center.y, gameManager.Z_PROJECTILE);
					Vector3 newPos = new Vector3 (rectTarget.center.x, rectTarget.center.y, gameManager.Z_PROJECTILE);
					moveTime += Time.deltaTime;
					Vector3 curPos = Vector3.Lerp (oldPos, newPos, moveTime / moveDuration);
					spriteShot.transform.position = curPos;
					int curX = (int)curPos.x;
					int curY = (int)curPos.y;
					//Debug.Log (new Address (curX, curY).ToString ());
					if (curX != originTile.x || curY != originTile.y) {
						if (!gameManager.map.Cells [curX, curY].Passable) {
							spriteShot.SetActive (false);
							inactiveCount++;
							if (gameManager.pc.Location.x == curX && gameManager.pc.Location.y == curY) {
								gameManager.CombatCheck (enemiesShooting [i], gameManager.pc);		
							} else {
								audioManager.Play1Shot (audioManager.audioShotMiss);
							}
						}
					}
				}
			}
			//Debug.Log ("Shots:" + enemyShots.Count + ", Inactive shots:" + inactiveCount);
			//are there still any sprites active?
			if (inactiveCount == enemyShots.Count) {
				//delete all shot sprites
				while (enemyShots.Count>0) {
					GameObject.Destroy (enemyShots [0]);
					enemyShots.RemoveAt (0);	
				}
				//delete all enemiesShooting;
				enemiesShooting.Clear ();
				gameManager.gameState = GameManager.GameState.TurnPlayer;
				break;
			}
			yield return new WaitForSeconds (0.02f);
		}
	}
	
	public void ShootEnemyShots ()
	{
		gameManager.gameState = GameManager.GameState.EnemiesShooting;
		enemyShots.Clear ();
		for (int i=0; i<enemiesShooting.Count; i++) {
			Enemy enemy = enemiesShooting [i];
			//create a sprite for each enemy projectile
			AddEnemyShot (enemy);
		}
		StartCoroutine (MoveEnemyShots ());
	}
	
	public bool IsClearPath (Address originTile, Address targetTile)
	{
		
		//Debug.Log ("IsClearPath: " + originTile.ToString () + " to " + targetTile.ToString ());
		
		float distance = gameManager.map.Distance (originTile, targetTile);
		//get screen coordinates of origin and target
		Rect rectOrigin = gameManager.tileMapTerrain.GetTileBoundsLocal (originTile.x, originTile.y);
		Rect rectTarget = gameManager.tileMapTerrain.GetTileBoundsLocal (targetTile.x, targetTile.y);
		
		// store the starting position of the object this script is attached to as well as the target position
		Vector3 oldPos = new Vector3 (rectOrigin.center.x, rectOrigin.center.y, 0);
		Vector3 newPos = new Vector3 (rectTarget.center.x, rectTarget.center.y, 0);
		
		Vector3 testPos;
		
		float testDistance = 0.0f;
		float testIncrement = 0.5f;
		while (testDistance < distance) {
			testPos = Vector3.Lerp (oldPos, newPos, testDistance / distance);
			//Debug.Log ("  testPos: " + testPos.x + "," + testPos.y);
			int testX = (int)(testPos.x);
			int testY = (int)(testPos.y);
			//Debug.Log ("    to tile: " + testX + "," + testY + " - which is " + map.Cells [testX, testY].Type.ToString ());
			if (testX != originTile.x || testY != originTile.y) {
				//tileMapFOW [testX, testY] = TILE_FOW_VIS50;
				if (testX == targetTile.x && testY == targetTile.y) {
					return true;
				}
				if (!gameManager.map.Contains (new Address (testX, testY)) 
					|| !gameManager.map.Cells [testX, testY].Passable 
					|| gameManager.tileMapTerrain [testX, testY] == gameManager.TILE_DOORCLOSED) {
					//Debug.Log ("clunk");
					return false;
				}
			}
			testDistance = testDistance + testIncrement;
		}
		return true;
	}
	
	public IEnumerator MovePCShot (Actor defender)
	{	
		Actor attacker = gameManager.pc;
		Address originTile = attacker.Location;
		Address targetTile = defender.Location;
		
		spriteProjectile.SetActive (true);
		SpriteRenderer sr = spriteProjectile.GetComponent<SpriteRenderer> ();
		sr.sprite = gameManager.FindSpriteInTextures (gameManager.pc.CurrentWeapon.ProjectileSprite, gameManager.texturesProjectile);
		spriteProjectile.transform.rotation = LookAt2D (originTile, targetTile);
		float moveDuration = gameManager.map.Distance (originTile, targetTile) * PROJECTILE_SECONDS_PER_TILE;
		
		//get screen coordinates of origin and target
		Rect rectOrigin = gameManager.tileMapTerrain.GetTileBoundsLocal (originTile.x, originTile.y);
		Rect rectTarget = gameManager.tileMapTerrain.GetTileBoundsLocal (targetTile.x, targetTile.y);
		
		// store the starting position of the object this script is attached to as well as the target position
		Vector3 oldPos = new Vector3 (rectOrigin.center.x, rectOrigin.center.y, gameManager.Z_PROJECTILE);
		Vector3 newPos = new Vector3 (rectTarget.center.x, rectTarget.center.y, gameManager.Z_PROJECTILE);
		float moveTime = 0.0f;
		
		while (moveTime < moveDuration) {
			moveTime += Time.deltaTime;
			Vector3 curPos = Vector3.Lerp (oldPos, newPos, moveTime / moveDuration);
			spriteProjectile.transform.position = curPos;
			int curX = (int)curPos.x;
			int curY = (int)curPos.y;
			//Debug.Log (new Address (curX, curY).ToString ());
			if (curX != originTile.x || curY != originTile.y) {
				if (!gameManager.map.Cells [curX, curY].Passable) {
					spriteProjectile.SetActive (false);
					//figure out what we hit
					int enemyIndex = gameManager.EnemyAt (new Address (curX, curY));
					if (enemyIndex == -1) {
						audioManager.Play1Shot (audioManager.audioShotMiss);
					} else {
						gameManager.CombatCheck (attacker, defender);
					}
					gameManager.EndTurnInProgress ();
					break;
				}
			}
			yield return null;
		}
		spriteProjectile.SetActive (false);
	}
	
	void Start ()
	{
		gameManager = GameObject.Find ("GameController").GetComponent<GameManager> ();
		audioManager = GameObject.Find ("GameController").GetComponent<AudioManager> ();
		spriteProjectile = GameObject.Find ("SpriteProjectile");
		spriteProjectile.SetActive (false);
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
