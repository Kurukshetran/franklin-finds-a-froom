using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	#region Public
	// Player's walk speed
	public float walkSpeed = 6f;

	// Player's run speed
	public float runSpeed = 9f;

	// Force the player jumps with while walking
	public float walkJumpForce = 800f;

	// Force the player jumps with while running
	public float runJumpForce = 1050f;

	// Force applied to player after stomping on enemy
	public float stompBounceForce = 150f;

	// Normal gravity scale of player falling to the ground
	public float gravityScaleNormal = 10f;

	// Gravity scale when player is floating to the ground
	public float gravityScaleFloating = 2f;

	// Max amount of time the float effect can be applied to a jump
	public float jumpFloatTime = 0.39f;

	// Time to delay respawn on death
	public int respawnDelay = 4;
	#endregion

	#region References to other GameObjects
	// Location to respawn at
	public GameObject respawnLocation;
	#endregion

	#region Private
	// Game controller
	private GameController gameController;

	// Player's Animator component
	private Animator animator;

	private Transform groundCheckLeft;

	private Transform groundCheckRight;

	private float jumpFloatTimer;

	private bool doJump;

	private bool doStompBounce;

	private bool ignoreInput;

	private bool isRunning;
	#endregion

	void Awake() {
		animator = this.GetComponent<Animator>();

		gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

		groundCheckLeft = transform.Find("groundCheck_left");
		groundCheckRight = transform.Find("groundCheck_right");
	}

	void Update() {
		if (ignoreInput)
			return;

		float hMovement = Input.GetAxis("Horizontal");

		// Set "Speed" on the animator for the controller to apply the animation if needed
		animator.SetFloat("Speed", Mathf.Abs(hMovement));

		// Determine movement and animation speed 
		float speed;
		if (Input.GetButton("Run")) {
			isRunning = true;
			speed = runSpeed;
			animator.speed = 2;
		}
		else {
			isRunning = false;
			speed = walkSpeed;
			animator.speed = 1;
		}

		// Translate player's position along the X axis
		float newX = transform.position.x + hMovement * speed * Time.deltaTime;
		transform.position = new Vector3(newX, transform.position.y, transform.position.z);

		// If the player's forward vector is in the opposite direction of the movement, rotate the character 180 degrees
		if ((hMovement > 0f && transform.right.x == -1) || (hMovement < 0f && transform.right.x == 1)) {
			transform.Rotate(Vector3.up, 180f);
		}

		// Check if player can jump, and mark it to jump at the next iteration of FixedUpdate()
		bool isOnGround = false;
		if (Physics2D.Linecast(transform.position, groundCheckLeft.position, 1 << LayerMask.NameToLayer("Ground"))
		    || Physics2D.Linecast(transform.position, groundCheckRight.position, 1 << LayerMask.NameToLayer("Ground"))) {
			isOnGround = true;
		}

		// Notify animator to show jump animation
		if (isOnGround) {
			animator.SetBool("Jump", false);
		}
		else {
			animator.SetBool("Jump", true);
		}

		// Flag a jump to occur on the next FixedUpdate
		if (isOnGround && Input.GetButtonDown("Jump")) {
			doJump = true;
		}

		jumpFloatTimer -= Time.deltaTime;
		// Adjust gravity scale based on the jump button
		if (Input.GetButton("Jump") && jumpFloatTimer > 0) {
			rigidbody2D.gravityScale = gravityScaleFloating;
		}
		else {
			rigidbody2D.gravityScale = gravityScaleNormal;
		}
	}

	void FixedUpdate() {
		if (doJump) {
			doJump = false;
			jumpFloatTimer = jumpFloatTime;

			// Using animator speed to determine whether or not character is running
			float jumpForce;
			if (isRunning) {
				jumpForce = runJumpForce;
			}
			else {
				jumpForce = walkJumpForce;
			}

			// Apply force for the jump
			rigidbody2D.AddForce(new Vector2(0, jumpForce));
		}

		// Create bounce effect after stomping on an enemy
		if (doStompBounce) {
			doStompBounce = false;

			int xForce = 0;
			if (transform.forward.x > 0) {
				xForce = 1;
			}
			else if (transform.forward.x < 0) {
				xForce = -1;
			}

			rigidbody2D.AddForce(new Vector2(stompBounceForce * xForce, walkJumpForce * 2));
		}
	}

	public void OnEnemyStomp() {
		doStompBounce = true;
	}

	public void TriggerDeath() {
		// Trigger death animation
		animator.SetBool("Dead", true);

		// Ignore user input
		ignoreInput = true;

		// Set to a layer for enemies to not collide with
		gameObject.layer = 10; // "EnemyIgnore"

		// Decrement number of current lives
		gameController.DecrementCurrentLives();
		if (gameController.GetCurrentLives() >= 0) {
			// Respawn after a few seconds if we still have remaining lives
			Invoke("Respawn", 4);
		}
	}

	public void Respawn() {
		animator.SetBool("Dead", false);
		transform.position = respawnLocation.transform.position;

		// Re-enable input
		ignoreInput = false;

		// Reset layer to allow enemy collisions
		gameObject.layer = 0; // "Default"

		// Reset camera position
		GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
		CameraController cameraController = camera.GetComponent<CameraController>();
		cameraController.ResetPosition();
	}
}
