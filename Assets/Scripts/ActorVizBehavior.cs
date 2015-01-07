using UnityEngine;
using System.Collections;

public class ActorVizBehavior : MonoBehaviour
{

	public bool isAnimated = true;
	public bool isHealthBarVisible = true;
	public Sprite sprite1, sprite2;
	
	SpriteRenderer renderer;
	
	float startTime;
	float timePerFrame = 0.5f;
	
	// Use this for initialization
	void Start ()
	{
		renderer = GetComponent<SpriteRenderer> ();
		renderer.sprite = sprite1;
		startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (isAnimated) {
			if (Time.time > startTime + timePerFrame) {
				if (renderer.sprite == sprite1) {
					renderer.sprite = sprite2;
				} else {
					renderer.sprite = sprite1;
				}
				startTime = Time.time;
			}
		}
	}
}
