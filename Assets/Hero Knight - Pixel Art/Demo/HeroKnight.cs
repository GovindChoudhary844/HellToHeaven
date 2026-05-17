using UnityEngine;

public class HeroKnight : MonoBehaviour
{
    [Header("Movement (Crisp & Heavy)")]
    [SerializeField] private float walkSpeed = 4.0f;
    [SerializeField] private float sprintSpeed = 7.0f;
    [SerializeField] private float acceleration = 40.0f;
    [SerializeField] private float deceleration = 40.0f;
    [SerializeField] private float airAcceleration = 20.0f;

    [Header("Jumping (Smooth & Forgiving)")]
    [SerializeField] private float jumpForce = 12.0f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float jumpCutMultiplier = 0.4f;
    [SerializeField] private int maxJumps = 2;

    [Header("FOOLPROOF GROUND DETECTION")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Invisible Polish")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Combat & Actions")]
    [SerializeField] private float rollForce = 6.0f;
    [SerializeField] private float attackLockDuration = 0.4f; // NEW: How long he is locked in place while attacking
    [SerializeField] private bool noBlood = false;
    [SerializeField] private GameObject slideDust;

    // Internal Physics & Timers
    private Animator animator;
    private Rigidbody2D body2d;

    private Sensor_HeroKnight wallSensorR1;
    private Sensor_HeroKnight wallSensorR2;
    private Sensor_HeroKnight wallSensorL1;
    private Sensor_HeroKnight wallSensorL2;

    private bool isWallSliding = false;
    private bool grounded = false;
    private bool rolling = false;
    private int facingDirection = 1;

    [SerializeField] private int jumpsRemaining;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float rollCurrentTime;
    private float rollDuration = 8.0f / 14.0f;

    private int currentAttack = 0;
    private float timeSinceAttack = 0.0f;
    private float delayToIdle = 0.0f;

    // Cached Animation Hashes
    private readonly int hashGrounded = Animator.StringToHash("Grounded");
    private readonly int hashJump = Animator.StringToHash("Jump");
    private readonly int hashAnimState = Animator.StringToHash("AnimState");
    private readonly int hashAirSpeedY = Animator.StringToHash("AirSpeedY");
    private readonly int hashWallSlide = Animator.StringToHash("WallSlide");
    private readonly int hashDeath = Animator.StringToHash("Death");
    private readonly int hashHurt = Animator.StringToHash("Hurt");
    private readonly int hashAttack1 = Animator.StringToHash("Attack1");
    private readonly int hashAttack2 = Animator.StringToHash("Attack2");
    private readonly int hashAttack3 = Animator.StringToHash("Attack3");
    private readonly int hashBlock = Animator.StringToHash("Block");
    private readonly int hashIdleBlock = Animator.StringToHash("IdleBlock");
    private readonly int hashRoll = Animator.StringToHash("Roll");
    private readonly int hashNoBlood = Animator.StringToHash("noBlood");

    void Start()
    {
        animator = GetComponent<Animator>();
        body2d = GetComponent<Rigidbody2D>();

        wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();

        if (groundCheck == null)
        {
            groundCheck = transform.Find("GroundSensor");
        }
    }

    void Update()
    {
        timeSinceAttack += Time.deltaTime;

        if (rolling)
        {
            rollCurrentTime += Time.deltaTime;
            if (rollCurrentTime > rollDuration) rolling = false;
        }

        CheckGroundAndCoyote();
        HandleInput();
        ApplySmoothMovement();
        ApplyCinematicGravity();
        UpdateAnimations();
        HandleCombatAndActions();
    }

    private void CheckGroundAndCoyote()
    {
        bool wasGrounded = grounded;
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        if (grounded)
        {
            coyoteTimeCounter = coyoteTime;
            if (body2d.velocity.y <= 0.1f)
            {
                jumpsRemaining = maxJumps;
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            if (wasGrounded && jumpsRemaining == maxJumps)
            {
                jumpsRemaining = maxJumps - 1;
            }
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Space) && body2d.velocity.y > 0)
        {
            body2d.velocity = new Vector2(body2d.velocity.x, body2d.velocity.y * jumpCutMultiplier);
        }

        // UPDATED: Prevent jumping if the player is currently locked in an attack animation
        if (jumpBufferCounter > 0f && !rolling && timeSinceAttack >= attackLockDuration)
        {
            if (coyoteTimeCounter > 0f || jumpsRemaining > 0)
            {
                ExecuteJump();
            }
        }
    }

    private void ExecuteJump()
    {
        animator.SetTrigger(hashJump);
        grounded = false;
        animator.SetBool(hashGrounded, grounded);

        body2d.velocity = new Vector2(body2d.velocity.x, 0);
        body2d.velocity = new Vector2(body2d.velocity.x, jumpForce);

        jumpsRemaining--;
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
    }

    private void ApplySmoothMovement()
    {
        if (rolling) return;

        float inputX = Input.GetAxisRaw("Horizontal");
        bool isSSprint = Input.GetKey(KeyCode.LeftShift);

        // --- THE FIX: Block movement input if attacking ---
        // If Kael is on the ground and recently attacked, force his input to 0 so he slides to a stop
        if (grounded && timeSinceAttack < attackLockDuration)
        {
            inputX = 0f;
        }

        if (inputX > 0) { GetComponent<SpriteRenderer>().flipX = false; facingDirection = 1; }
        else if (inputX < 0) { GetComponent<SpriteRenderer>().flipX = true; facingDirection = -1; }

        float targetSpeed = inputX * (isSSprint ? sprintSpeed : walkSpeed);

        float accelRate;
        if (Mathf.Abs(targetSpeed) > 0.01f)
            accelRate = grounded ? acceleration : airAcceleration;
        else
            accelRate = grounded ? deceleration : airAcceleration;

        float speedDif = targetSpeed - body2d.velocity.x;
        float movement = speedDif * accelRate * Time.deltaTime;

        body2d.velocity = new Vector2(body2d.velocity.x + movement, body2d.velocity.y);
    }

    private void ApplyCinematicGravity()
    {
        if (body2d.velocity.y < 0 && !grounded)
        {
            body2d.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    private void UpdateAnimations()
    {
        animator.SetFloat(hashAirSpeedY, body2d.velocity.y);
        animator.SetBool(hashGrounded, grounded);

        isWallSliding = (wallSensorR1.State() && wallSensorR2.State()) || (wallSensorL1.State() && wallSensorL2.State());
        animator.SetBool(hashWallSlide, isWallSliding);

        float inputX = Input.GetAxisRaw("Horizontal");

        // Prevent the run animation from playing if we are attacking
        if (Mathf.Abs(inputX) > Mathf.Epsilon && timeSinceAttack >= attackLockDuration)
        {
            delayToIdle = 0.05f;
            animator.SetInteger(hashAnimState, 1);
        }
        else
        {
            delayToIdle -= Time.deltaTime;
            if (delayToIdle < 0) animator.SetInteger(hashAnimState, 0);
        }
    }

    private void HandleCombatAndActions()
    {
        if (Input.GetKeyDown(KeyCode.E) && !rolling)
        {
            animator.SetBool(hashNoBlood, noBlood);
            animator.SetTrigger(hashDeath);
        }
        else if (Input.GetKeyDown(KeyCode.Q) && !rolling)
            animator.SetTrigger(hashHurt);

        else if (Input.GetMouseButtonDown(0) && timeSinceAttack > 0.25f && !rolling && grounded) // Only attack on ground
        {
            currentAttack++;
            if (currentAttack > 3) currentAttack = 1;
            if (timeSinceAttack > 1.0f) currentAttack = 1;

            if (currentAttack == 1) animator.SetTrigger(hashAttack1);
            else if (currentAttack == 2) animator.SetTrigger(hashAttack2);
            else if (currentAttack == 3) animator.SetTrigger(hashAttack3);

            timeSinceAttack = 0.0f;
        }

        else if (Input.GetMouseButtonDown(1) && !rolling)
        {
            animator.SetTrigger(hashBlock);
            animator.SetBool(hashIdleBlock, true);
        }
        else if (Input.GetMouseButtonUp(1))
            animator.SetBool(hashIdleBlock, false);

        else if (Input.GetKeyDown(KeyCode.LeftControl) && !rolling && !isWallSliding && timeSinceAttack >= attackLockDuration)
        {
            rolling = true;
            rollCurrentTime = 0f;
            animator.SetTrigger(hashRoll);
            body2d.velocity = new Vector2(facingDirection * rollForce, body2d.velocity.y);
        }
    }

    void AE_SlideDust()
    {
        Vector3 spawnPosition = (facingDirection == 1) ? wallSensorR2.transform.position : wallSensorL2.transform.position;
        if (slideDust != null)
        {
            GameObject dust = Instantiate(slideDust, spawnPosition, gameObject.transform.localRotation);
            dust.transform.localScale = new Vector3(facingDirection, 1, 1);
        }
    }
}