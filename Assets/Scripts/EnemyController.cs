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
	public float disabledImmuneTime = 1f;
	#endregion

	#region References to other game objects
	protected GameController gameController;
	#endregion

	#region Private
	// Handle to the Animation
	private Animator animator;

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
		DEAD
	};
	
	protected EnemyState currState = EnemyState.NORMAL;
	protected EnemyState nextState = EnemyState.NORMAL;
	#endregion

	protected virtual void Awake() {
		gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
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
				animator.SetBool("Disabled", false);
				currState = EnemyState.NORMAL;
				nextState = EnemyState.NORMAL;
			}
		}
	}

	protected virtual void FixedUpdate() {
		int dir = this.directionToRight ? 1 : -1;
		float mag = speed;
		if (currState == EnemyState.DISABLED || currState == EnemyState.DISABLED_IMMUNE)
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

	public void SetDirection(bool dir) {
		this.directionToRight = dir;
	}

	public void SetDisabled() {
		nextState = EnemyState.DISABLED_IMMUNE;
	}

	protected virtual void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "Player") {
		    if (currState == EnemyState.NORMAL) {
				PlayerController pc = coll.gameObject.GetComponent<PlayerController>();

				if (coll.contacts[0].normal.y == -1 && coll.relativeVelocity.y > 0) {
					SetDisabled();

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
