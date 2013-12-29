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
	#endregion

	#region Private
	// Handle to the Animation
	private Animator animator;

	// If true, enemy movement is to the right.
	private bool directionToRight = true;

	private float disabledTimer;

	private enum EnemyState {
		NORMAL,
		DISABLED,
		DEAD
	};

	private EnemyState currState = EnemyState.NORMAL;
	private EnemyState nextState = EnemyState.NORMAL;
	#endregion

	void Start() {
		Transform renderObject = transform.Find("renderObject");
		animator = renderObject.GetComponent<Animator>();
	}

	void Update() {
		if (currState == EnemyState.DISABLED) {
			disabledTimer -= Time.deltaTime;

			if (disabledTimer <= 0) {
				animator.SetBool("Disabled", false);
				currState = EnemyState.NORMAL;
				nextState = EnemyState.NORMAL;
			}
		}
	}

	void FixedUpdate() {
		int dir = this.directionToRight ? 1 : -1;
		float mag = speed;
		if (currState == EnemyState.DISABLED)
			mag = 0;

		float velocity = mag * dir;
		rigidbody2D.velocity = new Vector2(transform.localScale.x * velocity, rigidbody2D.velocity.y);

		if (this.directionToRight && transform.eulerAngles.y != 180) {
			transform.eulerAngles = new Vector3(0, 180f, 0);
		}

		if (nextState == EnemyState.DISABLED && currState != EnemyState.DISABLED) {
			currState = EnemyState.DISABLED;

			// Trigger disabled animation
			animator.SetBool("Disabled", true);

			// Start the disabled timer
			disabledTimer = disabledTime;
		}
	}

	public void SetDirection(bool dir) {
		this.directionToRight = dir;
	}

	public void SetDisabled() {
		nextState = EnemyState.DISABLED;
	}

	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "Player" && currState == EnemyState.DISABLED) {
			collider2D.enabled = false;
			rigidbody2D.gravityScale = 2f;
			rigidbody2D.velocity = new Vector2(0, 20f);
			speed = 0;
			currState = EnemyState.DEAD;
			nextState = EnemyState.DEAD;

			Invoke("Destroy", 2);
		}
	}

	void Destroy() {
		Destroy(this.gameObject);
	}
}
