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

	private GameObject pcViz;
	private ActorVizBehavior avb;

	private Sprite[] texturesPC;
	private GameObject optSex;
	private GameObject optClass;
	private OptionScrollerBehavior obSex, obClass;

	void Start ()
	{
		texturesPC = Resources.LoadAll<Sprite> ("Textures/Player");
		sliderSound = GameObject.Find ("sliderSound").GetComponent<Slider> ();
		sliderMusic = GameObject.Find ("sliderMusic").GetComponent<Slider> ();
		sourceFX = GameObject.Find ("sourceFX").GetComponent<AudioSource> ();
		sourceMusic = GameObject.Find ("sourceMusic").GetComponent<AudioSource> ();
		pcViz = GameObject.Find ("pcViz");
		avb = pcViz.GetComponent<ActorVizBehavior> ();
		optSex = GameObject.Find ("optSex");
		obSex = optSex.GetComponent<OptionScrollerBehavior> ();
		optClass = GameObject.Find ("optClass");
		obClass = optClass.GetComponent<OptionScrollerBehavior> ();
		sliderSound.value = 0.5f;
		sliderMusic.value = 0.25f;
		SetSoundLevel ();
		SetMusicLevel ();
	}
	
	void Update ()
	{
		string pcString = obClass.text.ToLower ();
		if (obSex.text == "Male") {
			pcString = pcString + "_m";
		} else {
			pcString = pcString + "_f";
		}
		Debug.Log (pcString);
		if (avb.sprite1.name != pcString) {
			avb.sprite1 = FindSpriteInTextures (pcString, texturesPC);
			avb.sprite2 = FindSpriteInTextures (pcString + "2", texturesPC);
		}
	}

	public Sprite FindSpriteInTextures (string spriteName, Sprite[] textures)
	{
		for (int i=0; i<textures.Length; i++) {
			if (textures [i].name == spriteName) {
				return textures [i];
			} 
		}
		return null;
	}

	public void PCFromOption ()
	{
		CreatePC (obClass.text.ToLower () + "_" + obSex.text.ToString ().ToLower () [0]);
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
