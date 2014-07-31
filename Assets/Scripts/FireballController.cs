using UnityEngine;
using System.Collections;

public class FireballController : MonoBehaviour {

	// Probability the fireball will collide with the Ground layer
	public float groundCollidePercent;

	// Time fire remains on ground before burning out
	public float timeOnGround;

	// Seconds remaining in timeOnGround before changing to burnout stage 1
	public float timeBurnout1;

	// Seconds remaining in timeOnGround before changing to burnout stage 2
	public float timeBurnout2;

	// Internal timer to track how long fireball should remain alive
	private float burnoutTimer;

	// Animator
	private Animator anim;

	// Track which objects we've already calculated collision against
	private int[] hitIds;
	private int hitsDetected;

    private float speed;

    #region References to other game objects
    private GameController gameController;
    #endregion

	private void Awake() {
		anim = GetComponent<Animator>();

        // GameController
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

		// 8 for the number of platforms that can be hit
		hitIds = new int[8];
		hitsDetected = 0;

        burnoutTimer = 0;
	}

	private void Start() {
		// TODO: randomize start frame for the animation. Otherwise all fireballs play the same animation each frame.
//		System.Random rand = new System.Random();
//		anim.playbackTime = anim.playbackTime * (float)rand.NextDouble();
	}

	private void Update() {
		if (burnoutTimer > 0) {
			burnoutTimer -= Time.deltaTime;

			if (burnoutTimer <= 0) {
				Destroy(this.gameObject);
			}
			else if (burnoutTimer <= timeBurnout2) {
				anim.SetBool("Burnout2", true);
			}
			else if (burnoutTimer <= timeBurnout1) {
				anim.SetBool("Burnout1", true);
			}
		}
        else {
            float newY = transform.position.y - (this.speed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.tag == "Player") {
			PlayerController pc = other.gameObject.GetComponent<PlayerController>();

            // Log player death, current level, and enemy type to analytics
            GA.API.Design.NewEvent("PlayerDeath:" + gameController.GetCurrentLevel() + ":Fireball", pc.transform.position);

			pc.TriggerDeath();
		}
		else if (other.gameObject.layer == 8) { // 8 = "Ground"
			int id = other.gameObject.GetInstanceID();
			if (!HasHit(id)) {
				AddHit(id);

				System.Random rand = new System.Random();
				double num = rand.NextDouble();
				if (num < groundCollidePercent) {
                    // Change its animation
					anim.SetBool("Ground", true);

					// Start burnout timer
					burnoutTimer = timeOnGround;
				}
			}
		}
	}

	private bool HasHit(int id) {
		for (int i = 0; i < hitsDetected; i++) {
			if (hitIds[i] == id) {
				return true;
			}
		}

		return false;
	}

	private void AddHit(int id) {
		hitIds[hitsDetected] = id;
		hitsDetected++;
	}

    public void SetSpeed(float speed) {
        this.speed = speed;
    }
}
