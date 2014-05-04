using UnityEngine;
using System.Collections;

public class PauseMenuUI : MonoBehaviour {

    #region Reference to other game objects
    // Home screen controller
    public HomeScreenUI homeScreenUI;

    // Game controller
    public GameController gameController;
    #endregion

    #region UI elements to hide
    public GUITexture leftTouchButton;
    public GUITexture rightTouchButton;
    public GUITexture jumpTouchButton;
    public GameObject uiLevelIntro;
    #endregion

    #region Pause UI elements to show
    public GUITexture pauseMenuBg;
    public GUITexture exitTouchButton;
    public GUITexture resumeTouchButton;
    public GUITexture soundOffTouchButton;
    public GUITexture soundOnTouchButton;
    #endregion

    // Flag tracking whether or not game is paused.
    private bool isGamePaused;

    private float savedTimeScale;

    void Awake() {
        isGamePaused = false;

        // Set the width and height of the pause menu bg to match the screen size
        if (pauseMenuBg != null) {
            pauseMenuBg.guiTexture.pixelInset = new Rect(0, 0, Screen.width, Screen.height);
        }
    }

    // Update is called once per frame
    void Update () {
        // Only allow for Puase menu controls to work if gameplay is in progress
        if (gameController.GameState != GameController.FFGameState.InProgress) {
            return;
        }

        if (Input.GetButtonUp("Pause")) {
            Debug.Log("game state: " + gameController.GameState);
            // Pause the game
            if (!isGamePaused) {
                this.Pause();
            }
            // Unpause the game
            else {
                this.Unpause();
            }
        }

        // Check touches to the pause menu items
        if (isGamePaused) {
            bool endGame = false;
            bool unpause = false;
            bool unmute = false;
            bool mute = false;

            foreach (Touch touch in Input.touches) {
                if (touch.phase == TouchPhase.Ended) {
                    if (exitTouchButton.HitTest(touch.position)) {
                        endGame = true;
                        break;
                    }
                    else if (resumeTouchButton.HitTest(touch.position)) {
                        unpause = true;
                        break;
                    }
                    else if (soundOffTouchButton.guiTexture.enabled && soundOffTouchButton.HitTest(touch.position)) {
                        unmute = true;
                        break;
                    }
                    else if (soundOnTouchButton.guiTexture.enabled && soundOnTouchButton.HitTest(touch.position)) {
                        mute = true;
                        break;
                    }
                }
            }

            // If no touch events are detected, check for keyboard input.
            if (!endGame && !unpause && !unmute && !mute) {
                if (Input.GetButtonUp("End Game Exit")) {
                    endGame = true;
                }
                else if (Input.GetButtonUp("Pause Menu Sound")) {
                    if (soundOffTouchButton.guiTexture.enabled) {
                        unmute = true;
                    }
                    else if (soundOnTouchButton.guiTexture.enabled) {
                        mute = true;
                    }
                }
            }

            if (endGame) {
                // End game and return to home screen.
                this.EndGame();
            }
            else if (unpause) {
                // Resume gameplay.
                this.Unpause();
            }
            else if (unmute) {
                // Turn sound back on.
                this.UnmuteSound();
            }
            else if (mute) {
                // Turn sound off.
                this.MuteSound();
            }
        }
    }

    /**
     * Pause the game.
     */
    private void Pause() {
        isGamePaused = true;
        
        // Setting time scale to 0 for any components that rely on time to execute.
        savedTimeScale = Time.timeScale;
        Time.timeScale = 0;
        
        // Pause audio
        AudioListener.pause = true;
        
        // Show the Pause Menu elements
        this.ShowMenuUI();
        
        // Hide the UI controls
        this.HideControlsUI();
    }

    /**
     * Unpause the game and resume gameplay.
     */
    private void Unpause() {
        isGamePaused = false;
        
        // Restore time scale
        Time.timeScale = savedTimeScale;
        
        // Unpause audio
        AudioListener.pause = false;
        
        // Hide the Pause Menu elements
        this.HideMenuUI();
        
        // Show the UI controls
        this.ShowControlsUI();
    }

    /**
     * Hide the UI for the game controls.
     */
    private void HideControlsUI() {
        leftTouchButton.guiTexture.enabled = false;
        rightTouchButton.guiTexture.enabled = false;
        jumpTouchButton.guiTexture.enabled = false;

        uiLevelIntro.SetActive(false);
    }

    /**
     * Show the UI for the game controls.
     */
    private void ShowControlsUI() {
        leftTouchButton.guiTexture.enabled = true;
        rightTouchButton.guiTexture.enabled = true;
        jumpTouchButton.guiTexture.enabled = true;
    }

    /**
     * Hide the Pause Menu UI elements.
     */
    private void HideMenuUI() {
        pauseMenuBg.guiTexture.enabled = false;
        exitTouchButton.guiTexture.enabled = false;
        resumeTouchButton.guiTexture.enabled = false;
        soundOffTouchButton.guiTexture.enabled = false;
        soundOnTouchButton.guiTexture.enabled = false;
    }
    
    /**
     * Show the Pause Menu UI elements.
     */
    private void ShowMenuUI() {
        pauseMenuBg.guiTexture.enabled = true;
        exitTouchButton.guiTexture.enabled = true;
        resumeTouchButton.guiTexture.enabled = true;
        if (AudioListener.volume == 0) {
            soundOffTouchButton.guiTexture.enabled = true;
            soundOnTouchButton.guiTexture.enabled = false;
        }
        else {
            soundOnTouchButton.guiTexture.enabled = true;
            soundOffTouchButton.guiTexture.enabled = false;
        }
    }

    /**
     *  Mute the sound and change UI if pause menu is open.
     */
    private void MuteSound() {
        AudioListener.volume = 0;

        if (isGamePaused) {
            soundOffTouchButton.guiTexture.enabled = true;
            soundOnTouchButton.guiTexture.enabled = false;
        }
    }

    /**
     * Unmute the sound and change UI if pause menu is open.
     */
    private void UnmuteSound() {
        AudioListener.volume = 1;

        if (isGamePaused) {
            soundOffTouchButton.guiTexture.enabled = false;
            soundOnTouchButton.guiTexture.enabled = true;
        }
    }

    /**
     * End the game and return to the home screen.
     */
    private void EndGame() {
        // Unpause the game state
        this.Unpause();

        // Hide gameplay UI elements
        this.HideMenuUI();

        // Show home screen
        homeScreenUI.ShowUI();

        // Allow GameController to clean up whatever it needs to clean up.
        gameController.EndGame();
    }

}
