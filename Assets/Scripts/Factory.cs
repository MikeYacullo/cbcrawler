using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Factory
{

	public enum EnemyType
	{
		Bat,
		GreenSlime,
		GoblinFighter,
		GoblinHunter,
		Spider,
		Mimic,
		
	}
	
	public enum ItemType
	{
		Armor,
		Dagger,
		Helm,
		Potion,
		Sword,
		Wand
	}
	
	static Factory ()
	{
		
	}
	
	public static Item GetItemForLevel (int level)
	{
		List<ItemType> items = new List<ItemType> ();
		switch (level) {
		default:
			items = new List<ItemType>{ItemType.Armor,ItemType.Dagger,ItemType.Helm,ItemType.Potion,ItemType.Sword,ItemType.Wand};
			break;
		}
		ItemType type = items [Random.Range (0, items.Count)];
		return CreateItem (type, level);
	}
	
	public static Item CreateItem (ItemType iType, int level)
	{
		Item item = new Item ();
		switch (iType) {
		case ItemType.Armor:
			item = NewArmor (level);
			break;
		case ItemType.Dagger:
			item = NewDagger (level);
			break;
		case ItemType.Potion:
			item = NewPotion (level);
			break;
		case ItemType.Helm:
			item = NewHelm (level);
			break;
		case ItemType.Sword:
			item = NewSword (level);
			break;
		case ItemType.Wand:
			item = NewWand (level);
			break;
		default:
			break;
		}
		return item;
	}
	
	public static Item NewArmor (int level)
	{
		Item item = new ItemConsumable ();
		item.SpriteName = "armor";
		item.Name = "Armor";
		return item;
	}
	
	public static Item NewDagger (int level)
	{
		Item item = new ItemConsumable ();
		item.SpriteName = "dagger";
		item.Name = "Dagger";
		return item;
	}
	
	public static Item NewHelm (int level)
	{
		Item item = new Item ();
		item.SpriteName = "helm";
		item.Name = "Helm";
		return item;
	}
	
	public static Item NewPotion (int level)
	{
		Item item = new ItemConsumable ();
		item.SpriteName = "potionred_s";
		item.Name = "Small Red Potion";
		return item;
	}
	
	public static Item NewSword (int level)
	{
		Item item = new Item ();
		item.SpriteName = "sword";
		item.Name = "Sword";
		return item;
	}
	
	public static Item NewWand (int level)
	{
		Item item = new ItemConsumable ();
		item.SpriteName = "wand";
		item.Name = "Wand";
		return item;
	}
	
	public static Enemy GetEnemyForLevel (int level)
	{
		List<EnemyType> enemies = new List<EnemyType> ();
		switch (level) {
		case 1:
			enemies = new List<EnemyType>{EnemyType.GoblinFighter,EnemyType.GoblinHunter};
			break;
		default:
			enemies = new List<EnemyType>{EnemyType.Bat,EnemyType.Spider,EnemyType.GreenSlime};
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
		case EnemyType.Spider:
			enemy = NewSpider ();
			break;
		default:
			//this should never happen!
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
		enemy.Stats.HasRangedAttack = true;
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



}
