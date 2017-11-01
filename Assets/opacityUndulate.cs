using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class opacityUndulate : MonoBehaviour {
	public float rateOfChange = .005f;
	public float min = .6f;
	public float max = 1f;
	private SpriteRenderer sr;
	private bool increasing = false;
	private float alpha = 1f;
	// Use this for initialization
	void Start () {
		sr = gameObject.GetComponent<SpriteRenderer> ();
	    StartCoroutine("Fade");
	}
	
	// Update is called once per frame
	void Update() {

	}

	IEnumerator Fade() {
		
    //for (float f = 1f; f >= 0; f -= 0.01f) {
		while (true) {
			if (increasing) {
				alpha += rateOfChange;
				if (alpha >= max) {
					increasing = false;
				}
			} else {
				alpha -= rateOfChange;
				if (alpha <= min) {
					increasing = true;
				}
			}
			sr.color = new Color (sr.color.r, sr.color.g, sr.color.b, alpha);
			yield return null;
		}
    //}
	}
}
