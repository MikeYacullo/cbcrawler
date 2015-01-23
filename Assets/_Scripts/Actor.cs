using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Actor : Object
{

	public string Name;
	public Address Location = new Address (0, 0);
	public Stats Stats = new Stats ();
	public List<DamageType> Vulnerabilities = new List<DamageType> ();
	public List<DamageType> Resistances = new List<DamageType> ();
	public WeaponProperty CurrentWeapon = new WeaponProperty ();
	
	public int TakeDamage (int amount, DamageType type)
	{
		if (Vulnerabilities.Contains (type)) {
			amount = amount * 2;
		}
		if (Resistances.Contains (type)) {
			amount = amount / 2;
		}
		Stats.CurrentHealth -= amount;
		return amount;
	}
	
}
