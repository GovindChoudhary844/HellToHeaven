// =============================================================================
//  PlayerControls.cs  —  Auto-generated C# wrapper for PlayerControls.inputactions
//  DO NOT EDIT MANUALLY. Regenerate by re-importing the .inputactions asset
//  via Edit > Project Settings > Input System Package, or by right-clicking
//  the asset and selecting "Generate C# Class".
//
//  To use this class:
//      private PlayerControls _controls;
//      void Awake() { _controls = new PlayerControls(); }
//      void OnEnable()  { _controls.Enable(); }
//      void OnDisable() { _controls.Disable(); }
//
//  Then subscribe to callbacks:
//      _controls.Gameplay.Jump.performed += ctx => OnJump(ctx);
// =============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

/// <summary>
/// Strongly-typed wrapper around the PlayerControls.inputactions asset.
/// Provides event callbacks for all Gameplay actions defined in the asset.
/// </summary>
public class PlayerControls : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }

    public PlayerControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""Gameplay"",
            ""id"": ""a1b2c3d4-0001-0001-0001-aaaaaaaaaaaa"",
            ""actions"": [
                { ""name"": ""Move"",           ""type"": ""Value"",  ""id"": ""a1b2c3d4-0002-0001-0001-aaaaaaaaaaaa"", ""expectedControlType"": ""Vector2"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": true  },
                { ""name"": ""Jump"",           ""type"": ""Button"", ""id"": ""a1b2c3d4-0003-0001-0001-aaaaaaaaaaaa"", ""expectedControlType"": ""Button"",  ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""Sprint"",         ""type"": ""Button"", ""id"": ""a1b2c3d4-0004-0001-0001-aaaaaaaaaaaa"", ""expectedControlType"": ""Button"",  ""processors"": """", ""interactions"": """", ""initialStateCheck"": true  },
                { ""name"": ""Roll"",           ""type"": ""Button"", ""id"": ""a1b2c3d4-0005-0001-0001-aaaaaaaaaaaa"", ""expectedControlType"": ""Button"",  ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""AttackPrimary"",  ""type"": ""Button"", ""id"": ""a1b2c3d4-0006-0001-0001-aaaaaaaaaaaa"", ""expectedControlType"": ""Button"",  ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""BlockHold"",      ""type"": ""Button"", ""id"": ""a1b2c3d4-0007-0001-0001-aaaaaaaaaaaa"", ""expectedControlType"": ""Button"",  ""processors"": """", ""interactions"": """", ""initialStateCheck"": true  },
                { ""name"": ""Death"",          ""type"": ""Button"", ""id"": ""a1b2c3d4-0008-0001-0001-aaaaaaaaaaaa"", ""expectedControlType"": ""Button"",  ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""Hurt"",           ""type"": ""Button"", ""id"": ""a1b2c3d4-0009-0001-0001-aaaaaaaaaaaa"", ""expectedControlType"": ""Button"",  ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""Interact"",       ""type"": ""Button"", ""id"": ""a1b2c3d4-0010-0001-0001-aaaaaaaaaaaa"", ""expectedControlType"": ""Button"",  ""processors"": """", ""interactions"": """", ""initialStateCheck"": true  },
                { ""name"": ""GrabRope"",       ""type"": ""Button"", ""id"": ""a1b2c3d4-0011-0001-0001-aaaaaaaaaaaa"", ""expectedControlType"": ""Button"",  ""processors"": """", ""interactions"": """", ""initialStateCheck"": false }
            ],
            ""bindings"": [
                { ""name"": ""WASD"",      ""id"": ""b1b2c3d4-0001-0001-0001-aaaaaaaaaaaa"", ""path"": ""2DVector"", ""action"": ""Move"", ""isComposite"": true,  ""isPartOfComposite"": false, ""groups"": """" },
                { ""name"": ""up"",        ""id"": ""b1b2c3d4-0001-0001-0002-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/w"",           ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true,  ""groups"": ""Keyboard&Mouse"" },
                { ""name"": ""down"",      ""id"": ""b1b2c3d4-0001-0001-0003-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/s"",           ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true,  ""groups"": ""Keyboard&Mouse"" },
                { ""name"": ""left"",      ""id"": ""b1b2c3d4-0001-0001-0004-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/a"",           ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true,  ""groups"": ""Keyboard&Mouse"" },
                { ""name"": ""right"",     ""id"": ""b1b2c3d4-0001-0001-0005-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/d"",           ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true,  ""groups"": ""Keyboard&Mouse"" },
                { ""name"": ""Arrow Keys"",""id"": ""b1b2c3d4-0001-0002-0001-aaaaaaaaaaaa"", ""path"": ""2DVector"", ""action"": ""Move"", ""isComposite"": true,  ""isPartOfComposite"": false, ""groups"": """" },
                { ""name"": ""up"",        ""id"": ""b1b2c3d4-0001-0002-0002-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/upArrow"",     ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true,  ""groups"": ""Keyboard&Mouse"" },
                { ""name"": ""down"",      ""id"": ""b1b2c3d4-0001-0002-0003-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/downArrow"",   ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true,  ""groups"": ""Keyboard&Mouse"" },
                { ""name"": ""left"",      ""id"": ""b1b2c3d4-0001-0002-0004-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/leftArrow"",   ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true,  ""groups"": ""Keyboard&Mouse"" },
                { ""name"": ""right"",     ""id"": ""b1b2c3d4-0001-0002-0005-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/rightArrow"",  ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true,  ""groups"": ""Keyboard&Mouse"" },
                { ""name"": """",          ""id"": ""b1b2c3d4-0003-0001-0001-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/space"",       ""action"": ""Jump"",          ""isComposite"": false, ""isPartOfComposite"": false, ""groups"": ""Keyboard&Mouse"" },
                { ""name"": """",          ""id"": ""b1b2c3d4-0004-0001-0001-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/leftShift"",   ""action"": ""Sprint"",        ""isComposite"": false, ""isPartOfComposite"": false, ""groups"": ""Keyboard&Mouse"" },
                { ""name"": """",          ""id"": ""b1b2c3d4-0005-0001-0001-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/leftCtrl"",    ""action"": ""Roll"",          ""isComposite"": false, ""isPartOfComposite"": false, ""groups"": ""Keyboard&Mouse"" },
                { ""name"": """",          ""id"": ""b1b2c3d4-0006-0001-0001-aaaaaaaaaaaa"", ""path"": ""<Mouse>/leftButton"",     ""action"": ""AttackPrimary"", ""isComposite"": false, ""isPartOfComposite"": false, ""groups"": ""Keyboard&Mouse"" },
                { ""name"": """",          ""id"": ""b1b2c3d4-0007-0001-0001-aaaaaaaaaaaa"", ""path"": ""<Mouse>/rightButton"",    ""action"": ""BlockHold"",     ""isComposite"": false, ""isPartOfComposite"": false, ""groups"": ""Keyboard&Mouse"" },
                { ""name"": """",          ""id"": ""b1b2c3d4-0008-0001-0001-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/e"",           ""action"": ""Death"",         ""isComposite"": false, ""isPartOfComposite"": false, ""groups"": ""Keyboard&Mouse"" },
                { ""name"": """",          ""id"": ""b1b2c3d4-0009-0001-0001-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/q"",           ""action"": ""Hurt"",          ""isComposite"": false, ""isPartOfComposite"": false, ""groups"": ""Keyboard&Mouse"" },
                { ""name"": """",          ""id"": ""b1b2c3d4-0010-0001-0001-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/f"",           ""action"": ""Interact"",      ""isComposite"": false, ""isPartOfComposite"": false, ""groups"": ""Keyboard&Mouse"" },
                { ""name"": """",          ""id"": ""b1b2c3d4-0011-0001-0001-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/w"",           ""action"": ""GrabRope"",      ""isComposite"": false, ""isPartOfComposite"": false, ""groups"": ""Keyboard&Mouse"" },
                { ""name"": """",          ""id"": ""b1b2c3d4-0011-0002-0001-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/upArrow"",     ""action"": ""GrabRope"",      ""isComposite"": false, ""isPartOfComposite"": false, ""groups"": ""Keyboard&Mouse"" }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard&Mouse"",
            ""bindingGroup"": ""Keyboard&Mouse"",
            ""devices"": [
                { ""devicePath"": ""<Keyboard>"", ""isOptional"": false, ""isOR"": false },
                { ""devicePath"": ""<Mouse>"",    ""isOptional"": false, ""isOR"": false }
            ]
        }
    ]
}");

        // Cache action map
        m_Gameplay = asset.FindActionMap("Gameplay", throwIfNotFound: true);

        // Cache individual actions
        m_Gameplay_Move          = m_Gameplay.FindAction("Move",          throwIfNotFound: true);
        m_Gameplay_Jump          = m_Gameplay.FindAction("Jump",          throwIfNotFound: true);
        m_Gameplay_Sprint        = m_Gameplay.FindAction("Sprint",        throwIfNotFound: true);
        m_Gameplay_Roll          = m_Gameplay.FindAction("Roll",          throwIfNotFound: true);
        m_Gameplay_AttackPrimary = m_Gameplay.FindAction("AttackPrimary", throwIfNotFound: true);
        m_Gameplay_BlockHold     = m_Gameplay.FindAction("BlockHold",     throwIfNotFound: true);
        m_Gameplay_Death         = m_Gameplay.FindAction("Death",         throwIfNotFound: true);
        m_Gameplay_Hurt          = m_Gameplay.FindAction("Hurt",          throwIfNotFound: true);
        m_Gameplay_Interact      = m_Gameplay.FindAction("Interact",      throwIfNotFound: true);
        m_Gameplay_GrabRope      = m_Gameplay.FindAction("GrabRope",      throwIfNotFound: true);
    }

    ~PlayerControls() { UnityEngine.Object.Destroy(asset); }

    // -------------------------------------------------------------------------
    // IInputActionCollection2 boilerplate
    // -------------------------------------------------------------------------
    public InputBinding? bindingMask { get => asset.bindingMask; set => asset.bindingMask = value; }
    public ReadOnlyArray<InputDevice>? devices { get => asset.devices; set => asset.devices = value; }
    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;
    public bool Contains(InputAction action) => asset.Contains(action);
    public IEnumerator<InputAction> GetEnumerator() => asset.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Enable()  => asset.Enable();
    public void Disable() => asset.Disable();

    public IEnumerable<InputBinding> bindings => asset.bindings;
    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        => asset.FindAction(actionNameOrId, throwIfNotFound);
    public int FindBinding(InputBinding bindingMask, out InputAction action)
        => asset.FindBinding(bindingMask, out action);

    public void Dispose() { UnityEngine.Object.Destroy(asset); }

    // -------------------------------------------------------------------------
    // Gameplay action map
    // -------------------------------------------------------------------------
    private readonly InputActionMap  m_Gameplay;
    private readonly InputAction     m_Gameplay_Move;
    private readonly InputAction     m_Gameplay_Jump;
    private readonly InputAction     m_Gameplay_Sprint;
    private readonly InputAction     m_Gameplay_Roll;
    private readonly InputAction     m_Gameplay_AttackPrimary;
    private readonly InputAction     m_Gameplay_BlockHold;
    private readonly InputAction     m_Gameplay_Death;
    private readonly InputAction     m_Gameplay_Hurt;
    private readonly InputAction     m_Gameplay_Interact;
    private readonly InputAction     m_Gameplay_GrabRope;

    private IGameplayActions m_GameplayActionsCallbackInterface;

    /// <summary>Provides typed access to the Gameplay action map and its callbacks.</summary>
    public GameplayActions Gameplay => new GameplayActions(this);

    public void SetCallbacks(IGameplayActions instance)
    {
        if (m_GameplayActionsCallbackInterface != null)
        {
            Gameplay.SetCallbacks(null);
        }
        m_GameplayActionsCallbackInterface = instance;
        if (instance != null)
        {
            Gameplay.SetCallbacks(instance);
        }
    }

    // -------------------------------------------------------------------------
    // Struct wrapping the Gameplay action map for clean callback wiring
    // -------------------------------------------------------------------------
    public struct GameplayActions
    {
        private readonly PlayerControls m_Wrapper;
        public GameplayActions(PlayerControls wrapper) { m_Wrapper = wrapper; }

        public InputAction Move          => m_Wrapper.m_Gameplay_Move;
        public InputAction Jump          => m_Wrapper.m_Gameplay_Jump;
        public InputAction Sprint        => m_Wrapper.m_Gameplay_Sprint;
        public InputAction Roll          => m_Wrapper.m_Gameplay_Roll;
        public InputAction AttackPrimary => m_Wrapper.m_Gameplay_AttackPrimary;
        public InputAction BlockHold     => m_Wrapper.m_Gameplay_BlockHold;
        public InputAction Death         => m_Wrapper.m_Gameplay_Death;
        public InputAction Hurt          => m_Wrapper.m_Gameplay_Hurt;
        public InputAction Interact      => m_Wrapper.m_Gameplay_Interact;
        public InputAction GrabRope      => m_Wrapper.m_Gameplay_GrabRope;

        public InputActionMap Get() => m_Wrapper.m_Gameplay;

        public void Enable()  => Get().Enable();
        public void Disable() => Get().Disable();
        public bool enabled   => Get().enabled;

        public InputActionMap Clone() => Get().Clone();

        public static implicit operator InputActionMap(GameplayActions set) => set.Get();

        public void SetCallbacks(IGameplayActions instance)
        {
            // Move
            m_Wrapper.m_Gameplay_Move.started   -= instance.OnMove;
            m_Wrapper.m_Gameplay_Move.performed -= instance.OnMove;
            m_Wrapper.m_Gameplay_Move.canceled  -= instance.OnMove;

            // Jump
            m_Wrapper.m_Gameplay_Jump.started   -= instance.OnJump;
            m_Wrapper.m_Gameplay_Jump.performed -= instance.OnJump;
            m_Wrapper.m_Gameplay_Jump.canceled  -= instance.OnJump;

            // Sprint
            m_Wrapper.m_Gameplay_Sprint.started   -= instance.OnSprint;
            m_Wrapper.m_Gameplay_Sprint.performed -= instance.OnSprint;
            m_Wrapper.m_Gameplay_Sprint.canceled  -= instance.OnSprint;

            // Roll
            m_Wrapper.m_Gameplay_Roll.started   -= instance.OnRoll;
            m_Wrapper.m_Gameplay_Roll.performed -= instance.OnRoll;
            m_Wrapper.m_Gameplay_Roll.canceled  -= instance.OnRoll;

            // AttackPrimary
            m_Wrapper.m_Gameplay_AttackPrimary.started   -= instance.OnAttackPrimary;
            m_Wrapper.m_Gameplay_AttackPrimary.performed -= instance.OnAttackPrimary;
            m_Wrapper.m_Gameplay_AttackPrimary.canceled  -= instance.OnAttackPrimary;

            // BlockHold
            m_Wrapper.m_Gameplay_BlockHold.started   -= instance.OnBlockHold;
            m_Wrapper.m_Gameplay_BlockHold.performed -= instance.OnBlockHold;
            m_Wrapper.m_Gameplay_BlockHold.canceled  -= instance.OnBlockHold;

            // Death
            m_Wrapper.m_Gameplay_Death.started   -= instance.OnDeath;
            m_Wrapper.m_Gameplay_Death.performed -= instance.OnDeath;
            m_Wrapper.m_Gameplay_Death.canceled  -= instance.OnDeath;

            // Hurt
            m_Wrapper.m_Gameplay_Hurt.started   -= instance.OnHurt;
            m_Wrapper.m_Gameplay_Hurt.performed -= instance.OnHurt;
            m_Wrapper.m_Gameplay_Hurt.canceled  -= instance.OnHurt;

            // Interact
            m_Wrapper.m_Gameplay_Interact.started   -= instance.OnInteract;
            m_Wrapper.m_Gameplay_Interact.performed -= instance.OnInteract;
            m_Wrapper.m_Gameplay_Interact.canceled  -= instance.OnInteract;

            // GrabRope
            m_Wrapper.m_Gameplay_GrabRope.started   -= instance.OnGrabRope;
            m_Wrapper.m_Gameplay_GrabRope.performed -= instance.OnGrabRope;
            m_Wrapper.m_Gameplay_GrabRope.canceled  -= instance.OnGrabRope;

            if (instance == null) return;

            // Re-subscribe
            m_Wrapper.m_Gameplay_Move.started   += instance.OnMove;
            m_Wrapper.m_Gameplay_Move.performed += instance.OnMove;
            m_Wrapper.m_Gameplay_Move.canceled  += instance.OnMove;

            m_Wrapper.m_Gameplay_Jump.started   += instance.OnJump;
            m_Wrapper.m_Gameplay_Jump.performed += instance.OnJump;
            m_Wrapper.m_Gameplay_Jump.canceled  += instance.OnJump;

            m_Wrapper.m_Gameplay_Sprint.started   += instance.OnSprint;
            m_Wrapper.m_Gameplay_Sprint.performed += instance.OnSprint;
            m_Wrapper.m_Gameplay_Sprint.canceled  += instance.OnSprint;

            m_Wrapper.m_Gameplay_Roll.started   += instance.OnRoll;
            m_Wrapper.m_Gameplay_Roll.performed += instance.OnRoll;
            m_Wrapper.m_Gameplay_Roll.canceled  += instance.OnRoll;

            m_Wrapper.m_Gameplay_AttackPrimary.started   += instance.OnAttackPrimary;
            m_Wrapper.m_Gameplay_AttackPrimary.performed += instance.OnAttackPrimary;
            m_Wrapper.m_Gameplay_AttackPrimary.canceled  += instance.OnAttackPrimary;

            m_Wrapper.m_Gameplay_BlockHold.started   += instance.OnBlockHold;
            m_Wrapper.m_Gameplay_BlockHold.performed += instance.OnBlockHold;
            m_Wrapper.m_Gameplay_BlockHold.canceled  += instance.OnBlockHold;

            m_Wrapper.m_Gameplay_Death.started   += instance.OnDeath;
            m_Wrapper.m_Gameplay_Death.performed += instance.OnDeath;
            m_Wrapper.m_Gameplay_Death.canceled  += instance.OnDeath;

            m_Wrapper.m_Gameplay_Hurt.started   += instance.OnHurt;
            m_Wrapper.m_Gameplay_Hurt.performed += instance.OnHurt;
            m_Wrapper.m_Gameplay_Hurt.canceled  += instance.OnHurt;

            m_Wrapper.m_Gameplay_Interact.started   += instance.OnInteract;
            m_Wrapper.m_Gameplay_Interact.performed += instance.OnInteract;
            m_Wrapper.m_Gameplay_Interact.canceled  += instance.OnInteract;

            m_Wrapper.m_Gameplay_GrabRope.started   += instance.OnGrabRope;
            m_Wrapper.m_Gameplay_GrabRope.performed += instance.OnGrabRope;
            m_Wrapper.m_Gameplay_GrabRope.canceled  += instance.OnGrabRope;
        }
    }

    // -------------------------------------------------------------------------
    // Callback interface — implement this on any MonoBehaviour that needs input
    // -------------------------------------------------------------------------
    public interface IGameplayActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
        void OnRoll(InputAction.CallbackContext context);
        void OnAttackPrimary(InputAction.CallbackContext context);
        void OnBlockHold(InputAction.CallbackContext context);
        void OnDeath(InputAction.CallbackContext context);
        void OnHurt(InputAction.CallbackContext context);
        void OnInteract(InputAction.CallbackContext context);
        void OnGrabRope(InputAction.CallbackContext context);
    }
}
