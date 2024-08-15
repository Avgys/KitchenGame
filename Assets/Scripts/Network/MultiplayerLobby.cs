using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class MultiplayerLobby : MonoBehaviour
{
    public static MultiplayerLobby Singleton { get; private set; }

    private Lobby joinedLobby;

    public event Action<string> LobbyFailed;
    public event Action<Lobby> OnLobbyUpdate;
    public event Action<List<Lobby>> OnLobbyListChange;

    private bool IsLobbyHost => joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;

    private float pollingTimer;
    private float lobbySearchTimer;
    private float heartbeatTimer;
    private const string ALLOCATION_KEY = "AllocationKey";
    private const string CONNECTION_TYPE = "dtls";

    public async void Awake()
    {
        Singleton = this;
        DontDestroyOnLoad(gameObject);
        await InitalizeServiceAuthentication();
    }

    private void Update()
    {
        HandlePolling();
        //HandleUpdateLobbyList();
    }

    private async void HandleUpdateLobbyList()
    {
        if (joinedLobby == null && AuthenticationService.Instance.IsSignedIn)
        {
            lobbySearchTimer -= Time.deltaTime;
            if (lobbySearchTimer < 0f)
            {
                const float lobbySearchTimerMax = 2f;
                lobbySearchTimer = lobbySearchTimerMax;
                await GetLobbies();
            }
        }
    }

    private void FixedUpdate()
    {
        HandleHeartbeat();
    }

    public async Task InitalizeServiceAuthentication()
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
                return;

            var options = new InitializationOptions();
            options.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

            await UnityServices.InitializeAsync(options);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            LocalPlayerData.localData.AuthPlayerId = AuthenticationService.Instance.PlayerId;
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (ServicesInitializationException ex)
        {
            LobbyFailed(ex.Message);
        }
    }

    public async Task<bool> CreateLobby(string name, bool isPrivate)
    {
        try
        {
            await WaitForAuthentication();
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(
                name,
                MultiplayerManager.Singleton.MaxPlayerCount,
                new CreateLobbyOptions()
                {
                    IsPrivate = isPrivate
                });

            Allocation relayAllocation = await RelayService.Instance.CreateAllocationAsync(MultiplayerManager.MAX_PLAYERS - 1/*Minus host*/);
            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            unityTransport.SetRelayServerData(new RelayServerData(relayAllocation, CONNECTION_TYPE));
            unityTransport.UseEncryption = true;

            var relayCode = await RelayService.Instance.GetJoinCodeAsync(relayAllocation.AllocationId);

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions()
            {
                Data = new() { { ALLOCATION_KEY, new DataObject(DataObject.VisibilityOptions.Public, relayCode) } }
            });
        }
        catch (Exception ex)
        {
            LobbyFailed(ex.Message);
            return false;
        }

        return true;
    }

    async Task JoinToRelay(Lobby joinedLobby)
    {
        var joinCode = joinedLobby.Data[ALLOCATION_KEY].Value;
        var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        unityTransport.SetRelayServerData(new RelayServerData(allocation, CONNECTION_TYPE));
        unityTransport.UseEncryption = true;
    }

    private async void HandlePolling()
    {
        if (IsLobbyHost)
        {
            pollingTimer -= Time.deltaTime;
            if (pollingTimer < 0f)
            {
                const float pollingTimerMax = 2f;
                pollingTimer = pollingTimerMax;
                await UpdateLobbyInfo();
            }
        }
    }

    private void HandleHeartbeat()
    {
        if (IsLobbyHost)
        {
            heartbeatTimer -= Time.fixedDeltaTime;
            if (heartbeatTimer < 0)
            {
                const float heartbeatTimerMax = 25f;
                heartbeatTimer = heartbeatTimerMax;
                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private async Task UpdateLobbyInfo()
    {
        try
        {
            var t = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            joinedLobby = t;
            OnLobbyUpdate?.Invoke(joinedLobby);
            Debug.Log(string.Join(',', joinedLobby.Players.Select(x => x.Id)));
        }
        catch (Exception ex)
        {
            LobbyFailed?.Invoke(ex.Message);
        }

    }

    internal string GetLobbyName()
        => joinedLobby != null
                ? "Lobby name: " + joinedLobby.Name
                : !MultiplayerManager.Singleton.IsSolo
                    ? "[Not joined to lobby]"
                    : "Solo mode";

    internal string GetLobbyCode()
        => joinedLobby == null || string.IsNullOrEmpty(joinedLobby.LobbyCode) ? string.Empty : "Lobby code: " + joinedLobby.LobbyCode;

    internal async Task<bool> QuickJoin()
    {
        try
        {
            await WaitForAuthentication();
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            await JoinToRelay(joinedLobby);
        }
        catch (Exception ex)
        {
            LobbyFailed(ex.Message);
            return false;
        }

        return true;
    }

    internal async Task<bool> JoinByCode(string lobbyCode)
    {
        try
        {
            await WaitForAuthentication();
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            await JoinToRelay(joinedLobby);
        }
        catch (Exception ex)
        {
            LobbyFailed(ex.Message);
            return false;
        }
        return true;
    }

    internal async Task<bool> JoinById(string lobbyId)
    {
        try
        {
            await WaitForAuthentication();
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            await JoinToRelay(joinedLobby);
        }
        catch (Exception ex)
        {
            LobbyFailed(ex.Message);
            return false;
        }
        return true;
    }

    private async Task WaitForAuthentication()
    {
        while (AuthenticationService.Instance.PlayerId == null)
            await Task.Delay(100);
    }

    public async Task LeaveLobby()
    {
        if (joinedLobby == null)
            return;

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby = null;
        }
        catch (Exception ex)
        {
            LobbyFailed(ex.Message);
        }
    }

    public async Task KickPlayer(string playerId)
    {
        if (joinedLobby == null || !IsLobbyHost)
            return;

        await UpdateLobbyInfo();

        if (!joinedLobby.Players.Any(x => x.Id == playerId))
            return;

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            joinedLobby.Players.RemoveAll(x => x.Id == playerId);
        }
        catch (Exception ex)
        {
            LobbyFailed(ex.Message);
        }
    }

    public async Task DeleteLobby()
    {
        if (joinedLobby == null)
            return;

        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            joinedLobby = null;
        }
        catch (LobbyServiceException ex)
        {
            LobbyFailed(ex.Message);
        }
    }


    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }

    internal async Task GetLobbies()
    {
        try
        {
            var options = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>() {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            }
            };
            var response = await LobbyService.Instance.QueryLobbiesAsync(options);
            OnLobbyListChange?.Invoke(response.Results);
        }
        catch (LobbyServiceException ex)
        {
            LobbyFailed(ex.Message);
        }
    }
}
