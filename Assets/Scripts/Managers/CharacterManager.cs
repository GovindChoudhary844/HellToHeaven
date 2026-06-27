using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

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
    public static event System.Action<bool> OnRealmSwapped;

    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Character Prefabs")]
    [Tooltip("HeroKnight prefab for Kael.")]
    [SerializeField] private GameObject kaelPrefab;

    [Tooltip("HeroKnight prefab for Elara.")]
    [SerializeField] private GameObject elaraPrefab;

    // Track instantiated characters
    private HeroKnight characterA;
    private HeroKnight characterB;

    [Header("Spawning & Location")]
    [Tooltip("The transform where characters spawn when the scene starts.")]
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Camera")]
    [Tooltip("The main CameraFollow component in the scene (Legacy).")]
    [SerializeField] private CameraFollow cameraFollow;

    [Tooltip("The Cinemachine Virtual Camera in the scene.")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

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
        // Validate: must have prefabs assigned
        if (kaelPrefab == null || elaraPrefab == null)
        {
            Debug.LogError("[CharacterManager] Prefabs are not assigned. Disabling.");
            enabled = false;
            return;
        }

        // Resolve camera
        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<CameraFollow>();

        if (virtualCamera == null)
            virtualCamera = FindFirstObjectByType<CinemachineVirtualCamera>();

        // Do NOT spawn here. We wait for SelectCharacter() from the UI.
        
        // Ensure Character Selection UI is active even if user disabled it in the editor
        CharacterSelectionUI selectionUI = FindFirstObjectByType<CharacterSelectionUI>(FindObjectsInactive.Include);
        if (selectionUI != null && !selectionUI.gameObject.activeSelf)
        {
            selectionUI.gameObject.SetActive(true);
        }

        // Dedicated swap input
        _managerControls = new PlayerControls();
        _managerControls.Gameplay.Interact.performed += OnSwapPerformed;
        // Do not enable controls until character is selected
    }

    /// <summary>
    /// Called by the UI buttons to finalize character selection and start the game.
    /// </summary>
    public void SelectCharacter(bool chooseKael)
    {
        Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;

        // Instantiate both characters
        GameObject kaelGo = Instantiate(kaelPrefab, spawnPos, Quaternion.identity);
        GameObject elaraGo = Instantiate(elaraPrefab, spawnPos, Quaternion.identity);

        characterA = kaelGo.GetComponent<HeroKnight>();
        characterB = elaraGo.GetComponent<HeroKnight>();

        // Cache components
        _rendererA  = characterA.GetComponent<SpriteRenderer>();
        _collidersA = characterA.GetComponents<Collider2D>();

        _rendererB  = characterB.GetComponent<SpriteRenderer>();
        _collidersB = characterB.GetComponents<Collider2D>();

        // Set active/inactive based on selection
        _aIsActive = chooseKael;
        ActiveCharacter = chooseKael ? characterA : characterB;
        InactiveCharacter = chooseKael ? characterB : characterA;

        SetHeroKnightActive(characterA, _rendererA, _collidersA, isActive: chooseKael);
        SetHeroKnightActive(characterB, _rendererB, _collidersB, isActive: !chooseKael);

        SetCameraTarget(ActiveCharacter.transform);

        // Enable input
        _managerControls.Gameplay.Enable();

        // Broadcast initial state
        OnRealmSwapped?.Invoke(chooseKael);

        Debug.Log($"[CharacterManager] Game Started. Selected Kael: {chooseKael}");
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
        // Capture outgoing state before swap
        Vector3 syncPosition = transform.position;
        Vector2 syncVelocity = Vector2.zero;
        bool faceLeft = false;

        if (ActiveCharacter != null)
        {
            syncPosition = ActiveCharacter.transform.position;
            var activeRb = ActiveCharacter.GetComponent<Rigidbody2D>();
            if (activeRb != null) syncVelocity = activeRb.linearVelocity;
            
            var activeRend = ActiveCharacter.GetComponent<SpriteRenderer>();
            if (activeRend != null) faceLeft = activeRend.flipX;
        }

        _aIsActive = !_aIsActive;

        // Apply states
        SetHeroKnightActive(characterA, _rendererA, _collidersA, isActive:  _aIsActive);

        if (characterB != null)
        {
            // Sync incoming position
            if (!_aIsActive) characterB.transform.position = syncPosition;
            else if (_aIsActive && characterA != null) characterA.transform.position = syncPosition;

            SetHeroKnightActive(characterB, _rendererB, _collidersB, isActive: !_aIsActive);
            InactiveCharacter = _aIsActive ? characterB : characterA;
        }

        ActiveCharacter = _aIsActive ? characterA : characterB;

        // Sync incoming velocity and facing to preserve momentum in mid-air
        if (ActiveCharacter != null)
        {
            var incomingRb = ActiveCharacter.GetComponent<Rigidbody2D>();
            if (incomingRb != null) incomingRb.linearVelocity = syncVelocity;

            var incomingRend = ActiveCharacter.GetComponent<SpriteRenderer>();
            if (incomingRend != null) incomingRend.flipX = faceLeft;
        }

        // Redirect camera to the new active character
        Transform camTarget = ActiveCharacter != null ? ActiveCharacter.transform : transform;
        SetCameraTarget(camTarget);

        // Broadcast swap to environment
        OnRealmSwapped?.Invoke(_aIsActive);

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

        // Suspend physics for the inactive character so they don't fall through the floor!
        var rb = hk.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = isActive;
            if (!isActive) rb.linearVelocity = Vector2.zero; // Clear velocity when putting to sleep
        }

        var pp = hk.GetComponent<PlayerPushPull>();
        if (pp != null) pp.enabled = isActive;

        var rg = hk.GetComponent<PlayerRopeGrab>();
        if (rg != null) rg.enabled = isActive;
    }

    private void SetCameraTarget(Transform newTarget)
    {
        if (cameraFollow != null)
            cameraFollow.SetTarget(newTarget);
            
        if (virtualCamera != null)
            virtualCamera.Follow = newTarget;
    }
}
