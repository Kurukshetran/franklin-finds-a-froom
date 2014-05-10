using UnityEngine;
using System.Collections;

public class EndGameUI : MonoBehaviour {

    private bool isVisible;

    #region Reference to other objects
    public GameController gameController;

    // Home screen controller
    public HomeScreenUI homeScreenUI;
    #endregion

    #region UI elements to hide
    public GameObject leftTouchButton;
    public GameObject rightTouchButton;
    public GameObject jumpTouchButton;
    #endregion

    #region UI elements to show
    public GameObject gameOverUi;
    public GameObject gameScoreUi;
    public GameObject highScoreUi;
    public GUITexture menuBg;
    public GUITexture exitTouchButton;
    public GUITexture restartTouchButton;
    #endregion

    void Start() {
        isVisible = false;

        if (menuBg != null) {
            menuBg.guiTexture.pixelInset = new Rect(0, 0, Screen.width, Screen.height);
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
            if (touch.phase == TouchPhase.Ended) {
                if (exitTouchButton.HitTest(touch.position)) {
                    exitGame = true;
                    break;
                }
                else if (restartTouchButton.HitTest(touch.position)) {
                    restartGame = true;
                    break;
                }
            }
        }

        if (!exitGame && !restartGame) {
            if (Input.GetButtonUp("End Game Exit")) {
                exitGame = true;
            }
            else if (Input.GetButtonUp("End Game Restart")) {
                restartGame = true;
            }
        }

        if (exitGame) {
            // Handle the UI stuff
            this.HideEndGameMenu();
            homeScreenUI.ShowUI();

            // Wrap up anything the GameController needs to wrap up.
            gameController.EndGame();
        }
        else if (restartGame) {
            this.HideEndGameMenu();
            gameController.StartGame();
        }
    }

    /**
     * Displays UI for the end game screen.
     */
    public void ShowEndGameMenu() {
        isVisible = true;

        // Hide the control buttons
        leftTouchButton.SetActive(false);
        rightTouchButton.SetActive(false);
        jumpTouchButton.SetActive(false);

        // Enable and show end game menu's UI
        gameOverUi.SetActive(true);
        gameScoreUi.SetActive(true);
        highScoreUi.SetActive(true);
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
        gameOverUi.SetActive(false);
        gameScoreUi.SetActive(false);
        highScoreUi.SetActive(false);
        menuBg.guiTexture.enabled = false;
        exitTouchButton.guiTexture.enabled = false;
        restartTouchButton.guiTexture.enabled = false;
    }

    /**
     * Set the text to show for the game score UI.
     */
    public void SetGameScoreUI(int score, int highScore) {
        // Current game score
        if (gameScoreUi && gameScoreUi.guiText) {
            string strScore;
            if (score == 0) {
                strScore = "000";
            }
            else {
                strScore = score.ToString();
            }

            gameScoreUi.guiText.text = "Score: " + strScore;
        }

        // Display to the UI
        if (highScoreUi && highScoreUi.guiText) {
            string strHighScore;
            if (highScore == 0) {
                strHighScore = "000";
            }
            else {
                strHighScore = highScore.ToString();
            }
            
            highScoreUi.guiText.text = "High Score: " + strHighScore;
        }
    }
}
