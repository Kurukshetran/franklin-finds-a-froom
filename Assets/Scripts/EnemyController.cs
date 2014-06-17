using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {

	#region Public
	// Movement speed.
	public float speed = 5f;

	// Magnitude of the bounce when enemy is disabled
	public float disableBounceVelocity = 10f;

	// Time the enemy remains disabled
	public float disabledTime = 5f;

	// Time the enemy is immune from player collision at beginning of the disabled state
	public float disabledImmuneTime = 0.75f;

	// Has the helmet equipped. Prevents stomp attacks.
	public bool hasHelmet = false;

	// Has the boots equipped. Prevents bottom bumps.
	public bool hasBoots = false;

	// If hasBoots flag is true, then this is the force applied to the character on bottom bump.
	public float bumpForceWithBoots = 400f;

	// Number of points to reward
	public int pointValue;
	#endregion

	#region References to other game objects
	// Game Controller
	protected GameController gameController;

	// Audio to play on hit
	public AudioClip hitAudio;
	
	// Audio to play on bottom bump that's been blocked
	public AudioClip bumpBlockedAudio;
	
	// Audio to play when enemy is kicked while disabled
	public AudioClip kickAudio;

	// Text object to show points received
	public GameObject pointsIndicator;
	#endregion

	#region Internal vars
	// Handle to the Animation
	protected Animator animator;

	// Reference to particle system played when player stomps on enemy
	protected ParticleSystem particleSysStomp;

	// Reference to particle system player when enemy is bumped from below
	protected ParticleSystem particleSysBump;

	// If true, enemy movement is to the right.
	protected bool directionToRight = true;

	// Timers for managing disabled states.
	protected float disabledTimer;
	protected float disabledImmuneTimer;

	public enum EnemyState {
		NORMAL,
		DISABLED,
		DISABLED_IMMUNE,
		DOUBLE_SPEED,
		TRANSITION_TO_DOUBLE,
		DEAD
	};
	
	protected EnemyState currState = EnemyState.NORMAL;
	protected EnemyState nextState = EnemyState.NORMAL;
	#endregion

	protected virtual void Awake() {
		// GameController
		gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

		// Particle effects played on hits
		particleSysStomp = transform.FindChild("particleSystem_stomp").GetComponent<ParticleSystem>();
		particleSysBump = transform.FindChild("particleSystem_bump").GetComponent<ParticleSystem>();
	}

	protected virtual void Start() {
		Transform renderObject = transform.Find("renderObject");
		animator = renderObject.GetComponent<Animator>();
	}

	protected virtual void Update() {
		if (currState == EnemyState.DISABLED_IMMUNE) {
			disabledImmuneTimer -= Time.deltaTime;

			if (disabledImmuneTimer <= 0) {
				currState = EnemyState.DISABLED;
				nextState = EnemyState.DISABLED;

				// Start the disabled timer
				disabledTimer = disabledTime;
			}
		}
		else if (currState == EnemyState.DISABLED) {
			disabledTimer -= Time.deltaTime;

			if (disabledTimer <= 0) {
				RecoverFromDisabled();
			}
		}
	}

	protected virtual void FixedUpdate() {
		int dir = this.directionToRight ? 1 : -1;
		float mag = speed;
		if (currState == EnemyState.DISABLED || currState == EnemyState.DISABLED_IMMUNE || currState == EnemyState.TRANSITION_TO_DOUBLE)
			mag = 0;

		float velocity = mag * dir;
		rigidbody2D.velocity = new Vector2(transform.localScale.x * velocity, rigidbody2D.velocity.y);

		if (this.directionToRight && transform.eulerAngles.y != 180) {
			transform.eulerAngles = new Vector3(0, 180f, 0);
		}

		if (nextState == EnemyState.DISABLED_IMMUNE && 
		    (currState != EnemyState.DISABLED_IMMUNE && currState != EnemyState.DISABLED)) {
			currState = EnemyState.DISABLED_IMMUNE;

			// Trigger disabled animation
			animator.SetBool("Disabled", true);

			// Start the disabled immune timer
			disabledImmuneTimer = disabledImmuneTime;
		}
	}

	public EnemyState CurrState {
		get { return currState; }
	}

	protected virtual void RecoverFromDisabled() {
		animator.SetBool("Disabled", false);
		currState = EnemyState.NORMAL;
		nextState = EnemyState.NORMAL;
	}
	
	public virtual void ResetProperties() {}

	public void SetDirection(bool dir) {
		this.directionToRight = dir;
	}

	protected virtual void SetDisabled() {
		nextState = EnemyState.DISABLED_IMMUNE;

        // Log enemy disabled state
        string evt = "Enemy:" + gameController.GetCurrentLevel() + ":" + this.name + ":Disabled";
        GA.API.Design.NewEvent(evt, this.transform.position);
	}

	public virtual void OnBottomBump() {
		// If boots are not equipped, then disable the character
		if (!hasBoots) {
			SetDisabled();

            // Log enemy bump
            string evt = "Enemy:" + gameController.GetCurrentLevel() + ":" + this.name + ":Bump";
            GA.API.Design.NewEvent(evt, this.transform.position);

		}
		// Otherwise, just simulate an upward bounce
		else {
			rigidbody2D.AddForce(new Vector2(0, bumpForceWithBoots));

            // Log failed enemy bump
            string evt = "Enemy:" + gameController.GetCurrentLevel() + ":" + this.name + ":BumpBlocked";
            GA.API.Design.NewEvent(evt, this.transform.position);
		}

		// Display particle effects and play audio only if in attackable state
		if (currState != EnemyState.DISABLED && currState != EnemyState.DISABLED_IMMUNE) {
			particleSysBump.Play();

			if (!hasBoots) {
				AudioSource.PlayClipAtPoint(hitAudio, transform.position);
			}
			else {
				AudioSource.PlayClipAtPoint(bumpBlockedAudio, transform.position);
			}
		}
	}

	public void OnStomped() {
		SetDisabled();

		// Display particle effects
		particleSysStomp.Play();

		// Play hit audio
		if (hitAudio.isReadyToPlay) {
			AudioSource.PlayClipAtPoint(hitAudio, transform.position);
		}
	}

	protected virtual void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "Player") {
		    if (currState == EnemyState.NORMAL) {
				PlayerController pc = coll.gameObject.GetComponent<PlayerController>();

				// Using position of the stomp particle system to check if collision with player came from above or not.
				// Also, if enemy is wearing the helmet, then trigger player death.
				if (particleSysStomp.transform.position.y < coll.gameObject.transform.position.y && !hasHelmet) {
                    // Log enemy stomped position
                    string evt = "Enemy:" + gameController.GetCurrentLevel() + ":" + this.name + ":Stomped";
                    GA.API.Design.NewEvent(evt, this.transform.position);

					OnStomped();

					pc.OnEnemyStomp();
				}
				else {
                    // Log player death, current level, and enemy type to analytics
                    GA.API.Design.NewEvent("PlayerDeath:" + gameController.GetCurrentLevel() + ":" + this.name, pc.transform.position);
					
                    pc.TriggerDeath();
				}
			}
		    else if (currState == EnemyState.DISABLED) {
				KillEnemy();
			}
		}
	}

	protected virtual void KillEnemy() {
		collider2D.enabled = false;
		rigidbody2D.velocity = new Vector2(0, 20f);
		speed = 0;
		currState = EnemyState.DEAD;
		nextState = EnemyState.DEAD;
		
		// Play kick audio
		AudioSource.PlayClipAtPoint(kickAudio, transform.position);

		AddPoints(pointValue);

        // Log enemy kill position
        string evt = "Enemy:" + gameController.GetCurrentLevel() + ":" + this.name + ":Killed";
        GA.API.Design.NewEvent(evt, this.transform.position);
		
		Invoke("Destroy", 2);
	}

	/**
	 * Add points. Display points text and add to the game total.
	 * 
	 * @param int points Points to add
	 */
	public void AddPoints(int points) {
		GameObject pointsText = (GameObject)Instantiate(pointsIndicator);
		pointsText.GetComponent<PointsIndicator>().AddPoints(points, transform.position);
	}

	/**
	 * Add points. Display points text and add to the game total.
	 * 
	 * @param int points Points to add
	 * @param Vector3 startPosOverride Override starting position for the text to animate from
	 */
	public void AddPoints(int points, Vector3 startPosOverride) {
		GameObject pointsText = (GameObject)Instantiate(pointsIndicator);
		pointsText.GetComponent<PointsIndicator>().AddPoints(points, startPosOverride);
	}

	void Destroy() {
		Destroy(this.gameObject);

		gameController.CheckIfLevelCompleted();
	}
}
