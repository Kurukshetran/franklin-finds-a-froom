using UnityEngine;
using System.Collections;

public class CompletedGameUI : MonoBehaviour {

    private const string COMPLETED_TEXT = "Thanks for playing!\nMore levels coming soon...";
    private const string GAME_SCORE_TEXT = "Game Score: ";
    private const string HIGH_SCORE_TEXT = "High Score: ";

    private bool isVisible;

    #region Game completed screen UI
    // Background
    public GameObject screenBg;

    // Exit Button
    public GameObject exitTouchButton;

    // Restart Button
    public GameObject restartTouchButton;

    // Primary text
    public GameObject mainText;

    // Game score text
    public GameObject gameScoreUi;

    // High Score text
    public GameObject highScoreUi;
    #endregion

    #region References to other game objects
    // Game controller
    public GameController gameController;

    // Home screen
    public HomeScreenUI homeScreenUI;
    #endregion

    // Called once per object lifetime after all Awakes.
    void Start() {
        this.isVisible = false;

        // Fit background to the screen size.
        if (screenBg != null) {
            screenBg.guiTexture.pixelInset = new Rect(0, 0, Screen.width, Screen.height);
        }
    }

    // Update is called once per frame
    void Update () {
        if (!this.isVisible)
            return;

        bool exitGame = false;
        bool restartGame = false;

        // Listen for touches on exit and restart
        foreach (Touch touch in Input.touches) {
            if (touch.phase == TouchPhase.Ended) {
                if (exitTouchButton.guiTexture.HitTest(touch.position)) {
                    exitGame = true;
                    break;
                }
                else if (restartTouchButton.guiTexture.HitTest(touch.position)) {
                    restartGame = true;
                    break;
                }
            }
        }

        // Check keyboard input if no touchscreen input
        if (!exitGame && !restartGame) {
            if (Input.GetButtonUp("End Game Exit")) {
                exitGame = true;
            }
            else if (Input.GetButtonUp("End Game Restart")) {
                restartGame = true;
            }
        }

        if (exitGame) {
            // UI
            this.HideUI();
            homeScreenUI.ShowUI();

            // Game cleanup
            gameController.EndGame();
        }
        else if (restartGame) {
            // UI
            this.HideUI();

            // Restart the game
            gameController.StartGame();
        }
    }

    /**
     * Display the UI and set text where necessary.
     */
    public void ShowUI(int gameScore, int highScore) {
        this.isVisible = true;

        // Activate all UI elements
        screenBg.SetActive(true);
        mainText.SetActive(true);
        gameScoreUi.SetActive(true);
        highScoreUi.SetActive(true);
        exitTouchButton.SetActive(true);
        restartTouchButton.SetActive(true);

        // Set main text
        mainText.guiText.text = COMPLETED_TEXT;

        // If scores are 0, display "000".
        string strGameScore;
        if (gameScore == 0) {
            strGameScore = "000";
        }
        else {
            strGameScore = gameScore.ToString();
        }

        string strHighScore;
        if (highScore == 0) {
            strHighScore = "000";
        }
        else {
            strHighScore = highScore.ToString();
        }

        // Set the score values
        gameScoreUi.guiText.text = GAME_SCORE_TEXT + strGameScore;
        highScoreUi.guiText.text = HIGH_SCORE_TEXT + strHighScore;
    }

    /**
     * Hide the UI.
     */
    public void HideUI() {
        this.isVisible = false;

        // Deactivate the UI elements
        screenBg.SetActive(false);
        mainText.SetActive(false);
        gameScoreUi.SetActive(false);
        highScoreUi.SetActive(false);
        exitTouchButton.SetActive(false);
        restartTouchButton.SetActive(false);
    }
}
