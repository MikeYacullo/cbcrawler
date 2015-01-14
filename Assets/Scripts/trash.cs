using UnityEngine;
using System.Collections;

public class trash : MonoBehaviour
{

	public GameObject actorVizPrefab;

	// Use this for initialization
	void Start ()
	{
		for (int i=0; i<10; i++) {
			GameObject viz = (GameObject)Instantiate (actorVizPrefab, new Vector3 (i * 2, i * 2, 0), Quaternion.identity);
			ActorVizBehavior b = viz.GetComponent<ActorVizBehavior> ();
			if (i == 2) {
				b.isAnimated = false;
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
