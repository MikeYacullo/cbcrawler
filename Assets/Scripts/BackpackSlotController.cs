using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BackpackSlotController : MonoBehaviour
{

	public Item item;
	public Image itemImage;
	
	Sprite[] textures;
	string[] names;

	// Use this for initialization
	void Start ()
	{
		itemImage = gameObject.transform.GetChild (0).GetComponent<Image> ();
		textures = Resources.LoadAll<Sprite> ("Textures/Item");
		names = new string[textures.Length];
		
		for (int ii=0; ii< names.Length; ii++) {
			names [ii] = textures [ii].name;
		}
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (item != null) {
			itemImage.enabled = true;
			itemImage.sprite = textures [ArrayUtility.IndexOf (names, item.SpriteName)];
		} else {
			itemImage.enabled = false;
		}
	}
}
