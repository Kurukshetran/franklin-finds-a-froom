using UnityEngine;
using System.Collections;

public class HomeScreenUI : MonoBehaviour {

    private bool isVisible;

    private string[] menuSelectOptions = {
        "PLAY",
        "CREDITS",
        "EXIT"
    };

    // Currently selected menu index.
    private int menuSelectIndex;

    #region UI elements
    // Background
    public GUITexture homeScreenBg;

    // Left/right buttons to cycle through menu options
    public GameObject leftTouchButton;
    public GameObject rightTouchButton;

    // Menu 
    public GameObject menuSelect;
    #endregion

    #region Reference to other game objects
    // Game controller
    public GameController gameController;

    // Audio to play on menu toggling.
    public AudioClip toggleAudio;

    // AUdio to play on menu selection.
    public AudioClip selectAudio;
    #endregion

    void Awake() {
        if (homeScreenBg != null) {
            homeScreenBg.guiTexture.pixelInset = new Rect(0, 0, Screen.width, Screen.height);
        }

        menuSelectIndex = 0;

        this.ShowUI();
    }
	
	void Update () {
	    if (!isVisible)
            return;

        bool leftTouched = false;
        bool rightTouched = false;
        bool itemSelected = false;

        // Check for touchscreen input.
        foreach (Touch touch in Input.touches) {
            if (touch.phase == TouchPhase.Ended) {
                if (leftTouchButton.guiTexture.HitTest(touch.position)) {
                    leftTouched = true;
                    break;
                }
                else if (rightTouchButton.guiTexture.HitTest(touch.position)) {
                    rightTouched = true;
                    break;
                }
                else if (menuSelect.guiText.HitTest(touch.position)) {
                    itemSelected = true;
                    break;
                }
            }
        }

        // Check for keyboard input.
        if (!leftTouched && !rightTouched) {
            if (Input.GetButtonUp("Left")) {
                leftTouched = true;
            }
            else if (Input.GetButtonUp("Right")) {
                rightTouched = true;
            }
            else if (Input.GetButtonUp("Return")) {
                itemSelected = true;
            }
        }

        // Change the item in the menu select.
        if (leftTouched) {
            this.PreviousMenuItem();
        }
        else if (rightTouched) {
            this.NextMenuItem();
        }
        else if (itemSelected) {
            //@todo do whatever the thing selected is
            this.SelectMenuItem();
        }
	}

    /**
     * Show all home screen UI elements.
     */
    public void ShowUI() {
        isVisible = true;

        homeScreenBg.guiTexture.enabled = true;
        leftTouchButton.SetActive(true);
        rightTouchButton.SetActive(true);
        menuSelect.SetActive(true);
    }

    /**
     * Hide all home screen UI elements.
     */
    private void HideUI() {
        isVisible = false;

        homeScreenBg.guiTexture.enabled = false;
        leftTouchButton.SetActive(false);
        rightTouchButton.SetActive(false);
        menuSelect.SetActive(false);
    }

    /**
     * Change menu select to the previous item in the list.
     */
    private void PreviousMenuItem() {
        menuSelectIndex--;

        if (menuSelectIndex < 0) {
            menuSelectIndex = menuSelectOptions.Length - 1;
        }

        menuSelect.guiText.text = menuSelectOptions[menuSelectIndex];

        AudioSource.PlayClipAtPoint(toggleAudio, transform.position);
    }

    /**
     * Change menu select to the next item in the list.
     */
    private void NextMenuItem() {
        menuSelectIndex++;

        if (menuSelectIndex >= menuSelectOptions.Length) {
            menuSelectIndex = 0;
        }

        menuSelect.guiText.text = menuSelectOptions[menuSelectIndex];

        AudioSource.PlayClipAtPoint(toggleAudio, transform.position);
    }

    /**
     * Select the currently selected menu item.
     */
    private void SelectMenuItem() {
        AudioSource.PlayClipAtPoint(selectAudio, transform.position);

        // Not really elegant, but whatever for now.
        switch (menuSelectIndex) {
        case 0:
            this.Play();
            break;
        case 1:
            this.Credits();
            break;
        case 2:
            this.Exit();
            break;
        }
    }

    /**
     * Start the game.
     */
    private void Play() { 
        // Hide the home screen UI.
        this.HideUI();

        // Start the game.
        gameController.StartGame();
    }

    /**
     * Open the Credits screen.
     */
    private void Credits() {
        Debug.Log("TODO: open the credits screen");
    }

    /**
     * Exit the game.
     */
    private void Exit() {
        Application.Quit();
    }
}
