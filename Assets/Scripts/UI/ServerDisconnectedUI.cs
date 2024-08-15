using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ServerDisconnectedUI : MonoBehaviour
{
    [SerializeField] private Button MainMenuButton;

    void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;

        MainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            SceneLoader.Load(SceneLoader.Scene.MainMenuScene, true, false);
        });
        gameObject.SetActive(false);
    }

    private void Singleton_OnClientDisconnectCallback(ulong clientId)
    {
        if (gameObject != null && !gameObject.IsDestroyed() && clientId == NetworkManager.Singleton.LocalClientId)
            gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
    }
}
