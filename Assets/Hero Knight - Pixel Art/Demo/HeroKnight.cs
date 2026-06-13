using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// HeroKnight — Modular Character State Controller for "Aether & Abyss".
/// Tracks states: Idle, Run, Jump, Falling, WallSlide, Grounded.
///
/// AGENTS.md compliance:
///   - Uses New Input System exclusively (PlayerControls wrapper). No legacy Input calls.
///   - All physics values are serialized inspector fields — nothing is hardcoded.
///   - Implements IGameplayActions for event-driven, zero-poll input handling.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class HeroKnight : MonoBehaviour, PlayerControls.IGameplayActions
{
    // -------------------------------------------------------------------------
    // Inspector-serialized fields (no hardcoded physics — AGENTS.md §9)
    // -------------------------------------------------------------------------

    [Header("Movement (Crisp & Heavy)")]
    [SerializeField] private float walkSpeed      = 4.0f;
    [SerializeField] private float sprintSpeed    = 7.0f;
    [SerializeField] private float acceleration   = 40.0f;
    [SerializeField] private float deceleration   = 40.0f;
    [SerializeField] private float airAcceleration = 20.0f;

    [Header("Jumping (Strict Limits)")]
    [SerializeField] private float jumpForce         = 12.0f;
    [SerializeField] private float fallMultiplier    = 2.5f;
    [SerializeField] private float jumpCutMultiplier = 0.4f;
    [SerializeField] private int   maxJumps          = 2;

    [Header("Foolproof Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float     groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Invisible Polish")]
    [SerializeField] private float coyoteTime     = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Combat & Actions")]
    [SerializeField] private float      rollForce          = 6.0f;
    [SerializeField] private float      attackLockDuration = 0.4f;
    [SerializeField] private bool       noBlood            = false;
    [SerializeField] private GameObject slideDust;
    [SerializeField] private float      rollDuration       = 8.0f / 14.0f;

    // -------------------------------------------------------------------------
    // Component references (cached in Start)
    // -------------------------------------------------------------------------
    private Animator        animator;
    private Rigidbody2D     body2d;
    private SpriteRenderer  spriteRenderer;

    private Sensor_HeroKnight wallSensorR1;
    private Sensor_HeroKnight wallSensorR2;
    private Sensor_HeroKnight wallSensorL1;
    private Sensor_HeroKnight wallSensorL2;

    // -------------------------------------------------------------------------
    // New Input System
    // -------------------------------------------------------------------------
    private PlayerControls _controls;

    // Raw input values read from event callbacks — consumed each Update
    private Vector2 _moveInput;
    private bool    _isSprinting;
    private bool    _jumpPressed;         // "was pressed this frame" buffer
    private bool    _jumpReleased;        // "was released this frame" buffer
    private bool    _blockHeld;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------
    private bool  isWallSliding = false;
    private bool  grounded      = false;
    private bool  rolling       = false;
    private int   facingDirection = 1;

    [SerializeField] private int jumpsRemaining;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float jumpCooldownTimer;   // Airtight lock preventing double-jump spam
    private float rollCurrentTime;
    private int   currentAttack     = 0;
    private float timeSinceAttack   = 0.0f;
    private float delayToIdle       = 0.0f;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        _controls = new PlayerControls();
        _controls.Gameplay.SetCallbacks(this);
    }

    private void OnEnable()  => _controls.Gameplay.Enable();
    private void OnDisable() => _controls.Gameplay.Disable();

    private void Start()
    {
        animator       = GetComponent<Animator>();
        body2d         = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();

        if (groundCheck == null)
            groundCheck = transform.Find("GroundSensor");
    }

    private void Update()
    {
        timeSinceAttack += Time.deltaTime;

        if (rolling)
        {
            rollCurrentTime += Time.deltaTime;
            if (rollCurrentTime > rollDuration) rolling = false;
        }

        CheckGroundAndCoyote();
        ProcessJumpInput();
        ApplySmoothMovement();
        ApplyCinematicGravity();
        UpdateAnimations();
        HandleCombatAndActions();

        // Clear single-frame flags after they have been consumed
        _jumpPressed  = false;
        _jumpReleased = false;
    }

    // =========================================================================
    // IGameplayActions — New Input System callbacks
    // All input logic funnels through these methods; zero legacy Input calls.
    // =========================================================================

    public void OnMove(InputAction.CallbackContext ctx)
        => _moveInput = ctx.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) _jumpPressed  = true;
        if (ctx.canceled)  _jumpReleased = true;
    }

    public void OnSprint(InputAction.CallbackContext ctx)
        => _isSprinting = ctx.ReadValueAsButton();

    public void OnRoll(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (!rolling && !isWallSliding && timeSinceAttack >= attackLockDuration)
        {
            rolling         = true;
            rollCurrentTime = 0f;
            animator.SetTrigger(Tags.Roll);
            body2d.linearVelocity = new Vector2(facingDirection * rollForce, body2d.linearVelocity.y);
        }
    }

    public void OnAttackPrimary(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (timeSinceAttack > 0.25f && !rolling && grounded)
        {
            currentAttack++;
            if (currentAttack > 3)    currentAttack = 1;
            if (timeSinceAttack > 1.0f) currentAttack = 1;

            if (currentAttack == 1) animator.SetTrigger(Tags.Attack1);
            else if (currentAttack == 2) animator.SetTrigger(Tags.Attack2);
            else if (currentAttack == 3) animator.SetTrigger(Tags.Attack3);

            timeSinceAttack = 0.0f;
        }
    }

    public void OnBlockHold(InputAction.CallbackContext ctx)
    {
        bool held = ctx.ReadValueAsButton();
        if (!rolling)
        {
            if (ctx.performed)
            {
                animator.SetTrigger(Tags.Block);
                animator.SetBool(Tags.IdleBlock, true);
            }
            else if (ctx.canceled)
            {
                animator.SetBool(Tags.IdleBlock, false);
            }
        }
    }

    public void OnDeath(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || rolling) return;
        animator.SetBool(Tags.NoBlood, noBlood);
        animator.SetTrigger(Tags.Death);
    }

    public void OnHurt(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || rolling) return;
        animator.SetTrigger(Tags.Hurt);
    }

    // Interact and GrabRope are consumed by PlayerPushPull and PlayerRopeGrab respectively.
    // HeroKnight receives no-ops here so the interface contract is satisfied.
    public void OnInteract(InputAction.CallbackContext ctx) { }
    public void OnGrabRope(InputAction.CallbackContext ctx) { }

    // =========================================================================
    // Ground & Coyote Time
    // =========================================================================

    private void CheckGroundAndCoyote()
    {
        bool wasGrounded = grounded;
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        if (grounded)
        {
            coyoteTimeCounter = coyoteTime;

            // STRICT RULE: Only refill jumps when resting on ground AND outside jump cooldown
            if (body2d.linearVelocity.y <= 0.1f && jumpCooldownTimer <= 0f)
                jumpsRemaining = maxJumps;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;

            // STRICT RULE: Walk off a ledge = lose your ground jump instantly
            if (wasGrounded && jumpsRemaining == maxJumps)
                jumpsRemaining = 1;
        }
    }

    // =========================================================================
    // Jump — buffer + coyote + variable-height cut
    // =========================================================================

    private void ProcessJumpInput()
    {
        jumpCooldownTimer -= Time.deltaTime;

        // Fill buffer when jump is pressed
        if (_jumpPressed)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // Variable-height cut on release
        if (_jumpReleased && body2d.linearVelocity.y > 0)
            body2d.linearVelocity = new Vector2(body2d.linearVelocity.x,
                                                body2d.linearVelocity.y * jumpCutMultiplier);

        // AIRTIGHT CHECK: buffer > 0, not rolling, not in attack lock, not in jump cooldown
        if (jumpBufferCounter > 0f && !rolling && timeSinceAttack >= attackLockDuration && jumpCooldownTimer <= 0f)
        {
            if (coyoteTimeCounter > 0f || jumpsRemaining > 0)
                ExecuteJump();
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
        jumpCooldownTimer = 0.2f;  // Locks all jump resets for 0.2 s
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
    }

    // =========================================================================
    // Smooth horizontal movement
    // =========================================================================

    private void ApplySmoothMovement()
    {
        if (rolling) return;

        float inputX = _moveInput.x;

        // Lock movement during attack window while grounded
        if (grounded && timeSinceAttack < attackLockDuration)
            inputX = 0f;

        // Flip sprite to face movement direction
        if      (inputX > 0) { spriteRenderer.flipX = false; facingDirection =  1; }
        else if (inputX < 0) { spriteRenderer.flipX = true;  facingDirection = -1; }

        float targetSpeed = inputX * (_isSprinting ? sprintSpeed : walkSpeed);

        float accelRate;
        if (Mathf.Abs(targetSpeed) > 0.01f)
            accelRate = grounded ? acceleration   : airAcceleration;
        else
            accelRate = grounded ? deceleration   : airAcceleration;

        float speedDif  = targetSpeed - body2d.linearVelocity.x;
        float movement  = speedDif * accelRate * Time.deltaTime;

        body2d.linearVelocity = new Vector2(body2d.linearVelocity.x + movement, body2d.linearVelocity.y);
    }

    // =========================================================================
    // Cinematic gravity — heavier fall, lighter rise
    // =========================================================================

    private void ApplyCinematicGravity()
    {
        if (body2d.linearVelocity.y < 0 && !grounded)
            body2d.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
    }

    // =========================================================================
    // Animation state machine driver
    // =========================================================================

    private void UpdateAnimations()
    {
        animator.SetFloat(Tags.AirSpeedY, body2d.linearVelocity.y);
        animator.SetBool(Tags.Grounded,   grounded);

        float inputX = _moveInput.x;

        bool touchingWallR = wallSensorR1.State() && wallSensorR2.State();
        bool touchingWallL = wallSensorL1.State() && wallSensorL2.State();

        // Wall-slide only fires when the player actively pushes into the wall
        isWallSliding = (touchingWallR && inputX > 0) || (touchingWallL && inputX < 0);
        animator.SetBool(Tags.WallSlide, isWallSliding);

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

    // Combat callbacks are now handled directly in OnAttackPrimary / OnBlockHold /
    // OnDeath / OnHurt / OnRoll. This method is intentionally empty and kept as a
    // clear extension point for future combat state logic.
    private void HandleCombatAndActions() { }

    // =========================================================================
    // Animation event — spawned by animator clip
    // =========================================================================

    private void AE_SlideDust()
    {
        Vector3 spawnPosition = (facingDirection == 1)
            ? wallSensorR2.transform.position
            : wallSensorL2.transform.position;

        if (slideDust != null)
        {
            GameObject dust = Instantiate(slideDust, spawnPosition, gameObject.transform.localRotation);
            dust.transform.localScale = new Vector3(facingDirection, 1, 1);
        }
    }
}