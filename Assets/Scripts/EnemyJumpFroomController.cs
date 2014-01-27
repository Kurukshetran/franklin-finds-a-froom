using UnityEngine;
using System.Collections;

public class EnemyJumpFroomController : EnemyController {

	// Force to apply in the Y direction on each jump
	public float jumpForceY = 500;

	// Time delay between jumps in seconds
	public float jumpDelay = 2;

	#region Private
	// Transform objects used to check if character is on the ground
	private Transform groundCheckLeft;
	private Transform groundCheckRight;

	// Timer to track time in between jumps
	private float jumpTimer = 0;

	// Internal flag indicating when character should jump
	private bool doJump = false;

	// Set to true if character is in the process fo jumping
	private bool isJumping = false;

	// Set to true if character is jumping and we should check for when it hits the ground again
	private bool doGroundCheck = false;
	#endregion

	protected override void Awake() {
		base.Awake();

		groundCheckLeft = transform.Find("groundCheck_left");
		groundCheckRight = transform.Find("groundCheck_right");
	}

	protected override void Start() {
		base.Start();
	}
	
	protected override void Update() {
		base.Update();

		if (jumpTimer <= 0 && currState == EnemyState.NORMAL) {
			jumpTimer = jumpDelay;
			doJump = true;
		}
		else {
			jumpTimer -= Time.deltaTime;
		}
	}

	/**
	 * Overrides base class since this character movement is different.
	 * 
	 * TODO: if you really feel like cleaning it up, is make EnemyController more generic, and EnemyFroom gets
	 * its own controller.
	 * */
	protected override void FixedUpdate() {
		// Fix the direction the sprite is facing
		if (this.directionToRight && transform.eulerAngles.y != 180) {
			transform.eulerAngles = new Vector3(0, 180f, 0);
		}

		// Use force on the rigid body to create the jump effect
		if (doJump && currState == EnemyState.NORMAL) {
			doJump = false;

			int dir = base.directionToRight ? 1 : -1;
			float xForce = base.speed * dir;
			rigidbody2D.AddForce(new Vector2(xForce, jumpForceY));

			isJumping = true;
			base.animator.SetBool("Jump", true);
		}

		if (isJumping) {
			// Check if character is on the ground
			bool isOnGround = false;
			if (Physics2D.Linecast(transform.position, groundCheckLeft.position, 1 << LayerMask.NameToLayer("Ground"))
			    || Physics2D.Linecast(transform.position, groundCheckRight.position, 1 << LayerMask.NameToLayer("Ground"))) {
				isOnGround = true;
			}

			// Off the ground, so now it's ok to check when the character comes back down to the ground
			if (!isOnGround) {
				doGroundCheck = true;
			}
			// Once the character hits the ground again, change animation back to normal
			else if (doGroundCheck && isOnGround) {
				doGroundCheck = false;
				isJumping = false;
				base.animator.SetBool("Jump", false);
			}
		}

		// "Disabled" state code copied from base EnemyController.FixedUpdate()
		if (nextState == EnemyState.DISABLED_IMMUNE && 
		    (currState != EnemyState.DISABLED_IMMUNE && currState != EnemyState.DISABLED)) {
			currState = EnemyState.DISABLED_IMMUNE;
			
			// Trigger disabled animation
			animator.SetBool("Disabled", true);
			
			// Start the disabled immune timer
			base.disabledImmuneTimer = base.disabledImmuneTime;
		}
	}

	/**
	 * Called when the enemy character recovers from the disabled state.
	 */
	protected override void RecoverFromDisabled() {
		base.RecoverFromDisabled();

		// Trigger a jump immediately after a recovery
		doJump = true;
	}
}
