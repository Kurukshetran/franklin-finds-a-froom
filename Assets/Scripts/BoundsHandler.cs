using UnityEngine;
using System.Collections;

public class BoundsHandler : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D other) {
		float newX = other.gameObject.transform.position.x * -1;
		other.gameObject.transform.position = new Vector3(newX, other.gameObject.transform.position.y);
	}
}
