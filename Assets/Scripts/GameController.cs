using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	#region Public
	// Number of lives the player can start out with
	public int startingLives = 3;
	#endregion

	#region Handlers to other GameObjects
	// UI Text displaying remaining lives
	public GameObject uiLives;

	// UI Text displaying current level
	public GameObject uiLevel;

	// UI Text displaying score
	public GameObject uiScore;

	// UI Text to to display level on intro
	public GameObject uiIntroLevel;

	// Reference to the player
	public GameObject player;

	// The player's starting position
	public Vector3 playerStartingPosition;

	// Fire shower spawn object
	public GameObject fireSpawner;
	#endregion

	#region Private
	// Current number of lives the player has remaining
	private int currentLives;

	private int currentLevel = 0;

	private int score;

	private LevelConfig levelConfig;
	private SpawnController leftSpawn;
	private SpawnController rightSpawn;

	// Controller to the fire shower spawner
	private FireShowerController fireController;
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

		// Fire shower controller
		fireController = fireSpawner.GetComponent<FireShowerController>();

		// Starting # of lives
		currentLives = startingLives;

		// Starting score
		score = 0;

		ResetGameState();
	}

	/**
	 * Reset the state of the level. Includes clearing the enemies off the screen and resetting the UI.
	 */
	public void ResetGameState() {
		// Clear objects off the scene
		GameObject[] objects = FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach(GameObject obj in objects) {
			// 9 = "Enemy" layer. 11 = "Pickup" layer. 12 = "Fire"
			if (obj.layer == 9 || obj.layer == 11 || obj.layer == 12) {
				Destroy(obj);
			}
		}
		
		// Setup UI
		UpdateGUI();

		// Show intro level UI
		uiIntroLevel.SetActive(true);
		uiIntroLevel.guiText.text = "Level " + currentLevel;

		// Move player to beginning state
		player.transform.position = playerStartingPosition;

		// Suspend any fire showers until level starts
		fireController.Suspend();

		// Start level
		Debug.Log("Starting the level in 2 seconds");
		Invoke("StartLevel", 2);
	}

	private void StartLevel() {
		Debug.Log("Starting the level: " + currentLevel);
		SetupSpawnPoints();
		SetupFireShowers();

		leftSpawn.enabled = true;
		rightSpawn.enabled = true;

		// Hide intro level UI
		uiIntroLevel.SetActive(false);
	}

	/**
	 * Setup for the fire shower spawner.
	 */
	private void SetupFireShowers() {
		Level level = levelConfig.GetLevel(currentLevel);
		if (level.fireShowers.Length > 0) {
			fireController.Setup(level.fireShowers);
		}
	}

	/**
	 * Sets up the enemy spawners.
	 */
	private	void SetupSpawnPoints() {
		Level level = levelConfig.GetLevel(currentLevel);

		// Setup configs on the left spawn
		leftSpawn.Setup(level.leftEnemies, level.leftSpawnStart, level.leftSpawnDelay, level.endlessMode);

		// Setup configs on the right spawn
		rightSpawn.Setup(level.rightEnemies, level.rightSpawnStart, level.rightSpawnDelay, level.endlessMode);
	}

	private void RestartGame() {
		currentLevel = 0;
		currentLives = startingLives;
		score = 0;
		ResetGameState();
	}

	private void UpdateGUI() {
		if (uiLives && uiLives.guiText) {
			uiLives.guiText.text = "Remaining Lives: " + currentLives;
		}

		if (uiLevel && uiLevel.guiText) {
			uiLevel.guiText.text = "Level: " + currentLevel;
		}

		if (uiScore && uiScore.guiText) {
			uiScore.guiText.text = "Score: " + score;
		}
	}

	private void TriggerLevelComplete() {
		Debug.Log("level complete. start next level.");

		currentLevel++;
		ResetGameState();
	}

	public void CheckIfLevelCompleted() {
		// Checking if any pending enemies to be spawned or if this is an endless mode level
		Level level = levelConfig.GetLevel(currentLevel);
		if (level.endlessMode || leftSpawn.GetNumPendingEnemies() > 0 || rightSpawn.GetNumPendingEnemies() > 0) {
			return;
		}
		
		// Check if any living enemies still on the scene
		GameObject[] objects = FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach(GameObject obj in objects) {
			if (obj.layer == 9) { // "Enemy" layer
				EnemyController enemyController = obj.GetComponent<EnemyController>();
				if (enemyController.CurrState != EnemyController.EnemyState.DEAD) {
					return;
				}
			}
		}
		
		TriggerLevelComplete();
	}

	public int GetCurrentLives() {
		return currentLives;
	}

	public void DecrementCurrentLives() {
		currentLives--;
		UpdateGUI();

		if (currentLives < 0) {
			RestartGame();
		}
	}

	public void AddToScore(int addPoints) {
		score += addPoints;
		UpdateGUI();
	}
	
}
