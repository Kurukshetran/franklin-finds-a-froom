using UnityEngine;
using System.Collections;

public class LevelConfig : MonoBehaviour {

	// Array of level definitions
	public Level[] levels;

	#region Controls effects of the fire shower.
	// Intensity of the camera shake
	public float cameraShakeIntensity;

	// Rate at which the shake decays each frame
	public float cameraShakeDecay;
	#endregion

	public Level GetLevel(int levelIndex) {
		if (levelIndex < levels.Length) {
			return levels[levelIndex];
		}
		else {
			return null;
		}
	}
}

[System.Serializable]
public class Level {
	public SpawnConfig[] leftSpawnConfigs;
	public SpawnConfig[] rightSpawnConfigs;
	public float respawnDelay;
	public bool endlessMode;
	public FireShower[] fireShowers;
}

[System.Serializable]
public class SpawnConfig {
	public GameObject spawnObject;
	public float spawnDelay;
}

[System.Serializable]
public class FireShower {
	public int numFireballs;
	public float startDelay;
	public float gravityScale;
}