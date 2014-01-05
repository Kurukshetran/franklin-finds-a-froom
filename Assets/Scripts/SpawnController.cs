using UnityEngine;
using System.Collections;

public class SpawnController : MonoBehaviour {

	// If true, enemies spawn going to the right
	public bool directionToRight = false;
	
	// If true, randomize the type of enemy spawned.
	public bool randomizeEnemies = false;

	// Array of enemy prefabs to spawn.
	private GameObject[] initialEnemies;

	// Array of enemies that are yet to be spawned into the level.
	private GameObject[] pendingEnemies;

	// Time in seconds between spawns.
	private float spawnDelay;

	// Time in seconds before the first spawn.
	private float spawnStart;

	void Start() {
		pendingEnemies = new GameObject[initialEnemies.Length];
		for (int i = 0; i < initialEnemies.Length; i++) {
			GameObject enemy = (GameObject)Instantiate(initialEnemies[i]);
			enemy.SetActive(false);
			pendingEnemies[i] = enemy;
		}

		InvokeRepeating("Spawn", spawnStart, spawnDelay);
	}

	private void Spawn() {
		if (pendingEnemies.Length > 0) {
			// Instantiate enemy at front of array
			GameObject enemy = pendingEnemies[0];
			enemy.SetActive(true);
			enemy.transform.position = transform.position;
			enemy.transform.rotation = transform.rotation;

			EnemyController enemyController = enemy.GetComponent<EnemyController>();
			enemyController.SetDirection(directionToRight);

			// Recreate array sans the first element
			GameObject[] remainingEnemies = new GameObject[pendingEnemies.Length - 1];
			if (pendingEnemies.Length > 1) {
				pendingEnemies.CopyTo(remainingEnemies, 1);
			}

			pendingEnemies = remainingEnemies;
		}
	}

	public void AddEnemyToQueue(GameObject enemy) {
		GameObject[] remainingEnemies = new GameObject[pendingEnemies.Length + 1];

		if (pendingEnemies.Length > 0) {
			pendingEnemies.CopyTo(remainingEnemies, 0);
		}

		enemy.SetActive(false);
		remainingEnemies[remainingEnemies.Length - 1] = enemy;

		pendingEnemies = remainingEnemies;
	}

	public int GetNumPendingEnemies() {
		return pendingEnemies.Length;
	}

	public void SetSpawnDelay(float spawnDelay) {
		this.spawnDelay = spawnDelay;
	}

	public void SetSpawnStart(float spawnStart) {
		this.spawnStart = spawnStart;
	}

	public void SetEnemies(GameObject[] enemies) {
		this.initialEnemies = enemies;
	}
}
