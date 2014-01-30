using UnityEngine;
using System.Collections;

public class GroundTile : MonoBehaviour {

	private Transform tileRenderer;

	private Transform enemyCheckLeft;
	private Transform enemyCheckRight;

	void Awake() {
		tileRenderer = transform.FindChild("tileRenderer");
		enemyCheckLeft = transform.FindChild("enemyCheck_left");
		enemyCheckRight = transform.FindChild("enemyCheck_right");
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
			if (enemyCheckLeft && enemyCheckRight) {
				RaycastHit2D hitLeft = Physics2D.Linecast(transform.position, enemyCheckLeft.position, 1 << LayerMask.NameToLayer("Enemy"));
				RaycastHit2D hitRight = Physics2D.Linecast(transform.position, enemyCheckRight.position, 1 << LayerMask.NameToLayer("Enemy"));
				RaycastHit2D hit = hitLeft ? hitLeft : hitRight;

				if (hit && hit.transform) {
					EnemyController enemyController = hit.transform.GetComponent<EnemyController>();
					enemyController.OnBottomBump();
				}

				// Bumping coin picks it up
				RaycastHit2D hitLeftPickup = Physics2D.Linecast(transform.position, enemyCheckLeft.position, 1 << LayerMask.NameToLayer("Pickup"));
				RaycastHit2D hitRightPickup = Physics2D.Linecast(transform.position, enemyCheckRight.position, 1 << LayerMask.NameToLayer("Pickup"));
				RaycastHit2D hitPickup = hitLeftPickup ? hitLeftPickup : hitRightPickup;
				
				if (hitPickup && hitPickup.transform) {
					CoinController coin = hitPickup.transform.GetComponent<CoinController>();
					coin.Pickup();
				}
			}
		}
	}
}
