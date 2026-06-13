using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// CharacterManager — Dual-Character System for "Aether & Abyss".
///
/// Design contract:
///   - ONE character is the active Player Character (PC) in Hell.
///   - The OTHER is the inactive Partner NPC in Heaven.
///
/// Partner placeholder support:
///   - If characterB has no HeroKnight script (e.g. a cube placeholder),
///     the manager tracks it via elaraPlaceholder and controls its Renderer
///     and Colliders directly. No crash, no null-ref.
///   - Replace elaraPlaceholder with a real HeroKnight prefab for Elara in Phase 4.
///
/// Camera:
///   - On swap, CameraFollow.target redirects to the new active character.
///
/// Input:
///   - Swap listens on a dedicated PlayerControls instance (F key / Interact).
///
/// AGENTS.md compliance:
///   - New Input System only.
///   - No hardcoded physics or values — all fields are serialized.
///   - Inspects components before modifying them (§10).
/// </summary>
public class CharacterManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Active Player Character (Hell — Kael)")]
    [Tooltip("HeroKnight instance that starts as the playable PC.")]
    [SerializeField] private HeroKnight characterA;

    [Header("Partner NPC (Heaven — Elara)")]
    [Tooltip("HeroKnight instance for Elara. Leave null if using a placeholder.")]
    [SerializeField] private HeroKnight characterB;

    [Tooltip("Fallback: assign Elara's placeholder GameObject (Cube) here when characterB has no HeroKnight.")]
    [SerializeField] private GameObject elaraPlaceholder;

    [Header("Camera")]
    [Tooltip("The main CameraFollow component in the scene.")]
    [SerializeField] private CameraFollow cameraFollow;

    // -------------------------------------------------------------------------
    // Runtime state
    // -------------------------------------------------------------------------

    /// <summary>The character currently under player control (Hell realm).</summary>
    public HeroKnight ActiveCharacter  { get; private set; }

    /// <summary>The character dormant in the Heaven realm (null when placeholder active).</summary>
    public HeroKnight InactiveCharacter { get; private set; }

    // Whether Kael (A) is currently the active character
    private bool _aIsActive = true;

    // Caches filled in Awake — never re-queried at runtime
    private SpriteRenderer  _rendererA;
    private Collider2D[]    _collidersA;

    // For the real HeroKnight B (when available)
    private SpriteRenderer  _rendererB;
    private Collider2D[]    _collidersB;

    // For the cube/placeholder B
    private Renderer        _placeholderRenderer;
    private Collider[]      _placeholderColliders;

    private PlayerControls  _managerControls;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        // Validate: must have at least characterA
        if (characterA == null)
        {
            Debug.LogError("[CharacterManager] characterA (Kael) is not assigned. Disabling.");
            enabled = false;
            return;
        }

        // Warn if neither characterB nor placeholder is set
        if (characterB == null && elaraPlaceholder == null)
            Debug.LogWarning("[CharacterManager] Neither characterB nor elaraPlaceholder is assigned. Swap will have no target.");

        // Resolve camera
        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<CameraFollow>();

        // Cache characterA components
        _rendererA  = characterA.GetComponent<SpriteRenderer>();
        _collidersA = characterA.GetComponents<Collider2D>();

        // Cache characterB (real HeroKnight) components if present
        if (characterB != null)
        {
            _rendererB  = characterB.GetComponent<SpriteRenderer>();
            _collidersB = characterB.GetComponents<Collider2D>();
        }

        // Cache placeholder components if using a cube
        if (elaraPlaceholder != null)
        {
            _placeholderRenderer  = elaraPlaceholder.GetComponent<Renderer>();
            _placeholderColliders = elaraPlaceholder.GetComponents<Collider>();
        }

        // Initial state: Kael active, Elara/placeholder dormant
        _aIsActive      = true;
        ActiveCharacter   = characterA;
        InactiveCharacter = characterB; // may be null if placeholder

        SetHeroKnightActive(characterA, _rendererA, _collidersA, isActive: true);
        SetHeroKnightActive(characterB, _rendererB, _collidersB, isActive: false);
        SetPlaceholderActive(isActive: false);

        SetCameraTarget(characterA.transform);

        // Dedicated swap input
        _managerControls = new PlayerControls();
        _managerControls.Gameplay.Interact.performed += OnSwapPerformed;
        _managerControls.Gameplay.Enable();
    }

    private void OnDestroy()
    {
        if (_managerControls == null) return;
        _managerControls.Gameplay.Interact.performed -= OnSwapPerformed;
        _managerControls.Dispose();
    }

    // =========================================================================
    // Swap
    // =========================================================================

    private void OnSwapPerformed(InputAction.CallbackContext ctx) => SwapCharacters();

    /// <summary>Public swap — can also be called from UI or triggers.</summary>
    public void SwapCharacters()
    {
        _aIsActive = !_aIsActive;

        // Apply states
        SetHeroKnightActive(characterA, _rendererA, _collidersA, isActive:  _aIsActive);

        if (characterB != null)
        {
            SetHeroKnightActive(characterB, _rendererB, _collidersB, isActive: !_aIsActive);
            InactiveCharacter = _aIsActive ? characterB : characterA;
        }
        else
        {
            // Toggle placeholder visibility instead
            SetPlaceholderActive(isActive: !_aIsActive);
        }

        ActiveCharacter = _aIsActive ? characterA : characterB;

        // Redirect camera to the new active character (or keep on Kael if B has no HeroKnight)
        Transform camTarget = ActiveCharacter != null
            ? ActiveCharacter.transform
            : (elaraPlaceholder != null ? elaraPlaceholder.transform : characterA.transform);

        SetCameraTarget(camTarget);

        Debug.Log($"[CharacterManager] Swapped — Kael active: {_aIsActive}");
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private void SetHeroKnightActive(HeroKnight hk, SpriteRenderer rend,
                                      Collider2D[] cols, bool isActive)
    {
        if (hk == null) return;

        hk.enabled = isActive;
        if (rend != null) rend.enabled = isActive;
        if (cols != null)
            foreach (var col in cols) col.enabled = isActive;

        var pp = hk.GetComponent<PlayerPushPull>();
        if (pp != null) pp.enabled = isActive;

        var rg = hk.GetComponent<PlayerRopeGrab>();
        if (rg != null) rg.enabled = isActive;
    }

    private void SetPlaceholderActive(bool isActive)
    {
        if (elaraPlaceholder == null) return;

        if (_placeholderRenderer  != null) _placeholderRenderer.enabled  = isActive;
        if (_placeholderColliders != null)
            foreach (var col in _placeholderColliders) col.enabled = isActive;
    }

    private void SetCameraTarget(Transform newTarget)
    {
        if (cameraFollow != null)
            cameraFollow.SetTarget(newTarget);
    }
}
