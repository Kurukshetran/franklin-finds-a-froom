using UnityEngine;
using System.Collections;

public class CreditsMenuUI : MonoBehaviour {

    private static string CREDITS = "CREATED BY:\n@jonathanjuy\n\nMUSIC BY:\n@IAmPeaTV";

    // Is Credits screen currently visible.
    private bool isVisible;

    #region Credits UI elements
    // Object for the on-screen text
    public GameObject creditsText;

    // Touch button to exit the Credits screen.
    public GameObject backTouchButton;
    #endregion

    #region References to other objects
    // Home screen.
    public HomeScreenUI homeScreen;
    #endregion

    void Awake() {
        this.isVisible = false;
    }
	
	void Update() {
        if (!this.isVisible)
            return;

        bool exitCredits = false;

        // Check Android back button
        if (Input.GetButtonUp("Back")) {
            exitCredits = true;
        }

        // Check UI touch button
        if (!exitCredits) {
            foreach (Touch touch in Input.touches) {
                if (touch.phase == TouchPhase.Ended && backTouchButton.guiTexture.HitTest(touch.position)) {
                    exitCredits = true;
                    break;
                }
            }
        }

        if (exitCredits) {
            this.HideUI();
        }

	}

    /**
     * Show the Credits screen.
     */
    public void ShowUI() {
        this.isVisible = true;

        // Set credits text
        creditsText.guiText.text = CREDITS;

        // Show Credits UI
        creditsText.SetActive(true);
        backTouchButton.SetActive(true);
    }

    /**
     * Hide the Credits screen and show the Home screen.
     */
    private void HideUI() {
        this.isVisible = false;

        // Hide the Credits UI
        creditsText.SetActive(false);
        backTouchButton.SetActive(false);

        // Show Home screen UI
        homeScreen.ShowUI();
    }
}
