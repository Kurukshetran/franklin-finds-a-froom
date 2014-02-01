using UnityEngine;
using System.Collections;

public class DestroyLayer : MonoBehaviour {

	private void OnTriggerEnter2D(Collider2D other) {
		Destroy(other.gameObject);
	}
}
