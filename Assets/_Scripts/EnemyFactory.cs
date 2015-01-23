using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class EnemyFactory
{
	
	public enum EnemyType
	{
		Bat,
		GreenSlime,
		GoblinFighter,
		GoblinHunter,
		Rat,
		Snake,
		Spider,
		SpittingSpider,
		Wolf,
		Mimic,
		
	}
	
	static EnemyFactory ()
	{
		
	}
	
	public static Enemy GetEnemyForLevel (int level)
	{
		List<EnemyType> enemies = new List<EnemyType> ();
		switch (level) {
		case 0:
			enemies = new List<EnemyType>{EnemyType.Bat,EnemyType.Rat,EnemyType.Snake,EnemyType.Spider};			
			break;
		case 1:
			enemies = new List<EnemyType>{EnemyType.Bat,EnemyType.Snake,EnemyType.Spider,EnemyType.SpittingSpider, EnemyType.Wolf};
			break;
		case 3:
			enemies = new List<EnemyType>{EnemyType.GoblinFighter,EnemyType.GoblinHunter,EnemyType.GreenSlime};
			break;
		default:
			enemies = new List<EnemyType>{EnemyType.Bat,EnemyType.Spider};
			break;
		}
		EnemyType type = enemies [Random.Range (0, enemies.Count)];
		return CreateEnemy (type, level);
	}
	
	public static Enemy CreateEnemy (EnemyType eType, int level)
	{
		Enemy enemy = new Enemy ();
		switch (eType) {
		case EnemyType.Bat:
			enemy = NewBat ();
			break;
		case EnemyType.GoblinFighter:
			enemy = NewGoblinFighter ();
			break;
		case EnemyType.GoblinHunter:
			enemy = NewGoblinHunter ();
			break;
		case EnemyType.GreenSlime:
			enemy = NewGreenSlime ();
			break;
		case EnemyType.Mimic:
			enemy = NewMimic ();
			break;
		case EnemyType.Rat:
			enemy = NewRat ();
			break;
		case EnemyType.Snake:
			enemy = NewSnake ();
			break;
		case EnemyType.SpittingSpider:
			enemy = NewSpittingSpider ();
			break;
		case EnemyType.Spider:
			enemy = NewSpider ();
			break;
		case EnemyType.Wolf:
			enemy = NewWolf ();
			break;
		default:
			//this should never happen!
			Debug.Log ("WARNING: CreateEnemy received unhandled enemy type " + eType.ToString ());
			break;
		}
		if (Random.Range (0, 10) == 0) {
			enemy = Mutator.Mutate (enemy, level + 1);
		}
		return enemy;
	}
	
	public static Enemy NewBat ()
	{
		Enemy enemy = new Enemy ();
		enemy.Name = "Bat";
		enemy.SpriteName = "bat";
		enemy.Stats.MaxHealth = 5;
		enemy.Stats.CurrentHealth = enemy.Stats.MaxHealth;
		enemy.Stats.AttackPower = 1;
		enemy.Stats.DefensePower = 1;
		enemy.Stats.AttackMaxDamage = 1;
		return enemy;
	}
	
	public static Enemy NewGoblinFighter ()
	{
		Enemy enemy = new Enemy ();
		enemy.Name = "Goblin Fighter";
		enemy.SpriteName = "goblinfighter";
		enemy.Stats.MaxHealth = 2;
		enemy.Stats.CurrentHealth = enemy.Stats.MaxHealth;
		enemy.Stats.AttackPower = 1;
		enemy.Stats.DefensePower = 1;
		enemy.Stats.AttackMaxDamage = 1;
		return enemy;
	}
	
	public static Enemy NewGoblinHunter ()
	{
		Enemy enemy = new Enemy ();
		enemy.Name = "Goblin Hunter";
		enemy.SpriteName = "goblinhunter";
		enemy.Stats.MaxHealth = 2;
		enemy.Stats.CurrentHealth = enemy.Stats.MaxHealth;
		enemy.Stats.AttackPower = 1;
		enemy.Stats.DefensePower = 1;
		enemy.Stats.AttackMaxDamage = 1;
		enemy.CurrentWeapon.IsRanged = true;
		enemy.CurrentWeapon.ProjectileSprite = "arrow";
		return enemy;
	}
	
	public static Enemy NewGreenSlime ()
	{
		Enemy enemy = new Enemy ();
		enemy.Name = "Green Slime";
		enemy.SpriteName = "greenslime";
		enemy.Stats.MaxHealth = 5;
		enemy.Stats.CurrentHealth = enemy.Stats.MaxHealth;
		enemy.Stats.AttackPower = 1;
		enemy.Stats.DefensePower = 1;
		enemy.Stats.AttackMaxDamage = 1;
		enemy.Vulnerabilities.Add (DamageType.Fire);
		return enemy;
	}
	
	public static Enemy NewMimic ()
	{
		Enemy enemy = new Enemy ();
		enemy.Name = "Mimic";
		enemy.SpriteName = "mimic";
		enemy.Stats.MaxHealth = 5;
		enemy.Stats.CurrentHealth = enemy.Stats.MaxHealth;
		enemy.Stats.AttackPower = 1;
		enemy.Stats.DefensePower = 1;
		enemy.Stats.AttackMaxDamage = 1;
		return enemy;
	}
	
	public static Enemy NewRat ()
	{
		Enemy enemy = new Enemy ();
		enemy.Name = "Rat";
		enemy.SpriteName = "rat";
		enemy.Stats.MaxHealth = 5;
		enemy.Stats.CurrentHealth = enemy.Stats.MaxHealth;
		enemy.Stats.AttackPower = 1;
		enemy.Stats.DefensePower = 1;
		enemy.Stats.AttackMaxDamage = 1;
		return enemy;
	}
	
	public static Enemy NewSnake ()
	{
		Enemy enemy = new Enemy ();
		enemy.Name = "Snake";
		enemy.SpriteName = "snake";
		enemy.Stats.MaxHealth = 5;
		enemy.Stats.CurrentHealth = enemy.Stats.MaxHealth;
		enemy.Stats.AttackPower = 1;
		enemy.Stats.DefensePower = 1;
		enemy.Stats.AttackMaxDamage = 1;
		return enemy;
	}
	
	public static Enemy NewSpider ()
	{
		Enemy enemy = new Enemy ();
		enemy.Name = "Spider";
		enemy.SpriteName = "spider";
		enemy.Stats.MaxHealth = 5;
		enemy.Stats.CurrentHealth = enemy.Stats.MaxHealth;
		enemy.Stats.AttackPower = 1;
		enemy.Stats.DefensePower = 1;
		enemy.Stats.AttackMaxDamage = 1;
		return enemy;
	}
	
	public static Enemy NewSpittingSpider ()
	{
		Enemy enemy = new Enemy ();
		enemy.Name = "Spitting Spider";
		enemy.SpriteName = "spittingspider";
		enemy.Stats.MaxHealth = 5;
		enemy.Stats.CurrentHealth = enemy.Stats.MaxHealth;
		enemy.Stats.AttackPower = 1;
		enemy.Stats.DefensePower = 1;
		enemy.Stats.AttackMaxDamage = 1;
		enemy.CurrentWeapon.IsRanged = true;
		enemy.CurrentWeapon.ProjectileSprite = "poison";
		return enemy;
	}
	
	public static Enemy NewWolf ()
	{
		Enemy enemy = new Enemy ();
		enemy.Name = "Wolf";
		enemy.SpriteName = "wolf";
		enemy.Stats.MaxHealth = 5;
		enemy.Stats.CurrentHealth = enemy.Stats.MaxHealth;
		enemy.Stats.AttackPower = 1;
		enemy.Stats.DefensePower = 1;
		enemy.Stats.AttackMaxDamage = 1;
		return enemy;
	}
	
	
	
}
