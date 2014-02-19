using UnityEngine;
using System.Collections;

public class PauseMenuUI : MonoBehaviour {

	// Flag tracking whether or not game is paused.
	private bool isGamePaused;

	private float savedTimeScale;

	void Start() {
		isGamePaused = false;
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
			}
			// Unpause the game
			else {
				isGamePaused = false;

				// Restore time scale
				Time.timeScale = savedTimeScale;

				// Unpause audio
				AudioListener.pause = false;
			}
		}
	}

}
