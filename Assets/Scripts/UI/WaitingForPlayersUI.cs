using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WaitingForPlayersUI : MonoBehaviour
{
    [SerializeField] private Toggle readyButton;
    [SerializeField] private Button backButton;
    [SerializeField] private SceneLoader.Scene previousScene;
    [SerializeField] private TextMeshProUGUI waitingForText;

    private WaitingForPlayers waiterForPlayers => WaitingForPlayers.Singleton;

    void Start()
    {
        readyButton.onValueChanged.AddListener((value) =>
        {
            waiterForPlayers.ToggleReady(value);
        });

        backButton.onClick.AddListener(async () =>
        {
            await MultiplayerManager.Singleton.Shutdown();
            SceneLoader.Load(previousScene, true, false);
        });

        waiterForPlayers.TotalCount.OnValueChanged += UpdateText;
        waiterForPlayers.ReadyPlayersCount.OnValueChanged += UpdateText;
        waiterForPlayers.OnReadyChange += Selection_OnReadyChange;
        waiterForPlayers.OnAllPlayersReady += WaitingForPlayers_OnAllPlayersReady;
        UpdateText(0, 0);
    }

    private void WaitingForPlayers_OnAllPlayersReady()
    {
        gameObject.SetActive(false);
    }

    private void Selection_OnReadyChange(ulong clientId, bool isReady)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
            readyButton.isOn = isReady;
    }

    private void UpdateText(int previousValue, int newValue)
    {
        if (!MultiplayerManager.Singleton.IsSolo)
            waitingForText.text = $"Ready players {waiterForPlayers.ReadyPlayersCount.Value} / {waiterForPlayers.TotalCount.Value}";
        else
            waitingForText.text = string.Empty;
    }

    private void OnDestroy()
    {
        if (waiterForPlayers != null)
            waiterForPlayers.ReadyPlayersCount.OnValueChanged -= UpdateText;
    }
}
