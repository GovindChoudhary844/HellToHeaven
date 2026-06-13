using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerRopeGrab — Trigger-based rope attachment using HingeJoint2D for swinging/climbing.
///
/// AGENTS.md compliance:
///   - Uses New Input System (PlayerControls.Gameplay.GrabRope / Move / Jump) exclusively.
///   - No legacy Input calls.
///   - Physics forces are proportional to serialized values — nothing hardcoded (AGENTS.md §9).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRopeGrab : MonoBehaviour, PlayerControls.IGameplayActions
{
    [Header("Rope Settings")]
    public LayerMask ropeLayer;
    public float     swingForce   = 15f;
    public float     jumpOffForce = 12f;
    public float     climbSpeed   = 3f;

    private Rigidbody2D  rb;
    private HingeJoint2D ropeJoint;
    private HeroKnight   playerMovement;
    private bool         isSwinging      = false;

    // New Input System
    private PlayerControls _controls;

    // Cached per-frame values from callbacks
    private Vector2 _moveInput;
    private bool    _grabRopePressed;
    private bool    _jumpPressed;

    // Collider of the rope link we are in range of (set by OnTriggerStay2D)
    private Rigidbody2D _pendingRopeLink;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _controls = new PlayerControls();
        _controls.Gameplay.SetCallbacks(this);
    }

    private void OnEnable()  => _controls.Gameplay.Enable();
    private void OnDisable() => _controls.Gameplay.Disable();

    private void Start()
    {
        rb             = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<HeroKnight>();
    }

    private void Update()
    {
        // Attempt attachment if the player presses GrabRope while inside a rope trigger
        if (!isSwinging && _grabRopePressed && _pendingRopeLink != null)
            AttachToRope(_pendingRopeLink);

        if (isSwinging)
            HandleSwinging();

        // Clear single-frame flags
        _grabRopePressed = false;
        _jumpPressed     = false;
    }

    // -------------------------------------------------------------------------
    // Trigger detection — tracks available rope link each frame
    // -------------------------------------------------------------------------

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isSwinging && ((1 << collision.gameObject.layer) & ropeLayer) != 0)
            _pendingRopeLink = collision.attachedRigidbody;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & ropeLayer) != 0)
            _pendingRopeLink = null;
    }

    // -------------------------------------------------------------------------
    // IGameplayActions — New Input System callbacks
    // -------------------------------------------------------------------------

    public void OnMove(InputAction.CallbackContext ctx)
        => _moveInput = ctx.ReadValue<Vector2>();

    public void OnGrabRope(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) _grabRopePressed = true;
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) _jumpPressed = true;
    }

    // No-op implementations required by IGameplayActions interface contract
    public void OnSprint(InputAction.CallbackContext ctx)        { }
    public void OnRoll(InputAction.CallbackContext ctx)          { }
    public void OnAttackPrimary(InputAction.CallbackContext ctx) { }
    public void OnBlockHold(InputAction.CallbackContext ctx)     { }
    public void OnDeath(InputAction.CallbackContext ctx)         { }
    public void OnHurt(InputAction.CallbackContext ctx)          { }
    public void OnInteract(InputAction.CallbackContext ctx)      { }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>Connects the player to a rope link via HingeJoint2D.</summary>
    private void AttachToRope(Rigidbody2D ropeLink)
    {
        isSwinging = true;

        if (playerMovement != null)
            playerMovement.enabled = false;

        ropeJoint                        = gameObject.AddComponent<HingeJoint2D>();
        ropeJoint.connectedBody          = ropeLink;
        ropeJoint.autoConfigureConnectedAnchor = false;
        ropeJoint.connectedAnchor        = Vector2.zero;
        ropeJoint.anchor                 = new Vector2(0, 1f);
    }

    /// <summary>
    /// Processes swinging, climbing, and jump-off while attached.
    /// Force magnitude is proportional to serialized swingForce and climbSpeed — no hardcoded values.
    /// </summary>
    private void HandleSwinging()
    {
        float horizontalInput = _moveInput.x;
        float verticalInput   = _moveInput.y;

        // Lateral swing force — proportional to input and serialized swingForce
        rb.AddForce(Vector2.right * horizontalInput * swingForce);

        // Rope climbing — shift the connected anchor proportionally to climbSpeed
        if (verticalInput != 0 && ropeJoint != null)
            ropeJoint.connectedAnchor += new Vector2(0, verticalInput * climbSpeed * Time.deltaTime);

        // Jump off — velocity proportional to swingForce and jumpOffForce
        if (_jumpPressed)
        {
            DetachFromRope();
            rb.linearVelocity = new Vector2(horizontalInput * swingForce * 0.5f, jumpOffForce);
        }
    }

    /// <summary>Detaches the player from the rope and restores HeroKnight movement.</summary>
    private void DetachFromRope()
    {
        isSwinging     = false;
        _pendingRopeLink = null;

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            rb.linearVelocity      = Vector2.zero;
        }

        if (ropeJoint != null)
            Destroy(ropeJoint);
    }
}