using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyGridInfo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textName;
    [SerializeField] TextMeshProUGUI textPlayerCount;

    [SerializeField] Button joinButton;

    private Lobby Value;
    private void Start()
    {
        joinButton.onClick.AddListener(JoinLobby);
    }

    private async void JoinLobby()
    {
        await MultiplayerManager.Singleton.JoinAsync(lobbyId: Value.Id);
    }

    internal void SetValue(Lobby lobby)
    {
        Value = lobby;
        textName.text = lobby.Name;
        textPlayerCount.text = $"{lobby.Players.Count} / {lobby.MaxPlayers}";
    }
}
