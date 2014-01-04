﻿using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	#region Public
	// Number of lives the player can start out with
	public int startingLives = 3;
	#endregion

	#region Handlers to other GameObjects
	// UI Text displaying remaining lives
	public GameObject uiLives;

	// UI Text display current level
	public GameObject uiLevel;
	#endregion

	#region Private
	// Current number of lives the player has remaining
	private int currentLives;

	private int currentLevel = 0;

	private LevelConfig levelConfig;
	private SpawnController leftSpawn;
	private SpawnController rightSpawn;
	#endregion

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

		ResetGameState();
	}

	private void ResetGameState() {
		// Clear any enemies off the scene
		GameObject[] objects = FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach(GameObject obj in objects) {
			if (obj.layer == 9) { // "Enemy" layer
				Destroy(obj);
			}
		}

		// Starting # of lives
		currentLives = startingLives;
		
		// Setup UI
		UpdateGUI();

		// Start level
		Debug.Log("Starting the level in 2 seconds");
		Invoke("StartLevel", 2);
	}

	private void StartLevel() {
		Debug.Log("Starting the level");
		SetupSpawnPoints();

		leftSpawn.enabled = true;
		rightSpawn.enabled = true;
	}

	private	 void SetupSpawnPoints() {
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

	private void UpdateGUI() {
		if (uiLives && uiLives.guiText) {
			uiLives.guiText.text = "Remaining Lives: " + currentLives;
		}

		if (uiLevel && uiLevel.guiText) {
			uiLevel.guiText.text = "Level: " + currentLevel;
		}
	}

	public int GetCurrentLives() {
		return currentLives;
	}

	public void DecrementCurrentLives() {
		currentLives--;
		UpdateGUI();

		if (currentLives < 0) {
			Debug.Log("should reset game state");
			// Restart the game
			ResetGameState();
		}
	}
}