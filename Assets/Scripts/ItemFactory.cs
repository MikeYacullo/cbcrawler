using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ItemFactory
{

	public enum ItemType
	{
		Armor,
		Dagger,
		Helm,
		Potion,
		Sword,
		Wand
	}
	
	static ItemFactory ()
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


}
