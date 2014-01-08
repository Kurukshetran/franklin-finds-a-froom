using UnityEngine;
using System.Collections;

public class LevelConfig : MonoBehaviour {

	public Level[] levels;

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
	public GameObject[] leftEnemies;
	public GameObject[] rightEnemies;
	public float leftSpawnStart;
	public float leftSpawnDelay;
	public float rightSpawnStart;
	public float rightSpawnDelay;
	public bool endlessMode;
}