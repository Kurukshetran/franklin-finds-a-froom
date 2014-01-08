using UnityEngine;
using System.Collections;

public class DummyEnemy2Controller : EnemyController {
	
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

			if (currState == EnemyState.NORMAL || currState == EnemyState.DOUBLE_SPEED) {
				PlayerController pc = coll.gameObject.GetComponent<PlayerController>();
				
				if (coll.contacts[0].normal.y == -1 && coll.relativeVelocity.y > 0) {
					if (currState == EnemyState.NORMAL) {
						nextState = EnemyState.DOUBLE_SPEED;

						this.speed = speedIncreased;
					}
					else {
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

	private void ResetPlayerColliding() {
		isPlayerColliding = false;
	}

}
