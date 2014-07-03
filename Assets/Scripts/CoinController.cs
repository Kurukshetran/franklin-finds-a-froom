using UnityEngine;
using System.Collections;

public class CoinController : MonoBehaviour {

  // Speed the coin travels
  public float speed = 5f;

  public float pickupBounceHeight = 4f;

  public float smoothingValue = 6.5f;

  // Number of points to reward
  public int pointValue;

  // Text object to show points received
  public GameObject pointsIndicator;

  // Sound to play
  public AudioClip pickupAudio;

  // Game Controller
  private GameController gameController;

  // Coin travels towards the right if set to true. To the left if false.
  private bool directionToRight = false;

  private bool doPickup = false;

  private float pickupFinalYPos;

  private void Awake() {
    gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
  }

  public void FixedUpdate() {
    // Set direction and velocity of the coin
    int dir = this.directionToRight ? 1 : -1;
    float velocity = speed * dir;
    rigidbody2D.velocity = new Vector2(transform.localScale.x * velocity, rigidbody2D.velocity.y);

    if (doPickup) {
      // If the position of the coin not within some threshold, then continue animating
      float deltaY = Mathf.Abs(pickupFinalYPos - transform.position.y);
      if (deltaY > 0.25) {
        float incrementalY = Mathf.Lerp(transform.position.y, pickupFinalYPos, Time.deltaTime * smoothingValue);
        transform.position = new Vector3(transform.position.x, incrementalY, transform.position.z);
      }
      else {
        // Remove this coin when done
        Destroy(this.gameObject);
      }
    }
  }

  /**
   * Sets the coin's direction of movement.
   */
  public void SetDirection(bool dir) {
    directionToRight = dir;
  }

  /**
   * Handle collision with the Player.
   */
  private void OnCollisionEnter2D(Collision2D coll) {
    if (coll.gameObject.tag == "Player") {
      Pickup();
    }
  }

  public void Pickup() {
    // Disable collider, gravity, and speed
    collider2D.enabled = false;
    rigidbody2D.gravityScale = 0;
    speed = 0;
    
    // Setup to programmatically animate the pickup movement in FixedUpdate()
    doPickup = true;
    pickupFinalYPos = transform.position.y + pickupBounceHeight;

    // Play the sound
    AudioSource.PlayClipAtPoint(pickupAudio, transform.position);

    // Display and add the points
    GameObject pointsText = (GameObject)Instantiate(pointsIndicator);
    pointsText.GetComponent<PointsIndicator>().AddPoints(pointValue, transform.position);

    // Notify game controller
    gameController.AddCoinCollected();
  }
}
