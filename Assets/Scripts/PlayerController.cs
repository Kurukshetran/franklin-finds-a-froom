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

    // Force applied in X direction to player after stomping on enemy
    public float stompBounceForceX = 200f;

    // Force applied in X direction to player after stomping on enemy
    public float stompBounceForceY = 1000f;

    // Normal gravity scale of player falling to the ground
    public float gravityScaleNormal = 10f;

    // Gravity scale when player is floating to the ground
    public float gravityScaleFloating = 2f;

    // Max amount of time the float effect can be applied to a jump
    public float jumpFloatTime = 0.39f;

    // Time to delay respawn on death
    public int respawnDelay = 4;

    // Jump audio clip
    public AudioClip audioJump;

    // Death audio clip
    public AudioClip audioDeath;
    #endregion

    #region UI
    public GUITexture leftTouchButton;
    public GUITexture rightTouchButton;
    public GUITexture jumpTouchButton;

    private float uiPressedAlpha = 0.5f;
    private float uiUnpressedAlpha = 0.137f;
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

    private bool isRespawnAtLevelStart;
    #endregion

    void Awake() {
        animator = this.GetComponent<Animator>();

        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        groundCheckLeft = transform.Find("groundCheck_left");
        groundCheckRight = transform.Find("groundCheck_right");

        // Ignore user input until a respawn
        ignoreInput = true;

        // There's no sprinting for touchscreen game, so make jump force always the max
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
            walkJumpForce = runJumpForce;
        }
    }

    void Update() {
        if (ignoreInput || gameController.GameState != GameController.FFGameState.InProgress)
            return;

        // Mobile touch screen support
        bool jumpTouchBegan = false;
        bool jumpTouchContinue = false;
        bool leftTouched = false;
        bool rightTouched = false;
        foreach (Touch touch in Input.touches) {
            // Detect if button is touched
            if (touch.phase != TouchPhase.Canceled && touch.phase != TouchPhase.Ended) {
                if (jumpTouchButton.HitTest(touch.position)) {
                    if (touch.phase == TouchPhase.Began) {
                        jumpTouchBegan = true;
                    }
                    else {
                        jumpTouchContinue = true;
                    }

                    jumpTouchButton.color = new Color(jumpTouchButton.color.r, jumpTouchButton.color.g, jumpTouchButton.color.b, uiPressedAlpha);
                }
                else if (leftTouchButton.HitTest(touch.position)) {
                    leftTouched = true;
                    leftTouchButton.color = new Color(leftTouchButton.color.r, leftTouchButton.color.g, leftTouchButton.color.b, uiPressedAlpha);
                }
                else if (rightTouchButton.HitTest(touch.position)) {
                    rightTouched = true;
                    rightTouchButton.color = new Color(rightTouchButton.color.r, rightTouchButton.color.g, rightTouchButton.color.b, uiPressedAlpha);
                }
            }
        }

        // If button is untouched, change the alpha back to more transparent state
        if (!jumpTouchBegan && !jumpTouchContinue && jumpTouchButton.color.a == uiPressedAlpha) {
            jumpTouchButton.color = new Color(jumpTouchButton.color.r, jumpTouchButton.color.g, jumpTouchButton.color.b, uiUnpressedAlpha);
        }

        if (!leftTouched && leftTouchButton.color.a == uiPressedAlpha) {
            leftTouchButton.color = new Color(leftTouchButton.color.r, leftTouchButton.color.g, leftTouchButton.color.b, uiUnpressedAlpha);
        }

        if (!rightTouched && rightTouchButton.color.a == uiPressedAlpha) {
            rightTouchButton.color = new Color(rightTouchButton.color.r, rightTouchButton.color.g, rightTouchButton.color.b, uiUnpressedAlpha);
        }

        // And the below is a mix of desktop keyboard input handling along with touch input where applicable
        float hMovement = Input.GetAxis("Horizontal");
        if (leftTouched) {
            hMovement = -1f;
        }
        else if (rightTouched) {
            hMovement = 1f;
        }

        // Set "Speed" on the animator for the controller to apply the animation if needed
        animator.SetFloat("Speed", Mathf.Abs(hMovement));

        // Determine movement and animation speed 
        float speed;
        if (Input.GetButton("Run") || leftTouched || rightTouched) {
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
        if ((hMovement > 0f && transform.right.x < 0) || (hMovement < 0f && transform.right.x > 0)) {
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
        if (isOnGround && (Input.GetButtonDown("Jump") || jumpTouchBegan)) {
            doJump = true;
        }

        jumpFloatTimer -= Time.deltaTime;
        // Adjust gravity scale based on the jump button
        if ((Input.GetButton("Jump") || jumpTouchContinue) && jumpFloatTimer > 0) {
            rigidbody2D.gravityScale = gravityScaleFloating;
        }
        else {
            rigidbody2D.gravityScale = gravityScaleNormal;
        }
    }

    void FixedUpdate() {
        // Flag to make sure a second upward force is applied later
        bool didJump = false;

        if (doJump) {
            doJump = false;
            didJump = true;

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

            // Play the jump sound
            AudioSource.PlayClipAtPoint(audioJump, transform.position);
        }

        // Create bounce effect after stomping on an enemy
        if (!didJump && doStompBounce) {
            doStompBounce = false;

            int xForce = 0;
            if (transform.forward.x > 0) {
                xForce = 1;
            }
            else if (transform.forward.x < 0) {
                xForce = -1;
            }

            rigidbody2D.AddForce(new Vector2(stompBounceForceX * xForce, stompBounceForceY));
        }
    }

    public void OnEnemyStomp() {
        doStompBounce = true;
    }

    public void TriggerDeath() {
        // Play death audio
        AudioSource.PlayClipAtPoint(audioDeath, transform.position);

        // Trigger death animation
        animator.SetBool("Dead", true);

        // Ignore user input
        ignoreInput = true;

        // Set to a layer for enemies to not collide with
        gameObject.layer = 10; // "EnemyIgnore"

        // Handle anything on the game controller for player death.
        gameController.OnPlayerDeath();

        // Respawn after a few seconds if we still have remaining lives
        if (gameController.GetCurrentLives() >= 0) {
            Respawn(false, 4f);
        }
    }

    public void Respawn(bool isStartingLevel, float delay) {
        isRespawnAtLevelStart = isStartingLevel;
        Invoke("_respawn", delay);
    }

    private void _respawn() {
        animator.SetBool("Dead", false);
        
        // Reset the level
        gameController.ResetGameState(isRespawnAtLevelStart);
        
        // Restart the background music
        gameController.StartBackgroundMusic();
        
        // Re-enable input
        ignoreInput = false;
        
        // Reset layer to allow enemy collisions
        gameObject.layer = 13; // "Player"
        
        // Reset camera position
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        CameraController cameraController = camera.GetComponent<CameraController>();
        cameraController.ResetPosition();
    }

}
