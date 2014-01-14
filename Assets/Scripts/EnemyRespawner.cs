using UnityEngine;
using System.Collections;

public class EnemyRespawner : MonoBehaviour {

	public GameObject respawnObject;

	private SpawnController spawnController;

	void Awake() {
		spawnController = respawnObject.GetComponent<SpawnController>();
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.layer == 9) { // "Enemy" layer
			spawnController.AddEnemyToQueue(other.gameObject);
		}
	}
}
