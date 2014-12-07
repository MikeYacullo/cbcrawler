using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{

	GameManager gameManager;
	
	public float FXVOLUME = 1.0f;
	public AudioClip audioStep;
	public AudioClip audioDoor;
	public AudioClip audioHitEnemy;
	public AudioClip audioHitPlayer;
	public AudioClip audioWhiff;
	public AudioClip audioDieEnemy;
	public AudioClip audioLoot;
	public AudioClip audioChestOpen;
	public AudioClip audioShotBow;
	public AudioClip audioShotMiss;


	public void Play1Shot (AudioClip clip)
	{
		audio.PlayOneShot (clip, FXVOLUME);
	}
	
	// Use this for initialization
	void Start ()
	{
		gameManager = GameObject.Find ("GameController").GetComponent<GameManager> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
