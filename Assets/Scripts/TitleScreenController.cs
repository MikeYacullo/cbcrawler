using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TitleScreenController : MonoBehaviour
{

	public string ClassName;
	
	public AudioClip titleMusic;
	public AudioClip titleSoundTest;
	
	AudioSource sourceFX, sourceMusic;
	
	private UnityEngine.UI.Slider sliderSound;
	private UnityEngine.UI.Slider sliderMusic;

	// Use this for initialization
	void Start ()
	{
		sliderSound = GameObject.Find ("sliderSound").GetComponent<Slider> ();
		sliderMusic = GameObject.Find ("sliderMusic").GetComponent<Slider> ();
		sourceFX = GameObject.Find ("sourceFX").GetComponent<AudioSource> ();
		sourceMusic = GameObject.Find ("sourceMusic").GetComponent<AudioSource> ();
		sliderSound.value = 0.5f;
		sliderMusic.value = 0.25f;
		SetSoundLevel ();
		SetMusicLevel ();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	public void CreatePC (string className)
	{	
		PlayerPrefs.SetString ("className", className);
		Application.LoadLevel ("Main");
	}
	
	public void SetSoundLevel ()
	{
		if (sliderSound != null) {
			float level = sliderSound.value;
			PlayerPrefs.SetString ("soundLevel", level.ToString ());
			sourceFX.volume = level;
			sourceFX.Play ();
			//audio.PlayOneShot (titleSoundTest, level);
		}
	}
	
	public void SetMusicLevel ()
	{
		if (sliderMusic != null) {
			float level = sliderMusic.value;
			PlayerPrefs.SetString ("musicLevel", level.ToString ());
			sourceMusic.volume = level;
			sourceMusic.Play ();
		}
	}
}
