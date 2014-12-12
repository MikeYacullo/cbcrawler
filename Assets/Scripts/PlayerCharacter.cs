using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCharacter : Actor
{

	public enum ClassType
	{
		None = 0
		,
		Cleric = 1
		,
		Fighter = 2
		,
		Rogue = 3
		,
		Wizard = 4
	}
		
	public ClassType classType;
	
	public bool IsMale;
	
	public List<Item> Inventory = new List<Item> ();
	public int InventoryMaxSize = 25;
		
	public PlayerCharacter ()
	{
		Name = "Player";
		Stats.AttackPower = 1;
		Stats.DefensePower = 1;
		Stats.AttackMaxDamage = 5;
		Stats.MaxHealth = 10;
		Stats.CurrentHealth = 10;
		Stats.VisionRange = 6;
		CurrentWeapon.ProjectileSprite = "unassigned";
		CurrentWeapon.DmgType = DamageType.Physical;
	}
	
	public bool AddToInventory (Item item)
	{
		if (Inventory.Count < InventoryMaxSize) {
			Inventory.Add (item);
			return true;
		} else {
			return false;
		}
	}
	
	public PlayerCharacter (ClassType classType, bool isMale)
	{
		this.classType = classType;
		IsMale = isMale;
		Name = classType.ToString ();
		switch (this.classType) {
		case ClassType.Cleric:
			Stats.AttackPower = 3;
			Stats.DefensePower = 4;
			Stats.AttackMaxDamage = 5;
			Stats.MaxHealth = 10;
			Stats.CurrentHealth = 10;
			Stats.VisionRange = 6;
			CurrentWeapon.IsRanged = true;
			CurrentWeapon.ProjectileSprite = "holy";
			break;
		case ClassType.Fighter:
			Stats.AttackPower = 2;
			Stats.DefensePower = 2;
			Stats.AttackMaxDamage = 5;
			Stats.MaxHealth = 10;
			Stats.CurrentHealth = 10;
			Stats.VisionRange = 6;
			break;
		case ClassType.Rogue:
			Stats.AttackPower = 2;
			Stats.DefensePower = 2;
			Stats.AttackMaxDamage = 5;
			Stats.MaxHealth = 10;
			Stats.CurrentHealth = 10;
			Stats.VisionRange = 8;
			break;
		case ClassType.Wizard:
			Stats.AttackPower = 4;
			Stats.DefensePower = 2;
			Stats.AttackMaxDamage = 5;
			Stats.MaxHealth = 6;
			Stats.CurrentHealth = 6;
			Stats.VisionRange = 6;
			CurrentWeapon.IsRanged = true;
			CurrentWeapon.ProjectileSprite = "fire";
			CurrentWeapon.DmgType = DamageType.Fire;
			break;
		default:
			break;
		}
	}
	
		
}
