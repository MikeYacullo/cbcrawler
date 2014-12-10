using UnityEngine;
using System.Collections;

public class Actor : Object
{

	public string Name;
	public Address Location = new Address (0, 0);
	public Stats Stats = new Stats ();
	public DamageType Weakness;
	public DamageType Resistance;
	public WeaponProperty CurrentWeapon = new WeaponProperty ();
	
	public int TakeDamage (int amount, DamageType type)
	{
		if (type == this.Weakness) {
			amount = amount * 2;
		}
		if (type == this.Resistance) {
			amount = amount / 2;
		}
		Stats.CurrentHealth -= amount;
		return amount;
	}
	
}
