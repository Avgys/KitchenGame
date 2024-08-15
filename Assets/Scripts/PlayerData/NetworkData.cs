using System;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;

public class NetworkData : NetworkBehaviour
{
    public NetworkList<PlayerData> playersList;
    public int PlayerCount => playersList.Count;

    public event NetworkList<PlayerData>.OnListChangedDelegate ListChanged
    {
        add { playersList.OnListChanged += value; }
        remove { playersList.OnListChanged -= value; }
    }

    public static NetworkData Singleton { get; private set; }
    public static Action ActionBuffer { get; internal set; }

    public static event Action OnSpawned;
    public PlayerColors PlayerColors;

    private void Awake()
    {
        Singleton = this;
        playersList = new NetworkList<PlayerData>();

        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        LocalPlayerData.PlayerDataChanged += InformServer;

        //if (IsServer)
        //    RecheckForPlayers();
    }


    //private void RecheckForPlayers()
    //{
    //    if (NetworkManager.ConnectedClientsIds.Count > playersList.Count)
    //        foreach (var id in NetworkManager.ConnectedClientsIds)
    //            if (!GetPlayerDataClientId(id).HasValue)
    //                AddSelectionCharacter(id);
    //}

    public void AddSelectionCharacter(ulong clientId)
    {
        if (!IsServer)
            throw new NotServerException();

        playersList.Add(new PlayerData { ClientId = clientId, PlayerIndex = playersList.Count });

        AskPersonalDataRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void AskPersonalDataRpc(RpcParams rpc)
    {
        LocalPlayerData.localData.ClientId = NetworkManager.LocalClientId;
        InformServer(LocalPlayerData.localData);
    }

    private void InformServer(PlayerData data) => SendToServerPersonalDataRpc(data);

    [Rpc(SendTo.Server)]
    private void SendToServerPersonalDataRpc(PlayerData clientLocalData, RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId == clientLocalData.ClientId && NetworkManager.ConnectedClientsIds.Contains(rpcParams.Receive.SenderClientId))
        {
            var index = GetPlayerIndex(rpcParams.Receive.SenderClientId);
            var playerServerData = playersList[index];
            clientLocalData.PlayerIndex = playerServerData.PlayerIndex;
            clientLocalData.ClientId = rpcParams.Receive.SenderClientId;
            playersList[index] = clientLocalData;
        }
    }

    public PlayerData? RemoveSelectionCharacter(ulong cliendId)
    {
        if (!IsServer)
            throw new NotServerException();

        var playerData = GetPlayerDataClientId(cliendId);
        if (playerData.HasValue)
            playersList.Remove(playerData.Value);
        return playerData;
    }

    public PlayerData? GetPlayerDataClientId(ulong clientId)
    {
        var index = GetPlayerIndex(clientId);

        return index != -1 ? playersList[index] : null;
    }

    public PlayerData? GetPlayerDataByIndex(int index)
    {
        return index >= 0 && index < playersList.Count ? playersList[index] : null;
    }

    public int GetPlayerIndex(ulong clientId)
    {
        int index = -1;
        for (var i = 0; i < playersList.Count; i++)
        {
            if (playersList[i].ClientId == clientId)
            {
                index = i;
                break;
            }
        }

        return index;
    }

    public override void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;

        LocalPlayerData.PlayerDataChanged -= InformServer;

        base.OnDestroy();
    }
}
