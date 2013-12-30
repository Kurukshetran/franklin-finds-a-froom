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