using UnityEngine;
using System.Collections;

public class Item
{
	public int Value = 0;
	public Address Location;
	public string Name;
	public string SpriteName;
	public Stats Stats = new Stats ();
}

public enum ItemSlotType
{
	Weapon,
	Armor,
	Boots,
	Helm,
	Ring
}

public class ItemDecor:Item
{

}

public class ItemDecorBlocking:ItemDecor
{

}

public class ItemChest:ItemDecorBlocking
{
	public enum ChestState
	{
		Closed,
		Open,
		Empty
	}
	public ChestState State;
	public ItemChest ()
	{
		SpriteName = "chestclosed";
		State = ChestState.Closed;
	}
}

public class ItemWeb:ItemDecor
{
	public ItemWeb (Map.Direction dir)
	{
		switch (dir) {
		case Map.Direction.Northeast:
			SpriteName = "webNE";
			break;
		case Map.Direction.Northwest:
			SpriteName = "webNW";
			break;
		case Map.Direction.Southeast:
			SpriteName = "webSE";
			break;
		case Map.Direction.Southwest:
			SpriteName = "webSW";
			break;
		default:
			break;
		}
	}
}

public class ItemEquippable:Item
{
	public ItemSlotType EquipSlot;
}

public class ItemConsumable : Item
{
	public bool IsConsumedOnPIckup = false;
}
/*
public class Gold_S : ItemConsumable
{
	public Gold_S ()
	{
		Name = "Small Pile of Gold";
		SpriteName = "gold_s";
		Value = Random.Range (1, 10);
		IsConsumedOnPIckup = true;
	}
	public string Consume ()
	{
		return "You're rich!";
	}
}

public class PotionRed_S : ItemConsumable
{
	public PotionRed_S ()
	{
		Name = "Small Red Potion";
		SpriteName = "potionRed_s";
	}
	public string Consume ()
	{
		throw new System.NotImplementedException ();
	}
}
	*/