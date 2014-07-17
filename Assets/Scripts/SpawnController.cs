using UnityEngine;
using System.Collections;

public class SpawnController : MonoBehaviour {

    // If true, enemies spawn going to the right
    public bool directionToRight = false;
    
    // If true, randomize the type of enemy spawned.
    public bool randomizeEnemies = false;

    // Array of enemies that are yet to be spawned into the level.
    private SpawnConfig[] pendingSpawns;

    // Time in seconds between spawns.
    private float respawnDelay;

    // Vars for endless mode
    private bool endlessMode;
    private int endlessSpawnIndex;

    /**
     * Setup the spawn controller.
     * 
     * @param spawnConfigs Array of spawn configuration settings
     * @param delay Time delay for respawns
     * @param endless Endless mode flag'
     */
    public void Setup(SpawnConfig[] spawnConfigs, float delay, bool endless) {
        // Use pendingSpawns to track upcoming spawns
        pendingSpawns = spawnConfigs;

        // Time delay for respawns
        respawnDelay = delay;

        // Copy endlessMode flag
        endlessMode = endless;

        // Get start delay time for the first element and SpawnNext() after that amount of time
        SpawnNext();
    }

    /**
     * Queues up the next spawn.
     */
    private void SpawnNext() {
        if (pendingSpawns.Length > 0) {
            // Instantiate enemy at front of array
            SpawnConfig spawnConfig = pendingSpawns[0];
            Invoke("Spawn", spawnConfig.spawnDelay);
        }
    }

    /**
     * Spawn the object into the world.
     */
    private void Spawn() {
        SpawnConfig spawnConfig = pendingSpawns[0];

        // Instantiate the spawn object. Set position and rotation.
        GameObject spawnObj = (GameObject)Instantiate(spawnConfig.spawnObject);

        // Not sure why, but the first spawn of these things off of prefabs have activeSelf = true. When false,
        // it indicates that this is a respawn of an old object, so delete this one, and use the clone that was 
        // instantiated just prior to this.
        if (!spawnConfig.spawnObject.activeSelf) {
            Destroy(spawnConfig.spawnObject);
        }

        // Respawned objects are copied from inactive objects, so need to activate
        spawnObj.SetActive(true);

        // Set start position at the spawn point
        spawnObj.transform.position = transform.position;
        spawnObj.transform.rotation = transform.rotation;

        // Set direction of the spawned object
        EnemyController enemyController = spawnObj.GetComponent<EnemyController>();
        if (enemyController) {
            enemyController.SetDirection(directionToRight);
            enemyController.ResetProperties();
            enemyController.speed = spawnConfig.speedModifier * enemyController.speed;
        }
        else {
            DirectionController dirController = spawnObj.GetComponent<DirectionController>();
            if (dirController) {
                dirController.SetDirection(directionToRight);
            }
        }

        // Recreate the pendingSpawns array without the first element
        SpawnConfig[] tmp = new SpawnConfig[pendingSpawns.Length - 1];
        if (pendingSpawns.Length > 1) {
            for (int i = 1; i < pendingSpawns.Length; i++) {
                tmp[i - 1] = pendingSpawns[i];
            }
        }

        pendingSpawns = tmp;

        // Queue up the next spawn object
        SpawnNext();
    }

    /**
     * Stop the spawning.
     */
    public void StopSpawning() {
        CancelInvoke();
    }

    /**
     * Re-add enemy to the queue to spawn again.
     */
    public void AddEnemyToQueue(GameObject enemy) {
        if (endlessMode) {
            Destroy(enemy);
        }
        else {
            SpawnConfig[] tmpSpawns = new SpawnConfig[pendingSpawns.Length + 1];
            if (pendingSpawns.Length > 0) {
                pendingSpawns.CopyTo(tmpSpawns, 0);
            }

            // Make object inactive. Will be destroyed later when a copy is spawned.
            enemy.SetActive(false);

            SpawnConfig config = new SpawnConfig();
            config.spawnDelay = respawnDelay;
            config.spawnObject = enemy;

            tmpSpawns[tmpSpawns.Length - 1] = config;
            pendingSpawns = tmpSpawns;

            if (pendingSpawns.Length == 1) {
                SpawnNext();
            }
        }
    }

    /**
     * Return the number of spawns pending.
     */
    public int GetNumPendingEnemies() {
        if (pendingSpawns != null)
            return pendingSpawns.Length;
        else
            return -1;
    }
}
