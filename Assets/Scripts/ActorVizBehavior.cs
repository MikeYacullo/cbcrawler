﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ActorVizBehavior : MonoBehaviour
{

	public bool isAnimated = true;
	public bool isHealthBarVisible = true;
	public Sprite sprite1, sprite2;
	
	public Image healthBar;
	
	SpriteRenderer renderer;
	
	float startTime;
	float timePerFrame = 0.5f;
	
	// Use this for initialization
	void Start ()
	{
		renderer = GetComponent<SpriteRenderer> ();
		renderer.sprite = sprite1;
		startTime = Time.time;
		
		healthBar = GetComponentInChildren<Image> ();
	}
	
	// Health between [0.0f,1.0f] == (currentHealth / totalHealth)
	public void SetHealthVisual (float healthNormalized)
	{
		healthBar.transform.localScale = new Vector3 (healthNormalized,
		                                             healthBar.transform.localScale.y,
		                                             healthBar.transform.localScale.z);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (isHealthBarVisible) {
			healthBar.enabled = true;
		} else {
			healthBar.enabled = false;
		}
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
