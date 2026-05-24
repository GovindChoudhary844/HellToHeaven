using UnityEngine;

public class HeroKnight : MonoBehaviour
{
    [Header("Movement (Crisp & Heavy)")]
    [SerializeField] private float walkSpeed = 4.0f;
    [SerializeField] private float sprintSpeed = 7.0f;
    [SerializeField] private float acceleration = 40.0f;
    [SerializeField] private float deceleration = 40.0f;
    [SerializeField] private float airAcceleration = 20.0f;

    [Header("Jumping (Strict Limits)")]
    [SerializeField] private float jumpForce = 12.0f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float jumpCutMultiplier = 0.4f;
    [SerializeField] private int maxJumps = 2;

    [Header("Foolproof Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Invisible Polish")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Combat & Actions")]
    [SerializeField] private float rollForce = 6.0f;
    [SerializeField] private float attackLockDuration = 0.4f;
    [SerializeField] private bool noBlood = false;
    [SerializeField] private GameObject slideDust;

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
    private float jumpCooldownTimer; // CRITICAL: The airtight lock
    private float rollCurrentTime;
    private float rollDuration = 8.0f / 14.0f;

    private int currentAttack = 0;
    private float timeSinceAttack = 0.0f;
    private float delayToIdle = 0.0f;

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

            // STRICT RULE: Only refill jumps if resting on the ground AND not in a jump cooldown
            if (body2d.linearVelocity.y <= 0.1f && jumpCooldownTimer <= 0f)
            {
                jumpsRemaining = maxJumps;
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;

            // STRICT RULE: Walk off a ledge = lose your ground jump instantly
            if (wasGrounded && jumpsRemaining == maxJumps)
            {
                jumpsRemaining = 1;
            }
        }
    }

    private void HandleInput()
    {
        jumpCooldownTimer -= Time.deltaTime; // Always tick down the lock timer

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Space) && body2d.linearVelocity.y > 0)
        {
            body2d.linearVelocity = new Vector2(body2d.linearVelocity.x, body2d.linearVelocity.y * jumpCutMultiplier);
        }

        // AIRTIGHT CHECK: jumpCooldownTimer must be <= 0f to allow jumping
        if (jumpBufferCounter > 0f && !rolling && timeSinceAttack >= attackLockDuration && jumpCooldownTimer <= 0f)
        {
            if (coyoteTimeCounter > 0f || jumpsRemaining > 0)
            {
                ExecuteJump();
            }
        }
    }

    private void ExecuteJump()
    {
        animator.SetTrigger(Tags.Jump);
        grounded = false;
        animator.SetBool(Tags.Grounded, grounded);

        body2d.linearVelocity = new Vector2(body2d.linearVelocity.x, 0);
        body2d.linearVelocity = new Vector2(body2d.linearVelocity.x, jumpForce);

        jumpsRemaining--;
        jumpCooldownTimer = 0.2f; // CRITICAL: Locks out all jumps and jump-resets for 0.2 seconds!
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
    }

    private void ApplySmoothMovement()
    {
        if (rolling) return;

        float inputX = Input.GetAxisRaw("Horizontal");
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (grounded && timeSinceAttack < attackLockDuration)
        {
            inputX = 0f;
        }

        if (inputX > 0) { GetComponent<SpriteRenderer>().flipX = false; facingDirection = 1; }
        else if (inputX < 0) { GetComponent<SpriteRenderer>().flipX = true; facingDirection = -1; }

        float targetSpeed = inputX * (isSprinting ? sprintSpeed : walkSpeed);

        float accelRate;
        if (Mathf.Abs(targetSpeed) > 0.01f)
            accelRate = grounded ? acceleration : airAcceleration;
        else
            accelRate = grounded ? deceleration : airAcceleration;

        float speedDif = targetSpeed - body2d.linearVelocity.x;
        float movement = speedDif * accelRate * Time.deltaTime;

        body2d.linearVelocity = new Vector2(body2d.linearVelocity.x + movement, body2d.linearVelocity.y);
    }

    private void ApplyCinematicGravity()
    {
        if (body2d.linearVelocity.y < 0 && !grounded)
        {
            body2d.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    private void UpdateAnimations()
    {
        animator.SetFloat(Tags.AirSpeedY, body2d.linearVelocity.y);
        animator.SetBool(Tags.Grounded, grounded);

        isWallSliding = (wallSensorR1.State() && wallSensorR2.State()) || (wallSensorL1.State() && wallSensorL2.State());
        animator.SetBool(Tags.WallSlide, isWallSliding);

        float inputX = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(inputX) > Mathf.Epsilon && timeSinceAttack >= attackLockDuration)
        {
            delayToIdle = 0.05f;
            animator.SetInteger(Tags.AnimState, 1);
        }
        else
        {
            delayToIdle -= Time.deltaTime;
            if (delayToIdle < 0) animator.SetInteger(Tags.AnimState, 0);
        }
    }

    private void HandleCombatAndActions()
    {
        if (Input.GetKeyDown(KeyCode.E) && !rolling)
        {
            animator.SetBool(Tags.NoBlood, noBlood);
            animator.SetTrigger(Tags.Death);
        }
        else if (Input.GetKeyDown(KeyCode.Q) && !rolling)
            animator.SetTrigger(Tags.Hurt);

        else if (Input.GetMouseButtonDown(0) && timeSinceAttack > 0.25f && !rolling && grounded)
        {
            currentAttack++;
            if (currentAttack > 3) currentAttack = 1;
            if (timeSinceAttack > 1.0f) currentAttack = 1;

            if (currentAttack == 1) animator.SetTrigger(Tags.Attack1);
            else if (currentAttack == 2) animator.SetTrigger(Tags.Attack2);
            else if (currentAttack == 3) animator.SetTrigger(Tags.Attack3);

            timeSinceAttack = 0.0f;
        }

        else if (Input.GetMouseButtonDown(1) && !rolling)
        {
            animator.SetTrigger(Tags.Block);
            animator.SetBool(Tags.IdleBlock, true);
        }
        else if (Input.GetMouseButtonUp(1))
            animator.SetBool(Tags.IdleBlock, false);

        else if (Input.GetKeyDown(KeyCode.LeftControl) && !rolling && !isWallSliding && timeSinceAttack >= attackLockDuration)
        {
            rolling = true;
            rollCurrentTime = 0f;
            animator.SetTrigger(Tags.Roll);
            body2d.linearVelocity = new Vector2(facingDirection * rollForce, body2d.linearVelocity.y);
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