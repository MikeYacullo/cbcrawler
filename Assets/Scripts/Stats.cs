using UnityEngine;
using System.Collections;

public class Stats
{

	public int MaxHealth = 0;
	public int CurrentHealth = 0;
	public int VisionRange = 0;
	public int AttackPower = 0;
	public int DefensePower = 0;
	public int AttackMaxDamage = 0;
	
	
	public Stats ()
	{
	
	}
	
	public void Add (Stats modifier)
	{
		MaxHealth += modifier.MaxHealth;
		CurrentHealth += modifier.MaxHealth;
		VisionRange += modifier.VisionRange;
		AttackPower += modifier.AttackPower;
		DefensePower += modifier.DefensePower;
		AttackMaxDamage += modifier.AttackMaxDamage;
	}
	
	public void Remove (Stats modifier)
	{
		//this will come in handy for unequipping
		//so we don't have to invert each stat
		MaxHealth -= modifier.MaxHealth;
		CurrentHealth -= modifier.MaxHealth;
		VisionRange -= modifier.VisionRange;
		AttackPower -= modifier.AttackPower;
		DefensePower -= modifier.DefensePower;
		AttackMaxDamage -= modifier.AttackMaxDamage;
	}
	
}
