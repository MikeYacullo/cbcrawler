using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InventoryController : MonoBehaviour
{
	public GameObject inventorySlot;
	
	public List<GameObject> slots = new List<GameObject> ();
	
	void Start ()
	{
		int size = 60;
		int startX = -120;
		int startY = 125;
		for (int r=0; r<5; r++) {
			for (int c=0; c<5; c++) {
				GameObject slot = (GameObject)Instantiate (inventorySlot);
				slot.transform.parent = this.gameObject.transform;
				slot.name = "backpackSlot" + (r * 5 + c);
				slots.Add (slot);
				slot.GetComponent<RectTransform> ().localPosition = new Vector3 (startX + (c * size), startY - (r * size), 0);
			}
		}
	}
	
	public void RenderInventory (List<Item> items)
	{
		for (int i=0; i<items.Count; i++) {
			BackpackSlotController bsc = (BackpackSlotController)slots [i].GetComponent<BackpackSlotController> ();
			bsc.item = items [i];
		}
	}
	
}
