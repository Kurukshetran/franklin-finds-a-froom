using UnityEngine;
using System.Collections;

public class GroundTile : MonoBehaviour {

	private Transform tileRenderer;
	
	void Awake() {
		tileRenderer = transform.FindChild("tileRenderer");
	}
	
	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "Player") {
			// Should be sufficient only checking the initial contact point
			bool contactFromBelow = coll.contacts[0].normal.x == 0 && coll.contacts[0].normal.y == 1;
			if (!tileRenderer.animation.isPlaying && contactFromBelow)
				tileRenderer.animation.Play();
		}
	}
}
