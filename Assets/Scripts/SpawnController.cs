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

	private bool endlessMode;
	private int endlessSpawnIndex;

	public void Setup(GameObject[] enemies, float start, float delay, bool endless) {
		initialEnemies = enemies;
		spawnStart = start;
		spawnDelay = delay;
		endlessMode = endless;
		endlessSpawnIndex = -1;

		pendingEnemies = new GameObject[initialEnemies.Length];
		for (int i = 0; i < initialEnemies.Length; i++) {
			GameObject enemy = (GameObject)Instantiate(initialEnemies[i]);
			enemy.SetActive(false);
			pendingEnemies[i] = enemy;
		}

		// First, cancel any previously invoked Spawn() calls
		CancelInvoke();

		// Then repeat Spawns at set intervals
		InvokeRepeating("Spawn", spawnStart, spawnDelay);
	}

	private void Spawn() {
		if (endlessMode) {
			if (endlessSpawnIndex < initialEnemies.Length - 1) {
				endlessSpawnIndex++;
			}
			else {
				endlessSpawnIndex = 0;
			}

			GameObject enemy = (GameObject)Instantiate(initialEnemies[endlessSpawnIndex]);
				enemy.transform.position = transform.position;
			enemy.transform.rotation = transform.rotation;
			
			EnemyController enemyController = enemy.GetComponent<EnemyController>();
			enemyController.SetDirection(directionToRight);
		}
		else if (pendingEnemies.Length > 0) {
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
				for (int i = 1; i < pendingEnemies.Length; i++) {
					remainingEnemies[i - 1] = pendingEnemies[i];
				}
			}

			pendingEnemies = remainingEnemies;
		}
	}

	public void AddEnemyToQueue(GameObject enemy) {
		if (endlessMode) {
			Destroy(enemy);
		}
		else {
			GameObject[] remainingEnemies = new GameObject[pendingEnemies.Length + 1];

			if (pendingEnemies.Length > 0) {
				pendingEnemies.CopyTo(remainingEnemies, 0);
			}

			enemy.SetActive(false);
			remainingEnemies[remainingEnemies.Length - 1] = enemy;

			pendingEnemies = remainingEnemies;
		}
	}

	public int GetNumPendingEnemies() {
		if (pendingEnemies != null)
			return pendingEnemies.Length;
		else
			return -1;
	}
}
