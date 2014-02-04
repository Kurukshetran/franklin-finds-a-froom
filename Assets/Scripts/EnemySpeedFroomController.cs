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
				// Also, if enemy is wearing the helmet, then trigger player death.
				if (base.particleSysStomp.transform.position.y < coll.gameObject.transform.position.y
				    && !hasHelmet) {
					if (currState == EnemyState.NORMAL) {
						// On initial stomp, change to angry state to increase speed movement
						ChangeStateToAngry(true);
					}
					else {
						// On second stomp, place in the disabled state
						SetDisabled();
					}

					// Player behavior when it stomps on enemy
					pc.OnEnemyStomp();

					// Player the stomp particle system any time there's a stomp
					base.particleSysStomp.Play();

					// Play audio for hit
					AudioSource.PlayClipAtPoint(hitAudio, transform.position);
				}
				else {
					pc.TriggerDeath();
				}
			}
			else if (currState == EnemyState.DISABLED) {
				base.KillEnemy();
			}
		}
	}

	/**
	 * Triggered when enemy is bumped by ground from below.
	 */
	public override void OnBottomBump() {
		// If boots are not equipped, then disable the character
		if (!hasBoots) {
			// On initial bump, change to angry state 
			if (currState == EnemyState.NORMAL) {
				ChangeStateToAngry(true);
			}
			// On second bump, disable the character
			else if (currState == EnemyState.DOUBLE_SPEED) {
				SetDisabled();
			}
		}
		// Otherwise, just simulate an upward bounce
		else {
			rigidbody2D.AddForce(new Vector2(0, bumpForceWithBoots));
		}

		// Display particle effects and play audio only if in attackable state
		if (currState != EnemyState.DISABLED && currState != EnemyState.DISABLED_IMMUNE) {
			base.particleSysBump.Play();

			if (!hasBoots && base.hitAudio.isReadyToPlay) {
				AudioSource.PlayClipAtPoint(base.hitAudio, transform.position);
			}
			else if(base.bumpBlockedAudio.isReadyToPlay) {
				AudioSource.PlayClipAtPoint(base.bumpBlockedAudio, transform.position);
			}
		}
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
