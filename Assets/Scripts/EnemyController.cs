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
	#endregion

	#region References to other game objects
	protected GameController gameController;
	#endregion

	#region Internal vars
	// Handle to the Animation
	protected Animator animator;

	// Reference to particle system played when player stomps on enemy
	protected ParticleSystem particleSysStomp;

	// Reference to particle system player when enemy is bumped from below
	protected ParticleSystem particleSysBump;

	// If true, enemy movement is to the right.
	private bool directionToRight = true;

	// Timers for managing disabled states.
	private float disabledTimer;
	private float disabledImmuneTimer;

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
		gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		particleSysStomp = transform.FindChild("particleSystem_stomp").GetComponent<ParticleSystem>();
		particleSysBump = transform.FindChild("particleSystem_bump").GetComponent<ParticleSystem>();
	}

	void Start() {
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
	}

	public virtual void OnBottomBump() {
		SetDisabled();
		particleSysBump.Play();
	}

	public void OnStomped() {
		SetDisabled();
		particleSysStomp.Play();
	}

	protected virtual void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "Player") {
		    if (currState == EnemyState.NORMAL) {
				PlayerController pc = coll.gameObject.GetComponent<PlayerController>();

				// Using position of the stomp particle system to check if collision with player came from above or not
				if (particleSysStomp.transform.position.y < coll.gameObject.transform.position.y) {
					OnStomped();

					pc.OnEnemyStomp();
					gameController.AddToScore(50);
				}
				else {
					pc.TriggerDeath();
				}
			}
		    else if (currState == EnemyState.DISABLED) {
				collider2D.enabled = false;
				rigidbody2D.velocity = new Vector2(0, 20f);
				speed = 0;
				currState = EnemyState.DEAD;
				nextState = EnemyState.DEAD;

				gameController.AddToScore(100);

				Invoke("Destroy", 2);
			}
		}
	}

	void Destroy() {
		Destroy(this.gameObject);

		gameController.CheckIfLevelCompleted();
	}
}
