using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using static UnityEngine.InputSystem.InputAction;

public class GameInput : MonoBehaviour
{
    private InputSystem_Actions _playerInput;
    public (Binding Binding, InputAction InputAction)[] Actions { get; private set; }

    private const string GAME_INPUT_KEY = "GAME_INPUT_KEY";

    public static GameInput Instance { get; private set; }

    public enum Binding
    {
        Move,
        Interact,
        AlternateInteract,
        Drop,
        Combine
    }

    private void Awake()
    {
        Instance = this;
        _playerInput = new InputSystem_Actions();
        
        Actions = new[] {
            (Binding.Move, _playerInput.Player.Move),
            (Binding.Interact,_playerInput.Player.Interact),
            (Binding.AlternateInteract,_playerInput.Player.AlternateInteract),
            (Binding.Drop,_playerInput.Player.Drop),
            (Binding.Drop,_playerInput.Player.Dropapart),
            (Binding.Drop,_playerInput.Player.Throw),
            (Binding.Combine,_playerInput.Player.Combine)
        };

        if(PlayerPrefs.HasKey(GAME_INPUT_KEY))
            _playerInput.LoadBindingOverridesFromJson(PlayerPrefs.GetString(GAME_INPUT_KEY));
        _playerInput.Enable();
    }

    void Start()
    {
        GameManager.StateChanged += GameMode_StateChanged;
    }

    private void OnDestroy()
    {
        _playerInput.Disable();
        _playerInput.Dispose();
        if (Instance == this)
            Instance = null;
    }

    private void GameMode_StateChanged(GameManager.State obj)
    {
        foreach (var action in Actions)
        {
            if (action.Binding == Binding.Interact)
                continue;

            if (obj == GameManager.State.GamePlaying)
                action.InputAction.Enable();
            else
                action.InputAction.Disable();
        }
    }

    public Vector3 MovementVectorNormalized
    {
        get
        {
            var vector2 = _playerInput.Player.Move.ReadValue<Vector2>().normalized;
            Vector3 movementVector = new(vector2.x, 0, vector2.y);
            return movementVector;
        }
    }

    public event Action<CallbackContext> InteractEvent
    {
        add => _playerInput.Player.Interact.performed += value;
        remove => _playerInput.Player.Interact.performed -= value;
    }

    public event Action<CallbackContext> DropEvent
    {
        add => _playerInput.Player.Drop.performed += value;
        remove => _playerInput.Player.Drop.performed -= value;
    }

    public event Action<CallbackContext> ThrowEvent
    {
        add => _playerInput.Player.Throw.performed += value;
        remove => _playerInput.Player.Throw.performed -= value;
    }

    public event Action<CallbackContext> DropPartEvent
    {
        add => _playerInput.Player.Dropapart.performed += value;
        remove => _playerInput.Player.Dropapart.performed -= value;
    }

    public event Action<CallbackContext> AlternateInteractEvent
    {
        add => _playerInput.Player.AlternateInteract.performed += value;
        remove => _playerInput.Player.AlternateInteract.performed -= value;
    }

    public event Action<CallbackContext> Combine
    {
        add => _playerInput.Player.Combine.performed += value;
        remove => _playerInput.Player.Combine.performed -= value;
    }

    public event Action<CallbackContext> Pause
    {
        add => _playerInput.Player.Pause.performed += value;
        remove => _playerInput.Player.Pause.performed -= value;
    }

    public bool SpeedBoost => _playerInput.Player.Sprint.IsPressed();


    public void SetBinding(Binding binding, int bindIndex, Action onCallbackRebound)
    {
        var item = Actions.First(x => x.Binding == binding).InputAction;
        _playerInput.Player.Disable();

        item.PerformInteractiveRebinding(bindIndex)
            .OnComplete(callback =>
            {
                Debug.Log(item.bindings[bindIndex].path);
                Debug.Log(item.bindings[bindIndex].overridePath);
                callback.Dispose();
                PlayerPrefs.SetString(GAME_INPUT_KEY, _playerInput.SaveBindingOverridesAsJson());
                _playerInput.Player.Enable();
                onCallbackRebound.Invoke();
            })
            .Start();
    }

    public IEnumerable<(Binding binding, string name, int bindings)> GetGameInputSettings()
    {
        return Actions.Where(x => x.Binding != Binding.Move).Select(x => (x.Binding, x.InputAction.name, x.InputAction.bindings.Count));
    }

    internal string GetBinding(Binding binding, int bindIndex = 0)
    {
        var action = Actions.First(x => x.Binding == binding).InputAction;
        var bind = action.bindings[bindIndex];
        return bind.ToDisplayString();
    }
}