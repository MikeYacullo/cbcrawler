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
	
	public enum Direction
	{
		North,
		Northeast,
		East,
		Southeast,
		South,
		Southwest,
		West,
		Northwest
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
	
	public Address GetRandomCorner (Direction direction)
	{
		bool ok = false;
		int x = -1;
		int y = -1;
		while (!ok) {
			Address loc = GetRandomCell (true);
			x = loc.x;
			y = loc.y;
			switch (direction) {
			case Direction.Northeast:
				if (Contains (x, y + 1) && Contains (x + 1, y) && Cells [x, y + 1].Type == CellType.Wall && Cells [x + 1, y].Type == CellType.Wall) {
					ok = true;
				}
				break;
			case Direction.Northwest:
				if (Contains (x - 1, y) && Contains (x, y + 1) && Cells [x, y + 1].Type == CellType.Wall && Cells [x - 1, y].Type == CellType.Wall) {
					ok = true;
				}
				break;
			case Direction.Southeast:
				if (Contains (x, y - 1) && Contains (x + 1, y) && Cells [x, y - 1].Type == CellType.Wall && Cells [x + 1, y].Type == CellType.Wall) {
					ok = true;
				}
				break;
			case Direction.Southwest:
				if (Contains (x, y - 1) && Contains (x - 1, y) && Cells [x, y - 1].Type == CellType.Wall && Cells [x - 1, y].Type == CellType.Wall) {
					ok = true;
				}
				break;
			default:
				break;
			}
		}
		return new Address (x, y);
	}
	
	public bool IsListPassable (List<Address> locs)
	{
		bool ok = true;
		foreach (Address loc in locs) {
			if (!Contains (loc.x, loc.y) || Cells [loc.x, loc.y].Type != CellType.Floor || !Cells [loc.x, loc.y].Passable) {
				return false;
			}
		}
		return ok;
	}
	
	public bool IsOpenAreaCenter (Address loc)
	{
		int x = loc.x;
		int y = loc.y;
		bool ok = true;
		List<Address> locs = new List<Address> ();
		for (int xo = -1; xo<=1; xo++) {
			for (int yo = -1; yo<=1; yo++) {
				locs.Add (new Address (x + xo, y + yo));
			}
		}
		return IsListPassable (locs);
	}
	
	public Address GetRandomOpenArea ()
	{
		bool ok = false;
		Address address = new Address (0, 0);
		while (!ok) {
			//keep doing this until we find a floor surrounded by open floor
			address = GetRandomCell (true);
			int x = address.x;
			int y = address.y;
			ok = IsOpenAreaCenter (new Address (x, y));
		}
		return address;
	}
	
	public float Distance (Address fromLocation, Address toLocation)
	{
		return (float)Math.Sqrt (Math.Pow ((fromLocation.x - toLocation.x), 2) + Math.Pow ((fromLocation.y - toLocation.y), 2));
	}
	
	public float DistanceToPlayer (Address fromLocation)
	{
		return Distance (fromLocation, pcLocation);
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
	
	public List<Address> CastRay (int x0, int y0, int x1, int y1)
	{
		//cast a ray until you hit the target, the edge of the map, or something impassible
		List<Address> result = new List<Address> ();
		
		bool steep = false;
		if (Mathf.Abs (y1 - y0) > Mathf.Abs (x1 - x0))
			steep = true;
		
		int deltax = Mathf.Abs (x1 - x0);
		int deltay = Mathf.Abs (y1 - y0);
		int error = 0;
		int ystep;
		int xstep;
		int x = x0;
		int y = y0;
		if (x0 < x1)
			xstep = 1;
		else
			xstep = -1;
		if (y0 < y1)
			ystep = 1;
		else
			ystep = -1;
		
		if (!steep) {
			for (int i = 0; i <= deltax; i++) {
				if (Contains (x, y) == false)
					return result;
				result.Add (new Address (x, y));
				if (!Cells [x, y].Passable && (x != x0 || y != y0))
					return result;
				x += xstep;
				error += deltay;
				if (2 * error >= deltax) {
					y += ystep;
					error -= deltax;
				}
			}
		} else {
			for (int i = 0; i <= deltay; i++) {
				if (Contains (x, y) == false)
					return result;
				result.Add (new Address (x, y));
				if (!Cells [x, y].Passable && (x != x0 || y != y0))
					return result;
				y += ystep;
				error += deltax;
				if (2 * error >= deltay) {
					x += xstep;
					error -= deltay;
				}
			}
		}
		
		return result;
	}
	
	private void MakeDungeon ()
	{		
		Generator_Dungeon.SetGenericProperties (Width, Height, "Stone Dungeon Theme");
		Dungeon = Generator_Dungeon.Generate ();		
		//Generator_Castle.SetGenericProperties (Width, Height, "Stone Dungeon Theme");
		//Dungeon = Generator_Castle.Generate ();
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