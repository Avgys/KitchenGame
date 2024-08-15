using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Transform createLobbyWindow;

    [SerializeField] private Button joinLobbyButton;

    [SerializeField] private Button joinLobbyByCodeButton;
    [SerializeField] private TMP_InputField lobbyCodeInput;

    [SerializeField] private TMP_InputField playerNameInput;

    [SerializeField] private Button mainMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        createLobbyButton.onClick.AddListener(() =>
        {
            createLobbyWindow.gameObject.SetActive(true);
            //await MultiplayerManager.Singleton.CreateLobbyAsync();
            //SceneLoader.Load(SceneLoader.Scene.CharacterSelectScene, false, true);
        });

        joinLobbyButton.onClick.AddListener(async () =>
        {
            PopupMessages.ShowMessage("Connection...");
            DisableButtons();
            await MultiplayerManager.Singleton.JoinAsync(); 
            EnableButtons();
        });

        joinLobbyByCodeButton.onClick.AddListener(async () =>
        {
            PopupMessages.ShowMessage("Connection..."); 
            DisableButtons();
            await MultiplayerManager.Singleton.JoinAsync(lobbyCode: lobbyCodeInput.text);
            EnableButtons();
        });

        mainMenu.onClick.AddListener(() =>
        {
            SceneLoader.Load(SceneLoader.Scene.MainMenuScene, false, false);
        });


        playerNameInput.text = LocalPlayerData.localData.PlayerName.ToString();
        playerNameInput.onValueChanged.AddListener((newValue) =>
        {
            LocalPlayerData.SetName(newValue);
        });

        MultiplayerManager.Singleton.OnConnectionFailed += OnConnectionFailed;
    }

    void EnableButtons()
    {
        joinLobbyButton.interactable = true;
        createLobbyButton.interactable = true;
        joinLobbyByCodeButton.interactable = true;
    }

    void DisableButtons()
    {
        joinLobbyButton.interactable = false;
        createLobbyButton.interactable = false;
        joinLobbyByCodeButton.interactable = false;
    }

    void OnConnectionFailed(string reason)
    {
        EnableButtons();
        joinLobbyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Join lobby";
    }

    private void OnDestroy()
    {
        if (MultiplayerManager.Singleton != null)
            MultiplayerManager.Singleton.OnConnectionFailed -= OnConnectionFailed;
    }
}
