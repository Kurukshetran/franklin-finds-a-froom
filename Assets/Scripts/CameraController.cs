using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	#region Public
	// Player object to follow
	public GameObject player;

	// Controls how smooth the camera pans
	public float smoothingValue = 5f;

	// Camera regions based on player position
	public CameraRegion[] cameraRegions;
	#endregion

	#region Private
	// Tracks current camera region
	private int currRegion = 0;
	#endregion

    void Start() {
        // Borrowing code from the following URL. Sets the aspect ratio to 16/9
        // which is closer to what this game was designed at.
        // http://gamedesigntheory.blogspot.ie/2010/09/controlling-aspect-ratio-in-unity.html 
        float targetaspect = 16.0f / 9.0f;
        
        // Determine the game window's current aspect ratio
        float windowaspect = (float)Screen.width / (float)Screen.height;
        
        // Current viewport height should be scaled by this amount
        float scaleheight = windowaspect / targetaspect;
        
        // Obtain camera component so we can modify its viewport
        Camera camera = GetComponent<Camera>();
        
        // If scaled height is less than current height, add letterbox
        if (scaleheight < 1.0f)
        {  
            Rect rect = camera.rect;
            
            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;
            
            camera.rect = rect;
        }
        else // add pillarbox
        {
            float scalewidth = 1.0f / scaleheight;
            
            Rect rect = camera.rect;
            
            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;
            
            camera.rect = rect;
        }
    }

	void Update() {
		int nextRegion = currRegion;
		float playerY = player.transform.position.y;
		for (int i = 0; i < cameraRegions.Length; i++) {
			if (playerY >= cameraRegions[i].minY) {
				nextRegion = i;
			}
			else {
				break;
			}
		}
		currRegion = nextRegion;
	}

	void FixedUpdate() {
		if (transform.position.y != cameraRegions[currRegion].cameraY) {
			float incrementalY = Mathf.Lerp(transform.position.y, cameraRegions[currRegion].cameraY, Time.deltaTime * smoothingValue);
			transform.position = new Vector3(transform.position.x, incrementalY, transform.position.z);
		}
	}

	public void ResetPosition() {
		transform.position = new Vector3(0, 0, transform.position.z);
	}
}

[System.Serializable]
public class CameraRegion {
	public float minY;
	public float cameraY;
}