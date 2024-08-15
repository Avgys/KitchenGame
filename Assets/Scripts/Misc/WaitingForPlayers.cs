using System;
using System.Collections.Generic;
using Unity.Netcode;

public class WaitingForPlayers : NetworkBehaviour
{
    private HashSet<ulong> readyPlayers;

    public NetworkVariable<int> TotalCount = new(0);
    public NetworkVariable<int> ReadyPlayersCount = new(0);

    public event Action OnAllPlayersReady;

    public static WaitingForPlayers Singleton { get; private set; }
    private bool triggered;

    public event Action<ulong, bool> OnReadyChange;

    public void Awake()
    {
        if (Singleton != null)
            Destroy(gameObject);

        Singleton = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
            return;

        readyPlayers = new HashSet<ulong>();
        NetworkManager.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        NetworkData.Singleton.ListChanged += PlayerListChanged;
        CheckTotalCount();
    }

    private void PlayerListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<PlayerData>.EventType.Add:
            case NetworkListEvent<PlayerData>.EventType.Insert:
            case NetworkListEvent<PlayerData>.EventType.Remove:
            case NetworkListEvent<PlayerData>.EventType.RemoveAt:
            case NetworkListEvent<PlayerData>.EventType.Clear:
                CheckTotalCount();
                break;
        }
    }

    private void CheckTotalCount() => TotalCount.Value = MultiplayerManager.Singleton.PlayerCount;

    private void NetworkManager_OnClientDisconnectCallback(ulong disconnectedClientId)
    {
        if (triggered)
            return;

        ReadyPlayersCount.Value = 0;
        readyPlayers.Clear();

        foreach (var clientId in NetworkManager.ConnectedClientsIds)
        {
            if (disconnectedClientId == clientId)
                continue;

            InformReadyRpc(clientId, false);
        }
    }

    internal void ToggleReady(bool value)
    {
        if (NetworkManager.Singleton != null)
            ToggleReadyRpc(value);
    }

    [Rpc(SendTo.Server)]
    public void ToggleReadyRpc(bool isReady, RpcParams rpc = default)
    {
        if (triggered)
            return;

        var clientId = rpc.Receive.SenderClientId;

        if (isReady && !readyPlayers.Contains(clientId))
        {
            readyPlayers.Add(clientId);
            ReadyPlayersCount.Value++;
        }
        else if (!isReady && readyPlayers.Contains(clientId))
        {
            readyPlayers.Remove(clientId);
            ReadyPlayersCount.Value--;
        }
        else
            throw new ArgumentException("Desync in readiness");

        InformReadyRpc(clientId, isReady);

        CheckAllReady();
    }

    [Rpc(SendTo.Everyone)]
    private void InformReadyRpc(ulong clientId, bool isReady)
    {
        OnReadyChange?.Invoke(clientId, isReady);
    }

    void CheckAllReady()
    {
        foreach (var client in NetworkManager.ConnectedClientsIds)
            if (!readyPlayers.Contains(client))
                return;

        InvokeCallbacksRpc();
        triggered = true;
    }

    [Rpc(SendTo.Everyone)]
    private void InvokeCallbacksRpc()
    {
        OnAllPlayersReady?.Invoke();
    }

    public override void OnDestroy()
    {
        if (this == Singleton)
            Singleton = null;

        if (IsServer && NetworkManager != null)
            NetworkManager.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;

        if (NetworkData.Singleton != null)
            NetworkData.Singleton.ListChanged -= PlayerListChanged;

        base.OnDestroy();
    }
}
