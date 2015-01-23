using UnityEngine;
using System.Collections;

public class OptionScrollerBehavior : MonoBehaviour
{

	public string[] options;
	public UnityEngine.UI.Text txtSelected;

	public string text;
	public int index;

	// Use this for initialization
	void Start ()
	{
		SetOption (0);
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	public void ClickLeft ()
	{
		SetOption (index - 1);
	}
	
	public void ClickRight ()
	{
		SetOption (index + 1);
	}
	
	void SetOption (int newIndex)
	{
		//Debug.Log ("SetOption " + newIndex);
		if (newIndex > options.Length - 1) {
			newIndex = 0;
		}
		if (newIndex < 0) {
			newIndex = options.Length - 1;
		}
		index = newIndex;
		txtSelected.text = options [index];
		text = txtSelected.text;
	}
}
