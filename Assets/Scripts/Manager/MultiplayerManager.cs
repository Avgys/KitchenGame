using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerManager : MonoBehaviour
{
    public int MaxPlayerCount { get; private set; } = 4;

    public static MultiplayerManager Singleton { get; private set; }

    public event Action<string> OnConnectionFailed;

    [SerializeField] private NetworkManager networkManagerPrefab;
    [SerializeField] private NetworkData networkDataPrefab;

    private MultiplayerLobby networkLobby;
    private NetworkManager networkManager;
    private NetworkData networkData;
    public const int MAX_PLAYERS = 4;

    private Action OnInitialized;
    private bool IsInitialized;
    public bool IsSolo { get; private set; }

    public int PlayerCount => networkData?.PlayerCount
                                ?? networkManager?.ConnectedClientsList.Count
                                ?? 1;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        SceneLoader.OnSceneLoaded += SceneLoader_OnSceneLoaded;

        Singleton = this;

        IsInitialized = false;

        OnConnectionFailed += Debug.Log;
        OnConnectionFailed += PopupMessages.ShowMessage;
    }

    async void Start()
    {
        await PrepareNetworkLobby();
    }

    internal void StartSolo()
    {
        PrepareNetworkManager();
        PreparePlayerData();

        PrepareSolo();
        Subscribe(networkManager);

        networkManager.StartHost();
        InvokeInitialized();
        IsSolo = true;
    }

    private void PrepareSolo()
    {
        var unityTransport = networkManager.GetComponent<UnityTransport>();
        unityTransport.SetConnectionData("127.0.0.1", 9999);
        unityTransport.UseEncryption = false;
    }

    public async Task<bool> CreateLobbyAsync(string name = "My lobby", bool isPrivate = false)
    {
        await PrepareNetworkLobby();
        PrepareNetworkManager();

        if (!await networkLobby.CreateLobby(name, isPrivate))
            return false;

        Subscribe(networkManager);
        networkManager.StartHost();
        PreparePlayerData();

        InvokeInitialized();
        IsSolo = false;
        return true;
    }

    public async Task JoinAsync(string lobbyCode = null, string lobbyId = null)
    {
        await PrepareNetworkLobby();
        PrepareNetworkManager();

        var isConnected = await (lobbyCode != null
            ? networkLobby.JoinByCode(lobbyCode)
            : lobbyId != null
                ? networkLobby.JoinById(lobbyId)
                : networkLobby.QuickJoin());

        if (!isConnected)
            return;

        networkManager.OnClientDisconnectCallback += Singleton_OnClientConnectionFailed;

        networkManager.StartClient();

        InvokeInitialized();
        IsSolo = false;
    }

    void InvokeInitialized()
    {
        IsInitialized = true;
        OnInitialized?.Invoke();
        OnInitialized = null;
    }

    private async Task PrepareNetworkLobby()
    {
        if (networkLobby != null)
            return;

        networkLobby = networkLobby != null
        ? networkLobby
        : MultiplayerLobby.Singleton != null
            ? MultiplayerLobby.Singleton
            : new GameObject(nameof(MultiplayerLobby)).AddComponent<MultiplayerLobby>();

        void LinkEvent(string str) => OnConnectionFailed?.Invoke(str);

        networkLobby.LobbyFailed += LinkEvent;

        await networkLobby.InitalizeServiceAuthentication();
    }

    private void PreparePlayerData()
    {
        networkData = networkData != null
        ? networkData
        : NetworkData.Singleton != null
            ? NetworkData.Singleton
            : Instantiate(networkDataPrefab);

        if (networkManager != null && networkManager.IsServer && !networkData.IsSpawned)
            networkData.NetworkObject.Spawn();
    }

    void PrepareNetworkManager()
    {
        if (networkManager == null)
            networkManager = Instantiate(networkManagerPrefab);
    }

    void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        bool isApproved = true;

        if (networkManager.IsServer)
        {
            response.Approved = isApproved;
            return;
        }

        if (SceneManager.GetActiveScene().name != SceneLoader.Scene.CharacterSelectScene.ToString())
        {
            isApproved = false;
            response.Reason = "Game not in lobby scene";
        }

        if (MaxPlayerCount <= networkManager.ConnectedClientsIds.Count)
        {
            isApproved = false;
            response.Reason = "Game not in lobby scene";
        }

        response.Approved = isApproved;
    }

    private void Subscribe(NetworkManager networkManager)
    {
        networkManager.ConnectionApprovalCallback += ConnectionApprovalCallback;
        networkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        networkManager.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallbackAsync;
        networkManager.OnTransportFailure += NetworkManager_OnTransportFailure;
    }

    private void Unsubscribe(NetworkManager networkManager)
    {
        networkManager.ConnectionApprovalCallback -= ConnectionApprovalCallback;
        networkManager.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
        networkManager.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallbackAsync;
        networkManager.OnTransportFailure -= NetworkManager_OnTransportFailure;
    }

    private async void NetworkManager_OnTransportFailure()
    {
        await Shutdown();
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        if (!IsInitialized)
        {
            OnInitialized += () => NetworkManager_OnClientConnectedCallback(clientId);
            return;
        }

        networkData.AddSelectionCharacter(clientId);
    }

    private void NetworkManager_OnClientDisconnectCallbackAsync(ulong clientId)
    {
        var playerData = networkData.GetPlayerDataClientId(clientId);

        if (playerData.HasValue)
            ClearPlayer(playerData.Value);
    }

    private void Singleton_OnClientConnectionFailed(ulong clientId)
    {
        OnConnectionFailed?.Invoke(networkManager.DisconnectReason);
        networkManager.OnClientDisconnectCallback -= Singleton_OnClientConnectionFailed;
    }

    void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;

        if (networkManager != null)
        {
            Unsubscribe(networkManager);
            Destroy(networkManager.gameObject);
        }
    }

    internal async Task Shutdown()
    {
        await networkLobby.LeaveLobby();

        if (networkManager != null)
        {
            Unsubscribe(networkManager);
            networkData?.NetworkObject?.Despawn();
            networkManager.Shutdown();
        }

        IsInitialized = false;
    }

    private void SceneLoader_OnSceneLoaded(SceneLoader.Scene scene)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            switch (scene)
            {
                case SceneLoader.Scene.CharacterSelectScene:
                    WaitingForPlayers.Singleton.OnAllPlayersReady += LoadGameScene;
                    break;
            }
        else if (IsSolo)
            switch (scene)
            {
                case SceneLoader.Scene.CharacterSelectScene:
                    WaitingForPlayers.Singleton.OnAllPlayersReady += LoadGameSceneSolo;
                    break;
            }
    }

    private void LoadGameSceneSolo()
    {
        SceneLoader.Load(SceneLoader.Scene.GameScene, true, false);
    }

    private async void LoadGameScene()
    {
        await networkLobby.DeleteLobby();
        SceneLoader.Load(SceneLoader.Scene.GameScene, true, true);
    }

    internal async void KickPlayer(int clientIndex)
    {
        var playerData = networkData.GetPlayerDataByIndex(clientIndex);

        if (networkManager.ConnectedClientsIds.Contains(playerData.Value.ClientId))
            networkManager.DisconnectClient(playerData.Value.ClientId, "You've been kicked out");
    }

    private async Task ClearPlayer(PlayerData player)
    {
        networkData.RemoveSelectionCharacter(player.ClientId);
        await networkLobby.KickPlayer(player.AuthPlayerId.ToString());
    }
}
