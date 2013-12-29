using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {

	#region Public
	// Movement speed.
	public float speed = 5f;

	// Magnitude of the bounce when enemy is disabled
	public float disableBounceVelocity = 10f;
	#endregion

	#region Private
	// Handle to the Animation
	private Animator animator;

	// If true, enemy movement is to the right.
	private bool directionToRight = true;

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

	void FixedUpdate() {
		int dir = this.directionToRight ? 1 : -1;
		float enemySpeed = speed * dir;
		rigidbody2D.velocity = new Vector2(transform.localScale.x * enemySpeed, rigidbody2D.velocity.y);

		if (this.directionToRight && transform.eulerAngles.y != 180) {
			transform.eulerAngles = new Vector3(0, 180f, 0);
		}

		if (nextState == EnemyState.DISABLED && currState != EnemyState.DISABLED) {
			currState = EnemyState.DISABLED;

			// Stop forward motion and trigger disabled animation
			speed = 0;
			animator.SetBool("Disabled", true);
		}
	}

	public void SetDirection(bool dir) {
		this.directionToRight = dir;
	}

	public void SetDisabled() {
		nextState = EnemyState.DISABLED;
	}
}
