using UnityEngine;
using System.Collections;

public class FireShowerController : MonoBehaviour {

    // Handle to the main camera
    public Camera mainCamera;

    // Handle to the fireball prefab
    public GameObject fireballObj;

    // Camera shake intensity before shower
    public float shakeIntensity;

    // Shake intensity's decay
    public float shakeDecay;

    // Audio to play when shake occurs.
    public AudioClip audioShake;

    private FireShower[] showerConfigs;

    private Vector3 camOriginPos;
    private Quaternion camOriginRot;

    private float startDelay;

    private float currShakeIntensity;
    private bool pendingShower;

    private int currentShower;

    void Update () {
        if (startDelay > 0) {
            startDelay -= Time.deltaTime;
            if (startDelay <= 0) {
                StartShake();
            }
        }

        if (pendingShower) {
            if (currShakeIntensity > 0) {
                mainCamera.transform.position = camOriginPos + Random.insideUnitSphere * currShakeIntensity;
                mainCamera.transform.rotation = new Quaternion(
                    camOriginRot.x + Random.Range(-currShakeIntensity, currShakeIntensity),
                    camOriginRot.y + Random.Range(-currShakeIntensity, currShakeIntensity),
                    camOriginRot.z + Random.Range(-currShakeIntensity, currShakeIntensity),
                    camOriginRot.w + Random.Range(-currShakeIntensity, currShakeIntensity));
                currShakeIntensity -= shakeDecay;
            }
            else {
                StartShower();
            }
        }
    }

    /**
     * Trigger the camera shake prior to a fire shower.
     */
    private void StartShake() {
        camOriginPos = mainCamera.transform.position;
        camOriginRot = mainCamera.transform.rotation;
        currShakeIntensity = shakeIntensity;

        pendingShower = true;

        // Play shake audio
        AudioSource.PlayClipAtPoint(this.audioShake, transform.position);
    }

    /**
     * Spawn the fireballs and randomize their starting positions.
     */
    private void StartShower() {
        pendingShower = false;

        System.Random random = new System.Random();

        FireShower config = showerConfigs[currentShower];
        for (int i = 0; i < config.numFireballs; i++) {
            GameObject obj = (GameObject)Instantiate(fireballObj);

            float randX = (float)random.NextDouble();
            float deltaX = randX * transform.localScale.x;
            float xPos = (-1 * (transform.localScale.x / 2)) + deltaX;

            float randY = (float)random.NextDouble();
            float deltaY = randY * 30;
            float yPos = transform.position.y + deltaY;
            obj.transform.position = new Vector3(xPos, yPos, transform.position.z);

            obj.GetComponent<FireballController>().SetSpeed(config.speed);
        }

        // If there's another shower, start the timer for it
        if (showerConfigs.Length > currentShower + 1) {
            currentShower++;
            startDelay = showerConfigs[currentShower].startDelay;
        }
    }

    /**
     * Setup the shower configs.
     */
    public void Setup(FireShower[] showers) {
        showerConfigs = showers;

        // Setup the first shower
        currentShower = 0;
        startDelay = showerConfigs[currentShower].startDelay;
    }

    /**
     * Stop any further showers from occurring.
     */
    public void Suspend() {
        startDelay = 0;
    }
}
