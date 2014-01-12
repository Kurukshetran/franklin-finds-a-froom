using UnityEngine;
using System.Collections;

public class GroundTile : MonoBehaviour {

	private Transform tileRenderer;

	private Transform enemyCheck;

	void Awake() {
		tileRenderer = transform.FindChild("tileRenderer");
		enemyCheck = transform.FindChild("enemyCheck");
	}
	
	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "Player") {
			// Play the bounce animation on the tile
			// Should be sufficient only checking the initial contact point
			bool contactFromBelow = coll.contacts[0].normal.x == 0 && coll.contacts[0].normal.y == 1;
			if (!tileRenderer.animation.isPlaying && contactFromBelow) {
				tileRenderer.animation.Play();
			}

			// If an enemy is above the tile, flip over the enemy
			if (enemyCheck) {
				RaycastHit2D hit = Physics2D.Linecast(transform.position, enemyCheck.position, 1 << LayerMask.NameToLayer("Enemy"));
				if (hit && hit.transform) {
					EnemyController enemyController = hit.transform.GetComponent<EnemyController>();
					enemyController.SetDisabled();
				}
			}
		}
	}
}
