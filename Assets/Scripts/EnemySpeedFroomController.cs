using UnityEngine;
using System.Collections;

public class EnemySpeedFroomController : EnemyController {

	// Speed of enemy in its normal state
	public float speedInitial;
	// Speed of enemy in its increased-speed state
	public float speedIncreased;

	private bool isPlayerColliding;

	protected override void Awake() {
		base.Awake();

		isPlayerColliding = false;
	}

	protected override void FixedUpdate() {
		base.FixedUpdate();

		currState = nextState;
	}

	protected override void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "Player" && !isPlayerColliding) {
			// HACK :(  OnCollisionEnter2D gets called twice for the seemingly same collision. And OnCollisionExit2D somehow
			// isn't getting called for the player object so I can't reset it there.
			isPlayerColliding = true;
			Invoke("ResetPlayerColliding", 0.1f);

			Debug.Log("state: "+currState+", nextState: "+nextState);

			if (currState == EnemyState.NORMAL || currState == EnemyState.DOUBLE_SPEED) {
				PlayerController pc = coll.gameObject.GetComponent<PlayerController>();
				
				// Using position of the stomp particle system to check if collision with player came from above or not
				if (base.particleSysStomp.transform.position.y < coll.gameObject.transform.position.y) {
					Debug.Log("  stomp");
					if (currState == EnemyState.NORMAL) {
						nextState = EnemyState.DOUBLE_SPEED;

						// On initial stomp, increase speed movement and change animation
						this.speed = speedIncreased;
						base.animator.SetBool("Angry", true);
					}
					else {
						// On second stomp, place in the disabled state
						SetDisabled();
					}

					pc.OnEnemyStomp();
				}
				else {
					Debug.Log("PC TRIGGER DEATH");
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

	protected void OnCollisionExit2D(Collision2D coll) {
		// The Player collision exit somehow never gets triggered...
		if (coll.gameObject.tag == "Player" && isPlayerColliding) {
			isPlayerColliding = false;
		}
	}

	protected override void RecoverFromDisabled() {
		base.RecoverFromDisabled();

		currState = EnemyState.DOUBLE_SPEED;
		nextState = EnemyState.DOUBLE_SPEED;
	}

	public override void ResetProperties() {
		currState = EnemyState.NORMAL;
		nextState = EnemyState.NORMAL;
		speed = speedInitial;

		if (base.animator)
			base.animator.SetBool("Angry", false);
	}

	protected override void SetDisabled() {
		base.SetDisabled();

		// Increase speed
		speed = speedIncreased;
	}

	private void ResetPlayerColliding() {
		isPlayerColliding = false;
	}

}
