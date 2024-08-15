using TMPro;
using UnityEngine;

public class LobbyInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    void Start()
    {
        lobbyName.text = MultiplayerLobby.Singleton.GetLobbyName();
        lobbyCodeText.text = MultiplayerLobby.Singleton.GetLobbyCode();
    }
}
