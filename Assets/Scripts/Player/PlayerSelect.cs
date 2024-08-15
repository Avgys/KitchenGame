using System;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerSelect : NetworkBehaviour
{
    private const int freeIndex = -1;
    private readonly NetworkVariable<int> playerIndex = new(freeIndex);

    [SerializeField] private int sortOrder;

    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private Button kickButton;
    [SerializeField] private TextMeshProUGUI playerName;

    private static PlayerSelect[] allSelects = new PlayerSelect[MultiplayerManager.MAX_PLAYERS];
    private bool IsAvailable => playerIndex.Value == freeIndex;

    public UnityEvent<bool> isReady;

    public static PlayerSelect? GetAvailablePlayerSelect()
    {
        for (int i = 0; i < allSelects.Length; i++)
            if (allSelects[i] != null && allSelects[i].IsAvailable)
            {
                return allSelects[i];
            }

        return null;
    }

    private void Free()
    {
        NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
        playerIndex.Value = freeIndex;
        gameObject.SetActive(false);
    }

    //private static void CheckForNotSelectedPlayers()
    //{
    //    var usedSelects = allSelects.Where(x => !x.IsAvailable).Count();

    //    while (usedSelects < NetworkData.Singleton.PlayerCount
    //        && NetworkData.Singleton.PlayerCount <= allSelects.Length)
    //    {
    //        var freeSelect = GetAvailablePlayerSelect();

    //        if (freeSelect == null)
    //            break;

    //        foreach (var clientData in NetworkData.Singleton.playersList)
    //        {
    //            if (allSelects.Any(x => x.playerIndex.Value == clientData.PlayerIndex))
    //                continue;

    //            freeSelect.SetPlayerData(clientData);
    //            usedSelects++;
    //            break;
    //        }
    //    }
    //}

    private static void CheckForNotSelectedPlayers()
    {
        foreach (var clientData in NetworkData.Singleton.playersList)
            OnNewPlayerAdded(new NetworkListEvent<PlayerData> { Value = clientData, Type = NetworkListEvent<PlayerData>.EventType.Add });
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playerIndex.OnValueChanged += OnPlayerIdChanged;
        WaitingForPlayers.Singleton.OnReadyChange += Singleton_OnReadyChange;
        NetworkData.Singleton.ListChanged += OnPlayerDataListChanged;
        gameObject.SetActive(false);

        AddSelfToArray();
        PrepareButtons();

        if (IsServer)
            LastInitialization();
        else if (IsClient)
            LateJoinSync();
    }

    private void OnPlayerIdChanged(int previousValue, int newValue)
    {
        gameObject.SetActive(newValue != freeIndex);

        if (newValue == freeIndex)
            return;

        var playerData = NetworkData.Singleton.GetPlayerDataByIndex(newValue).Value;
        UpdateValue(playerData);
    }

    private void LateJoinSync()
    {
        gameObject.SetActive(playerIndex.Value != freeIndex);

        if (playerIndex.Value != freeIndex)
            UpdateValue(NetworkData.Singleton.GetPlayerDataClientId(NetworkObject.OwnerClientId).Value);
    }

    private void LastInitialization()
    {
        if (!allSelects.Any(x => x == null))
        {
            NetworkData.Singleton.ListChanged += OnNewPlayerAdded;
            Array.Sort(allSelects.Select(x => x.sortOrder).ToArray(), allSelects);

            CheckForNotSelectedPlayers();
        }
    }

    private void PrepareButtons()
    {
        if (IsServer)
        {
            kickButton.onClick.AddListener(() => Kick());
            kickButton.gameObject.SetActive(false);
        }
        else
            Destroy(kickButton.gameObject);

        isReady?.Invoke(false);
    }

    private void AddSelfToArray()
    {
        for (int i = 0; i < allSelects.Length; i++)
            if (allSelects[i] == null)
            {
                allSelects[i] = this;
                break;
            }
    }

    private void Singleton_OnReadyChange(ulong clientId, bool isReady)
    {
        var playerData = NetworkData.Singleton.GetPlayerDataByIndex(playerIndex.Value);
        if (playerData.HasValue && playerData.Value.ClientId == clientId)
            this.isReady?.Invoke(isReady);
    }

    private void SetPlayerData(PlayerData clientData)
    {
        if (!IsServer)
            throw new NotServerException();

        if (clientData.ClientId != NetworkObject.OwnerClientId)
            NetworkObject.ChangeOwnership(clientData.ClientId);

        kickButton.gameObject.SetActive(clientData.ClientId != NetworkManager.ServerClientId);

        playerIndex.Value = clientData.PlayerIndex;
    }

    private void Kick()
    {
        if (OwnerClientId != NetworkManager.ServerClientId)
            MultiplayerManager.Singleton.KickPlayer(playerIndex.Value);
    }

    private static void OnNewPlayerAdded(NetworkListEvent<PlayerData> changeEvent)
    {
        if (changeEvent.Type == NetworkListEvent<PlayerData>.EventType.Insert || changeEvent.Type == NetworkListEvent<PlayerData>.EventType.Add)
        {
            var freeSelect = GetAvailablePlayerSelect();

            if (freeSelect != null)
                freeSelect.SetPlayerData(changeEvent.Value);
        }
    }

    private void OnPlayerDataListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        if (playerIndex.Value == changeEvent.Value.PlayerIndex)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<PlayerData>.EventType.Add:
                case NetworkListEvent<PlayerData>.EventType.Insert:
                case NetworkListEvent<PlayerData>.EventType.Value:
                    UpdateValue(changeEvent.Value);
                    break;
                case NetworkListEvent<PlayerData>.EventType.Remove:
                case NetworkListEvent<PlayerData>.EventType.RemoveAt:
                case NetworkListEvent<PlayerData>.EventType.Clear:
                    if (IsServer)
                        Free();
                    break;
                default:
                    break;
            }
        }
    }

    private void UpdateValue(PlayerData value)
    {
        if (playerName.text != value.PlayerName)
            playerName.text = value.PlayerName.ToString();
    }

    public override void OnDestroy()
    {
        if (NetworkData.Singleton != null)
        {
            NetworkData.Singleton.ListChanged -= OnPlayerDataListChanged;
            NetworkData.Singleton.ListChanged -= OnNewPlayerAdded;
        }

        if (WaitingForPlayers.Singleton != null)
            WaitingForPlayers.Singleton.OnReadyChange -= Singleton_OnReadyChange;

        base.OnDestroy();
    }
}
