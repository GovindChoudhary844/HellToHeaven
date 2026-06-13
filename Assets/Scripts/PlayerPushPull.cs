using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerPushPull — Raycast-based push/pull interaction using FixedJoint2D.
///
/// AGENTS.md compliance:
///   - Uses New Input System (PlayerControls.Gameplay.Interact) exclusively.
///   - No legacy Input.GetKeyDown calls.
///   - Physics values are serialized; no hardcoding.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPushPull : MonoBehaviour, PlayerControls.IGameplayActions
{
    [Header("Interaction Settings")]
    public float    grabDistance    = 1.5f;
    public float    raycastOffsetY  = 0.5f;
    public LayerMask pushableLayer;

    private Rigidbody2D     rb;
    private FixedJoint2D    fixedJoint;
    private bool            isGrabbing     = false;
    private GameObject      grabbedObject;
    private SpriteRenderer  spriteRenderer;

    // New Input System
    private PlayerControls _controls;

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
        spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
            Debug.LogError("PlayerPushPull: SpriteRenderer not found on player or children!");

        fixedJoint         = gameObject.AddComponent<FixedJoint2D>();
        fixedJoint.enabled = false;
    }

    private void Update()
    {
        // Guard: release if grabbed object was destroyed externally
        if (isGrabbing && grabbedObject == null)
        {
            ReleaseObject();
        }
    }

    private void LateUpdate()
    {
        if (!isGrabbing || grabbedObject == null) return;

        // Force facing direction towards the grabbed object while holding
        float dirToObject = grabbedObject.transform.position.x - transform.position.x;
        if (Mathf.Abs(dirToObject) > 0.01f)
            spriteRenderer.flipX = dirToObject < 0;
    }

    // -------------------------------------------------------------------------
    // IGameplayActions — New Input System callbacks
    // -------------------------------------------------------------------------

    /// <summary>
    /// Triggered by the "Interact" action (default: F key).
    /// performed = grab attempt; canceled = release.
    /// </summary>
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !isGrabbing)
        {
            float   facingDir  = spriteRenderer.flipX ? -1f : 1f;
            Vector2 rayOrigin  = new Vector2(transform.position.x, transform.position.y + raycastOffsetY);
            Vector2 rayDir     = new Vector2(facingDir, 0);

            Debug.DrawRay(rayOrigin, rayDir * grabDistance, Color.yellow, 0.1f);
            TryGrab(rayOrigin, rayDir);
        }
        else if (ctx.canceled && isGrabbing)
        {
            ReleaseObject();
        }
    }

    // No-op implementations required by IGameplayActions interface contract
    public void OnMove(InputAction.CallbackContext ctx)          { }
    public void OnJump(InputAction.CallbackContext ctx)          { }
    public void OnSprint(InputAction.CallbackContext ctx)        { }
    public void OnRoll(InputAction.CallbackContext ctx)          { }
    public void OnAttackPrimary(InputAction.CallbackContext ctx) { }
    public void OnBlockHold(InputAction.CallbackContext ctx)     { }
    public void OnDeath(InputAction.CallbackContext ctx)         { }
    public void OnHurt(InputAction.CallbackContext ctx)          { }
    public void OnGrabRope(InputAction.CallbackContext ctx)      { }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>Raycasts forward and attaches a FixedJoint2D to a pushable object.</summary>
    private void TryGrab(Vector2 origin, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, grabDistance, pushableLayer);

        if (hit.collider != null && hit.collider.GetComponent<Rigidbody2D>() != null)
        {
            isGrabbing    = true;
            grabbedObject = hit.collider.gameObject;

            fixedJoint.connectedBody = grabbedObject.GetComponent<Rigidbody2D>();
            fixedJoint.enabled       = true;
        }
    }

    /// <summary>Releases the currently grabbed object and disables the joint.</summary>
    private void ReleaseObject()
    {
        isGrabbing               = false;
        fixedJoint.enabled       = false;
        fixedJoint.connectedBody = null;
        grabbedObject            = null;
    }
}