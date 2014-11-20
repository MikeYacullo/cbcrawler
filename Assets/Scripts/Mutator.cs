using UnityEngine;
using System.Collections;

public static class Mutator
{

	public static Enemy Mutate (Enemy enemy, int factor)
	{
		int choice = Random.Range (0, 5);
		Stats stats = new Stats ();
		switch (choice) {
		case 0:
			enemy.Name = "Weak " + enemy.Name;
			stats.AttackPower = -2 * factor;
			break;
		case 1:
			enemy.Name = "Tough " + enemy.Name;
			stats.DefensePower = 5 * factor;
			break;
		case 2:
			enemy.Name = "Resilient " + enemy.Name;
			stats.MaxHealth = 5 * factor;
			stats.CurrentHealth = 5 * factor;
			break;
		case 3:
			enemy.Name = "Mighty " + enemy.Name;
			stats.AttackPower = 5 * factor;
			break;
		case 4:
			enemy.Name = "Wounded " + enemy.Name;
			stats.CurrentHealth = -2 * factor;
			break;
		default:
			enemy.Name = "Typical " + enemy.Name;
			stats.AttackMaxDamage = 0;
			stats.AttackPower = 0;
			stats.CurrentHealth = 0;
			stats.DefensePower = 0;
			stats.MaxHealth = 0;
			stats.VisionRange = 0;
			break;
		}
		enemy.Stats.Add (stats);
		return enemy;
	}

	public static Item Mutate (Item item)
	{
		int choice = Random.Range (0, 5);
		Stats stats = new Stats ();
		switch (choice) {
		case 0:
			
			break;
		case 1:
			break;
		case 2:
			break;
		case 3:
			break;
		case 4:
			break;
		default:
			break;
		}
		return item;
	}
	
}
