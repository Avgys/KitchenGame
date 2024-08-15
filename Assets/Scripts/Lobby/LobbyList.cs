using System.Collections.Generic;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyList : MonoBehaviour
{
    private List<LobbyGridInfo> lobbiesInList = new();
    [SerializeField] LobbyGridInfo prefab;
    [SerializeField] Transform gridTransform;

    [SerializeField] Button RefreshList;

    [SerializeField] TextMeshProUGUI noLobbies;

    private void Start()
    {
        if (MultiplayerLobby.Singleton == null)
            UnityServices.Initialized += Subscribe;
        else
            Subscribe();

        RefreshList.onClick.AddListener(AskRefresh);

        void Subscribe()
        {
            MultiplayerLobby.Singleton.OnLobbyListChange += UpdateList;
            UnityServices.Initialized -= Subscribe;
        }
    }

    async void AskRefresh()
    {
        await MultiplayerLobby.Singleton.GetLobbies();
    }

    void UpdateList(List<Lobby> lobbies)
    {
        if (lobbies.Count > lobbiesInList.Count)
        {
            var diff = lobbies.Count - lobbiesInList.Count;
            for (int i = 0; i < diff; i++)
            {
                var lobby = Instantiate(prefab, gridTransform);
                lobbiesInList.Add(lobby);
            }
        }

        if (lobbies.Count < lobbiesInList.Count)
        {
            var diff = lobbiesInList.Count - lobbies.Count;
            for (int i = 0; i < diff; i++)
            {
                lobbiesInList[i + lobbies.Count].gameObject.SetActive(false);
            }
        }

        noLobbies.gameObject.SetActive(lobbies.Count == 0);

        for (int i = 0; i < lobbies.Count; i++)
        {
            lobbiesInList[i].SetValue(lobbies[i]);
            lobbiesInList[i].gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (MultiplayerLobby.Singleton != null)
            MultiplayerLobby.Singleton.OnLobbyListChange -= UpdateList;
    }
}
