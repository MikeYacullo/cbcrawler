using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BackpackSlotController : MonoBehaviour
{

	public Item item;
	public Image itemImage;
	
	Sprite[] textures;

	Sprite FindSpriteInTextures (string spriteName)
	{
		for (int i=0; i<textures.Length; i++) {
			if (textures [i].name == spriteName) {
				return textures [i];
			} 
		}
		return null;
	}

	// Use this for initialization
	void Start ()
	{
		itemImage = gameObject.transform.GetChild (0).GetComponent<Image> ();
		textures = Resources.LoadAll<Sprite> ("Textures/Item");
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (item != null) {
			itemImage.enabled = true;
			itemImage.sprite = FindSpriteInTextures (item.SpriteName);
		} else {
			itemImage.enabled = false;
		}
	}
}
