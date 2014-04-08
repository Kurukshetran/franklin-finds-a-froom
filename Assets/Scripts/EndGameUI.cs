using UnityEngine;
using System.Collections;

public class EndGameUI : MonoBehaviour {

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
		if (menuBg != null) {
			menuBg.guiTexture.pixelInset = new Rect(0, 0, Screen.width, Screen.height);
		}
	}

	void Update() {
	}

	public void ShowEndGameMenu() {
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

	public void HideEndGameMenu() {
		// Hide the end game menu's UI elements
		gameOver.SetActive(false);
		gameScore.SetActive(false);
		highScore.SetActive(false);
		menuBg.guiTexture.enabled = false;
		exitTouchButton.guiTexture.enabled = false;
		restartTouchButton.guiTexture.enabled = false;
	}

}
