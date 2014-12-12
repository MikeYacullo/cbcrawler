using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : Actor
{
	public string SpriteName;
	public Item Loot;
	public Enemy ()
	{
		Stats.VisionRange = 6;
		Stats.AttackPower = 1;
		Stats.AttackMaxDamage = 1;
		Stats.DefensePower = 1;
		CurrentWeapon.DmgType = DamageType.Physical;
		CurrentWeapon.IsRanged = false;
		CurrentWeapon.ProjectileSprite = "unassigned";
	}
	
	public bool Chase (Map map)
	{
		//if the player is not in vision range we don't even need to do the check
		float distance = map.DistanceToPlayer (Location);
		//Debug.Log (distance);
		if (Mathf.RoundToInt (distance) > Stats.VisionRange) {
			//Debug.Log("i can't see pc");
			return false;
		}
		if (distance <= 1) {
			//stay where you are
			return true;
		}
		//get visible cells
		List<Address> visible = map.findVisibleCellsFlood (Location, Stats.VisionRange);
		//is the player location in one of those?
		Address pc = map.pcLocation;
		//Debug.Log ("Can " + Location.x + "," + Location.y + " see " + pc.x + "," + pc.y + "?");	
		//if (visible.Contains (pc)) // contains does not work for some reason. equality check failing?
		bool isPCVisible = false;
		foreach (Address loc in visible) {
			if (loc.x == pc.x && loc.y == pc.y) {
				isPCVisible = true;
			}
		}
		if (isPCVisible) {
			//Debug.Log ("i can see pc");
			//move one square closer to the player
			List<Address> newLocs = new List<Address> ();
			if (pc.x > Location.x && map.Cells [Location.x + 1, Location.y].Passable) {
				newLocs.Add (new Address (Location.x + 1, Location.y));
			}
			if (pc.x < Location.x && map.Cells [Location.x - 1, Location.y].Passable) {
				newLocs.Add (new Address (Location.x - 1, Location.y));
			}
			if (pc.y > Location.y && map.Cells [Location.x, Location.y + 1].Passable) {
				newLocs.Add (new Address (Location.x, Location.y + 1));
			}
			if (pc.y < Location.y && map.Cells [Location.x, Location.y - 1].Passable) {
				newLocs.Add (new Address (Location.x, Location.y - 1));
			}
			if (newLocs.Count > 0) {
				MoveToLocation (map, newLocs [Random.Range (0, newLocs.Count)]);
			} else {
				//couldn't find a valid place to move
				return false;
			}
		} else {
			//Debug.Log ("Can't see the PC");
			return false;
		}
		return true;
	}
	
	public void Move (Map map)
	{
		if (!Chase (map)) {
			//flap around randomly
			int dx = Random.Range (-1, 2);
			int dy = Random.Range (-1, 2);
			MoveToLocation (map, new Address (Location.x + dx, Location.y + dy));
		}
	}
	
	public void MoveToLocation (Map map, Address newLocation)
	{
		if (map.Contains (newLocation) && map.Cells [newLocation.x, newLocation.y].Passable) {
			//mark old location passable
			map.Cells [Location.x, Location.y].Passable = true;
			Location = newLocation;
			//mark new location impassable
			map.Cells [Location.x, Location.y].Passable = false;
		}
	}
}


