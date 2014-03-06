using UnityEngine;
using System.Collections;

public class PauseMenuUI : MonoBehaviour {

	#region UI elements to hide
	public GUITexture leftTouchButton;
	public GUITexture rightTouchButton;
	public GUITexture jumpTouchButton;
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

	void Start() {
		isGamePaused = false;

		// Set the width and height of the pause menu bg to match the screen size
		if (pauseMenuBg != null) {
			pauseMenuBg.guiTexture.pixelInset = new Rect(0, 0, Screen.width, Screen.height);
		}
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Pause")) {
			// Pause the game
			if (!isGamePaused) {
				isGamePaused = true;

				// Setting time scale to 0 for any components that rely on time to execute.
				savedTimeScale = Time.timeScale;
				Time.timeScale = 0;

				// Pause audio
				AudioListener.pause = true;

				// Show the Pause Menu elements
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

				// Hide the UI controls
				leftTouchButton.guiTexture.enabled = false;
				rightTouchButton.guiTexture.enabled = false;
				jumpTouchButton.guiTexture.enabled = false;
			}
			// Unpause the game
			else {
				Unpause();
			}
		}

		// Check touches to the pause menu items
		if (isGamePaused) {
			foreach (Touch touch in Input.touches) {
				if (touch.phase == TouchPhase.Began) {
					if (exitTouchButton.HitTest(touch.position)) {

					}
					else if (resumeTouchButton.HitTest(touch.position)) {
						Unpause();
					}
					else if (soundOffTouchButton.guiTexture.enabled && soundOffTouchButton.HitTest(touch.position)) {
						// Turn sound back on
						AudioListener.volume = 1;
						soundOffTouchButton.guiTexture.enabled = false;
						soundOnTouchButton.guiTexture.enabled = true;
					}
					else if (soundOnTouchButton.guiTexture.enabled && soundOnTouchButton.HitTest(touch.position)) {
						// Turn sound off
						AudioListener.volume = 0;
						soundOffTouchButton.guiTexture.enabled = true;
						soundOnTouchButton.guiTexture.enabled = false;
					}
				}
			}
		}
	}

	private void Unpause() {
		isGamePaused = false;
		
		// Restore time scale
		Time.timeScale = savedTimeScale;
		
		// Unpause audio
		AudioListener.pause = false;
		
		// Hide the Pause Menu elements
		pauseMenuBg.guiTexture.enabled = false;
		exitTouchButton.guiTexture.enabled = false;
		resumeTouchButton.guiTexture.enabled = false;
		soundOffTouchButton.guiTexture.enabled = false;
		soundOnTouchButton.guiTexture.enabled = false;
		
		// Show the UI controls
		leftTouchButton.guiTexture.enabled = true;
		rightTouchButton.guiTexture.enabled = true;
		jumpTouchButton.guiTexture.enabled = true;
	}

}
