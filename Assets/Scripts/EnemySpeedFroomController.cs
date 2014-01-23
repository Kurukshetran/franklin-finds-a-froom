using UnityEngine;
using System.Collections;

public class EnemySpeedFroomController : EnemyController {

	// Speed of enemy in its normal state
	public float speedInitial;
	// Speed of enemy in its increased-speed state
	public float speedIncreased;

	// Time to transition from normal to double speed. This should probably match up with the animation transition.
	public float timeTransitionToDouble = 0.5f;

	// Internal timer used to track how long to stay in the transition state
	private float transitionTimer;

	private bool isPlayerColliding;

	protected override void Awake() {
		base.Awake();

		isPlayerColliding = false;
	}

	protected override void Update() {
		base.Update();

		if (currState == EnemyState.TRANSITION_TO_DOUBLE) {
			transitionTimer -= Time.deltaTime;

			if (transitionTimer <= 0) {
				currState = EnemyState.DOUBLE_SPEED;
				nextState = EnemyState.DOUBLE_SPEED;
			}
		}
	}

	protected override void FixedUpdate() {
		base.FixedUpdate();
	}

	protected override void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "Player" && !isPlayerColliding) {
			// HACK :(  OnCollisionEnter2D gets called twice for the seemingly same collision. And OnCollisionExit2D somehow
			// isn't getting called for the player object so I can't reset it there.
			isPlayerColliding = true;
			Invoke("ResetPlayerColliding", 0.1f);

			if (currState == EnemyState.NORMAL || currState == EnemyState.DOUBLE_SPEED) {
				PlayerController pc = coll.gameObject.GetComponent<PlayerController>();
				
				// Using position of the stomp particle system to check if collision with player came from above or not
				if (base.particleSysStomp.transform.position.y < coll.gameObject.transform.position.y) {
					if (currState == EnemyState.NORMAL) {
						// On initial stomp, change to angry state to increase speed movement
						ChangeStateToAngry(true);
					}
					else {
						// On second stomp, place in the disabled state
						SetDisabled();
					}

					pc.OnEnemyStomp();
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

	/**
	 * Triggered when enemy is bumped by ground from below.
	 */
	public override void OnBottomBump() {
		if (currState == EnemyState.DOUBLE_SPEED) {
			SetDisabled();
		}
		else if (currState == EnemyState.NORMAL) {
			ChangeStateToAngry(true);
		}

		base.particleSysBump.Play();
	}

	protected void OnCollisionExit2D(Collision2D coll) {
		// The Player collision exit somehow never gets triggered...
		if (coll.gameObject.tag == "Player" && isPlayerColliding) {
			isPlayerColliding = false;
		}
	}

	protected override void RecoverFromDisabled() {
		base.RecoverFromDisabled();

		ChangeStateToAngry(true);
	}

	public override void ResetProperties() {
		ChangeStateToAngry(false);
	}

	protected override void SetDisabled() {
		base.SetDisabled();

		// Increase speed
		speed = speedIncreased;
	}

	private void ChangeStateToAngry(bool beAngry) {
		if (beAngry) {
			transitionTimer = timeTransitionToDouble;

			currState = EnemyState.TRANSITION_TO_DOUBLE;
			nextState = EnemyState.TRANSITION_TO_DOUBLE;

			speed = speedIncreased;
			if (base.animator)
				base.animator.SetBool("Angry", true);
		}
		else {
			currState = EnemyState.NORMAL;
			nextState = EnemyState.NORMAL;

			speed = speedInitial;

			if (base.animator)
				base.animator.SetBool("Angry", false);
		}
	}

	private void ResetPlayerColliding() {
		isPlayerColliding = false;
	}

}
