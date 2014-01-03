using UnityEngine;
using System.Collections;

public class SpawnController : MonoBehaviour {

	// If true, enemies spawn going to the right
	public bool directionToRight = false;
	
	// If true, randomize the type of enemy spawned.
	public bool randomizeEnemies = false;

	// Array of enemy prefabs to spawn.
	private GameObject[] enemies;

	// Time in seconds between spawns.
	private float spawnDelay;

	// Time in seconds before the first spawn.
	private float spawnStart;

	void Start() {
		InvokeRepeating("Spawn", spawnStart, spawnDelay);
	}

	private void Spawn() {
		GameObject enemy = (GameObject)Instantiate(enemies[0], transform.position, transform.rotation);
		EnemyController enemyController = enemy.GetComponent<EnemyController>();
		enemyController.SetDirection(directionToRight);
	}

	public void SetSpawnDelay(float spawnDelay) {
		this.spawnDelay = spawnDelay;
	}

	public void SetSpawnStart(float spawnStart) {
		this.spawnStart = spawnStart;
	}

	public void SetEnemies(GameObject[] enemies) {
		this.enemies = enemies;
	}
}
