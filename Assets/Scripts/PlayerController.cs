using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	// Player's walk speed
	public float walkSpeed;

	// Player's run speed
	public float runSpeed;

	// Player's Animator component
	private Animator animator;

	void Awake() {
		animator = this.GetComponent<Animator>();
	}

	void Update() {
		float hMovement = Input.GetAxis("Horizontal");

		// Set "Speed" on the animator for the controller to apply the animation if needed
		animator.SetFloat("Speed", Mathf.Abs(hMovement));

		// Determine movement and animation speed 
		float speed = walkSpeed;
		animator.speed = 1;
		if (Input.GetButton("Run")) {
			speed = runSpeed;
			animator.speed = 2;
		}

		// Translate player's position along the X axis
		float newX = transform.position.x + hMovement * speed * Time.deltaTime;
		transform.position = new Vector3(newX, transform.position.y, transform.position.z);

		// If the player's forward vector is in the opposite direction of the movement, rotate the character 180 degrees
		if ((hMovement > 0f && transform.right.x == -1) || (hMovement < 0f && transform.right.x == 1)) {
			transform.Rotate(Vector3.up, 180f);
		}
	}

	void FixedUpdate() {

	}
}
