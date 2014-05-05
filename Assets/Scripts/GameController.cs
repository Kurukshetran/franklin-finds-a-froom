using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

    private static string SOUND_PREFS_KEY = "SOUND_PREFS";

    #region Public
    // Number of lives the player can start out with
    public int startingLives = 3;

    // Intended for development. Sets the starting level.
    public int startingLevel = 0;

    // Number of seconds before showing end game UI.
    public int endGameUiDelay = 2;

    // Background music.
    public AudioSource bgAudioSource;
    public AudioClip bgMusic1;
    public AudioClip bgMusic2;

    // Gameplay state
    public enum FFGameState {
        NotInGame,
        InProgress,
        Ended
    };
    private FFGameState gameState;
    public FFGameState GameState {
        get { return gameState; }
    }
    #endregion

    #region Handlers to other GameObjects
    // Container where the life icons will go
    public GameObject uiLivesContainer;

    // Container for end game UI elements
    public GameObject uiEndGameContainer;

    // UI Text displaying score
    public GameObject uiScore;

    // UI Text to to display level on intro
    public GameObject uiIntroLevel;

    // Control button UI
    public GameObject uiLeftButton;
    public GameObject uiRightButton;
    public GameObject uiJumpButton;

    // Level config container
    public GameObject levelConfigContainer;

    // Reference to the player
    public GameObject player;

    // Reference to the PlayerController
    private PlayerController playerController;

    // The player's starting position
    public Vector3 playerStartingPosition;

    // Fire shower spawn object
    public GameObject fireSpawner;
    #endregion

    #region Private
    // Current number of lives the player has remaining
    private int currentLives;

    private int currentLevel = 0;

    // Total points accumulated
    private int score;

    // Total # of coins collected
    private int coinsCollected;

    // Number of coins needed to add life
    private int coinsFor1Up = 10;

    private LevelConfig levelConfig;
    private SpawnController leftSpawn;
    private SpawnController rightSpawn;

    // Controller to the fire shower spawner
    private FireShowerController fireController;

    // Handle to LivesUI script
    private LivesUI livesUI;

    // EndGameUI script
    private EndGameUI endGameUI;
    #endregion

    void Awake() {
        // Level config
        levelConfig = levelConfigContainer.GetComponent<LevelConfig>();

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

        // Player controller
        playerController = player.GetComponent<PlayerController>();

        // Fire shower controller
        fireController = fireSpawner.GetComponent<FireShowerController>();

        // Controls lives icon
        livesUI = uiLivesContainer.GetComponent<LivesUI>();

        // Starting # of lives
        currentLives = startingLives;

        // Starting level
        currentLevel = startingLevel;

        // Starting score
        score = 0;

        // Starting coins
        coinsCollected = 0;

        // Controls the end game UI
        endGameUI = uiEndGameContainer.GetComponent<EndGameUI>();

        // Game hasn't started yet.
        gameState = FFGameState.NotInGame;

        // Saved PlayerPrefs settings
        if (PlayerPrefs.GetInt(SOUND_PREFS_KEY, 1) == 1) {
            AudioListener.volume = 1;
        }
        else {
            AudioListener.volume = 0;
        }
    }

    /**
     * Reset the state of the level. Includes clearing the enemies off the screen and resetting the UI.
     */
    public void ResetGameState() {
        // Clear objects off the scene
        this.ClearNPCs();

        // Setup UI
        UpdateGUI();

        // Show in-game UI
        uiIntroLevel.SetActive(true);
        uiScore.SetActive(true);
        uiLeftButton.SetActive(true);
        uiRightButton.SetActive(true);
        uiJumpButton.SetActive(true);

        // +1 since it starts at 0
        uiIntroLevel.guiText.text = "Level " + (currentLevel + 1);

        // Remove any end game UI
        endGameUI.HideEndGameMenu();

        // Move player to beginning state
        player.transform.position = playerStartingPosition;

        // Suspend any fire showers until level starts
        fireController.Suspend();

        // Start the music
        // Use bgMusic1 if on a level 3 or earlier.
        if (currentLevel < 3) {
            bgAudioSource.clip = bgMusic1;
        }
        // Randomly select the background music to play.
        else {
            System.Random random = new System.Random();
            int randResult = random.Next(0, 2);
            if (randResult == 1) {
                bgAudioSource.clip = bgMusic1;
            }
            else {
                bgAudioSource.clip = bgMusic2;
            }
        }
        bgAudioSource.Play();


        // Start level
        Invoke("StartLevel", 2);
    }

    private void StartLevel() {
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
    private void SetupSpawnPoints() {
        Level level = levelConfig.GetLevel(currentLevel);

        // Setup configs on the left spawn
        leftSpawn.Setup(level.leftSpawnConfigs, level.respawnDelay, level.endlessMode);

        // Setup configs on the right spawn
        rightSpawn.Setup(level.rightSpawnConfigs, level.respawnDelay, level.endlessMode);
    }

    /**
     * Set initial starting values after all lives have been lost and the game's restarting.
     */
    public void StartGame() {
        gameState = FFGameState.InProgress;

        // Starting values
        currentLevel = startingLevel;
        currentLives = startingLives;
        score = 0;

        // PlayerController.Respawn() will also call GameController.ResetGameState()
        playerController.Respawn();
    }

    /**
     * Handle anything that needs to be done when gameplay ends/exits.
     */
    public void EndGame() {
        gameState = FFGameState.NotInGame;

        // Stops music
        this.StopBackgroundMusic();

        // Stops spawning
        leftSpawn.StopSpawning();
        rightSpawn.StopSpawning();

        // Removes any NPCs in the level
        this.ClearNPCs();
    }

    private void UpdateGUI() {
        if (livesUI) {
            livesUI.UpdateRemainingLives(currentLives);
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
            Invoke("StartEndGameState", endGameUiDelay);
        }
    }

    public void AddToScore(int addPoints) {
        score += addPoints;
        UpdateGUI();
    }

    public void AddCoinCollected() {
        coinsCollected++;
    }

    /**
     * Stop playing the background music.
     */
    public void StopBackgroundMusic() {
        bgAudioSource.Stop();
    }

    /**
     *  Start of the end game state after last life is lost.
     */
    private void StartEndGameState() {
        gameState = FFGameState.Ended;
        endGameUI.ShowEndGameMenu();
        endGameUI.SetGameScoreUI(score);
    }

    /**
     * Remove all the non-playable characters from the game.
     */
    private void ClearNPCs() {
        GameObject[] objects = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach(GameObject obj in objects) {
            // 9 = "Enemy" layer. 11 = "Pickup" layer. 12 = "Fire"
            if (obj.layer == 9 || obj.layer == 11 || obj.layer == 12) {
                Destroy(obj);
            }
        }
    }

}
