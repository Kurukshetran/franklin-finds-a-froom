using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

    private static string SOUND_PREFS_KEY = "SOUND_PREFS";
    private static string HIGH_SCORE_KEY = "HIGH_SCORE";

    #region Public
    // Number of coins needed to add life
    public static int coinsFor1Up = 5;

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

    // Background music to play when game is completed.
    public AudioClip bgMusicGameCompleted;

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

    // End game UI - when player loses all lives.
    public EndGameUI endGameUI;

    // Game completed UI - when player completes all levels.
    public CompletedGameUI completedGameUI;

    // UI Text displaying score
    public GameObject uiScore;

    // UI Text to to display level on intro
    public GameObject uiIntroLevel;

    // UI coin counter
    public GUITexture uiCoinCounterIcon;
    public GUIText uiCoinCounterText;

    // UI pause button
    public GUITexture uiPauseButton;

    // Control button UI
    public GameObject uiLeftButton;
    public GameObject uiRightButton;
    public GameObject uiJumpButton;

    // Level config container
    public LevelConfig levelConfig;

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

    // Coins collected leading to next 1Up
    private int coinsCollected;

    // Left side and right side spawn controllers
    private SpawnController leftSpawn;
    private SpawnController rightSpawn;

    // Controller to the fire shower spawner
    private FireShowerController fireController;

    // Handle to LivesUI script
    private LivesUI livesUI;
    #endregion

    void Awake() {
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
    public void ResetGameState(bool isStartingLevel) {
        // Clear objects off the scene
        if (isStartingLevel)
            this.ClearNPCs();

        // Setup UI
        UpdateGUI();

        // Show in-game UI
        uiIntroLevel.SetActive(true);
        uiScore.SetActive(true);
        uiLeftButton.SetActive(true);
        uiRightButton.SetActive(true);
        uiJumpButton.SetActive(true);
        uiPauseButton.enabled = true;
        uiCoinCounterIcon.enabled = true;
        uiCoinCounterText.enabled = true;

        // +1 since it starts at 0
        uiIntroLevel.guiText.text = "Level " + (currentLevel + 1);

        // Remove any end game UI
        endGameUI.HideEndGameMenu();

        // Move player to beginning state
        player.transform.position = playerStartingPosition;

        if (isStartingLevel) {
            // Suspend any fire showers until level starts
            fireController.Suspend();

            // Start level
            Invoke("StartLevel", 2);
        }
        else {
            Invoke("ContinueLevel", 2);
        }
    }

    private void StartLevel() {
        SetupSpawnPoints();
        SetupFireShowers();

        leftSpawn.enabled = true;
        rightSpawn.enabled = true;

        // Hide intro level UI
        uiIntroLevel.SetActive(false);

        // GameAnalytics tracking
        GA.API.Design.NewEvent("Level Started", currentLevel);
        GA.API.Design.NewEvent("Lives At Level:" + currentLevel, currentLives);
    }

    private void ContinueLevel() {
        uiIntroLevel.SetActive(false);

        // GameAnalytics tracking
        GA.API.Design.NewEvent("Level Continued", currentLevel);
        GA.API.Design.NewEvent("Lives At Level (continued):" + currentLevel, currentLives);
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
        coinsCollected = 0;

        // PlayerController.Respawn() will also call GameController.ResetGameState()
        playerController.Respawn(true, 0);
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

        // Cancel any pending invoke calls.
        CancelInvoke();

        // Hide any gameplay UI elements
        this.HideGameplayUI();
    }

    private void UpdateGUI() {
        if (livesUI) {
            livesUI.UpdateRemainingLives(currentLives);
            livesUI.UpdateCoinCounter(coinsCollected);
        }

        if (uiScore && uiScore.guiText) {
            uiScore.guiText.text = "Score: " + score;
        }
    }

    private void TriggerLevelComplete() {
        Debug.Log("level complete. start next level.");

        // Increment level counter
        currentLevel++;

        // If there is no next level, show end game screen.
        if (currentLevel >= levelConfig.levels.Length) {
            Invoke("StartCompletedGameState", endGameUiDelay);
        }
        // Otherwise, reset game state to go onto next level.
        else {
            ResetGameState(true);
        }
    }

    public void CheckIfLevelCompleted() {
        // Checking if any pending enemies to be spawned or if this is an endless mode level
        Level level = levelConfig.GetLevel(currentLevel);
        if ((level != null && level.endlessMode) || leftSpawn.GetNumPendingEnemies() > 0 || rightSpawn.GetNumPendingEnemies() > 0) {
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

    /**
     * Make any updates necessary on player death.
     */
    public void OnPlayerDeath() {
        // Stop the background music.
        this.StopBackgroundMusic();

        // Decrement the number of current lives.
        this.DecrementCurrentLives();
    }

    public int GetCurrentLevel() {
        return currentLevel;
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

    /**
     * Get the high score. Update if current score is greater than the high
     * score saved in the PlayerPrefs.
     */
    public int GetHighScore() {
        // Overall high score
        int highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY);
        if (score > highScore) {
            // Update the high score saved in PlayerPrefs
            highScore = score;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        }

        return highScore;
    }

    /**
     * Handler for a coin getting collected.
     */
    public void AddCoinCollected() {
        coinsCollected++;

        // Award new life and reset coin count
        if (coinsCollected == coinsFor1Up) {
            coinsCollected = 0;
            currentLives++;
        }

        UpdateGUI();
    }

    /**
     * Hide gameplay UI.
     */
    public void HideGameplayUI() {
        uiIntroLevel.SetActive(false);
        uiScore.SetActive(false);
        uiLeftButton.SetActive(false);
        uiRightButton.SetActive(false);
        uiJumpButton.SetActive(false);
        uiPauseButton.enabled = false;
        uiCoinCounterIcon.enabled = false;
        uiCoinCounterText.enabled = false;
    }

    /**
     * Select and play the background music.
     */
    public void StartBackgroundMusic() {
        // Use bgMusic1 if on level 3 or earlier.
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
    }

    /**
     * Stop playing the background music.
     */
    public void StopBackgroundMusic() {
        bgAudioSource.Stop();
    }

    /**
     * Start of the end game state after last life is lost.
     */
    private void StartEndGameState() {
        gameState = FFGameState.Ended;
        endGameUI.ShowEndGameMenu();
        endGameUI.SetGameScoreUI(score, this.GetHighScore());

        // Log high score
        GA.API.Design.NewEvent("Score:OnDeath", this.GetHighScore());
    }

    /**
     * Move into game completed state after all levels are completed.
     */
    private void StartCompletedGameState() {
        gameState = FFGameState.Ended;

        // Hide the gameplay UI elements (controls, lives, score, etc).
        this.HideGameplayUI();

        // Show the completed game UI.
        completedGameUI.ShowUI(score, this.GetHighScore());

        // Play game completed music
        bgAudioSource.clip = bgMusicGameCompleted;
        bgAudioSource.Play();

        // Log high score
        GA.API.Design.NewEvent("Score:OnComplete", this.GetHighScore());
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
