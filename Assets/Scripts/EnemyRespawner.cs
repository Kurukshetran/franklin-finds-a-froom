using UnityEngine;
using System.Collections;

public class EnemyRespawner : MonoBehaviour {

	// Spawn object to add the enemy back into
	public GameObject respawnObject;

	// Points deducted for allowing a respawn
	public int pointDeduction;

	// Location to draw the point deduction text
	public GameObject pointDeductionPosition;

	// Reference to SpawnController script
	private SpawnController spawnController;

	void Awake() {
		spawnController = respawnObject.GetComponent<SpawnController>();
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.layer == 9) { // "Enemy" layer
			// Add enemy back into the queue to respawn
			spawnController.AddEnemyToQueue(other.gameObject);

			// Deduct points and set position where points animation should start from
			EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
			if (enemy) {
				enemy.AddPoints(pointDeduction, pointDeductionPosition.transform.position);
			}
		}
		// "Pickup" layer objects get destroyed
		else if (other.gameObject.layer == 11) {
			Destroy(other.gameObject);
		}

	}
}
