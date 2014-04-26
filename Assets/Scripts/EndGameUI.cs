using UnityEngine;
using System.Collections;

public class EndGameUI : MonoBehaviour {

    private bool isVisible;

    #region Reference to other objects
    public GameObject gameControllerContainer;
    private GameController gameController;
    #endregion

    #region UI elements to hide
    public GUITexture leftTouchButton;
    public GUITexture rightTouchButton;
    public GUITexture jumpTouchButton;
    #endregion

    #region UI elements to show
    public GameObject gameOver;
    public GameObject gameScore;
    public GameObject highScore;
    public GUITexture menuBg;
    public GUITexture exitTouchButton;
    public GUITexture restartTouchButton;
    #endregion

    void Start() {
        isVisible = false;

        if (menuBg != null) {
            menuBg.guiTexture.pixelInset = new Rect(0, 0, Screen.width, Screen.height);
        }

        if (gameControllerContainer != null) {
            gameController = gameControllerContainer.GetComponent<GameController>();
        }
    }

    void Update() {
        // Only listen for touch events if UI is showing.
        if (!isVisible) {
            return;
        }

        bool exitGame = false;
        bool restartGame = false;

        foreach (Touch touch in Input.touches) {
            if (touch.phase == TouchPhase.Began) {
                if (exitTouchButton.HitTest(touch.position)) {
                    exitGame = true;
                }
                else if (restartTouchButton.HitTest(touch.position)) {
                    restartGame = true;
                }
            }
        }

        if (!exitGame && !restartGame) {
            if (Input.GetButton("End Game Exit")) {
                exitGame = true;
            }
            else if (Input.GetButton("End Game Restart")) {
                restartGame = true;
            }
        }

        if (exitGame) {
            // @todo Implement game exit
            Debug.Log("TODO: Implement game exit");
        }
        else if (restartGame) {
            gameController.RestartGame();
        }
    }

    /**
     * Displays UI for the end game screen.
     */
    public void ShowEndGameMenu() {
        isVisible = true;

        // Hide the control buttons
        leftTouchButton.guiTexture.enabled = false;
        rightTouchButton.guiTexture.enabled = false;
        jumpTouchButton.guiTexture.enabled = false;

        // Enable and show end game menu's UI
        gameOver.SetActive(true);
        gameScore.SetActive(true);
        highScore.SetActive(true);
        menuBg.guiTexture.enabled = true;
        exitTouchButton.guiTexture.enabled = true;
        restartTouchButton.guiTexture.enabled = true;
    }

    /**
     * Hides UI for the end game screen.
     */
    public void HideEndGameMenu() {
        isVisible = false;

        // Hide the end game menu's UI elements
        gameOver.SetActive(false);
        gameScore.SetActive(false);
        highScore.SetActive(false);
        menuBg.guiTexture.enabled = false;
        exitTouchButton.guiTexture.enabled = false;
        restartTouchButton.guiTexture.enabled = false;
    }

    /**
     * Set the text to show for the game score UI.
     */
    public void SetGameScoreUI(int score) {
        if (gameScore && gameScore.guiText) {
            gameScore.guiText.text = "Score: " + score;
        }
    }
}
