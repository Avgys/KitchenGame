using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.InputAction;

public class GameManager : NetworkBehaviour
{
    public enum State
    {
        //WaitingForServer,
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
        Pause
    }

    public class Timer
    {
        public float currentTime;
        public float maxTime;

        public Timer(float maxTimer)
        {
            currentTime = maxTimer;
            maxTime = maxTimer;
        }

        public void Restart() => currentTime = maxTime;
        public void DecreaseTimer(float time) => currentTime -= time;
        public float Percentage => currentTime / maxTime;
    }

    public static GameManager Singleton { get; private set; }

    [SerializeField] private Player playerPrefab;

    private Dictionary<State, Timer> timers;
    public float StartGameTimer => timers[State.CountdownToStart].currentTime;
    public Timer GameTimer => timers[State.GamePlaying];
    [SerializeField] private float RoundTime = 120f;
    [SerializeField] private float CountDownTime = 3f;

    [SerializeField] private NetworkVariable<State> currentState = new();
    private State previousState;

    public static event Action<State> StateChanged;
    public event Action<bool> PauseToggled;


    private void Awake()
    {
        Singleton = this;
        timers = new()
        {
            { State.CountdownToStart, new Timer(CountDownTime) },
            { State.GamePlaying, new Timer(RoundTime) }
        };

        ResetTimers(); 
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
    }

    private void Start()
    {
        StateChanged?.Invoke(currentState.Value);

        currentState.OnValueChanged += OnStateUpdate;
        currentState.OnValueChanged += ShowPausePauseMenuRpc;
        GameInput.Instance.Pause += (CallbackContext t) => ToggleGamePauseRpc();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {            
            WaitingForPlayers.Singleton.OnAllPlayersReady += Singleton_OnAllPlayersReady;
            NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
        }
    }

    private void OnStateUpdate(State previousValue, State newValue)
    {
        previousState = previousValue;
        StateChanged?.Invoke(newValue);
    }

    private void Singleton_OnAllPlayersReady()
    {
        currentState.Value = State.CountdownToStart;
        WaitingForPlayers.Singleton.OnAllPlayersReady -= Singleton_OnAllPlayersReady;
    }

    private void Singleton_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId != OwnerClientId)
            return;

        Debug.Log($"Client with {clientId} disconnected");
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        SpawnPlayers(clientsCompleted);
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
    }

    private void SpawnPlayers(List<ulong> connectedClients)
    {
        if (!IsServer)
            return;

        foreach (var clientId in connectedClients)
        {
            var player = Instantiate(playerPrefab);
            player.NetworkObject.SpawnAsPlayerObject(clientId, true);
        }
    }

    [Rpc(SendTo.Server)]
    public void ToggleGamePauseRpc()
    {
        currentState.Value = currentState.Value == State.Pause ? previousState : State.Pause;
    }

    public void ShowPausePauseMenuRpc(State previousValue, State newValue)
    {
        var isPaused = newValue == State.Pause;
        Time.timeScale = isPaused ? 0f : 1f;
        PauseToggled?.Invoke(isPaused);
    }

    private void ResetTimers()
    {
        foreach (var timer in timers.Values)
            timer.Restart();
    }

    private void Update()
    {
        HandleTimers();
        HandleState();
    }

    private void HandleTimers()
    {
        if (timers.ContainsKey(currentState.Value))
            timers[currentState.Value].DecreaseTimer(Time.deltaTime);
    }

    private void HandleState()
    {
        if (!IsServer)
            return;

        if (timers.ContainsKey(currentState.Value) && timers[currentState.Value].currentTime < 0f)
            currentState.Value = currentState.Value switch
            {
                State.CountdownToStart => State.GamePlaying,
                State.GamePlaying => State.GameOver,
            };
    }

    public override void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;

        StateChanged = null;
        base.OnDestroy();
    }

    internal static void ResetStatic()
    {
        StateChanged = null;
    }
}