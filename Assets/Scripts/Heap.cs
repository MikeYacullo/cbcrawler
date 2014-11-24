using UnityEngine;
using System;
using System.Collections.Generic;


public class Heap<T> where T : IComparable<T>
{
	private IList<T> _elements;
	
	public Heap ()
	{
		_elements = new List<T> ();
		//we always need to look from index 1 instead of index 0. 
		//so, when adding cells to the list, they should be added from 1 and up iso 0 and up
		_elements.Add (default(T));
	}
	
	public bool Contains (T element)
	{
		return _elements.Contains (element);
	}
	
	public void Clear ()
	{
		for (int i = _elements.Count - 1; i >= 1; --i) {
			_elements.RemoveAt (i);
		}
	}
	
	public void Push (T item)
	{
		_elements.Add (item);
		int index = _elements.Count - 1;
		
		while (index > 1) {
			int parentIndex = (int)Mathf.Floor (index / 2);
			//if the parent node has a bigger value than the child.
			if (_elements [index].CompareTo (_elements [parentIndex]) >= 0) {
				Swap (index, parentIndex);	//swap the parent and child node.
				index = parentIndex;		//and adjust the index.
			} else
				break;	//if not, break out of the loop				
		}
	}
	
	public T Pop ()
	{
		//remember, the first cell is never used.
		if (_elements.Count == 1)
			throw new IndexOutOfRangeException ("there are no more elements in the heap.");
		//take the first node from the heap
		T element = _elements [1];
		//set the value of the first node equal to the value of the last node.
		_elements [1] = _elements [_elements.Count - 1];
		_elements.RemoveAt (_elements.Count - 1);
		int index = 1;
		
		//bubble the node down
		while (true) {
			//left child.
			int lChild = index * 2;
			//right child
			int rChild = index * 2 + 1;
			
			//does the node have a right child (and therefore a left child as well)?
			if (rChild < _elements.Count) {
				//if index is smaller than right child
				if (_elements [index].CompareTo (_elements [rChild]) < 0) {
					int lowest = _elements [lChild].CompareTo (_elements [rChild]) > 0 ? lChild : rChild;
					Swap (index, lowest);
					index = lowest;
					continue;
				}
				//otherwise, if no right child, does the parent have a left child?
			} else if (lChild < _elements.Count) {
				if (_elements [index].CompareTo (_elements [lChild]) > 0) {
					Swap (index, lChild);
					index = lChild;
					continue;
				}
			}
			break;
		}
		return element;
	}
	
	//swaps two nodes from the _nodes array
	private void Swap (int i1, int i2)
	{
		T temp = _elements [i1];
		_elements [i1] = _elements [i2];
		_elements [i2] = temp;
	}
}

