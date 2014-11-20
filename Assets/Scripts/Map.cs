using UnityEngine;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System;

using ProD;

public class Map
{
	
	public ProD.Map Dungeon;
	
	public enum CellType
	{
		None = 0
		,
		Wall = 1
		,
		Floor = 2
		,
		Door = 3
		,
		Entrance = 4
		,
		Exit = 5
	}
	
	public class Cell
	{
		public CellType Type;
		public bool Visited;
		public bool Passable;
		public bool BlocksVision;
	}
	
	public Cell[,] Cells;
	public int Width;
	public int Height;
	
	public Address pcLocation;
	public Address entranceLocation;
	public Address exitLocation;
	
	public Map (int width, int height)
	{
		this.Width = width;
		this.Height = height;
		this.Cells = new Cell[Width, Height];
		MakeDungeon ();
	}
	
	public bool Contains (int x, int y)
	{
		if (x >= 0 && x < Width && y >= 0 && y < Height) {
			return true;
		} else {
			return false;
		}
	}
	
	public bool Contains (Address address)
	{
		return Contains (address.x, address.y);
	}
	
	public Address GetRandomCell (bool isFloor)
	{
		int x = -1;
		int y = -1;
		while (!(Contains(x,y) && (!isFloor || (isFloor && Cells[x,y].Type==CellType.Floor)))) {
			x = UnityEngine.Random.Range (0, Width);
			y = UnityEngine.Random.Range (0, Height);
		}
		return new Address (x, y);
	}
	
	public float DistanceToPlayer (Address fromLocation)
	{
		return (float)Math.Sqrt (Math.Pow ((fromLocation.x - pcLocation.x), 2) + Math.Pow ((fromLocation.y - pcLocation.y), 2));
	}
	
	public List<Address> findVisibleCellsFlood (Address position, int range)
	{
		int arrSize = range * 3;
		
		int[,] arr = new int[arrSize, arrSize];
		
		for (int i = 0; i < arrSize; i++)
			for (int j = 0; j < arrSize; j++)
				arr [i, j] = range + 1; //init array with high distance
		
		arr [range, range] = 0; // center cell is target
		
		Stack<Address> stack = new Stack<Address> ();
		
		stack.Push (new Address (range, range));
		
		while (stack.Count != 0) {
			Address current = stack.Pop ();
			
			Address mapAddress = new Address (position.x + current.x - range, position.y + current.y - range);
			
			if (!Contains (mapAddress.x, mapAddress.y))
				continue;
			
			if (Cells [mapAddress.x, mapAddress.y].BlocksVision) {
				
				continue;
			}
			//Debug.Log (current.x+","+current.y);
			int newDist = arr [current.x, current.y] + 1;
			
			if (current.x > 0 && newDist < arr [current.x - 1, current.y]) {
				stack.Push (new Address (current.x - 1, current.y));
				arr [current.x - 1, current.y] = newDist;
			}
			if (current.x < arrSize - 1 && newDist < arr [current.x + 1, current.y]) {
				stack.Push (new Address (current.x + 1, current.y));
				arr [current.x + 1, current.y] = newDist;
			}
			if (current.y > 0 && newDist < arr [current.x, current.y - 1]) {
				stack.Push (new Address (current.x, current.y - 1));
				arr [current.x, current.y - 1] = newDist;
			}
			if (current.y < arrSize - 1 && newDist < arr [current.x, current.y + 1]) {
				stack.Push (new Address (current.x, current.y + 1));
				arr [current.x, current.y + 1] = newDist;
			}
			
			if (current.x > 0 && current.y > 0 && newDist < arr [current.x - 1, current.y - 1]) {
				stack.Push (new Address (current.x - 1, current.y - 1));
				arr [current.x - 1, current.y - 1] = newDist;
			}
			if (current.x < arrSize && current.y < arrSize && newDist < arr [current.x + 1, current.y + 1]) {
				stack.Push (new Address (current.x + 1, current.y + 1));
				arr [current.x + 1, current.y + 1] = newDist;
			}
			
			if (current.x > 0 && current.y < arrSize && newDist < arr [current.x - 1, current.y + 1]) {
				stack.Push (new Address (current.x - 1, current.y + 1));
				arr [current.x - 1, current.y + 1] = newDist;
			}
			if (current.x < arrSize && current.y > 0 && newDist < arr [current.x + 1, current.y - 1]) {
				stack.Push (new Address (current.x + 1, current.y - 1));
				arr [current.x + 1, current.y - 1] = newDist;
			}
			
			
		}
		
		List<Address> result = new List<Address> ();
		
		for (int i = 0; i < arrSize; i++) {
			for (int j = 0; j < arrSize; j++) {
				Address mapAddress = new Address (position.x + i - range, position.y + j - range);
				
				if (Contains (mapAddress.x, mapAddress.y) && arr [i, j] <= range)
					result.Add (mapAddress);
			}
		}
		
		
		return result;
	}
	
	private void MakeDungeon ()
	{		
		Generator_Dungeon.SetGenericProperties (Width, Height, "Stone Dungeon Theme");
		Dungeon = Generator_Dungeon.Generate ();		
		for (int h=0; h<Height; h++) {
			for (int w=0; w<Width; w++) {
				Cells [w, h] = new Cell ();
				Cells [w, h].Visited = false;
				Cells [w, h].Passable = false;
				Cells [w, h].BlocksVision = false;
				//Debug.Log(Dungeon.GetCell(w,h).type);
				switch (Dungeon.GetCell (w, h).type) {
				case "Abyss":
					Cells [w, h].Type = CellType.None;
					break;
				case "Door":
					Cells [w, h].Type = CellType.Door;
					Cells [w, h].Passable = true;
					Cells [w, h].BlocksVision = true;
					break;
				case "Entrance":
					Cells [w, h].Type = CellType.Entrance;
					Cells [w, h].Passable = true;
					break;
				case "Exit":
					Cells [w, h].Type = CellType.Exit;
					Cells [w, h].Passable = true;
					break;
				case "Path":
					Cells [w, h].Type = CellType.Floor;
					Cells [w, h].Passable = true;
					break;
				case "Wall":
					Cells [w, h].Type = CellType.Wall;
					Cells [w, h].BlocksVision = true;
					break;
				default:
					Cells [w, h].Type = CellType.Floor;
					Cells [w, h].Passable = true;
					break;
				}
			}
		}
	}
	
	
	
	
}