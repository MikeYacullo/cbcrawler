using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AStar
{

	public List<string> walkableCellTypes;
	
	private Heap<Node> _openList;
	private IList<Node> _closedList;
	private Map _map;
	private Node[,] _world;
	
	
	public class Node:IComparable<Node>
	{
		public int g { get; set; }
		public int h { get; set; }
		public int f { get; set; }
		public int x { get; set; }
		public int y { get; set; }
		public Address address{ get { return new Address (x, y); } set { address = value; } }
		public Map.Cell cell{ get; set; }
		public Node parent{ get; set; }
		public Node (Map.Cell cell, int X, int Y)
		{
			this.cell = cell;
			this.x = X;
			this.y = Y;
		}
		public int CompareTo (Node other)
		{
			if (this.f == other.f)
				return 0;
			//higher priority when f is smaller than the others f.
			else if (this.f < other.f)
				return 1;
			else
				return -1;
		}
	}
	
	public AStar (Map map)
	{
		_map = map;
		_world = MapToNodes ();
		_openList = new Heap<Node> ();
		_closedList = new List<Node> ();
	}
	
	public Node[,] MapToNodes ()
	{
		Node[,] nodes = new Node[_map.Width, _map.Height];
		for (int x = 0; x < _map.Width; ++x) {
			for (int y = 0; y < _map.Height; ++y) {
				nodes [x, y] = new Node (_map.Cells [x, y], x, y);
			}
		}
		return nodes;
	}
	
	private void SetManhattenDist (Address start, Address target)
	{
		for (int x = 0; x < _map.Width; ++x) {
			for (int y = 0; y < _map.Height; ++y) {
				Node c = _world [x, y];
				if (c.x == start.x && c.y == start.y)
					continue;
				
				c.h = Mathf.Abs (target.x - x) + Mathf.Abs (target.y - y);
				c.g = int.MaxValue;
				c.f = 0;
				c.parent = null;
			}
		}
	}
	
	private List<Node> FindNeighbors (Map map, int x, int y)
	{
		Debug.Log ("Finding neighbors of " + x + "," + y);
		List<Node> nodes = new List<Node> ();
		Map.Cell[,] cells = map.Cells;
		//orthoganal only
		if (map.Contains (new Address (x, y - 1)) && map.Cells [x, y - 1].Passable) {
			nodes.Add (_world [x, y - 1]);
		}
		if (map.Contains (new Address (x, y + 1)) && map.Cells [x, y + 1].Passable) {
			nodes.Add (_world [x, y + 1]);
		}
		if (map.Contains (new Address (x - 1, y)) && map.Cells [x - 1, y].Passable) {
			nodes.Add (_world [x - 1, y]);
		}
		if (map.Contains (new Address (x + 1, y)) && map.Cells [x + 1, y].Passable) {
			nodes.Add (_world [x + 1, y]);
		}
		
		return nodes;
	}
	
	public int ReturnManhattanDist (Address start, Address target)
	{
		return Mathf.Abs (target.x - start.x) + Mathf.Abs (target.y - start.y);
	}
	
	public Stack<Node> GetFastestPath (Address startAddress, Address targetAddress)
	{
		Node start = _world [startAddress.x, startAddress.y];
		Node target = _world [targetAddress.x, targetAddress.y];
		if (target == null) {
			throw new Exception ("target is null.");
		}
		
		if (!target.cell.Passable)
			throw new Exception ("Cannot move to an unwalkable cell.");
		
		_openList.Clear ();
		_closedList.Clear ();
		
		SetManhattenDist (start.address, target.address);
		start.g = 0;
		//the estimation of going from target cell to target cell is 0 of course.
		target.h = 0;
		Stack<Node> nodes = new Stack<Node> ();
		
		if (!(start.x == target.x && start.y == target.y)) {
			_openList.Push (start);
			while (true) {
				Node c;
				try {
					c = _openList.Pop ();
				} catch (IndexOutOfRangeException e) {
					e.ToString ();
					break;
				}
				if (c == null)
					break;
				
				//if we havent already checked out the cell c
				if (!_closedList.Contains (c)) {
					List<Node> neighbours = FindNeighbors (_map, c.x, c.y);
					
					foreach (Node cc in neighbours) {
						if (cc == null)
							continue;
						
						if (cc.cell.Passable && !_closedList.Contains (cc)) {
							int dist = Mathf.Abs (cc.address.x - c.address.x) != Mathf.Abs (cc.address.y - c.address.y) ? 14 : 10;
							if (c.g + dist < cc.g) {
								cc.g = c.g + dist;
								cc.parent = c;
								cc.f = c.g + c.h;
							}
							if (!_openList.Contains (cc))
								_openList.Push (cc);
						}
					}
					_closedList.Add (c);
				}
				if (c.x == target.x && c.y == target.y) {
					nodes.Push (c);
					while (c.parent != null && !(c.parent.x==start.x && c.parent.y==start.y)) {
						c = c.parent;
						nodes.Push (c);
					}
					break;
				}
			}
		}
		return nodes;
	}
}
