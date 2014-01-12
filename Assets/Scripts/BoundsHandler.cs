using UnityEngine;
using System.Collections;

public class BoundsHandler : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D other) {
		float buffer = 0.25f; // Adding a small buffer so the player doesn't get stuck warping in between to the two boundaries
		float currPos = other.gameObject.transform.position.x;
		float newPos = 0;
		if (currPos < 0) {
			newPos = (currPos + buffer) * -1;
		}
		else if (currPos > 0) {
			newPos = (currPos - buffer) * -1;
		}

		other.gameObject.transform.position = new Vector3(newPos, other.gameObject.transform.position.y);
	}
}
