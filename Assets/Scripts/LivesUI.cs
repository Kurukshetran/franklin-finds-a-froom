using UnityEngine;
using System.Collections;

public class LivesUI : MonoBehaviour {

    // Prefab of the sprite object for the life icon
    public GameObject iconPrefab;

    // X distance between icons
    public float iconXPadding;

    // Array of references to the instantiated icon objects
    private GameObject[] iconRefs;

    // Text displaying how many coins are collected and how many are needed until a new life
    public GUIText coinCounterText;

    /**
     * Update the # of icons shown on the screen
     */
    public void UpdateRemainingLives(int lives) {
        // Early out if all remaining lives are lost
        if (lives < 0) {
            return;
        }

        // Clear and rebuild the reference array
        if (iconRefs != null) {
            for (int i = iconRefs.Length - 1; i >= 0; i--) {
                Destroy(iconRefs[i]);
            }
        }
        iconRefs = new GameObject[lives];

        // Then recreate the icons
        for (int i = 0; i < lives; i++) {
            GameObject newIcon = (GameObject)Instantiate(iconPrefab);

            // Set its position
            if (newIcon.guiTexture) {
                Rect currInset = newIcon.guiTexture.pixelInset;
                int insetX = (int)(currInset.x + ((currInset.x + iconXPadding) * i));
                newIcon.guiTexture.pixelInset = new Rect(insetX, currInset.y, currInset.width, currInset.height);
            }

            // Add into the reference array
            iconRefs[i] = newIcon;
        }
    }

    /**
     * Update the coin counter.
     */
    public void UpdateCoinCounter(int coins) {
        coinCounterText.text = coins + " / " + GameController.coinsFor1Up;
    }
}
