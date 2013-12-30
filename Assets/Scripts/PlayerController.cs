using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	#region Public
	// Player's walk speed
	public float walkSpeed;

	// Player's run speed
	public float runSpeed;

	// Force the player jumps with while walking
	public float walkJumpForce;

	// Force the player jumps with while running
	public float runJumpForce;

	// Normal gravity scale of player falling to the ground
	public float gravityScaleNormal;

	// Gravity scale when player is floating to the ground
	public float gravityScaleFloating;

	public GameObject respawnLocation;
	#endregion

	#region Private
	// Player's Animator component
	private Animator animator;

	private Transform groundCheck;

	private bool doJump;

	private bool doStompBounce;

	private bool isRunning;
	#endregion

	void Awake() {
		animator = this.GetComponent<Animator>();

		groundCheck = transform.Find("groundCheck");
	}

	void Update() {
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
		if (Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"))) {
			isOnGround = true;
		}

		if (isOnGround && Input.GetButtonDown("Jump")) {
			doJump = true;
		}

		// Adjust gravity scale based on the jump button
		if (Input.GetButton("Jump")) {
			rigidbody2D.gravityScale = gravityScaleFloating;
		}
		else {
			rigidbody2D.gravityScale = gravityScaleNormal;
		}
	}

	void FixedUpdate() {
		if (doJump) {
			doJump = false;

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

			rigidbody2D.AddForce(new Vector2(walkJumpForce / 3 * xForce, walkJumpForce * 2));
		}
	}

	public void OnEnemyStomp() {
		doStompBounce = true;
	}

	public void TriggerDeath() {
		transform.position = respawnLocation.transform.position;

		// Reset camera position
		GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
		CameraController cameraController = camera.GetComponent<CameraController>();
		cameraController.ResetPosition();
	}
}
