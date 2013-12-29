using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {

	// Movement speed.
	public float speed = 5;

	// If true, enemy movement is to the right.
	private bool directionToRight = true;

	void Update() {
	}

	void FixedUpdate() {
		int dir = this.directionToRight ? 1 : -1;
		float enemySpeed = speed * dir;
		rigidbody2D.velocity = new Vector2(transform.localScale.x * enemySpeed, rigidbody2D.velocity.y);

		if (this.directionToRight) {
			transform.eulerAngles = new Vector3(0, 180f, 0);
		}
	}

	public void SetDirection(bool dir) {
		this.directionToRight = dir;
	}
}
