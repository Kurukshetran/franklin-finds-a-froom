using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	private int currentLevel = 0;

	private LevelConfig levelConfig;
	private SpawnController leftSpawn;
	private SpawnController rightSpawn;

	void Awake() {
		// Level config
		levelConfig = GameObject.FindGameObjectWithTag("LevelConfig").GetComponent<LevelConfig>();

		// Spawn points
		GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
		foreach(GameObject spawnPoint in spawnPoints) {
			if (spawnPoint.name == "leftSpawn") {
				leftSpawn = spawnPoint.GetComponent<SpawnController>();
				leftSpawn.enabled = false;
			}
			else if (spawnPoint.name == "rightSpawn") {
				rightSpawn = spawnPoint.GetComponent<SpawnController>();
				rightSpawn.enabled = false;
			}
		}

//		Invoke("StartLevel", 5);
		StartLevel();
	}

	public void StartLevel() {
		SetupSpawnPoints();

		leftSpawn.enabled = true;
		rightSpawn.enabled = true;
	}

	public void SetupSpawnPoints() {
		Level level = levelConfig.GetLevel(currentLevel);

		// Setup configs on the left spawn
		leftSpawn.SetSpawnStart(level.leftSpawnStart);
		leftSpawn.SetSpawnDelay(level.leftSpawnDelay);
		leftSpawn.SetEnemies(level.leftEnemies);

		// Setup configs on the right spawn
		rightSpawn.SetSpawnStart(level.rightSpawnStart);
		rightSpawn.SetSpawnDelay(level.rightSpawnDelay);
		rightSpawn.SetEnemies(level.rightEnemies);
	}
}
