using UnityEngine;
using System.Collections;

public class SpawnController : MonoBehaviour {

	// Array of enemy prefabs to spawn.
	public GameObject[] enemyPrefabs;

	// Time in seconds between spawns.
	public float spawnDelay;

	// Time in seconds before the first spawn.
	public float spawnStart;

	// 
	public bool directionToRight = false;

	// If true, randomize the type of enemy spawned.
	public bool randomizeEnemies = false;

	void Start() {
		InvokeRepeating("Spawn", spawnStart, spawnDelay);
	}

	void Spawn() {
		GameObject enemy = (GameObject)Instantiate(enemyPrefabs[0], transform.position, transform.rotation);
		EnemyController enemyController = enemy.GetComponent<EnemyController>();
		enemyController.SetDirection(directionToRight);
	}
}
